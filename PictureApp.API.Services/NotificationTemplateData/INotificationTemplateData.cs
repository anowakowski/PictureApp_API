using System.Collections.Generic;

namespace PictureApp.API.Services.NotificationTemplateData
{
    public interface INotificationTemplateData
    {
        string TemplateAbbreviation { get; }

        IEnumerable<string> GetKeys();

        string GetValue(string key);
    }
}
