using BExIS.UI.Helpers;
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
        [HttpGet]
        public JsonResult Get(string id)
        {
            List<string> ids = new List<string>();

            ids = ModuleManager.ModuleInfos.Where(m => ModuleManager.IsActive(m.Id)).Select(m => m.Id).ToList();

            return Json(ids, JsonRequestBehavior.AllowGet);

        }

        // GET: Settings
        [HttpGet]
        public JsonResult Load(string id)
        {
            List<string> settings = new List<string>();


            if (ModuleManager.IsActive(id))
            {
                SettingsHelper settingsHelper = new SettingsHelper(id);
                return Json(settingsHelper.AsJson(), JsonRequestBehavior.AllowGet);
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