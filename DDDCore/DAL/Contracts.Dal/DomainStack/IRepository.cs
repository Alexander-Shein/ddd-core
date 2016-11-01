﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Contracts.Domain.Entities;

namespace Contracts.Dal.DomainStack
{
    public interface IRepository<T, in TKey> where T : class, IAggregateRootEntity<TKey>
    {
        void PersistEntityGraph(T entity);
        Task<T> ReadAsync(TKey key, params Expression<Func<T, object>>[] includes);
    }
}