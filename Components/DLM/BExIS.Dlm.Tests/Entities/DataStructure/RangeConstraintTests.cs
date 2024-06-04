using BExIS.Dlm.Entities.DataStructure;
using NUnit.Framework;

namespace BExIS.Dlm.Tests.Entities.DataStructure
{
    [TestFixture()]
    public class RangeConstraintTests
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

        //double min, double max, bool negated, bool lowerBoundIncluded, bool upperBoundIncluded, culture
        //[TestCase("0.1",0,1,false,true,true,"en-US", false)]
        [TestCase(0, 0, 1, false, true, true, "en-US")]
        [TestCase(0.1, 0, 1, false, true, true, "en-US")]
        [TestCase(0.4, 0, 1, false, true, true, "en-US")]
        [TestCase(1, 0, 1, false, true, true, "en-US")]
        public void IsSatisfied_ValueIsInRangeWithBounds_ReturnTrue(object value, double min, double max, bool negated, bool lowerBoundIncluded, bool upperBoundIncluded, string culture)
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
        [TestCase("0.1", 0, 1, false, true, true, "en-US")]
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

        [Test]
        public void IsSatisfied_BoundInlcude_ValuesValid()
        {
            //Arrange
            RangeConstraint rangeConstraint = new RangeConstraint(
                ConstraintProviderSource.Internal,
                "",
                "en-us",
                "unit test",
                false,
                null,
                null,
                null,
                0,
                true,
                1,
                true
                );

            //Act
            bool minBoundIsValid = rangeConstraint.IsSatisfied(0);
            bool maxBoundIsValid = rangeConstraint.IsSatisfied(1);

            //Assert
            Assert.AreEqual(minBoundIsValid, true, "0 is min and should be valid");
            Assert.AreEqual(maxBoundIsValid, true, "1 is max and should be valid");
        }

        [Test]
        public void IsSatisfied_BoundExclude_ValuesNotValid()
        {
            //Arrange
            RangeConstraint rangeConstraint = new RangeConstraint(
                ConstraintProviderSource.Internal,
                "",
                "en-us",
                "unit test",
                false,
                null,
                null,
                null,
                0,
                false,
                1,
                false
                );

            //Act
            bool minBoundIsValid = rangeConstraint.IsSatisfied(0);
            bool maxBoundIsValid = rangeConstraint.IsSatisfied(1);

            //Assert
            Assert.AreEqual(minBoundIsValid, false);
            Assert.AreEqual(maxBoundIsValid, false);
        }
    }
}