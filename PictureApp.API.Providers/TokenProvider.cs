using System;
using System.Linq;
using PictureApp.API.Extensions;

namespace PictureApp.API.Providers
{
    public class TokenProvider : ITokenProvider
    {
        public string CreateToken()
        {
            var whenCreated = BitConverter.GetBytes(SystemTime.Now().ToBinary());
            var key = SystemGuid.NewGuid().ToByteArray();
            return Convert.ToBase64String(whenCreated.Concat(key).ToArray());
        }

        public bool IsTokenExpired(string token, int expirationTimeInHours)
        {
            bool result;
            try
            {
                var data = Convert.FromBase64String(token);
                result = DateTime.FromBinary(BitConverter.ToInt64(data, 0)) <
                         SystemTime.Now().AddHours(-expirationTimeInHours);
            }
            catch (FormatException ex)
            {
                result = false;
            }

            return result;
        }
    }    
}
