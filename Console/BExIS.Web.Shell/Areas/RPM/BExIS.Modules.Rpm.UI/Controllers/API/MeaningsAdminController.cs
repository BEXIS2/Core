using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
using BExIS.Utils.Route;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BExIS.Modules.Rpm.UI.Api.Controllers
{
    public class MeaningsAdminController : ApiController
    {

        private readonly ImeaningManagr _meaningManager;
        public MeaningsAdminController(ImeaningManagr _meaningManager)
        {
            this._meaningManager = _meaningManager;
        }
        public MeaningsAdminController()
        {
            if (this._meaningManager == null)
               this._meaningManager = new meaningManager();
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost]
        [PostRoute("api/MeaningsAdmin/Create")]
        public HttpResponseMessage Create(Meaning data)
        {
            try
            {

                Meaning res = _meaningManager.addMeaning(data);
                return (cretae_response(res));
            }
            catch
            {
                return (cretae_response((Meaning)null));
            }
        }


        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost]
        [PostRoute("api/MeaningsAdmin/EditMeaning")]
        public HttpResponseMessage EditMeaning(Meaning data)
        {
            Meaning m = null;
            try
            {
                Meaning res = _meaningManager.editMeaning(data);
                return (cretae_response(res));
            }
            catch
            {
                return (cretae_response((Meaning)null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpDelete,HttpPost]
        [DeleteRoute("api/MeaningsAdmin/Delete")]
        [PostRoute("api/MeaningsAdmin/Delete")]
        public HttpResponseMessage Delete( long id)
        {
            try
            {
                return (cretae_response(_meaningManager.deleteMeaning(id))); 
            }
            catch
            {
                return cretae_response((Meaning)null);
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpGet]
        [GetRoute("api/MeaningsAdmin/getVariables")]
        public HttpResponseMessage getVariables()
        {
            using (DataStructureManager dsm = new DataStructureManager())
            {
                List<Variable> variables = (List<Variable>)dsm.VariableRepo.Get();
                return cretae_response(variables);
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost]
        [PostRoute("api/MeaningsAdmin/updateRelatedManings")]
        public HttpResponseMessage updateRelatedManings (HttpRequestMessage request)
        {
            try
            {
                String parentID = Convert.ToString(HttpContext.Current.Request.Form["parentID"]);
                String childID = Convert.ToString(HttpContext.Current.Request.Form["childID"]);
                return cretae_response(_meaningManager.updateRelatedManings(parentID, childID)); 
            }
            catch(Exception exc)
            {
                return null;
            }
        }


        // external links endpoints

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost, HttpGet]
        [PostRoute("api/MeaningsAdmin/createExternalLink")]
        [GetRoute("api/MeaningsAdmin/createExternalLink")]
        public HttpResponseMessage createExternalLink(ExternalLink data)
        {
            try
            {
                ExternalLink res = _meaningManager.addExternalLink(data);
                return (cretae_response(_meaningManager.addExternalLink(data)));
            }
            catch
            {
                return (cretae_response(null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost, HttpGet]
        [PostRoute("api/MeaningsAdmin/editExternalLinks")]
        [GetRoute("api/MeaningsAdmin/editExternalLinks")]
        public HttpResponseMessage editExternalLinks(ExternalLink data)
        {
            ExternalLink m = null;
            try
            {

                ExternalLink res = _meaningManager.editExternalLink(data);
                return (cretae_response(res));
            }
            catch (Exception ex)
            {
                return (cretae_response(null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost, HttpGet]
        [PostRoute("api/MeaningsAdmin/deleteExternalLinks")]
        [GetRoute("api/MeaningsAdmin/deleteExternalLinks")]
        public HttpResponseMessage deleteExternalLinks(HttpRequestMessage request)
        {
            try
            {
                string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
                return cretae_response(_meaningManager.deleteExternalLink(Int64.Parse(id))); 
            }
            catch
            {
                return (cretae_response(null));
            }
        }

        private HttpResponseMessage cretae_response(Object return_object)
        {
            if (return_object == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "bad request / problem occured");
            var response = Request.CreateResponse(HttpStatusCode.OK);
            string resp = JsonConvert.SerializeObject(return_object);

            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //set headers on the "response"
            return response;
        }
        private HttpResponseMessage cretae_response(List<Object> return_object)
        {
            if (return_object == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "bad request / problem occured");
            var response = Request.CreateResponse(HttpStatusCode.OK);
            string resp = JsonConvert.SerializeObject(return_object);

            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //set headers on the "response"
            return response;
        }
    }
}
