using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PictureApp.API.Data.Repository
{
    public interface IRepository<TEntity>
    {            
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        void AddAsync(TEntity entity);

        TEntity GetById(int id);

        void Delete(TEntity entity);

        void Update(TEntity entity);
    }
}
