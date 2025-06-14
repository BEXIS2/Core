﻿using BExIS.Dim.Entities.Export;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Services.Data;
using BEXIS.JSON.Helpers.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaelastrasz.Library.Models;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Helpers.BIOSCHEMA
{
    public class BioSchemaHelper
    {
        public string GetBioSchemaForDataset(long id, int versionNr, string url)
        {
            try
            {
                using (var conceptManager = new ConceptManager())
                using (var datasetManager = new DatasetManager())
                {
                    var dataset = datasetManager.GetDataset(id);
                    if (dataset == null) return string.Empty;

                    var firstVersion = datasetManager.GetDatasetVersion(id, 1);
                    var version = datasetManager.GetDatasetVersion(id, versionNr);
                    if (version == null) return string.Empty;


                    var concept = conceptManager.FindByName("BIOSCHEMA-Dataset");
                    if (concept == null) return string.Empty;

                    var xml = MappingUtils.GetConceptOutput(dataset.MetadataStructure.Id, concept.Id, version.Metadata);
                    var json = JsonConvert.SerializeObject(xml);

                    var jObject = JObject.Parse(json);
                    JsonExtensions.TransformToMatchClassTypes(jObject, typeof(BioSchema));
                    json = JsonConvert.SerializeObject(jObject, Newtonsoft.Json.Formatting.Indented);

                    DateTime creationDate = (DateTime)firstVersion.ModificationInfo?.Timestamp;
                    DateTime modificationDate = (DateTime)version?.ModificationInfo?.Timestamp;

                    BioSchema bioSchema = JsonConvert.DeserializeObject<BioSchema>(json);
                    if (bioSchema.Dataset != null)
                    {
                        bioSchema.Dataset.Id = url;
                        bioSchema.Dataset.Version = "" + versionNr;
                        bioSchema.Dataset.Identifier = "" + id;
                        bioSchema.Dataset.Url = url;
                        bioSchema.Dataset.DateCreated = creationDate.ToString();
                        bioSchema.Dataset.DateModified = modificationDate.ToString();

                        if (version.PublicAccess)
                        {
                            bioSchema.Dataset.DatePublished = version.ModificationInfo?.Timestamp.ToString();
                        }
                    }

                    json = JsonConvert.SerializeObject(bioSchema.Dataset);

                    return json;
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

    }
}
