using System;
using System.Net.Mail;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Dtos;
using PictureApp.API.Providers;
using PictureApp.API.Services;
using PictureApp.Notifications;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class EmailNotificationServiceTests
    {
        [Test]
        public void Ctor_WhenCalledWithNullFirstDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new EmailNotificationService(null, Substitute.For<INotificationTemplateService>());

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNullSecondDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new EmailNotificationService(Substitute.For<IEmailClientProvider>(), null);

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithAllNotNullDependencies_ExceptionDoesNotThrow()
        {
            // ARRANGE
            Action action = () => new EmailNotificationService(Substitute.For<IEmailClientProvider>(),
                Substitute.For<INotificationTemplateService>());

            // ACT & ASSERT
            action.Should().NotThrow();
        }

        [Test]
        public void SendAsync_WhenCalledWithNullNotificationTemplateData_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            var service = new EmailNotificationService(Substitute.For<IEmailClientProvider>(), Substitute.For<INotificationTemplateService>());

            Func<Task> action = async () => await service.SendAsync("user@post.com", null);

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void SendAsync_WhenCalledWithIncorrectRecipientEmail_ArgumentExceptionExpected()
        {
            // ARRANGE
            var service = new EmailNotificationService(Substitute.For<IEmailClientProvider>(), Substitute.For<INotificationTemplateService>());
            var recipientEmail = "incorrect recipient email";
            Func<Task> action = async () => await service.SendAsync(recipientEmail, Substitute.For<INotificationTemplateData>());

            // ACT & ASSERT
            action.Should().Throw<ArgumentException>()
                .WithMessage($"Incorrect format for passed recipient email: `{recipientEmail}`.");
        }

        [Test]
        public async Task SendAsync_WhenCalled_AttemptToSendAnEmailExpected()
        {
            // ARRANGE
            var notification = new NotificationDto { Subject = "The subject", Body = "The body" };
            var notificationTemplateService = Substitute.For<INotificationTemplateService>();
            notificationTemplateService.CreateNotification(Arg.Any<INotificationTemplateData>()).Returns(notification);            
            MailAddress actualEmailRecipient = null;
            var actualEmailSubject = string.Empty;
            var actualEmailBody = string.Empty;
            var emailClientProvider = Substitute.For<IEmailClientProvider>();
            emailClientProvider.When(x => x.SendAsync(Arg.Any<MailAddress>(), Arg.Any<string>(), Arg.Any<string>())).Do(
                x =>
                {
                    actualEmailRecipient = x.ArgAt<MailAddress>(0);
                    actualEmailSubject = x.ArgAt<string>(1);
                    actualEmailBody = x.ArgAt<string>(2);
                });
            var service = new EmailNotificationService(emailClientProvider, notificationTemplateService);

            // ACT
            var recipientEmail = "user@post.com";
            await service.SendAsync(recipientEmail, Substitute.For<INotificationTemplateData>());

            // ASSERT
            actualEmailRecipient.Should().BeEquivalentTo(new MailAddress(recipientEmail));
            actualEmailSubject.Should().BeEquivalentTo(notification.Subject);
            actualEmailBody.Should().BeEquivalentTo(notification.Body);
        }
    }
}