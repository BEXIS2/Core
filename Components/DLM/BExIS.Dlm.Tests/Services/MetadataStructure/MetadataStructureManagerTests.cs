using BExIS.App.Testing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Tests.Helpers;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using BExIS.Dlm.Entities.Administration;
using BExIS.Utils.NH.Querying;

namespace BExIS.Dlm.Tests.Services.Data
{
    public class MetadataStructureManagerTests
    {
        private TestDBSetupHelper helper = null;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestDBSetupHelper();
            helper.Start();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper.Dispose();
        }

        [Test()]
        public void Get_valid_returnMetadataStructure()
        {
            //test
            Assert.That(true);
        }

            
    }
}