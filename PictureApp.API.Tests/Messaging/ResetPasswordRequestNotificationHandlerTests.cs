using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Dtos;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Services;
using PictureApp.Messaging;
using PictureApp.Notifications;

namespace PictureApp.API.Tests.Messaging
{
    [TestFixture]
    public class ResetPasswordRequestNotificationHandlerTests
    {
        [Test]
        public void Ctor_WhenCalledWithNullFirstDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new ResetPasswordRequestNotificationHandler(null,
                Substitute.For<IUserService>(), Substitute.For<IConfiguration>());

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNullSecondDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new ResetPasswordRequestNotificationHandler(Substitute.For<INotificationService>(),
                null, Substitute.For<IConfiguration>());

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNullThirdDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new ResetPasswordRequestNotificationHandler(Substitute.For<INotificationService>(),
                Substitute.For<IUserService>(), null);

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNotNullDependency_ExceptionDoesNotThrow()
        {
            // ARRANGE
            Action action = () => new ResetPasswordRequestNotificationHandler(Substitute.For<INotificationService>(),
                Substitute.For<IUserService>(), Substitute.For<IConfiguration>());

            // ACT & ASSERT
            action.Should().NotThrow();
        }

        [Test]
        public void Handle_WhenCalledAndUserDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var userEmail = "user@post.com";
            var configuration = Substitute.For<IConfiguration>();
            var userService = Substitute.For<IUserService>();            
            userService.GetUser(default(string)).ReturnsForAnyArgs((UserForDetailedDto)null);

            var notificationService = Substitute.For<INotificationService>();            
            var sut = new ResetPasswordRequestNotificationHandler(notificationService, userService, configuration);
            var resetPasswordRequestNotificationEvent = new ResetPasswordRequestNotificationEvent(userEmail);
            Func<Task> action = async () => await sut.Handle(resetPasswordRequestNotificationEvent, new CancellationToken());

            // ACT & ASSERT
            action.Should().Throw<EntityNotFoundException>()
                .WithMessage($"The user with given email: {userEmail} does not exist in data store");
        }

        [Test]
        public async Task Handle_WhenCalled_AttemptToSendProperNotificationExpected()
        {
            // ARRANGE
            var userEmail = "user@post.com";
            var configuration = Substitute.For<IConfiguration>();
            var configurationSection = Substitute.For<IConfigurationSection>();
            configurationSection.Value = "http://server.com/ResetPassword/{token}";
            configuration.GetSection("AppSettings:ResetPasswordUriFormat").Returns(configurationSection);
                        
            var userService = Substitute.For<IUserService>();
            var user = new UserForDetailedDto {Email = userEmail, ResetPasswordToken = "reset_password_token", Username = "Alexander" };
            userService.GetUser(default(string)).ReturnsForAnyArgs(user);

            var notificationService = Substitute.For<INotificationService>();
            INotificationTemplateData actualNotificationTemplateData = null;
            string actualRecipient = string.Empty;
            notificationService.When(x => x.SendAsync(Arg.Any<string>(), Arg.Any<INotificationTemplateData>())).Do(x =>
            {
                actualRecipient = x.ArgAt<string>(0);
                actualNotificationTemplateData = x.ArgAt<INotificationTemplateData>(1);
            });
            var expectedNotificationTemplateData =
                new ResetPasswordNotificationTemplateData("http://server.com/ResetPassword/reset_password_token",
                    user.Username);

            var sut = new ResetPasswordRequestNotificationHandler(notificationService, userService, configuration);            
            var resetPasswordRequestNotificationEvent = new ResetPasswordRequestNotificationEvent(userEmail);

            // ACT
            await sut.Handle(resetPasswordRequestNotificationEvent, new CancellationToken());

            // ASSERT
            actualRecipient.Should().BeEquivalentTo(userEmail);
            actualNotificationTemplateData.Should().BeEquivalentTo(expectedNotificationTemplateData);
        }
    }
}
