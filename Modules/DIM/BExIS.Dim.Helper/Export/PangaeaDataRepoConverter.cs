using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Helpers.Models;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

namespace BExIS.Dim.Helpers.Export
{
    public class PangaeaDataRepoConverter : IDataRepoConverter
    {
        private Repository _dataRepo { get; set; }
        private Broker _broker { get; set; }

        //ToDO -> David <- do not store the files on the server
        public string Convert(long datasetVersionId)
        {
            DatasetManager datasetManager = new DatasetManager();

            PublicationManager publicationManager = new PublicationManager();
            try
            {
                _broker = publicationManager.GetBroker(_broker.Id);
                _dataRepo = publicationManager.GetRepository(_dataRepo.Id);

                /***
                 *  Generate a txt file for pangaea
                 *  1. json metadata
                 *  2. tabseperated primary Data
                 *
                 *
                 * if data only unstructred, then only metadata
                 */

                string primaryDataFilePath = "";

                #region create primary Data

                primaryDataFilePath = generatePrimaryData(datasetVersionId);

                #endregion create primary Data

                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);
                long datasetId = datasetVersion.Dataset.Id;
                long metadataStructureId = datasetVersion.Dataset.MetadataStructure.Id;

                DataStructureDataList datastructureDataList = OutputDataStructureManager.GetVariableList(datasetVersion.Dataset.DataStructure.Id);

                PanageaMetadata metadata = getMetadata(datasetVersion.Metadata, metadataStructureId);
                metadata.ParameterIDs = datastructureDataList.Variables;

                string json = JsonConvert.SerializeObject(metadata, Formatting.Indented);

                SubmissionManager submissionManager = new SubmissionManager();

                string path = submissionManager.GetDirectoryPath(datasetId, _broker.Name);

                int verionNr = datasetManager.GetDatasetVersionNr(datasetVersion);

                string filename = submissionManager.GetFileNameForDataRepo(datasetId, verionNr, _dataRepo.Name, "txt");

                string filepath = Path.Combine(path, filename);

                try
                {
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }

                    FileHelper.Create(filepath).Close();
                    File.WriteAllText(filepath, json + Environment.NewLine + Environment.NewLine);

                    if (!string.IsNullOrEmpty(primaryDataFilePath))
                    {
                        File.AppendAllText(filepath, File.ReadAllText(primaryDataFilePath));
                    }

                    return filepath;
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
            finally
            {
                datasetManager.Dispose();
                publicationManager.Dispose();
            }
        }

        public bool Validate(long datasetVersionId, out List<string> errors)
        {
            errors = new List<string>();
            return true; //throw new NotImplementedException();
        }

        private string generatePrimaryData(long datasetVersionId)
        {
            try
            {
                using (DatasetManager datasetManager = new DatasetManager())
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

                    if (datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                    {
                        OutputDataManager outputDataManager = new OutputDataManager();
                        SubmissionManager submissionManager = new SubmissionManager();

                        long datasetId = datasetVersion.Dataset.Id;
                        int versionNr = datasetManager.GetDatasetVersionNr(datasetVersion);

                        string fileName = submissionManager.GetFileNameForDataRepo(datasetId, versionNr,
                            _dataRepo.Name,
                            "csv");

                        string filepath = outputDataManager.GenerateAsciiFile(
                            datasetId,
                            datasetVersionId,
                            "text/txt",
                            false);

                        return filepath;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return "";
        }

        private PanageaMetadata getMetadata(XmlDocument metadata, long metadataStructureId)
        {
            List<string> tmp = new List<string>();

            PanageaMetadata pangaeaMetadata = new PanageaMetadata();

            //get title
            tmp = MappingUtils.GetValuesFromMetadata((int)Key.Title, LinkElementType.Key,
               metadataStructureId, XmlUtility.ToXDocument(metadata));

            if (tmp != null) pangaeaMetadata.Title = string.Join(",", tmp.ToArray());

            //get Description
            tmp = MappingUtils.GetValuesFromMetadata((int)Key.Description, LinkElementType.Key,
               metadataStructureId, XmlUtility.ToXDocument(metadata));

            if (tmp != null) pangaeaMetadata.Abstract = string.Join(",", tmp.ToArray());

            // get Authors
            tmp = MappingUtils.GetValuesFromMetadata((int)Key.Author, LinkElementType.Key,
               metadataStructureId, XmlUtility.ToXDocument(metadata));

            if (tmp != null) pangaeaMetadata.AuthorIDs = tmp;

            // get Projects
            tmp = MappingUtils.GetValuesFromMetadata((int)Key.ProjectTitle, LinkElementType.Key,
               metadataStructureId, XmlUtility.ToXDocument(metadata));

            if (tmp != null) pangaeaMetadata.ProjectIDs = tmp;

            // Defauls sets
            //ExportFileName
            pangaeaMetadata.ExportFilename = Regex.Replace(pangaeaMetadata.Title, "[^0-9a-zA-Z]+", "");

            return pangaeaMetadata;
        }

        public PangaeaDataRepoConverter(Broker broker)
        {
            _dataRepo = broker.Repository;
            _broker = broker;
        }
    }
}