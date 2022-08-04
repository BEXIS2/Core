using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Output;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
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
        /// With the Get function you get an overview of the exiting datasets from which you can load metadata.
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
        /// With the Get function you get all metadata based on the given metadata schema name
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

        // GET: api/Metadata/5
        // HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(doc.innerXml, Encoding.UTF8,"application/xml") };

        /// <summary>
        /// This Get function has been extended by a parameter id. The id refers to the dataset. The metadata will be loaded from the dataset
        /// </summary>
        /// <param name="id">Dataset Id</param>
        /// <returns>Xml Document</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Metadata/{id}")]
        public HttpResponseMessage Get(int id, [FromUri] string format = null)
        {
            DatasetManager dm = new DatasetManager();

            string returnType = "";
                returnType = Request.Content.Headers.ContentType?.MediaType;

            try
            {
                if (id == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "dataset id should be greater then 0.");

                //entity permissions
                if (id > 0)
                {
                    Dataset d = dm.GetDataset(id);
                    if (d == null)
                        return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset with the id (" + id + ") does not exist.");
                }

                string convertTo = "";

                try
                {
                    convertTo = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "format".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
                }
                catch (Exception ex) { }

                DatasetVersion dsv = dm.GetDatasetLatestVersion(id);
                XmlDocument xmldoc = dsv.Metadata;

                if (string.IsNullOrEmpty(convertTo))
                {


                    switch (returnType)
                    {
                        case "application/json": {
                                HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(JsonConvert.SerializeObject(xmldoc.DocumentElement), Encoding.UTF8, "application/json") };
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
            finally
            {
                dm.Dispose();
            }
        }
    }

    public class MetadataViewObject
    {
        public long DatasetId { get; set; }
        public string[] Format { get; set; }
    }
}