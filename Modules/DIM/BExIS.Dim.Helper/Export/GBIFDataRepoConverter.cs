using BExIS.Dim.Entities.Export.GBIF;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.GBIF;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Helpers.Models;
using BExIS.Dim.Services;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;
using static BExIS.Dim.Helpers.Export.GBIFDataRepoConverter;

namespace BExIS.Dim.Helpers.Export
{
    public class GBIFDataRepoConverter : IDataRepoConverter
    {
        public enum ExtensionType
        {
            Occurence,
            Event,
            Humbolt,
            EMof,
            Organism,
            MaterialEntity,
            MaterialSample,
            MeasurementOrFact,
            ResourceRelationship,
            Taxon,
            Identification,
            Location,
            ChronometricAge,
            GeologicalContext
        }

        private Repository _dataRepo { get; set; }

        private GbifDataType _type { get; set; }

        public string Convert(long datasetVersionId)
        {
            GbifHelper helper = new GbifHelper();

            string path = "";
            using (var datasetManager = new DatasetManager())
            using (var conceptManager = new ConceptManager())
            using (var entityReferenceManager = new EntityReferenceManager())
            using (ZipFile zip = new ZipFile())
            {
                string dwcStorePath = ModuleManager.GetModuleSettings("DIM").GetValueByKey("gbifCollectionArea").ToString();


                var datasetversion = datasetManager.GetDatasetVersion(datasetVersionId);
                var dataset = datasetversion.Dataset;
                var versionNumber = datasetManager.GetDatasetVersionNr(datasetVersionId);

                // setup folder to store the dwc zip
                // if starts with / it means it should store under {DATA}/ dwcStorePath
                // if starts with Char then it means the full Folepath is defined 

                string folder = Regex.Match(dwcStorePath, "^([A-Z]:\\|/)").Success ? dwcStorePath : Path.Combine(AppConfiguration.DataPath, dwcStorePath);

                FileHelper.CreateDicrectoriesIfNotExist(folder);

                string zipfilename = "dwc_" + dataset.Id + "_" + versionNumber + "_Dataset.zip";

                string zipfilepath = Path.Combine(folder, zipfilename);

                if (File.Exists(zipfilepath)) return zipfilepath;

                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);


                // get metadata
                var concept = conceptManager.MappingConceptRepository.Get().FirstOrDefault(c => c.Name.ToLower().Equals(_dataRepo.Name.ToLower()));
                if (concept != null)
                {
                    long metadatastructureId = dataset.MetadataStructure.Id;
                    long id = dataset.Id;
                    long versionId = datasetversion.Id;
                    string xsdPath = Path.Combine(AppConfiguration.WorkspaceRootPath, concept.XSD);

                    // generate metadata
                    string metadataFilePath = helper.GenerateResourceMetadata(concept.Id, metadatastructureId, datasetversion.Metadata, folder, xsdPath);
                    if (File.Exists(metadataFilePath)) zip.AddFile(metadataFilePath, "");

                    // get data
                    string datapath = helper.GenerateData(id, versionId);
                    if (File.Exists(datapath)) zip.AddFile(datapath, "");

                    // has links to extentions? (IsDwcEventOf,IsDwcHumboltExtensionOf, IsEwcEMofExtensionOf)
                    // has links?
                    var links = helper.GetExtentions().Select(e=>e.LinkName);
                    var refs = entityReferenceManager.ReferenceRepository.Get().Where(r => r.SourceEntityId == dataset.EntityTemplate.EntityType.Id && r.SourceId == dataset.Id && links.Contains(r.ReferenceType)).ToList();
                    // add data from linked datasets links 
                    List<ExtentionEntity> extentions = new List<ExtentionEntity>();
                    foreach (var r in refs)
                    {
                        if (links.Contains(r.ReferenceType))
                        {
                            var ext = datasetManager.GetDataset(r.TargetId);
                            var structureId = ext.DataStructure == null?0: ext.DataStructure.Id;
                            var refVersion = datasetManager.GetDatasetVersion(r.TargetId, r.TargetVersion);
                            string rPath = helper.GenerateData(r.TargetId, refVersion.Id);

                            if (File.Exists(rPath))
                            {
                                // how to find the position of id of the core
                                // check links for the specifiy types
                                var extention = helper.GetExtention(r.ReferenceType);

                                extentions.Add(new ExtentionEntity() { IdIndex = 0, Version = r.SourceVersion, StructureId = structureId, Extention = extention, dataPath = Path.GetFileName(rPath) });
                                zip.AddFile(rPath, "");
                            }
                        }
                    }

                    // generate meta file
                    string metaFilePath = helper.GenerateMeta(_type, dataset.DataStructure.Id, folder, Path.GetFileName(datapath), extentions);
                    if (File.Exists(metaFilePath)) zip.AddFile(metaFilePath, "");

                    zip.Save(zipfilepath);

                    return zipfilepath;

                }
            }

