using System.Collections.Generic;

namespace PictureApp.API.Services.NotificationTemplates
{
    public interface INotificationTemplateData
    {
        string TemplateAbbreviation { get; }

        IDictionary<string, string> GetData();
    }
}
