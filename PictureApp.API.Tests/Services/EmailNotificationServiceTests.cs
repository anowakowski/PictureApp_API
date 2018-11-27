using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Mail;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Extensions.Exceptions;
using PictureApp.API.Models;
using PictureApp.API.Providers;
using PictureApp.API.Services;
using PictureApp.API.Services.NotificationTemplateData;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class EmailNotificationServiceTests
    {
        [Test]
        public void Ctor_WhenCalledWithNullFirstDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new EmailNotificationService(null, Substitute.For<IEmailClientProvider>());

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithNullSecondDependency_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            Action action = () => new EmailNotificationService(Substitute.For<IRepository<NotificationTemplate>>(), null);

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Ctor_WhenCalledWithAllNotNullDependencies_ExceptionDoesNotThrow()
        {
            // ARRANGE
            Action action = () => new EmailNotificationService(Substitute.For<IRepository<NotificationTemplate>>(),
                Substitute.For<IEmailClientProvider>());

            // ACT & ASSERT
            action.Should().NotThrow();
        }

        [Test]
        public void SendAsync_WhenCalledWithUnknownNotificationTemplate_EntityNotFoundExceptionExpected()
        {
            // ARRANGE
            var repository = Substitute.For<IRepository<NotificationTemplate>>();
            repository.Find(Arg.Any<Expression<Func<NotificationTemplate, bool>>>()).Returns(new List<NotificationTemplate>());
            var service = new EmailNotificationService(repository,
                Substitute.For<IEmailClientProvider>());
            Func<Task> action = async () => await service.SendAsync("user@post.com", new SampleNotificationTemplateData());

            // ACT & ASSERT
            action.Should().Throw<EntityNotFoundException>().WithMessage("Can not find notification template with following abbreviation: ABR");
        }

        [Test]
        public async Task SendAsync_WhenCalled_AttemptToSendAnEmailExpected()
        {
            // ARRANGE
            var notificationTemplate = new NotificationTemplate
                {Abbreviation = "ABR", Body = "The body content: {content}", Subject = "The subject"};
            var repository = Substitute.For<IRepository<NotificationTemplate>>();
            repository.Find(Arg.Any<Expression<Func<NotificationTemplate, bool>>>()).Returns(
                new List<NotificationTemplate>
                {
                    notificationTemplate
                });
            MailAddress actualEmailRecipient = null;
            var actualEmailSubject = string.Empty;
            var actualEmailBody = string.Empty;
            var emailClientProvider = Substitute.For<IEmailClientProvider>();
            emailClientProvider.When(x => x.SendAsync(Arg.Any<MailAddress>(), Arg.Any<string>(), Arg.Any<string>())).Do(x =>
            {
                actualEmailRecipient = x.ArgAt<MailAddress>(0);
                actualEmailSubject = x.ArgAt<string>(1);
                actualEmailBody = x.ArgAt<string>(2);
            });
            var service = new EmailNotificationService(repository,
                emailClientProvider);

            // ACT
            await service.SendAsync("user@post.com", new SampleNotificationTemplateData());

            // ASSERT
            actualEmailRecipient.Should().BeEquivalentTo(new MailAddress("user@post.com"));
            actualEmailSubject.Should().BeEquivalentTo(notificationTemplate.Subject);
            actualEmailBody.Should()
                .BeEquivalentTo(notificationTemplate.Body.Replace("\\{content\\}",
                    "The body content: Lorem ipsum dolor sit amet")
                );
        }

        private class SampleNotificationTemplateData : INotificationTemplateData
        {
            public string TemplateAbbreviation => "ABR";
            private readonly IDictionary<string, string> _dictionary = new Dictionary<string, string>();

            public SampleNotificationTemplateData()
            {
                _dictionary.Add("\\{content\\}",
                    "Lorem ipsum dolor sit amet");
            }

            public IEnumerable<string> GetKeys()
            {
                return _dictionary.Keys;
            }

            public string GetValue(string key)
            {
                return _dictionary[key];
            }
        }
    }
}