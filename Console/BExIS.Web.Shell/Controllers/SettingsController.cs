using BExIS.App.Bootstrap;
using BExIS.App.Bootstrap.Attributes;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using BExIS.Utils;
using BExIS.Utils.Config;
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
using Vaiona.IoC;
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

            // add shell
            ModulModel module = new ModulModel();
            module.Id = "shell";
            module.Title = "Website";
            module.Description = "Website";
            modules.Add(module);

            foreach (var m in ModuleManager.ModuleInfos.Where(m => ModuleManager.IsActive(m.Id)))
            {
                module = new ModulModel();
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
            if (id == "shell")
            {
                GeneralSettings generalSettings = IoCFactory.Container.Resolve<GeneralSettings>();
                generalSettings.Get();
                return Json(generalSettings.Get(), JsonRequestBehavior.AllowGet);
            }

            if (ModuleManager.IsActive(id))
            {
                var moduleInfo = ModuleManager.GetModuleInfo(id);
                
                return Json(moduleInfo.Plugin.Settings.Get(), JsonRequestBehavior.AllowGet);
            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(JsonSettings settings)
        {
            //check incoming values
            if (settings == null) throw new ArgumentNullException("settings");
            if (string.IsNullOrEmpty(settings.Id)) throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(settings.Description)) throw new ArgumentNullException("Description");
            if (settings.Entry == null) throw new ArgumentNullException("entry");

            try
            {
                if (settings.Id == "shell")
                {
                    GeneralSettings generalSettings = IoCFactory.Container.Resolve<GeneralSettings>();
                    generalSettings.Update(settings);
                }

                if (ModuleManager.IsActive(settings.Id))
                {
                    var moduleInfo = ModuleManager.GetModuleInfo(settings.Id);
                    moduleInfo.Plugin.Settings.Update(settings);
                }

                return Json(true);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}