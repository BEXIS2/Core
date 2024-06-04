using BExIS.App.Testing;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Utils.Config;
using NUnit.Framework;

namespace BExIS.Dlm.Tests.Services.DataStructure
{
    public class DataStructureManagerTests
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
            helper.Dispose();
        }

        [Test()]
        public void Create_valid_newStructure()
        {
            using (var dataStructureManager = new DataStructureManager())
            {
                //Arrange
                //Act
                StructuredDataStructure dataStructure = dataStructureManager.CreateStructuredDataStructure("dsForTesting", "DS for unit testing", "", "", Dlm.Entities.DataStructure.DataStructureCategory.Generic);

                //Assert
                Assert.IsNotNull(dataStructure);
            }
        }
    }
}