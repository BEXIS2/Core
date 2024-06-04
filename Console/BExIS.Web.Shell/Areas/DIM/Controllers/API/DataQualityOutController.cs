using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

//using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class DataQualityOutController : ApiController
    {
        // GET: api/data
        /// <summary>
        /// Get a list of all dataset ids
        /// </summary>
        /// <returns>List of ids</returns>
        [BExISApiAuthorize]
        [GetRoute("api/DataQuality")]
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

        /// <param name="id">Dataset Id</param>
        [BExISApiAuthorize]
        //[Route("api/Data")]
        [GetRoute("api/DataQuality/{id}")]
        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            string token = this.Request.Headers.Authorization?.Parameter;

            return getData(id, -1, token);
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

                User user = ControllerContext.RouteData.Values["user"] as User;

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
                            if (variableId == -1)
                            {
                                StructuredDataStructure structuredDataStructure = dataStructureManager.StructuredDataStructureRepo.Get(datasetVersion.Dataset.DataStructure.Id);
                                List<string> varIds = new List<string>();
                                foreach (Variable vs in structuredDataStructure.Variables)
                                {
                                    varIds.Add("var" + vs.Id);
                                }
                                dt = GetDuplicates(id, varIds);
                            }
                            else
                            {
                            }
                            //dt.Strip();

                            dt.TableName = id + "_data";

                            DatasetModel model = new DatasetModel();
                            model.DataTable = dt;

                            var response = Request.CreateResponse(HttpStatusCode.OK);
                            string resp = JsonConvert.SerializeObject(model);

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

        private DataTable GetDuplicates(long datasetId, List<string> variables)
        {
            StringBuilder mvBuilder = new StringBuilder();

            mvBuilder.AppendLine($"SELECT {string.Join(",", variables)} , count(*) FROM {BuildName(datasetId).ToLower()} group by {string.Join(",", variables)} HAVING count(*) > 1;");
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
    }
}