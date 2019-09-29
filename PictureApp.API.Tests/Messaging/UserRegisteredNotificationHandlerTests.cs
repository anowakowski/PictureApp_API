using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Dtos.UserDto;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Services;
using PictureApp.Messaging;
using PictureApp.Notifications;

namespace PictureApp.API.Tests.Messaging
{
    [TestFixture]
    public class UserRegisteredNotificationHandlerTests : GuardClauseAssertionTests<UserRegisteredNotificationHandler>
    {
        [Test]
        public void Handle_WhenCalledAndUserDoesNotExist_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var userEmail = "user@post.com";
            var configuration = Substitute.For<IConfiguration>();
            var userService = Substitute.For<IUserService>();
            userService.GetUser(default(string)).ReturnsForAnyArgs((UserForDetailedDto)null);
            var notificationService = Substitute.For<INotificationService>();
            var sut = new UserRegisteredNotificationHandler(notificationService, userService, configuration);
            var userRegisteredNotificationEvent = new UserRegisteredNotificationEvent(userEmail);
            Func<Task> action = async () => await sut.Handle(userRegisteredNotificationEvent, new CancellationToken());

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
            configurationSection.Value = "http://localhost:4200/AccountActivate/{token}";
            configuration.GetSection("AppSettings:AccountActivationUriFormat").Returns(configurationSection);

            var userService = Substitute.For<IUserService>();
            var user = new UserForDetailedDto { Email = userEmail, ActivationToken = "activation_token"};
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
                new UserRegisteredNotificationTemplateData($"http://localhost:4200/AccountActivate/{user.ActivationToken}",
                    user.Username);

            var sut = new UserRegisteredNotificationHandler(notificationService, userService, configuration);
            var userRegisteredNotificationEvent = new UserRegisteredNotificationEvent(userEmail);

            // ACT
            await sut.Handle(userRegisteredNotificationEvent, new CancellationToken());

            // ASSERT
            actualRecipient.Should().BeEquivalentTo(userEmail);
            actualNotificationTemplateData.Should().BeEquivalentTo(expectedNotificationTemplateData);
        }
    }
}