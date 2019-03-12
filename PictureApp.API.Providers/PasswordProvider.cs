using System.Linq;
using System.Text;

namespace PictureApp.API.Providers
{
    public class PasswordProvider : IPasswordProvider
    {
        //public (byte[] passwordHash, byte[] passwordSalt) CreatePasswordHash(string password)
        //{
        //    using (var hmac = new System.Security.Cryptography.HMACSHA512())
        //    {
        //        return (hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), hmac.Key);
        //    }
        //}

        public ComputedPassword CreatePasswordHash(string plainPassword, byte[] salt)
        {
            return ComputePassword(plainPassword, salt);
        }

        public ComputedPassword CreatePasswordHash(string plainPassword)
        {
            return ComputePassword(plainPassword, string.Empty);
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                if (computedHash.Where((t, i) => t != passwordHash[i]).Any())
                {
                    return false;
                }
            }

            return true;
        }

        private ComputedPassword ComputePassword(string plainPassword, string salt)
        {
            using (var hmac = string.IsNullOrEmpty(salt)
                ? new System.Security.Cryptography.HMACSHA512()
                : new System.Security.Cryptography.HMACSHA512(Encoding.UTF8.GetBytes(salt))) 
            {
                return ComputedPassword.Create(hmac.ComputeHash(Encoding.UTF8.GetBytes(plainPassword)), hmac.Key);
            }
        }

        private ComputedPassword ComputePassword(string plainPassword, byte[] salt)
        {
            using (var hmac = salt.Any()
                ? new System.Security.Cryptography.HMACSHA512(salt)
                : new System.Security.Cryptography.HMACSHA512())
            {
                return ComputedPassword.Create(hmac.ComputeHash(Encoding.UTF8.GetBytes(plainPassword)), hmac.Key);
            }
        }
    }
}
