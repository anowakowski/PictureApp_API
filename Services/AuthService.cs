using System;
using System.Linq;
using System.Threading.Tasks;
using PictureApp.API.Data.Repository;
using PictureApp.API.Dtos;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _repository;

        public AuthService(IRepository<User> repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public void Register(UserForRegisterDto userForRegister, string password)
        {
            //byte[] passwordHash, passwordSalt;
            var (passwordHash, passwordSalt) = CreatePasswordHash(password);

            var userToCreate = new User
            {
                Username = userForRegister.Username,
                Email = userForRegister.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
            
            _repository.AddAsync(userToCreate);
            
            // TODO: save changes to database
            //await dataContext.SaveChangesAsync();

            //return user;
        }

        public void Login(string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UserExists(string username)
        {
            throw new NotImplementedException();
        }

        private (byte[] passwordSalt, byte[] passwordHash) CreatePasswordHash(string password)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                return (hmac.Key, hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                if (computedHash.Where((t, i) => t != passwordHash[i]).Any())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
