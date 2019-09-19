using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PictureApp.Messaging;

namespace PictureApp.API.Tests.Messaging
{
    [TestFixture]
    public class PhotoUploadedNotificationHandlerTests : GuardClauseAssertionTests<PhotoUploadedNotificationHandler>
    {
        [Test]
        public async Task Handle_WhenCalled_AttemptToUpdatePhotoForUserAndRemoveItFromFilesStorageExpected()
        {
            // ARRANGE            
            // ACT
            // ASSERT
        }
    }
}