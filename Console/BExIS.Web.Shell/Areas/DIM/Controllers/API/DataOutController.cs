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
using System.Linq;

//using System.Linq.Dynamic;
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
                var datasetIds = dm.GetDatasetLatestIds();
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
        public HttpResponseMessage Get(long id, int version, [FromUri] string header = null, [FromUri] string filter = null)
        {
            string projection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "header".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string selection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "filter".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string token = this.Request.Headers.Authorization?.Parameter;

            return getData(id, version, token, projection, selection);
        }

        private HttpResponseMessage getData(long id, int version, string token, string projection = null, string selection = null)
        {
            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            EntityManager entityManager = new EntityManager();

            bool isPublic = false;
            try
            {
                // if a dataset is public, then the api should also return data if there is no token for a user

                #region is public

                entityManager = new EntityManager();
                long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;

                isPublic = entityPermissionManager.Exists(null, entityTypeId.Value, id);

                #endregion is public

                if (!isPublic && String.IsNullOrEmpty(token))

                {
                    var request = Request.CreateResponse();
                    request.Content = new StringContent("Bearer token not exist.");

                    return request;
                }

                User user = userManager.Users.Where(u => u.Token.Equals(token)).FirstOrDefault();

                if (isPublic || user != null)
                {
                    if (isPublic || entityPermissionManager.HasEffectiveRight(user.Name, "Dataset", typeof(Dataset), id, RightType.Read))
                    {
                        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                        OutputDataManager ioOutputDataManager = new OutputDataManager();

                        Dataset dataset = datasetManager.GetDataset(id);

                        // If the requested version is -1 or the last version of the dataset, then the data will be loaded in a
                        // different way than when loading the data from an older version
                        bool isLatestVersion = false;
                        if (version == -1 || dataset.Versions.Count == version) isLatestVersion = true;

                        if (isLatestVersion)
                        {
                            #region get data from the latest version of a dataset

                            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);

                            string title = xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);

                            // check the data sturcture type ...
                            if (datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                            {
                                //FilterExpression filter = null;
                                //OrderByExpression orderBy = null;
                                //ProjectionExpression projectionExpression = GetProjectionExpression(projection);

                                // apply selection and projection
                                long count = datasetManager.RowCount(id);

                                DataTable dt = datasetManager.GetLatestDatasetVersionTuples(id, null, null, null, 0, (int)count);
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

                                dt.TableName = id + "_data";

                                DatasetModel model = new DatasetModel();
                                model.DataTable = dt;

                                var response = Request.CreateResponse();
                                response.Content = new ObjectContent(typeof(DatasetModel), model, new DatasetModelCsvFormatter(model.DataTable.TableName));
                                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

                                //set headers on the "response"
                                return response;

                                #endregion get data from the latest version of a dataset

                                //return model;
                            }
                            else
                            {
                                return Request.CreateResponse();
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

                            string title = xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title);

                            // check the data sturcture type ...
                            if (datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                            {
                                //FilterExpression filter = null;
                                //OrderByExpression orderBy = null;

                                // apply selection and projection
                                int count = datasetManager.GetDatasetVersionEffectiveTuples(datasetVersion).Count;
                                DataTable dt = datasetManager.GetDatasetVersionTuples(datasetVersion.Id, 0, count);

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

                                dt.TableName = id + "_data";

                                DatasetModel model = new DatasetModel();
                                model.DataTable = dt;

                                var response = Request.CreateResponse();
                                response.Content = new ObjectContent(typeof(DatasetModel), model, new DatasetModelCsvFormatter(model.DataTable.TableName));
                                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

                                //set headers on the "response"
                                return response;
                            }
                            else // return files of the unstructure dataset
                            {
                                return Request.CreateResponse();
                            }

                            #endregion load data of a older version of a dataset
                        }
                    }
                    else // has rights?
                    {
                        var request = Request.CreateResponse();
                        request.Content = new StringContent("User has no read right.");

                        return request;
                    }
                }
                else
                {
                    var request = Request.CreateResponse();
                    request.Content = new StringContent("User is not available.");

                    return request;
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

        private ProjectionExpression GetProjectionExpression(string projection)
        {
            ProjectionExpression pe = new ProjectionExpression();

            string[] columns = projection.Split(',');

            foreach (string c in columns)
            {
                if (!string.IsNullOrEmpty(c))
                {
                    pe.Items.Add(new ProjectionItemExpression() { FieldName = c });
                }
            }

            return pe;
        }
    }
}