using System;
using Microsoft.EntityFrameworkCore;

namespace PictureApp.API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        protected DbContext DbContext;

        public UnitOfWork(DbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async void CompleteAsync()
        {
            await DbContext.SaveChangesAsync();
        }
    }
}
