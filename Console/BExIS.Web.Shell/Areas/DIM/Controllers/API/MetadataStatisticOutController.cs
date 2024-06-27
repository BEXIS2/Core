using BExIS.App.Bootstrap.Attributes;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class MetadataStatisticOutController : ApiController
    {
        // GET: "api/MetadataStatistic"
        /// <summary>
        /// Get a list of unique metadata values, count and list of occurrence for a given XPath.
        /// </summary>
        /// <remarks>Data is directly returned from the database. A valid token is needed to access also non-public dataset.</remarks>
        /// <returns>JSON object</returns>
        [BExISApiAuthorize]
        [PostRoute("api/MetadataStatistic")]
        public HttpResponseMessage Post([FromBody] PostApiMetadataStatisticModel data)
        {
            bool publicOnly = true;
            User user = null;

            using (UserManager userManager = new UserManager())
            {
                user = ControllerContext.RouteData.Values["user"] as User;

                // If user is registered, include also non-public datasets
                if (user != null)
                {
                    publicOnly = false;
                }
                // Return error, if token is provided, but not valid
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Token is not valid.");
                }

                // Check input values to prevent SQL injection
                string errorMessage = "";
                if (data.Xpath.Length == 0) errorMessage = "Xpath is empty, but required.";
                if (!data.Xpath.EndsWith("/")) errorMessage = "Xpath does not end with /.";
                if (data.DatasetIdsInclude != null && data.DatasetIdsInclude.Any(x => !Regex.IsMatch(x, @"^\d+$"))) errorMessage = "DatasetIdsInclude contains non numeric values.";
                if (data.DatasetIdsExclude != null && data.DatasetIdsExclude.Any(x => !Regex.IsMatch(x, @"^\d+$"))) errorMessage = "DatasetIdsExclude contains non numeric values.";
                if (data.MetadatastructureIdsInclude != null && data.MetadatastructureIdsInclude.Any(x => !Regex.IsMatch(x, @"^\d+$"))) errorMessage = "MetadatastructureIdsInclude contains non numeric values.";
                if (data.MetadatastructureIdsExclude != null && data.MetadatastructureIdsExclude.Any(x => !Regex.IsMatch(x, @"^\d+$"))) errorMessage = "MetadatastructureIdsExclude contains non numeric values.";
                if (data.RegexInclude != null && (data.RegexInclude.Contains("Drop") == true || data.RegexInclude.Contains("Delete") || data.RegexInclude.Contains("Update"))) errorMessage = "RegexInclude contains not allowed keyword (drop, update or delete).";
                if (data.RegexExclude != null && (data.RegexExclude.Contains("Drop") == true || data.RegexExclude.Contains("Delete") || data.RegexExclude.Contains("Update"))) errorMessage = "RegexExclude contains not allowed keyword (drop, update or delete).";

                if (errorMessage != "")
                {
                    return Request.CreateResponse(HttpStatusCode.PreconditionFailed, errorMessage);
                }

                // Create and execute SQL
                var result = UniqueValuesByXPATH(data, publicOnly);

                // Return empty result (204)
                if (result == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "no result");
                }

                // JSON is returned, but not correctly detected within the repsonse function.
                // Workaround: Deserialized and Serialize from and to JSON before
                var resultObject = JsonConvert.DeserializeObject(result.ToString());
                string resp = JsonConvert.SerializeObject(resultObject);

                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                return response;
            }
        }

        private object UniqueValuesByXPATH(PostApiMetadataStatisticModel data, bool publicOnly = true)
        {
            StringBuilder mvBuilder = new StringBuilder();

            string publicOnlySQL1 = "";
            string publicOnlySQL2 = "";
            if (publicOnly == true)
            {
                publicOnlySQL1 = "left join entitypermissions as e on b.datasetref=e.key";
                publicOnlySQL2 = "and subjectref is null";
            }

            string datasetVersionIds1 = "b.status = 2";
            string datasetVersionIds2 = "";
            if (data.DatasetVersionIdsInclude != null)
            {
                datasetVersionIds1 = "";
                datasetVersionIds2 = "id in (" + String.Join(",", data.DatasetVersionIdsInclude) + ")";
            }

            string datasetIdsInclude = "";
            if (data.DatasetIdsInclude != null)
            {
                datasetIdsInclude = "and datasetref in(" + String.Join(",", data.DatasetIdsInclude) + ")";
            }

            string datasetIDsExclude = "";
            if (data.DatasetIdsExclude != null)
            {
                datasetIDsExclude = "and datasetref not in(" + String.Join(",", data.DatasetIdsExclude) + ")";
            }

            string metadatastructureIdsInclude = "";
            if (data.MetadatastructureIdsInclude != null)
            {
                metadatastructureIdsInclude = "and (xpath('/Metadata/@id', metadata))[1]::text::integer in(" + String.Join(",", data.MetadatastructureIdsInclude) + ")";
            }

            string metadatastructureIdsExclude = "";
            if (data.MetadatastructureIdsExclude != null)
            {
                metadatastructureIdsExclude = "and (xpath('/Metadata/@id', metadata))[1]::text::integer not in(" + String.Join(",", data.MetadatastructureIdsExclude) + ")";
            }

            string regexInclude = "";
            if (data.RegexInclude != null)
            {
                regexInclude = " where xpath ~'" + data.RegexInclude + "'";
            }

            string regexExclude = "";
            if (data.RegexExclude != null)
            {
                regexExclude = " where xpath !~'" + data.RegexExclude + "'";
            }

            // example xpath: " /Metadata/contacts/contactsType/contactPerson/contactType/institute/instituteType/"
            mvBuilder.AppendLine(string.Format("with result as (select  xpath,json_build_object(\'data\', json_agg(to_jsonb(row(b.datasetref, b.id, b.title, (xpath('/Metadata/@id', b.metadata))[1]::text::int))), \'count\', count(b.id)) as json from datasetversions as b {1} left join lateral unnest(xpath('{0}/text()', b.metadata)::text[]) as xpath on true where {9} {10} {2} {3} {4} {5} {6} group by xpath order by xpath asc) select json_object_agg(coalesce(xpath, \'null\'), result.json) from result {7} {8}; ",
                data.Xpath, publicOnlySQL1, datasetIdsInclude, datasetIDsExclude, metadatastructureIdsInclude, metadatastructureIdsExclude, publicOnlySQL2, regexInclude, regexExclude, datasetVersionIds1, datasetVersionIds2));
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
                return null;
            }
        }
    }
}