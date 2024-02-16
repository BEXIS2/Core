using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Ddm.UI.Helpers;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using BExIS.Utils.NH.Querying;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.UI.WebControls;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Ddm.UI.Controllers
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
    public class DataTableController : ApiController
    {



        /// <summary>

        /// </summary>
        /// <returns></returns>
        /// <remarks> 
        /// </remarks>
        [BExISApiAuthorize]
        //[Route("api/Data")]
        [GetRoute("api/DataTable")]
        [HttpGet]
        public HttpResponseMessage Get(DataTableSendModel gridcommand)
        {
            if (gridcommand.Id <= 0)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Id should be greater then 0");

            // filter data

            // get missing values 
            return getData(gridcommand);



        }
        private HttpResponseMessage getData(DataTableSendModel gridcommand)
        {
            long id = gridcommand.Id;
            long versionId = gridcommand.Version;
            int pageNumber = gridcommand.Offset / gridcommand.Limit;
            int pageSize = gridcommand.Limit;

            DataTableRecieveModel recieveModel = new DataTableRecieveModel();
            recieveModel.SendModel = gridcommand;

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

                User user = ControllerContext.RouteData.Values["user"] as User;


                if (!isPublic && user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "No access to data.");
                }

                if (isPublic || user != null)
                {
                    if (isPublic || entityPermissionManager.HasEffectiveRight(user.Name, typeof(Dataset), id, RightType.Read))
                    {
                        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                        OutputDataManager ioOutputDataManager = new OutputDataManager();

                        Dataset dataset = datasetManager.GetDataset(id);
                        if (dataset == null) return Request.CreateResponse(HttpStatusCode.PreconditionFailed, "dataset " + id + " not exist.");

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
                            if (datasetVersion.Dataset.DataStructure != null)
                            {
                                // currently the primary data is loaded from the matView, for whatever reason the variables are
                                // defined with id as column name and not with the direct name of the columns.
                                // Therefore, there must be a transformation so that the query matches the columns.
                                // e.g.ScientificName - var61
                                StructuredDataStructure sds = (StructuredDataStructure)dataset.DataStructure.Self;
                                var varsAsKVP = DataTableHelper.variablesAsKVP(sds.Variables);

                                FilterExpression filter = DataTableHelper.ConvertTo(gridcommand.Filter,varsAsKVP);
                                OrderByExpression orderBy = DataTableHelper.ConvertTo(gridcommand.OrderBy,varsAsKVP);

                                // apply selection and projection
                                long count = datasetManager.GetDataTuplesCount(datasetVersion.Id);

                                if (count > 0)
                                {
                                    dt = datasetManager.GetLatestDatasetVersionTuples(id, filter, orderBy, null, pageNumber, pageSize);
                                    dt.Strip();

                                    // replace column name to caption
                                    for (int i = 0; i < dt.Columns.Count; i++)
                                    {
                                        var c = dt.Columns[i];
                                        c.ColumnName = c.Caption;
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

                                    dt = datasetManager.GetDatasetVersionTuples(datasetVersion.Id, pageNumber, pageSize);

                                    dt.Strip();

                                    //if (!string.IsNullOrEmpty(selection))
                                    //{
                                    //    dt = OutputDataManager.SelectionOnDataTable(dt, selection);
                                    //}

                                    //if (!string.IsNullOrEmpty(projection))
                                    //{
                                    //    // make the header names upper case to make them case insensitive
                                    //    dt = OutputDataManager.ProjectionOnDataTable(dt, projection.ToUpper().Split(','));
                                    //}
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

                            recieveModel.Data = dt;

                            var response = Request.CreateResponse();

                            string json = JsonConvert.SerializeObject(recieveModel);


                            response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                            //response.Content = new ObjectContent(typeof(DatasetModel), model, new DatasetModelCsvFormatter(model.DataTable.TableName));
                            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                            

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

        public string DataTableToJSONWithJSONNet(DataTable table)
        {
            string JSONString = string.Empty;
            JSONString = JsonConvert.SerializeObject(table);
            return JSONString;
        }

    }
}