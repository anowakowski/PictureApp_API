using System;
using System.Threading.Tasks;
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

        public async Task CompleteAsync()
        {
            await DbContext.SaveChangesAsync();
        }
    }
}
