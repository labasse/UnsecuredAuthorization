using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnsecuredAuthorization;
using System;

namespace UnsecuredAuthorizationTest
{
    [TestClass]
    public class AuthorizationInfoTest
    {
        private readonly Guid aGuid = Guid.Parse("6FABB6F2-4E38-49C5-B236-BF49C20098BA");

        [TestMethod]
        public void Initialization()
        {
            var test = new AuthorizationInfo(aGuid, "foo", "admin,guest");

            Assert.AreEqual(aGuid,test.Token);
            Assert.AreEqual("foo", test.Username);
            Assert.AreEqual(new AuthorizationInfo(aGuid, "foo", "admin,guest"), test);
        }

        [TestMethod]
        public void InRole()
        {
            var test = new AuthorizationInfo(aGuid, "foo", "admin,guest");

            Assert.IsTrue(test.IsInRole("admin"));
            Assert.IsTrue(test.IsInRole("guest"));
        }

        [TestMethod]
        public void NotInRole()
        {
            var test = new AuthorizationInfo(aGuid, "foo", "admin,guest");

            Assert.IsFalse(test.IsInRole("Admin"));
        }

    }
}