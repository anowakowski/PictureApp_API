using System.Collections.Generic;

namespace PictureApp.API.Services.NotificationTemplateData
{
    public abstract class NotificationTemplateData : INotificationTemplateData
    {
        protected IDictionary<string, string> DataContainer = new Dictionary<string, string>();

        public string TemplateAbbreviation => GetTemplateAbbreviation();

        public IEnumerable<string> GetKeys()
        {
            return DataContainer.Keys;
        }

        public string GetValue(string key)
        {
            return DataContainer[key];
        }
        
        protected abstract string GetTemplateAbbreviation();
    }
}
