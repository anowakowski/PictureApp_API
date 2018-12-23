using System.Collections.Generic;

namespace PictureApp.Notifications
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

        protected void SetDataContainerItem(string key, string value)
        {
            if (DataContainer.ContainsKey(key))
            {
                DataContainer[key] = value;
            }
            else
            {
                DataContainer.Add(key, value);
            }
        }
        
        protected abstract string GetTemplateAbbreviation();
    }
}
