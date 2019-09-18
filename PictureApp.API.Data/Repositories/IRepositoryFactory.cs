using PictureApp.API.Models;

namespace PictureApp.API.Data.Repositories
{
    public interface IRepositoryFactory
    {
        IRepository<TEntity> Create<TEntity>() where TEntity : IEntity;
    }
}
