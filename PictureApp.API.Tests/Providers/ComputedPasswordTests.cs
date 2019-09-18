using System.Text;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using PictureApp.API.Providers;

namespace PictureApp.API.Tests.Providers
{
    [TestFixture]
    public class ComputedPasswordTests
    {
        [Test]
        public void Hash_WhenInstanceCreated_ProperValueInCorrespondingPropertyExpected()
        {
            // ARRANGE
            var hash = Encoding.UTF8.GetBytes("the password hash");

            // ACT
            var sut = ComputedPassword.Create(hash, Arg.Any<byte[]>());

            // ASSERT
            sut.Hash.Should().BeEquivalentTo(hash);
        }

        [Test]
        public void Salt_WhenInstanceCreated_ProperValueInCorrespondingPropertyExpected()
        {
            // ARRANGE
            var salt = Encoding.UTF8.GetBytes("the password salt");

            // ACT
            var sut = ComputedPassword.Create(Arg.Any<byte[]>(), salt);

            // ASSERT
            sut.Salt.Should().BeEquivalentTo(salt);
        }

        [Test]
        public void EqualityOperator_WhenTwoInstancesAreEqual_TrueExpected()
        {
            // ARRANGE
            var computedPasswordHash1 = Encoding.UTF8.GetBytes("the password hash");
            var computedPasswordSalt1 = Encoding.UTF8.GetBytes("the password salt");
            var computedPassword1 = ComputedPassword.Create(computedPasswordHash1, computedPasswordSalt1);
            var computedPasswordHash2 = Encoding.UTF8.GetBytes("the password hash");
            var computedPasswordSalt2 = Encoding.UTF8.GetBytes("the password salt");
            var computedPassword2 = ComputedPassword.Create(computedPasswordHash2, computedPasswordSalt2);

            // ACT
            var result = computedPassword1 == computedPassword2;

            // ASSERT
            result.Should().BeTrue();
        }

        [Test]
        public void EqualityOperator_WhenTwoInstancesAreNotEqual_FalseExpected()
        {
            // ARRANGE
            var computedPasswordHash1 = Encoding.UTF8.GetBytes("the password hash 1");
            var computedPasswordSalt1 = Encoding.UTF8.GetBytes("the password salt 1");
            var computedPassword1 = ComputedPassword.Create(computedPasswordHash1, computedPasswordSalt1);
            var computedPasswordHash2 = Encoding.UTF8.GetBytes("the password hash 2");
            var computedPasswordSalt2 = Encoding.UTF8.GetBytes("the password salt 2");
            var computedPassword2 = ComputedPassword.Create(computedPasswordHash2, computedPasswordSalt2);

            // ACT
            var result = computedPassword1 == computedPassword2;

            // ASSERT
            result.Should().BeFalse();
        }
    }
}
