using BExIS.Dlm.Entities.DataStructure;
using NUnit.Framework;

namespace BExIS.Dlm.Tests.Entities.DataStructure
{
    [TestFixture()]
    public class PatternConstraintTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // because these tests are working on in-memory objects (datasets) only, there is no need to do the test app setup
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
        { }

        [TestCase("A", true)]
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