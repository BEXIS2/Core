using BExIS.Security.Services.Subjects;
using NUnit.Framework;

namespace BExIS.Security.Services.Tests.Subjects
{
    [TestFixture]
    public class GroupManagerTests
    {
        private readonly GroupManager _groupManager;



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
            //Act
            var result = _groupManager.CreateAsync(null);

            //Assert
            Assert.That(result, Is.EqualTo(0));
        }
    }
}