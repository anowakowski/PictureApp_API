using System.Collections.Generic;

namespace PictureApp.Notifications
{
    public interface INotificationTemplateData
    {
        string TemplateAbbreviation { get; }

        IEnumerable<string> GetKeys();

        string GetValue(string key);
    }
}
