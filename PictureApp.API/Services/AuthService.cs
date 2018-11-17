using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public AuthService(IAuthRepository authRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); 
        }

        public async Task Register(UserForRegisterDto userForRegister)
        {
            if (userForRegister == null) throw new ArgumentNullException(nameof(userForRegister));

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(userForRegister.Password, out passwordHash, out passwordSalt);

            var userToCreate = _mapper.Map<User>(userForRegister);
            userToCreate.PasswordHash = passwordHash;
            userToCreate.PasswordSalt = passwordSalt;
            
            await _authRepository.AddAsync(userToCreate);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<UserLoggedInDto> Login(string email, string password)
        {
            if (!await UserExists(email))
            {
                throw new EntityNotFoundException($"The user with email: {email} does not exist in datastore");
            }

            var userFromRepo = await _authRepository.Login(email, password);

            if (!VerifyPasswordHash(password, userFromRepo.PasswordHash, userFromRepo.PasswordSalt))
            {
                throw new NotAuthorizedException($"The user: {email} password verification has been failed");
            }

            return _mapper.Map<UserLoggedInDto>(userFromRepo);
        }

        public async Task<bool> UserExists(string email)
        {
            return await _authRepository.AnyAsync(x => x.Email == email.ToLower());
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
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
