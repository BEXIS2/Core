using BExIS.App.Testing;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Linq;

namespace BExIS.Dlm.Tests.Services.DataStructure
{
    [TestFixture()]
    public class ConstraintManagerTests
    {
        private TestSetupHelper helper = null;

        //[Test]
        public void CreateConstraints()
        {
            string description = "created by a unit test.";
            string name = "Test Range Constraint";

            RangeConstraint rangeConstraint = new RangeConstraint(
                ConstraintProviderSource.Internal,
                "",
                "en-US",
                "unit test",
                false,
                null,
                null,
                null,
                1,
                true,
                100,
                true
            );

            rangeConstraint.Description = description;
            rangeConstraint.Name = name;

            using (ConstraintManager constraintManager = new ConstraintManager())
            {
                Assert.Throws<ArgumentNullException>(() => constraintManager.Create(rangeConstraint));
            }

            name = "Test Pattern Constraint";

            PatternConstraint patternConstraint = new PatternConstraint();
            patternConstraint.Name = name;
            patternConstraint.Description = description;
            patternConstraint.MatchingPhrase = "[a-z]";

            using (ConstraintManager constraintManager = new ConstraintManager())
            {
                Assert.Throws<ArgumentNullException>(() => patternConstraint = constraintManager.Create(patternConstraint));
            }

            name = "Test Domain Constraint";

            DomainConstraint domainConstraint = new DomainConstraint();
            domainConstraint.Name = name;
            domainConstraint.Description = description;

            using (ConstraintManager constraintManager = new ConstraintManager())
            {
                Assert.Throws<ArgumentNullException>(() => domainConstraint = constraintManager.Create(domainConstraint));
            }
        }

        //[Test] func_usecase_result
        public void GetConstraints()
        {
            RangeConstraint rangeConstraint = new RangeConstraint();
            PatternConstraint patternConstraint = new PatternConstraint();
            DomainConstraint domainConstraint = new DomainConstraint();
            //List<Constraint> constraints = new List<Constraint>();

            using (ConstraintManager constraintManager = new ConstraintManager())
            {
                Assert.Throws<ArgumentNullException>(() => rangeConstraint = constraintManager.RangeConstraints.FirstOrDefault());
                Assert.Throws<ArgumentNullException>(() => patternConstraint = constraintManager.PatternConstraints.FirstOrDefault());
                Assert.Throws<ArgumentNullException>(() => domainConstraint = constraintManager.DomainConstraints.FirstOrDefault());
                //Assert.Throws<ArgumentNullException>(() => constraints = constraintManager.Repo.Get().ToList());
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [SetUp]
        protected void SetUp()
        {
        }
    }
}