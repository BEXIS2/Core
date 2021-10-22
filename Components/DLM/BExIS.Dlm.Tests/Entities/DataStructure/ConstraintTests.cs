using BExIS.Dlm.Entities.DataStructure;
using FluentAssertions;
using NUnit.Framework;
using Vaiona.Utils.Cfg;
namespace BExIS.Dlm.Tests.Entities.DataStructure
{
    [TestFixture()]
    public class ConstraintTests
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

        [Test]
        public void PatternConstraintRegExTest()
        {
            string description = "created by a unit test.";
            bool negated = false;
            string matchingPhrase = "^[a-w]+$";

            PatternConstraint constraint = new PatternConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, description, negated, null, null, null, matchingPhrase, true);

            constraint.IsSatisfied("abc").Should().Be(true, "should be true because : value = abc & regex= " + matchingPhrase);
            constraint.IsSatisfied("123").Should().Be(false, "should be false because : value = 123 & regex= " + matchingPhrase + " is not matching.");
            constraint.IsSatisfied("xyz").Should().Be(false, "should be false because : value = xyz & regex= " + matchingPhrase + " is not matching.");
            constraint.IsSatisfied("xyza").Should().Be(false, "should be false because : value = xyza & regex= " + matchingPhrase + " is not matching.");
            constraint.IsSatisfied("ab123").Should().Be(false, "should be false because : value = ab123 & regex= " + matchingPhrase + " is not matching.");
            constraint.IsSatisfied("Abc").Should().Be(false, "should be false because : value = Abc & regex= " + matchingPhrase + " is not matching.");

        }
    }
}