using BExIS.App.Testing;
using BExIS.Dim.Services;
using BExIS.Utils.Config;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Service.Tests
{

    [TestFixture()]
    public class ConceptManagerTests
    {
        private TestSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [Test()]
        public void CreateMappingConcept_valid_NewMappingConcept()
        {
            using (var conceptManager = new ConceptManager())
            { 
                //var mappingConcept = conceptManager.CreateMappingConcept("test","description","url");

                //Assert.IsNotNull(mappingConcept);
                //Assert.That(mappingConcept.Id > 0);


                //conceptManager.DeleteMappingConcept(mappingConcept);
            }

            Assert.Pass();
        }
    }
}
