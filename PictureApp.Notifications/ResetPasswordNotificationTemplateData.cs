namespace PictureApp.Notifications
{
    public class ResetPasswordNotificationTemplateData : NotificationTemplateData
    {
        public string ResetPasswordUri
        {
            get => GetValue($"{{{nameof(ResetPasswordUri)}}}");
            private set => SetDataContainerItem($"{{{nameof(ResetPasswordUri)}}}", value);
        }

        public string UserName
        {
            get => GetValue($"{{{nameof(UserName)}}}");
            private set => SetDataContainerItem($"{{{nameof(UserName)}}}", value);
        }

        public ResetPasswordNotificationTemplateData(string resetPasswordUri, string userName)
        {
            ResetPasswordUri = resetPasswordUri;
            UserName = userName;
        }

        protected override string GetTemplateAbbreviation()
        {
            return "RPS";
        }
    }
}
