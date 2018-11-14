namespace PictureApp.API.Data
{
    public interface IUnitOfWork
    {
        void CompleteAsync();
    }
}