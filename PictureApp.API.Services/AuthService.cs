using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PictureApp.API.Data;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Extensions;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Providers;

namespace PictureApp.API.Services
{
    public class AuthService : IAuthService
    {
        private const string AppSettingsAccountActivationTokenExpirationTimeInHoursKey = "AppSettings:AccountActivationTokenExpirationTimeInHours";
        private const string AppSettingsResetPasswordTokenExpirationTimeInHoursKey = "AppSettings:ResetPasswordTokenExpirationTimeInHours";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenProvider _tokenProvider;
        private readonly IPasswordProvider _passwordProvider;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IConfiguration _configuration;
        private readonly IFilesStorageProvider _filesStorageProvider;

        private IRepository<User> UserRepository => _repositoryFactory.Create<User>();
        private IRepository<AccountActivationToken> AccountActivationTokenRepository =>
            _repositoryFactory.Create<AccountActivationToken>();
        private IRepository<ResetPasswordToken> ResetPasswordTokenRepository =>
            _repositoryFactory.Create<ResetPasswordToken>();

        public AuthService(IRepositoryFactory repositoryFactory, IUnitOfWork unitOfWork,
            IMapper mapper, ITokenProvider tokenProvider, IPasswordProvider passwordProvider,
            IFilesStorageProvider filesStorageProvider, IConfiguration configuration)
        {
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _passwordProvider = passwordProvider ?? throw new ArgumentNullException(nameof(passwordProvider));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _filesStorageProvider = filesStorageProvider ?? throw new ArgumentNullException(nameof(filesStorageProvider));
        }

        public async Task Register(UserForRegisterDto userForRegister)
        {
            if (userForRegister == null) throw new ArgumentNullException(nameof(userForRegister));

            var userToCreate = _mapper.Map<User>(userForRegister);
            SetPasswordForUser(userToCreate, userForRegister.Password);

            userToCreate.ActivationToken = CreateToken<AccountActivationToken>();
            userToCreate.PendingUploadPhotosFolderName = _filesStorageProvider.CreateContainerName(SystemGuid.NewGuid().ToString("N"));

            await UserRepository.AddAsync(userToCreate);
            await _unitOfWork.CompleteAsync();
        }

        public async Task Reregister(UserForReregisterDto userForReregister)
        {
            if (userForReregister == null)
            {
                throw new ArgumentNullException(nameof(userForReregister));
            }

            var user = await UserRepository.SingleOrDefaultAsync(x => x.Email == userForReregister.Email.ToLower());
            if (user == null)
            {
                throw new EntityNotFoundException(
                    $"The user with email: {userForReregister.Email} does not exist in data store");
            }

            if (user.IsAccountActivated)
            {
                throw new NotAuthorizedException("The user account is already activated");
            }

            var currentActivationToken = await AccountActivationTokenRepository.SingleOrDefaultAsync(x => x.UserId == user.Id);

            var newActivationToken = CreateToken<AccountActivationToken>();
            if (currentActivationToken == null)
            {
                user.ActivationToken = newActivationToken;
                UserRepository.Update(user);
            }
            else
            {
                currentActivationToken.Token = newActivationToken.Token;
                AccountActivationTokenRepository.Update(currentActivationToken);
            }

            await _unitOfWork.CompleteAsync();
        }

        public async Task Activate(string token)
        {
            var expirationTime =
                GetConfigurationSectionValue(AppSettingsAccountActivationTokenExpirationTimeInHoursKey);
            var activationToken = await TokenValidation<AccountActivationToken>(token, expirationTime);

            var user = await UserRepository.SingleOrDefaultAsync(x => x.Id == activationToken.UserId);

            user.IsAccountActivated = true;
            AccountActivationTokenRepository.Delete(activationToken);

            await _unitOfWork.CompleteAsync();
        }

        public async Task ChangePassword(string email, string oldPassword, string newPassword, string retypedNewPassword)
        {
            var user = await UserValidation(email);

            var computedPassword = _passwordProvider.CreatePasswordHash(oldPassword, user.PasswordSalt);

            var userPassword = ComputedPassword.Create(user.PasswordHash, user.PasswordSalt);
            if (userPassword != computedPassword)
            {
                throw new ArgumentException("The given old password does not fit to the current user password");
            }

            if (newPassword != retypedNewPassword)
            {
                throw new ArgumentException("The new password is different than retyped new password");
            }

            SetPasswordForUser(user, newPassword);

            UserRepository.Update(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ResetPasswordRequest(string email)
        {
            var user = await UserValidation(email);

            var newToken = CreateToken<ResetPasswordToken>();

            if (user.ResetPasswordToken != null)
            {
                ResetPasswordTokenRepository.Delete(user.ResetPasswordToken);
            }

            user.ResetPasswordToken = newToken;

            UserRepository.Update(user);
            await _unitOfWork.CompleteAsync();            
        }

        public async Task ResetPassword(string token, string newPassword)
        {
            var expirationTime =
                GetConfigurationSectionValue(AppSettingsResetPasswordTokenExpirationTimeInHoursKey);
            var resetToken = await TokenValidation<ResetPasswordToken>(token, expirationTime);

            var user = await UserRepository.SingleOrDefaultAsync(x => x.Id == resetToken.UserId);
            SetPasswordForUser(user, newPassword);
            user.ResetPasswordToken = null;

            UserRepository.Update(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<UserLoggedInDto> Login(string email, string password)
        {
            var user = await GetUser(email);
            if (user == null)
            {
                throw new EntityNotFoundException($"The user with email: {email} does not exist in data store");
            }

            if (_passwordProvider.CreatePasswordHash(password, user.PasswordSalt) != ComputedPassword.Create(user.PasswordHash, user.PasswordSalt))
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
            return await UserRepository.SingleOrDefaultAsync(x => x.Email == email.ToLower());
        }

        private TTokenEntity CreateToken<TTokenEntity>() where TTokenEntity : ITokenEntity, new()
        {
            return new TTokenEntity
            {
                Token = _tokenProvider.CreateToken()
            };
        }

        private async Task<TTokenEntity> TokenValidation<TTokenEntity>(string token, int expirationTime) where TTokenEntity : ITokenEntity
        {
            if (_tokenProvider.IsTokenExpired(token, expirationTime))
            {
                throw new SecurityTokenExpiredException("Given token is already expired or in wrong format");
            }

            var repository = _repositoryFactory.Create<TTokenEntity>();
            var tokenEntity = await repository.SingleOrDefaultAsync(x => x.Token == token);
            if (tokenEntity == null)
            {
                throw new EntityNotFoundException($"Given token {token} does not exist in data store");
            }

            return tokenEntity;
        }

        private async Task<User> UserValidation(string userEmail)
        {
            var user = await GetUser(userEmail);
            if (user == null)
            {
                throw new EntityNotFoundException($"The user with email: {userEmail} does not exist in data store");
            }

            return user;
        }

        private int GetConfigurationSectionValue(string sectionName)
        {
            return int.Parse(_configuration.GetSection(sectionName).Value);
        }

        private void SetPasswordForUser(User user, string password)
        {
            var computedPassword = _passwordProvider.CreatePasswordHash(password);
            user.PasswordHash = computedPassword.Hash;
            user.PasswordSalt = computedPassword.Salt;
        }
    }
}