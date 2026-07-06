using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Exceptions;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dim.Entities.Export;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.BIOSCHEMA;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dim.Helpers.Models;
using BExIS.Dim.Services;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Helpers.View;
using BExIS.Modules.Dcm.UI.Models.View;
using BExIS.Modules.Dim.UI.Helpers;
using BExIS.Modules.Dim.UI.Models;
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
using BExIS.Utils.Data.Helpers;
using BExIS.Utils.Data.Upload;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using BEXIS.JSON.Helpers;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NHibernate.Engine;
using NHibernate.Mapping.ByCode.Impl;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using System.Web.UI.WebControls;
using System.Xml;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class ViewController : Controller
    {

        private readonly UserManager _userManager;

        public ViewController(UserManager userManager)
        {
            _userManager = userManager;
        }


        #region View

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

            if(id==0) throw new ArgumentException("id is not valid");

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
        [JsonNetFilter]
        public JsonResult Load(long id, int version = 0, double tag = 0)
        {

            if (id == 0) throw new ArgumentException("id is not valid");
            var user = BExISAuthorizeHelper.GetUserFromAuthorizationAsync(HttpContext).Result;

          
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            ApiDatasetHelper apiDatasetHelper = new ApiDatasetHelper();

            ViewModel model = new ViewModel();
            model.Id = id;

            // set User 
         
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

                    if (datasetManager.IsDatasetDeleted(id))
                    {
                        throw new EntityDeletedException("Entity is deleted.");
                    }

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
                            // check if its public or not
                            model.IsPublic = entityPermissionManager.ExistsAsync(entityTypeId.Value, id).Result;

                            // stop loading data if the dataset is not public and no user is logged in
                            if (model.IsPublic == false && user == null)
                            {
                                throw new EntityForbiddenException("You do not have permission to access this Entity.");
                            }


                            datasetVersion = datasetManager.DatasetVersionRepo.Get(versionId); // this is needed to allow dsv to access to an open session that is available via the repo
                            var dataset = datasetVersion.Dataset;
                            long datastructureId = dataset.DataStructure != null ? dataset.DataStructure.Id : -1;
                            ApiDatasetModel datasetModel = apiDatasetHelper.GetContent(datasetVersion, id, version, dataset.MetadataStructure.Id, datastructureId, dataset.EntityTemplate.Id);

                            model = ViewModel.Map(datasetModel);

                            if (datasetVersion != null && datasetVersion.StateInfo != null)
                            {
                                model.IsValid = DatasetStateInfo.Valid.ToString().Equals(datasetVersion.StateInfo.State);
                            }

                            model.MetadataStructureId = datasetVersion.Dataset.MetadataStructure.Id;

                            //MetadataStructureManager msm = new MetadataStructureManager();
                            //dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

                            model.Title = datasetVersion.Title; // this function only needs metadata and extra fields, there is no need to pass the version to it.
                            model.Labels = getLabels(id, versionId, tag, datasetVersion.Dataset.EntityTemplate.Name);

                            if (datasetVersion.Dataset.DataStructure != null)
                                model.DataStructureId = datasetVersion.Dataset.DataStructure.Id;


                            // check if the user has download rights

                            model.DownloadAccess = entityPermissionManager.HasEffectiveRightsAsync(BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext), typeof(Dataset), id, RightType.Read).Result;

                            model.HasEditRight = entityPermissionManager.HasEffectiveRightsAsync(BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext), typeof(Dataset), id, RightType.Write).Result;
               

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



                    if (version > 0)
                    {
                        // load BioSchema Description if exist
                        string bioschemadescription = getBioSchema(id, version);
                        if (!string.IsNullOrEmpty(bioschemadescription))
                            ViewData["bioSchema"] = bioschemadescription;
                    }
                }
                else
                {
                    if (datasetManager.IsDatasetCheckedIn(id)) // in process
                    {
                        throw new EntityLockedException("Entity is currently in Process");
                    }
                }


                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        public JsonResult LoadDeleted(long id, int version = 0, double tag = 0)
        {
            if (id == 0) throw new ArgumentException("id is not valid");

            DeletedModel model = new DeletedModel();
            // Load deleted dataset details here
            using (var datasetManager = new DatasetManager())
            using (EntityManager entityManager = new EntityManager())
            {
                // Retrieve data for active and hidden (marked as deleted) datasets
                if (datasetManager.IsDatasetDeleted(id))
                {

                    List<DatasetVersion> datasetVersions = datasetManager.GetDatasetVersions(id);
                    List<DatasetVersion> datasetVersionsAllowed = new List<DatasetVersion>();

                    if (datasetManager.IsDatasetDeleted(id))
                    {
                        var deletedVersion = datasetManager.GetDeletedDatasetLatestVersion(id);
                        string title = deletedVersion != null ? deletedVersion.Title : "n.a.";
                        model.Id = id;
                        model.Title = title;

                        long entityTypeId = deletedVersion.Dataset.EntityTemplate.EntityType.Id;

                        // get links
                        EntityReferenceHelper entityReferenceHelper = new EntityReferenceHelper();
                        model.Links.From = entityReferenceHelper.GetSourceReferences(id, entityTypeId);
                        model.Links.To = entityReferenceHelper.GetTargetReferences(id, entityTypeId);


                    }
                }


                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        // load bioschema
        public JsonResult BioSchema(long id, int version)
        {
            string bioschema = getBioSchema(id, version);
            return Json(bioschema, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        private string getBioSchema(long id, int version)
        {
            if (id <= 0) throw new ArgumentException("id is not valid");
            ViewData["Id"] = id;

            var helper = new BioSchemaHelper();
            string json = helper.GetBioSchemaForDataset(id, version, HttpContext.Request.Url.ToString());

            return json; // Replace "_PartialViewName" with your actual name

        }

        [JsonNetFilter]
        public JsonResult Citation(long id, int version)
        {
            // default setup for citation model if something goes wrong
            CitaionModelJson model = new CitaionModelJson()
            {
                Format = ReadCitationFormat.Default,
                Data = new CitationDataModel()
                {
                    Title = "Title is not available."
                }
            };

            try
            {
                using (var datasetManager = new DatasetManager())
                using (var conceptManager = new ConceptManager())
                {
                    var dataset = datasetManager.GetDataset(id);
                    DatasetVersion datasetVersion = null;

                    long datasetVersionId = 0;
                    if(version>0)
                        datasetVersionId = datasetManager.GetDatasetVersionId(id, version);
                    else
                        datasetVersionId = datasetManager.GetDatasetLatestVersionId(id); 

                    if (dataset.Status == DatasetStatus.Deleted)
                    {
                        datasetVersion = datasetManager.GetDeletedDatasetLatestVersion(id);
                    }
                    else
                    {
                        datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);
                    }

                    if (datasetVersion == null)
                    {
                        return Json(model, JsonRequestBehavior.AllowGet);
                    }

                    var settingsHelper = new DDMSettingsHelper();
                    var citationSettings = settingsHelper.GetCitationSettings();

                    var errors = new List<string>();
                    string conceptName = "Citation_" + citationSettings.ReadCitationFormat;
                    var concept = conceptManager.FindByName(conceptName);

                    model.Data = CitationsHelper.CreateCitationDataModel(datasetVersion);

                    if (model.Data == null)
                    { 
                        model.Data = new CitationDataModel()
                        {
                            Title = datasetVersion.Title
                        };
                    }

                    if (citationSettings == null || !citationSettings.ShowCitation || concept == null || !MappingUtils.IsMapped(datasetVersion.Dataset.MetadataStructure.Id, LinkElementType.MetadataStructure, concept.Id, LinkElementType.MappingConcept, out errors))
                    {
                        //get data not from a
                        ApiDatasetHelper apiDatasetHelper = new ApiDatasetHelper();
                        ApiDatasetModel datasetModel = apiDatasetHelper.GetContent(datasetVersion, id, version, dataset.MetadataStructure.Id, dataset.DataStructure?.Id ?? 0, dataset.EntityTemplate.Id);

                        // authors
                        if (datasetModel.AdditionalInformations.ContainsKey(Key.Author.ToString()))
                        {
                            model.Data.Authors = datasetModel.AdditionalInformations[Key.Author.ToString()].Split(',').ToList();
                        }


                        return Json(model, JsonRequestBehavior.AllowGet);
                    }

                    if (!CitationsHelper.IsCitationDataModelValid(model.Data))
                    {

                        return Json(model, JsonRequestBehavior.AllowGet);
                    }


                    model.Format = citationSettings.ReadCitationFormat;
                    
                    return Json(model, JsonRequestBehavior.AllowGet);
                    
                }
            }
            catch (Exception ex)
            {
                return Json(model, JsonRequestBehavior.AllowGet);
            }

        }

        [JsonNetFilter]
        public JsonResult GetCitationOptions(long id, int version, double tag)
        {
            if(id<=0)throw new ArgumentException("id is not valid");
            CitationFormatOptions model  = new CitationFormatOptions();

            using (var datasetManager = new DatasetManager())
            {
                var datasetVersion = datasetManager.GetDatasetVersion(id, version);

                if (datasetVersion == null) throw new ArgumentException("Version noi");

                long datastructureId = datasetVersion.Dataset.DataStructure != null ? datasetVersion.Dataset.DataStructure.Id : -1;
                var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
                var useTags = Convert.ToBoolean(moduleSettings.GetValueByKey("use_tags"));
               var useMinor = Convert.ToBoolean(moduleSettings.GetValueByKey("use_minor"));

                string filename = IOHelper.GetFileName(FileType.Citation, id, version, datastructureId, "", tag, useTags, useMinor);
                model.FileName = filename;

                foreach (CitationFormat format in Enum.GetValues(typeof(CitationFormat)))
                {

                    CitationDataModel m = CitationsHelper.CreateCitationDataModel(datasetVersion, format);
                    if (CitationsHelper.IsCitationDataModelValid(m))
                    {
                        string f = "";

                        switch (format.ToString())
                        {
                            case "APA":
                                f = "apa";
                                break;
                            case "RIS":
                                f = "ris";
                                break;
                            case "Text":
                                f = "txt";
                                break;
                            case "Bibtex":
                                f = "bib";
                                break;
                            //default: f = "txt";
                            default: f = "txt";
                                break;
                        };

                        model.Formats.Add(new CitationListItem()
                        {
                            Label = format.ToString(),
                            Format = f,
                            Value = format
                        });

                    }
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }


        #region version

        [JsonNetFilter]
        public JsonResult Tags(long id, int version)
        {
            if (id <= 0) throw new ArgumentException("id is not valid");

            List<TagInfoViewModel> tags = new List<TagInfoViewModel>();
            bool hasEditRights = hasUserRights(id, RightType.Write);

            if (version == 0) return Json(tags, JsonRequestBehavior.AllowGet); // return empty list



            using (DatasetManager datasetmanager = new DatasetManager())
            {
                TagInfoHelper _helper = new TagInfoHelper();
                var versions = datasetmanager.GetDatasetVersions(id);

                var currentVersion = datasetmanager.GetDatasetVersion(id, version);

                if (versions != null)
                {
                    tags = _helper.GetViews(versions, datasetmanager, !hasEditRights);
                }
            }

            return Json(tags, JsonRequestBehavior.AllowGet); // Replace "_PartialViewName" with your actual name

        }

        [JsonNetFilter]
        public JsonResult Versions(long id)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            {

                List<VersionListeItem> tmp = new List<VersionListeItem>();
                List<DatasetVersion> datasetVersionsAllowed = new List<DatasetVersion>();
                List<DatasetVersion> datasetVersions = datasetManager.GetDatasetVersions(id).OrderByDescending(d => d.Id).ToList();

                SettingsHelper helper = new SettingsHelper();

                EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
                bool hasEditPermission = false;

                if (GetUsernameOrDefault() != "DEFAULT")
                {
                    hasEditPermission = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), id, RightType.Write).Result;
                }

                // user has edit permission and can see all versions -> show full list
                var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
                if (hasEditPermission || !Convert.ToBoolean(moduleSettings.GetValueByKey("reduce_versions_select_logged_in")))
                {
                    datasetVersionsAllowed = datasetVersions;
                }
                // user is not logged in or has no edit permission -> show reduced list
                else
                {
                    datasetVersionsAllowed = datasetManager.GetDatasetVersionsAllowed(id, true, false, datasetVersions).OrderByDescending(d => d.Id).ToList();
                }

                // use reduced/ or full list, but allways create version number from full list.
                datasetVersionsAllowed.ForEach(d => tmp.Add(
                    new VersionListeItem()
                    {
                        Description = CreateVersionNumber(d, datasetVersions) + " " + getVersionInfo(d),
                        Id = (datasetVersions.Count - datasetVersions.IndexOf(d)),
                        Text = d.Title,
                        Date = d.Timestamp.ToString("dd.MM.yyyy")
                    }
                    ));

                return Json(tmp, JsonRequestBehavior.AllowGet);
            }

        }

        private static string CreateVersionNumber(DatasetVersion d, List<DatasetVersion> dsvs)
        {
            if (d.VersionType != null) // add version name, if version type is given and show version nummer in ()
            {
                return d.VersionName.ToString() + " (" + (dsvs.Count - dsvs.IndexOf(d)).ToString() + ")";
            }
            else
            {
                return (dsvs.Count - dsvs.IndexOf(d)).ToString();
            }
        }

        private string createEditedBy(string performer)
        {
            using (var partyManager = new PartyManager())
            {
                var user_performer = _userManager.FindByNameAsync(performer);

                // Replace account name by party name if exists
                if (user_performer.Result != null)
                {
                    Party party = partyManager.GetPartyByUser(user_performer.Result.Id);

                    if (party != null)
                    {
                        performer = party.Name;
                    }
                }

                // check if a user is logged in, if not do not show performer
                var user = GetUsernameOrDefault();
                if (user != "DEFAULT")
                {
                    return "by " + performer + ", ";
                }
                else
                {
                    return "";
                }
            }
        }

        private string getVersionInfo(DatasetVersion d)
        {
            StringBuilder sb = new StringBuilder();

            // modification, Performer and Comment exists (as indication for new version type tracking)
            if (d.ModificationInfo != null &&
                !string.IsNullOrEmpty(d.ModificationInfo.Performer) &&
                !string.IsNullOrEmpty(d.ModificationInfo.Comment))
            {
                // Metadata cration & edit
                if (d.ModificationInfo.Comment.Equals("Metadata") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Create)
                {
                    sb.Append(String.Format("Metadata creation ({0}{1})", createEditedBy(d.ModificationInfo.Performer), d.Timestamp.ToString("dd.MM.yyyy")));
                }
                else if (d.ModificationInfo.Comment.Equals("Metadata") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Edit)
                {
                    sb.Append(String.Format("Metadata edited ({0}{1})", createEditedBy(d.ModificationInfo.Performer), d.Timestamp.ToString("dd.MM.yyyy")));
                }

                //unstructured file upload & delete
                else if (d.ModificationInfo.Comment.Equals("File") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Create)
                {
                    sb.Append(String.Format("File uploaded: {0} ({1}{2})", Truncate(d.ChangeDescription, 30), createEditedBy(d.ModificationInfo.Performer), d.Timestamp.ToString("dd.MM.yyyy")));
                }
                else if (d.ModificationInfo.Comment.Equals("File") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Delete)
                {
                    sb.Append(String.Format("File deleted: {0} ({1}{2})", Truncate(d.ChangeDescription, 30), createEditedBy(d.ModificationInfo.Performer), d.Timestamp.ToString("dd.MM.yyyy")));
                }

                // structured data import & update & delete
                else if (d.ModificationInfo.Comment.Equals("Data") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Create)
                {
                    sb.Append(String.Format("Data imported: {0} ({1}{2})", Truncate(d.ChangeDescription, 30), createEditedBy(d.ModificationInfo.Performer), d.Timestamp.ToString("dd.MM.yyyy")));
                }
                else if (d.ModificationInfo.Comment.Equals("Data") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Edit)
                {
                    sb.Append(String.Format("Data added: {0} ({1}{2})", Truncate(d.ChangeDescription, 30), createEditedBy(d.ModificationInfo.Performer), d.Timestamp.ToString("dd.MM.yyyy")));
                }
                else if (d.ModificationInfo.Comment.Equals("Data") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Delete)
                {
                    sb.Append(String.Format("Data deleted ({0}{1})", createEditedBy(d.ModificationInfo.Performer), d.Timestamp.ToString("dd.MM.yyyy")));
                }

                // attachment
                else if (d.ModificationInfo.Comment.Equals("Attachment") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Create)
                {
                    sb.Append(String.Format("Attachment uploaded: {0} ({1}{2})", Truncate(d.ChangeDescription, 30), createEditedBy(d.ModificationInfo.Performer), d.Timestamp.ToString("dd.MM.yyyy")));
                }
                else if (d.ModificationInfo.Comment.Equals("Attachment") && d.ModificationInfo.ActionType == Vaiona.Entities.Common.AuditActionType.Delete)
                {
                    sb.Append(String.Format("Attachment deleted: {0} ({1}{2})", Truncate(d.ChangeDescription, 30), createEditedBy(d.ModificationInfo.Performer), d.Timestamp.ToString("dd.MM.yyyy")));
                }
                else
                {
                    sb.Append(d.ModificationInfo.Comment);
                    sb.Append(" - ");
                    sb.Append(d.ModificationInfo.ActionType);
                    sb.Append(" - ");
                    sb.Append(createEditedBy(d.ModificationInfo.Performer));

                    // both exits - needs separator
                    if (d.ModificationInfo != null &&
                        string.IsNullOrEmpty(d.ModificationInfo.Performer) &&
                        !string.IsNullOrEmpty(d.ModificationInfo.Comment) &&
                        !string.IsNullOrEmpty(d.ChangeDescription))
                    {
                        sb.Append(" : ");
                    }

                    //change description is not null or empty
                    if (!string.IsNullOrEmpty(d.ChangeDescription))
                    {
                        sb.Append(Truncate(d.ChangeDescription, 30));
                    }
                }
            }
            else
            {
                sb.Append(String.Format("{0} ({1})", Truncate(d.ChangeDescription, 30), d.Timestamp.ToString("dd.MM.yyyy")));
            }

            return sb.ToString();
        }

        public string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        private async Task<long> getVersionId(long datasetId, int versionNr = 0, string versionName = "", double tagNr = 0)
        {

            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            bool useTags = false;
            bool.TryParse(moduleSettings.GetValueByKey("use_tags").ToString(), out useTags);

            return await DatasetVersionHelper.GetVersionId(datasetId, GetUsernameOrDefault(), versionNr, useTags, tagNr);

        }


        #endregion

        #region request
        private bool hasOpenRequest(long datasetId)
        {
            using (RequestManager requestManager = new RequestManager())
            using (DecisionManager decisionManager = new DecisionManager())
            using (SubjectManager subjectManager = new SubjectManager())
            using (EntityManager entityManager = new EntityManager())
            {
                User user = BExISAuthorizeHelper.GetUserFromAuthorizationAsync(HttpContext).Result;
                if (user!=null)
                {
                    long userId = user.Id;
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

        #endregion

        #region download

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult DownloadZip(long id, string format, long version = -1, bool withFilter = false, bool withUnits = false)
        {
            if (this.IsAccessible("DIM", "Export", "GenerateZip"))
            {
                var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
                bool useTags = (Boolean)moduleSettings.GetValueByKey("use_tags");
                bool useMinorTag = (Boolean)moduleSettings.GetValueByKey("use_minor");

                var actionresult = this.Run("DIM", "Export", "GenerateZip", new RouteValueDictionary() { { "id", id }, { "versionid", version }, { "format", format }, { "withFilter", withFilter }, { "withUnits", withUnits }, { "useTags", useTags }, { "useMinor", useMinorTag } });

                Response.AppendHeader("Access-Control-Expose-Headers", "Content-Disposition");

                return actionresult;
            }

            return Json(false);
        }

        #endregion


        #endregion about view

        #region metadata
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

            return RedirectToAction("metadata", "view", new { area = "DCM", id, version});
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult Metadata(long id, int version = 0, double tag = 0)
        {

            // if version is 0 , get latest version, otherwise get the specified version
            long versionId = getVersionId(id, version, "", tag).Result;

            // get version based on version id
            using (var datasetManager = new DatasetManager())
            {
                version = datasetManager.GetDatasetVersionNr(versionId);
            }

            string module = "DCM";

            ViewData["id"] = id;
            ViewData["version"] = version;
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        #region download

        //html

        public ActionResult DownloadAsHtml(long id, int version)
        {


            return Content("not implemented.");
        }

        //flatten


        //json
        public ActionResult DownloadAsJson(long id, int version)
        {
            try
            {
                string metadata = OutputMetadataManager.GetMetadataAsJson(id, version, 2);

                byte[] bytes = Encoding.ASCII.GetBytes(metadata);

                return File(bytes, "application/json");

            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }


        //xml
        public ActionResult DownloadAsXml(long id, int version)
        {


            return Content("no metadata xml file is loaded.");
        }


        #endregion

        #region import

        [HttpPost]
        public JsonResult Import(long id)
        {
            #region check incomming metadata
            string errorMessage = "";

            if (Request.Files.Count > 0)
            {
                using (var datasetManager = new DatasetManager())
                {
                    Dataset dataset = datasetManager.GetDataset(id);
                    long metadataStructureId = dataset.MetadataStructure.Id;

                    Stream requestStream;
                    HttpFileCollectionBase files = Request.Files;
                    var file = files[0]; // one file only
                    requestStream = file.InputStream;
                    #endregion check incomming metadata
                    string contentType = file.ContentType;
                    XmlDocument completeMetadata = null;
                    JSchema schema;
                    XmlMetadataConverter converter = new XmlMetadataConverter();
                    MetadataStructureConverter metadataStructureConverter = new MetadataStructureConverter();
                    long mdid = 0;

                    if (contentType.Contains("xml"))
                    {
                        #region application/xml

                        XmlDocument metadataForImport = new XmlDocument();
                        metadataForImport.Load(requestStream);

                        if (metadataForImport.DocumentElement.HasAttribute("id"))
                        {
                            mdid = Convert.ToInt64(metadataForImport.DocumentElement.GetAttribute("id"));
                        }

                        if (mdid == metadataStructureId)
                        {

                            // metadataStructure ID

                            var metadataStructrueName = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId).Name;

                            // loadMapping file
                            var path_mappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), XmlMetadataImportHelper.GetMappingFileName(metadataStructureId, TransmissionType.mappingFileImport, metadataStructrueName));

                            // XML mapper + mapping file
                            var xmlMapperManager = new XmlMapperManager(TransactionDirection.ExternToIntern);
                            xmlMapperManager.Load(path_mappingFile, "IDIV");

                            // generate intern metadata without internal attributes
                            var metadataResult = xmlMapperManager.Generate(metadataForImport, 1, true);

                            // generate intern template metadata xml with needed attribtes
                            var xmlMetadatWriter = new XmlMetadataWriter(BExIS.Xml.Helpers.XmlNodeMode.xPath);
                            var metadataXml = xmlMetadatWriter.CreateMetadataXml(metadataStructureId,
                                XmlUtility.ToXDocument(metadataResult));

                            var metadataXmlTemplate = XmlMetadataWriter.ToXmlDocument(metadataXml);

                            // set attributes FROM metadataXmlTemplate TO metadataResult
                            completeMetadata = XmlMetadataImportHelper.FillInXmlValues(metadataResult,
                                metadataXmlTemplate);

                        }
                        else
                        {
                            Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                            errorMessage = "The metadata ID is either invalid or does not match the expected structure ID ({metadataStructureId}).";
                        }

                        #endregion application/xml
                    }
                    else
                    if (contentType.Contains("json"))
                    {
                        #region application/json

                        using (var streamReader = new StreamReader(requestStream))
                        using (var jsonReader = new JsonTextReader(streamReader))
                        {
                            JsonSerializer serializer = new JsonSerializer();

                            try
                            {
                                JObject metadataJson = serializer.Deserialize<JObject>(jsonReader);


                                if (metadataJson.ContainsKey("@id"))
                                {
                                    if (Int64.TryParse(metadataJson.Property("@id").Value.ToString(), out mdid) && mdid == metadataStructureId)
                                    {
                                        schema = metadataStructureConverter.ConvertToJsonSchema(mdid);

                                        List<string> notAllowedElements = new List<string>();
                                        if (converter.HasValidStructure(metadataJson, mdid, out notAllowedElements))
                                        {
                                            completeMetadata = converter.ConvertTo(metadataJson);

                                        }
                                        else
                                        {
                                            Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                                            errorMessage = "the json does not have the expected structure";
                                        }
                                    }
                                    else
                                    {
                                        Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                                        errorMessage = string.Format("The metadata ID is either invalid or does not match the expected structure ID ({0}).", metadataStructureId);
                                    }

                                }
                                else
                                {
                                    Response.StatusCode = (int)HttpStatusCode.ExpectationFailed;
                                    errorMessage = "the json does not contain any information about the metadata structure";
                                }
                            }
                            catch (JsonReaderException)
                            {
                                Console.WriteLine("Invalid JSON.");
                            }
                        }

                        #endregion application/json
                    }

                    if (completeMetadata != null)
                    {
                        HttpStatusCode statusCode = HttpStatusCode.OK;

                        string json = "";

                        json = OutputMetadataManager.GetMetadataAsJson(completeMetadata, 1);
                        return Json(json);

                    }

                }
            }
      

            // 2. Return the JSON error payload
            return Json(new
            {
                success = false,
                message = errorMessage, // Optional: Remove this in production for security reasons
                error = errorMessage // Optional: Remove this in production for security reasons
            }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #endregion

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
            User user = BExISAuthorizeHelper.GetUserFromAuthorizationAsync(HttpContext).Result;

            if(user != null) return true;

            return false;
        }

        public string GetUsernameOrDefault()
        {
            User user = BExISAuthorizeHelper.GetUserFromAuthorizationAsync(HttpContext).Result;
            string username = user?.Name?? "DEFAULT";
            return username;
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