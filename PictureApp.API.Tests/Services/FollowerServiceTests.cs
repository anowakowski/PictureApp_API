using System;
using NUnit.Framework;
using PictureApp.API.Services;
using NSubstitute;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Models;
using PictureApp.API.Data;
using AutoMapper;
using FluentAssertions;
using PictureApp.API.Dtos;
using System.Threading.Tasks;
using PictureApp.API.Extensions.Exceptions;
using System.Linq.Expressions;
using System.Collections.Generic;

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
        public void SetUpFollower_WhenCalledCorrect_ShouldCallFollowRepositoryAddAndSave()
        {
            var userService = Substitute.For<IUserService>();
            var userFollowerRepository = Substitute.For<IRepository<UserFollower>>();

            userFollowerRepository.AnyAsync(Arg.Any<Expression<Func<UserFollower, bool>>>()).Returns(false);

            var userRepository = Substitute.For<IRepository<User>>();
            var unitOfWork = Substitute.For<IUnitOfWork>();
            var mapper = Substitute.For<IMapper>();

            var service = new FollowerService(userService, userFollowerRepository, userRepository, unitOfWork, mapper);

            Action action = async () => await service.SetUpFollower(1, 2);

            userFollowerRepository.Received().AddAsync(Arg.Any<UserFollower>());
        }    
    }
}