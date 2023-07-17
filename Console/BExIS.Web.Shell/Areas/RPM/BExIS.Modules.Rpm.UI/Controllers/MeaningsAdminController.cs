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
    public class MeaningsAdminController : ApiController
    {

        private readonly BExIS.Dlm.Entities.Meanings.ImeaningManagr _meaningManager;
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
        [HttpPost,HttpGet]
        [PostRoute("api/MeaningsAdmin/Create")]
        [GetRoute("api/MeaningsAdmin/Create")]
        public JObject Create(HttpRequestMessage request)
        {
            try
            {
                String Name = Convert.ToString(HttpContext.Current.Request.Form["Name"]);

                String ShortName = Convert.ToString(HttpContext.Current.Request.Form["ShortName"]);
                
                String Description = Convert.ToString(HttpContext.Current.Request.Form["Description"]);

                Selectable selectable = (Selectable)Enum.Parse(typeof(Selectable), Convert.ToString(HttpContext.Current.Request.Form["selectable"]));

                Approved approved = (Approved)Enum.Parse(typeof(Approved), Convert.ToString(HttpContext.Current.Request.Form["approved"]));

                List<string> externalLink = HttpContext.Current.Request.Form["externalLink[]"] != null ? Convert.ToString(HttpContext.Current.Request.Form["externalLink[]"]).Split(',').ToList<string>() : new List<string>();

                List<string> related_meaning = HttpContext.Current.Request.Form["related_meaning"]!= null ? Convert.ToString(HttpContext.Current.Request.Form["related_meaning"]).Split(',').ToList<string>() : new List<string>();
                List<string> variable = HttpContext.Current.Request.Form["Variable"] != null ? Convert.ToString(HttpContext.Current.Request.Form["Variable"]).Split(',').ToList<string>() : new List<string>();

                JObject res = _meaningManager.addMeaning(Name, ShortName, Description, selectable, approved, variable, externalLink, related_meaning);
                return res;
            }
            catch
            {
                return null;
            }
        }


        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [PostRoute("api/MeaningsAdmin/EditMeaning")]
        [GetRoute("api/MeaningsAdmin/EditMeaning")]
        public JObject EditMeaning(HttpRequestMessage request)
        {
            Meaning m = null;
            try
            {
                String Name = Convert.ToString(HttpContext.Current.Request.Form["Name"]);

                String ShortName = Convert.ToString(HttpContext.Current.Request.Form["ShortName"]);

                String Description = Convert.ToString(HttpContext.Current.Request.Form["Description"]);

                Selectable selectable = (Selectable)Enum.Parse(typeof(Selectable), Convert.ToString(HttpContext.Current.Request.Form["selectable"]));

                Approved approved = (Approved)Enum.Parse(typeof(Approved), Convert.ToString(HttpContext.Current.Request.Form["approved"]));

                List<string> externalLink = HttpContext.Current.Request.Form["externalLink[]"] != null ? Convert.ToString(HttpContext.Current.Request.Form["externalLink[]"]).Split(',').ToList<string>() : new List<string>();

                List<string> related_meaning = HttpContext.Current.Request.Form["related_meaning"] != null ? Convert.ToString(HttpContext.Current.Request.Form["related_meaning"]).Split(',').ToList<string>() : new List<string>();
                List<string> variable = HttpContext.Current.Request.Form["Variable[]"] != null ? Convert.ToString(HttpContext.Current.Request.Form["Variable[]"]).Split(',').ToList<string>() : new List<string>();

                string id = Convert.ToString(HttpContext.Current.Request.Form["id"]);
                //if (related_meaning.Count== 0 )
                //    related_meaning = _meaningManager.getMeaning(long.Parse(id)).ToObject<Meaning>();
                JObject res = _meaningManager.editMeaning(id, Name, ShortName, Description, selectable, approved, variable, externalLink, related_meaning);
                return res;

                JObject obj = _meaningManager.getMeaning(Int64.Parse(id));
                m = JsonConvert.DeserializeObject<Meaning>(obj["Value"].ToString());
            }
            catch
            {
                return null;
            }
        }

        [BExISApiAuthorize]
        [HttpPost,HttpGet]
        [PostRoute("api/MeaningsAdmin/Delete")]
        [GetRoute("api/MeaningsAdmin/Delete")]
        public JObject Delete(HttpRequestMessage request)
        {
            try
            {
                string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
                return _meaningManager.deleteMeaning(Int64.Parse(id));
            }
            catch
            {
                return null;
            }
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [PostRoute("api/MeaningsAdmin/getVariables")]
        [GetRoute("api/MeaningsAdmin/getVariables")]
        public JObject getVariables()
        {
            using (DataStructureManager dsm = new DataStructureManager())
            {
                List<Variable> variables = (List<Variable>)dsm.VariableRepo.Get();
                Dictionary<long, string> fooDict = variables.ToDictionary(f => f.Id, f => f.Label);
                string json_string = JsonConvert.SerializeObject(fooDict);
                return JObject.Parse(json_string);
            }
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [PostRoute("api/MeaningsAdmin/updateRelatedManings")]
        [GetRoute("api/MeaningsAdmin/updateRelatedManings")]
        public JObject updateRelatedManings (HttpRequestMessage request)
        {
            try
            {
                String parentID = Convert.ToString(HttpContext.Current.Request.Form["parentID"]);
                String childID = Convert.ToString(HttpContext.Current.Request.Form["childID"]);
                return _meaningManager.updateRelatedManings(parentID, childID);
            }
            catch(Exception exc)
            {
                return null;
            }
        }


        // external links endpoints

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [PostRoute("api/MeaningsAdmin/createExternalLink")]
        [GetRoute("api/MeaningsAdmin/createExternalLink")]
        public JObject createExternalLink(HttpRequestMessage request)
        {
            try
            {
                String uri = Convert.ToString(HttpContext.Current.Request.Form["uri"]);

                String name = Convert.ToString(HttpContext.Current.Request.Form["name"]);

                String type = Convert.ToString(HttpContext.Current.Request.Form["type"]);

                JObject res = _meaningManager.addExternalLink(uri, name, type);
                return res;
            }
            catch
            {
                return null;
            }
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [PostRoute("api/MeaningsAdmin/editExternalLinks")]
        [GetRoute("api/MeaningsAdmin/editExternalLinks")]
        public JObject editExternalLinks(HttpRequestMessage request)
        {
            ExternalLink m = null;
            try
            {
                String URI = Convert.ToString(HttpContext.Current.Request.Form["URI"]);

                String Name = Convert.ToString(HttpContext.Current.Request.Form["Name"]);

                String Type = Convert.ToString(HttpContext.Current.Request.Form["Type"]);

                string id = Convert.ToString(HttpContext.Current.Request.Form["id"]);
                JObject res = _meaningManager.editExternalLink(id, URI, Name, Type);
                return res;

                JObject obj = _meaningManager.getExternalLink(Int64.Parse(id));
                m = JsonConvert.DeserializeObject<ExternalLink>(obj["Value"].ToString());
            }
            catch
            {
                return null;
            }
        }

        [BExISApiAuthorize]
        [HttpPost, HttpGet]
        [PostRoute("api/MeaningsAdmin/deleteExternalLinks")]
        [GetRoute("api/MeaningsAdmin/deleteExternalLinks")]
        public JObject deleteExternalLinks(HttpRequestMessage request)
        {
            try
            {
                string id = this.Request.Content.ReadAsStringAsync().Result.ToString();
                return _meaningManager.deleteExternalLink(Int64.Parse(id));
            }
            catch
            {
                return null;
            }
        }

    }
}
