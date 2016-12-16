using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;
using BExIS.Dlm.Services.Data;
using BExIS.Xml.Services;
using System.IO;
using BExIS.Dlm.Entities.Data;
using BExIS.Xml.Helpers.Mapping;
using Vaiona.Utils.Cfg;
using System.Text;
using BExIS.IO.Transform.Output;

namespace BExIS.Web.Shell.Areas.DIM.Controllers
{
    public class MetadataController : ApiController
    {
        // GET: api/Metadata
        public IEnumerable<MetadataViewObject> Get()
        {
            DatasetManager dm = new DatasetManager();
            var datasetIds = dm.GetDatasetLatestIds();

            List<MetadataViewObject> tmp = new List<MetadataViewObject>();

            foreach (var id in datasetIds)
            {
                MetadataViewObject mvo = new MetadataViewObject();
                mvo.DatasetId = id;
                
                List<string> t = XmlDatasetHelper.GetAllTransmissionInformation(id, TransmissionType.mappingFileExport, AttributeNames.name).ToList();

                mvo.Format = t.ToArray();

                tmp.Add(mvo);
            }

            return tmp;
        }

        // GET: api/Metadata/5
        // HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(doc.innerXml, Encoding.UTF8,"application/xml") };
        public HttpResponseMessage Get(int id)
        {
            string convertTo = "";
            try
            {
                convertTo = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "format".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            }
            catch (Exception ex) { }

            DatasetManager dm = new DatasetManager();
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

        // POST: api/Metadata
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Metadata/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Metadata/5
        public void Delete(int id)
        {
        }
    }

    public class MetadataViewObject
    {
        public long DatasetId { get; set; }
        public string[] Format { get; set; }
    }
}
