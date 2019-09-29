using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var inspector = new FileFormatInspector();
            var format = inspector.DetermineFileFormat(fileStream);
            if (format == null)
            {
                throw new FormatException("Can not recognize format of given file stream.");
            }

            var sectionValue = _configuration.GetSection("AzureCloud:AllowedMediaTypes").Value;
            var allowedMediaTypes = sectionValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return allowedMediaTypes.Contains(format.MediaType);
        }
    }
}