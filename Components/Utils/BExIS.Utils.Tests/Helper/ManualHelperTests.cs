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
        public void GetUrl_IsEmpty_FallBackString()
        {
            //Arrange
            //Act
            string url = ManualHelper.GetUrl(string.Empty);
            //Assert

            Assert.AreEqual(url,"/home/docs/general");
        }

        [Test]
        public void GetUrl_NoUrl_DocumentationLink()
        {
            //Arrange
            //Act
            string url = ManualHelper.GetUrl("search");
            //Assert

            Assert.AreEqual(url, "/home/docs/search");
        }


        [Test]
        public void GetUrl_Valid_GetUrl()
        {
            //Arrange
            string expected = "http://www.google.de";

            //Act
            string url = ManualHelper.GetUrl(expected);
            //Assert

            Assert.AreEqual(expected, url);
        }
    }
}