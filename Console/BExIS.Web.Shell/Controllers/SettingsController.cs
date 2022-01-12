using BExIS.UI.Helpers;
using BExIS.Web.Shell.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Web.Shell.Controllers
{
    public class SettingsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Get()
        {
            List<object> modules = new List<object>();

            foreach (var m in ModuleManager.ModuleInfos.Where(m => ModuleManager.IsActive(m.Id)))
            {
                ModulModel module = new ModulModel();
                module.Id = m.Id;

                // get displayname from manifest file root node
                var xmldoc = m.Manifest.ManifestDoc;

                if (xmldoc.Attribute("displayName") != null)
                    module.Title = xmldoc.Attribute("displayName").Value;
                else
                    module.Title = m.Id;

                module.Description = m.Manifest.Description;

                modules.Add(module);
            }

            return Json(modules, JsonRequestBehavior.AllowGet);
        }

        // GET: Settings
        [HttpGet]
        public JsonResult Load(string id)
        {
            List<string> settings = new List<string>();

            if (ModuleManager.IsActive(id))
            {
                SettingsHelper settingsHelper = new SettingsHelper(id);
                return Json(settingsHelper.AsJson().ToString(Newtonsoft.Json.Formatting.None), JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(string id, string json)
        {
            SettingsHelper settingsHelper = new SettingsHelper(id);
            settingsHelper.Update(json);

            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}