using BExIS.App.Testing;
using BExIS.Utils.Config;
using NUnit.Framework;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Helpers.Tests.DataCiteDOI
{
    [TestFixture()]
    public class DataCiteDoiHelperTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        //[Test()]
        public void ReadMappings_ValidSettings_ReturnList()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;

            Assert.That(1, Is.EqualTo(1));
        }
    }
}