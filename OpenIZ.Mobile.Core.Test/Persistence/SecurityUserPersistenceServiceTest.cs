﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenIZ.Core.Model.Security;
using OpenIZ.Mobile.Core.Services;
using OpenIZ.Mobile.Core.Serices;

namespace OpenIZ.Mobile.Core.Test.Persistence
{
    /// <summary>
    /// Summary description for SecurityUserPersistenceServiceTest
    /// </summary>
    [TestClass]
    public class SecurityUserPersistenceServiceTest : PersistenceTest<SecurityUser>
    {

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            (ApplicationContext.Current as TestApplicationContext).UnitTestContext = context;
        }

        /// <summary>
        /// Test the insertion of a valid security user
        /// </summary>
        [TestMethod]
        public void TestInsertValidSecurityUser()
        {

            SecurityUser userUnderTest = new SecurityUser()
            {
                Email = "admin@test.com",
                EmailConfirmed = true,
                PasswordHash = "test_user_hash_store",
                SecurityHash = "test_security_hash",
                UserName = "admin"
            };

            var userAfterTest = base.DoTestInsert(userUnderTest);
            Assert.AreEqual(userUnderTest.UserName, userAfterTest.UserName);
        }

        /// <summary>
        /// Test the updating of a valid security user
        /// </summary>
        [TestMethod]
        public void TestUpdateValidSecurityUser()
        {

            IPasswordHashingService hashingService = ApplicationContext.Current.GetService<IPasswordHashingService>();

            SecurityUser userUnderTest = new SecurityUser()
            {
                Email = "update@test.com",
                EmailConfirmed = false,
                PasswordHash = hashingService.ComputeHash("password"),
                SecurityHash = "cert",
                UserName = "updateTest"

            };

            // Store user
            IIdentityProviderService identityService = ApplicationContext.Current.GetService<IIdentityProviderService>();
            var userAfterUpdate = base.DoTestUpdate(userUnderTest, "PhoneNumber");

            // Update
            Assert.IsNotNull(userAfterUpdate.UpdatedTime);
            Assert.IsNotNull(userAfterUpdate.PhoneNumber);
        }

        /// <summary>
        /// Test valid query result
        /// </summary>
        [TestMethod]
        public void TestQueryValidResult()
        {

            IPasswordHashingService hashingService = ApplicationContext.Current.GetService<IPasswordHashingService>();
            String securityHash = Guid.NewGuid().ToString();
            SecurityUser userUnderTest = new SecurityUser()
            {
                Email = "query@test.com",
                EmailConfirmed = false,
                PasswordHash = hashingService.ComputeHash("password"),
                SecurityHash = securityHash,
                UserName = "queryTest"

            };

            var testUser = base.DoTestInsert(userUnderTest);
            IIdentityProviderService identityService = ApplicationContext.Current.GetService<IIdentityProviderService>();
            var results = base.DoTestQuery(o => o.Email == "query@test.com", testUser.Key);
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(userUnderTest.Email, results.First().Email);
        }



    }
}