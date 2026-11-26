using BExIS.App.Bootstrap.Attributes;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Utils.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BExIS.Modules.Sam.UI.Controllers.API
{
    public class EntityPermissionsController : ApiController
    {
        public EntityPermissionsController() { }

        [BExISApiAuthorize, HttpGet, GetRoute("api/entityPermissions/{id}")]
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

        [BExISApiAuthorize, HttpGet, GetRoute("api/entityPermissions")]
        public async Task<HttpResponseMessage> GetAsync()
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

        [BExISApiAuthorize, HttpPost, PostRoute("api/entityPermissions")]
        public async Task<HttpResponseMessage> PostAsync(CreateEntityPermissionModel model)
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


        [BExISApiAuthorize, HttpPut, PutRoute("api/entityPermissions/{id}")]
        public async Task<HttpResponseMessage> PutByIdAsync(long id, UpdateEntityPermissionModel model)
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


        [BExISApiAuthorize, HttpDelete, DeleteRoute("api/entityPermissions/{id}")]
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