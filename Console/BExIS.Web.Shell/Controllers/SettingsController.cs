using BExIS.App.Bootstrap;
using BExIS.App.Bootstrap.Attributes;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using BExIS.Utils;
using BExIS.Utils.Config;
using BExIS.Utils.Route;
using BExIS.Web.Shell.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Vaiona.IoC;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;
using ModuleSettings = Vaiona.Web.Mvc.Modularity.ModuleSettings;

namespace BExIS.Web.Shell.Controllers
{
    public class SettingsController : Controller
    {
        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetSettings()
        {
            try
            {
                List<ReadSettingModel> model = new List<ReadSettingModel>
                {
                    // add shell
                    ReadSettingModel.Convert(GeneralSettings.Get().GetAsJsonModel())
                };

                foreach (var m in ModuleManager.ModuleInfos.Where(m => ModuleManager.IsActive(m.Id)))
                {
                    model.Add(ReadSettingModel.Convert(ModuleManager.GetModuleSettings(m.Id).GetAsJsonModel()));
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetSettingsByModuleId(string moduleId)
        {
            try
            {
                ReadSettingModel model = null;

                if (moduleId == "shell")
                {
                    return Json(ReadSettingModel.Convert(GeneralSettings.Get().GetAsJsonModel()), JsonRequestBehavior.AllowGet);
                }

                return Json(ReadSettingModel.Convert(ModuleManager.GetModuleSettings(moduleId).GetAsJsonModel()), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult Index()
        {
            string module = "Shell";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        [JsonNetFilter]
        [HttpPut]
        public JsonResult PutSettingsByModuleId(string moduleId, UpdateSettingModel model)
        {
            try
            {
                if (moduleId == "shell")
                {
                    GeneralSettings settings = GeneralSettings.Get();

                    var json = new JsonSettings()
                    {
                        Id = moduleId,
                        Name = model.Name,
                        Description = model.Description,
                        Entries = model.Entries
                    };

                    settings.Update(json);

                    return Json(ReadSettingModel.Convert(settings.GetAsJsonModel()), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ModuleSettings settings = ModuleManager.GetModuleSettings(moduleId);

                    var json = new JsonSettings()
                    {
                        Id = moduleId,
                        Name = model.Name,
                        Description = model.Description,
                        Entries = model.Entries
                    };

                    settings.Update(json);

                    return Json(ReadSettingModel.Convert(settings.GetAsJsonModel()), JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}