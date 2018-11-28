using System.Threading.Tasks;

namespace PictureApp.API.Data
{
    public interface IUnitOfWork
    {
        Task CompleteAsync();
    }
}