﻿using System;
using System.Threading.Tasks;
using DddCore.Contracts.Crosscutting.UserContext;
using DddCore.Contracts.Dal.DomainStack;
using DddCore.Contracts.Domain.Entities;
using DddCore.Contracts.Domain.Entities.Audit.At;
using DddCore.Contracts.Domain.Entities.Audit.By;
using DddCore.Contracts.Domain.Entities.Model;
using DddCore.Dal.DomainStack.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DddCore.Dal.DomainStack.EntityFramework
{
    public class Repository<T, TKey> : IRepository<T, TKey> where T : class, IAggregateRootEntity<TKey>
    {
        #region Private Members

        readonly IUserContext<TKey> userContext;
        DateTime now;

        #endregion

        protected readonly IDataContext DataContext;

        protected Repository(IDataContext dataContext, IUserContext<TKey> userContext)
        {
            DataContext = dataContext;
            this.userContext = userContext;
        }

        #region Public Methods

        public virtual void PersistAggregateRoot(T entity)
        {
            now = DateTime.UtcNow;

            entity.WalkAggregateRootGraph(node =>
            {
                UpdateAuditableInfo(node);
                DataContext.SyncEntityState(node);
                node.CrudState = CrudState.Unchanged;
            });
        }

        public virtual async Task<T> ReadAggregateRootAsync(TKey key)
        {
            var query = GetDbSet().AsQueryable();

            foreach (var expr in GetPropertySelectors())
            {
                query = query.Include(expr);
            }

            return await query.FirstOrDefaultAsync(x => x.Id.Equals(key));
        }

        #endregion

        protected DbSet<T> GetDbSet()
        {
            var set = DataContext.Set<T>();
            return set;
        }

        #region Private Methods

        string[] GetPropertySelectors()
        {
            //TODO: implement
            return new string[0];
        }

        void UpdateAuditableInfo(ICrudState entity)
        {
            bool isAdded = entity.CrudState == CrudState.Added;
            bool isModified = isAdded || entity.CrudState == CrudState.Modified;

            if (isAdded)
            {
                AddCreatedAt(entity);
                AddCreatedBy(entity);
            }

            if (isModified)
            {
                AddModifiedAt(entity);
                AddModifiedBy(entity);
            }
        }

        void AddModifiedBy(object entity)
        {
            if (entity is IModifiedBy<TKey> model)
            {
                model.ModifiedBy = userContext.Id;
            }
        }

        void AddCreatedBy(object entity)
        {
            if (entity is ICreatedBy<TKey> model)
            {
                model.CreatedBy = userContext.Id;
            }
        }

        void AddCreatedAt(object entity)
        {
            if (entity is ICreatedAt model)
            {
                model.CreatedAt = now;
            }
        }

        void AddModifiedAt(object entity)
        {
            if (entity is IModifiedAt model)
            {
                model.ModifiedAt = now;
            }
        }

        #endregion
    }
}