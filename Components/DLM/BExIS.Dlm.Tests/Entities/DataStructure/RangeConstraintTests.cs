using BExIS.Dlm.Entities.DataStructure;
using FluentAssertions;
using NUnit.Framework;
using System.Globalization;
using Vaiona.Utils.Cfg;
namespace BExIS.Dlm.Tests.Entities.DataStructure
{
    [TestFixture()]
    public class RangeConstraintTests
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

        //double min, double max, bool negated, bool lowerBoundIncluded, bool upperBoundIncluded, culture
        //[TestCase("0.1",0,1,false,true,true,"en-US", false)]
        [TestCase(0,0,1,false,true,true,"en-US")]
        [TestCase(0.1,0,1,false,true,true,"en-US")]
        [TestCase(0.4,0,1,false,true,true,"en-US")]
        [TestCase(1,0,1,false,true,true,"en-US")]
        //[TestCase(2,0,1,false,true,true,"en-US", false)]
        public void IsSatisfied_ValueIsInRangeWithBounds_ReturnTrue(object value ,double min, double max, bool negated, bool lowerBoundIncluded, bool upperBoundIncluded, string culture)
        {
            //Arrange

            RangeConstraint rangeConstraint = new RangeConstraint(
                ConstraintProviderSource.Internal,
                "",
                culture,
                "unit test",
                negated,
                null,
                null,
                null,
                min,
                lowerBoundIncluded,
                max,
                upperBoundIncluded
                );

            //Act

            bool result = rangeConstraint.IsSatisfied(value);

            //Assert
            Assert.AreEqual(result, true);
        }

        //double min, double max, bool negated, bool lowerBoundIncluded, bool upperBoundIncluded, culture
        //[TestCase("0.1",0,1,false,true,true,"en-US", false)]
        [TestCase(2, 0, 1, false, true, true, "en-US")]
        public void IsSatisfied_ValueIsNotInRangeWithBounds_ReturnFalse(object value, double min, double max, bool negated, bool lowerBoundIncluded, bool upperBoundIncluded, string culture)
        {
            //Arrange

            RangeConstraint rangeConstraint = new RangeConstraint(
                ConstraintProviderSource.Internal,
                "",
                culture,
                "unit test",
                negated,
                null,
                null,
                null,
                min,
                lowerBoundIncluded,
                max,
                upperBoundIncluded
                );

            //Act

            bool result = rangeConstraint.IsSatisfied(value);

            //Assert
            Assert.AreEqual(result, false);
        }


        //double min, double max, bool negated, bool lowerBoundIncluded, bool upperBoundIncluded, culture
        [TestCase("0.1",0,1,false,true,true,"en-US")]
        public void IsSatisfied_ValueIsStringButRangeMeansNumber_ReturnFalse(object value, double min, double max, bool negated, bool lowerBoundIncluded, bool upperBoundIncluded, string culture)
        {
            //Arrange
            RangeConstraint rangeConstraint = new RangeConstraint(
                ConstraintProviderSource.Internal,
                "",
                culture,
                "unit test",
                negated,
                null,
                null,
                null,
                min,
                lowerBoundIncluded,
                max,
                upperBoundIncluded
                );

            //Act
            bool result = rangeConstraint.IsSatisfied(value);

            //Assert
            Assert.AreEqual(result, false);
        }

    }
}