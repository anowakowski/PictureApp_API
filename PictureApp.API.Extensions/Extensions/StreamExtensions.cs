using System.IO;

namespace PictureApp.API.Extensions.Extensions
{
    public static class StreamExtensions
    {
        public static void ResetPosition(this Stream stream)
        {
            stream.Position = 0;
        }
    }
}