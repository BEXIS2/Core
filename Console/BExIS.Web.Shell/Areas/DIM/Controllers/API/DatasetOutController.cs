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
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System.Globalization;

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
                    VersionName = datasetVersion.VersionName,
                    VersionPublicAccess = datasetVersion.PublicAccess,
                    VersionPublicAccessDate = datasetVersion.PublicAccessDate.ToString(new CultureInfo("en-US")),
                    Title = datasetVersion.Title,
                    Description = datasetVersion.Description,
                    DataStructureId = dataset.DataStructure.Id,
                    MetadataStructureId = dataset.MetadataStructure.Id
                };

                Dictionary<string, List<XElement>> elements = new Dictionary<string, List<XElement>>();
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
                        // collect all results for each system key
                        elements.Add(k.ToString(), MappingUtils.GetXElementFromMetadata(Convert.ToInt64(k), LinkElementType.Key,
                                    datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata)));

                    }
                }

                // get isMain parts for all found parties if exists (e.g. first and last name)
                datasetModel.Parties = getPartyIsMainAttributesForParties(elements);


                var publicAndDate = getPublicAndDate(id);
                // check is public
                datasetModel.IsPublic = publicAndDate.Item1;

                // check for publication date @TODO: replace by stored value
                datasetModel.PublicationDate = publicAndDate.Item2.ToString(new CultureInfo("en-US"));


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
                        VersionName = datasetVersion.VersionName,
                        VersionPublicAccess = datasetVersion.PublicAccess,
                        VersionPublicAccessDate = datasetVersion.PublicAccessDate.ToString(new CultureInfo("en-US")),
                        Title = datasetVersion.Title,
                        Description = datasetVersion.Description,
                        DataStructureId = dataset.DataStructure.Id,
                        MetadataStructureId = dataset.MetadataStructure.Id
                    };

                    Dictionary<string, List<XElement>> elements = new Dictionary<string, List<XElement>>();
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
                            // collect all results for each system key
                            elements.Add(k.ToString(), MappingUtils.GetXElementFromMetadata(Convert.ToInt64(k), LinkElementType.Key,
                                        datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata)));

                        }
                    }

                    // get isMain parts for all found parties if exists (e.g. first and last name)
                    datasetModel.Parties = getPartyIsMainAttributesForParties(elements);

                    var publicAndDate = getPublicAndDate(id);
                    // check is public
                    datasetModel.IsPublic = publicAndDate.Item1;

                    // check for publication date @TODO: replace by stored value
                    datasetModel.PublicationDate = publicAndDate.Item2.ToString(new CultureInfo("en-US"));

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
        // Search if the XML element contains a partyid and return it
        private long getPartyId(XElement e)
        {
            if (e.Attributes().Any(x => x.Name.LocalName.Equals("partyid")))
            {
                return Convert.ToInt64(e.Attribute("partyid").Value);
            }
            if (e.Parent != null) return getPartyId(e.Parent);

            return 0;
        } 

        private Dictionary<string, Dictionary<string, string>> getPartyIsMainAttributesForParties(Dictionary<string, List<XElement>> elements)
        {
            Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();
            using (PartyManager partyManager = new PartyManager())
            {
                foreach (var key in elements)
                {
                    foreach (XElement element in key.Value)
                    {
                        long partyid = getPartyId(element);
                        Dictionary<string, string> dict2 = new Dictionary<string, string>();
                        // id direct 
                        if (partyid > 0)
                        {

                            var party = partyManager.GetParty(partyid);

                            if (party != null)
                            {
                                var attrs = party.CustomAttributeValues.Where(a => a.CustomAttribute.IsMain == true).ToArray();

                                foreach (var attr in attrs)
                                {
                                    dict2.Add(attr.CustomAttribute.Name, attr.Value);
                                }
                            }
                            if (!dict.ContainsKey(element.Value))
                            {
                                dict.Add(element.Value, dict2);
                            }

                        }
                    }
                }
               
            }

            return dict;
        }

        // @TODO: move to dataset manager?
        private static Tuple<bool, DateTime> getPublicAndDate(long id)
        {
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (EntityManager entityManager = new EntityManager())
            {
                long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;
                return entityPermissionManager.GetPublicAndDate(entityTypeId.Value, id);
            }

        }

    }
}