using System;
using System.Collections.Generic;
using System.Text;

namespace PictureApp.API.Providers
{
    public class FileItemResult
    {
        public string FileName { get; private set; }

        private FileItemResult()
        {
        }

        public static FileItemResult Create(string fileName)
        {
            return new FileItemResult
            {
                FileName = fileName
            };
        }
    }
}
