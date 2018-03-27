using NUnit.Framework;
using System.IO;
using BExIS.IO;
using Vaiona.Utils.Cfg;
using FluentAssertions;


namespace BExIS.IO.Tests
{
    [TestFixture()]
    public class FileHelperTests
    {
        private string directory { get; set; }
        private string filePath { get; set; }
        private string errorFilePath { get; set; }

        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
            directory = Path.Combine(AppConfiguration.DataPath, "TestDirectory");
            filePath = Path.Combine(directory, "test.txt");
            errorFilePath = Path.Combine(directory, "errortest.txt");

        }

        [SetUp]
        /// performs the initial setup for the tests. This runs once per test, NOT per class!
        protected void SetUp()
        {
        }

        [TearDown]
        /// performs the cleanup after each test
        public void TearDown()
        {
        }

        [OneTimeTearDown]
        /// It is called once after executing all the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the children
        /// Executes only if: counterpart OneTimeSetUp exists and executed successfully.
        public void OneTimeTearDown()
        {
            File.Delete(filePath);
        }

        [Test()]
        public void CreateFileTest()
        {

            FileStream file = FileHelper.Create(filePath);
            file.Should().NotBeNull("File can´t create or already exist.");

            file.Close();
        }
    }
}
