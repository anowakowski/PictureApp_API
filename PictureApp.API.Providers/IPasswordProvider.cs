namespace PictureApp.API.Providers
{
    public interface IPasswordProvider
    {
        //(byte[] passwordHash, byte[] passwordSalt) CreatePasswordHash(string password);

        ComputedPassword CreatePasswordHash(string plainPassword);

        ComputedPassword CreatePasswordHash(string plainPassword, string salt);


        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
    }
}
