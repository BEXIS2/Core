using BExIS.App.Testing;
using BExIS.Modules.Dim.UI.Helpers;
using NUnit.Framework;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dim.UI.Tests.Helpers
{
    [TestFixture()]
    public class SettingsHelperTests
    {
        private TestSetupHelper helper = null;

        //[Test()]
        public void GetVaelastraszMappings()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();

            var mappings = settingsHelper.GetVaelastraszMappings();
            Assert.NotNull(mappings);
        }

        //[Test()]
        public void GetDataCiteDOIPlaceholders()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();

            var mappings = settingsHelper.GetVaelastraszPlaceholders();
            Assert.NotNull(mappings);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            //helper = new TestSetupHelper(WebApiConfig.Register, false);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }
    }
}