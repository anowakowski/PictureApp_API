using System;
using System.Linq;
using System.Threading.Tasks;
using PictureApp.API.Data;
using PictureApp.API.Data.Repository;
using PictureApp.API.Dtos;
using PictureApp.API.Exceptions;
using PictureApp.API.Models;

namespace PictureApp.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IRepository<User> repository, IUnitOfWork unitOfWork)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public void Register(UserForRegisterDto userForRegister)
        {
            if (userForRegister == null) throw new ArgumentNullException(nameof(userForRegister));

            var (passwordHash, passwordSalt) = CreatePasswordHash(userForRegister.Password);

            var userToCreate = new User
            {
                Username = userForRegister.Username,
                Email = userForRegister.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
            
            _repository.AddAsync(userToCreate);
            
            _unitOfWork.CompleteAsync();
        }

        public async void Login(string email, string password)
        {
            if (!await UserExists(email))
            {
                throw new EntityNotFoundException($"The user with email: {email} does not exist in datastore");
            }

            var user = GetUser(email);

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                throw new NotAuthorizedException($"The user: {email} password verification has been failed");
            }
        }

        public UserLoggedInDto GetLoggedInUser(string email)
        {
            var user = GetUser(email);
            return new UserLoggedInDto
            {
                Id = user.Id, 
                Email = user.Email
            };
        }

        public async Task<bool> UserExists(string email)
        {
            return await _repository.AnyAsync(x => x.Email == email.ToLower());
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

        private User GetUser(string email)
        {
            var users = _repository.Find(x => x.Email == email.ToLower());
            return users.Single();
        }
    }
}
