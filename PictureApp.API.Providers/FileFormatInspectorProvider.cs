using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FileSignatures;
using FileSignatures.Formats;
using Microsoft.Extensions.Configuration;

namespace PictureApp.API.Providers
{
    public class FileFormatInspectorProvider : IFileFormatInspectorProvider
    {
        private readonly IConfiguration _configuration;

        public FileFormatInspectorProvider(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public bool ValidateFileFormat(Stream fileStream)
        {
            throw new NotImplementedException();
            
        }
    }
}
