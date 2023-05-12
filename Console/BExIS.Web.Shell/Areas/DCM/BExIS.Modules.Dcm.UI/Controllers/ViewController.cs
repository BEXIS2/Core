using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dcm.UI.Models.View;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using System;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ViewController : Controller
    {
        // GET: View
        /// <summary>
        /// this action loads the main view page of the dataset.
        /// here all available hooks are loaded and checked and forwarded to ui.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public ActionResult Index(long id, int version = 0)
        {
            ViewBag.id = id;
            ViewBag.version = version;

            return View();
        }

        /// <summary>
        /// load the edit model of a dataset based on the id and the version number
        /// if version = 0 then it loads the latest version
        /// </summary>
        /// <param name="id">identifier of the dataset</param>
        /// <param name="version">version number of the dataset</param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        [JsonNetFilter]
        public JsonResult Load(long id, int version = 0)
        {
            ViewModel model = new ViewModel();
            model.Id = id;
            model.Version = version;

            using (var datasetManager = new DatasetManager())
            {
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
                model.Hooks = hooksManager.GetHooksFor("dataset", "details", HookMode.view);

                // run all checks
                string userName = "";
                if (HttpContext.User.Identity.IsAuthenticated)
                    userName = HttpContext.User.Identity.Name;

                model.Hooks.ForEach(h => h.Check(id, userName));

                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Start from Metadata Hook - view
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        //[BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult Start(long id, int version)
        {
            //throw new NotImplementedException();

            return RedirectToAction("LoadMetadata", "Form", new { area = "DCM", entityId = id, locked = true, created = false, fromEditMode = true });
        }

        /// <summary>
        /// Start from Data Hook - view
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult StartData(long id, int version)
        {
            using (var datasetManager = new DatasetManager())
            {
                long versionId = 0;

                // load dataset version
                // if version number = 0 load latest version
                DatasetVersion datasetVersion = null;
                if (version == 0) // get latest
                {
                    datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                }
                else // get specific
                {
                    versionId = datasetManager.GetDatasetVersionId(id, version); // load datasetversion id by dataset id and version number
                }

                if (versionId < 1)
                {
                    throw new Exception("version of entity with id:" + id + " not exist.");
                }

                return RedirectToAction("ShowPrimaryData", "Data", new { area = "DDM", datasetID = id, versionId });
            }
        }

        /// <summary>
        /// Start from DataSrtucturePreview Hook - view
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult StartDataStructure(long id, int version)
        {
            //throw new NotImplementedException();

            return RedirectToAction("ShowPreviewDataStructure", "Data", new { area = "DDM", datasetID = id });
        }


        public ActionResult Test()
        {
            return PartialView("_test");
        }

    }
}