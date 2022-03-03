using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        [ResponseType(typeof(ApiDatasetModel))]
        public HttpResponseMessage Get(long id)
        {
            if (id <= 0) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "No valid dataset id.");

            DatasetManager datasetManager = new DatasetManager();
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            try
            {
                Dataset dataset = datasetManager.GetDataset(id);
                if (dataset == null) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "This Dataset not exist");

                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                if (datasetVersion == null) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "It is not possible to load the latest version.");

                int versionNumber = dataset.Versions.Count;

                ApiDatasetModel datasetModel = new ApiDatasetModel()
                {
                    Id = id,
                    Version = versionNumber,
                    VersionId = datasetVersion.Id,
                    Title = datasetVersion.Title,
                    Description = datasetVersion.Description,
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

                var response = Request.CreateResponse(HttpStatusCode.OK);
                string resp = JsonConvert.SerializeObject(datasetModel);

                response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return response;
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
        [ResponseType(typeof(ApiDatasetModel))]
        public HttpResponseMessage Get(long id, int version)
        {
            if (id <= 0) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "No valid dataset id.");
            if (version <= 0) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "No valid version.");


            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            try
            {
                using (DatasetManager datasetManager = new DatasetManager())
                using (PartyManager partyManager = new PartyManager())
                {
                    Dataset dataset = datasetManager.GetDataset(id);
                    if (dataset == null) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "This Dataset not exist");

                    int index = version - 1;

                    if (version > dataset.Versions.Count) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "This version does not exit.");

                    DatasetVersion datasetVersion = dataset.Versions.OrderBy(d => d.Timestamp).ElementAt(index);
                    if (datasetVersion == null) return Request.CreateResponse(HttpStatusCode.InternalServerError, "It is not possible to load the latest version.");

                    ApiDatasetModel datasetModel = new ApiDatasetModel()
                    {
                        Id = id,
                        Version = version,
                        VersionId = datasetVersion.Id,
                        Title = datasetVersion.Title,
                        Description = datasetVersion.Description,
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

                    //get Author as xelement list
                    var authors = MappingUtils.GetXElementFromMetadata(Convert.ToInt64(Key.Author), LinkElementType.Key,
                       datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));

                    foreach (XElement author in authors)
                    {
                        long partyid = getPartyId(author);
                        // id direct 
                        if (partyid >0)
                        {
                          
                            var party = partyManager.GetParty(partyid);

                            if (party != null)
                            {
                                var attr = party.CustomAttributeValues.Where(a => a.CustomAttribute.IsMain = true).ToArray();

                                datasetModel.Citators.Add(new Citator() { FirstName = attr[0].Value, LastName = attr[1].Value });
                            }
                        }
                    }


                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    string resp = JsonConvert.SerializeObject(datasetModel);

                    response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    return response;
                } 
            }
            finally
            {
                
            }
        }

        private long getPartyId(XElement e)
        {
            if (e.Attributes().Any(x => x.Name.LocalName.Equals("partyid")))
            {
                return Convert.ToInt64(e.Attribute("partyid").Value);
            }
            if (e.Parent != null) return getPartyId(e.Parent);

            return 0;
        }
            
    }
}