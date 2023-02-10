using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using NameParser;
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
using System.Security.Cryptography;
using BExIS.Dlm.Entities.MetadataStructure;

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
        /// Get metadata of the latest version of a dataset by id.
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
        /// Get metadata of a specific version of a dataset by id and version number.
        /// </summary>
        ///
        /// <param name="id">Identifier of a dataset</param>
        /// <param name="version">Version of a dataset</param>
        [BExISApiAuthorize]
        [GetRoute("api/Dataset/{id}/{version}")]
        [ResponseType(typeof(ApiDatasetModel))]
        public HttpResponseMessage Get(long id, int version)
        {
            // Check parameter
            if (id <= 0) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "No valid dataset id.");
            if (version <= 0) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "No valid version.");


            using (DatasetManager datasetManager = new DatasetManager())
            {
                // try to get dataset by id
                Dataset dataset = datasetManager.GetDataset(id);
                if (dataset == null) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "This Dataset not exist");

                // check version number
                if (version > dataset.Versions.Count) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "This version does not exit.");
                
                // try to get dataset version 
                int index = version - 1;
                DatasetVersion datasetVersion = dataset.Versions.OrderBy(d => d.Timestamp).ElementAt(index);
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

        private ApiDatasetModel getContent(DatasetVersion datasetVersion, long id, long versionNumber, long metadataStructureId, long dataStructureId )
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