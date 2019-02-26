namespace PictureApp.API.Providers
{
    public interface IPasswordProvider
    {
        (byte[] passwordHash, byte[] passwordSalt) CreatePasswordHash(string password);

        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    }
}
