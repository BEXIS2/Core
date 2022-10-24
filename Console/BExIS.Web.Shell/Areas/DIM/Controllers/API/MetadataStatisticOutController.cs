using BExIS.App.Bootstrap.Attributes;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Utils.Route;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dim.UI.Controllers
{

    public class MetadataStatisticOutController : ApiController
    {

        // GET: "api/MetadataStatistic"
        /// <summary>
        /// Get a list of unique metadata values, count and list of occurrence
        /// </summary>
        /// <remarks>Data is directly returned from the database without (currently) any further permission check. Nested nodes are cuurently not supported.</remarks>
        /// <returns>JSON object</returns>
        [BExISApiAuthorize]
        [PostRoute("api/MetadataStatistic")]
        public HttpResponseMessage Post([FromBody] PostApiMetadataStatisticModel data)
        {

            // #TODO add security
            // string token = this.Request.Headers.Authorization?.Parameter;
            if (data.xpath.Length == 0) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "xpath is empty, but required");
            
            var result = UniqueValuesByXPATH(data.xpath, data.datasetIds, data.metadatastructureIds);

            // JSON is returned, but not correctly detected within the repsonse function.
            // Workaround: Deserialized and Serialize from and to JSON before
            var resultObject = JsonConvert.DeserializeObject(result.ToString());
            string resp = JsonConvert.SerializeObject(resultObject);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return response;           
        }

        private object UniqueValuesByXPATH(string xpath, string[] datatsetIDs = null, string[] metadatastructureIds = null)
        {
            StringBuilder mvBuilder = new StringBuilder();

            string datasetIDsIN = "";
            if (datatsetIDs != null )
            {
                datasetIDsIN = "and datasetref in(" + String.Join(",", datatsetIDs) + ")";
            }

            string metadatastructureIdsIN = "";
            if (metadatastructureIds != null)
            {
                metadatastructureIdsIN = "and (xpath('/Metadata/@id', metadata))[1]::text::integer in(" + String.Join(",", metadatastructureIds) + ")";
            }

            // example xpath: " /Metadata/contacts/contactsType/contactPerson/contactType/institute/instituteType/"
            mvBuilder.AppendLine(string.Format("with result as (select(xpath(\'{0}/text()\', metadata))[1]::text as xpath,json_build_object(\'data\', json_agg(to_jsonb(row(b.datasetref, b.title))), \'count\', count(id)) as json from datasetversions as b where status = 2 {1} {2} group by xpath order by xpath asc) select json_object_agg(coalesce(xpath, \'null\'), result.json) from result; ", xpath, datasetIDsIN, metadatastructureIdsIN));
            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetBulkUnitOfWork())
                {
                    var result = uow.ExecuteScalar(mvBuilder.ToString());
                    return result;
                }
            }
            catch (Exception e)
            {
                return -1;
            }
        }
    }
}
