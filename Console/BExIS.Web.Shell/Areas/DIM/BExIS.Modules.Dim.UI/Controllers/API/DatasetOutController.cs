using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Dim.UI.Helpers;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Utils.Data.Helpers;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using NameParser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.Linq;

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
        // GET api/Dataset
        /// <summary>
        /// Get a list of all datasets Id´s in the system.
        /// </summary>
        [BExISApiAuthorize]
        [GetRoute("api/Dataset")]
        [ResponseType(typeof(ApiSimpleDatasetModel))]
        public IEnumerable<ApiSimpleDatasetModel> Get()
        {
            List<ApiSimpleDatasetModel> datasetModels = new List<ApiSimpleDatasetModel>();

            using (DatasetManager datasetManager = new DatasetManager())
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
                        if(dsv.Tag != null) datasetVersionModel.Tag = dsv.Tag.Nr;


                        datasetModel.Versions.Add(datasetVersionModel);
                        datasetModel.Title = dsv.Title;
                    }
                    datasetModels.Add(datasetModel);
                }

                return datasetModels;
            }
        }

        // GET api/Dataset/{id}
        /// <summary>
        /// Get dataset information of the latest version of a dataset by id.
        /// </summary>
        ///
        /// <param name="id">Identifier of a dataset</param>
        [BExISApiAuthorize]
        [GetRoute("api/Dataset/{id}")]
        [ResponseType(typeof(ApiDatasetModel))]
        public HttpResponseMessage Get(long id)
        {
            // Check parameter
            if (id <= 0) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "No valid dataset id.");

            using (DatasetManager datasetManager = new DatasetManager())
            {
                // try to get dataset by id
                Dataset dataset = datasetManager.GetDataset(id);
                if (dataset == null) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "This Dataset not exist");

                // get latest dataset version
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                if (datasetVersion == null) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "It is not possible to load the latest version.");

                // get the latest version number
                int versionNumber = dataset.Versions.Count;

                long dataStructureId = 0;
                if (dataset.DataStructure != null) dataStructureId = dataset.DataStructure.Id;

                ApiDatasetHelper apiDatasetHelper = new ApiDatasetHelper();
                // get content
                ApiDatasetModel datasetModel = apiDatasetHelper.GetContent(datasetVersion, id, versionNumber, dataset.MetadataStructure.Id, dataStructureId);

                // get links
                EntityReferenceHelper entityReferenceHelper = new EntityReferenceHelper();
                datasetModel.Links.From = entityReferenceHelper.GetSourceReferences(dataset.Id, dataset.EntityTemplate.EntityType.Id, versionNumber);
                datasetModel.Links.To = entityReferenceHelper.GetTargetReferences(dataset.Id, dataset.EntityTemplate.EntityType.Id, versionNumber);

                // create response and return as JSON
                var response = Request.CreateResponse(HttpStatusCode.OK);
                string resp = JsonConvert.SerializeObject(datasetModel);

                response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return response;
            }
        }

        // GET api/DatasetOut/{id}/{version}
        /// <summary>
        /// Get dataset information of a specific version of a dataset by id and version id.
        /// </summary>
        ///
        /// <param name="id">Identifier of a dataset</param>
        /// <param name="versionId">Version Id of a dataset</param>
        [BExISApiAuthorize]
        [GetRoute("api/Dataset/{id}/{versionId}")]
        [ResponseType(typeof(ApiDatasetModel))]
        public HttpResponseMessage Get(long id, long versionId)
        {
            return get(id, versionId);
        }

        // GET api/DatasetOut/{id}/{version}
        /// <summary>
        /// Get dataset information of a specific version of a dataset by id and version number.
        /// </summary>
        ///
        /// <param name="id">Identifier of a dataset</param>
        /// <param name="version_number">Version of a dataset</param>
        [BExISApiAuthorize]
        [GetRoute("api/Dataset/{id}/version_number/{version_number}")]
        [ResponseType(typeof(ApiDatasetModel))]
        public HttpResponseMessage Get(long id, int version_number)
        {
            if (id <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Id should be greater then 0");

            if (version_number <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Version should be greater then 0");

            using (DatasetManager dm = new DatasetManager())
            {
                int index = version_number - 1;
                Dataset dataset = dm.GetDataset(id);

                int versions = dataset.Versions.Count;

                if (versions < version_number)
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "This version number does not exist for this dataset");

                var datasetVersion = dataset.Versions.OrderBy(d => d.Timestamp).ElementAt(index);

                return get(id, datasetVersion.Id);
            }
        }

        // GET api/DatasetOut/{id}/{version}
        /// <summary>
        /// Get dataset information of a specific version of a dataset by id and version name.
        /// </summary>
        ///
        /// <param name="id">Identifier of a dataset</param>
        /// <param name="version_name">Version name of a dataset</param>
        [BExISApiAuthorize]
        [GetRoute("api/Dataset/{id}/version_name/{version_name}")]
        [ResponseType(typeof(ApiDatasetModel))]
        public HttpResponseMessage Get(long id, string version_name)
        {
            if (id <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Id should be greater then 0");

            if (string.IsNullOrEmpty(version_name))
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Version name not exist");

            using (DatasetManager dm = new DatasetManager())
            {
                var versionId = dm.GetDatasetVersions(id).Where(d => d.VersionName == version_name).Select(d => d.Id).FirstOrDefault();

                if (versionId <= 0)
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "This version name does not exist for this dataset");

                return get(id, versionId);
            }
        }

        // GET api/DatasetOut/{id}/{version}
        /// <summary>
        /// Get dataset informations of a specific version of a dataset by id and tag.
        /// </summary>
        ///
        /// <param name="id">Identifier of a dataset</param>
        /// <param name="tag">tag number of a dataset</param>
        [BExISApiAuthorize]
        [GetRoute("api/Dataset/{id}/tag/{tag}")]
        [ResponseType(typeof(ApiDatasetModel))]
        public HttpResponseMessage Get(long id, double tag)
        {
            if (id <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Id should be greater then 0");

            if (tag<=0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Tag not exist");

            using (DatasetManager dm = new DatasetManager())
            {
                var versionId = dm.GetLatestVersionIdByTagNr(id, tag);

                if (versionId <= 0)
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "This tag does not exist for this dataset");

                return get(id, versionId);
            }
        }

        private HttpResponseMessage get(long id, long versionId)
        {
            try
            {
                // Check parameter
                if (id <= 0) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "No valid dataset id.");
                if (versionId <= 0) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "No valid version id.");

                using (DatasetManager datasetManager = new DatasetManager())
                {
                    // try to get dataset by id
                    Dataset dataset = datasetManager.GetDataset(id);
                    if (dataset == null) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "This Dataset not exist");

                    // get version number
                    int version = datasetManager.GetDatasetVersionNr(versionId);

                    // check version belongs to dataset
                    if (!dataset.Versions.Select(v => v.Id).Contains(versionId)) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "this version id is not part of the dataset " + id);

                    // check version number
                    if (version > dataset.Versions.Count) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "This version does not exit.");

                    // try to get dataset version
                    DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(versionId);//dataset.Versions.OrderBy(d => d.Timestamp).ElementAt(index);
                    if (datasetVersion == null) return Request.CreateResponse(HttpStatusCode.InternalServerError, "It is not possible to load the latest version.");

                    // get metadata structure id from XML as it could differ from the current metadata structure ID stored at dataset level
                    var metadata = datasetVersion.Metadata;
                    long metadataStructureId = -1;
                    if (metadata.DocumentElement.Attributes["id"] != null)
                    {
                        metadataStructureId = (long)Convert.ToInt64(metadata.DocumentElement.Attributes["id"].Value);
                    }

                    ApiDatasetHelper apiDatasetHelper = new ApiDatasetHelper();
                    // get content
                    ApiDatasetModel datasetModel = apiDatasetHelper.GetContent(datasetVersion, id, version, metadataStructureId, dataset.DataStructure.Id);


                    EntityReferenceHelper entityReferenceHelper = new EntityReferenceHelper();
                    datasetModel.Links.From = entityReferenceHelper.GetSourceReferences(dataset.Id, dataset.EntityTemplate.EntityType.Id, version);
                    datasetModel.Links.To = entityReferenceHelper.GetTargetReferences(dataset.Id, dataset.EntityTemplate.EntityType.Id, version);

                    // create response and return as JSON
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    string resp = JsonConvert.SerializeObject(datasetModel);

                    response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    return response;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        


    }
}