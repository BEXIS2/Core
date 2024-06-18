using BExIS.IO.Transform.Input;
using NUnit.Framework;
using System;

namespace BExIS.IO.Tests.Transform.Input
{
    [TestFixture()]
    public class ExcelReaderTest
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

        // 11 zeichen max lenght
        [TestCase("278.89999999999998", "278,9", "")] // roundungsfehler in Excel
        [TestCase("278,89999999999998", "278,9", "")] // roundungsfehler in Excel
        [TestCase("123,456789", "123,456789", "")] // wird nicht gerundet
        [TestCase("123,451789", "123,451789", "")] // wird nicht gerundet
        [TestCase("123456789,012345", "123456789", "")]
        [TestCase("12345678,9012345", "12345678,9", "")] //c# 12345678,9012345
        [TestCase("12345678,9512345", "12345678,95", "")]
        [TestCase("12345678,9552345", "12345678,96", "")] // warum rundet der hier? und hier nicht 123,451789
        [TestCase("278,89999999999998", "278,9", "0.0")] //definierter Roundungsbereich
        [TestCase("278,89999999999998", "278,90", "#,##0.00")] //definierter Roundungsbereich
        [TestCase("123,456789", "123,46", "#,##0.00")] //definierter Roundungsbereich
        [TestCase("4.9000001", "4.9", "0.0")] //definierter Roundungsbereich
        public void InputConvertionToDouble_ValueInToDataType_ReturnSameAsImported(string cellValue, string displayValue, string formatCode)
        {
            //Arrange
            double output = 0;
            string outputDisplay = "";
            string outputDisplayWithFormatCode = "";

            //Act
            output = ExcelHelper.Convert(cellValue);
            outputDisplay = ExcelHelper.ConvertDisplay(displayValue); // convert without format, for comparision
            outputDisplayWithFormatCode = ExcelHelper.ConvertWithFormat(cellValue, formatCode); // convert with format

            //Assert
            Assert.That(output, Is.Not.EqualTo(0));
            //Assert.That(outputDisplay, Is.EqualTo(displayValue));
            Assert.That(outputDisplayWithFormatCode, Is.EqualTo(displayValue));
        }

        [TestCase("1234567890,43759", "1,23E+09", "0.00E+00")]
        [TestCase("0,00000000123", "1,23E-09", "0.00E+00")]
        [TestCase("1,81655819999999E-20", "1,82E-20", "")]
        public void InputConvertionToDoubleScientificFormat_ValueInToDataType_ReturnSameAsImported(string cellValue, string displayValue, string formatCode)
        {
            //Arrange
            double output = 0;
            string outputDisplay = "";
            string outputDisplayWithFormatCode = "";

            try
            {
                //Act
                output = ExcelHelper.Convert(cellValue);

                //1,23E+09 -> 1230000000
                //1,23E-09 -> 1,23E-09
                outputDisplay = ExcelHelper.ConvertDisplay(displayValue);

                outputDisplayWithFormatCode = ExcelHelper.ConvertWithFormat(cellValue, formatCode);

                //Assert
                Assert.That(output, Is.Not.EqualTo(0));
                //Assert.That(outputDisplay, Is.EqualTo(expectedValue));
                Assert.That(outputDisplayWithFormatCode, Is.EqualTo(displayValue));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Test]
        public void ConvertExcelToCsv()
        {
            // Arrange

            // Act

            // Assert
        }
    }
}