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
    public class MetadataInController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        // POST: api/Metadata
        [BExISApiAuthorize]
        [Route("api/Metadata")]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Metadata/5
        [BExISApiAuthorize]
        [Route("api/Metadata")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Metadata/5
        [ApiExplorerSettings(IgnoreApi = true)]
        [BExISApiAuthorize]
        [Route("api/Metadata")]
        public void Delete(int id)
        {
        }
    }
}