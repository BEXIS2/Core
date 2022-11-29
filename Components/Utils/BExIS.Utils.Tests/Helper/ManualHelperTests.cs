
using BExIS.Utils.Helpers;
using NUnit.Framework;


namespace BExIS.Utils.Tests
{
    [TestFixture()]
    public class ManualHelperTests
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
        public void GetUrl_VersionIsEmpty_EmptyString()
        {
            //Arrange
            //Act
            string url = ManualHelper.GetUrl(string.Empty, "");
           //Assert

           Assert.That(url,Is.Empty);

        }

        [Test]
        public void GetUrl_ModuleIsEmpty_EmptyString()
        {
            //Arrange
            //Act
            string url = ManualHelper.GetUrl("2.15", "");
            //Assert

            Assert.That(url, Is.Empty);

        }

        //https://github.com/BEXIS2/Documents/blob/2.15/Manuals/BAM/Manual.md
        [Test]
        public void GetUrl_Valid_GetUrl()
        {
            //Arrange
            string expected = "https://github.com/BEXIS2/Documents/blob/2.15/Manuals/BAM/Manual.md";

            //Act
            string url = ManualHelper.GetUrl("2.15", "BAM");
            //Assert

            Assert.AreEqual(expected, url);

        }
    }
}