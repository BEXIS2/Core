using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BExIS.Modules.Dim.UI.Controllers.API
{
    public class DatasetOutController : ApiController
    {
        // GET api/<controller>
        [BExISApiAuthorize]
        //[Route("api/Dataset")]
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

        // GET api/<controller>/5
        [BExISApiAuthorize]
        //[Route("api/Dataset/{id}")]
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