using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
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
        // TODO: provide method, which will create instance of a SUT
        /*
        [Test]
        public void Activate_WhenCalledAndTokenExpired_SecurityTokenExpiredExceptionExpected()
        {
            // ARRANGE
            var activationTokenProvider = Substitute.For<ITokenProvider>();
            activationTokenProvider.IsTokenExpired(Arg.Any<string>()).Returns(true);
            var service = new AuthService(Substitute.For<IRepository<User>>(),
                Substitute.For<IRepository<AccountActivationToken>>(), Substitute.For<IUnitOfWork>(),
                Substitute.For<IMapper>(), activationTokenProvider, Substitute.For<ITokenProvider>(), Substitute.For<IPasswordProvider>());

            // ACT
            Func<Task> action = async () => await service.Activate("the expired token");

            // ASSERT
            action.Should().Throw<SecurityTokenExpiredException>().WithMessage("Given token is already expired");
        }

        [Test]
        public void Activate_WhenCalledAndTokenDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var activationTokenProvider = Substitute.For<ITokenProvider>();
            activationTokenProvider.IsTokenExpired(Arg.Any<string>()).Returns(false);
            var accountActivationTokenRepository = Substitute.For<IRepository<AccountActivationToken>>();
            accountActivationTokenRepository
                .SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns(x => (AccountActivationToken) null);
            var service = new AuthService(Substitute.For<IRepository<User>>(),
                accountActivationTokenRepository, Substitute.For<IUnitOfWork>(),
                Substitute.For<IMapper>(), activationTokenProvider, Substitute.For<IPasswordProvider>());

            // ACT
            Func<Task> action = async () => await service.Activate("the token");

            // ASSERT
            action.Should().Throw<EntityNotFoundException>().WithMessage("Given token the token does not exist in data store");
        }

        [Test]
        public async Task Activate_WhenCalledAndTokenNotExpired_FullAccountActivationExpected()
        {
            // ARRANGE
            var activationTokenProvider = Substitute.For<ITokenProvider>();
            activationTokenProvider.IsTokenExpired(Arg.Any<string>()).Returns(false);
            var actualUser = new User {Id = 99};
            var actualActivationToken = new AccountActivationToken
                {Token = "The token", UserId = actualUser.Id};
            var accountActivationTokenRepository = Substitute.For<IRepository<AccountActivationToken>>();
            accountActivationTokenRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns(x =>
                    {
                        var store = new List<AccountActivationToken> { actualActivationToken };
                        return store.Single(x.ArgAt<Expression<Func<AccountActivationToken, bool>>>(0).Compile());
                    }
                );
            actualUser.ActivationToken = actualActivationToken;
            AccountActivationToken tokenToDelete = null;
            accountActivationTokenRepository.When(x => x.Delete(Arg.Any<AccountActivationToken>()))
                .Do(x => tokenToDelete = x.ArgAt<AccountActivationToken>(0));
            var userRepository = Substitute.For<IRepository<User>>();
            userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> {actualUser};
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            var unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWork.When(x => x.CompleteAsync()).Do(x =>
            {
                if (tokenToDelete != null && tokenToDelete.UserId == actualActivationToken.UserId)
                {
                    actualUser.ActivationToken = null;
                }
            });
            var expected = new User {Id = 99, IsAccountActivated = true};
            var service = new AuthService(userRepository,
                accountActivationTokenRepository, unitOfWork,
                Substitute.For<IMapper>(), activationTokenProvider, Substitute.For<IPasswordProvider>());

            // ACT
            await service.Activate(actualActivationToken.Token);

            // ASSERT
            actualUser.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ChangePassword_WhenCalledAndUserWithGivenEmailDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var userRepository = Substitute.For<IRepository<User>>();
            userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns((User) null);
            var service = new AuthService(userRepository,
                Substitute.For<IRepository<AccountActivationToken>>(), Substitute.For<IUnitOfWork>(),
                Substitute.For<IMapper>(), Substitute.For<ITokenProvider>(), Substitute.For<IPasswordProvider>());
            var email = "user@post.com";

            // ACT
            Func<Task> action = async () =>
                await service.ChangePassword(email, "the old password", "the new password",
                    "the new password");

            // ASSERT
            action.Should().Throw<EntityNotFoundException>()
                .WithMessage($"The user with email: {email} does not exist in data store");
        }

        [Test]
        public void ChangePassword_WhenCalledAndOldPasswordIsDifferentThanUsersCurrentOne_ArgumentExceptionExpected()
        {
            // ARRANGE
            var userRepository = Substitute.For<IRepository<User>>();
            var user = new User {Email = "user@post.com", PasswordHash = Encoding.ASCII.GetBytes("current password hash")};
            userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { user };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            var service = new AuthService(userRepository,
                Substitute.For<IRepository<AccountActivationToken>>(), Substitute.For<IUnitOfWork>(),
                Substitute.For<IMapper>(), Substitute.For<ITokenProvider>(), Substitute.For<IPasswordProvider>());
            
            // ACT
            Func<Task> action = async () =>
                await service.ChangePassword(user.Email, "the old password", "the new password",
                    "the new password");

            // ASSERT
            action.Should().Throw<ArgumentException>()
                .WithMessage("The given old password does not fit to the current user password");
        }

        [Test]
        public void ChangePassword_WhenCalledAndNewPasswordIsDifferentThanRetypedPassword_ArgumentExceptionExpected()
        {
            // ARRANGE
            var userRepository = Substitute.For<IRepository<User>>();
            var user = new User
            {
                Email = "user@post.com",
                PasswordHash = Encoding.ASCII.GetBytes("current password hash"),
                PasswordSalt = Encoding.ASCII.GetBytes("current password salt")
            };
            userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { user };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            var passwordProvider = new MockPasswordProvider(("the old password", user.PasswordHash, user.PasswordSalt));            
            var service = new AuthService(userRepository,
                Substitute.For<IRepository<AccountActivationToken>>(), Substitute.For<IUnitOfWork>(),
                Substitute.For<IMapper>(), Substitute.For<ITokenProvider>(), passwordProvider);

            // ACT
            Func<Task> action = async () =>
                await service.ChangePassword(user.Email, "the old password", "the new password",
                    "the different retyped new password");

            // ASSERT
            action.Should().Throw<ArgumentException>()
                .WithMessage("The new password is different than retyped new password");
        }

        [Test]
        public async Task ChangePassword_WhenCalledAndAllPassingDataAreAppropriate_AttemptToSaveNewPasswordForGivenUserExpected()
        {
            // ARRANGE
            var userRepository = Substitute.For<IRepository<User>>();
            var user = new User
            {
                Email = "user@post.com",
                PasswordHash = Encoding.ASCII.GetBytes("current password hash"),
                PasswordSalt = Encoding.ASCII.GetBytes("current password salt")
            };
            userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { user };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            User userToUpdate = null;
            userRepository.When(x => x.Update(Arg.Any<User>())).Do(x => userToUpdate = x.ArgAt<User>(0));
            var passwordProvider = new MockPasswordProvider(("the old password", user.PasswordHash, user.PasswordSalt),
                ("the new password", Encoding.ASCII.GetBytes("the new password hash"),
                    Encoding.ASCII.GetBytes("the new password salt")));
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var service = new AuthService(userRepository,
                Substitute.For<IRepository<AccountActivationToken>>(), unitOfWork,
                Substitute.For<IMapper>(), Substitute.For<ITokenProvider>(), passwordProvider);
            var expectedUserToUpdate = new User
            {
                Email = user.Email,
                PasswordHash = Encoding.ASCII.GetBytes("the new password hash"),
                PasswordSalt = Encoding.ASCII.GetBytes("the new password salt")
            };

            // ACT            
            await service.ChangePassword(user.Email, "the old password", "the new password",
                "the new password");

            // ASSERT
            await unitOfWork.Received().CompleteAsync();
            userToUpdate.Should().BeEquivalentTo(expectedUserToUpdate);
        }
    }

    internal class MockPasswordProvider : IPasswordProvider
    {
        private readonly IDictionary<string, (byte[] passwordHash, byte[] passwordSalt)> _passwords;

        public MockPasswordProvider(params (string password, byte[] passwordHash, byte[] passwordSalt)[] passwords)
        {
            _passwords = new Dictionary<string, (byte[] passwordHash, byte[] passwordSalt)>();
            passwords.ForEach(x => _passwords.Add(x.password, (x.passwordHash, x.passwordSalt)));
        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            var pass = _passwords[password];
            passwordHash = pass.passwordHash;
            passwordSalt = pass.passwordSalt;
        }

        public (byte[] passwordHash, byte[] passwordSalt) CreatePasswordHash(string password)
        {
            var pass = _passwords[password];
            return (pass.passwordHash, pass.passwordSalt);
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            var pass = _passwords[password];
            if (passwordHash.Length == pass.passwordHash.Length && passwordSalt.Length == pass.passwordSalt.Length)
            {
                if (passwordHash.Where((t, i) => t != pass.passwordHash[i]).Any())
                {
                    return false;
                }

                if (passwordSalt.Where((t, i) => t != pass.passwordSalt[i]).Any())
                {
                    return false;
                }
            }

            return false;
        }
        */
    }
}
