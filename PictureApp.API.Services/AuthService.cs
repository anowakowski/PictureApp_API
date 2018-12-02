using System;
using System.Threading.Tasks;
using AutoMapper;
using PictureApp.API.Data;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Providers;

namespace PictureApp.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<AccountActivationToken> _accountActivationTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IActivationTokenProvider _activationTokenProvider;

        public AuthService(IRepository<User> userRepository,
            IRepository<AccountActivationToken> accountActivationTokenRepository, IUnitOfWork unitOfWork,
            IMapper mapper, IActivationTokenProvider activationTokenProvider)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _accountActivationTokenRepository = accountActivationTokenRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(accountActivationTokenRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _activationTokenProvider = activationTokenProvider ?? throw new ArgumentNullException(nameof(activationTokenProvider));
        }

        public async Task Register(UserForRegisterDto userForRegister)
        {
            if (userForRegister == null) throw new ArgumentNullException(nameof(userForRegister));

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(userForRegister.Password, out passwordHash, out passwordSalt);

            var userToCreate = _mapper.Map<User>(userForRegister);
            userToCreate.PasswordHash = passwordHash;
            userToCreate.PasswordSalt = passwordSalt;
            var activationToken = new AccountActivationToken
            {
                Token = _activationTokenProvider.CreateToken()
            };
            userToCreate.ActivationToken = activationToken;

            await _userRepository.AddAsync(userToCreate);
            await _unitOfWork.CompleteAsync();
        }

        public Task ReRegister(string email)
        {
            throw new NotImplementedException();
        }
        
        public async Task Activate(string token)
        {
            if (_activationTokenProvider.IsTokenExpired(token))
            {
                throw new ArgumentException("Given token is already expired");
            }

            var activationToken = await _accountActivationTokenRepository.SingleAsync(x => x.Token == token);
            var user = await _userRepository.SingleAsync(x => x.Id == activationToken.UserId);

            user.IsAccountActivated = true;
            _accountActivationTokenRepository.Delete(activationToken);

            await _unitOfWork.CompleteAsync();
        }

        public async Task<UserLoggedInDto> Login(string email, string password)
        {
            if (!await UserExists(email))
            {
                throw new EntityNotFoundException($"The user with email: {email} does not exist in datastore");
            }

            var userFromRepo = await _userRepository.SingleAsync(x => x.Email == email);

            if (!VerifyPasswordHash(password, userFromRepo.PasswordHash, userFromRepo.PasswordSalt))
            {
                throw new NotAuthorizedException($"The user: {email} password verification has been failed");
            }

            return _mapper.Map<UserLoggedInDto>(userFromRepo);
        }

        public async Task<bool> UserExists(string email)
        {
            return await _userRepository.AnyAsync(x => x.Email == email.ToLower());
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
    }
}
