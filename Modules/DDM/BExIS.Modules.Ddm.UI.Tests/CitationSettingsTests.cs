using BExIS.App.Testing;
using BExIS.Modules.Ddm.UI.Helpers;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Configuration;
using Vaiona.Utils.Cfg;


namespace BExIS.Modules.Ddm.UI.Tests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
    [TestFixture]
    public class CitationSettingsTests
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

        [Test]
        public void Test()
        {
            var appConfiguration = AppConfiguration.WorkspaceRootPath;
            SettingsHelper settingsHelper = new SettingsHelper();

            var citationSettings = settingsHelper.GetCitationSettings();
        }
    }
}
