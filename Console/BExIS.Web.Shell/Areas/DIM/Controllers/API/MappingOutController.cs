using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dim.Services;
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
using System.Net.Http.Headers;
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
    public class MappingOutController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();


        [BExISApiAuthorize]
        [GetRoute("api/Mapping")]
        public IEnumerable<Mapping> Get()
        {
            using (var mappingManager = new MappingManager())
            {
                return mappingManager.GetMappings();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceElementId"></param>
        /// <param name="sourceElementType"></param>
        /// <param name="targetElementId"></param>
        /// <param name="targetElementType"></param>
        /// <returns></returns>
        /// <remarks> 
        /// </remarks>
        [BExISApiAuthorize]

        [GetRoute("api/Mapping/{sourceElementId}/{sourceElementType}/{targetElementId}/{targetElementType}")]
        [HttpGet]
        public HttpResponseMessage Get(long sourceElementId, int sourceElementType, long targetElementId, int targetElementType)
        {

            using (var mappingManager = new MappingManager())
            {
                var sourceLE = mappingManager.GetLinkElement(sourceElementId, (LinkElementType)sourceElementType);
                var targetLE = mappingManager.GetLinkElement(targetElementId, (LinkElementType)targetElementType);

                var tmp = new List<Mapping>();

                var rootMapping = mappingManager.GetMapping(sourceLE, targetLE);
                tmp.Add(rootMapping);

                var complexMappings = mappingManager.GetChildMappingFromRoot(rootMapping.Id, 2);
                tmp.AddRange(complexMappings);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                string resp = JsonConvert.SerializeObject(tmp.OrderBy(m=>m.Id));


                response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                //set headers on the "response"
                return response;

            }
        }
    }
}