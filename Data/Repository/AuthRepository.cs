using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PictureApp_API.Models;

namespace PictureApp_API.Data.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext dataContext;
        public AuthRepository(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }
        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await dataContext.Users.AddAsync(user);

            await dataContext.SaveChangesAsync();

            return user;
        }

        public async Task<bool> UserExists(object username)
        {
            if (await dataContext.Users.AnyAsync(x => x.Username == username)){
                return true;
            }

            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }

}