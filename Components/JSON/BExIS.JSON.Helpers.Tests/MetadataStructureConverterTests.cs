using BExIS.App.Testing;
using BExIS.Utils.Config;
using BEXIS.JSON.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using NUnit.Framework;
using System;
using System.IO;

namespace BExIS.JSON.Helpers.UnitTests
{
    public class MetadataStructureManagerTests
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

        [Test]
        public void ConvertToJsonSchema_valid_returnJsonSchema()
        {
            var metadataStructureConverter = new MetadataStructureConverter();
            var schema = metadataStructureConverter.ConvertToJsonSchema(1);

            string path = AppDomain.CurrentDomain.BaseDirectory;
            string schemaPath = Path.Combine(path, "schema.json");


            // serialize JsonSchema directly to a file
            using (StreamWriter file = File.CreateText(schemaPath))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                schema.WriteTo(writer);
            }

            Assert.NotNull(schema);
        }

        [Test]
        public void ConvertToJsonSchema_IdNotExist_ArgumentException()
        {
            var metadataStructureConverter = new MetadataStructureConverter();

            Assert.That(() => metadataStructureConverter.ConvertToJsonSchema(10), Throws.ArgumentNullException);

        }

        [Test]
        public void ConvertToJsonSchema_IdIs0_ArgumentException()
        {
            var metadataStructureConverter = new MetadataStructureConverter();

            Assert.That(() => metadataStructureConverter.ConvertToJsonSchema(0), Throws.ArgumentException);

        }
    }
}