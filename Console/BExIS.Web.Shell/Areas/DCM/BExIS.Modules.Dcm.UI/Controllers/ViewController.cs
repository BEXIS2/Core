using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Helpers.BIOSCHEMA;
using BExIS.Dim.Helpers.Models;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models.View;
using BExIS.Modules.Dim.UI.Helpers;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Requests;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.UI.Hooks;
using BExIS.UI.Models;
using BExIS.Utils.Data;
using BExIS.Utils.Data.Upload;
using DocumentFormat.OpenXml.Office2013.Excel;
using Microsoft.AspNet.Identity;
using NHibernate.Engine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ViewController : Controller
    {

        private readonly UserManager _userManager;

        public ViewController(UserManager userManager)
        {
            _userManager = userManager;
        }


        #region about View

        // GET: View
        /// <summary>
        /// this action loads the main view page of the dataset.
        /// here all available hooks are loaded and checked and forwarded to ui.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public ActionResult Index(long id, int version = 0, double tag = 0)
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
                    ViewData["bioSchema"] = bioschemadescription;
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
        public JsonResult Load(long id, int version = 0, double tag = 0)
        {
 
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            ApiDatasetHelper apiDatasetHelper = new ApiDatasetHelper();

            ViewModel model = new ViewModel();
            model.Id = id;

            long versionId = 0;
            bool latestVersion = false;
            long latestVersionId = 0;
            long latestVersionNr = 0;
            bool useTags = false;

            // load dataset version
            // if version number = 0 load latest version
            DatasetVersion datasetVersion = null;
            string dataStructureType = DataStructureType.Structured.ToString();

            using (var datasetManager = new DatasetManager())
            using (EntityManager entityManager = new EntityManager())
            {
                // Retrieve data for active and hidden (marked as deleted) datasets
                if (datasetManager.IsDatasetCheckedIn(id) || datasetManager.IsDatasetDeleted(id))
                {
                    // check is public
                    long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                    entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;

                    List<DatasetVersion> datasetVersions = datasetManager.GetDatasetVersions(id);
                    List<DatasetVersion> datasetVersionsAllowed = new List<DatasetVersion>();

                    if (!datasetManager.IsDatasetDeleted(id)) // dataset should not be in delete state
                    {
                        // Get version id based on public or internal access. Version name has a higher priority as version.
                        // Public access has higher priority as major/minor versions
                        versionId = getVersionId(id, version, "", tag).Result;

                        if (useTags)
                        {
                            // compare the current version with the latest version id also based on tags
                            var x = datasetManager.GetLatestTag(id);
                            if (x != null)
                            {
                                latestVersionId = datasetManager.GetLatestVersionIdByTagNr(id, x.Nr);
                                latestVersion = (versionId >= latestVersionId);
                            }
                            else
                            {
                                latestVersionId = datasetManager.GetDatasetLatestVersionId(id);
                                latestVersion = (versionId >= latestVersionId);
                            }

                        }
                        else
                        {
                            // Set if the latest version is selected. Compare current version id against unfiltered max id

                            latestVersionId = datasetVersions.OrderByDescending(d => d.Timestamp).Select(d => d.Id).FirstOrDefault();
                            latestVersionNr = datasetManager.GetDatasetVersionNr(latestVersionId);
                            latestVersion = (versionId == latestVersionId);

                        }
                        // Get version number based on version id
                        if (versionId >= 0)
                        {
                            version = datasetManager.GetDatasetVersionNr(versionId);
                        }

                        // Throw error if no version id was found.
                        if (versionId <= 0)
                        {
              
                            ModelState.AddModelError("", string.Format("The requested version (release tag or version ID: {0}{1}) could not be found or you don’t have permission to access it.", version, ""));
                        }
                        else
                        {
                            datasetVersion = datasetManager.DatasetVersionRepo.Get(versionId); // this is needed to allow dsv to access to an open session that is available via the repo
                            var dataset = datasetVersion.Dataset;
                            ApiDatasetModel datasetModel = apiDatasetHelper.GetContent(datasetVersion, id, version, dataset.MetadataStructure.Id, dataset.DataStructure.Id, dataset.EntityTemplate.Id);

                            model = ViewModel.Map(datasetModel);
                        
                            if (datasetVersion != null && datasetVersion.StateInfo != null)
                            {
                                model.IsValid = DatasetStateInfo.Valid.ToString().Equals(datasetVersion.StateInfo.State) ;
                            }

                            model.MetadataStructureId = datasetVersion.Dataset.MetadataStructure.Id;

                            //MetadataStructureManager msm = new MetadataStructureManager();
                            //dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

                            model.Title = datasetVersion.Title; // this function only needs metadata and extra fields, there is no need to pass the version to it.
                            model.Labels = getLabels(id, versionId, tag, datasetVersion.Dataset.EntityTemplate.Name);

                            if (datasetVersion.Dataset.DataStructure != null)
                                model.DataStructureId = datasetVersion.Dataset.DataStructure.Id;

     
                            // check if the user has download rights
                            model.DownloadAccess = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), id, RightType.Read).Result;

                            model.IsPublic = entityPermissionManager.ExistsAsync(entityTypeId.Value, id).Result;
                            // if the dataset is public, user or even no user has download rights
                            if (model.IsPublic) model.DownloadAccess = model.IsPublic;

                            // check if a reuqest of this dataset exist
                            if (!model.DownloadAccess)
                            {
                                model.RequestExist = hasOpenRequest(id);

                                if (UserExist() && hasRequestMapping(id))
                                {
                                    model.RequestAble = true;
                                    model.HasRequestRight = hasUserRequestRight();
                                }
                            }

                            // get data structure type
                            if (datasetVersion.Dataset.DataStructure != null && datasetVersion.Dataset.DataStructure.Self.GetType().Equals(typeof(StructuredDataStructure)))
                            {
                                dataStructureType = DataStructureType.Structured.ToString();
                                long c = datasetManager.RowCount(datasetVersion.Dataset.Id, null);
                                ViewData["gridTotal"] = c;
                                if (c > 0) model.HasData = true;
                            }
                            else
                            {
                                dataStructureType = DataStructureType.Unstructured.ToString();
                                if (datasetVersion.ContentDescriptors.Where(c => c.Name.Equals("unstructuredData")).Any())
                                {
                                    model.HasData = true;
                                }
                            }
                        }

                        #region settings
                        // load settings from ddm
                        var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
                        model.Settings.UseTags = Convert.ToBoolean(moduleSettings.GetValueByKey("use_tags"));
                        model.Settings.UseMinor = Convert.ToBoolean(moduleSettings.GetValueByKey("use_minor"));
                        model.Settings.DataAggrement = moduleSettings.GetValueByKey("data_aggreement").ToString();

                        // load all hooks for the edit view
                        HookManager hooksManager = new HookManager();
                        model.Settings.Hooks = hooksManager.GetHooksFor("dataset", "details", HookMode.view);

                        // run all checks
                        string userName = "";
                        if (HttpContext.User.Identity.IsAuthenticated)
                            userName = HttpContext.User.Identity.Name;

                        model.Settings.Hooks.ForEach(h => h.Check(id, userName));

                        #endregion
                    }

                }

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

        private async Task<long> getVersionId(long datasetId, int versionNr = 0, string versionName = "", double tagNr = 0)
        {

            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            bool useTags = false;
            bool.TryParse(moduleSettings.GetValueByKey("use_tags").ToString(), out useTags);

            return await DatasetVersionHelper.GetVersionId(datasetId, GetUsernameOrDefault(), versionNr, useTags, tagNr);

        }

        // requests
        private bool hasOpenRequest(long datasetId)
        {
            using (RequestManager requestManager = new RequestManager())
            using (DecisionManager decisionManager = new DecisionManager())
            using (SubjectManager subjectManager = new SubjectManager())
            using (EntityManager entityManager = new EntityManager())
            {
                if (HttpContext.User != null && HttpContext.User.Identity != null && !string.IsNullOrEmpty(HttpContext.User.Identity.Name))
                {
                    long userId = subjectManager.Subjects.Where(s => s.Name.Equals(HttpContext.User.Identity.Name)).Select(s => s.Id).First();
                    long entityId = entityManager.Entities.Where(e => e.Name.ToLower().Equals("dataset")).First().Id;

                    var request = requestManager.Requests.Where(r =>
                                            r.Applicant.Id.Equals(userId) &&
                                            r.Entity.Id.Equals(entityId) &&
                                            r.Key.Equals(datasetId) &&
                                            r.Status == Security.Entities.Requests.RequestStatus.Open).FirstOrDefault();

                    if (request != null) return true;
                }

                return false;
            }
        }

        private bool hasRequestMapping(long datasetId)
        {
            using (EntityManager entityManager = new EntityManager())
            using (PartyManager partyManager = new PartyManager())
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            using (PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager())
            {
                try
                {
                    var datasetPartyType = partyTypeManager.PartyTypes.Where(pt => pt.DisplayName.ToLower().Equals("dataset")).FirstOrDefault();

                    long partyId = partyManager.Parties.Where(p => p.PartyType.Id.Equals(datasetPartyType.Id) && p.Name.Equals(datasetId.ToString())).FirstOrDefault().Id;

                    var ownerPartyRelationshipType = partyRelationshipTypeManager.PartyRelationshipTypes.Where(pt => pt.Title.Equals(ModuleManager.GetModuleSettings("bam").GetValueByKey("OwnerPartyRelationshipType").ToString())).FirstOrDefault();
                    if (ownerPartyRelationshipType == null) return false;

                    var ownerRelationships = partyManager.PartyRelationships.Where(p =>
                    p.TargetParty.Id.Equals(partyId) &&
                    p.PartyRelationshipType.Id.Equals(ownerPartyRelationshipType.Id));

                    if (ownerRelationships == null) return false;

                    var exist = ownerRelationships.Count() > 0 ? true : false;
                    return exist;
                }
                catch (Exception ex)
                {
                    LoggerFactory.LogCustom(ex.Message);
                    return false;
                }
            }
        }

        private bool hasUserRequestRight()
        {
            using (var featurePermissionManager = new FeaturePermissionManager())
            using (var operationManager = new OperationManager())
            {
                var operation = operationManager.Find("DDM", "RequestsSend", "*");
                if (operation != null)
                {
                    var feature = operation.Feature;

                    if (feature != null)
                    {
                        var result = _userManager.FindByNameAsync(GetUsernameOrDefault());

                        if (featurePermissionManager.HasAccessAsync(result.Result?.Id, feature.Id).Result) return true;
                    }
                }
            }

            return false;
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

        private bool hasUserRights(long entityId, RightType rightType)
        {
            #region security permissions and authorizations check

            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            
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