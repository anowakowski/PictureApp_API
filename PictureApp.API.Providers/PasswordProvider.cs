using System.Linq;
using System.Text;

namespace PictureApp.API.Providers
{
    public class PasswordProvider : IPasswordProvider
    {
        public ComputedPassword CreatePasswordHash(string plainPassword, byte[] salt)
        {
            return ComputePassword(plainPassword, salt);
        }

        public ComputedPassword CreatePasswordHash(string plainPassword)
        {
            return ComputePassword(plainPassword, null);
        }

        private ComputedPassword ComputePassword(string plainPassword, byte[] salt)
        {
            using (var hmac = salt != null && salt.Any()
                ? new System.Security.Cryptography.HMACSHA512(salt)
                : new System.Security.Cryptography.HMACSHA512())
            {
                return ComputedPassword.Create(hmac.ComputeHash(Encoding.UTF8.GetBytes(plainPassword)), hmac.Key);
            }
        }
    }
}
