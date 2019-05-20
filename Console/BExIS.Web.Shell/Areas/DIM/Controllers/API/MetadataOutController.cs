using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Output;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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
        //[Route("api/Metadata")]
        public IEnumerable<MetadataViewObject> Get()
        {
            DatasetManager dm = new DatasetManager();

            try
            {
                var datasetIds = dm.GetDatasetLatestIds();

                List<MetadataViewObject> tmp = new List<MetadataViewObject>();

                foreach (var id in datasetIds)
                {
                    MetadataViewObject mvo = new MetadataViewObject();
                    mvo.DatasetId = id;

                    List<string> t = xmlDatasetHelper.GetAllTransmissionInformation(id, TransmissionType.mappingFileExport, AttributeNames.name).ToList();

                    mvo.Format = t.ToArray();

                    tmp.Add(mvo);
                }

                return tmp;
            }
            finally
            {
                dm.Dispose();
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
        //[Route("api/Metadata")]
        public HttpResponseMessage Get(int id, [FromUri] string format = null)
        {
            DatasetManager dm = new DatasetManager();

            try
            {
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
                    //return xmldoc;
                    HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(xmldoc.InnerXml, Encoding.UTF8, "application/xml") };
                    return response;
                }
                else
                {
                    try
                    {
                        XmlDocument newXmlDoc = OutputMetadataManager.GetConvertedMetadata(id, TransmissionType.mappingFileExport,
                            convertTo);

                        HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(newXmlDoc.InnerXml, Encoding.UTF8, "application/xml") };

                        return response;
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