            return "";
        }

        public bool Validate(long datasetVersionId, out List<string> errors)
        {

            long metadataStructureId = 0;
            long dataStructureId = 0;
            bool valid = true;
            errors = new List<string>();
            // metadata ist valid
            using (var conceptManager = new ConceptManager())
            using (var datasetManager = new DatasetManager())
            using (var entityReferenceManager = new EntityReferenceManager())

            {
                var datasetversion = datasetManager.GetDatasetVersion(datasetVersionId);
                var dataset = datasetversion.Dataset;
                long datasetId = datasetversion.Dataset.Id;

                if (datasetversion.Dataset.DataStructure == null) errors.Add("no data structure exist for this entity.");


                dataStructureId = datasetversion.Dataset.DataStructure.Id;
                metadataStructureId = datasetversion.Dataset.MetadataStructure.Id;

                string datasetsPath = Path.Combine(AppConfiguration.DataPath, "Datasets");
                string subpath = Path.Combine(datasetId.ToString(), "publish", "gbif");
                string folder = Path.Combine(datasetsPath, subpath);

                FileHelper.CreateDicrectoriesIfNotExist(folder);
                GbifHelper helper = new GbifHelper();


                var concept = conceptManager.MappingConceptRepository.Get().Where(c => c.Name.Equals(_dataRepo.Name)).FirstOrDefault();

                if (concept == null)
                {
                    errors.Add("Concept not exist.");
                    return false;
                }
                else
                {
                    List<string> errorsList = new List<string>();
                    if (MappingUtils.IsMapped(metadataStructureId, LinkElementType.MetadataStructure, concept.Id, LinkElementType.MappingConcept, out errorsList) == false)
                    {
                        errors.AddRange(errorsList);
                        return false;
                    }

                    // if concept is linked to a xsd, generate metadata and validaed it against the schema
                    if (!string.IsNullOrEmpty(concept.XSD))
                    {
                        
                        string xsdPath = Path.Combine(AppConfiguration.WorkspaceRootPath, concept.XSD);

                        string metadataPath = helper.GenerateResourceMetadata(concept.Id, metadataStructureId, datasetversion.Metadata, folder, xsdPath);

                        List<string> metadataErrors = new List<string>();
                        helper.ValidateResourceMetadata(metadataPath, xsdPath, out metadataErrors);

                        errors.AddRange(metadataErrors);
                    }

                }
                // in V3 read from structre
                helper.ValidateDWCTerms(dataStructureId, _type, out errors);

                // validate extentions
                var links = helper.GetExtentions().Select(e=>e.LinkName);
                var refs = entityReferenceManager.ReferenceRepository.Get().Where(r => r.SourceEntityId == dataset.EntityTemplate.EntityType.Id && r.SourceId == dataset.Id && links.Contains(r.ReferenceType)).ToList();
                // add data from linked datasets links 
                List<ExtentionEntity> extentions = new List<ExtentionEntity>();
                foreach (var r in refs)
                {
                    var ext = datasetManager.GetDataset(r.SourceId);
                    var structureId = ext.DataStructure == null ? 0 : ext.DataStructure.Id;

                    // load structure and check if a dwc term id is available 
                    // based on type event or occurence
                    var dwcextention = helper.GetExtentions().FirstOrDefault(e => e.LinkName == r.ReferenceType);
                    List<string> extErrors = new List<string>();
                    helper.ValidateExtension(ext.Id, ext.DataStructure.Id, _type, dwcextention, out extErrors);
                    errors.AddRange(extErrors);
                    
                }

                //check if data exist
                if (datasetManager.GetDataTuplesCount(datasetVersionId)<=0)
                    errors.Add("no data available.");

            }

            if (errors.Any()) valid = false;

            return valid;

        }

        public GBIFDataRepoConverter(Broker _broker, GbifDataType type)
        {
            _dataRepo = _broker.Repository;
            _type = type;
        }
    }

}
