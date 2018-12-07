using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Services;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        [Test]
        public void Ctor_WhenCalledWithNullFirstDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new UserService(null, Substitute.For<IMapper>());

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNullSecondDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new UserService(Substitute.For<IRepository<User>>(), null);

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }    

        [Test]
        public void GetUser_WhenCalledWithUnknownUser_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var repository = Substitute.For<IRepository<User>>();
            repository.Find(Arg.Any<Expression<Func<User, bool>>>()).Returns(new List<User>());
            var service = new UserService(repository,
                Substitute.For<IMapper>());
            var userId = 0;    
            Func<UserForDetailedDto> action = () => service.GetUser(userId);

            // ACT & ASSERT
            action.Should().Throw<EntityNotFoundException>().WithMessage($"user by id {userId} not found");
        }  
    }
}