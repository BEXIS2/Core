using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.App.Testing;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using NUnit.Framework;

namespace BExIS.Security.Services.Tests.Authorization
{
    [TestFixture]
    public class EntityPermissionManagerTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
        }

        [SetUp]
        protected void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [TestCase(1, new[] { "Read" })]
        [TestCase(4, new[] { "Write" })]
        [TestCase(5, new[] { "Read", "Write" })]

        public void CreateAsync_GroupIsNull_ReturnZero(short rights, string[] result)
        {
            //Arrange
            var a = new EntityPermissionManager();

            //Act
            var r = a.GetRights(rights);

            //Assert
            Assert.That(result, Is.EquivalentTo(r));
        }
    }
}