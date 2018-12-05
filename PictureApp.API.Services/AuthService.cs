using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
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
            userToCreate.ActivationToken = CreateActivationToken();

            await _userRepository.AddAsync(userToCreate);
            await _unitOfWork.CompleteAsync();
        }

        public async Task Activate(string token)
        {
            if (_activationTokenProvider.IsTokenExpired(token))
            {
                throw new SecurityTokenExpiredException("Given token is already expired");
            }

            var activationToken = await _accountActivationTokenRepository.SingleOrDefaultAsync(x => x.Token == token);
            if (activationToken == null)
            {
                throw new EntityNotFoundException($"Given token {token} does not exist in data store");
            }

            var user = await _userRepository.SingleOrDefaultAsync(x => x.Id == activationToken.UserId);

            user.IsAccountActivated = true;
            _accountActivationTokenRepository.Delete(activationToken);

            await _unitOfWork.CompleteAsync();
        }

        public async Task<UserLoggedInDto> Login(string email, string password)
        {
            var user = await GetUser(email);
            if (user == null)
            {
                throw new EntityNotFoundException($"The user with email: {email} does not exist in data store");
            }

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                throw new NotAuthorizedException($"The user: {email} password verification has been failed");
            }

            return _mapper.Map<UserLoggedInDto>(user);
        }

        public async Task<bool> UserExists(string email)
        {
            return await GetUser(email) != null;
        }

        private async Task<User> GetUser(string email)
        {
            return await _userRepository.SingleOrDefaultAsync(x => x.Email == email.ToLower());
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private AccountActivationToken CreateActivationToken()
        {
            return new AccountActivationToken
            {
                Token = _activationTokenProvider.CreateToken()
            };
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
