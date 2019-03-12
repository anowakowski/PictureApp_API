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
        //private readonly IRepository<User> _userRepository;
        //private readonly IRepository<AccountActivationToken> _accountActivationTokenRepository;
        //private readonly IRepository<ResetPasswordToken> _resetPasswordTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenProvider _tokenProvider;
        private readonly IPasswordProvider _passwordProvider;
        private readonly IRepositoryFactory _repositoryFactory;

        private IRepository<User> UserRepository => _repositoryFactory.Create<User>();
        private IRepository<AccountActivationToken> AccountActivationTokenRepository =>
            _repositoryFactory.Create<AccountActivationToken>();
        private IRepository<ResetPasswordToken> ResetPasswordTokenRepository =>
            _repositoryFactory.Create<ResetPasswordToken>();

        public AuthService(IRepositoryFactory repositoryFactory, /*IRepository<User> userRepository,
            IRepository<AccountActivationToken> accountActivationTokenRepository, IRepository<ResetPasswordToken> resetPasswordTokenRepository,*/ IUnitOfWork unitOfWork,
            IMapper mapper, ITokenProvider tokenProvider, IPasswordProvider passwordProvider)
        {
            //_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            //_accountActivationTokenRepository = accountActivationTokenRepository ??
            //                                    throw new ArgumentNullException(
            //                                        nameof(accountActivationTokenRepository));
            //_resetPasswordTokenRepository = resetPasswordTokenRepository ?? throw new ArgumentNullException(nameof(resetPasswordTokenRepository)); 
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _passwordProvider = passwordProvider ?? throw new ArgumentNullException(nameof(passwordProvider));
        }

        public async Task Register(UserForRegisterDto userForRegister)
        {
            if (userForRegister == null) throw new ArgumentNullException(nameof(userForRegister));

            var password = _passwordProvider.CreatePasswordHash(userForRegister.Password);

            var userToCreate = _mapper.Map<User>(userForRegister);
            userToCreate.PasswordHash = password.Hash;
            userToCreate.PasswordSalt = password.Salt;
            userToCreate.ActivationToken = CreateToken<AccountActivationToken>();

            await UserRepository.AddAsync(userToCreate);
            await _unitOfWork.CompleteAsync();
        }

        public async Task Activate(string token)
        {
            var activationToken = await TokenValidation<AccountActivationToken>(token);
            //if (_tokenProvider.IsTokenExpired(token))
            //{
            //    throw new SecurityTokenExpiredException("Given token is already expired");
            //}

            //var activationToken = await AccountActivationTokenRepository.SingleOrDefaultAsync(x => x.Token == token);
            //if (activationToken == null)
            //{
            //    throw new EntityNotFoundException($"Given token {token} does not exist in data store");
            //}

            //var userRepository = _repositoryFactory.Create<User>();
            var user = await UserRepository.SingleOrDefaultAsync(x => x.Id == activationToken.UserId);

            user.IsAccountActivated = true;
            AccountActivationTokenRepository.Delete(activationToken);

            await _unitOfWork.CompleteAsync();
        }

        public async Task ChangePassword(string email, string oldPassword, string newPassword, string retypedNewPassword)
        {
            var user = await UserValidation(email);

            //var password = _passwordProvider.CreatePasswordHash(oldPassword);
            var computedPassword = _passwordProvider.CreatePasswordHash(oldPassword, user.PasswordSalt);

            //if (user.PasswordHash != password.passwordHash)
            var userPassword = ComputedPassword.Create(user.PasswordHash, user.PasswordSalt);
            if (userPassword != computedPassword)
            {
                throw new ArgumentException("The given old password does not fit to the current user password");
            }

            if (newPassword != retypedNewPassword)
            {
                throw new ArgumentException("The new password is different than retyped new password");
            }

            //password = _passwordProvider.CreatePasswordHash(newPassword);
            //user.PasswordHash = password.passwordHash;
            //user.PasswordSalt = password.passwordSalt;

            computedPassword = _passwordProvider.CreatePasswordHash(newPassword);
            user.PasswordHash = computedPassword.Hash;
            user.PasswordSalt = computedPassword.Salt;

            UserRepository.Update(user);
            await _unitOfWork.CompleteAsync();
        }

        public async Task ResetPasswordRequest(string email)
        {
            // TODO:
            // - get user by email
            var user = await UserValidation(email);

            // - generate token for password reset                         
            var newToken = CreateToken<ResetPasswordToken>();

            // - check whether token is already exist
            //   if so delete it and save the new one
            var oldToken = await ResetPasswordTokenRepository.SingleOrDefaultAsync(x => x.UserId == user.Id);
            if (oldToken != null)
            {
                ResetPasswordTokenRepository.Delete(oldToken);
            }
            
            await ResetPasswordTokenRepository.AddAsync(newToken);
            await _unitOfWork.CompleteAsync();            
        }

        public async Task ResetPassword(string token, string newPassword)
        {
            // TODO:            
            // - check whether token is valid            
            // - check whether token exists            
            var resetToken = await TokenValidation<ResetPasswordToken>(token);

            // - get user by token
            var user = await UserRepository.SingleOrDefaultAsync(x => x.Id == resetToken.UserId);

            // - set new password to the user
            var computedPassword = _passwordProvider.CreatePasswordHash(newPassword);
            user.PasswordHash = computedPassword.Hash;
            user.PasswordSalt = computedPassword.Salt;

            // - save user with new password
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
            return await UserRepository.SingleOrDefaultAsync(x => x.Email == email.ToLower());
        }

        //private AccountActivationToken CreateActivationToken()
        //{
        //    return new AccountActivationToken
        //    {
        //        Token = _tokenProvider.CreateToken()
        //    };
        //}

        //private ResetPasswordToken CreateResetPasswordToken()
        //{
        //    return new ResetPasswordToken
        //    {
        //        Token = _tokenProvider.CreateToken()
        //    };
        //}

        private TTokenEntity CreateToken<TTokenEntity>() where TTokenEntity : ITokenEntity, new()
        {
            return new TTokenEntity
            {
                Token = _tokenProvider.CreateToken()
            };
        }

        //private async Task<ResetPasswordToken> TokenValidation(string token)
        //{
        //    if (_tokenProvider.IsTokenExpired(token))
        //    {
        //        throw new SecurityTokenExpiredException("Given token is already expired");
        //    }

        //    var resetPasswordToken = await ResetPasswordTokenRepository.SingleOrDefaultAsync(x => x.Token == token);
        //    if (resetPasswordToken == null)
        //    {
        //        throw new EntityNotFoundException($"Given token {token} does not exist in data store");
        //    }

        //    return resetPasswordToken;
        //}

        private async Task<TTokenEntity> TokenValidation<TTokenEntity>(string token) where TTokenEntity : ITokenEntity
        {
            if (_tokenProvider.IsTokenExpired(token))
            {
                throw new SecurityTokenExpiredException("Given token is already expired");
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
    }
}