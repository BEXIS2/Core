using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Output;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Xml;

namespace BExIS.Modules.Dim.UI.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request metadata of a datasets and get the result in XML.
    /// </summary>
    public class MetadataOutController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        // GET: api/Metadata
        /// <summary>
        /// Get list of exiting datasets from which metadata can be loaded.
        /// </summary>
        /// <remarks>
        /// With the Get function you get an overview of the exiting datasets from which you can load metadata.
        /// The format indicates the possible conversions. Without format the system internal metadata xml document is loaded.
        /// </remarks>
        /// <returns>List of MetadataViewObject</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Metadata")]
        public IEnumerable<MetadataViewObject> Get()
        {
            List<MetadataViewObject> tmp = new List<MetadataViewObject>();
            using (var dm = new DatasetManager())
            {
                var datasetIds = dm.GetDatasetIds();

                foreach (var id in datasetIds)
                {
                    // add only datasets to the list where the status is checked in otherwise
                    // the system is not able to load metadat data informations
                    if (dm.IsDatasetCheckedIn(id))
                    {
                        var dataset = dm.GetDataset(id);

                        MetadataViewObject mvo = new MetadataViewObject();
                        mvo.DatasetId = id;

                        // load all metadata export options
                        // in the metadata extra field there are stored the import and export mapping files
                        // this funktion loads all transformations based on one direction
                        // AttributeNames.name means the destination metadata name
                        List<string> t = xmlDatasetHelper.GetAllTransmissionInformation(id, TransmissionType.mappingFileExport, AttributeNames.name).ToList();

                        //mvo.Format = t.ToArray();
                        mvo.SubsetType = getAllAvailableConcepts(dataset.MetadataStructure.Id);

                        tmp.Add(mvo);
                    }
                }
            }

            return tmp;
        }

        // GET:api/MetadataBySchema/GBIF
        /// <summary>
        /// Get all metadata of all datasets based on the given metadata schema name as XML
        /// </summary>
        /// <returns>XML with all metadata of the metadata schema</returns>
        [BExISApiAuthorize]
        [GetRoute("api/MetadataBySchema/{name}")]
        public HttpResponseMessage GetBySchema(string name)
        {
            using (DatasetManager dm = new DatasetManager())
            {
                var datasetIds = dm.GetDatasetIds();

                List<MetadataViewObject> tmp = new List<MetadataViewObject>();
                // create final XML document
                XmlDocument newXmlDoc = new XmlDocument();

                // create root element
                XmlElement elem = newXmlDoc.CreateElement("root");

                foreach (var id in datasetIds)
                {
                    MetadataViewObject mvo = new MetadataViewObject();
                    mvo.DatasetId = id;

                    // get metadata schema name
                    List<string> t = xmlDatasetHelper.GetAllTransmissionInformation(id, TransmissionType.mappingFileExport, AttributeNames.name).ToList();
                    string format = t.ToArray().FirstOrDefault();

                    // filter by metadata schema name
                    if (format == name)
                    {
                        // get latest version of dataset
                        DatasetVersion dsv = dm.GetDatasetLatestVersion(id);

                        // get metadata content
                        XmlDocument xmldoc = dsv.Metadata;
                        XmlElement element = xmldoc.DocumentElement;

                        // cerate root element for the dataset
                        XmlElement elemDataset = newXmlDoc.CreateElement("Dataset");

                        // add id attribute to root element (<Dataset id="12"></Datatset>)
                        XmlAttribute attr = newXmlDoc.CreateAttribute("id");
                        attr.Value = id.ToString();
                        elemDataset.SetAttributeNode(attr);

                        // append metadata to dataset element
                        elemDataset.AppendChild(newXmlDoc.ImportNode(element, true));

                        // append dataset element to root
                        elem.AppendChild(elemDataset);
                    }
                }
                // add root element to xml document
                newXmlDoc.AppendChild(elem);

                // return xml document as XML
                HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(newXmlDoc.InnerXml, Encoding.UTF8, "application/xml") };
                return response;
            }
        }

        /// <summary>
        /// Get metadata for a dataset as XML (default) or JSON (internal, complete or simplified structure)
        /// </summary>
        /// <remarks>
        ///
        /// ## format
        /// Based on the existing transformation options, the converted metadata can be obtained via format.
        ///
        /// ## simplfiedJson
        /// if you set the accept of the request to return a json, you can manipulate the json with this parameter. <br/>
        /// 0 = returns the metadata with full internal structure <br/>
        /// 1 = returns a simplified form of the structure with all fields and attributes <br/>
        /// 2 = returns the metadata in a simplified structure and does not add all fields and attributes that are empty.
        ///
        /// </remarks>
        /// <param name="id">Dataset Id</param>
        /// <param name="format">Internal,External,Subset</param>
        /// <param name="subsetType">Based on the existing concept mappings, the converted metadata can be obtained via subsetType.</param>
        /// <param name="simplifiedJson">accept 0,1,2</param>
        /// <returns>metadata as xml or json</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Metadata/{id}")]
        public HttpResponseMessage Get(int id, [FromUri] Format format = Format.Internal, [FromUri] string subsetType = null, [FromUri] int simplifiedJson = 0)
        {
            return GetMetadata(id, -1, format, subsetType, simplifiedJson);
        }

        /// <summary>
        /// Get metadata for a dataset as XML (default) or JSON (internal, complete or simplified structure)
        /// </summary>
        /// <remarks>
        ///
        /// ## format
        /// Based on the existing transformation options, the converted metadata can be obtained via format.
        ///
        /// ## simplfiedJson
        /// if you set the accept of the request to return a json, you can manipulate the json with this parameter. <br/>
        /// 0 = returns the metadata with full internal structure <br/>
        /// 1 = returns a simplified form of the structure with all fields and attributes <br/>
        /// 2 = returns the metadata in a simplified structure and does not add all fields and attributes that are empty.
        ///
        /// </remarks>
        /// <param name="id">Dataset Id</param>
        /// <param name="versionId">Version Id</param>
        /// <param name="format">Internal,External,Subset</param>
        /// <param name="subsetType">Based on the existing concept mappings, the converted metadata can be obtained via subsetType.</param>
        /// <param name="simplifiedJson">accept 0,1,2</param>
        /// <returns>metadata as xml or json</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Metadata/{id}/{versionId}")]
        public HttpResponseMessage Get(long id, long? versionId = -1, [FromUri] Format format = Format.Internal, [FromUri] string subsetType = null, [FromUri] int simplifiedJson = 0)
        {
            long vId = -1;
            if (versionId != null) vId = Convert.ToInt64(versionId);

            return GetMetadata(id, vId, format, subsetType, simplifiedJson);
        }

        /// <summary>
        /// Get metadata for a dataset as XML (default) or JSON (internal, complete or simplified structure)
        /// </summary>
        /// <remarks>
        ///
        /// ## format
        /// Based on the existing transformation options, the converted metadata can be obtained via format.
        ///
        /// ## simplfiedJson
        /// if you set the accept of the request to return a json, you can manipulate the json with this parameter. <br/>
        /// 0 = returns the metadata with full internal structure <br/>
        /// 1 = returns a simplified form of the structure with all fields and attributes <br/>
        /// 2 = returns the metadata in a simplified structure and does not add all fields and attributes that are empty.
        ///
        /// </remarks>
        /// <param name="id">Dataset Id</param>
        /// <param name="version_number">Version number</param>
        /// <param name="format">Internal,External,Subset</param>
        /// <param name="subsetType">Based on the existing concept mappings, the converted metadata can be obtained via subsetType.</param>
        /// <param name="simplifiedJson">accept 0,1,2</param>
        /// <returns>metadata as xml or json</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Metadata/{id}/version_number/{version_number}")]
        public HttpResponseMessage Get(long id, int version_number, [FromUri] Format format = Format.Internal, [FromUri] string subsetType = null, [FromUri] int simplifiedJson = 0)
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

                return GetMetadata(id, datasetVersion.Id, format, subsetType, simplifiedJson);
            }
        }

        /// <summary>
        /// Get metadata for a dataset as XML (default) or JSON (internal, complete or simplified structure)
        /// </summary>
        /// <remarks>
        ///
        /// ## format
        /// Based on the existing transformation options, the converted metadata can be obtained via format.
        ///
        /// ## simplfiedJson
        /// if you set the accept of the request to return a json, you can manipulate the json with this parameter. <br/>
        /// 0 = returns the metadata with full internal structure <br/>
        /// 1 = returns a simplified form of the structure with all fields and attributes <br/>
        /// 2 = returns the metadata in a simplified structure and does not add all fields and attributes that are empty.
        ///
        /// </remarks>
        /// <param name="id">Dataset Id</param>
        /// <param name="version_name">Version name</param>
        /// <param name="format">Internal,External,Subset</param>
        /// <param name="subsetType">Based on the existing concept mappings, the converted metadata can be obtained via subsetType.</param>
        /// <param name="simplifiedJson">accept 0,1,2</param>
        /// <returns>metadata as xml or json</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Metadata/{id}/version_name/{version_name}")]
        public HttpResponseMessage Get(long id, string version_name, [FromUri] Format format = Format.Internal, [FromUri] string subsetType = null, [FromUri] int simplifiedJson = 0)
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

                return GetMetadata(id, versionId, format, subsetType, simplifiedJson);
            }
        }

        /// <summary>
        /// Get metadata for a dataset as XML (default) or JSON (internal, complete or simplified structure)
        /// </summary>
        /// <remarks>
        ///
        /// ## format
        /// Based on the existing transformation options, the converted metadata can be obtained via format.
        ///
        /// ## simplfiedJson
        /// if you set the accept of the request to return a json, you can manipulate the json with this parameter. <br/>
        /// 0 = returns the metadata with full internal structure <br/>
        /// 1 = returns a simplified form of the structure with all fields and attributes <br/>
        /// 2 = returns the metadata in a simplified structure and does not add all fields and attributes that are empty.
        ///
        /// </remarks>
        /// <param name="id">Dataset Id</param>
        /// <param name="tag">Tag Nr</param>
        /// <param name="format">Internal,External,Subset</param>
        /// <param name="subsetType">Based on the existing concept mappings, the converted metadata can be obtained via subsetType.</param>
        /// <param name="simplifiedJson">accept 0,1,2</param>
        /// <returns>metadata as xml or json</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Metadata/{id}/tag/{tag}")]
        public HttpResponseMessage Get(long id, double tag, [FromUri] Format format = Format.Internal, [FromUri] string subsetType = null, [FromUri] int simplifiedJson = 0)
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

                return GetMetadata(id, versionId, format, subsetType, simplifiedJson);
            }
        }

        private HttpResponseMessage GetMetadata(long id, long versionId, Format format, string subsetType, int simplifiedJson)
        {
            DatasetVersion datasetVersion = null;

            string returnType = "";
            //returnType = Request.Content.Headers.ContentType?.MediaType;
            if (Request.Headers.Accept.Any())
                returnType = Request.Headers.Accept.First().MediaType;

            if (format == Format.Subset && simplifiedJson > 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "The output with format.subset 2 only works with simplifiedJson = 0");

            using (DatasetManager dm = new DatasetManager())
            using (EntityManager entityManager = new EntityManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (UserManager userManager = new UserManager())
            {
                if (id == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Dataset id should be greater then 0.");

                // try to get latest dataset version
                if (versionId == -1)
                {
                    datasetVersion = dm.GetDatasetLatestVersion(id);
                }

                // try to get dataset version by version number
                else
                {
                    Dataset dataset = dm.GetDataset(id);
                    try
                    {
                        // check version belongs to dataset
                        if (!dataset.Versions.Select(v => v.Id).Contains(versionId)) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "this version id is not part of the dataset " + id);

                        datasetVersion = dm.GetDatasetVersion(versionId);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                // Check if a dataset version was set
                if (datasetVersion == null) return Request.CreateResponse(HttpStatusCode.InternalServerError, "It is not possible to load the latest or given version.");

                //entity permissions
                if (id > 0)
                {
                    Dataset dataset = dm.GetDataset(id);

                    // Check if dataset is public
                    long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                    entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;
                    bool isPublic = false;
                    isPublic = entityPermissionManager.ExistsAsync(entityTypeId.Value, id).Result;

                    // If dataset is not public check if a valid token is provided
                    if (isPublic == false)
                    {
                        User user = null;
                        user = ControllerContext.RouteData.Values["user"] as User;

                        // If user is registered pass
                        if (user == null)
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "The dataset is not public and the token is not valid.");
                        }
                        //else
                        //{
                        //    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "The dataset is not public and a token is not provided.");
                        //}
                    }

                    if (dataset == null)
                        return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "The dataset with the id (" + id + ") does not exist.");
                }

                XmlDocument xmlDoc = datasetVersion.Metadata;

                //format = intern
                // do not transformation

                //format = extern
                if (format == Format.External)
                {
                    List<string> t = xmlDatasetHelper.GetAllTransmissionInformation(id, TransmissionType.mappingFileExport, AttributeNames.name).ToList();
                    string convertTo = t.ToArray().FirstOrDefault();
                    xmlDoc = OutputMetadataManager.GetConvertedMetadata(id, TransmissionType.mappingFileExport, convertTo);
                }

                //format subset && subsetType
                if (format == Format.Subset)
                {
                    if (string.IsNullOrEmpty(subsetType))
                        return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "In combination with the format subset - subsettype must not be empty");

                    using (var conceptManager = new ConceptManager())
                    {
                        var concept = conceptManager.MappingConceptRepository.Get().Where(c => c.Name.Equals(subsetType)).FirstOrDefault();

                        if (concept == null)
                            return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "In combination with the format subset - subsettype must not be empty");

                        long mdId = datasetVersion.Dataset.MetadataStructure.Id;

                        xmlDoc = MappingUtils.GetConceptOutput(mdId, concept.Id, xmlDoc);
                    }
                }

                try
                {
                    if (xmlDoc == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("No mapping found for this format : {0} .", format));

                    switch (returnType)
                    {
                        case "application/json":
                            {
                                string json = "";

                                switch (simplifiedJson)
                                {
                                    case 0:
                                        {
                                            json = JsonConvert.SerializeObject(xmlDoc.DocumentElement);
                                            break;
                                        }
                                    case 1:
                                        {
                                            XmlMetadataConverter xmlMetadataConverter = new XmlMetadataConverter();
                                            json = xmlMetadataConverter.ConvertTo(xmlDoc, true).ToString();

                                            break;
                                        }
                                    case 2:
                                        {
                                            XmlMetadataConverter xmlMetadataConverter = new XmlMetadataConverter();
                                            json = xmlMetadataConverter.ConvertTo(xmlDoc).ToString();

                                            break;
                                        }
                                }

                                HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(json, Encoding.UTF8, "application/json") };
                                return response;
                            }
                        case "application/xml":
                            {
                                HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(xmlDoc.InnerXml, Encoding.UTF8, "application/xml") };
                                return response;
                            }
                        default:
                            {
                                HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(xmlDoc.InnerXml, Encoding.UTF8, "application/xml") };
                                return response;
                            }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        private string[] getAllAvailableConcepts(long id)
        {
            using (var conceptManager = new ConceptManager())
            using (var mappingManager = new MappingManager())
            {
                string[] concepts = { };
                // get all root mappings to mds with id wich mapped to a mapping concept
                var allMappings = mappingManager.GetMappings()
                    .Where(m =>
                    m.Source.ElementId.Equals(id) &&
                    m.Source.Type.Equals(LinkElementType.MetadataStructure) &&
                    m.Target.Type.Equals(LinkElementType.MappingConcept));

                if (allMappings.Any())
                {
                    concepts = allMappings.Select(m => m.Target.Name).ToArray();
                }

                return concepts;
            }
        }
    }

    public class MetadataViewObject
    {
        public long DatasetId { get; set; }

        //public Format Format { get; set; }
        public string[] SubsetType { get; set; }
    }

    public enum Format
    {
        Internal,
        External,
        Subset
    }
}