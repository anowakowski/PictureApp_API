using System;
using NUnit.Framework;
using PictureApp.API.Services;
using NSubstitute;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Models;
using PictureApp.API.Data;
using AutoMapper;
using FluentAssertions;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using PictureApp.API.Providers;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Services.NotificationTemplateData;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class FollowerServiceTests
    {

        [Test]
        public void Ctor_WhenCalledWithNullFirstDependency_ArgumentNullExceptionExpected()
        {
            Action acttion = () => new FollowerService(null, Substitute.For<IRepository<UserFollower>>(),
                Substitute.For<IRepository<User>>(), Substitute.For<IUnitOfWork>(), Substitute.For<IMapper>());

            acttion.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNullSecondDependency_ArgumentNullExceptionExpected()
        {
            Action action = () => new FollowerService(Substitute.For<IUserService>(), null,
                Substitute.For<IRepository<User>>(), Substitute.For<IUnitOfWork>(), Substitute.For<IMapper>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNullThirdDependency_ArgumentNullExceptionExpected()
        {
            Action action = () => new FollowerService(Substitute.For<IUserService>(), Substitute.For<IRepository<UserFollower>>(),
                null, Substitute.For<IUnitOfWork>(), Substitute.For<IMapper>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNullFourthDependency_ArgumentNullExceptionExpected()
        {
            Action action = () => new FollowerService(Substitute.For<IUserService>(), Substitute.For<IRepository<UserFollower>>(),
                Substitute.For<IRepository<User>>(), null, Substitute.For<IMapper>());

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void SetUpFollower_WhenCalledCorrectUser_ShouldAddEntityToRepository()
        {
            // ARRANGE
            var userFollowerRepo = Substitute.For<IRepository<UserFollower>>();
            userFollowerRepo.AnyAsync(Arg.Any<Expression<Func<UserFollower, bool>>>()).Returns(false);

            var service = new FollowerService(Substitute.For<IUserService>(), userFollowerRepo,
                Substitute.For<IRepository<User>>(), Substitute.For<IUnitOfWork>(), Substitute.For<IMapper>());

            // ACT
            var userId = 1;
            var reciptientId = 2;

            Func<Task> action = async () => await service.SetUpFollower(userId, reciptientId);

            // ASSERT
            action.Should().NotThrow<EntityNotFoundException>();
            userFollowerRepo.Received().AddAsync(Arg.Any<UserFollower>());
        }
    }
}