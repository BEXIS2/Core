using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BExIS.IO.Transform.Output;
using System.Data;
using BExIS.Xml.Services;

namespace BExIS.Web.Shell.Areas.DIM.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request datasets or a view on data sets and get the result in 
    /// XML, JSON, or CSV formats.
    /// The design follows the RESTFull pattern mentioned in http://www.asp.net/web-api/overview/older-versions/creating-a-web-api-that-supports-crud-operations
    /// CSV formatter is implements in the DataTupleCsvFormatter class in the Models folder. The formatter is registered in the WebApiConfig as an automatic formatter,
    /// so if the clinet sets the request's Mime type to text/csv, this formatter will be automatically engaged. text/xml and text/json return XML and JSON content accordingly
    /// </summary>
    public class DataController : ApiController
    {
        // GET: api/data
        public IEnumerable<long> Get()
        {
            DatasetManager dm = new DatasetManager();
            var datasetIds = dm.GetDatasetLatestIds();
            return datasetIds;
        }

        // GET: api/data/5
        /// <summary>
        /// In addition to the id, it is possible to have projection and selection criteria passed to the action via query string parameters
        /// </summary>
        /// <param name="id">Dataset Id</param>
        /// <returns></returns>
        /// <remarks> The action accepts the following additional parameters via the query string
        /// 1: projection: is a comman separated list of ids that determines which variables of the dataset version tuples should take part in the result set
        /// 2: selection: is a logical expression that filters the tuples of the chosen dataset. The expression should have been written against the variables of the dataset only.
        /// logical operators, nesting, precedence, and SOME functions should be supported.
        /// </remarks>
        public DataTable Get(int id)
        {
            string projection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "projection".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string selection  = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "selection" .Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;

            string mimiType = "text/csv";
            
            OutputDataManager ioOutputDataManager = new OutputDataManager();

            DatasetManager dm = new DatasetManager();
            DatasetVersion version = dm.GetDatasetLatestVersion(id);

            string title = XmlDatasetHelper.GetInformation(version, NameAttributeValues.title);

            // check the data sturcture type ...
            if (version.Dataset.DataStructure.Self is StructuredDataStructure)
            {
                // apply selection and projection
                var tuples = dm.GetDatasetVersionEffectiveTuples(version);

                DataTable dt = OutputDataManager.ConvertPrimaryDataToDatatable(version,
                    dm.GetDatasetVersionEffectiveTupleIds(version), title, true);

                if (!string.IsNullOrEmpty(projection))
                {
                    dt = OutputDataManager.ProjectionOnDataTable(dt, projection.Split(','));
                }

                if (!string.IsNullOrEmpty(selection))
                {
                    dt = OutputDataManager.SelectionOnDataTable(dt, selection);
                }

                return dt;

            } else
            {
                //return File(Path.Combine(AppConfiguration.DataPath, path), mimeType, title);
                return null;
            }
        }

        // GET: api/data/5?projection=<projection>
        // If the above GET function fails to work, then indivial actions for each case: projection, selection, etc.
        // public string Get(int id, string projection)
        //{
        //    return "value";
        //}

        // POST: api/data
        /// <summary>
        /// Create a new dataset!!!
        /// </summary>
        /// <param name="value"></param>
        public void Post([FromBody]string value)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // PUT: api/Datasets/5
        /// <summary>
        /// Updates an existing dataset
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void Put(int id, [FromBody]string value)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // DELETE: api/data/5
        /// <summary>
        /// Deletes an existing dataset
        /// </summary>
        /// <param name="id"></param>
        public void Delete(int id)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}
