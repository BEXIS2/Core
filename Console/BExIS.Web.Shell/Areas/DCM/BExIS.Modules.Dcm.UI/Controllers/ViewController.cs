using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Helpers.BIOSCHEMA;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models.View;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.UI.Helpers;
using BExIS.UI.Hooks;
using BExIS.Utils.Data;
using BExIS.Utils.Data.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ViewController : Controller
    {
        #region about View

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
            string module = "DCM";

            ViewData["id"] = id;
            ViewData["version"] = version;
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            // load settings from ddm
            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            ViewData["use_tags"] = moduleSettings.GetValueByKey("use_tags");
            bool useTags = (bool)ViewData["use_tags"];
            ViewData["use_minor"] = moduleSettings.GetValueByKey("use_minor");
            ViewData["has_data"] = false;
            ViewData["data_aggreement"] = moduleSettings.GetValueByKey("data_aggreement");

            if (version > 0)
            {
                // load BioSchema Description if exist
                string bioschemadescription = getBioSchema(id, version);
                if (!string.IsNullOrEmpty(bioschemadescription))
                    ViewData["BioSchema"] = bioschemadescription;
            }

            //ToDo
            // add bioschema to view data
            // has data
            // data_aggreement
            // check_public_metadata

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
            ViewSettingsModel model = new ViewSettingsModel();
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

                if (datasetVersion != null)
                {
                    model.Labels = getLabels(id, datasetVersion.Id, datasetVersion.Tag?.Nr ?? 0, datasetVersion.Dataset.EntityTemplate.Name);

                    // check data
                    if (datasetVersion.Dataset.DataStructure != null && datasetVersion.Dataset.DataStructure.Self.GetType().Equals(typeof(StructuredDataStructure)))
                    {
                        long c = datasetManager.RowCount(datasetVersion.Dataset.Id, null);
                        ViewData["gridTotal"] = c;
                        if (c > 0) model.HasData = true;
                    }
                    else
                    {

                        if (datasetVersion.ContentDescriptors.Where(c => c.Name.Equals("unstructuredData")).Any())
                        {
                            model.HasData = true;
                        }
                    }


                    // get title
                    model.Title = datasetVersion.Title;

                    // load all hooks for the edit view
                    HookManager hooksManager = new HookManager();
                    model.Hooks = hooksManager.GetHooksFor("dataset", "details", HookMode.view);

                }
                // run all checks
                string userName = "";
                if (HttpContext.User.Identity.IsAuthenticated)
                    userName = HttpContext.User.Identity.Name;

                model.Hooks.ForEach(h => h.Check(id, userName));

                // settings
                // load settings from ddm
                var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
                model.UseTags = Convert.ToBoolean(moduleSettings.GetValueByKey("use_tags"));
                model.UseMinor = Convert.ToBoolean(moduleSettings.GetValueByKey("use_minor"));
                model.HasData = false;
                model.DataAggrement = moduleSettings.GetValueByKey("data_aggreement").ToString();




                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        public PartialViewResult Tags(long id, int version)
        {
            if (id <= 0) throw new ArgumentException("id is not valid");

            ViewData["Id"] = id;
            List<TagInfoViewModel> tags = new List<TagInfoViewModel>();
            bool hasEditRights = hasUserRights(id, RightType.Write);

            if (version == 0) return PartialView("_tagsView", tags); // return empty list



            using (DatasetManager datasetmanager = new DatasetManager())
            {
                TagInfoHelper _helper = new TagInfoHelper();
                var versions = datasetmanager.GetDatasetVersions(id);

                var currentVersion = datasetmanager.GetDatasetVersion(id, version);
                ViewData["Tag"] = currentVersion.Tag?.Nr;

                if (versions != null)
                {
                    tags = _helper.GetViews(versions, datasetmanager, !hasEditRights);
                }
            }

            return PartialView("_tagsView", tags); // Replace "_PartialViewName" with your actual name

        }

        private string getBioSchema(long id, int version)
        {
            if (id <= 0) throw new ArgumentException("id is not valid");
            ViewData["Id"] = id;

            var helper = new BioSchemaHelper();
            string json = helper.GetBioSchemaForDataset(id, version, HttpContext.Request.Url.ToString());

            return json; // Replace "_PartialViewName" with your actual name

        }

        #endregion about view

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

            return RedirectToAction("LoadMetadataByVersion", "Form", new { area = "DCM", entityId = id, version, locked = true, created = false, fromEditMode = false });
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

        #region Helpers

        public bool UserExist()
        {
            if (HttpContext.User != null && HttpContext.User.Identity != null && !string.IsNullOrEmpty(HttpContext.User.Identity.Name)) return true;

            return false;
        }

        public string GetUsernameOrDefault()
        {
            var username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        private string getPartyNameOrDefault()
        {
            var userName = string.Empty;
            try
            {
                userName = HttpContext.User.Identity.Name;
            }
            catch { }

            if (userName != null)
            {
                using (var uow = this.GetUnitOfWork())
                using (var partyManager = new PartyManager())
                {
                    var userRepository = uow.GetReadOnlyRepository<User>();
                    var user = userRepository.Query(s => s.Name.ToUpperInvariant() == userName.ToUpperInvariant()).FirstOrDefault();

                    if (user != null)
                    {
                        Party party = partyManager.GetPartyByUser(user.Id);
                        if (party != null)
                        {
                            return party.Name;
                        }
                    }
                }
            }
            return !string.IsNullOrWhiteSpace(userName) ? userName : "DEFAULT";
        }

        private async Task<long> getVersionId(long datasetId, int versionNr = 0, string versionName = "", double tagNr = 0)
        {

            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            bool useTags = false;
            bool.TryParse(moduleSettings.GetValueByKey("use_tags").ToString(), out useTags);

            return await DatasetVersionHelper.GetVersionId(datasetId, GetUsernameOrDefault(), versionNr, useTags, tagNr);

        }

        private bool hasUserRights(long entityId, RightType rightType)
        {
            #region security permissions and authorizations check

            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
                return entityPermissionManager.HasEffectiveRightsAsync(GetUsernameOrDefault(), typeof(Dataset), entityId, rightType).Result;

            #endregion security permissions and authorizations check
        }

        public Dictionary<string, string> getLabels(long id, long versionId, double tag, string template)
        {
            using (var publicationManager = new PublicationManager())
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

                var publications = publicationManager.PublicationRepo.Query(p => p.Dataset.Id == id && p.DatasetVersion.Id == versionId && p.ExternalLink != "");
                if (publications != null && publications.Any())
                {

                    foreach (var item in publications)
                    {
                        keyValuePairs.Add(item.ExternalLink, item.ExternalLinkType);
                    }
                }

                keyValuePairs.Add(template, "template");

                return keyValuePairs;
            }
        }

        #endregion
    }
}