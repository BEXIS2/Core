using Moq;
using NUnit.Framework;

namespace BExIS.Xml.Helpers.UnitTests
{
    [TestFixture]
    public class XsdSchemaReaderTests
    {
        private MockRepository mockRepository;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [TearDown]
        public void TearDown()
        {
            this.mockRepository.VerifyAll();
        }

        private XsdSchemaReader CreateXsdSchemaReader()
        {
            return new XsdSchemaReader();
        }

        public void Read_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var xsdSchemaReader = this.CreateXsdSchemaReader();

            // Act
            xsdSchemaReader.Read();

            // Assert
            Assert.Fail();
        }
    }
}