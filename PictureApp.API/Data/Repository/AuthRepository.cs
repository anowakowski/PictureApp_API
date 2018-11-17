using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PictureApp.API.Models;

namespace PictureApp.API.Data.Repository
{
    public class AuthRepository : Repository<User>, IAuthRepository
    {
        private readonly DataContext _dataContext;
        public AuthRepository(DataContext dataContext) : base(dataContext)
        {
            this._dataContext = dataContext;

        }
        public async Task<User> Login(string username, string password)
        {
            return await DbSet.FirstOrDefaultAsync(x => x.Email == username);
        }

        public async Task<User> Register(User user, string password)
        {
            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await DbSet.AddAsync(user);

            await _dataContext.SaveChangesAsync();

            return user;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public async Task<bool> UserExists(string username)
        {
            if (await DbSet.AnyAsync(x => x.Username == username))
            {
                return true;
            }

            return false;
        }
    }
}