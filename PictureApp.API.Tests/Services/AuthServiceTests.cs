using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
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
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private IActivationTokenProvider _activationTokenProvider;
        private IFilesStorageProvider _filesStorageProvider;
        private IAuthService _sut;

        [SetUp]
        public void SetUp()
        {
            _userRepository = Substitute.For<IRepository<User>>();
            _accountActivationTokenRepository = Substitute.For<IRepository<AccountActivationToken>>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            SetUpMapper();
            _activationTokenProvider = Substitute.For<IActivationTokenProvider>();
            SetUpFilesStorageProvider();

            _sut = new AuthService(_userRepository, _accountActivationTokenRepository, _unitOfWork, _mapper, _activationTokenProvider, _filesStorageProvider);

            SystemGuid.Reset();
        }

        [Test]
        public void Activate_WhenCalledAndTokenExpired_SecurityTokenExpiredExceptionExpected()
        {
            // ARRANGE
            _activationTokenProvider.IsTokenExpired(Arg.Any<string>()).Returns(true);

            // ACT
            Func<Task> action = async () => await _sut.Activate("the expired token");

            // ASSERT
            action.Should().Throw<SecurityTokenExpiredException>().WithMessage("Given token is already expired");
        }

        [Test]
        public void Activate_WhenCalledAndTokenDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE            
            _activationTokenProvider.IsTokenExpired(Arg.Any<string>()).Returns(false);            
            _accountActivationTokenRepository
                .SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns(x => (AccountActivationToken) null);            

            // ACT
            Func<Task> action = async () => await _sut.Activate("the token");

            // ASSERT
            action.Should().Throw<EntityNotFoundException>().WithMessage("Given token the token does not exist in data store");
        }

        [Test]
        public async Task Activate_WhenCalledAndTokenNotExpired_FullAccountActivationExpected()
        {
            // ARRANGE
            _activationTokenProvider.IsTokenExpired(Arg.Any<string>()).Returns(false);
            var actualUser = new User {Id = 99};
            var actualActivationToken = new AccountActivationToken
                {Token = "The token", UserId = actualUser.Id};
            _accountActivationTokenRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
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
                var store = new List<User> {actualUser};
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            _unitOfWork.When(x => x.CompleteAsync()).Do(x =>
            {
                if (tokenToDelete != null && tokenToDelete.UserId == actualActivationToken.UserId)
                {
                    actualUser.ActivationToken = null;
                }
            });
            var expected = new User {Id = 99, IsAccountActivated = true};

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
            var newActivationToken = new AccountActivationToken { Token = "The token" };
            object userToAdd = null;
            _userRepository.When(x => x.AddAsync(Arg.Any<User>())).Do(x =>
                {
                    var user = x.ArgAt<User>(0);
                    userToAdd = new
                    {
                        Username = user.Username,
                        Email = user.Email,
                        ActivationToken = user.ActivationToken,
                        IsAccountActivated = user.IsAccountActivated,
                        PendingUploadPhotosFolderName = user.PendingUploadPhotosFolderName
                    };
                }
            );
            _activationTokenProvider.CreateToken().Returns(newActivationToken.Token);
            _accountActivationTokenRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns((AccountActivationToken)null);
            var pendingUploadPhotosFolderName = Guid.NewGuid();
            SystemGuid.Set(() => pendingUploadPhotosFolderName);
            var expected = new
            {
                Username = userForRegister.Username,
                Email = userForRegister.Email,                
                ActivationToken = new AccountActivationToken { Token = newActivationToken.Token },
                IsAccountActivated = false,
                PendingUploadPhotosFolderName = pendingUploadPhotosFolderName.ToString("N")
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
            _activationTokenProvider.CreateToken().Returns(newActivationToken.Token);            
            _accountActivationTokenRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns((AccountActivationToken)null);
            var expected = new User
            {
                Id = actualUser.Id,
                Email = actualUser.Email,
                ActivationToken = new AccountActivationToken {Token = newActivationToken.Token}
            };

            // ACT
            await _sut.Reregister(new UserForReregisterDto {Email = actualUser .Email});

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
            _activationTokenProvider.CreateToken().Returns(newActivationToken.Token);
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

            // ACT
            await _sut.Reregister(new UserForReregisterDto { Email = actualUser.Email });

            // ASSERT
            accountActivationTokenToUpdate.Should().BeEquivalentTo(expected);
            await _unitOfWork.Received().CompleteAsync();
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

        private void SetUpFilesStorageProvider()
        {
            _filesStorageProvider = Substitute.For<IFilesStorageProvider>();
            _filesStorageProvider.CreateContainerName(Arg.Any<string>()).Returns(x => x.ArgAt<string>(0));
        }
    }
}