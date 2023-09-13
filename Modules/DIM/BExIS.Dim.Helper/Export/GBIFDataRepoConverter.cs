﻿using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Helpers.GBIF;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dim.Helpers.Models;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Vaiona.Utils.Cfg;

namespace BExIS.Dim.Helpers.Export
{
    public class GBIFDataRepoConverter : IDataRepoConverter
    {
        private Repository _dataRepo { get; set; }
        private Broker _broker { get; set; }

        public string Convert(long datasetVersionId)
        {
            GbifHelper helper = new GbifHelper();

            string path = "";
            using (var datasetManager = new DatasetManager())
            using (var conceptManager = new ConceptManager())
            using (ZipFile zip = new ZipFile())
            {
                var datasetversion = datasetManager.GetDatasetVersion(datasetVersionId);
                var dataset = datasetversion.Dataset;
                var versionNumber = datasetManager.GetDatasetVersionNr(datasetVersionId);

                string datasetsPath = Path.Combine(AppConfiguration.DataPath, "Datasets");
                string subpath = Path.Combine(dataset.Id.ToString(), "publish", "gbif");
                string folder = Path.Combine(datasetsPath, subpath);

                FileHelper.CreateDicrectoriesIfNotExist(folder);

                string zipfilename = "dwc_" + dataset.Id + "_" + versionNumber + "_Dataset.zip";

                string zipfilepath = Path.Combine(folder, zipfilename);

                if (File.Exists(zipfilepath)) return zipfilepath;


                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);


                // get metadata
                var concept = conceptManager.MappingConceptRepo.Get().FirstOrDefault(c => c.Name.ToLower().Equals(_dataRepo.Name.ToLower()));
                if (concept != null)
                {
                    long metadatastructureId = dataset.MetadataStructure.Id;
                    long id = dataset.Id;
                    long versionId = datasetversion.Id; 
                    string xsdPath = Path.Combine(AppConfiguration.WorkspaceRootPath, concept.XSD);

                    // generate metadata
                    string metadataFilePath = helper.GenerateResourceMetadata(concept.Id, metadatastructureId, datasetversion.Metadata, folder, xsdPath);
                    if(File.Exists(metadataFilePath)) zip.AddFile(metadataFilePath, "");
                
                    // get data
                    string datapath = helper.GenerateData(id, versionId);
                    if (File.Exists(datapath)) zip.AddFile(datapath, "");

                    // generate meta file
                    string metaFilePath = helper.GenerateMeta(GbifDataType.samplingEvent, dataset.DataStructure.Id, folder, Path.GetFileName(datapath));
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
            {
                var datasetversion = datasetManager.GetDatasetVersion(datasetVersionId);
                long datasetId = datasetversion.Dataset.Id;
                dataStructureId = datasetversion.Dataset.DataStructure.Id;
                metadataStructureId = datasetversion.Dataset.MetadataStructure.Id;

                string datasetsPath = Path.Combine(AppConfiguration.DataPath, "Datasets");
                string subpath = Path.Combine(datasetId.ToString(), "publish", "gbif");
                string folder = Path.Combine(datasetsPath, subpath);

                FileHelper.CreateDicrectoriesIfNotExist(folder);

                var concept = conceptManager.MappingConceptRepo.Get().Where(c => c.Name.Equals(_dataRepo.Name)).FirstOrDefault();

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
                        GbifHelper helper = new GbifHelper();
                        string xsdPath = Path.Combine(AppConfiguration.WorkspaceRootPath, concept.XSD);

                        string metadataPath = helper.GenerateResourceMetadata(concept.Id, metadataStructureId, datasetversion.Metadata, folder, xsdPath);

                        List<string> metadataErrors = new List<string>();
                        helper.ValidateResourceMetadata(metadataPath, xsdPath, out metadataErrors);

                        errors.AddRange(metadataErrors);
                    }

                }

                // in V2 file for structure exist
                // in data structure - dcw terms
                string dwtermsFilePath = Path.Combine(AppConfiguration.DataPath, "DataStructures", dataStructureId.ToString(), "dw_terms.json");

                if (!File.Exists(dwtermsFilePath))
                    errors.Add("dw_terms.json file not exist.");

                // check all needed dw terms mapped for the type



                //check if data exist
                if (datasetManager.GetDataTuplesCount(datasetVersionId)<=0)
                    errors.Add("no data available.");

            }

            if (errors.Any()) valid = false;

            return valid;

        }

        public GBIFDataRepoConverter(Repository datarepo)
        {
            _dataRepo = datarepo;
            _broker = datarepo.Broker;
        }
    }
}
