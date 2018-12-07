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
using PictureApp.API.Extensions.Exceptions;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class FollowerServiceTests
    {

        [Test]
        public void Ctor_WhenCalledWithNullFirstDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action acttion = () => new FollowerService(null, Substitute.For<IRepository<UserFollower>>(),
                Substitute.For<IUnitOfWork>(), Substitute.For<IMapper>());

            // ACT ASSERT
            acttion.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNullSecondDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new FollowerService(Substitute.For<IUserService>(), null,
                Substitute.For<IUnitOfWork>(), Substitute.For<IMapper>());

            // ACT ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNullThirdDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new FollowerService(Substitute.For<IUserService>(), Substitute.For<IRepository<UserFollower>>(),
                 null, Substitute.For<IMapper>());

            // ACT ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNullFourthDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new FollowerService(Substitute.For<IUserService>(), Substitute.For<IRepository<UserFollower>>(),
                Substitute.For<IUnitOfWork>(), null);

            // ACT ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void SetUpFollower_WhenCalledNonExistsRelationByUserAndFollower_ShouldAddUserFollowerEntityToRepository()
        {
            // ARRANGE
            var userFollowerRepo = Substitute.For<IRepository<UserFollower>>();
            userFollowerRepo.AnyAsync(Arg.Any<Expression<Func<UserFollower, bool>>>()).Returns(false);

            var unitOfWork = Substitute.For<IUnitOfWork>();
            var userService = Substitute.For<IUserService>();
            userService.GetUser(Arg.Any<int>()).Returns(new Dtos.UserForDetailedDto());

            var service = new FollowerService(userService, userFollowerRepo,
                unitOfWork, Substitute.For<IMapper>());

            // ACT
            var userId = 1;
            var recipientId = 2;

            Func<Task> action = async () => await service.SetUpFollower(userId, recipientId);

            // ASSERT
            action.Should().NotThrow<EntityNotFoundException>();
            userFollowerRepo.Received().AddAsync(Arg.Any<UserFollower>());
            unitOfWork.Received().CompleteAsync();
        }

        [Test]
        public void SetUpFollower_WhenCalledWithExistsRelationByUserAndFollower_ShouldNotAddUserFollowerEntityToRepository()
        {
            // ARRANGE
            var userFollowerRepo = Substitute.For<IRepository<UserFollower>>();

            userFollowerRepo.AnyAsync(Arg.Any<Expression<Func<UserFollower, bool>>>()).Returns(true);

            var unitOfWork = Substitute.For<IUnitOfWork>();

            var service = new FollowerService(Substitute.For<IUserService>(), userFollowerRepo,
                unitOfWork, Substitute.For<IMapper>());

            // ACT
            var userId = 1;
            var recipientId = 2;

            Func<Task> action = async () => await service.SetUpFollower(userId, recipientId);

            // ASSERT
            userFollowerRepo.DidNotReceive().AddAsync(Arg.Any<UserFollower>());
            unitOfWork.DidNotReceive().CompleteAsync();
        }

        [Test]
        public void SetUpUnfollower_WhenCalledWithExistsRelationByUserAndFollower_ShouldCallDeleteUserFollowerEntityFromRepository()
        {
            // ARRANGE
            var userFollowerRepo = Substitute.For<IRepository<UserFollower>>();

            userFollowerRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<UserFollower, bool>>>()).Returns(new UserFollower());

            var unitOfWork = Substitute.For<IUnitOfWork>();
            var userService = Substitute.For<IUserService>();
            userService.GetUser(Arg.Any<int>()).Returns(new Dtos.UserForDetailedDto());

            var service = new FollowerService(userService, userFollowerRepo,
                unitOfWork, Substitute.For<IMapper>());

            // ACT
            var userId = 1;
            var recipientId = 2;

            Func<Task> action = async () => await service.SetUpUnfollower(userId, recipientId);

            // ASSERT
            action.Should().NotThrow<EntityNotFoundException>();
            userFollowerRepo.Received().Delete(Arg.Any<UserFollower>());
            unitOfWork.Received().CompleteAsync();
        }

        [Test]
        public void SetUpUnfollower_WhenCalledWithNonExistsRelationByUserAndFollower_ShouldNotCallDeleteUserFollowerEntityFromRepository()
        {
            // ARRANGE
            var userFollowerRepo = Substitute.For<IRepository<UserFollower>>();

            userFollowerRepo.FirstOrDefaultAsync(Arg.Any<Expression<Func<UserFollower, bool>>>()).Returns((Task<UserFollower>)null);

            var unitOfWork = Substitute.For<IUnitOfWork>();

            var service = new FollowerService(Substitute.For<IUserService>(), userFollowerRepo,
                unitOfWork, Substitute.For<IMapper>());

            // ACT
            var userId = 1;
            var recipientId = 2;

            Func<Task> action = async () => await service.SetUpUnfollower(userId, recipientId);

            // ASSERT
            action.Should().NotThrow<EntityNotFoundException>();
            userFollowerRepo.DidNotReceive().Delete(Arg.Any<UserFollower>());
            unitOfWork.DidNotReceive().CompleteAsync();
        }
    }
}