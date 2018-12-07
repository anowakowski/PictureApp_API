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
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Providers;
using PictureApp.API.Services;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        [Test]
        public void Activate_WhenCalledAndTokenExpired_SecurityTokenExpiredExceptionExpected()
        {
            // ARRANGE
            var activationTokenProvider = Substitute.For<IActivationTokenProvider>();
            activationTokenProvider.IsTokenExpired(Arg.Any<string>()).Returns(true);
            var service = new AuthService(Substitute.For<IRepository<User>>(),
                Substitute.For<IRepository<AccountActivationToken>>(), Substitute.For<IUnitOfWork>(),
                Substitute.For<IMapper>(), activationTokenProvider);

            // ACT
            Func<Task> action = async () => await service.Activate("the expired token");

            // ASSERT
            action.Should().Throw<SecurityTokenExpiredException>().WithMessage("Given token is already expired");
        }

        [Test]
        public void Activate_WhenCalledAndTokenDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var activationTokenProvider = Substitute.For<IActivationTokenProvider>();
            activationTokenProvider.IsTokenExpired(Arg.Any<string>()).Returns(false);
            var accountActivationTokenRepository = Substitute.For<IRepository<AccountActivationToken>>();
            accountActivationTokenRepository
                .SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns(x => (AccountActivationToken) null);
            var service = new AuthService(Substitute.For<IRepository<User>>(),
                accountActivationTokenRepository, Substitute.For<IUnitOfWork>(),
                Substitute.For<IMapper>(), activationTokenProvider);

            // ACT
            Func<Task> action = async () => await service.Activate("the token");

            // ASSERT
            action.Should().Throw<EntityNotFoundException>().WithMessage("Given token the token does not exist in data store");
        }

        [Test]
        public async Task Activate_WhenCalledAndTokenNotExpired_FullAccountActivationExpected()
        {
            // ARRANGE
            var activationTokenProvider = Substitute.For<IActivationTokenProvider>();
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
                Substitute.For<IMapper>(), activationTokenProvider);

            // ACT
            await service.Activate(actualActivationToken.Token);

            // ASSERT
            actualUser.Should().BeEquivalentTo(expected);
        }
    }
}
