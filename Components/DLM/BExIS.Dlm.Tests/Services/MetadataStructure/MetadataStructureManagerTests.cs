using BExIS.App.Testing;
using NUnit.Framework;

namespace BExIS.Dlm.Tests.Services.Data
{
    public class MetadataStructureManagerTests
    {
        private TestDBSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestDBSetupHelper();
            helper.Start();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper.Dispose();
        }

        //[Test()]
        public void Get_valid_returnMetadataStructure()
        {
            //test
            Assert.That(true);
        }
    }
}