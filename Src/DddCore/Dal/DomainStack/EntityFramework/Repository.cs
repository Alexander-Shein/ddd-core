﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DddCore.Contracts.BLL.Domain.Entities;
using DddCore.Contracts.BLL.Domain.Entities.Audit.At;
using DddCore.Contracts.BLL.Domain.Entities.Audit.By;
using DddCore.Contracts.BLL.Domain.Entities.Model;
using DddCore.Contracts.Crosscutting.UserContext;
using DddCore.Contracts.DAL.DomainStack;
using DddCore.DAL.DomainStack.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace DddCore.DAL.DomainStack.EntityFramework
{
    public class Repository<T, TKey> : IRepository<T, TKey> where T : class, IAggregateRootEntity<TKey>
    {
        #region Private Members

        readonly IUserContext<TKey> userContext;
        DateTime now;

        #endregion

        protected readonly IDataContext DataContext;

        public Repository(IDataContext dataContext, IUserContext<TKey> userContext)
        {
            DataContext = dataContext;
            this.userContext = userContext;
        }

        #region Public Methods

        public virtual void Persist(T entity)
        {
            now = DateTime.UtcNow;

            entity.WalkGraph(node =>
            {
                UpdateAuditableInfo(node);
                DataContext.SyncEntityState(node);
                node.CrudState = CrudState.Unchanged;
            });
        }

        public virtual async Task<T> ReadByIdAsync(TKey key)
        {
            var query = GetDbSet().AsQueryable();

            foreach (var expr in GetEntityPropertyNames(typeof(T)))
            {
                query = query.Include(expr);
            }

            return await query.FirstOrDefaultAsync(x => x.Id.Equals(key));
        }

        public virtual T ReadById(TKey key)
        {
            var query = GetDbSet().AsQueryable();

            foreach (var expr in GetEntityPropertyNames(typeof(T)))
            {
                query = query.Include(expr);
            }

            return query.FirstOrDefault(x => x.Id.Equals(key));
        }

        #endregion

        protected DbSet<T> GetDbSet()
        {
            var set = DataContext.Set<T>();
            return set;
        }

        #region Private Methods

        string[] GetEntityPropertyNames(Type type)
        {
            var result = new List<string>();
            var entityType = typeof(IEntity<TKey>);
            var aggregateRootType = typeof(IAggregateRootEntity<TKey>);

            foreach (var p in type.GetProperties())
            {
                var propertyType = p.PropertyType;

                var enumerableType = GetEnumerableType(propertyType);

                if (enumerableType != null)
                {
                    propertyType = enumerableType;
                }

                if (entityType.IsAssignableFrom(propertyType) && !aggregateRootType.IsAssignableFrom(propertyType))
                {
                    var childPropertyNames = GetEntityPropertyNames(propertyType);

                    if (!childPropertyNames.Any())
                    {
                        result.Add(p.Name);
                        continue;
                    }

                    foreach (var propertyName in childPropertyNames)
                    {
                        result.Add($"{p.Name}.{propertyName}");
                    }
                }
            }

            return result.ToArray();
        }

        Type GetEnumerableType(Type type)
        {
            var enumerableType = typeof(IEnumerable<>);

            foreach (Type intType in type.GetInterfaces())
            {
                if (intType.GetTypeInfo().IsGenericType
                    && intType.GetGenericTypeDefinition() == enumerableType)
                {
                    return intType.GetGenericArguments()[0];
                }
            }
            return null;
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