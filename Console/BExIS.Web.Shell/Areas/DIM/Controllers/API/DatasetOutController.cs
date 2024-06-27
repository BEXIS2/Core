using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
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
        /// Get dataset informations of the latest version of a dataset by id.
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

                // get content
                ApiDatasetModel datasetModel = getContent(datasetVersion, id, versionNumber, dataset.MetadataStructure.Id, dataset.DataStructure.Id);

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
        /// Get dataset informations of a specific version of a dataset by id and version id.
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
        /// Get dataset informations of a specific version of a dataset by id and version number.
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
        /// Get dataset informations of a specific version of a dataset by id and version name.
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

                    // get content
                    ApiDatasetModel datasetModel = getContent(datasetVersion, id, version, metadataStructureId, dataset.DataStructure.Id);

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

        private ApiDatasetModel getContent(DatasetVersion datasetVersion, long id, long versionNumber, long metadataStructureId, long dataStructureId)
        {
            ApiDatasetModel datasetModel = new ApiDatasetModel()
            {
                Id = id,
                Version = versionNumber,
                VersionId = datasetVersion.Id,
                VersionName = datasetVersion.VersionName,
                VersionDate = datasetVersion.Timestamp.ToString(new CultureInfo("en-US")),
                VersionPublicAccess = datasetVersion.PublicAccess,
                VersionPublicAccessDate = datasetVersion.PublicAccessDate.ToString(new CultureInfo("en-US")),
                Title = datasetVersion.Title,
                Description = datasetVersion.Description,
                DataStructureId = dataStructureId,
                MetadataStructureId = metadataStructureId
            };

            Dictionary<string, List<XElement>> elements = new Dictionary<string, List<XElement>>();

            // add addtional Informations / mapped system keys
            foreach (Key k in Enum.GetValues(typeof(Key)))
            {
                var tmp = MappingUtils.GetValuesFromMetadata(Convert.ToInt64(k), LinkElementType.Key,
               metadataStructureId, XmlUtility.ToXDocument(datasetVersion.Metadata));

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

            // set up person key list
            var personKeyList = new List<string>();
            // setup dic with values of the persons
            var personKeyDic = new Dictionary<string, List<XElement>>();

            // add keys here
            personKeyList.Add(Key.Author.ToString());

            foreach (var key in personKeyList)
            {
                // check if key exists in alle mapped elements
                if (elements.ContainsKey(key))
                    personKeyDic.Add(key, elements[key]);
            }

            datasetModel.Names = getSplitedNames(personKeyDic);

            var publicAndDate = getPublicAndDate(id);
            // check is public
            datasetModel.IsPublic = publicAndDate.Item1;

            // check for publication date
            datasetModel.PublicationDate = publicAndDate.Item2.ToString(new CultureInfo("en-US"));

            return datasetModel;
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

        private Dictionary<string, Dictionary<string, string>> getSplitedNames(Dictionary<string, List<XElement>> elements)
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
                        Console.WriteLine(partyid);
                        if (partyid > 0) { }
                        else
                        {
                            var person = new HumanName(element.Value);
                            var GivenName = (person.Middle.Length > 0) ? $"{person.First} {person.Middle}" : $"{person.First}";
                            var FamilyName = person.Last;
                            dict2.Add("GivenName", GivenName);
                            dict2.Add("FamilyName", FamilyName);

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