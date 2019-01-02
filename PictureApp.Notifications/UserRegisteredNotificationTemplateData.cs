using System;

namespace PictureApp.Notifications
{
    public class UserRegisteredNotificationTemplateData : NotificationTemplateData
    {
        public string ActivationUri
        {
            get => GetValue($"{{{nameof(ActivationUri)}}}");
            private set => SetDataContainerItem($"{{{nameof(ActivationUri)}}}", value);
        }

        public string UserName
        {
            get => GetValue($"{{{nameof(UserName)}}}");
            private set => SetDataContainerItem($"{{{nameof(UserName)}}}", value);
        }

        public UserRegisteredNotificationTemplateData(string activationUri, string userName)
        {
            ActivationUri = activationUri;
            UserName = userName;
        }

        protected override string GetTemplateAbbreviation()
        {
            return "ARR";
        }
    }
}
