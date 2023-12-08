using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.NH.Querying;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

//using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dim.UI.Controllers
{

    public class DataStatisticOutController : ApiController
    {

        // GET api/DataStatistic
        /// <summary>
        /// Get a list of all dataset ids
        /// </summary>
        /// <returns>List of ids</returns>
        [BExISApiAuthorize]
        [GetRoute("api/DataStatistic")]
        public List<int> Get()
        {
            List<int> structuredIds = new List<int>();
            DatasetManager dm = new DatasetManager();
            try
            {
                var datasetIds = dm.GetDatasetIds();

                foreach (int id in datasetIds)
                {
                    DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(id);
                    if (datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                    {
                        structuredIds.Add(id);
                    }
                }
                return structuredIds;
            }
            finally
            {
                dm.Dispose();
            }
        }


        // GET api/DataStatistic/{id}
        /// <summary>
        /// Get unique values, max and min values, and max and min length for all variables
        /// </summary>
        /// <param name="id">Dataset Id</param>
        [BExISApiAuthorize]
        [GetRoute("api/DataStatistic/{id}")]
        [ResponseType(typeof(ApiDataStatisticModel))]
        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            string token = this.Request.Headers.Authorization?.Parameter;

            return getData(id, -1, token);
        }


        // GET api/DataStatistic/{id}/{variableId}
        /// <summary>
        /// Get unique values, max and min values, and max and min length for one variable
        /// </summary>
        /// <param name="id">Dataset Id</param>
        /// <param name="variableId">Variable id</param>
        /// <returns></returns>
        [BExISApiAuthorize]
        [GetRoute("api/DataStatistic/{id}/{variableId}")]
        [ResponseType(typeof(ApiDataStatisticModel))]
        [HttpGet]
        public HttpResponseMessage Get(long id, int variableId)
        {

            string token = this.Request.Headers.Authorization?.Parameter;

            return getData(id, variableId, token);
        }

        private HttpResponseMessage getData(long id, int variableId, string token)
        {
            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            EntityManager entityManager = new EntityManager();
            DataStructureManager dataStructureManager = null;

            bool isPublic = false;
            try
            {
                // if a dataset is public, then the api should also return data if there is no token for a user

                #region is public
                dataStructureManager = new DataStructureManager();

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
                    if (isPublic || entityPermissionManager.HasEffectiveRight(user.Name, typeof(Dataset), id, RightType.Read))
                    {
                        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                        OutputDataManager ioOutputDataManager = new OutputDataManager();

                        Dataset dataset = datasetManager.GetDataset(id);
                        DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);

                        string title = datasetVersion.Title;

                        // check the data sturcture type ...
                        if (datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
                        {
                            object stats = new object();

                            DataTable dt = new DataTable("Varibales");


                            List<ApiDataStatisticModel> dataStatisticModels = new List<ApiDataStatisticModel>();
                            StructuredDataStructure structuredDataStructure = dataStructureManager.StructuredDataStructureRepo.Get(datasetVersion.Dataset.DataStructure.Id);
                            if (variableId == -1)
                            {

                                foreach (VariableInstance vs in structuredDataStructure.Variables)
                                {
                                    ApiDataStatisticModel dataStatisticModel = new ApiDataStatisticModel();
                                    dt = GetUniqueValues(id, vs.Id);
                                    dataStatisticModel.VariableId = vs.Id;
                                    dataStatisticModel.VariableName = vs.Label;
                                    dataStatisticModel.VariableDescription = vs.Description;
                                    dataStatisticModel.DataTypeName = vs.DataType.Name;
                                    dataStatisticModel.DataTypeSystemType = vs.DataType.SystemType;

                                    DataTypeDisplayPattern dtdp = vs.DisplayPatternId>0?DataTypeDisplayPattern.Get(vs.DisplayPatternId):null;
                                    string displayPattern = "";
                                    if (dtdp != null) displayPattern = dtdp.StringPattern;

                                    dataStatisticModel.DataTypeDisplayPattern = displayPattern;
                                    dataStatisticModel.Unit = vs.Unit.Name;

                                    dataStatisticModel.uniqueValues = dt;
                                    dataStatisticModel.minLength = dt.Compute("Min(length)", string.Empty).ToString();
                                    dataStatisticModel.maxLength = dt.Compute("Max(length)", string.Empty).ToString();
                                    dataStatisticModel.count = dt.Compute("Sum(count)", string.Empty).ToString();

                                    DataTable dtMissingValues = new DataTable("MissingValues");
                                    dtMissingValues.Columns.Add("placeholder", typeof(String));
                                    dtMissingValues.Columns.Add("displayName", typeof(String));

                                    foreach (var missingValue in vs.MissingValues)
                                    {
                                        DataRow workRow = dtMissingValues.NewRow();
                                        workRow["placeholder"] = missingValue.Placeholder;
                                        workRow["displayName"] = missingValue.DisplayName;
                                        dtMissingValues.Rows.Add(workRow);
                                    }
                                    dataStatisticModel.min = GetMin(dtMissingValues, dt);
                                    dataStatisticModel.max = GetMax(dtMissingValues, dt);
                                    dataStatisticModel.missingValues = dtMissingValues;
                                    dataStatisticModels.Add(dataStatisticModel);
                                }
                            }
                            else
                            {

                                VariableInstance variable = new VariableInstance();

                                foreach (VariableInstance vs in structuredDataStructure.Variables)
                                {
                                    if (vs.Id == variableId)
                                    {
                                        variable = vs;
                                    }
                                }

                                ApiDataStatisticModel dataStatisticModel = new ApiDataStatisticModel();
                                dt = GetUniqueValues(id, variableId);
                                dataStatisticModel.VariableId = variableId;
                                dataStatisticModel.uniqueValues = dt;

                                dataStatisticModel.minLength = dt.Compute("Min(length)", string.Empty).ToString();
                                dataStatisticModel.maxLength = dt.Compute("Max(length)", string.Empty).ToString();
                                dataStatisticModel.count = dt.Compute("Sum(count)", string.Empty).ToString();

                                DataTable dtMissingValues = new DataTable("MissingValues");
                                dtMissingValues.Columns.Add("placeholder", typeof(String));
                                dtMissingValues.Columns.Add("displayName", typeof(String));

                                foreach (var missingValue in variable.MissingValues)
                                {
                                    DataRow workRow = dtMissingValues.NewRow();
                                    workRow["placeholder"] = missingValue.Placeholder;
                                    workRow["displayName"] = missingValue.DisplayName;
                                    dtMissingValues.Rows.Add(workRow);
                                }
                                dataStatisticModel.min = GetMin(dtMissingValues, dt);
                                dataStatisticModel.max = GetMax(dtMissingValues, dt);
                                dataStatisticModel.missingValues = dtMissingValues;
                                dataStatisticModels.Add(dataStatisticModel);
                            }
                            dt.Strip();


                            dt.TableName = id + "_data";

                            DatasetModel model = new DatasetModel();
                            model.DataTable = dt;

                            var response = Request.CreateResponse(HttpStatusCode.OK);
                            string resp = JsonConvert.SerializeObject(dataStatisticModels);

                            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                            return response;

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
                entityManager.Dispose();
                dataStructureManager.Dispose();
            }
        }

        private static ProjectionExpression GetProjectionExpression(string projection)
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

        private long Count(long datasetId)
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine(string.Format("SELECT COUNT(id) AS cnt FROM {0};", this.BuildName(datasetId).ToLower()));
            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetBulkUnitOfWork())
                {
                    var result = uow.ExecuteScalar(mvBuilder.ToString());
                    return (long)result;
                }
            }
            catch
            {
                return -1;
            }
        }

        private DataTable GetUniqueValues(long datasetId, long variableId)
        {
            StringBuilder mvBuilder = new StringBuilder();
            mvBuilder.AppendLine($"SELECT var{variableId} as var, count(id), length(var{variableId}::text) FROM {BuildName(datasetId).ToLower()} group by var{variableId} order by var;");
            // execute the statement
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var result = uow.ExecuteQuery(mvBuilder.ToString());
                    return result;
                }
            }
            catch
            {
                return null;
            }
        }

        private string BuildName(long datasetId)
        {
            return "mvDataset" + datasetId;
        }

        private string GetMin(DataTable missingValues, DataTable dt)
        {
            for (var i = 0; i < dt.Rows.Count - 1; i++)
            {
                DataRow[] found = missingValues.Select("placeholder = '" + dt.Rows[i][0] + "'");
                if (found.Length == 0)
                {
                    return dt.Rows[i][0].ToString();
                }
            }

            return dt.Rows[0][0].ToString();
        }

        private string GetMax(DataTable missingValues, DataTable dt)
        {

            for (var i = dt.Rows.Count - 1; i > 0; i--)
            {
                DataRow[] found = missingValues.Select("placeholder = '" + dt.Rows[i][0] + "'");
                if (found.Length == 0 && !string.IsNullOrEmpty(dt.Rows[i][0].ToString()))
                {
                    return dt.Rows[i][0].ToString();
                }
            }

            return dt.Rows[dt.Rows.Count - 1][0].ToString();
        }

    }

}
