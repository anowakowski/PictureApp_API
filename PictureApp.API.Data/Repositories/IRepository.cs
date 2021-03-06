﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PictureApp.API.Data.Repositories
{
    public interface IRepository<TEntity>
    {
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        Task AddAsync(TEntity entity);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity> GetById(int id);

        void Delete(TEntity entity);

        void Update(TEntity entity);

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<IEnumerable<TEntity>> FindAsyncWithIncludedEntities(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includeEntities);

        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
   }
}
