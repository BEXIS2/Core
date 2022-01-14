using BExIS.App.Bootstrap.Attributes;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
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
        [JsonNetFilter]
        public JsonResult Load(string id)
        {
            List<string> settings = new List<string>();

            if (ModuleManager.IsActive(id))
            {
                SettingsHelper settingsHelper = new SettingsHelper(id);
                return Json(settingsHelper.LoadSettings(), JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [JsonNetFilter]
        public JsonResult Save(UI.Models.ModuleSettings settings)
        {
            //check incoming values
            if (settings == null) throw new ArgumentNullException("settings");
            if (string.IsNullOrEmpty(settings.Id)) throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(settings.Description)) throw new ArgumentNullException("Description");
            if (settings.Entry == null) throw new ArgumentNullException("entry");

            try
            {
                SettingsHelper settingsHelper = new SettingsHelper(settings.Id);
                //update settings in json
                settingsHelper.Update(settings);

                return Json(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}