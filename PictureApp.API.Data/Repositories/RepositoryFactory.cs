using System;
using PictureApp.API.Models;

namespace PictureApp.API.Data.Repositories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public RepositoryFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IRepository<TEntity> Create<TEntity>() where TEntity : IEntity
        {
            return _serviceProvider.GetService(typeof(IRepository<TEntity>)) as IRepository<TEntity>;
        }
    }
}
