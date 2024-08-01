using BExIS.App.Testing;
using BExIS.Security.Services.Authorization;
using BExIS.Utils.Config;
using NUnit.Framework;
using System.Threading.Tasks;

namespace BExIS.Security.Services.Tests.Authorization
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
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
        public async Task CreateAsync_GroupIsNull_ReturnZero(short rights, string[] result)
        {
            //Arrange
            var a = new EntityPermissionManager();

            //Act
            var r = await a.GetRightsAsync(rights);

            //Assert
            Assert.That(result, Is.EquivalentTo(r));
        }
    }
}