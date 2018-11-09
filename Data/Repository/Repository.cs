using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PictureApp.API.Models;
using PictureApp_API.Data;

namespace PictureApp.API.Data.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity
    {
        protected DbContext DbContext;
        protected DbSet<TEntity> DbSet;

        public Repository(DataContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.Where(predicate).ToList();
        }

        public async void AddAsync(TEntity entity)
        {
            await DbContext.AddAsync(entity);
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
