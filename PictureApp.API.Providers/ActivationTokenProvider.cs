using System;
using System.Linq;
using PictureApp.API.Extensions;

namespace PictureApp.API.Providers
{
    public class ActivationTokenProvider : IActivationTokenProvider
    {
        private const int ExpirationTimeInHours = 24;

        public string CreateToken()
        {
            var whenCreated = BitConverter.GetBytes(SystemTime.Now().ToBinary());
            var key = SystemGuid.NewGuid().ToByteArray();
            return Convert.ToBase64String(whenCreated.Concat(key).ToArray());
        }

        public bool IsTokenExpired(string token)
        {
            var data = Convert.FromBase64String(token);
            return DateTime.FromBinary(BitConverter.ToInt64(data, 0)) <
                   SystemTime.Now().AddHours(-ExpirationTimeInHours);
        }
    }
}
