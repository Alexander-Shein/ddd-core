﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DddCore.DAL.DomainStack.EntityFramework.Mapping
{
    public interface IModelBuilder
    {
        EntityTypeBuilder<TEntity> Entity<TEntity>() where TEntity : class;
        IModelBuilder Ignore<TEntity>() where TEntity : class;
    }
}
