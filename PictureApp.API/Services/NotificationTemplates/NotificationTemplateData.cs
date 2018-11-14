using System.Collections.Generic;

namespace PictureApp.API.Services.NotificationTemplates
{
    public abstract class NotificationTemplateData : INotificationTemplateData
    {
        public string TemplateAbbreviation => GetTemplateAbbreviation();

        public IDictionary<string, string> GetData()
        {
            throw new System.NotImplementedException();
        }

        protected abstract string GetTemplateAbbreviation();
    }
}
