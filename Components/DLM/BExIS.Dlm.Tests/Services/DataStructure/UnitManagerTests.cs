using BExIS.App.Testing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Helpers;
using BExIS.Web.Shell;
using BExIS.Web.Shell.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Dlm.Tests.Services.DataStructure
{    
    public class UnitManagerTests
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
            helper.Dispose();
        }

        [Test()]
        public void CreateAMeasurementUnitTest()
        {
            UnitManager um = new UnitManager();
            var dummyDimension = um.DimensionRepo.Query().First();
            Unit km = um.Create("KilometerTest", "KmTest", "This is the Kilometer", dummyDimension, MeasurementSystem.Metric);

            km.Should().NotBeNull();
            km.Id.Should().BeGreaterThan(0);
            var fetchedKm = um.Repo.Get(km.Id);
            km.Abbreviation.Should().BeEquivalentTo(fetchedKm.Abbreviation);
            km.Name.Should().BeEquivalentTo(fetchedKm.Name);

            um.Delete(km); // cleanup the DB
        }

        [Test()]
        public void CreateConversionsBetweenUnitsTest()
        {
            UnitManager um = new UnitManager();
            var dummyDimension = um.DimensionRepo.Query().First();

            Unit km = um.Create("KilometerTest", "KmTest", "This is the Kilometer", dummyDimension, MeasurementSystem.Metric);
            Unit m = um.Create("MeterTest", "MTest", "This is the Meter", dummyDimension, MeasurementSystem.Metric);

            ConversionMethod cm2 = um.CreateConversionMethod("s*1000", "Converts kilometer to meter", km, m);
            ConversionMethod cm3 = um.CreateConversionMethod("s/1000", "Converts meter to kilometer", m, km);

            km.ConversionsIamTheSource.First().Target.Should().BeEquivalentTo(m);
            cm3.Source.Name.Should().BeEquivalentTo(m.Name);
            cm3.Target.Name.Should().BeEquivalentTo(cm2.Source.Name);

            // cleanup the DB.
            um.DeleteConversionMethod(new List<ConversionMethod>() { cm2, cm3 });
            um.Delete(new List<Unit>() { km, m });


        }
    }

}
