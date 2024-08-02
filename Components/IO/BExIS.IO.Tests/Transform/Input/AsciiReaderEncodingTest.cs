using BExIS.IO.Transform.Input;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vaiona.Utils.Cfg;

namespace BExIS.IO.Tests.Transform.Input
{
    public class AsciiReaderEncodingTest
    {
        private string filepath;
        private string filepathUTF8;
        private string filepathUnits;
        private Encoding windows;
        private List<string> lines;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // for data generation
            int numberOfRows = 100;
            int charLength = 10;
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ÄÜÖ";

            filepath = Path.Combine(AppConfiguration.DataPath, "EncodingWindowsFormat_testfile.txt");
            filepathUTF8 = Path.Combine(AppConfiguration.DataPath, "EncodingUTF8Format_testfile.txt");

            lines = new List<string>();
            string line = "";

            for (int i = 0; i < numberOfRows; i++)
            {
                line = new string(Enumerable.Repeat(chars, charLength).Select(s => s[random.Next(s.Length)]).ToArray());
                lines.Add(line);
            }

            // 1252 is the encoding number of the windows format
            windows = Encoding.GetEncoding(1252);

            // write all text line text file to string array
            File.WriteAllLines(filepath, lines.ToArray(), windows);

            File.WriteAllLines(filepathUTF8, lines.ToArray(), Encoding.UTF8);
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
            if (File.Exists(filepath)) File.Delete(filepath);
            if (File.Exists(filepathUTF8)) File.Delete(filepathUTF8);
        }

        [Test]
        public void encoding_detectEcodingByReadFileFirst_EncodingIsWindowsFormat()
        {
            var encoding = Encoding.Default;
            var line = "";
            var incoming = new List<string>();

            using (var reader = new StreamReader(filepath, Encoding.Default, true))
            {
                if (reader.Peek() >= 0) // you need this!
                    reader.Read();

                encoding = reader.CurrentEncoding;

                Assert.IsTrue(encoding.Equals(windows));
                reader.Close();
            }

            using (var reader = new StreamReader(filepath, encoding, true))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    incoming.Add(line);
                }
            }

            Assert.That(incoming, Is.EquivalentTo(lines));
        }

        [Test]
        public void encoding_detectEcodingByEveryRow_EncodingIsWindowsFormat()
        {
            var encoding = Encoding.Default;
            var incoming = new List<string>();
            var line = "";

            using (var reader = new StreamReader(filepath, Encoding.Default, true))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    encoding = reader.CurrentEncoding;
                    byte[] encBytes = encoding.GetBytes(line);
                    byte[] utf8Bytes = Encoding.Convert(encoding, Encoding.UTF8, encBytes);
                    incoming.Add(Encoding.UTF8.GetString(utf8Bytes));
                }

                if (reader.Peek() >= 0) // you need this!
                    reader.Read();

                encoding = reader.CurrentEncoding;

                Assert.IsTrue(encoding.Equals(windows));

                reader.Close();
            }

            Assert.That(incoming, Is.EquivalentTo(lines));
        }

        [Test]
        public void encoding_detectEcodingByReadFileFirst_EncodingIsUTF8Format()
        {
            var encoding = Encoding.Default;
            var line = "";
            var incoming = new List<string>();

            using (var reader = new StreamReader(filepathUTF8, Encoding.Default, true))
            {
                if (reader.Peek() >= 0) // you need this!
                    reader.Read();

                encoding = reader.CurrentEncoding;

                Assert.IsTrue(encoding.Equals(Encoding.UTF8));
                reader.Close();
            }

            using (var reader = new StreamReader(filepathUTF8, encoding, true))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    incoming.Add(line);
                }
            }

            Assert.That(incoming, Is.EquivalentTo(lines));
        }

        [Test]
        public void GetEncoding_ByEndocingType_EncodingIsWindowsFormat()
        {
            // Arrange
            EncodingType et = EncodingType.Windows;

            // Act
            Encoding encoding = AsciiFileReaderInfo.GetEncoding(et);

            Encoding source = Encoding.GetEncoding(1252);
            //Assert

            Assert.AreEqual(encoding, source);
        }

        [Test]
        public void GetEncoding_ByEndocingType_EncodingIsUTF8Format()
        {
            // Arrange
            EncodingType et = EncodingType.UTF8;

            // Act
            Encoding encoding = AsciiFileReaderInfo.GetEncoding(et);

            Encoding source = Encoding.UTF8;
            //Assert

            Assert.AreEqual(encoding, source);
        }
    }
}