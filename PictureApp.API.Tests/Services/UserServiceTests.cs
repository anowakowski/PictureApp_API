using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Services;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        [Test]
        public void Ctor_WhenCalledWithNullFirstDependencyForRepository_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new UserService(null, Substitute.For<IMapper>());

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }        
        
    }
}