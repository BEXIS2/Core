using BExIS.Utils.Files;
using BExIS.Utils.Helpers;
using NUnit.Framework;

namespace BExIS.Utils.Tests
{
    [TestFixture()]
    public class FileNameUtilityTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [Test]
        public void SanitizeFileName_invalidCharacters_ReturnValid()
        {
            //Arrange
            string input = "file<name>with:forbidden\"chars/\\|?*.txt";
            string expected = "file-name-with-forbidden-chars-.txt";

            //Act
            string result = FileNameUtility.SanitizeFileName(input);

            //Assert
            Assert.NotNull(result);
            Assert.AreEqual(expected, result);
        }

        
    }
}