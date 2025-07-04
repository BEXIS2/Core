﻿using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Services.Meanings;
using BExIS.Utils.Route;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class MeaningsController : ApiController
    {
        private readonly MeaningManager _meaningManager;

        public MeaningsController(MeaningManager _meaningManager)
        {
            this._meaningManager = _meaningManager;
        }

        public MeaningsController()
        {
            if (this._meaningManager == null)
                this._meaningManager = new MeaningManager();
        }

        [BExISAuthorize]
        [JsonNetFilter]
        [HttpGet, GetRoute("api/Meanings/Index")]
        public HttpResponseMessage Index()
        {
            return cretae_response(_meaningManager.GetMeanings());
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpGet, GetRoute("api/Meanings/Details")]
        public HttpResponseMessage Details()
        {
            string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(id);

            return cretae_response(_meaningManager.GetMeaning(long.Parse(dict["id"])));
        }

        public HttpResponseMessage getExternalLinks()
        {
            return cretae_response(_meaningManager.GetExternalLinks());
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpGet, GetRoute("api/Meanings/DetailExternalLinks")]
        public HttpResponseMessage DetailExternalLinks()
        {
            string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(id);

            return cretae_response(_meaningManager.GetExternalLink(long.Parse(dict["id"])));
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpGet, GetRoute("api/Meanings/getPrefixes")]
        public HttpResponseMessage getPrefixes()
        {
            return cretae_response(_meaningManager.GetPrefixes());
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpGet, GetRoute("api/Meanings/getPrefixfromUri")]
        public HttpResponseMessage getPrefixfromUri()
        {
            string uri = this.Request.Content.ReadAsStringAsync().Result.ToString();
            return cretae_response(_meaningManager.GetPrefixfromUri(uri));
        }

        private HttpResponseMessage cretae_response(object return_object)
        {
            if (return_object == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "bad request / problem occured");
            var response = Request.CreateResponse(HttpStatusCode.OK);

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string resp = JsonConvert.SerializeObject(return_object, serializerSettings);
            //string resp = JsonConvert.SerializeObject(return_object);

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

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            string resp = JsonConvert.SerializeObject(return_object, serializerSettings);
            //string resp = JsonConvert.SerializeObject(return_object);

            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            //set headers on the "response"
            return response;
        }
    }
}