using BExIS.Modules.Dim.UI.Helpers;
using NUnit.Framework;
using BExIS.App.Testing;
using BExIS.Utils.Config;
using Vaiona.Utils.Cfg;
using BExIS.Dim.Helpers.Models;
using System.Linq;

namespace BExIS.Modules.Dim.UI.Tests.Helpers
{
    [TestFixture()]
    public class SettingsHelperTests
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

        [Test()]
        public void ReadMappings_ValidSettings_ReturnList()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();


            var mappings = settingsHelper.GetDataCiteSettings("mappings");

            Assert.That(mappings.Count, Is.EqualTo(1));

        }

        [Test()]
        public void ReadPlaceholders_ValidSettings_ReturnList()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();


            var placeholders = settingsHelper.GetDataCiteSettings("placeholders");

            Assert.That(placeholders.Count, Is.EqualTo(1));
        }
    }
}
