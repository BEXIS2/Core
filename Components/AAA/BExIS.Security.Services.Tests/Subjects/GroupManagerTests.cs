using BExIS.Security.Services.Subjects;
using NUnit.Framework;

namespace BExIS.Security.Services.Tests.Subjects
{
    [TestFixture]
    public class GroupManagerTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
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

        public void CreateAsync_GroupIsNull_ReturnZero()
        {
            //Arrange
            using (var a = new GroupManager())
            {
                //Act
                var result = a.CreateAsync(null);

                //Assert
                Assert.That(result, Is.EqualTo(0));
            }
        }
    }
}