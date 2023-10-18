using BExIS.App.Testing;
using BExIS.Modules.Rpm.UI.Controllers.API;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Modules.Rpm.UI.Test.Controllers.API
{
    [TestFixture()]
    public class ConstraintsControllerTests
    {
        private TestSetupHelper helper = null;

        [Test()]
        public void GetDataCiteDOICredentials()
        {
            //var appConfiguration = AppConfiguration.WorkspaceRootPath;
            var constraintsController = new ConstraintsController();
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