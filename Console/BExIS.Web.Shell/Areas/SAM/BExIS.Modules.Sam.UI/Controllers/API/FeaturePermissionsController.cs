using BExIS.App.Bootstrap.Attributes;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using Microsoft.AspNet.Identity;
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
    public class FeaturePermissionsController : ApiController
    {
        public FeaturePermissionsController() { }


        [BExISApiAuthorize, HttpGet, GetRoute("api/featurePermissions/{id}")]
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

        [BExISApiAuthorize, HttpGet, GetRoute("api/featurePermissions")]
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

        [BExISApiAuthorize, HttpPost, PostRoute("api/featurePermissions")]
        public async Task<HttpResponseMessage> PostAsync(CreateFeaturePermissionModel model)
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



        [BExISApiAuthorize, HttpPut, PutRoute("api/featurePermissions/{id}")]
        public async Task<HttpResponseMessage> PutByIdAsync(long id, UpdateFeaturePermissionModel model)
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

        [BExISApiAuthorize, HttpDelete, DeleteRoute("api/featurePermissions/{id}")]
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