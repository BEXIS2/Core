using BExIS.App.Bootstrap.Attributes;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Vaelastrasz.Library.Models;

namespace BExIS.Modules.Dim.UI.Controllers.API
{
    public class DataCiteController : ApiController
    {
        public DataCiteController() { }

        [BExISApiAuthorize, HttpGet, GetRoute("api/dataCite")]
        public async Task<HttpResponseMessage> GetAsync()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.NotImplemented, "DataCite API is not implemented yet!");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [BExISApiAuthorize, HttpGet, GetRoute("api/dataCite/{id}")]
        public async Task<HttpResponseMessage> GetByIdAsync(long id)
        {
            try
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [BExISApiAuthorize, HttpPost, PostRoute("api/dataCite")]
        public async Task<HttpResponseMessage> PostAsync(CreateDataCiteModel model)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.NotImplemented, "DataCite API is not implemented yet!");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [BExISApiAuthorize, HttpPut, PutRoute("api/dataCite/{id}")]
        public async Task<HttpResponseMessage> PutByIdAsync(long id, UpdateDataCiteModel model)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.NotImplemented, "DataCite API is not implemented yet!");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [BExISApiAuthorize, HttpDelete, DeleteRoute("api/dataCite/{id}")]
        public async Task<HttpResponseMessage> DeleteByIdAsync(long id)
        {
            try
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotImplemented);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}