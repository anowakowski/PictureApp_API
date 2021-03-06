using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MoreLinq;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Data;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Extensions;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Providers;
using PictureApp.API.Services;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class AuthServiceTests : GuardClauseAssertionTests<AuthService>
    {
		private IRepository<User> _userRepository;
        private IRepository<AccountActivationToken> _accountActivationTokenRepository;
		private IRepository<ResetPasswordToken> _resetPasswordTokenRepository;
		private IConfiguration _configuration;
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
		private ITokenProvider _tokenProvider;
        private IPasswordProvider _passwordProvider;
        private IRepositoryFactory _repositoryFactory;
        private IAuthService _sut;
		
		[SetUp]
        public void SetUp()
        {
			_userRepository = Substitute.For<IRepository<User>>();
            _accountActivationTokenRepository = Substitute.For<IRepository<AccountActivationToken>>();
            _resetPasswordTokenRepository = Substitute.For<IRepository<ResetPasswordToken>>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            SetUpMapper();
            //SetUpFilesStorageProvider();
			
			_tokenProvider = Substitute.For<ITokenProvider>();
            _passwordProvider = new MockPasswordProvider();
            _configuration = Substitute.For<IConfiguration>();
            SetConfigurationSection(default(string), default(int?));
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _repositoryFactory.Create<AccountActivationToken>().Returns(_accountActivationTokenRepository);
			_repositoryFactory.Create<ResetPasswordToken>().Returns(_resetPasswordTokenRepository);
			_repositoryFactory.Create<User>().Returns(x => _userRepository);

            _sut = new AuthService(_repositoryFactory, _unitOfWork, _mapper, _tokenProvider, _passwordProvider, _configuration);

            SystemGuid.Reset();
		}
		
		[Test]
        public void Login_WhenCalledAndUserDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var email = "the user email";
            _userRepository.SingleOrDefaultAsync(default(Expression<Func<User, bool>>))
                .ReturnsForAnyArgs((User) null);
            Func<Task> action = async () => await _sut.Login(email, "the user password");

            // ACT & ASSERT
            action.Should().Throw<EntityNotFoundException>()
                .WithMessage($"The user with email: {email} does not exist in data store");
        }

        [Test]
        public void Login_WhenCalledAndPassedPasswordDoesNotMeetExistingOne_NotAuthorizedExceptionExpected()
        {
            // ARRANGE
            var user = new User
            {
                Email = "user@post.com",
                PasswordHash = Encoding.ASCII.GetBytes("current password hash"),
                PasswordSalt = Encoding.ASCII.GetBytes("current password salt")
            };
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { user };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            SetPasswordProvider(("the user password",
                ComputedPassword.Create(user.PasswordHash, user.PasswordSalt)));
            Func<Task> action = async () => await _sut.Login(user.Email, "the user wrong password");

            // ACT & ASSERT
            action.Should().Throw<NotAuthorizedException>()
                .WithMessage($"The user: {user.Email} password verification has been failed");
        }

        [Test]
        public async Task Login_WhenCalledAndPassedPasswordMeetsExistingOne_ProperUserLoggedInDtoExpected()
        {
            var userPassword = "the user password";
            var user = new User
            {
                Id = 99,
                Username = "the user",
                Email = "user@post.com",
                PasswordHash = Encoding.ASCII.GetBytes("current password hash"),
                PasswordSalt = Encoding.ASCII.GetBytes("current password salt")
            };
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { user };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });

            SetPasswordProvider((userPassword,
                ComputedPassword.Create(user.PasswordHash, user.PasswordSalt)));

            _mapper.Map<UserLoggedInDto>(Arg.Any<User>()).Returns(x =>
            {
                var inputUser = x.ArgAt<User>(0);
                var dto = new UserLoggedInDto
                { Id = inputUser.Id, Email = inputUser.Email, Username = inputUser.Username };
                return dto;
            });
            var expected = new UserLoggedInDto { Id = user.Id, Email = user.Email, Username = user.Username };

            // ACT
            var actual = await _sut.Login(user.Email, userPassword);

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }
		
		[Test]
        public void Activate_WhenCalledAndTokenDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            _tokenProvider.IsTokenExpired(default(string), default(int)).ReturnsForAnyArgs(false);
            _accountActivationTokenRepository
                .SingleOrDefaultAsync(default(Expression<Func<AccountActivationToken, bool>>))
                .ReturnsForAnyArgs((AccountActivationToken)null);
            Func<Task> action = async () => await _sut.Activate("the token");

            // ACT & ASSERT
            action.Should().Throw<EntityNotFoundException>()
                .WithMessage("Given token the token does not exist in data store");
        }
		
		[Test]
        public void Activate_WhenCalledAndTokenExpired_SecurityTokenExpiredExceptionExpected()
        {
            // ARRANGE
            _tokenProvider.IsTokenExpired(default(string), default(int)).ReturnsForAnyArgs(true);
            Func<Task> action = async () => await _sut.Activate("the expired token");

            // ACT & ASSERT
            action.Should().Throw<SecurityTokenExpiredException>().WithMessage("Given token is already expired or in wrong format");
        }
		
		[Test]
        public async Task Activate_WhenCalledAndTokenNotExpired_FullAccountActivationExpected()
        {
            // ARRANGE
            _tokenProvider.IsTokenExpired(default(string), default(int)).ReturnsForAnyArgs(false);
            var actualUser = new User { Id = 99 };
            var actualActivationToken = new AccountActivationToken
            { Token = "The token", UserId = actualUser.Id };
            _accountActivationTokenRepository
                .SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns(x =>
                    {
                        var store = new List<AccountActivationToken> { actualActivationToken };
                        return store.Single(x.ArgAt<Expression<Func<AccountActivationToken, bool>>>(0).Compile());
                    }
                );
            actualUser.ActivationToken = actualActivationToken;
            AccountActivationToken tokenToDelete = null;
            _accountActivationTokenRepository.When(x => x.Delete(Arg.Any<AccountActivationToken>()))
                .Do(x => tokenToDelete = x.ArgAt<AccountActivationToken>(0));
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { actualUser };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            _unitOfWork.When(x => x.CompleteAsync()).Do(x =>
            {
                if (tokenToDelete != null && tokenToDelete.UserId == actualActivationToken.UserId)
                {
                    actualUser.ActivationToken = null;
                }
            });
            var expected = new User { Id = 99, IsAccountActivated = true };

            // ACT
            await _sut.Activate(actualActivationToken.Token);

            // ASSERT
            actualUser.Should().BeEquivalentTo(expected);
        }
		
		[Test]
        public async Task Register_WhenCalled_ProperUserToAddExpected()
        {
            // ARRANGE
            var userForRegister = new UserForRegisterDto
                {Username = "User for register", Email = "user@post.com", Password = "password"};
            (string password, ComputedPassword computedPassword) password = (userForRegister.Password,
                ComputedPassword.Create(Encoding.ASCII.GetBytes("password hash"), Encoding.ASCII.GetBytes("password salt")));
            SetPasswordProvider(password);
            var newActivationToken = new AccountActivationToken { Token = "The token" };
            User userToAdd = null;
            _userRepository.When(x => x.AddAsync(Arg.Any<User>())).Do(x => { userToAdd = x.ArgAt<User>(0); });
            _tokenProvider.CreateToken().Returns(newActivationToken.Token);
            _accountActivationTokenRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns((AccountActivationToken)null);
            var pendingUploadPhotosFolderName = Guid.NewGuid();
            SystemGuid.Set(() => pendingUploadPhotosFolderName);
            var expected = new User
            {
                Username = userForRegister.Username,
                Email = userForRegister.Email,
                ActivationToken = new AccountActivationToken {Token = newActivationToken.Token},
                IsAccountActivated = false,
                PendingUploadPhotosFolderName = pendingUploadPhotosFolderName.ToString("N"),
                PasswordHash = password.computedPassword.Hash,
                PasswordSalt = password.computedPassword.Salt
            };

            // ACT
            await _sut.Register(userForRegister);

            // ASSERT
            userToAdd.Should().BeEquivalentTo(expected);
            await _unitOfWork.Received().CompleteAsync();
        }
		
		[Test]
        public void Reregister_WhenCalledWithNull_ArgumentNullExceptionExpected()
        {
            // ARRANGE & ACT
            Func<Task> action = async () => await _sut.Reregister(null);

            // ASSERT
            action.Should().Throw<ArgumentNullException>();
        }
		
		[Test]
        public void Reregister_WhenCalledEndUserWithGivenEmailDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns((User)null);

            // ACT
            var userForReregister = new UserForReregisterDto {Email = "user@post.com"};
            Func<Task> action = async () => await _sut.Reregister(userForReregister);

            // ASSERT
            action.Should().Throw<EntityNotFoundException>()
                .WithMessage($"The user with email: {userForReregister.Email} does not exist in data store");
        }
		
		[Test]
        public void Reregister_WhenCalledEndUserHasAlreadyActivatedAccount_NotAuthorizedExceptionExpected()
        {
            // ARRANGE
            var actualUser = new User { Email = "user@post.com", IsAccountActivated = true };
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { actualUser };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });

            // ACT
            var userForReregister = new UserForReregisterDto { Email = actualUser.Email };
            Func<Task> action = async () => await _sut.Reregister(userForReregister);

            // ASSERT
            action.Should().Throw<NotAuthorizedException>()
                .WithMessage("The user account is already activated");
        }
		
		[Test]
        public async Task Reregister_WhenCalledAndUserHasNoToken_NewTokenInUserExpected()
        {
            // ARRANGE
            var actualUser = new User {Id = 99, Email = "user@post.com", IsAccountActivated = false};
            var newActivationToken = new AccountActivationToken { Token = "The token" };
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { actualUser };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            User userToUpdate = null;
            _userRepository.When(x => x.Update(Arg.Any<User>())).Do(x => userToUpdate = x.ArgAt<User>(0));
            _tokenProvider.CreateToken().Returns(newActivationToken.Token);
            _accountActivationTokenRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns((AccountActivationToken)null);
            var expected = new User
            {
                Id = actualUser.Id,
                Email = actualUser.Email,
                ActivationToken = new AccountActivationToken {Token = newActivationToken.Token}
            };
            var sut = _sut;

            // ACT
            await sut.Reregister(new UserForReregisterDto {Email = actualUser .Email});

            // ASSERT
            userToUpdate.Should().BeEquivalentTo(expected);
            await _unitOfWork.Received().CompleteAsync();
        }
		
		[Test]
        public async Task Reregister_WhenCalledAndUserHasAlreadyToken_UpdatedTokenExpected()
        {
            // ARRANGE
            var actualUser = new User { Id = 99, Email = "user@post.com", IsAccountActivated = false };
            var newActivationToken = new AccountActivationToken { Token = "The new token" };
            
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { actualUser };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            _tokenProvider.CreateToken().Returns(newActivationToken.Token);
            _accountActivationTokenRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns(x =>
                    {
                        var store = new List<AccountActivationToken> { new AccountActivationToken {Token = "The old token", UserId = actualUser.Id} };
                        return store.Single(x.ArgAt<Expression<Func<AccountActivationToken, bool>>>(0).Compile());
                    }
                );
            AccountActivationToken accountActivationTokenToUpdate = null;
            _accountActivationTokenRepository.When(x => x.Update(Arg.Any<AccountActivationToken>())).Do(x =>
                accountActivationTokenToUpdate = x.ArgAt<AccountActivationToken>(0));
            var expected = new AccountActivationToken
            {
                UserId = actualUser.Id,
                Token = newActivationToken.Token
            };
            var sut = _sut;

            // ACT
            await sut.Reregister(new UserForReregisterDto { Email = actualUser.Email });

            // ASSERT
            accountActivationTokenToUpdate.Should().BeEquivalentTo(expected);
            await _unitOfWork.Received().CompleteAsync();
        }
		
		[Test]
        public void ChangePassword_WhenCalledAndUserWithGivenEmailDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            _userRepository.SingleOrDefaultAsync(default(Expression<Func<User, bool>>)).ReturnsForAnyArgs((User)null);
            var email = "user@post.com";

            // ACT
            Func<Task> action = async () =>
                await _sut.ChangePassword(email, "the old password", "the new password",
                    "the new password");

            // ASSERT
            action.Should().Throw<EntityNotFoundException>()
                .WithMessage($"The user with email: {email} does not exist in data store");
        }

        [Test]
        public void ChangePassword_WhenCalledAndOldPasswordIsDifferentThanUsersCurrentOne_ArgumentExceptionExpected()
        {
            // ARRANGE
            var user = new User
            {
                Email = "user@post.com", PasswordHash = Encoding.ASCII.GetBytes("current password hash")
            };
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { user };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });

            // ACT
            Func<Task> action = async () =>
                await _sut.ChangePassword(user.Email, "the old password", "the new password",
                    "the new password");

            // ASSERT
            action.Should().Throw<ArgumentException>()
                .WithMessage("The given old password does not fit to the current user password");
        }

        [Test]
        public void ChangePassword_WhenCalledAndNewPasswordIsDifferentThanRetypedPassword_ArgumentExceptionExpected()
        {
            // ARRANGE
            var user = new User
            {
                Email = "user@post.com",
                PasswordHash = Encoding.ASCII.GetBytes("current password hash"),
                PasswordSalt = Encoding.ASCII.GetBytes("current password salt")
            };
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { user };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            SetPasswordProvider(("the old password",
                ComputedPassword.Create(user.PasswordHash, user.PasswordSalt)));

            // ACT
            Func<Task> action = async () =>
                await _sut.ChangePassword(user.Email, "the old password", "the new password",
                    "the different retyped new password");

            // ASSERT
            action.Should().Throw<ArgumentException>()
                .WithMessage("The new password is different than retyped new password");
        }

        [Test]
        public async Task ChangePassword_WhenCalledAndAllPassingDataAreAppropriate_AttemptToSaveNewPasswordForGivenUserExpected()
        {
            // ARRANGE
            var user = new User
            {
                Email = "user@post.com",
                PasswordHash = Encoding.ASCII.GetBytes("current password hash"),
                PasswordSalt = Encoding.ASCII.GetBytes("current password salt")
            };
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { user };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            User userToUpdate = null;
            _userRepository.When(x => x.Update(Arg.Any<User>())).Do(x => userToUpdate = x.ArgAt<User>(0));
            SetPasswordProvider(("the old password", ComputedPassword.Create(user.PasswordHash, user.PasswordSalt)),
                ("the new password",
                    ComputedPassword.Create(Encoding.ASCII.GetBytes("the new password hash"),
                        Encoding.ASCII.GetBytes("the new password salt"))));
            var expectedUserToUpdate = new User
            {
                Email = user.Email,
                PasswordHash = Encoding.ASCII.GetBytes("the new password hash"),
                PasswordSalt = Encoding.ASCII.GetBytes("the new password salt")
            };

            // ACT            
            await _sut.ChangePassword(user.Email, "the old password", "the new password",
                "the new password");

            // ASSERT
            await _unitOfWork.Received().CompleteAsync();
            userToUpdate.Should().BeEquivalentTo(expectedUserToUpdate);
        }

        [Test]
        public void ResetPasswordRequest_WhenCalledAndUserWithGivenEmailDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            _userRepository.SingleOrDefaultAsync(default(Expression<Func<User, bool>>)).ReturnsForAnyArgs((User)null);
            var email = "user@post.com";

            // ACT
            Func<Task> action = async () => await _sut.ResetPasswordRequest(email);

            // ASSERT
            action.Should().Throw<EntityNotFoundException>()
                .WithMessage($"The user with email: {email} does not exist in data store");
        }

        [Test]
        public async Task ResetPasswordRequest_WhenCalledAndOldResetPasswordTokenExists_AttemptToDeleteTheOldOneExpected()
        {
            // ARRANGE
            var email = "user@post.com";
            var oldToken = "the old token";
            var user = new User
            {
                Email = email,
                ResetPasswordToken = new ResetPasswordToken { Token = oldToken }
            };
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { user };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            string resetPasswordTokenToDelete = null;
            _resetPasswordTokenRepository.When(x => x.Delete(Arg.Any<ResetPasswordToken>()))
                .Do(x => resetPasswordTokenToDelete = x.ArgAt<ResetPasswordToken>(0).Token);

            // ACT
            await _sut.ResetPasswordRequest(email);

            // ASSERT            
            resetPasswordTokenToDelete.Should().BeEquivalentTo(oldToken);
            await _unitOfWork.Received().CompleteAsync();
        }

        [Test]
        public async Task ResetPasswordRequest_WhenCalled_UserWithAssociatedNewResetTokenExpected()
        {
            // ARRANGE
            var email = "user@post.com";
            var newToken = "the new token";
            var user = new User
            {
                Email = email
            };
            _tokenProvider.CreateToken().Returns(newToken);
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { user };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            string newResetPasswordToken = null;
            _userRepository.When(x => x.Update(Arg.Any<User>()))
                .Do(x => newResetPasswordToken = x.ArgAt<User>(0).ResetPasswordToken.Token);

            // ACT
            await _sut.ResetPasswordRequest(email);

            // ASSERT            
            newResetPasswordToken.Should().BeEquivalentTo(newToken);
            await _unitOfWork.Received().CompleteAsync();
        }

        [Test]
        public void ResetPassword_WhenCalledAndTokenIsExpired_SecurityTokenExpiredExceptionExpected()
        {
            // ARRANGE
            _tokenProvider.IsTokenExpired(default(string), default(int)).ReturnsForAnyArgs(true);

            // ACT
            Func<Task> action = async () => await _sut.ResetPassword("the token", "the new password");

            // ASSERT
            action.Should().Throw<SecurityTokenExpiredException>()
                .WithMessage("Given token is already expired or in wrong format");
        }

        [Test]
        public void ResetPassword_WhenCalledAndTokenDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var token = "the token";
            _tokenProvider.IsTokenExpired(Arg.Any<string>(), Arg.Any<int>()).Returns(false);
            _resetPasswordTokenRepository.SingleOrDefaultAsync(default(Expression<Func<ResetPasswordToken, bool>>))
                .ReturnsForAnyArgs((ResetPasswordToken)null);

            // ACT
            Func<Task> action = async () => await _sut.ResetPassword(token, "the new password");

            // ASSERT
            action.Should().Throw<EntityNotFoundException>()
                .WithMessage($"Given token {token} does not exist in data store");
        }

        [Test]
        public async Task ResetPassword_WhenCalled_AttemptToSaveUserWithTheNewPasswordExpected()
        {
            // ARRANGE
            var newPassword = "the new password";
            var token = new ResetPasswordToken
            {
                Token = "the token"
            };
            _tokenProvider.IsTokenExpired(Arg.Any<string>(), Arg.Any<int>()).Returns(false);
            _resetPasswordTokenRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<ResetPasswordToken, bool>>>())
                .Returns(x =>
                {
                    var store = new List<ResetPasswordToken> { token };
                    return store.Single(x.ArgAt<Expression<Func<ResetPasswordToken, bool>>>(0).Compile());
                });
            var user = new User
            {
                PasswordHash = Encoding.ASCII.GetBytes("the old password hash"),
                PasswordSalt = Encoding.ASCII.GetBytes("the old password salt"),
                ResetPasswordToken = new ResetPasswordToken()
            };
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>())
                .Returns(x =>
                {
                    var store = new List<User> { user };
                    return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
                });
            User actualUserEntityToSave = null;
            _userRepository.When(x => x.Update(Arg.Any<User>())).Do(x => actualUserEntityToSave = x.ArgAt<User>(0));
            var computedNewPassword = ComputedPassword.Create(
                Encoding.ASCII.GetBytes("the new password hash"), Encoding.ASCII.GetBytes("the new password salt"));
            SetPasswordProvider((newPassword, computedNewPassword));
            var expectedUserEntityToSave = new User
            {
                PasswordHash = computedNewPassword.Hash,
                PasswordSalt = computedNewPassword.Salt
            };

            // ACT
            await _sut.ResetPassword(token.Token, newPassword);

            // ASSERT
            actualUserEntityToSave.Should().BeEquivalentTo(expectedUserEntityToSave);
            await _unitOfWork.Received().CompleteAsync();
        }

        private void SetConfigurationSection(string sectionName, int? value)
        {            
            var configurationSection = Substitute.For<IConfigurationSection>();
            int configValue;
            if (!value.HasValue)
            {
                var random = new Random();
                configValue = random.Next(int.MinValue, int.MaxValue);
            }
            else
            {
                configValue = value.Value;
            }
            configurationSection.Value = configValue.ToString();

            if (sectionName == null)
            {
                _configuration.GetSection(default(string)).ReturnsForAnyArgs(configurationSection);
            }
            else
            {
                _configuration.GetSection(sectionName).Returns(configurationSection);
            }
        }
		
		private void SetUpMapper()
		{
            _mapper = Substitute.For<IMapper>();
            _mapper.Map<User>(Arg.Any<UserForRegisterDto>()).Returns(x =>
            {
                var userForRegister = x.ArgAt<UserForRegisterDto>(0);
                var user = new User
                {
                    Username = userForRegister.Username,
                    Email = userForRegister.Email
                };
                return user;
            });
        }

        private void SetPasswordProvider(params (string password, ComputedPassword computedPassword)[] passwords)
        {
            (_passwordProvider as MockPasswordProvider)?.AddPasswords(passwords);
        }
	}
	
	internal class MockPasswordProvider : IPasswordProvider
    {
        private readonly IDictionary<string, ComputedPassword> _passwords;

        public MockPasswordProvider()
        {
            _passwords = new Dictionary<string, ComputedPassword>();
        }

        public ComputedPassword CreatePasswordHash(string plainPassword, byte[] salt)
        {
            return _passwords.ContainsKey(plainPassword) ? _passwords[plainPassword] : null;
        }

        public ComputedPassword CreatePasswordHash(string plainPassword)
        {
            return _passwords.ContainsKey(plainPassword) ? _passwords[plainPassword] : null;
        }

        public void AddPasswords((string password, ComputedPassword computedPassword)[] passwords)
        {
            passwords.ForEach(x => _passwords.Add(x.password, x.computedPassword));
        }
    }
}