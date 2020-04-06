using BExIS.App.Testing;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Tests.Helpers;
using BExIS.IO.Transform.Input;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Utils.Cfg;

namespace BExIS.IO.Tests.Transform.Input
{
    public class AsciiReaderEncodingTest
    {

        private string filepath;
        private Encoding windows;
        List<string> lines;

        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved.
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
            // for data generation
            int numberOfRows = 10;
            int charLength = 10;
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ÄÜÖ";

           
            filepath = Path.Combine(AppConfiguration.DataPath , "EncodingWindowsFormat_testfile.txt");


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

        }

        [SetUp]
        /// performs the initial setup for the tests. This runs once per test, NOT per class!
        protected void SetUp()
        {

        }

        [TearDown]
        /// performs the cleanup after each test
        public void TearDown()
        {
            
        }

        [OneTimeTearDown]
        /// It is called once after executing all the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved.
        /// Inheritance is supported, call sequence form the children
        /// Executes only if: counterpart OneTimeSetUp exists and executed successfully.
        public void OneTimeTearDown()
        {
            if (File.Exists(filepath)) File.Delete(filepath);
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



            //Assert.That(incoming, Is.EquivalentTo(lines));

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

  
            //Assert.That(incoming, Is.EquivalentTo(lines));

        }

    }
}