using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Services;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class UserServiceTests : GuardClauseAssertionTests<UserService>
    {
        [Test]
        public void GetUser_WhenCalledWithUnknownUser_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var repository = Substitute.For<IRepository<User>>();
            repository.Find(Arg.Any<Expression<Func<User, bool>>>()).Returns(new List<User>());
            var service = new UserService(repository,
                Substitute.For<IMapper>());
            var userId = 0;
            Assert.ThrowsAsync<EntityNotFoundException>(async () => await service.GetUser(userId, Arg.Any<Func<User, UserForDetailedDto>>()));
        }

        [Test]
        public void GetUser_WhenCalledWithEmailThatIsNotAssociatedWithAnyUser_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var repository = Substitute.For<IRepository<User>>();
            repository.Find(Arg.Any<Expression<Func<User, bool>>>()).Returns(new List<User>());
            var service = new UserService(repository,
                Substitute.For<IMapper>());
            var userEmail = "name.surname@domain.com";
            Func<UserForDetailedDto> action = () => service.GetUser(userEmail);

            // ACT & ASSERT
            action.Should().Throw<EntityNotFoundException>().WithMessage($"User identifies by email {userEmail} does not exist in data store");
        }
    }
}