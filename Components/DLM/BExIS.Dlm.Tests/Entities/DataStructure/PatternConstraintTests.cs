using BExIS.Dlm.Entities.DataStructure;
using NUnit.Framework;
namespace BExIS.Dlm.Tests.Entities.DataStructure
{
    [TestFixture()]
    public class PatternConstraintTests
    {
        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
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
        { }

        [TestCase("A",true)]
        [TestCase("a", false)]
        [TestCase("0", false)]
        [TestCase("ABCD", true)]
        [TestCase("1A", false)]
        public void IsSatisfied_PatternCaseSensitive_ReturnValid(string value, bool expect)
        {
            //Arrange

            PatternConstraint constraint = new PatternConstraint(
             ConstraintProviderSource.Internal,
                "",
                "en-us",
                "unit test",
                false,
                "",
                "",
                "",
                "^[A-Z]+$",
                true);

            //Act

            bool result = constraint.IsSatisfied(value);

            //Assert
            Assert.AreEqual(expect, result);
        }

        [TestCase("A", true)]
        [TestCase("a", true)]
        [TestCase("0", false)]
        [TestCase("ABCD", true)]
        [TestCase("ABCD1", false)]

        public void IsSatisfied_Pattern_ReturnValid(string value, bool expect)
        {
            //Arrange

            PatternConstraint constraint = new PatternConstraint(
             ConstraintProviderSource.Internal,
                "",
                "en-us",
                "unit test",
                false,
                "",
                "",
                "",
                 "^[A-Z]+$",
                false);

            //Act

            bool result = constraint.IsSatisfied(value);

            //Assert
            Assert.AreEqual(expect, result);

        }


    }
}