namespace PictureApp.API.Providers
{
    public interface IPasswordProvider
    {
        ComputedPassword CreatePasswordHash(string plainPassword);

        ComputedPassword CreatePasswordHash(string plainPassword, byte[] salt);        
    }
}
