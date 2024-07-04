using BExIS.App.Testing;
using BExIS.Modules.Rpm.UI.Controllers;
using NUnit.Framework;

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