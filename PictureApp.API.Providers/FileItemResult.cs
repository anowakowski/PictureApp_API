using System;
using System.Collections.Generic;
using System.Text;

namespace PictureApp.API.Providers
{
    public class FileItemResult
    {
        public string FileId { get; private set; }

        private FileItemResult()
        {
        }

        public static FileItemResult Create(string fileId)
        {
            return new FileItemResult
            {
                FileId = fileId
            };
        }
    }
}