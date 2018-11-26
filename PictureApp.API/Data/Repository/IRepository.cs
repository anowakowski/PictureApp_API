using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
        Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
   }
}
