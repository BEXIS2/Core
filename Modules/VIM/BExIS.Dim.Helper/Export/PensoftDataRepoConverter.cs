﻿using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Xml.Helpers;
using System;
using System.IO;
using System.Xml;

namespace BExIS.Dim.Helpers.Export
{
    public class PensoftDataRepoConverter : IDataRepoConverter
    {

        private Repository _dataRepo { get; set; }
        private Broker _broker { get; set; }


        public string Convert(long datasetVersionId)
        {

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            DatasetManager datasetManager = new DatasetManager();


            SubmissionManager submissionManager = new SubmissionManager();
            PublicationManager publicationManager = new PublicationManager();


            try
            {
                DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

                _broker = publicationManager.GetBroker(_broker.Id);
                _dataRepo = publicationManager.GetRepository(_dataRepo.Id);


                long datasetId = datasetVersion.Dataset.Id;
                string name = datasetManager.GetDatasetVersion(datasetVersionId).Dataset.MetadataStructure.Name;

                XmlDocument metadata = OutputMetadataManager.GetConvertedMetadata(datasetId,
                    TransmissionType.mappingFileExport, name, false);

                // create links to the api calls of the primary data?



                //add all to a zip


                //save document  and return filepath for download


                string path = submissionManager.GetDirectoryPath(datasetId, _broker.Name);
                string filename = submissionManager.GetFileNameForDataRepo(datasetId, datasetVersionId, _dataRepo.Name, "xml");

                string filepath = Path.Combine(path, filename);

                FileHelper.CreateDicrectoriesIfNotExist(path);

                metadata.Save(filepath);


                return filepath;
            }
            finally
            {
                datasetManager.Dispose();
                metadataStructureManager.Dispose();
                publicationManager.Dispose();
            }
        }

        public bool Validate(long datasetVersionId)
        {
            throw new NotImplementedException();
        }

        public PensoftDataRepoConverter(Repository datarepo)
        {
            _dataRepo = datarepo;
            _broker = datarepo.Broker;
        }
    }
}
