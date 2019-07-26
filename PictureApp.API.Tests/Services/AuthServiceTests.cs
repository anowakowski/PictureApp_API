﻿using System;
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
using PictureApp.API.Dtos;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Providers;
using PictureApp.API.Services;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        /*
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

        [Test]
        public void Reregister_WhenCalledWithNull_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            var service = new AuthService(Substitute.For<IRepository<User>>(),
                Substitute.For<IRepository<AccountActivationToken>>(), Substitute.For<IUnitOfWork>(),
                Substitute.For<IMapper>(), Substitute.For<IActivationTokenProvider>());

            // ACT
            Func<Task> action = async () => await service.Reregister(null);

            // ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Reregister_WhenCalledEndUserWithGivenEmailDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var userRepository = Substitute.For<IRepository<User>>();
            userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns((User)null);
            var service = new AuthService(userRepository,
                Substitute.For<IRepository<AccountActivationToken>>(), Substitute.For<IUnitOfWork>(),
                Substitute.For<IMapper>(), Substitute.For<IActivationTokenProvider>());

            // ACT
            var userForReregister = new UserForReregisterDto {Email = "user@post.com"};
            Func<Task> action = async () => await service.Reregister(userForReregister);

            // ASSERT
            action.Should().Throw<EntityNotFoundException>()
                .WithMessage($"The user with email: {userForReregister.Email} does not exist in data store");
        }

        [Test]
        public void Reregister_WhenCalledEndUserHasAlreadyActivatedAccount_NotAuthorizedExceptionExpected()
        {
            // ARRANGE
            var actualUser = new User { Email = "user@post.com", IsAccountActivated = true };
            var userRepository = Substitute.For<IRepository<User>>();
            userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { actualUser };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            var service = new AuthService(userRepository,
                Substitute.For<IRepository<AccountActivationToken>>(), Substitute.For<IUnitOfWork>(),
                Substitute.For<IMapper>(), Substitute.For<IActivationTokenProvider>());

            // ACT
            var userForReregister = new UserForReregisterDto { Email = actualUser.Email };
            Func<Task> action = async () => await service.Reregister(userForReregister);

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
            var userRepository = Substitute.For<IRepository<User>>();
            userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { actualUser };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            User userToUpdate = null;
            userRepository.When(x => x.Update(Arg.Any<User>())).Do(x => userToUpdate = x.ArgAt<User>(0));
            var activationTokenProvider = Substitute.For<IActivationTokenProvider>();
            activationTokenProvider.CreateToken().Returns(newActivationToken.Token);
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var accountActivationTokenRepository = Substitute.For<IRepository<AccountActivationToken>>();
            accountActivationTokenRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns((AccountActivationToken)null);
            var expected = new User
            {
                Id = actualUser.Id,
                Email = actualUser.Email,
                ActivationToken = new AccountActivationToken {Token = newActivationToken.Token}
            };
            var service = new AuthService(userRepository,
                accountActivationTokenRepository, unitOfWork,
                Substitute.For<IMapper>(), activationTokenProvider);

            // ACT
            await service.Reregister(new UserForReregisterDto {Email = actualUser .Email});

            // ASSERT
            userToUpdate.Should().BeEquivalentTo(expected);
            await unitOfWork.Received().CompleteAsync();
        }

        [Test]
        public async Task Reregister_WhenCalledAndUserHasAlreadyToken_UpdatedTokenExpected()
        {
            // ARRANGE
            var actualUser = new User { Id = 99, Email = "user@post.com", IsAccountActivated = false };
            var newActivationToken = new AccountActivationToken { Token = "The new token" };
            var userRepository = Substitute.For<IRepository<User>>();
            userRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(x =>
            {
                var store = new List<User> { actualUser };
                return store.Single(x.ArgAt<Expression<Func<User, bool>>>(0).Compile());
            });
            var activationTokenProvider = Substitute.For<IActivationTokenProvider>();
            activationTokenProvider.CreateToken().Returns(newActivationToken.Token);
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var accountActivationTokenRepository = Substitute.For<IRepository<AccountActivationToken>>();
            accountActivationTokenRepository.SingleOrDefaultAsync(Arg.Any<Expression<Func<AccountActivationToken, bool>>>())
                .Returns(x =>
                    {
                        var store = new List<AccountActivationToken> { new AccountActivationToken {Token = "The old token", UserId = actualUser.Id} };
                        return store.Single(x.ArgAt<Expression<Func<AccountActivationToken, bool>>>(0).Compile());
                    }
                );
            AccountActivationToken accountActivationTokenToUpdate = null;
            accountActivationTokenRepository.When(x => x.Update(Arg.Any<AccountActivationToken>())).Do(x =>
                accountActivationTokenToUpdate = x.ArgAt<AccountActivationToken>(0));
            var expected = new AccountActivationToken
            {
                UserId = actualUser.Id,
                Token = newActivationToken.Token
            };
            var service = new AuthService(userRepository,
                accountActivationTokenRepository, unitOfWork,
                Substitute.For<IMapper>(), activationTokenProvider);

            // ACT
            await service.Reregister(new UserForReregisterDto { Email = actualUser.Email });

            // ASSERT
            accountActivationTokenToUpdate.Should().BeEquivalentTo(expected);
            await unitOfWork.Received().CompleteAsync();
        }
        */
    }
}
