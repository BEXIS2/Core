/*
using BExIS.Dlm.Entities.Meanings;
using Moq;
using NUnit.Framework;

namespace BExIS.Dlm.Tests.Meanings
{
    [TestFixture]
    public class MeaningTests
    {
        private MockRepository mockRepository;



        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private Meaning CreateMeaning()
        {
            return new Meaning();
        }

        [Test]
        public void Dispose_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var meaning = this.CreateMeaning();

            // Act
            meaning.Dispose();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }
    }
}
*/