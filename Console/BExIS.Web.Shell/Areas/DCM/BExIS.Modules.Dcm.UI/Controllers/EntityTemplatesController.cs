﻿using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models.EntityTemplate;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Hooks;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using static BExIS.Modules.Dcm.UI.Models.EntityTemplate.EntityTemplateModel;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EntityTemplatesController : Controller
    {
        // GET: EntityTemplate
        public ActionResult Index()
        {
            return View();
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult Load()
        {
            List<EntityTemplateModel> entityTemplateModels = new List<EntityTemplateModel>();
            using (var entityTemplateManager = new EntityTemplateManager())
            {
                foreach (var e in entityTemplateManager.Repo.Get())
                {
                    entityTemplateModels.Add(EntityTemplateHelper.ConvertTo(e));
                }

                return Json(entityTemplateModels, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult Get(long id)
        {
            if(id==0) return Json(new EntityTemplateModel(), JsonRequestBehavior.AllowGet);


            using (var entityTemplateManager = new EntityTemplateManager())
            {
                var entityTemplate = entityTemplateManager.Repo.Get(id);
                return Json(EntityTemplateHelper.ConvertTo(entityTemplate), JsonRequestBehavior.AllowGet);
            }
        }


        [JsonNetFilter]
        [HttpPost]
        public JsonResult Delete(long id)
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            {
                var result = entityTemplateManager.Delete(id);
                return Json(result);
            }
        }

        //[JsonNetFilter]
        //[HttpPost]
        //public JsonResult Create(EntityTemplateModel entityTemplate)
        //{
        //    using (var entityTemplateManager = new EntityTemplateManager())
        //    {
        //        var result = entityTemplateManager.Create(EntityTemplateHelper.ConvertTo(entityTemplate));
        //        if(result != null) return Json(true);
        //    }

        //    return Json(false);
        //}

        [JsonNetFilter]
        [HttpPost]
        public JsonResult Update(EntityTemplateModel entityTemplate)
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            {
                EntityTemplate result = null;

                if (entityTemplate.Id == 0)
                    result = entityTemplateManager.Create(EntityTemplateHelper.ConvertTo(entityTemplate));
                else
                    result = entityTemplateManager.Update(EntityTemplateHelper.Merge(entityTemplate));

                if (result != null) return Json(EntityTemplateHelper.ConvertTo(result));
            }

            return Json(false);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult Entities()
        {
            List<KvP> tmp = new List<KvP>();
            using (var entityManager = new EntityManager())
            {
                foreach (var entity in entityManager.EntityRepository.Get())
                {
                    tmp.Add(new KvP(entity.Id, entity.Name));
                }
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult MetadataStructures()
        {
            List<KvP> tmp = new List<KvP>();
            using (var metadataStrutcureManager = new MetadataStructureManager())
            {
                foreach (var entity in metadataStrutcureManager.Repo.Get())
                {
                    tmp.Add(new KvP(entity.Id, entity.Name));
                }
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult SystemKeys()
        {
            List<KeyValuePair<int, string>> tmp = new List<KeyValuePair<int, string>>();

            foreach (var key in Enum.GetValues(typeof(Key)))
            {
                tmp.Add(new KeyValuePair<int,string>(Convert.ToInt32(key),key.ToString()));
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult DataStructures()
        {
            List<KeyValuePair<long, string>> tmp = new List<KeyValuePair<long, string>>();
            using (var dataStructureManager = new DataStructureManager())
            {
                foreach (var structure in dataStructureManager.StructuredDataStructureRepo.Get())
                {
                    tmp.Add(new KeyValuePair<long, string>(structure.Id, structure.Name));
                }
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult Hooks()
        {
            HookManager hookManager = new HookManager();

            List<string> tmp = new List<string>();

            foreach (var hook in hookManager.GetHooksFor("dataset", "details", HookMode.edit))
            {
                tmp.Add(hook.DisplayName);
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult Groups()
        {
            List<KeyValuePair<long, string>> tmp = new List<KeyValuePair<long, string>>();
            using (var groupManager = new GroupManager())
            {
                foreach (var group in groupManager.Groups)
                {
                    tmp.Add(new KeyValuePair<long, string>(group.Id, group.Name));
                }
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult FileTypes()
        {
            List<string> tmp = new List<string>();

            var tenant = Session.GetTenant();
            if (tenant != null)
            {
                foreach (var ext in tenant.AllowedFileExtensions)
                {
                    tmp.Add(ext);
                }
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }
    }
}