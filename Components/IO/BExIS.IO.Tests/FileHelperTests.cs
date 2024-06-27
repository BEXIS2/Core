using FluentAssertions;
using NUnit.Framework;
using System.IO;
using Vaiona.Utils.Cfg;

namespace BExIS.IO.Tests
{
    public class FileHelperTests
    {
        private string directory { get; set; }
        private string destinationDirectory { get; set; }
        private string filePath { get; set; }
        private string fileDestinationPath { get; set; }
        private string errorFilePath { get; set; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
            directory = Path.Combine(AppConfiguration.DataPath, "TestDirectory");
            destinationDirectory = Path.Combine(AppConfiguration.DataPath, "TestDestinationDirectory");
            filePath = Path.Combine(directory, "test.txt");
            fileDestinationPath = Path.Combine(destinationDirectory, "test.txt");
            errorFilePath = Path.Combine(directory, "errortest.txt");
        }

        [SetUp]
        protected void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(filePath)) File.Delete(filePath);

            if (File.Exists(fileDestinationPath)) File.Delete(fileDestinationPath);

            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [Test()]
        public void CreateFileTest()
        {
            FileStream file = FileHelper.Create(filePath);
            file.Should().NotBeNull("File can´t create or already exist.");
            file.Close();
        }

        [Test()]
        public void ExistFileTest()
        {
            FileStream file = FileHelper.Create(filePath);
            file.Should().NotBeNull("File can´t create or already exist.");
            file.Close();

            string pathOfNotExistingFile = @"x\y\z\test.txt";

            bool fileExist = FileHelper.FileExist(filePath);
            fileExist.Should().BeTrue("because file was created before.");

            fileExist = FileHelper.FileExist(pathOfNotExistingFile);
            fileExist.Should().BeFalse("because file was not created before.");
        }

        [Test()]
        public void DeleteFileTest()
        {
            Directory.CreateDirectory(directory);
            File.Create(filePath).Close();

            bool deleted = FileHelper.Delete(filePath);

            deleted.Should().BeTrue("because file is not deleted");
        }

        [Test()]
        public void CreateDicrectoriesIfNotExistTest()
        {
            FileHelper.CreateDicrectoriesIfNotExist(directory);

            bool exist = Directory.Exists(directory);

            exist.Should().BeTrue("because directory is not created");
        }

        [Test()]
        public void MoveFileTest()
        {
            Directory.CreateDirectory(directory);
            Directory.CreateDirectory(destinationDirectory);
            File.Create(filePath).Close();

            bool moved = FileHelper.MoveFile(filePath, fileDestinationPath);

            moved.Should().BeTrue("because file is not moved");
        }
    }
}