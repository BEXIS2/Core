using BExIS.Utils.Config;
using BExIS.Utils.Route;
using BExIS.Web.Shell.Models;
using FluentBootstrap.Typography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Web.Shell.Controllers.API
{
    public class ModulesController : ApiController
    {
        [HttpGet, GetRoute("api/modules")]
        public async Task<HttpResponseMessage> GetModules()
        {
            try
            {
                List<ReadModuleModel> model = new List<ReadModuleModel>();

                // add shell
                var shell = new ReadModuleModel()
                {
                    Id = "shell",
                    Title = "Website",
                    Description = "Website",
                    Settings = GeneralSettings.Get().GetAsJsonModel()
                };

                model.Add(shell);

                ReadModuleModel module = null;
                foreach (var m in ModuleManager.ModuleInfos.Where(m => ModuleManager.IsActive(m.Id)))
                {
                    module = new ReadModuleModel()
                    {
                        Id = m.Id,
                        Title = m.Manifest.ManifestDoc.Attribute("displayName")?.Value ?? m.Id,
                        Description = m.Manifest.Description,
                        Settings = ModuleManager.GetModuleSettings(m.Id).GetAsJsonModel()
                    };

                    model.Add(module);
                }

                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/modules/{moduleId}")]
        public async Task<HttpResponseMessage> GetModuleById(string moduleId)
        {
            try
            {
                ReadModuleModel model = null;

                if (moduleId == "shell")
                {
                    model = new ReadModuleModel()
                    {
                        Id = "shell",
                        Title = "Website",
                        Description = "Website",
                        Settings = GeneralSettings.Get().GetAsJsonModel()
                    };

                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }

                var module = ModuleManager.GetModuleInfo(moduleId);

                model = new ReadModuleModel()
                {
                    Id = module.Id,
                    Title = module.Manifest.ManifestDoc.Attribute("displayName")?.Value ?? "",
                    Description = module.Manifest.Description,
                    Settings = ModuleManager.GetModuleSettings(moduleId).GetAsJsonModel()
                };

                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/modules/{moduleId}/settings")]
        public async Task<HttpResponseMessage> GetModuleSettingsById(string moduleId)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, moduleId == "shell" ? GeneralSettings.Get().GetAsJsonModel() : ModuleManager.GetModuleSettings(moduleId).GetAsJsonModel());
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
