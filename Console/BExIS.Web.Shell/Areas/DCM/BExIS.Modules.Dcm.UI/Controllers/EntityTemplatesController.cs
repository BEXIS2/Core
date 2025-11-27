using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Services;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models.EntityTemplate;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.UI.Hooks;
using BExIS.UI.Models;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Vaiona.Web.Extensions;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EntityTemplatesController : Controller
    {
        // GET: EntityTemplate
        public ActionResult Index()
        {
            string module = "DCM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

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
            if (id == 0) return Json(new EntityTemplateModel(), JsonRequestBehavior.AllowGet);

            using (var entityTemplateManager = new EntityTemplateManager())
            {
                var entityTemplate = entityTemplateManager.Repo.Get(id);
                return Json(EntityTemplateHelper.ConvertTo(entityTemplate), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpDelete]
        public JsonResult Delete(long id)
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            {
                var result = entityTemplateManager.Delete(id);
                return Json(result);
            }
        }

        [JsonNetFilter]
        [HttpPost]
        [CustomValidateAntiForgeryToken]
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
            List<ListItem> tmp = new List<ListItem>();
            using (var entityManager = new EntityManager())
            {
                foreach (var entity in entityManager.EntityRepository.Get())
                {
                    tmp.Add(new ListItem(entity.Id, entity.Name));
                }
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult MetadataStructures()
        {
            List<ListItem> tmp = new List<ListItem>();
            using (var metadataStrutcureManager = new MetadataStructureManager())
            {
                foreach (var metadataStructure in metadataStrutcureManager.Repo.Get())
                {
                    var xmlDatasetHelper = new XmlDatasetHelper();
                    var entity = xmlDatasetHelper.GetEntityNameFromMetadatStructure(metadataStructure.Id);

                    tmp.Add(new ListItem(metadataStructure.Id, metadataStructure.Name, entity));
                }
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult SystemKeys(long metadataStructureId)
        {
            List<ListItem> tmp = new List<ListItem>();

            if (metadataStructureId > 0)
            {
                // check wheter mapping exist based on metadata structure id
                using (var mappingManager = new MappingManager())
                {
                    var source = mappingManager.LinkElementRepo.Get().Where(l => l.Type.Equals(LinkElementType.System)).FirstOrDefault();
                    var target = mappingManager.LinkElementRepo.Get().Where(l => l.ElementId.Equals(metadataStructureId) && l.Type.Equals(LinkElementType.MetadataStructure)).FirstOrDefault();
                    Mapping rootMapping = null;

                    if (target != null && source != null)
                        rootMapping = mappingManager.GetMapping(source, target);

                    IEnumerable<string> sources = new List<string>();
                    if (rootMapping != null) // root mapping to system keys exist
                    {
                        var childMappings = mappingManager.GetChildMappingFromRoot(rootMapping.Id, 1);
                        sources = childMappings.Select(m => m.Source.Name);
                    }

                    foreach (var key in Enum.GetValues(typeof(Key)))
                    {
                        var mapped = "mapped";
                        if (!sources.Contains(key.ToString())) mapped = "unmapped";

                        ListItem item = new ListItem()
                        {
                            Id = Convert.ToInt64(key),
                            Text = key.ToString(),
                            Group = mapped
                        };

                        tmp.Add(item);
                    }
                }
            }

            return Json(tmp.OrderBy(i => i.Group), JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult DataStructures()
        {
            List<KeyValuePair<long, string>> tmp = new List<KeyValuePair<long, string>>();
            using (var dataStructureManager = new DataStructureManager())
            {
                tmp = dataStructureManager.GetStructuredDataStructuresAsKVP();
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

            foreach (var hook in hookManager.GetHooksFor("dataset", "details", HookMode.view))
            {
                tmp.Add(hook.DisplayName);
            }

            return Json(tmp.Distinct(), JsonRequestBehavior.AllowGet);
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