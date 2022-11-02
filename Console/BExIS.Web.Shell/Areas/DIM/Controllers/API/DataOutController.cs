using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Output;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace BExIS.Modules.Dim.UI.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request datasets or a view on data sets and get the result in
    /// either of XML, JSON, or CSV formats.
    /// </summary>
    /// <remarks>
    /// This class is designed as a Web API to allow various client tools request datasets or a view on data sets and get the result in
    /// either of XML, JSON, or CSV formats.
    /// The design follows the RESTFull pattern mentioned in http://www.asp.net/web-api/overview/older-versions/creating-a-web-api-that-supports-crud-operations
    /// CSV formatter is implemented in the DataTupleCsvFormatter class in the Models folder.
    /// The formatter is registered in the WebApiConfig as an automatic formatter, so if the clinet sets the request's Mime type to text/csv, this formatter will be automatically engaged.
    /// text/xml and text/json return XML and JSON content accordingly.
    /// </remarks>
    public class DataOutController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        // GET: api/data
        /// <summary>
        /// Get a list of all dataset ids
        /// </summary>
        /// <returns>List of ids</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Data")]
        public IEnumerable<long> Get()
        {
            DatasetManager dm = new DatasetManager();
            try
            {
                var datasetIds = dm.GetDatasetIds();
                //test
                return datasetIds;
            }
            finally
            {
                dm.Dispose();
            }
        }

        // GET: api/data/5
        /// <summary>
        /// In addition to the id, it is possible to have projection and selection criteria passed to the action via query string parameters
        /// </summary>
        /// <param name="id">Dataset Id</param>
        /// <param name="header">Is a comman separated list of variable names that determines which variables of the dataset. e.g: Var1,Var2,var3</param>
        /// <param name="filter">is a logical expression that filters the tuples of the chosen dataset. e.g. : Var1='Value'</param>
        /// <returns> data from the latest version of a dataset</returns>
        /// <remarks> The action accepts the following additional parameters via the query string
        /// 1: header: is a comman separated list of variable names that determines which variables of the dataset version tuples should take part in the result set
        /// 2: filter: is a logical expression that filters the tuples of the chosen dataset. The expression should have been written against the variables of the dataset only.
        /// logical operators, nesting, precedence, and SOME functions should be supported.
        /// </remarks>
        [BExISApiAuthorize]
        //[Route("api/Data")]
        [GetRoute("api/Data/{id}")]
        [HttpGet]
        public HttpResponseMessage Get(int id, [FromUri] string header = null, [FromUri] string filter = null, [FromUri] string take = null, [FromUri] string skip = null)
        {
            string projection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "header".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string selection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "filter".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string takeAsString = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "take".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string skipAsString = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "skip".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string token = this.Request.Headers.Authorization?.Parameter;


            int _take = 0;
            int _skip = 0;

            // check wheter take exist and it is a integer
            if (!string.IsNullOrEmpty(takeAsString) && !int.TryParse(takeAsString, out _take))
            {
                var request = Request.CreateResponse();
                request.Content = new StringContent("take is not a integer.");

                return request;
            }

            // check wheter skip exist and it is a integer
            if (!string.IsNullOrEmpty(skipAsString) && !int.TryParse(skipAsString, out _skip))
            {
                var request = Request.CreateResponse();
                request.Content = new StringContent("skip is not a integer.");

                return request;
            }

            return getData(id, -1, token, projection, selection, _skip, _take);
        }

        // GET: api/data/5
        /// <summary>
        /// In addition to the id, it is possible to have projection and selection criteria passed to the action via query string parameters
        /// </summary>
        /// <param name="id">Dataset Id</param>
        /// <param name="version">Version number of the dataset</param>
        /// <returns></returns>
        /// <remarks> The action accepts the following additional parameters via the query string
        /// 1: header: is a comman separated list of ids that determines which variables of the dataset version tuples should take part in the result set
        /// 2: filter: is a logical expression that filters the tuples of the chosen dataset. The expression should have been written against the variables of the dataset only.
        /// logical operators, nesting, precedence, and SOME functions should be supported.
        /// </remarks>
        [BExISApiAuthorize]
        //[Route("api/Data")]
        [GetRoute("api/Data/{id}/{version}")]
        [HttpGet]
        public HttpResponseMessage Get(long id, int version, [FromUri] string header = null, [FromUri] string filter = null, [FromUri] string take = null, [FromUri] string skip = null)
        {
            string projection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "header".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string selection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "filter".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string takeAsString = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "take".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string skipAsString = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "skip".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string token = this.Request.Headers.Authorization?.Parameter;

            int _take = 0;
            int _skip = 0;

            // check wheter take exist and it is a integer
            if (!string.IsNullOrEmpty(takeAsString))
            {
                if (!int.TryParse(takeAsString, out _take))
                {
                    var request = Request.CreateResponse();
                    request.Content = new StringContent("take is not a integer.");

                    return request;
                }
            }

            // check wheter skip exist and it is a integer
            if (!string.IsNullOrEmpty(skipAsString))
            {
                if (!int.TryParse(skipAsString, out _skip))
                {
                    var request = Request.CreateResponse();
                    request.Content = new StringContent("skip is not a integer.");

                    return request;
                }
            }


            return getData(id, version, token, projection, selection, _skip, _take);
        }

        private HttpResponseMessage getData(long id, int version, string token, string projection = null, string selection = null, int skip = 0, int take = 0)
        {

            if (id <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Id should be greater then 0");

          
            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            EntityManager entityManager = new EntityManager();


            bool isPublic = false;

            using (DatasetManager datasetManager = new DatasetManager())
            using (UserManager userManager = new UserManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (DataStructureManager dataStrutcureManager = new DataStructureManager())
            using (EntityManager entityManager = new EntityManager())
            {
                try
                {
                    // if a dataset is public, then the api should also return data if there is no token for a user

                    #region is public

                    long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                    entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;

                    isPublic = entityPermissionManager.Exists(null, entityTypeId.Value, id);

                    #endregion is public

                    if (!isPublic && String.IsNullOrEmpty(token))

                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Bearer token not exist.");
                }

                    User user = userManager.Users.Where(u => u.Token.Equals(token)).FirstOrDefault();

                    if (isPublic || user != null)
                    {
                        if (isPublic || entityPermissionManager.HasEffectiveRight(user.Name, typeof(Dataset), id, RightType.Read))
                        {
                            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                            OutputDataManager ioOutputDataManager = new OutputDataManager();

                            Dataset dataset = datasetManager.GetDataset(id);

                            // If the requested version is -1 or the last version of the dataset, then the data will be loaded in a
                            // different way than when loading the data from an older version
                            bool isLatestVersion = false;
                            if (version == -1 || dataset.Versions.Count == version) isLatestVersion = true;

                        DataTable dt = null;

                            if (isLatestVersion)
                            {
                                #region get data from the latest version of a dataset

                                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);

                                string title = datasetVersion.Title;

                            // check the data sturcture type ...
                            if (datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                            {


                                // apply selection and projection
                                long count = datasetManager.GetDataTuplesCount(datasetVersion.Id);

                                if (count > 0)
                                {
                                    dt = datasetManager.GetLatestDatasetVersionTuples(id, null, null, null, 0, (int)count);
                                    dt.Strip();

                                    if (!string.IsNullOrEmpty(selection))
                                    {
                                        dt = OutputDataManager.SelectionOnDataTable(dt, selection, true);
                                    }

                                    if (!string.IsNullOrEmpty(projection))
                                    {
                                        // make the header names upper case to make them case insensitive
                                        dt = OutputDataManager.ProjectionOnDataTable(dt, projection.ToUpper().Split(','));
                                    }
                                }
                                else
                                {
                                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "There is no data for the dataset.");
                                }


                                #endregion get data from the latest version of a dataset

                                //return model;
                            }
                            else
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The data of this dataset is not structured.");

                            }
                        }
                        else
                        {
                            #region load data of a older version of a dataset

                                int index = version - 1;
                                if (version >= dataset.Versions.Count)
                                {
                                    return Request.CreateResponse(HttpStatusCode.PreconditionFailed, String.Format("This version ({0}) is not available for the dataset", version));
                                }

                                DatasetVersion datasetVersion = dataset.Versions.OrderBy(d => d.Timestamp).ElementAt(version - 1);

                                string title = datasetVersion.Title;

                                // check the data sturcture type ...
                                if (datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                                {
                                    //FilterExpression filter = null;
                                    //OrderByExpression orderBy = null;

                                // apply selection and projection
                                int count = datasetManager.GetDatasetVersionEffectiveTuples(datasetVersion).Count;

                                if (count > 0) // has primary data
                                {

                                    dt = datasetManager.GetDatasetVersionTuples(datasetVersion.Id, 0, count);

                                    dt.Strip();

                                    // if ther is a selection - filter the data
                                    if (!string.IsNullOrEmpty(selection))
                                    {
                                        dt = OutputDataManager.SelectionOnDataTable(dt, selection, true);
                                    }

                                    if (!string.IsNullOrEmpty(projection))
                                    {
                                        // make the header names upper case to make them case insensitive
                                        dt = OutputDataManager.ProjectionOnDataTable(dt, projection.ToUpper().Split(','));
                                    }
                                }
                                else // has no primary data
                                {
                                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "There is no data for the dataset version.");
                                }

                            }
                            else // return files of the unstructure dataset
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The data is not structured.");
                            }

                                #endregion load data of a older version of a dataset
                            }


                            if (dt != null)
                            {
                                dt.TableName = id + "_data";

                                var outputDataManager = new OutputDataManager();
                                var apifilePath = outputDataManager.GenerateAsciiFile("api", dt, "test", "text/csv", dataset.DataStructure.Id);

                                var response = Request.CreateResponse();

                                using (StreamReader sr = new StreamReader(apifilePath))
                                {
                                    response.Content = new StringContent(sr.ReadToEnd());
                                    //response.Content = new ObjectContent(typeof(DatasetModel), model, new DatasetModelCsvFormatter(model.DataTable.TableName));
                                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                                }

                            //set headers on the "response"
                            return response;
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "The data could not be loaded.");
                        }
                       

                    }
                    else // has rights?
                    {

                        return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User has no read right.");
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User is not available.");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                datasetManager.Dispose();
                userManager.Dispose();
                entityPermissionManager.Dispose();
                entityManager.Dispose();
            }

        }

    }
}