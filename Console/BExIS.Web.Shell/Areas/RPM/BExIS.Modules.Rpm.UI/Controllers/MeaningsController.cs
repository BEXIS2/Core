using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        public JObject Index()
        {
            return _meaningManager.getMeanings();
        }

        [BExISApiAuthorize]
        [HttpPost,HttpGet]
        [PostRoute("api/Meanings/Details")]
        [GetRoute("api/Meanings/Details")]
        public JObject Details()
        {
            string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(id);

            return  _meaningManager.getMeaning(long.Parse(dict["id"]));
        }

        public JObject getExternalLinks()
        {
            return _meaningManager.getExternalLinks();
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [PostRoute("api/Meanings/DetailExternalLinks")]
        [GetRoute("api/Meanings/DetailExternalLinks")]
        public JObject DetailExternalLinks()
        {
            string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
            Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(id);

            return _meaningManager.getExternalLink(long.Parse(dict["id"]));
        }
    }
}
