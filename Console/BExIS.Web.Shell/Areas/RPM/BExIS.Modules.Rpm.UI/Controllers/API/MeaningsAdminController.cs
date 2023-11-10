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

        #region meanings
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
        [HttpPost,HttpDelete]
        [PostRoute("api/MeaningsAdmin/Delete")]
        [DeleteRoute("api/MeaningsAdmin/Delete")]
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
        #endregion

        #region external links
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
        [HttpPost, HttpDelete]
        [PostRoute("api/MeaningsAdmin/deleteExternalLinks")]
        [DeleteRoute("api/MeaningsAdmin/deleteExternalLinks")]
        public HttpResponseMessage deleteExternalLinks(long id)
        {
            try
            {
                Int64.TryParse( this.Request.Content.ReadAsStringAsync().Result.ToString(), out id);
                return cretae_response(_meaningManager.deleteExternalLink(id));
            }
            catch
            {
                return (cretae_response(null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost, HttpDelete]
        [PostRoute("api/MeaningsAdmin/updatePreviousLinks")]
        [DeleteRoute("api/MeaningsAdmin/updatePreviousLinks")]
        public HttpResponseMessage updatePreviousLinks()
        {
            return cretae_response(_meaningManager.updatePreviousLinks());
        }
        #endregion

        #region PrefixCategory
        // external links endpoints

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost, HttpGet]
        [PostRoute("api/MeaningsAdmin/createPrefixCategory")]
        [GetRoute("api/MeaningsAdmin/createEPrefixCategory")]
        public HttpResponseMessage createPrefixCategory(PrefixCategory data)
        {
            try
            {
                PrefixCategory res = _meaningManager.addPrefixCategory(data);
                return (cretae_response(res));
            }
            catch
            {
                return (cretae_response(null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost, HttpGet]
        [PostRoute("api/MeaningsAdmin/editPrefixCategory")]
        [GetRoute("api/MeaningsAdmin/editPrefixCategory")]
        public HttpResponseMessage editPrefixCategory(PrefixCategory data)
        {
            ExternalLink m = null;
            try
            {

                PrefixCategory res = _meaningManager.editPrefixCategory(data);
                return (cretae_response(res));
            }
            catch (Exception ex)
            {
                return (cretae_response(null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost, HttpDelete]
        [PostRoute("api/MeaningsAdmin/deletePrefixCategory")]
        [DeleteRoute("api/MeaningsAdmin/deletePrefixCategory")]
        public HttpResponseMessage deletePrefixCategory(long id)
        {
            try
            {
                return cretae_response(_meaningManager.deletePrefixCategory(id));
            }
            catch
            {
                return (cretae_response(null));
            }
        }
        #endregion

    }
}
