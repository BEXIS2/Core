using BExIS.App.Testing;
using BExIS.UI.Hooks;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Reflection;
using Assert = NUnit.Framework.Assert;

namespace BExIS.UI.Tests
{
    [TestFixture()]
    public class HookManagerTests
    {

        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved.
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
            helper = new TestSetupHelper(WebApiConfig.Register, false);
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
        }

        [Test()]
        public void Construtor_New_ReturnNewHookManager()
        {
            //Arrange
            HookManager hookManager = new HookManager();
            //Act

            //Assert
            Assert.IsNotNull(hookManager);
        }

        [Test()]
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

    }
}