using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PictureApp.API.Providers
{
    public interface IFileFormatInspectorProvider
    {
        bool ValidateFileFormat(Stream fileStream);
    }
}
