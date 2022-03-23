using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq;
using UnsecuredAuthorization;
using System.Collections.Generic;

namespace UnsecuredAuthorizationTest
{
    [TestClass]
    public class UnsecuredAuthorizationServiceImplTest
    {
        private readonly TimeSpan _1h_ = TimeSpan.FromHours(1);
        private readonly TimeSpan _15min_ = TimeSpan.FromMinutes(15);

        private readonly DateTime _12h00_ = new DateTime(2022, 4, 1, 12,  0, 0);
        private readonly DateTime _12h30_ = new DateTime(2022, 4, 1, 12, 30, 0);
        private readonly DateTime _13h00_ = new DateTime(2022, 4, 1, 13,  0, 0);
        private readonly DateTime _13h20_ = new DateTime(2022, 4, 1, 13, 20, 0);
        private readonly DateTime _14h00_ = new DateTime(2022, 4, 1, 14,  0, 0);

        private readonly Guid guid1 = Guid.Parse("1FABB6F2-4E38-49C5-B236-BF49C20098BA");
        private readonly Guid guid2 = Guid.Parse("2FABB6F2-4E38-49C5-B236-BF49C20098BA");

        private AuthorizationInfo Guid1DaveAdminGuest => new AuthorizationInfo(guid1, "Dave", "Admin,Guest");
        private AuthorizationInfo Guid2HalGuest => new AuthorizationInfo(guid2, "Hal", "Guest");

        public UnsecuredAuthorizationServiceImpl New1hAuthService(params DateTime[] sequence)
        {
            var stubClock = new Mock<IClock>();
            var stubGuid = new Mock<IGuidGen>();

            switch(sequence.Length)
            {
                case 0: stubClock.Setup(c => c.Now).Returns(_12h00_); break;
                case 1: stubClock.Setup(c => c.Now).Returns(sequence[0]); break;
                default:
                {
                    var ss = stubClock.SetupSequence(c => c.Now);

                    foreach (var dt in sequence)
                    {
                        ss.Returns(dt);
                    }
                    break;
                }
            }
            stubGuid.SetupSequence(g => g.NewGuid()).Returns(guid1).Returns(guid2);

            return new UnsecuredAuthorizationServiceImpl()
            {
                TokenLifetime = _1h_,
                Clock = stubClock.Object,
                GuidGen = stubGuid.Object
            };
        }

        #region Initialize
        [TestMethod]
        public void InitializationDefaultLifeTime()
        {
            UnsecuredAuthorizationServiceImpl test = new ();

            Assert.AreEqual(_15min_, test.TokenLifetime);
            Assert.IsFalse(test.Exists(guid1));
            Assert.ThrowsException<KeyNotFoundException>(() => test[guid1]);
        }

        [TestMethod]
        public void InitializationSpecifiedLifeTime()
        {
            var test = New1hAuthService();

            Assert.AreEqual(_1h_, test.TokenLifetime);
            Assert.IsFalse(test.Exists(guid1));
        }
        #endregion

        #region SignIn
        [TestMethod]
        public void SignInNewUser()
        {
            var test = New1hAuthService(_12h00_);
            var guid = test.SignIn("Dave", new[] { "Guest", "Admin" });
            
            Assert.AreEqual(guid1, guid);
            Assert.AreEqual(Guid1DaveAdminGuest, test[guid]);
            Assert.IsTrue(test.Exists(guid1));
        }
        [TestMethod]
        public void SignIn2Users()
        {
            var test = New1hAuthService(_12h00_);
            test.SignIn("Dave", new[] { "Guest", "Admin" });
            var guid = test.SignIn("Hal", new[] { "Guest" });
            
            Assert.AreEqual(guid2, guid);
            Assert.AreEqual(Guid2HalGuest, test[guid]);
            Assert.IsTrue(test.Exists(guid1));
            Assert.IsTrue(test.Exists(guid2));
        }

        [TestMethod]
        public void SignInExistingUser()
        {
            var test = New1hAuthService(_12h00_, _12h30_);
            
            test.SignIn("Dave", new[] { "Guest","Admin" });
            var guid = test.SignIn("Dave", new[] { "Admin" });

            Assert.AreEqual(guid2, guid);
            Assert.AreEqual(Guid1DaveAdminGuest, test[guid1]);
            Assert.IsTrue(test.Exists(guid1));
        }

        [TestMethod]
        public void SignInEmptyUsername()
        {
            var test = New1hAuthService(_12h00_);

            Assert.ThrowsException<ArgumentException>(() => test.SignIn("", new[] { "Admin" }));
        }

        [TestMethod]
        public void SignInEmptyRoles()
        {
            var test = New1hAuthService(_12h00_);

            Assert.ThrowsException<ArgumentException>(() => test.SignIn("Dave", Array.Empty<string>()));
        }
        #endregion

        #region SignOut
        [TestMethod]
        public void SignOutWithExpiredToken()
        {
            var test = New1hAuthService(_12h00_, _14h00_);

            test.SignIn("Dave", new[] { "Admin" });
            Assert.ThrowsException<KeyNotFoundException>(()=>test.SignOut(guid1));
        }
        [TestMethod]
        public void SignOutWithUnknownToken()
        {
            var test = New1hAuthService(_12h00_);

            Assert.ThrowsException<KeyNotFoundException>(() => test.SignOut(guid1));
        }
        [TestMethod]
        public void SignOutWithValidToken()
        {
            var test = New1hAuthService(_12h00_, _12h30_);

            test.SignIn("Dave", new[] { "Admin" });
            test.SignOut(guid1);

            Assert.IsFalse(test.Exists(guid1));
        }
        #endregion

        #region Lifetime management
        [TestMethod]
        public void TokenBeforeLifetimeDelay()
        {
            var test = New1hAuthService(_12h00_, _12h30_);

            test.SignIn("Dave", new[] { "Admin" });
            Assert.IsTrue(test.Exists(guid1));
        }

        [TestMethod]
        public void ReadTokenExtendsLifetime()
        {
            var test = New1hAuthService(_12h00_, _12h30_, _13h20_);

            test.SignIn("Dave", new[] { "Admin" });
            var infos = test[guid1];

            Assert.IsTrue(test.Exists(guid1));
        }

        [TestMethod]
        public void TokenOnLifetimeDelay()
        {
            var test = New1hAuthService(_12h00_, _13h00_);

            test.SignIn("Dave", new[] { "Admin" });

            Assert.IsTrue(test.Exists(guid1));
        }

        [TestMethod]
        public void TokenAfterLifetimeDelay()
        {
            var test = New1hAuthService(_12h00_, _13h20_);

            test.SignIn("Dave", new[] { "Admin" });

            Assert.IsFalse(test.Exists(guid1));
        }
        #endregion
    }
}
