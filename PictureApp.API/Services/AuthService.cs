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
        private readonly IAuthRepository _authRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IAuthRepository authRepository, IUnitOfWork unitOfWork)
        {
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
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
            
            _authRepository.AddAsync(userToCreate);
            
            _unitOfWork.CompleteAsync();
        }

        public async Task<UserLoggedInDto> Login(string email, string password)
        {
            if (!await UserExists(email))
            {
                throw new EntityNotFoundException($"The user with email: {email} does not exist in datastore");
            }

            var user = await _authRepository.Login(email, password);

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                throw new NotAuthorizedException($"The user: {email} password verification has been failed");
            }

            return GetLoggedInUser(user);
        }

        public UserLoggedInDto GetLoggedInUser(User user)
        {
            return new UserLoggedInDto
            {
                Id = user.Id, 
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task<bool> UserExists(string email)
        {
            return await _authRepository.AnyAsync(x => x.Email == email.ToLower());
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

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }

            return true;
        }

        private User GetUser(string email)
        {
            var users = _authRepository.Find(x => x.Email == email.ToLower());
            return users.Single();
        }
    }
}
