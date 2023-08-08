using BExIS.App.Bootstrap.Attributes;
using BExIS.Utils.Config;
using BExIS.Utils.Route;
using BExIS.Web.Shell.Models;
using FluentBootstrap.Typography;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Schema;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Web.Shell.Controllers.API
{
    public class SettingsController : ApiController
    {
        [HttpGet, GetRoute("api/settings")]
        public async Task<HttpResponseMessage> GetSettings()
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

                return Request.CreateResponse(HttpStatusCode.OK, model);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/settings/{moduleId}")]
        public async Task<HttpResponseMessage> GetSettingsByModuleId(string moduleId)
        {
            try
            {
                ReadSettingModel model = null;

                if (moduleId == "shell")
                {
                    return Request.CreateResponse(HttpStatusCode.OK, ReadSettingModel.Convert(GeneralSettings.Get().GetAsJsonModel()));
                }

                return Request.CreateResponse(HttpStatusCode.OK, ReadSettingModel.Convert(ModuleManager.GetModuleSettings(moduleId).GetAsJsonModel()));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPut, PutRoute("api/settings/{moduleId}")]
        public async Task<HttpResponseMessage> PutSettingsByModuleId(string moduleId, UpdateSettingModel model)
        {
            try
            {
                var settings = ModuleManager.GetModuleSettings(moduleId);

                var json = new JsonSettings()
                {
                    Id = moduleId,
                    Name = model.Name,
                    Description = model.Description,
                    Entries = model.Entries
                };

                settings.Update(json);

                return Request.CreateResponse(HttpStatusCode.OK, model);
            }catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        //[HttpPut, PutRoute("api/settings")]
        //public async Task<HttpResponseMessage> PutSettings(List<UpdateSettingModel> model)
        //{
        //    try
        //    {
        //        foreach (var item in model)
        //        {
        //            var settings = ModuleManager.GetModuleSettings(item.Id);

        //            var json = new JsonSettings()
        //            {
        //                Id = item.Id,
        //                Name = item.Name,
        //                Description = item.Description,
        //                Entries = item.Entries
        //            };

        //            settings.Update(json);
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, "sdsfdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
        //    }
        //}
    }
}
