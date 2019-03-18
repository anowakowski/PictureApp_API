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
using MoreLinq.Extensions;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Data;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Providers;
using PictureApp.API.Services;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private ITokenProvider _tokenProvider;
        private IPasswordProvider _passwordProvider;
        private IRepositoryFactory _repositoryFactory;
        private IRepository<AccountActivationToken> _accountActivationTokenRepository;
        private IRepository<ResetPasswordToken> _resetPasswordTokenRepository;
        private IRepository<User> _userRepository;
        private IConfiguration _configuration;

        [SetUp]
        public void SetUp()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _mapper = Substitute.For<IMapper>();
            _tokenProvider = Substitute.For<ITokenProvider>();
            _passwordProvider = Substitute.For<IPasswordProvider>();
            _configuration = Substitute.For<IConfiguration>();

            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _accountActivationTokenRepository = Substitute.For<IRepository<AccountActivationToken>>();
            _repositoryFactory.Create<AccountActivationToken>().Returns(x => _accountActivationTokenRepository);
            _resetPasswordTokenRepository = Substitute.For<IRepository<ResetPasswordToken>>();
            _repositoryFactory.Create<ResetPasswordToken>().Returns(x => _resetPasswordTokenRepository);
            _userRepository = Substitute.For<IRepository<User>>();
            _repositoryFactory.Create<User>().Returns(x => _userRepository);
        }
        
        /*
        [Test]
        public void Activate_WhenCalledAndTokenExpired_SecurityTokenExpiredExceptionExpected()
        {
            // ARRANGE
            _tokenProvider.IsTokenExpired(Arg.Any<string>()).Returns(true);

            // ACT
            Func<Task> action = async () => await GetSUT().Activate("the expired token");

            // ASSERT
            action.Should().Throw<SecurityTokenExpiredException>().WithMessage("Given token is already expired");
        }

        [Test]
        public void Activate_WhenCalledAndTokenDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            _tokenProvider.IsTokenExpired(Arg.Any<string>()).Returns(false);
            _accountActivationTokenRepository
                .SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns(x => (AccountActivationToken) null);            

            // ACT
            Func<Task> action = async () => await GetSUT().Activate("the token");

            // ASSERT
            action.Should().Throw<EntityNotFoundException>()
                .WithMessage("Given token the token does not exist in data store");
        }

        [Test]
        public async Task Activate_WhenCalledAndTokenNotExpired_FullAccountActivationExpected()
        {
            // ARRANGE
            _tokenProvider.IsTokenExpired(Arg.Any<string>()).Returns(false);
            var actualUser = new User {Id = 99};
            var actualActivationToken = new AccountActivationToken
                {Token = "The token", UserId = actualUser.Id};            
            _accountActivationTokenRepository
                .SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns(x =>
                    {
                        var store = new List<AccountActivationToken> {actualActivationToken};
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
            await GetSUT().Activate(actualActivationToken.Token);

            // ASSERT
            actualUser.Should().BeEquivalentTo(expected);
        }
        */
        [Test]
        public void ChangePassword_WhenCalledAndUserWithGivenEmailDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns((User) null);
            var email = "user@post.com";

            // ACT
            Func<Task> action = async () =>
                await GetSUT().ChangePassword(email, "the old password", "the new password",
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
                {Email = "user@post.com", PasswordHash = Encoding.ASCII.GetBytes("current password hash")};
            _userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> {user};
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });

            // ACT
            Func<Task> action = async () =>
                await GetSUT().ChangePassword(user.Email, "the old password", "the new password",
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
                var store = new List<User> {user};
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });            
            _passwordProvider = new MockPasswordProvider(("the old password", ComputedPassword.Create(user.PasswordHash, user.PasswordSalt)));

            // ACT
            Func<Task> action = async () =>
                await GetSUT().ChangePassword(user.Email, "the old password", "the new password",
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
            _passwordProvider = new MockPasswordProvider(
                ("the old password", ComputedPassword.Create(user.PasswordHash, user.PasswordSalt)),
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
            await GetSUT().ChangePassword(user.Email, "the old password", "the new password",
                "the new password");

            // ASSERT
            await _unitOfWork.Received().CompleteAsync();
            userToUpdate.Should().BeEquivalentTo(expectedUserToUpdate);
        }

        // TODO
        // consider which test would be useful in terms of reset/changed password feature
        // - ResetPasswordRequest
        //   + when the email not linked to any user EntityNotFoundException
        //   + when the old token exists - delete it
        //   + link new token with user and save it
        // - ResetPassword
        //   + when token is expired SecurityTokenExpiredException
        //   + when token not exists in data store EntityNotFoundException
        //   + save user with new password

        private IAuthService GetSUT()
        {
            return new AuthService(_repositoryFactory, _unitOfWork, _mapper, _tokenProvider, _passwordProvider, _configuration);
        }
    }

    internal class MockPasswordProvider : IPasswordProvider
    {
        private readonly IDictionary<string, ComputedPassword> _passwords;

        public MockPasswordProvider(params (string password, ComputedPassword computedPassword)[] passwords)
        {
            _passwords = new Dictionary<string, ComputedPassword>();
            passwords.ForEach(x => _passwords.Add(x.password, x.computedPassword));
        }

        public ComputedPassword CreatePasswordHash(string plainPassword, byte[] salt)
        {
            return _passwords[plainPassword];
        }

        ComputedPassword IPasswordProvider.CreatePasswordHash(string plainPassword)
        {
            return _passwords[plainPassword];
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            var pass = _passwords[password];
            if (passwordHash.Length == pass.Hash.Length && passwordSalt.Length == pass.Salt.Length)
            {
                if (passwordHash.Where((t, i) => t != pass.Hash[i]).Any())
                {
                    return false;
                }

                if (passwordSalt.Where((t, i) => t != pass.Salt[i]).Any())
                {
                    return false;
                }
            }

            return false;
        }
    }
}