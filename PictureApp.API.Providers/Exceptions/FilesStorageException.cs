using System;

namespace PictureApp.API.Providers.Exceptions
{
    public class FilesStorageException : Exception
    {
        public FilesStorageException(string message) : base(message)
        {

        }
        public FilesStorageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
