using BExIS.App.Testing;
using BExIS.IO.Transform.Input;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using Assert = NUnit.Framework.Assert;

namespace BExIS.UI.Tests
{
    [TestFixture()]
    public class HookManagerTests
    {
        //private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
            //helper = new TestSetupHelper(WebApiConfig.Register, false);
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

        //[Test()]
        public void Construtor_New_ReturnNewHookManager()
        {
            //Arrange
            HookManager hookManager = new HookManager();
            //Act

            //Assert
            Assert.IsNotNull(hookManager);
        }

        //[Test()]
        public void GetHooksFor_WithoutErrors_ReturnHooksAsList()
        {
            //Arrange
            HookManager hookManager = new HookManager();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var x = Assembly.Load("BExIS.Modules.Dcm.UI");

            //Act
            //var l = hookManager.GetHooksFor("", "", "");

            //Assert
            Assert.IsNotNull(hookManager);
        }

        [Test()]
        public void LoadCache_ValidParameters_ReturnCache()
        {
            //Arrange
            HookManager hookManager = new HookManager();

            //Act
            EditDatasetDetailsCache cache = null;

            cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, 1);

            //Assert
            Assert.IsNotNull(cache);
        }

        [Test()]
        public void LoadCache_InValidParamaters_ThrowArgumentException()
        {
            //Arrange
            HookManager hookManager = new HookManager();

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => hookManager.LoadCache<EditDatasetDetailsCache>("", "details", HookMode.edit, 1));
            Assert.Throws<ArgumentNullException>(() => hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "", HookMode.edit, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, 0));
        }

        [Test()]
        public void SaveCache_ValidParameters_ReturnTrue()
        {
            //Arrange
            HookManager hookManager = new HookManager();

            //Act
            EditDatasetDetailsCache cache = new EditDatasetDetailsCache();

            bool result = hookManager.SaveCache<EditDatasetDetailsCache>(cache, "dataset", "details", HookMode.edit, 1);

            //Assert
            Assert.IsTrue(result);
        }

        [Test()]
        public void SaveCache_InValidParamaters_ThrowArgumentException()
        {
            //Arrange
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = new EditDatasetDetailsCache();

            //Act
            //Assert
            Assert.Throws<ArgumentNullException>(() => hookManager.SaveCache<EditDatasetDetailsCache>(null, "dataset", "details", HookMode.edit, 1));
            Assert.Throws<ArgumentNullException>(() => hookManager.SaveCache<EditDatasetDetailsCache>(cache, "", "details", HookMode.edit, 1));
            Assert.Throws<ArgumentNullException>(() => hookManager.SaveCache<EditDatasetDetailsCache>(cache, "dataset", "", HookMode.edit, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => hookManager.SaveCache<EditDatasetDetailsCache>(cache, "dataset", "details", HookMode.edit, 0));
        }

        [Test()]
        public void SaveCache_ComplexExample_ReturnTrue()
        {
            //Arrange
            HookManager hookManager = new HookManager();

            // pepare cache
            EditDatasetDetailsCache cache = new EditDatasetDetailsCache();
            cache.IsMetadataValid = true;
            cache.ExcelFileReaderInfo = new ExcelFileReaderInfo();
            cache.UpdateSetup = new UpdateSetup() { RowsCount = 1000, VariablesCount = 10 };

            //var list = new System.Collections.Generic.List<ResultMessage>();
            //for (int i = 0; i < 10; i++)
            //{
            //    list.Add(new ResultMessage() { Messages = new List<string>() { i.ToString() + "_test" }, Timestamp = DateTime.Now }); ;
            //}

            //cache.Messages = list;

            //Act
            bool result = hookManager.SaveCache<EditDatasetDetailsCache>(cache, "dataset", "details", HookMode.edit, 1);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void LoadCache_LoadExcelReaderInfo_ReturnExcelReaderInfo()
        {
            //Arrange
            HookManager hookManager = new HookManager();

            // pepare cache
            EditDatasetDetailsCache cache = new EditDatasetDetailsCache();
            cache.IsMetadataValid = true;
            cache.ExcelFileReaderInfo = new ExcelFileReaderInfo();
            cache.UpdateSetup = new UpdateSetup() { RowsCount = 1000, VariablesCount = 10 };

            //var list = new System.Collections.Generic.List<ResultMessage>();
            ////for (int i = 0; i < 10; i++)
            ////{
            ////    list.Add(new ResultMessage() { Messages = new List<string>() { i.ToString() + "_test" }, Timestamp = DateTime.Now }); ;
            ////}

            //cache.Messages = list;

            //Act
            bool result = hookManager.SaveCache<EditDatasetDetailsCache>(cache, "dataset", "details", HookMode.edit, 1);

            cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, 1);

            var excelFileReader = cache.ExcelFileReaderInfo as ExcelFileReaderInfo;

            //Assert
            Assert.NotNull(excelFileReader);
        }

        [Test]
        public void LoadCache_LoadAsciiReaderInfo_ReturnAsciiReaderInfo()
        {
            //Arrange
            HookManager hookManager = new HookManager();

            // pepare cache
            EditDatasetDetailsCache cache = new EditDatasetDetailsCache();
            cache.IsMetadataValid = true;
            cache.AsciiFileReaderInfo = new AsciiFileReaderInfo();
            cache.UpdateSetup = new UpdateSetup() { RowsCount = 1000, VariablesCount = 10 };

            //var list = new System.Collections.Generic.List<ResultMessage>();
            ////for (int i = 0; i < 10; i++)
            ////{
            ////    list.Add(new ResultMessage() { Messages = new List<string>() { i.ToString() + "_test" }, Timestamp = DateTime.Now }); ;
            ////}

            //cache.Messages = list;

            //Act
            bool result = hookManager.SaveCache<EditDatasetDetailsCache>(cache, "dataset", "details", HookMode.edit, 1);

            cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, 1);

            var readerInfo = cache.AsciiFileReaderInfo as AsciiFileReaderInfo;

            //Assert
            Assert.NotNull(readerInfo);
        }
    }
}