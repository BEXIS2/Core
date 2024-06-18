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
        public void GetDataCiteDOICredentials()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();

            var mappings = settingsHelper.GetDataCiteDOICredentials();
            Assert.NotNull(mappings);
        }

        //[Test()]
        public void GetDataCiteDOIMappingss()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();

            var mappings = settingsHelper.GetDataCiteDOIMappings();
            Assert.NotNull(mappings);
        }

        //[Test()]
        public void GetDataCiteDOIPlaceholders()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();

            var mappings = settingsHelper.GetDataCiteDOIPlaceholders();
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