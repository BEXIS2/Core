using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BExIS.Modules.Dim.UI.Controllers.API
{
    /// <summary>
    /// This class is designed as a Web API to access data sets externally.
    /// The provided HTTPGET functions provide an overview of all data records and basic information about a selected data record.
    ///
    /// The information about a dataset shows the title, description, data structure, and metadata structure.
    /// </summary>
    /// <remarks>
    /// This class is designed as a Web API to access data sets externally.
    /// The provided HTTPGET functions provide an overview of all data records and basic information about a selected data record.
    ///
    /// The information about a dataset shows the title, description, data structure, and metadata structure.
    /// </remarks>
    public class DatasetOutController : ApiController
    {
        // GET api/DatasetOut
        /// <summary>
        /// This function displays a list of all datasets Id´s in the system.
        /// </summary>
        /// <returns>IEnumerable with ApiSimpleDatasetModel</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Dataset")]
        public IEnumerable<ApiSimpleDatasetModel> Get()
        {
            DatasetManager datasetManager = new DatasetManager();
            List<ApiSimpleDatasetModel> datasetModels = new List<ApiSimpleDatasetModel>();

            try
            {
                IEnumerable<long> datasetIds = datasetManager.DatasetRepo.Get().Select(d => d.Id);

                foreach (long id in datasetIds)
                {
                    Dataset tmpDataset = datasetManager.GetDataset(id);
                    ApiSimpleDatasetModel datasetModel = new ApiSimpleDatasetModel();
                    datasetModel.Id = tmpDataset.Id;

                    DatasetVersion[] tmpVersions = tmpDataset.Versions.OrderBy(ds => ds.Timestamp).ToArray();

                    for (int i = 0; i < tmpVersions.Length; i++)
                    {
                        DatasetVersion dsv = tmpVersions.ElementAt(i);
                        ApiSimpleDatasetVersionModel datasetVersionModel = new ApiSimpleDatasetVersionModel()
                        {
                            Id = dsv.Id,
                            Number = i + 1
                        };

                        datasetModel.Versions.Add(datasetVersionModel);
                    }

                    datasetModels.Add(datasetModel);
                }

                return datasetModels;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        // GET api/DatasetOut/{id}
        /// <summary>
        /// This function returns basic information from the latest version of a data set. An identifier is required.
        /// </summary>
        ///
        /// <param name="id">Identifier of a dataset</param>
        /// <returns>
        ///{
        /// "Title":"Title of my Dataset.",
        /// "Description":"Description of my Dataset.",
        /// "DataStructureId":1,
        /// "MetadataStructureId":1,
        ///}
        /// </returns>
        [BExISApiAuthorize]
        [GetRoute("api/Dataset/{id}")]
        public ApiDatasetModel Get(long id)
        {
            if (id <= 0)
            {
                ApiDatasetModel datasetModel = new ApiDatasetModel();
            }

            DatasetManager datasetManager = new DatasetManager();
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            try
            {
                Dataset dataset = datasetManager.GetDataset(id);
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);

                int versionNumber = dataset.Versions.Count;

                xmlDatasetHelper.GetInformation(dataset, NameAttributeValues.title);
                string title = xmlDatasetHelper.GetInformation(dataset, NameAttributeValues.title);
                string description = xmlDatasetHelper.GetInformation(dataset, NameAttributeValues.title);

                ApiDatasetModel datasetModel = new ApiDatasetModel()
                {
                    Id = id,
                    Version = versionNumber,
                    VersionId = datasetVersion.Id,
                    Title = title,
                    Description = description,
                    DataStructureId = dataset.DataStructure.Id,
                    MetadataStructureId = dataset.MetadataStructure.Id
                };

                // add addtional Iformations
                foreach (Key k in Enum.GetValues(typeof(Key)))
                {
                    var tmp = MappingUtils.GetValuesFromMetadata(Convert.ToInt64(k), LinkElementType.Key,
                   datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));

                    if (tmp != null)
                    {
                        string value = string.Join(",", tmp.Distinct());
                        if (!string.IsNullOrEmpty(value))
                        {
                            datasetModel.AdditionalInformations.Add(k.ToString(), value);
                        }
                    }
                }

                return datasetModel;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        // GET api/DatasetOut/{id}/{version}
        /// <summary>
        /// This function returns basic information from a version of a data set. An identifier and version is required.
        /// </summary>
        ///
        /// <param name="id">Identifier of a dataset</param>
        /// <param name="version">Version of a dataset</param>
        /// <returns>
        ///{
        /// "Title":"Title of my Dataset.",
        /// "Description":"Description of my Dataset.",
        /// "DataStructureId":1,
        /// "MetadataStructureId":1,
        ///}
        /// </returns>
        [BExISApiAuthorize]
        [GetRoute("api/Dataset/{id}/{version}")]
        public ApiDatasetModel Get(long id, int version)
        {
            if (id <= 0)
            {
                ApiDatasetModel datasetModel = new ApiDatasetModel();
            }

            DatasetManager datasetManager = new DatasetManager();
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            try
            {
                Dataset dataset = datasetManager.GetDataset(id);
                int index = version - 1;
                DatasetVersion datasetVersion = dataset.Versions.OrderBy(d => d.Timestamp).ElementAt(index);

                xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);
                string title = xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);
                string description = xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);

                ApiDatasetModel datasetModel = new ApiDatasetModel()
                {
                    Id = id,
                    Version = version,
                    VersionId = datasetVersion.Id,
                    Title = title,
                    Description = description,
                    DataStructureId = dataset.DataStructure.Id,
                    MetadataStructureId = dataset.MetadataStructure.Id
                };

                // add addtional Iformations
                foreach (Key k in Enum.GetValues(typeof(Key)))
                {
                    var tmp = MappingUtils.GetValuesFromMetadata(Convert.ToInt64(k), LinkElementType.Key,
                   datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));

                    if (tmp != null)
                    {
                        string value = string.Join(",", tmp.Distinct());
                        if (!string.IsNullOrEmpty(value))
                        {
                            datasetModel.AdditionalInformations.Add(k.ToString(), value);
                        }
                    }
                }

                return datasetModel;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }
    }
}