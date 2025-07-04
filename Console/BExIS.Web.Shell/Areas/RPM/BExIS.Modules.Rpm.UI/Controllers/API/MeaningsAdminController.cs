﻿using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
using BExIS.Utils.Route;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace BExIS.Modules.Rpm.UI.Api.Controllers
{
    public class MeaningsAdminController : ApiController
    {
        private readonly MeaningManager _meaningManager;

        public MeaningsAdminController(MeaningManager _meaningManager)
        {
            this._meaningManager = _meaningManager;
        }

        public MeaningsAdminController()
        {
            if (this._meaningManager == null)
                this._meaningManager = new MeaningManager();
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
                Meaning res = _meaningManager.AddMeaning(data);
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
                Meaning res = _meaningManager.EditMeaning(data);
                return (cretae_response(res));
            }
            catch
            {
                return (cretae_response((Meaning)null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpDelete, HttpPost]
        [DeleteRoute("api/MeaningsAdmin/Delete")]
        [PostRoute("api/MeaningsAdmin/Delete")]
        public HttpResponseMessage Delete(long id)
        {
            try
            {
                return (cretae_response(_meaningManager.DeleteMeaning(id)));
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
        public HttpResponseMessage updateRelatedManings(HttpRequestMessage request)
        {
            try
            {
                String parentID = Convert.ToString(HttpContext.Current.Request.Form["parentID"]);
                String childID = Convert.ToString(HttpContext.Current.Request.Form["childID"]);
                return cretae_response(_meaningManager.UpdateRelatedManings(parentID, childID));
            }
            catch (Exception exc)
            {
                return null;
            }
        }

        #endregion meanings

        #region external links

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost, PostRoute("api/MeaningsAdmin/createExternalLink")]
        public HttpResponseMessage createExternalLink(ExternalLink data)
        {
            try
            {
                ExternalLink res = _meaningManager.AddExternalLink(data);
                return (cretae_response(_meaningManager.AddExternalLink(data)));
            }
            catch
            {
                return (cretae_response(null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPut, PutRoute("api/MeaningsAdmin/editExternalLinks")]
        public HttpResponseMessage editExternalLinks(ExternalLink data)
        {
            ExternalLink m = null;
            try
            {
                ExternalLink res = _meaningManager.EditExternalLink(data);
                return (cretae_response(res));
            }
            catch (Exception ex)
            {
                return (cretae_response(null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [DeleteRoute("api/MeaningsAdmin/deleteExternalLinks")]
        public HttpResponseMessage deleteExternalLinks(long id)
        {
            try
            {
                Int64.TryParse(this.Request.Content.ReadAsStringAsync().Result.ToString(), out id);
                return cretae_response(_meaningManager.DeleteExternalLink(id));
            }
            catch
            {
                return (cretae_response(null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPut, PutRoute("api/MeaningsAdmin/updatePreviousLinks")]
        public HttpResponseMessage updatePreviousLinks()
        {
            return cretae_response(_meaningManager.UpdatePreviousLinks());
        }

        #endregion external links

        #region PrefixCategory

        // external links endpoints

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost, PostRoute("api/MeaningsAdmin/createPrefixCategory")]
        public HttpResponseMessage createPrefixCategory(PrefixCategory data)
        {
            try
            {
                PrefixCategory res = _meaningManager.AddPrefixCategory(data);
                return (cretae_response(res));
            }
            catch
            {
                return (cretae_response(null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPut, PutRoute("api/MeaningsAdmin/editPrefixCategory")]
        public HttpResponseMessage editPrefixCategory(PrefixCategory data)
        {
            ExternalLink m = null;
            try
            {
                PrefixCategory res = _meaningManager.EditPrefixCategory(data);
                return (cretae_response(res));
            }
            catch (Exception ex)
            {
                return (cretae_response(null));
            }
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpDelete, DeleteRoute("api/MeaningsAdmin/deletePrefixCategory")]
        public HttpResponseMessage deletePrefixCategory(long id)
        {
            try
            {
                return cretae_response(_meaningManager.DeletePrefixCategory(id));
            }
            catch
            {
                return (cretae_response(null));
            }
        }

        #endregion PrefixCategory
    }
}