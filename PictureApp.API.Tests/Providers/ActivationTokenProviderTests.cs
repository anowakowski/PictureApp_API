using System;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using PictureApp.API.Extensions;
using PictureApp.API.Providers;

namespace PictureApp.API.Tests.Providers
{
    [TestFixture]
    public class ActivationTokenProviderTests
    {
        [TearDown]
        public void TearDown()
        {
            SystemTime.Reset();
            SystemGuid.Reset();
        }

        [Test]
        public void CreateToken_WhenCalled_ProperTokenExpected()
        {
            // ARRANGE
            var provider = new TokenProvider();
            var time = DateTime.Now;
            SystemTime.Set(() => time);
            var key = SystemGuid.NewGuid();
            SystemGuid.Set(() => key);
            var expected = Convert.ToBase64String(BitConverter.GetBytes(time.ToBinary())
                .Concat(key.ToByteArray()).ToArray());

            // ACT
            var actual = provider.CreateToken();

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void IsTokenExpired_WhenCalledAndTokenIsOlderThan24Hours_TrueExpected()
        {
            // ARRANGE
            var provider = new TokenProvider();
            var token = provider.CreateToken();
            var time = DateTime.UtcNow.AddHours(24).AddMinutes(1);
            SystemTime.Set(() => time);

            // ACT
            var actual = provider.IsTokenExpired(token);

            // ASSERT
            actual.Should().BeTrue();
        }

        [Test]
        public void IsTokenExpired_WhenCalledAndTokenIsNotOlderThan24Hours_FalseExpected()
        {
            // ARRANGE
            var provider = new TokenProvider();
            var token = provider.CreateToken();
            var time = DateTime.UtcNow.AddHours(23).AddMinutes(59).AddSeconds(59);
            SystemTime.Set(() => time);

            // ACT
            var actual = provider.IsTokenExpired(token);

            // ASSERT
            actual.Should().BeFalse();
        }

        [Test]
        public void PasswordTest()
        {
            var plainPassword = "password";
            byte[] passwordHash1;
            byte[] passwordHash2;
            byte[] passwordSalt;
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordHash1 = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainPassword));
                passwordSalt = hmac.Key;
                //return (hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), hmac.Key);
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                passwordHash2 = hmac.ComputeHash(Encoding.UTF8.GetBytes(plainPassword));               
                //return (hmac.ComputeHash(Encoding.UTF8.GetBytes(password)), hmac.Key);
            }

            if (passwordHash1 == passwordHash2)
            {
                Console.WriteLine("They are common");
            }
        }
    }
}
