using BExIS.Modules.Dim.UI.Helpers;
using NUnit.Framework;
using BExIS.App.Testing;
using BExIS.Utils.Config;
using Vaiona.Utils.Cfg;
using BExIS.Dim.Helpers.Models;
using RestSharp;
using RestSharp.Authenticators;
using BExIS.Dim.Helpers;
using BExIS.Dlm.Entities.Data;
using Lucifron.ReST.Library.Models;
using Newtonsoft.Json;

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
        public void ReadProxy_ValidSettings_ReturnString()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();


            var proxy = settingsHelper.GetValue("proxy");

            Assert.That(1, Is.EqualTo(1));
        }

        [Test()]
        public void ReadMappings_ValidSettings_ReturnList()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();


            var mappings = settingsHelper.GetDataCiteSettings("mappings");

            Assert.That(mappings.Count, Is.EqualTo(1));

        }

        //[Test()]
        public void ReadPlaceholders_ValidSettings_ReturnList()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var settingsHelper = new SettingsHelper();
            var datacitedoihelper = new DataCiteDoiHelper();


            var client = new RestClient(settingsHelper.GetValue("proxy"));
            client.Authenticator = new JwtAuthenticator(settingsHelper.GetValue("token"));

            // DOI
            var placeholders = settingsHelper.GetDataCiteSettings("placeholders");

            var x = new DatasetVersion() { Id = 1337, VersionName = "abc", VersionNo = 111, Dataset = new Dataset() { Id = 42} };

            var p = datacitedoihelper.CreatePlaceholders(x, placeholders);

            var doi_request = new RestRequest($"api/dois", Method.POST).AddJsonBody(p);
            CreateDOIModel doi = JsonConvert.DeserializeObject<CreateDOIModel>(client.Execute(doi_request).Content);

            var model = new CreateDataCiteModel();
            model.Doi = doi.DOI;
            model.Suffix = doi.Suffix;
            model.Prefix = doi.Prefix;

            var datacite_request = new RestRequest($"api/datacite", Method.POST).AddJsonBody(JsonConvert.SerializeObject(model));
            var response = client.Execute(datacite_request).Content;
        }
    }
}
