using System;

namespace PictureApp.Notifications
{
    public class UserRegisteredNotificationTemplateData : NotificationTemplateData
    {
        public string ActivationToken
        {
            get => GetValue(nameof(ActivationToken)); // TODO: sprawdzenie czy klucz istnieje, jezeli brak to zwrocenie domyslnej wartosci
            private set => SetDataContainerItem(nameof(ActivationToken), value);
        }

        public string UserName
        {
            get => GetValue(nameof(UserName));
            private set => SetDataContainerItem(nameof(UserName), value);
        }

        public UserRegisteredNotificationTemplateData(string activationToken, string userName)
        {
            ActivationToken = activationToken;
            UserName = userName;
        }

        protected override string GetTemplateAbbreviation()
        {
            throw new NotImplementedException();
        }
    }
}
