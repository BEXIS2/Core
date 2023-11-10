using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Services.Meanings;
using BExIS.Utils.Route;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BExIS.Modules.Rpm.UI.Api.Controllers
{
    public class MeaningsController : ApiController
    {

        private readonly ImeaningManagr _meaningManager;
        public MeaningsController(ImeaningManagr _meaningManager)
        {
            this._meaningManager = _meaningManager;
        }
        public MeaningsController()
        {
            if (this._meaningManager == null)
               this._meaningManager = new meaningManager();
        }


        [BExISApiAuthorize]
        [JsonNetFilter]
        [HttpPost,HttpGet]
        [PostRoute("api/Meanings/Index")]
        [GetRoute("api/Meanings/Index")]
        public HttpResponseMessage Index()
        {
            return cretae_response( _meaningManager.getMeanings());
        }

        [BExISApiAuthorize]
        [HttpPost,HttpGet]
        [JsonNetFilter]
        [PostRoute("api/Meanings/Details")]
        [GetRoute("api/Meanings/Details")]
        public HttpResponseMessage Details()
        {
            string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(id);

            return cretae_response(_meaningManager.getMeaning(long.Parse(dict["id"])));
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [JsonNetFilter]
        [PostRoute("api/Meanings/getExternalLinks")]
        [GetRoute("api/Meanings/getExternalLinks")]
        public HttpResponseMessage getExternalLinks()
        {
            return cretae_response(_meaningManager.getExternalLinks());
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [JsonNetFilter]
        [PostRoute("api/Meanings/DetailExternalLinks")]
        [GetRoute("api/Meanings/DetailExternalLinks")]
        public HttpResponseMessage DetailExternalLinks()
        {
            string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(id);

            return cretae_response(_meaningManager.getExternalLink(long.Parse(dict["id"])));
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [JsonNetFilter]
        [PostRoute("api/Meanings/getPrefixes")]
        [GetRoute("api/Meanings/getPrefixes")]
        public HttpResponseMessage getPrefixes()
        {
            return cretae_response(_meaningManager.getPrefixes());
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [JsonNetFilter]
        [PostRoute("api/Meanings/getPrefixfromUri")]
        [GetRoute("api/Meanings/getPrefixfromUri")]
        public HttpResponseMessage getPrefixfromUri()
        {
            string uri = this.Request.Content.ReadAsStringAsync().Result.ToString();
            return cretae_response(_meaningManager.getPrefixfromUri(uri));
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [JsonNetFilter]
        [PostRoute("api/Meanings/getPrefixCategory")]
        [GetRoute("api/Meanings/getPrefixCategory")]
        public HttpResponseMessage getPrefixCategory()
        {
            return cretae_response(_meaningManager.getPrefixCategory());
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [JsonNetFilter]
        [PostRoute("api/Meanings/DetailPrefixCategory")]
        [GetRoute("api/Meanings/DetailPrefixCategory")]
        public HttpResponseMessage DetailPrefixCategory()
        {
            string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(id);

            return cretae_response(_meaningManager.getPrefixCategory(long.Parse(dict["id"])));
        }
        private HttpResponseMessage cretae_response(object return_object)
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
