using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO.Transform.Output;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using BEXIS.JSON.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;

namespace BExIS.Modules.Dim.UI.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request metadata of a datasets and get the result in XML.
    /// </summary>
    public class MetadataStructureOutController : ApiController
    {

        [BExISApiAuthorize]
        [GetRoute("api/MetadataStructure")]
        public IEnumerable<MetadataStructureViewObject> Get()
        {
            List<MetadataStructureViewObject> tmp = new List<MetadataStructureViewObject>();

            using (var metadataStructureManager = new MetadataStructureManager())
            {
                foreach (var metadataStructure in metadataStructureManager.Repo.Get())
                {
                    tmp.Add(new MetadataStructureViewObject()
                    {
                        Id = metadataStructure.Id,
                        Name = metadataStructure.Name
                    });
                }
            }

            return tmp;
        }

        [BExISApiAuthorize]
        [GetRoute("api/MetadataStructure/{id}")]
        [HttpGet]
        public HttpResponseMessage Get(long id)
        {

            if (id == 0)
            {
                return Request.CreateResponse(HttpStatusCode.PreconditionFailed, String.Format("This metadata structure ({0}) is not available ", id));
            }
            else
            {
                using (var metadataStructureManager = new MetadataStructureManager())
                {
                    if (metadataStructureManager.Repo.Get(id) == null)
                    { 
                        return Request.CreateResponse(HttpStatusCode.PreconditionFailed, String.Format("The metadata structure with id ({0}) not exist.", id));

                    }
                }
            }

            MetadataStructureConverter converter = new MetadataStructureConverter();
            var schema = converter.ConvertToJsonSchema(id);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(schema.ToString(), System.Text.Encoding.UTF8, "application/json");
            return response;
        }
    }

        

    public class MetadataStructureViewObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}