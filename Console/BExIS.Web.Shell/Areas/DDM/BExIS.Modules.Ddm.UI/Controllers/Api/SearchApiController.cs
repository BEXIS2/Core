using BExIS.App.Bootstrap.Attributes;
using BExIS.Ddm.Api;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.IO.Transform.Output;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using BExIS.Utils.Models;
using BExIS.Utils.NH.Querying;
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
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Telerik.Web.Mvc.Extensions;
using Telerik.Web.Mvc.UI;
using Vaiona.IoC;
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
    public class SearchApiController : ApiController
    {
        [BExISApiAuthorize]
        [System.Web.Http.HttpGet,GetRoute("api/SearchApi/getData/{command}")]
        public HttpResponseMessage getData(DataTableSendModel command)
        {
            long id = command.Id;
            long versionId = command.Version;
            int pageNumber = command.Offset / command.Limit;
            int pageSize = command.Limit;

            DataTableRecieveModel recieveModel = new DataTableRecieveModel();
            recieveModel.Send = command;

            if (id <= -1)
                return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Id should be greater then 0");
            if (id == 0)
            {
                try
                {
                    ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

                    provider.WorkingSearchModel.CriteriaComponent.Clear();
                    provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);


                    recieveModel.Count = provider.WorkingSearchModel.ResultComponent.Rows.ToList().Count();
                    recieveModel.Data = provider.WorkingSearchModel.ResultComponent.ConvertToDataTable();
                    recieveModel.Send = new DataTableSendModel();
                    recieveModel.Columns = new List<DataTableColumn>();


                    var response = Request.CreateResponse();

                    string json = JsonConvert.SerializeObject(recieveModel);
                    //new JsonSerializerSettings
                    //{
                    //    ContractResolver = new CamelCasePropertyNamesContractResolver()
                    //});

                    response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    //response.Content = new ObjectContent(typeof(DatasetModel), model, new DatasetModelCsvFormatter(model.DataTable.TableName));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    //set headers on the "response"
                    return response;
                }
                catch (Exception e)
                {
                    ModelState.AddModelError(String.Empty, e.Message);

                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message.ToString());
                }
            }
            return Request.CreateErrorResponse(HttpStatusCode.NotImplemented, " ---- ");
        }

        [BExISApiAuthorize]
        [System.Web.Http.HttpGet, GetRoute("api/SearchApi/getData")]
        public HttpResponseMessage getData()
        {

            DataTableRecieveModel recieveModel = new DataTableRecieveModel();
            recieveModel.Send = new DataTableSendModel();

            try
            {
                ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

                provider.WorkingSearchModel.CriteriaComponent.Clear();
                provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);


                recieveModel.Count = provider.WorkingSearchModel.ResultComponent.Rows.ToList().Count();
                recieveModel.Data = provider.WorkingSearchModel.ResultComponent.ConvertToDataTable();
                recieveModel.Send = new DataTableSendModel();
                recieveModel.Columns = new List<DataTableColumn>();


                var response = Request.CreateResponse();

                string json = JsonConvert.SerializeObject(recieveModel);
                //new JsonSerializerSettings
                //{
                //    ContractResolver = new CamelCasePropertyNamesContractResolver()
                //});

                response.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                //response.Content = new ObjectContent(typeof(DatasetModel), model, new DatasetModelCsvFormatter(model.DataTable.TableName));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                //set headers on the "response"
                return response;
            }
            catch (Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message.ToString());
            }
        }

    }
}