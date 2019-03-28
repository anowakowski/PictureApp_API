using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using PictureApp.API.Extensions;
using PictureApp.API.Providers;

namespace PictureApp.API.Tests.Providers
{
    [TestFixture]
    public class TokenProviderTests
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
        public void IsTokenExpired_WhenCalledAndTokenIsOlderThanGivenTime_TrueExpected()
        {
            // ARRANGE
            var provider = new TokenProvider();
            var token = provider.CreateToken();
            var expirationTime = 24;
            var time = DateTime.UtcNow.AddHours(expirationTime).AddMinutes(1);
            SystemTime.Set(() => time);

            // ACT
            var actual = provider.IsTokenExpired(token, expirationTime);

            // ASSERT
            actual.Should().BeTrue();
        }

        [Test]
        public void IsTokenExpired_WhenCalledAndTokenIsNotOlderThanGivenTime_FalseExpected()
        {
            // ARRANGE
            var provider = new TokenProvider();
            var token = provider.CreateToken();
            var expirationTime = 24;
            var time = DateTime.UtcNow.AddHours(23).AddMinutes(59).AddSeconds(59);
            SystemTime.Set(() => time);

            // ACT
            var actual = provider.IsTokenExpired(token, expirationTime);

            // ASSERT
            actual.Should().BeFalse();
        }

        [Test]
        public void IsTokenExpired_WhenCalledAndTokenIsInNotValidFormat_FalseExpected()
        {
            // ARRANGE
            var provider = new TokenProvider();            
            var token = "this is not valid token";
            var expirationTime = 24;
            var time = DateTime.UtcNow.AddHours(23).AddMinutes(59).AddSeconds(59);
            SystemTime.Set(() => time);

            // ACT
            var actual = provider.IsTokenExpired(token, expirationTime);

            // ASSERT
            actual.Should().BeFalse();
        }
    }
}
