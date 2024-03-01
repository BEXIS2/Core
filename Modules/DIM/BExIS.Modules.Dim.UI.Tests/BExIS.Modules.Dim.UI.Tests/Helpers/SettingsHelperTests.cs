using BExIS.App.Testing;
using BExIS.Dim.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Modules.Dim.UI.Helpers;
using BExIS.Utils.Config;
using Lucifron.ReST.Library.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
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

            var mappings = settingsHelper.GetDataCiteCredentials();
            Assert.NotNull(mappings);
        }

        //[Test()]
        public void GetDataCiteDOIMappingss()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();

            var mappings = settingsHelper.GetDataCiteMappings();
            Assert.NotNull(mappings);
        }

        //[Test()]
        public void GetDataCiteDOIPlaceholders()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();

            var mappings = settingsHelper.GetDataCitePlaceholders();
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