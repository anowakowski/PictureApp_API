using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace PictureApp.Messaging
{
    public class PhotoUploadedNotificationHandler : INotificationHandler<PhotoUploadedNotificationEvent>
    {
        public Task Handle(PhotoUploadedNotificationEvent notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
