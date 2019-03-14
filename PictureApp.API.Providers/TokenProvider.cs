using System;
using System.Linq;
using PictureApp.API.Extensions;

namespace PictureApp.API.Providers
{
    public class TokenProvider : ITokenProvider
    {
        private const int ExpirationTimeInHours = 24; // TODO: move expiration time outside the class

        public class Token
        {
            private int _expirationTimeInHours;

            private Token(string data, int expirationTimeInHours)
            {
                Data = data;
                _expirationTimeInHours = expirationTimeInHours;
            }

            public Token Create(string data, int expirationTimeInHours)
            {
                return new Token(data, expirationTimeInHours);
            }

            public bool IsExpired()
            {
                throw new NotImplementedException();
            }

            public string Data { get; }
        }

        public string CreateToken() // TODO: pass expiration time
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

        // TODO: it is useful to have Token class?
        // - isTokenExpired - when class is created than there can be easily pass expiration time in hours

        // TODO:
        // provide string extension toToken()
    }    
}
