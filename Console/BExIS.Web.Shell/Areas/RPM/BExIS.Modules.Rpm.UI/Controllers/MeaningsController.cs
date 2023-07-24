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
using BExIS.Utils.Route;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class MeaningsController : ApiController
    {

        private readonly BExIS.Dlm.Entities.Meanings.ImeaningManagr _meaningManager;
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
        [HttpPost,HttpGet]
        [PostRoute("api/Meanings/Index")]
        [GetRoute("api/Meanings/Index")]
        public HttpResponseMessage Index()
        {
            return cretae_response( _meaningManager.getMeanings());
        }

        [BExISApiAuthorize]
        [HttpPost,HttpGet]
        [PostRoute("api/Meanings/Details")]
        [GetRoute("api/Meanings/Details")]
        public HttpResponseMessage Details()
        {
            string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(id);

            return cretae_response(_meaningManager.getMeaning(long.Parse(dict["id"])));
        }

        public HttpResponseMessage getExternalLinks()
        {
            return cretae_response(_meaningManager.getExternalLinks());
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [PostRoute("api/Meanings/DetailExternalLinks")]
        [GetRoute("api/Meanings/DetailExternalLinks")]
        public HttpResponseMessage DetailExternalLinks()
        {
            string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(id);

            return cretae_response(_meaningManager.getExternalLink(long.Parse(dict["id"])));
        }

        private HttpResponseMessage cretae_response(JObject return_object)
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
