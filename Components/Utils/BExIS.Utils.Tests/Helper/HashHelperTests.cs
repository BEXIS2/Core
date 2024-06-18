using BExIS.Utils.Helpers;
using NUnit.Framework;

namespace BExIS.Utils.Tests
{
    [TestFixture()]
    public class HashHelperTests
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
        public void CreateMD5Hash_validMulitInputs_hashedString()
        {
            //Arrange
            string file = "filename.txt";
            string size = "1234.45kb";
            string date = "2022-12-12";

            string output = "bf9fbd11199cfb7e79d5ed3538cb967a";

            //Act
            string result = HashHelper.CreateMD5Hash(file, size, date);

            //Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.ToLower(), output);
        }

        [Test]
        public void CreateMD5Hash_validInput_hashedString()
        {
            //Arrange
            string input = "Dies ist ein Test Text und wird in einen Hashwert umgewandelt.";
            string output = "c370da64d7b130c4aa776431f371f835";
            //Act
            string result = HashHelper.CreateMD5Hash(input);
            //Assert

            Assert.NotNull(result);
            Assert.AreEqual(result.ToLower(), output);
        }

        [Test]
        public void CreateMD5Hash_EmptyInput_EmptyString()
        {
            //Arrange
            string input = "";
            string output = "";
            //Act
            string result = HashHelper.CreateMD5Hash(input);
            //Assert

            Assert.NotNull(result);
            Assert.AreEqual(result.ToLower(), output);
        }

        [Test]
        public void CreateMD5Hash_InputIsNull_EmptyString()
        {
            //Arrange
            string input = "";
            string output = "";
            //Act
            string result = HashHelper.CreateMD5Hash(null);
            //Assert

            Assert.NotNull(result);
            Assert.AreEqual(result.ToLower(), output);
        }
    }
}