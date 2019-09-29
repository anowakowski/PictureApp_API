using System;
using System.Collections.Generic;
using System.Text;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Idioms;
using NUnit.Framework;

namespace PictureApp.API.Tests
{
    [TestFixture]
    public abstract class GuardClauseAssertionTests<T>
    {
        [Test]
        public void AllConstructorsMustBeGuardClaused()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var assertion = new GuardClauseAssertion(fixture);

            assertion.Verify(typeof(T).GetConstructors());
        }
    }
}