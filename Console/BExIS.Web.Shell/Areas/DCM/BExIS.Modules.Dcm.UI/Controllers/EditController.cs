using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.Modules.Dcm.UI.Models.EntityTemplate;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using BExIS.UI.Helpers;
using BExIS.UI.Hooks;
using BExIS.UI.Models;
using BExIS.Utils.Data.Helpers;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EditController : Controller
    {
        // GET: Edit
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        public ActionResult Index(long id, int version = 0)
        {
            string module = "DCM";

            ViewData["id"] = id;
            ViewData["version"] = version;
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        /// <summary>
        /// load the edit model of a dataset based on the id and the version number
        /// if version = 0 then it loads the latest version
        /// </summary>
        /// <param name="id">identifier of the dataset</param>
        /// <param name="version">version number of the dataset</param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        [JsonNetFilter]
        public JsonResult Load(long id, int version = 0)
        {
            EditModel model = new EditModel();
            model.Id = id;
            model.Version = version;

            using (var datasetManager = new DatasetManager())
            {
                if (datasetManager.IsDatasetCheckedIn(id) == false) throw new Exception("Dataset is in process, try again later");

                // load dataset version
                // if version number = 0 load latest version
                DatasetVersion datasetVersion = null;
                if (version == 0) // get latest
                {
                    datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    model.Version = datasetManager.GetDatasetVersionCount(id); // get number of the latest version
                }
                else // get specific
                {
                    model.VersionId = datasetManager.GetDatasetVersionId(id, version); // get version id
                    datasetVersion = datasetManager.GetDatasetVersion(model.VersionId); // load datasetversion by id
                }

                // get title
                model.Title = datasetVersion.Title;

                // load all hooks for the edit view
                HookManager hooksManager = new HookManager();
                model.Hooks = hooksManager.GetHooksFor("dataset", "details", HookMode.edit);
                model.Views = hooksManager.GetViewsFor("dataset", "details", HookMode.edit);

                // run all checks
                string userName = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

                model.Hooks.ForEach(h => h.Check(id, userName));

                // add information disabled hooks from the entity template
                // based on the entity template, hooks can be disabled.
                foreach (var hook in model.Hooks)
                {
                    if (datasetVersion.Dataset.EntityTemplate.DisabledHooks != null && datasetVersion.Dataset.EntityTemplate.DisabledHooks.Contains(hook.DisplayName))
                        hook.Status = HookStatus.Disabled;
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// return hooks with new status, check was running again
        /// </summary>
        /// <param name="id">subject id</param>
        /// <param name="versionId">specific subject version based on version id</param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        [JsonNetFilter]
        public JsonResult Hooks(long id, int version = 0)
        {
            List<Hook> Hooks = new List<Hook>();

            using (var datasetManager = new DatasetManager())
            {
                var dataset = datasetManager.GetDataset(id);
                // load all hooks for the edit view
                HookManager hooksManager = new HookManager();
                Hooks = hooksManager.GetHooksFor("dataset", "details", HookMode.edit);

                // run all checks
                string userName = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

                Hooks.ForEach(h => h.Check(id, userName));

                // add information disabled hooks from the entity template
                // based on the entity template, hooks can be disabled.
                foreach (var hook in Hooks)
                {
                    if (dataset.EntityTemplate.DisabledHooks != null && dataset.EntityTemplate.DisabledHooks.Contains(hook.DisplayName))
                        hook.Status = HookStatus.Disabled;
                }

                return Json(Hooks, JsonRequestBehavior.AllowGet);
            }
        }

        #region extension
        [JsonNetFilter]
        public JsonResult GetExtensions(long id)
        {
            List<ExtensionItem> tmp = new List<ExtensionItem>();

            using (var datasetManager = new DatasetManager())
            using (var entityReferenceManager = new EntityReferenceManager())
            { 
                var dataset = datasetManager.GetDataset(id);
                var entity = dataset.EntityTemplate.EntityType;

                var entityreferences = entityReferenceManager.ReferenceRepository.Query(e =>
                        e.LinkType.Equals("extension") &&
                        e.SourceId.Equals(id) &&
                        e.SourceEntityId.Equals(entity.Id)).ToList();

                foreach (var x in entityreferences)
                {
                    string title = "";
                    if (x.TargetVersion == 0) // latest
                        title = datasetManager.GetDatasetLatestVersion(x.TargetId).Title;
                    else
                     title  = datasetManager.GetDatasetVersion(x.TargetId, x.TargetVersion).Title;

                    tmp.Add(new ExtensionItem()
                    {
                        Id = x.TargetId,
                        Version = x.TargetVersion,
                        Title = title,
                        LinkType = x.LinkType,
                        ReferenceType = x.ReferenceType
                    });
                }
            }
            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetExtensionEntityTemplateList(long id)
        {
            if(id==0) throw new Exception("id is missing");

            List<EntityTemplateModel> entityTemplateModels = new List<EntityTemplateModel>();

            using (var datasetManager = new DatasetManager())
            using (var entityTemplateManager = new EntityTemplateManager())
            {
                
                var dataset = datasetManager.GetDataset(id);
                var template = dataset.EntityTemplate;
                var extensions = template.ExtensionList.Select(e=>e.TemplateId);

                foreach (var e in entityTemplateManager.Repo.Query(e => e.Activated && extensions.Contains(e.Id)).ToList())
                {
                    entityTemplateModels.Add(EntityTemplateHelper.ConvertTo(e, false));
                }

                }
            
                return Json(entityTemplateModels, JsonRequestBehavior.AllowGet);
            
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult CreateExtensionLink(long id, long extensionId)
        {
            using (var datasetmanager = new DatasetManager())
            using (var entityReferenceManager = new EntityReferenceManager())
            {
                try
                {

                    if (id == 0) throw new Exception("id is missing");
                    if (extensionId == 0) throw new Exception("extensionId is missing");


                    // get object
                    var dataset = datasetmanager.GetDataset(id);
                    var extension = datasetmanager.GetDataset(extensionId);

                    // get templates
                    var datasetTemplate = dataset.EntityTemplate;
                    var extensionTemplate = extension.EntityTemplate;

                    var extensionType = datasetTemplate.ExtensionList.Where(e => e.TemplateId.Equals(extensionTemplate.Id)).FirstOrDefault();

                    // check if extension is allowed
                    if (datasetTemplate.ExtensionList == null || extensionType == null)
                        throw new Exception("This extension is not allowed for this subject.");

                    // check if extension is unique and allready exists
                    if (datasetTemplate.ExtensionList.Any(e => e.TemplateId.Equals(extensionTemplate.Id) && e.Unique))
                    {
                        var existingLinks = entityReferenceManager.ReferenceRepository.Query(e =>
                            e.LinkType.Equals("extension") &&
                            e.SourceId.Equals(id) &&
                            e.SourceEntityId.Equals(dataset.EntityTemplate.EntityType.Id) &&
                            e.TargetEntityId.Equals(extension.EntityTemplate.EntityType.Id)).ToList();

                        if (existingLinks.Count > 0)
                            throw new Exception("This extension is unique and allready exists.");
                    }

                    // create link
                    EntityReferenceHelper entityReferenceHelper = new EntityReferenceHelper();
                    var extensionRef = entityReferenceHelper.GetReferenceConfigByType(extensionType.ReferenceType);



                    EntityReference entitreference = new EntityReference();
                    entitreference.SourceId = id;
                    entitreference.SourceEntityId = dataset.EntityTemplate.EntityType.Id;
                    entitreference.SourceVersion = 0; // not used
                    entitreference.TargetId = extensionId;
                    entitreference.TargetEntityId = extension.EntityTemplate.EntityType.Id;
                    entitreference.TargetVersion = 0;
                    entitreference.LinkType = extensionRef.LinkType;
                    entitreference.ReferenceType = extensionRef.ReferenceType;
                    //entitreference.Context = "added";
                    entitreference.Category = extensionRef.Category;

                    entityReferenceManager.Create(entitreference);

                        

                }
                catch (Exception ex)
                {
                    datasetmanager.PurgeDataset(extensionId);

                    throw new Exception(ex.Message);    
                }
            }

            return Json(true);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult DeleteExtension(long extensionId)
        {


            using (var datasetmanager = new DatasetManager())
            using (var entityReferenceManager = new EntityReferenceManager())
            {
                try
                {
                    if (extensionId == 0) throw new Exception("extensionId is missing");

                    // remove all existing links

                    var linkIds = entityReferenceManager.ReferenceRepository.Query(e =>
                        e.TargetId.Equals(extensionId) &&
                        e.LinkType.Equals("extension")).Select(l=>l.Id);

                    entityReferenceManager.Delete(linkIds.ToList());


                    // purge dataset
                    datasetmanager.PurgeDataset(extensionId);


                }
                catch (Exception ex)
                {
              
                    throw new Exception(ex.Message);
                }
            }

            Response.StatusCode = (int)HttpStatusCode.OK;
         
            return Json( new { Success = true, Message = "Extension successful deleted" });
        
        }

        #endregion
    }
}