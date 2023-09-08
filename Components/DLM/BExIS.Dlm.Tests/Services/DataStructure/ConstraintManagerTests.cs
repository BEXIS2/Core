using BExIS.App.Testing;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Utils.Config;
using FluentAssertions;
using NHibernate.Mapping;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Vaiona.Utils.Cfg;
namespace BExIS.Dlm.Tests.Services.DataStructure
{
    [TestFixture()]
    public class ConstraintManagerTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        /// It is called once prior to executing any of the tests in a fixture.
        /// Multiple methods can be marked. Order is not preserved. 
        /// Inheritance is supported, call sequence form the parents
        public void OneTimeSetUp()
        {
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
            helper.Dispose();
        }

        [Test]
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

        [Test]
        public void GetConstraints()
        {
            RangeConstraint rangeConstraint = new RangeConstraint();
            PatternConstraint patternConstraint = new PatternConstraint();
            DomainConstraint domainConstraint = new DomainConstraint();
            //List<Constraint> constraints = new List<Constraint>();

            using (ConstraintManager constraintManager = new ConstraintManager())
            {
                Assert.Throws<ArgumentNullException>(() => rangeConstraint = constraintManager.RangeConstraintRepo.Get().FirstOrDefault());
                Assert.Throws<ArgumentNullException>(() => patternConstraint = constraintManager.PatternConstraintRepo.Get().FirstOrDefault());
                Assert.Throws<ArgumentNullException>(() => domainConstraint = constraintManager.DomainConstraintRepo.Get().FirstOrDefault());
                //Assert.Throws<ArgumentNullException>(() => constraints = constraintManager.Repo.Get().ToList());
            }
        }
    }
}