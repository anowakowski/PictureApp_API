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
        private readonly IPasswordProvider _passwordProvider;

        public AuthService(IRepository<User> userRepository,
            IRepository<AccountActivationToken> accountActivationTokenRepository, IUnitOfWork unitOfWork,
            IMapper mapper, IActivationTokenProvider activationTokenProvider, IPasswordProvider passwordProvider)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _accountActivationTokenRepository = accountActivationTokenRepository ??
                                                throw new ArgumentNullException(
                                                    nameof(accountActivationTokenRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _activationTokenProvider = activationTokenProvider ?? throw new ArgumentNullException(nameof(activationTokenProvider));
            _passwordProvider = passwordProvider ?? throw new ArgumentNullException(nameof(passwordProvider));
        }

        public async Task Register(UserForRegisterDto userForRegister)
        {
            if (userForRegister == null) throw new ArgumentNullException(nameof(userForRegister));

            var password = _passwordProvider.CreatePasswordHash(userForRegister.Password);

            var userToCreate = _mapper.Map<User>(userForRegister);
            userToCreate.PasswordHash = password.passwordHash;
            userToCreate.PasswordSalt = password.passwordSalt;
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

        public async Task ChangePassword(string email, string oldPassword, string newPassword, string retypedNewPassword)
        {
            var user = await GetUser(email);
            if (user == null)
            {
                throw new EntityNotFoundException($"The user with email: {email} does not exist in data store");
            }

            var password = _passwordProvider.CreatePasswordHash(oldPassword);

            if (user.PasswordHash != password.passwordHash)
            {
                throw new ArgumentException("The given old password does not fit to the current user password");
            }

            if (newPassword != retypedNewPassword)
            {
                throw new ArgumentException("The new password is different than retyped new password");
            }

            password = _passwordProvider.CreatePasswordHash(newPassword);
            user.PasswordHash = password.passwordHash;
            user.PasswordSalt = password.passwordSalt;

            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ResetPasswordRequest(string email)
        {
            // TODO:
            // - get user by email
            var user = await GetUser(email);
            if (user == null)
            {
                throw new EntityNotFoundException($"The user with email: {email} does not exist in data store");
            }

            // - generate token for password reset                         
            var resetToken = CreateActivationToken();

            // - save token in data store
            await _accountActivationTokenRepository.AddAsync(resetToken);
            await _unitOfWork.CompleteAsync();            
        }

        public async Task ResetPassword(string token, string newPassword)
        {
            // TODO:            
            // - check whether token is valid            
            // - check whether token exists            
            var resetToken = await TokenValidation(token);

            // - get user by token
            var user = await _userRepository.SingleOrDefaultAsync(x => x.Id == resetToken.UserId);

            // - set new password to the user
            var password = _passwordProvider.CreatePasswordHash(newPassword);
            user.PasswordHash = password.passwordHash;
            user.PasswordSalt = password.passwordSalt;

            // - save user with new password
            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<UserLoggedInDto> Login(string email, string password)
        {
            var user = await GetUser(email);
            if (user == null)
            {
                throw new EntityNotFoundException($"The user with email: {email} does not exist in data store");
            }

            if (!_passwordProvider.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
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

        private AccountActivationToken CreateActivationToken()
        {
            return new AccountActivationToken
            {
                Token = _activationTokenProvider.CreateToken()
            };
        }

        private async Task<AccountActivationToken> TokenValidation(string token)
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

            return activationToken;
        }
    }
}
