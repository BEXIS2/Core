using BExIS.App.Testing;
using BExIS.Modules.Dim.UI.Helpers;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dim.UI.Tests.Publications
{
    [TestFixture]
    public class VaelastraszTests
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
        public void GetVaelastraszMappings()
        {
            var settingsHelper = new SettingsHelper();

            var mappings = settingsHelper.GetVaelastraszMappings();
            Assert.NotNull(mappings);
        }

        [Test()]
        public void GetDataCiteDOIPlaceholders()
        {
            var settingsHelper = new SettingsHelper();

            var mappings = settingsHelper.GetVaelastraszPlaceholders();
            Assert.NotNull(mappings);
        }

        [Test()]
        public void GetDataCiteModel()
        {

        }

        [Test()]
        public void GetVaelastraszResponse()
        {

        }
    }
}
