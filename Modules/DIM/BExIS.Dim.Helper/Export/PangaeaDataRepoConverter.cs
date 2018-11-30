using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Helpers.Mapping;
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


                #endregion


                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);
                long datasetId = datasetVersion.Dataset.Id;
                long metadataStructureId = datasetVersion.Dataset.MetadataStructure.Id;

                DataStructureDataList datastructureDataList = OutputDataStructureManager.GetVariableList(datasetVersion.Dataset.DataStructure.Id);

                PanageaMetadata metadata = getMetadata(datasetVersion.Metadata, metadataStructureId);
                metadata.ParameterIDs = datastructureDataList.Variables;

                string json = JsonConvert.SerializeObject(metadata, Formatting.Indented);


                SubmissionManager submissionManager = new SubmissionManager();



                string path = submissionManager.GetDirectoryPath(datasetId, _broker.Name);
                string filename = submissionManager.GetFileNameForDataRepo(datasetVersionId, datasetId, _dataRepo.Name, "txt");

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





                return "";
            }
            finally
            {
                datasetManager.Dispose();
                publicationManager.Dispose();
            }
        }

        public bool Validate(long datasetVersionId)
        {
            throw new NotImplementedException();
        }

        private string generatePrimaryData(long datasetVersionId)
        {
            try
            {
                DatasetManager datasetManager = new DatasetManager();
                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);


                if (datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                {
                    OutputDataManager outputDataManager = new OutputDataManager();
                    SubmissionManager submissionManager = new SubmissionManager();

                    long datasetId = datasetVersion.Dataset.Id;

                    string fileName = submissionManager.GetFileNameForDataRepo(datasetId, datasetVersionId,
                        _dataRepo.Name,
                        "csv");

                    string filepath = outputDataManager.GenerateAsciiFile(
                        datasetId,
                        datasetVersionId,
                        fileName,
                        "text/txt");

                    return filepath;
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

        public PangaeaDataRepoConverter(Repository datarepo)
        {
            _dataRepo = datarepo;
            _broker = datarepo.Broker;
        }
    }


    public class PanageaMetadata
    {
        public List<string> AuthorIDs { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }
        public List<ReferenceID> ReferenceIDs { get; set; }
        public string ExportFilename { get; set; }
        public string EventLabel { get; set; }
        public List<VariableElement> ParameterIDs { get; set; }
        public List<string> ProjectIDs { get; set; }
        public PangaeaMetadataTopologicType TopologicTypeID { get; set; }
        public PangaeaMetadataStatus StatusID { get; set; }
        public int LoginID { get; set; }

        public PanageaMetadata()
        {
            AuthorIDs = new List<string>();
            Title = "";
            Abstract = "";
            ReferenceIDs = new List<ReferenceID>();
            ExportFilename = "";
            EventLabel = "";
            ParameterIDs = new List<VariableElement>();
            ProjectIDs = new List<string>();
            TopologicTypeID = PangaeaMetadataTopologicType.notGiven;
            StatusID = PangaeaMetadataStatus.NotValidated;
            LoginID = 0;
        }
    }

    public class ReferenceID
    {
        public long ID { get; set; }
        public long IDRelationTypeID { get; set; }
    }

    public enum PangaeaMetadataTopologicType
    {
        notGiven,
        notSpecified
    }

    public enum PangaeaMetadataStatus
    {
        NotValidated,
        Validated,
        Published
    }
}
