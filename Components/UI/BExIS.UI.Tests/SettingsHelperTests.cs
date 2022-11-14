using NUnit.Framework;

namespace BExIS.UI.Tests
{
    public class SettingsHelperTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
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

        [Test()]
        public void LoadSettings_TestNewtonSoft_Convertion_ModuleSettingsShouldbeLoaded()
        {
            //Arrange
            string moduleId = "Dcm";

            string path = @"C:\Users\admin\source\repos\Bexis2\BEXIS2 - Core - Workspace\Core\Console\Workspace\Modules\DCM\Dcm.Settings.json";

            //Act

            //var settings = settingsHelper.LoadSettings(path);

            //Assert
            //Assert.IsNotNull(settings);
            //Assert.IsTrue(settings.Id.Equals(moduleId));
        }
    }
}