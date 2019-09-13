using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PictureApp.API.Helpers
{
    public static class StreamExtensions
    {
        public static void ResetPosition(this Stream stream)
        {
            stream.Position = 0;
        }
    }
}