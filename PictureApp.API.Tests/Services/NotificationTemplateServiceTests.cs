using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Data.Repositories;
using PictureApp.API.Dtos;
using PictureApp.API.Models;
using PictureApp.API.Services;
using PictureApp.Notifications;

namespace PictureApp.API.Tests.Services
{
    [TestFixture]
    public class NotificationTemplateServiceTests : GuardClauseAssertionTests<NotificationTemplateService>
    {
        [Test]
        public void CreateNotification_WhenCalledWithNull_ArgumentNullExceptionExpected()
        {
            // ARRANGE
            var repository = Substitute.For<IRepository<NotificationTemplate>>();
            repository.SingleOrDefaultAsync(Arg.Any<Expression<Func<NotificationTemplate, bool>>>())
                .Returns(new NotificationTemplate());
            var service = new NotificationTemplateService(repository);
            Func<Task> action = async () => await service.CreateNotification(null);

            // ACT & ASSERT
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void CreateNotification_WhenCalledWithTemplateDataNotRelatedToAnyNotificationTemplate_ArgumentExceptionExpected()
        {
            // ARRANGE
            var templateData = Substitute.For<INotificationTemplateData>();
            templateData.TemplateAbbreviation.Returns("ABC");
            var repository = Substitute.For<IRepository<NotificationTemplate>>();
            repository.SingleOrDefaultAsync(Arg.Any<Expression<Func<NotificationTemplate, bool>>>())
                .Returns((NotificationTemplate) null);
            var service = new NotificationTemplateService(repository);
            Func<Task> action = async () => await service.CreateNotification(templateData);

            // ACT & ASSERT
            action.Should().Throw<ArgumentException>().WithMessage(
                $"The notification template with abbreviation: {templateData.TemplateAbbreviation} does not exist in data store.");
        }

        [Test]
        public async Task CreateNotification_WhenCalled_ProperNotificationExpected()
        {
            // ARRANGE
            var templateData = Substitute.For<INotificationTemplateData>();
            templateData.GetKeys().Returns(new List<string>{"{body}"});
            templateData.GetValue("{body}").Returns("The body content");
            var repository = Substitute.For<IRepository<NotificationTemplate>>();
            var notificationTemplate = new NotificationTemplate {Abbreviation = "ABC", Body = "{body}", Subject = "The subject"};
            repository.SingleOrDefaultAsync(Arg.Any<Expression<Func<NotificationTemplate, bool>>>()).Returns(notificationTemplate);
            var expected = new NotificationDto {Body = "The body content", Subject = notificationTemplate.Subject};
            var service = new NotificationTemplateService(repository);

            // ACT
            var actual = await service.CreateNotification(templateData);

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
