using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dim.UI.Helper.API;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Modules.Dim.UI.Models.API;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
//using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

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
    public class DataController : ApiController
    {

        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();


        // GET: api/data
        /// <summary>
        /// Get a list of all dataset ids
        /// </summary>
        /// <returns>List of ids</returns>
        [BExISApiAuthorize]
        public IEnumerable<long> Get()
        {
            DatasetManager dm = new DatasetManager();
            try
            {
                var datasetIds = dm.GetDatasetLatestIds();
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
        /// <returns></returns>
        /// <remarks> The action accepts the following additional parameters via the query string
        /// 1: header: is a comman separated list of ids that determines which variables of the dataset version tuples should take part in the result set
        /// 2: filter: is a logical expression that filters the tuples of the chosen dataset. The expression should have been written against the variables of the dataset only.
        /// logical operators, nesting, precedence, and SOME functions should be supported.
        /// </remarks>
        [BExISApiAuthorize]
        public HttpResponseMessage Get(int id, [FromUri] string header = null, [FromUri] string filter = null)
        {
            string projection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "header".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string selection = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => "filter".Equals(p.Key, StringComparison.InvariantCultureIgnoreCase)).Value;
            string token = this.Request.Headers.Authorization?.Parameter;

            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

            try
            {

                if (String.IsNullOrEmpty(token))
                {
                    var request = Request.CreateResponse();
                    request.Content = new StringContent("Bearer token not exist.");

                    return request;

                }

                User user = userManager.Users.Where(u => u.Token.Equals(token)).FirstOrDefault();

                if (user != null)
                {

                    if (entityPermissionManager.HasEffectiveRight(user.Name, "Dataset", typeof(Dataset), id, RightType.Read))
                    {
                        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                        OutputDataManager ioOutputDataManager = new OutputDataManager();

                        DatasetVersion version = datasetManager.GetDatasetLatestVersion(id);

                        string title = xmlDatasetHelper.GetInformationFromVersion(version.Id, NameAttributeValues.title);

                        // check the data sturcture type ...
                        if (version.Dataset.DataStructure.Self is StructuredDataStructure)
                        {
                            //FilterExpression filter = null;
                            //OrderByExpression orderBy = null;

                            // apply selection and projection
                            long count = datasetManager.RowCount(id);


                            DataTable dt = datasetManager.GetLatestDatasetVersionTuples(id, null, null, null, 0, (int)count);
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

                            if (dt.TableName == "") dt.TableName = id + "_data";


                            DatasetModel model = new DatasetModel();
                            model.DataTable = dt;


                            var response = Request.CreateResponse();
                            response.Content = new ObjectContent(typeof(DatasetModel), model, new DatasetModelCsvFormatter(model.DataTable.TableName));
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");


                            //set headers on the "response"
                            return response;

                            //return model;


                        }
                        else
                        {
                            return Request.CreateResponse();
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

            }
        }

        // POST: api/data
        /// <summary>
        /// Create a new dataset!!!
        /// </summary>
        /// <param name="data"></param>
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<HttpResponseMessage> Post([FromBody]PushDataModel data)
        {
            var request = Request.CreateResponse();
            User user = null;
            string error = "";

            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            DataStructureManager dataStructureManager = new DataStructureManager();

            DatasetVersion workingCopy = new DatasetVersion();
            List<DataTuple> rows = new List<DataTuple>();



            try
            {
                #region security

                string token = this.Request.Headers.Authorization?.Parameter;

                if (String.IsNullOrEmpty(token))
                {
                    request.Content = new StringContent("Bearer token not exist.");

                    return request;

                }

                user = userManager.Users.Where(u => u.Token.Equals(token)).FirstOrDefault();

                if (user == null)
                {
                    request.Content = new StringContent("Token is not valid.");

                    return request;
                }


                //check permissions

                //entity permissions
                if (data.DatasetId > 0)
                {

                    Dataset d = datasetManager.GetDataset(data.DatasetId);
                    if (d == null)
                    {
                        request.Content = new StringContent("the dataset with the id (" + data.DatasetId + ") does not exist.");

                        return request;
                    }

                    if (!entityPermissionManager.HasEffectiveRight(user.Name, "Dataset", typeof(Dataset), data.DatasetId, RightType.Write))
                    {
                        request.Content = new StringContent("The token is not authorized to write into the dataset.");

                        return request;
                    }
                }


                #endregion

                #region incomming values check
                // check incomming values


                if (data.DatasetId == 0) error += "dataset id should be greater then 0.";
                //if (data.UpdateMethod == null) error += "update method is not set"; 
                if (data.Count == 0) error += "count should be greater then 0. ";
                if (data.Columns == null) error += "cloumns should not be null. ";
                if (data.Data == null) error += "data is empty. ";

                if (data.UpdateMethod == UpdateMethod.Update)
                {
                    if (data.PrimaryKeys == null || data.PrimaryKeys.Count() == 0) error += "the UpdateMethod update has been selected but there are no primary keys available. ";
                }

                if (!string.IsNullOrEmpty(error))
                {
                    request.Content = new StringContent(error);

                    return request;
                }

                #endregion

                #region async upload with big data
                // if dataste is not in the dataset
                Dataset dataset = datasetManager.GetDataset(data.DatasetId);
                if (dataset == null)
                {
                    request.Content = new StringContent("Dataset not exist.");

                    return request;
                }


                PushDataApiHelper helper = new PushDataApiHelper(dataset, user, data);
                Task.Run(() => helper.Run());

                #endregion

                request.Content = new StringContent("till now it works");

                Debug.WriteLine("end of api call");

                return request;
            }
            finally
            {
                datasetManager.Dispose();
                entityPermissionManager.Dispose();
                dataStructureManager.Dispose();
                userManager.Dispose();
            }
        }

        // PUT: api/data/5
        /// <summary>
        /// Updates an existing dataset
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        [ApiExplorerSettings(IgnoreApi = true)]
        public void Put(int id, [FromBody]string value)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // DELETE: api/data/5
        /// <summary>
        /// Deletes an existing dataset
        /// </summary>
        /// <param name="id"></param>
        [ApiExplorerSettings(IgnoreApi = true)]
        public void Delete(int id)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}
