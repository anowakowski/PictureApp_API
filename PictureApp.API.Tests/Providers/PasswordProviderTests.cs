using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Internal;
using NUnit.Framework;
using PictureApp.API.Providers;

namespace PictureApp.API.Tests.Providers
{
    [TestFixture]
    public class PasswordProviderTests
    {
        [Test]
        public void CreatePasswordHash_WhenCalledWithoutSalt_ProperComputedPasswordExpected()
        {
            // ARRANGE
            var sut = new PasswordProvider();
            var plainPassword = "the password";

            // ACT
            var computedPassword = sut.CreatePasswordHash(plainPassword);

            // ASSERT
            computedPassword.Should().NotBeNull();
            computedPassword.Hash.Any().Should().BeTrue();
            computedPassword.Salt.Any().Should().BeTrue();            
        }

        [Test]
        public void CreatePasswordHash_WhenCalledWithSalt_ProperComputedPasswordExpected()
        {
            // ARRANGE
            var sut = new PasswordProvider();
            var salt = Encoding.UTF8.GetBytes("the salt");
            var plainPassword = "the password";
            var hmac = new System.Security.Cryptography.HMACSHA512(salt);
            var expected = ComputedPassword.Create(hmac.ComputeHash(Encoding.UTF8.GetBytes(plainPassword)), hmac.Key);

            // ACT
            var actual = sut.CreatePasswordHash(plainPassword, salt);

            // ASSERT
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
