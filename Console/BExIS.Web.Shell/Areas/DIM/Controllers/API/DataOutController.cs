using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.NH.Querying;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

//using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        public HttpResponseMessage Get(int id, [FromUri] string header = null, [FromUri] string filter = null)
        {
            string projection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "header".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string selection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "filter".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string token = this.Request.Headers.Authorization?.Parameter;

            return getData(id, -1, token, projection, selection);
        }


        /// <summary>
        /// In addition to the id and version id, it is possible to have projection and selection criteria passed to the action via query string parameters
        /// </summary>
        /// <param name="id">Dataset Id</param>
        /// <param name="versionId">Version Id of the dataset</param>
        /// <returns></returns>
        /// <remarks> The action accepts the following additional parameters via the query string
        /// 1: header: is a comman separated list of ids that determines which variables of the dataset version tuples should take part in the result set
        /// 2: filter: is a logical expression that filters the tuples of the chosen dataset. The expression should have been written against the variables of the dataset only.
        /// logical operators, nesting, precedence, and SOME functions should be supported.
        /// </remarks>
        [BExISApiAuthorize]
        //[Route("api/Data")]
        [GetRoute("api/Data/{id}/{versionId}")]
        [HttpGet]
        public HttpResponseMessage Get(long id, long versionId, [FromUri] string header = null, [FromUri] string filter = null)
        {
            if (id <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Id should be greater then 0");

            string projection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "header".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string selection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "filter".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string token = this.Request.Headers.Authorization?.Parameter;

            return getData(id, versionId, token, projection, selection);
        }

        /// <summary>
        /// In addition to the id and version number, it is possible to have projection and selection criteria passed to the action via query string parameters
        /// </summary>
        /// <param name="id">Dataset Id</param>
        /// <param name="version_number">Version number of the dataset</param>
        /// <returns></returns>
        /// <remarks> The action accepts the following additional parameters via the query string
        /// 1: header: is a comman separated list of ids that determines which variables of the dataset version tuples should take part in the result set
        /// 2: filter: is a logical expression that filters the tuples of the chosen dataset. The expression should have been written against the variables of the dataset only.
        /// logical operators, nesting, precedence, and SOME functions should be supported.
        /// </remarks>
        [BExISApiAuthorize]
        //[Route("api/Data")]
        [GetRoute("api/Data/{id}/version_number/{version_number}")]
        [HttpGet]
        public HttpResponseMessage Get(long id, int version_number, [FromUri] string header = null, [FromUri] string filter = null)
        {
            if (id <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Id should be greater then 0");

            if (version_number <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Version should be greater then 0");

            string projection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "header".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string selection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "filter".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string token = this.Request.Headers.Authorization?.Parameter;

            using (DatasetManager dm = new DatasetManager())
            {
                int index = version_number - 1;
                Dataset dataset = dm.GetDataset(id);

                int versions = dataset.Versions.Count;

                if (versions < version_number)
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "This version number does not exist for this dataset");

                var datasetVersion = dataset.Versions.OrderBy(d => d.Timestamp).ElementAt(index);

                return getData(id, datasetVersion.Id, token, projection, selection);
            }

        }

        /// <summary>
        /// In addition to the id and version name, it is possible to have projection and selection criteria passed to the action via query string parameters
        /// </summary>
        /// <param name="id">Dataset Id</param>
        /// <param name="version_name">Version name of the dataset</param>
        /// <returns></returns>
        /// <remarks> The action accepts the following additional parameters via the query string
        /// 1: header: is a comman separated list of ids that determines which variables of the dataset version tuples should take part in the result set
        /// 2: filter: is a logical expression that filters the tuples of the chosen dataset. The expression should have been written against the variables of the dataset only.
        /// logical operators, nesting, precedence, and SOME functions should be supported.
        /// </remarks>
        [BExISApiAuthorize]
        //[Route("api/Data")]
        [GetRoute("api/Data/{id}/version_name/{version_name}")]
        [HttpGet]
        public HttpResponseMessage Get(long id, string version_name, [FromUri] string header = null, [FromUri] string filter = null)
        {
            if (id <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Id should be greater then 0");

            if (string.IsNullOrEmpty(version_name))
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Version name not exist");


            string projection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "header".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string selection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "filter".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string token = this.Request.Headers.Authorization?.Parameter;

            using (DatasetManager dm = new DatasetManager())
            {

                var versionId = dm.GetDatasetVersions(id).Where(d => d.VersionName == version_name).Select(d => d.Id).FirstOrDefault();

                if (versionId <= 0)
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "This version name does not exist for this dataset");

                return getData(id, versionId, token, projection, selection);
            }
        }

        private HttpResponseMessage getData(long id, long versionId, string token, string projection = null, string selection = null)
        {

            if (id <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Id should be greater then 0");

          
            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            EntityManager entityManager = new EntityManager();


            bool isPublic = false;
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

                User user = ControllerContext.RouteData.Values["user"] as User;

                if (isPublic || user != null)
                {
                    if (isPublic || entityPermissionManager.HasEffectiveRight(user.Name, typeof(Dataset), id, RightType.Read))
                    {
                        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                        OutputDataManager ioOutputDataManager = new OutputDataManager();
                        
                        Dataset dataset = datasetManager.GetDataset(id);
                        if(dataset == null) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "dataset " + id +" not exist.");

                        DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
             
                        // If the requested version is -1 or the last version of the dataset, then the data will be loaded in a
                        // different way than when loading the data from an older version
                        bool isLatestVersion = false;

                        if (versionId == -1) isLatestVersion = true;
                        else
                        {
                            // check version id belongs to dataset
                            if (!dataset.Versions.Select(v => v.Id).Contains(versionId)) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "this version id is not part of the dataset " + id);
                            if (versionId == -1 || datasetVersion.Id == versionId) isLatestVersion = true;
                        }
                        DataTable dt = null;

                        if (isLatestVersion)
                        {
                            #region get data from the latest version of a dataset

                            string title = datasetVersion.Title;

                            // check the data sturcture type ...
                            if (datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                            {


                                // apply selection and projection
                                long count = datasetManager.GetDataTuplesCount(datasetVersion.Id);

                                if (count > 0)
                                {
                                    dt = datasetManager.GetLatestDatasetVersionTuples(id, null, null, null, "", 0, (int)count);
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

                            datasetVersion = datasetManager.GetDatasetVersion(versionId);

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

                                    if (!string.IsNullOrEmpty(selection))
                                    {
                                        dt = OutputDataManager.SelectionOnDataTable(dt, selection);
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

                            using (StreamReader sr = new StreamReader(apifilePath, Encoding.UTF8, true))
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