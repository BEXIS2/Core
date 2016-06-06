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

namespace BExIS.Web.Shell.Areas.DIM.Controllers
{
    public class MetadataController : ApiController
    {
        // GET: api/Metadata
        public IEnumerable<MatadataViewObject> Get()
        {
            DatasetManager dm = new DatasetManager();
            var datasetIds = dm.GetDatasetLatestIds();

            List<MatadataViewObject> tmp = new List<MatadataViewObject>();

            foreach (var id in datasetIds)
            {
                MatadataViewObject mvo = new MatadataViewObject();
                mvo.DatasetId = id;
                
                List<string> t = XmlDatasetHelper.GetAllExportInformation(id, TransmissionType.mappingFileExport, AttributeNames.name).ToList();

                mvo.Export = t.ToArray();

                tmp.Add(mvo);
            }

            return tmp;
        }

        // GET: api/Metadata/5
        // HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(doc.innerXml, Encoding.UTF8,"application/xml") };
        public HttpResponseMessage Get(int id)
        {
            string convertTo = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "convertTo".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;

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
                    string mappingFileName = XmlDatasetHelper.GetExportInformation(dsv, TransmissionType.mappingFileExport,convertTo);
                    string pathMappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), mappingFileName);

                    XmlMapperManager xmlMapperManager = new XmlMapperManager();
                    xmlMapperManager.Load(pathMappingFile, "exporttest");

                    XmlDocument newXml = xmlMapperManager.Export(dsv.Metadata, dsv.Id, true);

                    HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(newXml.InnerXml, Encoding.UTF8, "application/xml") };

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

    public class MatadataViewObject
    {
        public long DatasetId { get; set; }
        public string[] Export { get; set; }
    }
}
