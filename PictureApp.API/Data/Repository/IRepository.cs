using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PictureApp.API.Data.Repository
{
    public interface IRepository<TEntity>
    {            
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        Task AddAsync(TEntity entity);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity> GetById(int id);

        void Delete(TEntity entity);

        void Update(TEntity entity);
    }
}
