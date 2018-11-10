using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PictureApp.API.Models;

namespace PictureApp.API.Data.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        protected DbContext DbContext;
        protected DbSet<TEntity> DbSet;

        public Repository(DbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            DbSet = DbContext.Set<TEntity>();
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {            
            return DbSet.Where(predicate).ToList();
        }

        public async void AddAsync(TEntity entity)
        {
            await DbContext.AddAsync(entity);
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.AnyAsync(predicate);
        }

        public TEntity GetById(int id)
        {            
            return DbSet.Single(x => x.Id == id);
        }

        public void Delete(TEntity entity)
        {
            DbContext.Remove(entity);
        }

        public void Update(TEntity entity)
        {
            DbContext.Update(entity);
        }
    }
}
