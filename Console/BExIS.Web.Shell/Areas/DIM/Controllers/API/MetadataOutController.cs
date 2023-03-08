using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Output;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Entities.Versions;
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
using System.Web.Http.Description;
using System.Xml;
using Vaiona.Persistence.Api;

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
            DatasetManager dm = new DatasetManager();

            try
            {
                var datasetIds = dm.GetDatasetIds();

                List<MetadataViewObject> tmp = new List<MetadataViewObject>();

                foreach (var id in datasetIds)
                {
                    // add only datasets to the list where the status is checked in otherwise
                    // the system is not able to load metadat data informations
                    if (dm.IsDatasetCheckedIn(id))
                    {
                        MetadataViewObject mvo = new MetadataViewObject();
                        mvo.DatasetId = id;

                        // load all metadata export options 
                        // in the metadata extra field there are stored the import and export mapping files
                        // this funktion loads all transformations based on one direction
                        // AttributeNames.name means the destination metadata name
                        List<string> t = xmlDatasetHelper.GetAllTransmissionInformation(id, TransmissionType.mappingFileExport, AttributeNames.name).ToList();

                        mvo.Format = t.ToArray();

                        tmp.Add(mvo);
                    }
                }

                return tmp;
            }
            finally
            {
                dm.Dispose();
            }
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
                    mvo.Format = t.ToArray();

                    // filter by metadata schema name
                    if (mvo.Format.FirstOrDefault() == name)
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
        /// <param name="format">Based on the existing transformation options, the converted metadata can be obtained via format.</param>
        /// <param name="simplifiedJson">accept 0,1,2</param>
        /// <returns>metadata as xml or json</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Metadata/{id}")]
        public HttpResponseMessage Get(int id, [FromUri] string format = null, [FromUri] int simplifiedJson = 0) {

            return GetMetadata(id, -1, format, simplifiedJson);
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
        /// <param name="version">Version number</param>
        /// <param name="format">Based on the existing transformation options, the converted metadata can be obtained via format.</param>
        /// <param name="simplifiedJson">accept 0,1,2</param>
        /// <returns>metadata as xml or json</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Metadata/{id}/{version}")]
        public HttpResponseMessage Get(int id, int version = -1, [FromUri] string format = null, [FromUri] int simplifiedJson = 0)
        {
            return GetMetadata(id, version, format, simplifiedJson);
        }

        private HttpResponseMessage GetMetadata(int id, int version, string format, int simplifiedJson)
        {
            
            DatasetVersion datasetVersion = null;

            string returnType = "";
            //returnType = Request.Content.Headers.ContentType?.MediaType;
            if (Request.Headers.Accept.Any())
                returnType = Request.Headers.Accept.First().MediaType;

            using (DatasetManager dm = new DatasetManager())
            using (EntityManager entityManager = new EntityManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (UserManager userManager = new UserManager())
            {
                if (id == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Dataset id should be greater then 0.");

                //entity permissions
                if (id > 0)
                {
                    Dataset dataset = dm.GetDataset(id);

                    // Check if dataset is public
                    long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                    entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;
                    bool isPublic = false;
                    isPublic = entityPermissionManager.Exists(null, entityTypeId.Value, id);

                    // If dataset is not public check if a valid token is provided
                    if (isPublic == false)
                    {
                        string token = this.Request.Headers.Authorization?.Parameter;
                        User user = null;

                        if (!String.IsNullOrEmpty(token))
                        {
                            user = userManager.Users.Where(u => u.Token.Equals(token)).FirstOrDefault();

                            // If user is registered pass
                            if (user == null)
                            { 
                                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "The dataset is not public and the token is not valid.");
                            }
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "The dataset is not public and a token is not provided.");
                        }
                    }

                    if (dataset == null)
                        return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "The dataset with the id (" + id + ") does not exist.");
                }

                string convertTo = "";

                try
                {
                    convertTo = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "format".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
                }
                catch (Exception ex) { }

                // try to get latest dataset version 
                if (version == -1)
                {
                    datasetVersion = dm.GetDatasetLatestVersion(id);
                }
                
                // try to get dataset version by version number
                else
                {
                    int index = version - 1;
                    Dataset dataset = dm.GetDataset(id);
                    try
                    {
                        datasetVersion = dataset.Versions.OrderBy(d => d.Timestamp).ElementAt(index);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                // Check if a dataset version was set
                if (datasetVersion == null) return Request.CreateResponse(HttpStatusCode.InternalServerError, "It is not possible to load the latest or given version.");


                XmlDocument xmldoc = datasetVersion.Metadata;

                if (string.IsNullOrEmpty(convertTo))
                {

                    switch (returnType)
                    {
                        case "application/json":
                            {

                                string json = "";

                                switch (simplifiedJson)
                                {
                                    case 0:
                                        {
                                            json = JsonConvert.SerializeObject(xmldoc.DocumentElement);
                                            break;
                                        }
                                    case 1:
                                        {
                                            XmlMetadataConverter xmlMetadataConverter = new XmlMetadataConverter();
                                            json = xmlMetadataConverter.ConvertTo(xmldoc, true).ToString();

                                            break;
                                        }
                                    case 2:
                                        {
                                            XmlMetadataConverter xmlMetadataConverter = new XmlMetadataConverter();
                                            json = xmlMetadataConverter.ConvertTo(xmldoc).ToString();

                                            break;
                                        }
                                }

                                HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(json, Encoding.UTF8, "application/json") };
                                return response;
                            }
                        case "application/xml":
                            {
                                HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(xmldoc.InnerXml, Encoding.UTF8, "application/xml") };
                                return response;
                            }
                        default:
                            {

                                HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(xmldoc.InnerXml, Encoding.UTF8, "application/xml") };
                                return response;
                            }
                    }

                }
                else
                {
                    try
                    {
                        XmlDocument newXmlDoc = OutputMetadataManager.GetConvertedMetadata(id, TransmissionType.mappingFileExport,
                            convertTo);

                        if (newXmlDoc == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Format("No mapping found for this format : {0} .", format));

                        switch (returnType)
                        {
                            case "application/json":
                                {
                                    HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(newXmlDoc.DocumentElement), Encoding.UTF8, "application/json") };
                                    return response;
                                }
                            case "application/xml":
                                {
                                    HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(newXmlDoc.InnerXml, Encoding.UTF8, "application/xml") };
                                    return response;
                                }
                            default:
                                {

                                    HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(newXmlDoc.InnerXml, Encoding.UTF8, "application/xml") };
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
        }

    }



    public class MetadataViewObject
    {
        public long DatasetId { get; set; }
        public string[] Format { get; set; }
    }
}