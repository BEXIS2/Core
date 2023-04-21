/*
using BExIS.Dlm.Entities.Meanings;
using Moq;
using NUnit.Framework;

namespace BExIS.Dlm.Tests.Meanings
{
    [TestFixture]
    public class meaningManagrTests
    {
        private MockRepository mockRepository;



        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private meaningManagr CreatemeaningManagr()
        {
            using (meaningManagr mm = new meaningManagr())
            {
                return mm;
            }
                
        }

        [Test]
        public void addExternalLinks_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var meaningManagr = this.CreatemeaningManagr();
            ExternalLink externalLink = null;

            // Act
            var result = meaningManagr.addExternalLinks(
                externalLink);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void addMeaning_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var meaningManagr = this.CreatemeaningManagr();
            BExIS.Dlm.Entities.Meanings.Meaning meaning = null;

            // Act
            var result = meaningManagr.addMeaning(
                meaning);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void deleteExternalLinks_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var meaningManagr = this.CreatemeaningManagr();
            ExternalLink externalLink = null;

            // Act
            var result = meaningManagr.deleteExternalLinks(
                externalLink);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void deleteMeaning_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var meaningManagr = this.CreatemeaningManagr();
            BExIS.Dlm.Entities.Meanings.Meaning meaning = null;

            // Act
            var result = meaningManagr.deleteMeaning(
                meaning);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void editExternalLinks_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var meaningManagr = this.CreatemeaningManagr();
            ExternalLink externalLink = null;

            // Act
            var result = meaningManagr.editExternalLinks(
                externalLink);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void editMeaning_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var meaningManagr = this.CreatemeaningManagr();
            BExIS.Dlm.Entities.Meanings.Meaning meaning = null;

            // Act
            var result = meaningManagr.editMeaning(
                meaning);

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void getExternalLinks_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var meaningManagr = this.CreatemeaningManagr();

            // Act
            var result = meaningManagr.getExternalLinks();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

        [Test]
        public void getMeaning_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            meaningManagr meaningManagr = this.CreatemeaningManagr();

            // Act
            var result = meaningManagr.getMeaning();

            // Assert
            Assert.Fail();
            this.mockRepository.VerifyAll();
        }

    }
}
*/