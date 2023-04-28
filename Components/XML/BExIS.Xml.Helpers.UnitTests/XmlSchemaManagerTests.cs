using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using BExIS.App.Testing;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Utils.Config;
using BExIS.Xml.Helpers.Mapping;
using NUnit.Framework;
using Vaiona.Utils.Cfg;

namespace BExIS.Xml.Helpers.UnitTests
{
    [TestFixture()]
    public class XmlSchemaManagerTests
    {
        private string _schemaPath;
        private string _temp;
        private string _username = "david";

        private List<long> deleteMDS = new List<long>();

        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved.
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);

            // get schame path
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string dic = Path.Combine(path, "App_Data/datacite/");
            _schemaPath = Path.Combine(path, dic, "metadata - Kopie.xsd");

            if (!File.Exists(_schemaPath))
            {
                throw new NullReferenceException("xsd not exist");
            }

            // copy all paths to temp folder
            _temp = Path.Combine(AppConfiguration.DataPath, "Temp", _username);

            string[] filePaths = Directory.GetFiles(dic);

            foreach (var filename in filePaths)
            {
                string file = filename.ToString();

                //Do your job with "file"  
                string str = Path.Combine(_temp, file.ToString().Replace(dic,""));
                if (!File.Exists(str))
                {
                    string d = Path.GetDirectoryName(str);
                    if(!Directory.Exists(d)) Directory.CreateDirectory(d);

                    File.Copy(file, str);
                }
            }

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
            DirectoryInfo di = new DirectoryInfo(_temp);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            // delete created metadata structures
            using (var metadataStructureManager = new MetadataStructureManager())
            {
                foreach (var md in metadataStructureManager.Repo.Get())
                {
                    if (deleteMDS.Contains(md.Id))
                        metadataStructureManager.Delete(md);
                }
            }

            // reset app data folder
            var targetDir = Path.GetDirectoryName(_schemaPath);
            var sourceDir = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Metadata", "test");

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var targetFilePath = Path.Combine(targetDir, Path.GetFileName(file));
                if(!File.Exists(targetFilePath))
                    File.Copy(file, targetFilePath);
            }
        }

        [OneTimeTearDown]
        /// It is called once after executing all the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved.
        /// Inheritance is supported, call sequence form the children
        /// Executes only if: counterpart OneTimeSetUp exists and executed successfully.
        public void OneTimeTearDown()
        {
        }

        [Test()]
        public void Load_Valid_loadedXsd()
        {
            //Arrange
            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();
            //Act
            xmlSchemaManager.Load(_schemaPath, _username);

            //Assert
            Assert.IsNotNull(xmlSchemaManager.Schema);
        }

        [Test()]
        public void Load_FileNotExist_NullException()
        {
            //Arrange
            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();
            //Act

            //Assert
            Assert.Throws<FileNotFoundException>(() => xmlSchemaManager.Load("C:/test/notexist.xsd", _username));

        }

        [Test()]
        public void Load_PathIsNull_NullArgumentException()
        {
            //Arrange
            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();
            //Act
            //Assert
            Assert.That(() => xmlSchemaManager.Load("", _username), Throws.ArgumentNullException);

        }

        [Test()]
        public void Load_UserIsNull_NullArgumentException()
        {
            //Arrange
            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();
            //Act
            //Assert
            Assert.That(() => xmlSchemaManager.Load(_schemaPath,""), Throws.ArgumentNullException);

        }

        [Test()]
        public void Generate_valid_MetadataStructure()
        {
            //Arrange
            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();
            xmlSchemaManager.Load(_schemaPath, _username);

            //act
            var id =  xmlSchemaManager.GenerateMetadataStructure("", "test");

            Assert.That(id > 0);

            using (var metadataStructureManager = new MetadataStructureManager())
            using (var metadataAttributeManager = new MetadataAttributeManager())
            {
                var metadataStructure = metadataStructureManager.Repo.Get(id);

                //15 package usages

            }

            deleteMDS.Add(id);

        }


    }
}