using NUnit.Framework;

namespace PictureApp.API.Tests.Providers
{
    [TestFixture]
    public class PasswordProviderTests
    {
        // TODO what need to be tested?
        // - CreatePasswordHash in both methods
        // - VerifyPasswordHash
        [Test]
        public void CreatePasswordHash_WhenCalledWithSalt_ProperComputedPasswordExpected()
        {

        }

        [Test]
        public void CreatePasswordHash_WhenCalledWithoutSalt_ProperComputedPasswordExpected()
        {

        }

        //[Test]
        //public void VerifyPasswordHash_WhenPassedPasswordAnd_ProperComputedPasswordExpected()
        //{

        //}
    }
}
