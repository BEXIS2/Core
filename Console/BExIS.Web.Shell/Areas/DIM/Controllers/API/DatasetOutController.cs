using BExIS.App.Bootstrap.Attributes;
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
        /// <returns>IEnumerable with datatype long</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Dataset")]
        public IEnumerable<long> Get()
        {
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                IEnumerable<long> datasetIds = datasetManager.DatasetRepo.Get().Select(d => d.Id);

                return datasetIds;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        // GET api/DatasetOut/{id}
        /// <summary>
        /// This function returns basic information from a data set. An identifier is required.
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
                string title = xmlDatasetHelper.GetInformation(dataset, NameAttributeValues.title);
                string description = xmlDatasetHelper.GetInformation(dataset, NameAttributeValues.description);
                ApiDatasetModel datasetModel = new ApiDatasetModel()
                {
                    Id = id,
                    Title = title,
                    Description = description,
                    DataStructureId = dataset.DataStructure.Id,
                    MetadataStructureId = dataset.MetadataStructure.Id
                };

                return datasetModel;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }
    }
}