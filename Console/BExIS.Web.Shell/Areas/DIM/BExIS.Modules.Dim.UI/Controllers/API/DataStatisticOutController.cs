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
using BExIS.Utils.Data;
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
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Vaiona.Web.Mvc.Modularity;


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
                    if (datasetVersion.Dataset.DataStructure != null &&
                        datasetVersion.Dataset.DataStructure is StructuredDataStructure)
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

        private HttpResponseMessage getData(long id, int variableId, string token, double tag = 0)
        {
            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            EntityManager entityManager = new EntityManager();
            DataStructureManager dataStructureManager = null;

            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            bool useTags = (bool)moduleSettings.GetValueByKey("use_tags");

            bool isPublic = false;
            try
            {
                // if a dataset is public, then the api should also return data if there is no token for a user

                #region is public

                dataStructureManager = new DataStructureManager();

                long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;

                isPublic = entityPermissionManager.ExistsAsync(entityTypeId.Value, id).Result;

                #endregion is public

                if (!isPublic && String.IsNullOrEmpty(token))

                {
                    var request = Request.CreateResponse();
                    request.Content = new StringContent("Bearer token not exist.");

                    return request;
                }

                User user = ControllerContext.RouteData.Values["user"] as User;

                if (isPublic || user != null)
                {
                    if (isPublic || entityPermissionManager.HasEffectiveRightsAsync(user.Name, typeof(Dataset), id, RightType.Read).Result)
                    {
                        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                        OutputDataManager ioOutputDataManager = new OutputDataManager();
                       
                        DatasetVersion datasetVersion = getDatasetVersion(id, datasetManager).Result;

                        int count = datasetManager.GetDatasetVersionEffectiveTuples(datasetVersion).Count;
                        if (count > 0)
                        {
                            string title = datasetVersion.Title;

                            // check the data sturcture type ...
                            if (datasetVersion.Dataset.DataStructure != null && datasetVersion.Dataset.DataStructure.Self is StructuredDataStructure)
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
                                        dt = GetUniqueValues(datasetVersion, vs.Label, count, vs.Id);
                                        dataStatisticModel.VariableId = vs.Id;
                                        dataStatisticModel.VariableName = vs.Label;
                                        dataStatisticModel.VariableDescription = vs.Description;
                                        dataStatisticModel.DataTypeName = vs.DataType.Name;
                                        dataStatisticModel.DataTypeSystemType = vs.DataType.SystemType;

                                        DataTypeDisplayPattern dtdp = vs.DisplayPatternId > 0 ? DataTypeDisplayPattern.Get(vs.DisplayPatternId) : null;
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
                                    dt = GetUniqueValues(datasetVersion, variable.Label, count, variable.Id); ;
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
                                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "There is no structured data for the dataset.");
                            }
                        }
                        else
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "There is no data for the dataset version.");

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

        private async Task<DatasetVersion> getDatasetVersion(long datasetId, DatasetManager datasetManager, int versionNr= 0)
        {
            string username = GetUsernameOrDefault();

            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            bool useTags = false;
            bool.TryParse(moduleSettings.GetValueByKey("use_tags").ToString(), out useTags);

            // get version based on situtaion / tags, versions, etc.    
            long versionId = await DatasetVersionHelper.GetVersionId(datasetId, username, versionNr, useTags);
            DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(versionId);

            return datasetVersion;
        }

        public string GetUsernameOrDefault()
        {
            var username = string.Empty;
            try
            {
                var user = ControllerContext.RouteData.Values["user"] as User;
                username = user.UserName;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        private DataTable GetUniqueValues(DatasetVersion datasetVersion, string variableName, int count, long variableId)
        {
            using (var datasetManager = new DatasetManager())
            {
                DataTable dt = datasetManager.GetDatasetVersionTuples(datasetVersion.Id, 0, count);
                dt.Strip(); 

                dt = OutputDataManager.ProjectionOnDataTable(dt, variableName.ToUpper().Split(','));

                string requested = "var"+ variableId;
                string targetCol = dt.Columns.Cast<DataColumn>()
                    .Where(c => string.Equals(c.ColumnName, requested, StringComparison.OrdinalIgnoreCase))
                    .Select(c => c.ColumnName)
                    .FirstOrDefault()
                    ?? throw new ArgumentException(
                         $"Spalte '{requested}' nicht in Tabelle. Vorhanden: " +
                         string.Join(", ", dt.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));

                bool trim = true;
                var cmp = StringComparer.OrdinalIgnoreCase;

                var query =
                    dt.Rows.Cast<DataRow>()
                      .Select(r =>
                      {
                          var v = r[targetCol];
                          string s = (v == null || v == DBNull.Value) ? null : v.ToString();
                          return (trim && s != null) ? s.Trim() : s;
                      })
                      .GroupBy(s => s, cmp)
                      .Select(g => new { Value = g.Key, Count = g.Count(), Length = (g.Key ?? string.Empty).Length })
                      .OrderBy(x => x.Value);

                // 4) Ergebnis bauen
                var result = new DataTable();
                result.Columns.Add(targetCol, typeof(string));
                result.Columns.Add("Count", typeof(int));
                result.Columns.Add("Length", typeof(int));

                foreach (var x in query)
                    result.Rows.Add(x.Value, x.Count, x.Length);

                return result;
            }
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