using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using BEXIS.JSON.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using System.Xml.Linq;

namespace BExIS.Modules.Dim.UI.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request metadata structure and get the result in json.
    /// </summary>
    public class MetadataStructureOutController : ApiController
    {
        /// <summary>
        /// this function return an overview about existing metadata structures
        /// </summary>
        /// <returns></returns>
        [BExISApiAuthorize]
        [GetRoute("api/MetadataStructure")]
        [ResponseType(typeof(MetadataStructureViewObject))]
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
                        Name = metadataStructure.Name,
                        EntityType = GetEntityNameFromMetadatStructure(metadataStructure)
                    });
                }
            }
            return tmp;
        }

        /// <summary>
        /// this api get a metadata structure based on the incoming api.
        /// it converts the structure into a json schema
        /// </summary>
        /// <remarks>
        /// the API returns the selected metadata structure in json schema.
        /// each object contains a @ref attribute of type string, simple types contain a #text where the value is entered.
        /// even a simple value therefore consists of @ref and #text.
        /// @ref will beused fpor internal or external references, #text is used to store input values
        /// </remarks>
        /// <param name="id"></param>
        /// <returns>metadata structure as json schema</returns>
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

        private string GetEntityNameFromMetadatStructure(MetadataStructure metadataStructure)
        {
            // get MetadataStructure
            if (metadataStructure != null && metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> tmp = XmlUtility.GetXElementByNodeName(nodeNames.entity.ToString(), xDoc);
                if (tmp.Any())
                    return tmp.First().Attribute("name").Value;
            }

            return string.Empty;
        }
    }

    public class MetadataStructureViewObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string EntityType { get; set; }
    }
}