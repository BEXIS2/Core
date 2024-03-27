using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Services.Party;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Ddm.UI.Helpers;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Requests;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Helpers;
using BExIS.UI.Hooks;
using BExIS.Utils.Config;
using BExIS.Utils.NH.Querying;
using BExIS.Xml.Helpers;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Dlm.Services.MetadataStructure;
using System.Web.UI.WebControls;

namespace BExIS.Modules.Ddm.UI.Controllers

{
    public class DataController : BaseController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();


        [BExISEntityAuthorize(typeof(Dataset), "datasetId", RightType.Grant)]
        public ActionResult DatasetPermissions(long datasetId)
        {
            var entityManager = new EntityManager();

            try
            {
                if (this.IsAccessible("SAM", "UserPermissions", "Subjects"))
                {
                    var result = this.Render("SAM", "UserPermissions", "Subjects",
                        new RouteValueDictionary() { { "EntityId", entityManager.FindByName("Dataset").Id }, { "InstanceId", datasetId } });

                    return Content(result.ToHtmlString(), "text/html");
                }
                else
                {
                    return PartialView("Error");
                }
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public JsonResult IsDatasetCheckedIn(long id)
        {
            using (DatasetManager dm = new DatasetManager())
            {
                if (id != -1 && dm.IsDatasetCheckedIn(id))
                    return Json(true);
                else
                    return Json(false);
            }
        }

        public ActionResult Show(long id, long version = 0)
        {
            //get the researchobject (cuurently called dataset) to get the id of a metadata structure
            Dataset researcobject = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(id);

            if (researcobject != null)
            {
                long metadataStrutcureId = researcobject.MetadataStructure.Id;

                using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
                {
                    string entityName = xmlDatasetHelper.GetEntityNameFromMetadatStructure(metadataStrutcureId, metadataStructureManager);
                    string entityType = xmlDatasetHelper.GetEntityTypeFromMetadatStructure(metadataStrutcureId, metadataStructureManager);

                    //ToDo in the entity table there must be the information
                    using (EntityManager entityManager = new EntityManager())
                    {
                        var entity = entityManager.Entities.Where(e => e.Name.Equals(entityName)).FirstOrDefault();

                        string moduleId = "";
                        Tuple<string, string, string> action = null;
                        string defaultAction = "ShowData";

                        if (entity != null && entity.Extra != null)
                        {
                            var node = entity.Extra.SelectSingleNode("extra/modules/module");

                            if (node != null) moduleId = node.Attributes["value"].Value;

                            string modus = "show";

                            action = EntityViewerHelper.GetEntityViewAction(entityName, moduleId, modus);
                        }
                        if (action == null) RedirectToAction(defaultAction, new { id, version });

                        try
                        {
                            if (version == 0) return RedirectToAction(action.Item3, action.Item2, new { area = action.Item1, id });
                            else return RedirectToAction(action.Item3, action.Item2, new { area = action.Item1, id, version });
                        }
                        catch
                        {
                            return RedirectToAction(defaultAction, new { id, version });
                        }
                    }
                }
            }

            ModelState.AddModelError("", string.Format("The object with the id {0} does not exist", id));

            return View("Show");
        }

        public ActionResult ShowData(long id, int version = 0, bool asPartial = false, string versionName = "")
        {
            using (DatasetManager dm = new DatasetManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (EntityManager entityManager = new EntityManager())
            {
                Dataset researcobject = dm.GetDataset(id);

                if (researcobject != null)
                {
                    DatasetVersion dsv = null;
                    ShowDataModel model = new ShowDataModel();

                    string title = "";
                    long metadataStructureId = -1;
                    long dataStructureId = -1;
                    long researchPlanId = 1;
                    long versionId = 0;

                    string dataStructureType = DataStructureType.Structured.ToString();
                    bool downloadAccess = false;
                    bool requestExist = false;
                    bool requestAble = false;
                    bool hasRequestRight = false;
                    bool latestVersion = false;
                    long latestVersionId = 0;
                    long latestVersionNr = 0;
                    string isValid = "no";
                    bool isPublic = false;

                    XmlDocument metadata = new XmlDocument();


                    // Retrieve data for active and hidden (marked as deleted) datasets
                    if (dm.IsDatasetCheckedIn(id) || dm.IsDatasetDeleted(id))
                    {
                        List<DatasetVersion> datasetVersions = dm.GetDatasetVersions(id);
                        List<DatasetVersion> datasetVersionsAllowed = new List<DatasetVersion>();

                        // Get version id based on public or internal access. Version name has a higher priority as version.
                        // Public access has higher priority as major/minor versions
                        versionId = getVersionId(id, version, versionName, datasetVersions, researcobject.Status);
                        // Set if the latest version is selected. Compare current version id against unfiltered max id
                        latestVersionId = datasetVersions.OrderByDescending(d => d.Timestamp).Select(d => d.Id).FirstOrDefault();
                        latestVersionNr = dm.GetDatasetVersionNr(latestVersionId);
                        latestVersion = (versionId == latestVersionId);

                        // Get version number based on version id
                        if (versionId != 0)
                        {
                            version = dm.GetDatasetVersionNr(versionId);
                        }

                        // Throw error if no version id was found.
                        if (versionId == 0)
                        {
                            ModelState.AddModelError("", string.Format("The version with the requested name {1} or id {0} does not exist or is not publicly accessible", version, versionName));
                        }
                        else
                        {
                            dsv = dm.DatasetVersionRepo.Get(versionId); // this is needed to allow dsv to access to an open session that is available via the repo

                            if (dsv != null && dsv.StateInfo != null)
                            {
                                isValid = DatasetStateInfo.Valid.ToString().Equals(dsv.StateInfo.State) ? "yes" : "no";
                            }

                            metadataStructureId = dsv.Dataset.MetadataStructure.Id;

                            //MetadataStructureManager msm = new MetadataStructureManager();
                            //dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

                            title = dsv.Title; // this function only needs metadata and extra fields, there is no need to pass the version to it.
                            if (dsv.Dataset.DataStructure != null)
                                dataStructureId = dsv.Dataset.DataStructure.Id;

                            researchPlanId = dsv.Dataset.ResearchPlan.Id;
                            metadata = dsv.Metadata;

                            // check if the user has download rights
                            downloadAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), id, RightType.Read);

                            // check if a reuqest of this dataset exist
                            if (!downloadAccess)
                            {
                                requestExist = HasOpenRequest(id);

                                if (UserExist() && HasRequestMapping(id))
                                {
                                    requestAble = true;
                                    hasRequestRight = hasUserRequestRight();
                                }
                            }

                            // check is public
                            long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                            entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;

                            isPublic = entityPermissionManager.Exists(null, entityTypeId.Value, id);

                            // get data structure type

                            if (dsv.Dataset.DataStructure != null && dsv.Dataset.DataStructure.Self.GetType().Equals(typeof(StructuredDataStructure)))
                            {
                                dataStructureType = DataStructureType.Structured.ToString();
                                ViewData["gridTotal"] = dm.RowCount(dsv.Dataset.Id, null);
                            }
                            else
                            {
                                dataStructureType = DataStructureType.Unstructured.ToString();
                            }

                            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Show " + typeof(Dataset).Name + ": " + title, this.Session.GetTenant());
                        }
                    }
                    else
                    {
                            ModelState.AddModelError(string.Empty, "Dataset is just in processing.");
                    }

                    model = new ShowDataModel()
                    {
                        Id = id,
                        Version = version,
                        VersionSelect = version,
                        VersionId = versionId,
                        LatestVersion = latestVersion,
                        LatestVersionNumber = latestVersionNr,
                        Title = title,
                        MetadataStructureId = metadataStructureId,
                        DataStructureId = dataStructureId,
                        ResearchPlanId = researchPlanId,
                        ViewAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), id, RightType.Read),
                        GrantAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), id, RightType.Grant),
                        HasEditRight = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), id, RightType.Write),
                        DataStructureType = dataStructureType,
                        DownloadAccess = downloadAccess,
                        RequestExist = requestExist,
                        RequestAble = requestAble,
                        HasRequestRight = hasRequestRight,
                        IsPublic = isPublic
                    };

                   //set metadata in session
                    Session["ShowDataMetadata"] = metadata;
                    ViewData["VersionSelect"] = getVersionsSelectList(id, dm);
                    ViewData["isValid"] = isValid;
                    ViewData["datasetSettings"] = getSettingsDataset();
                    ViewData["Message"] = "";
                    ViewData["State"] = "";

                    // set message and unset all tabs except of metadata (+ data structure & links)
                    if (dm.IsDatasetDeleted(id))
                    {
                        ViewData["Message"] = "The dataset has been withdrawn. Reason: " + researcobject.ModificationInfo.Comment + ". Please check the \'Links\' section if a new version is available.";
                        ViewData["State"] = "hidden";
                        model.GrantAccess = false;
                        model.ViewAccess = false;
                        model.DownloadAccess = false;
                    }

                    // load all hooks for the edit view
                    HookManager hooksManager = new HookManager();
                    model.Hooks = hooksManager.GetHooksFor("dataset", "details", HookMode.view);

                    // run all checks
                    string userName = "";
                    if (HttpContext.User.Identity.IsAuthenticated)
                        userName = HttpContext.User.Identity.Name;

                    model.Hooks.ForEach(h => h.Check(id, userName));

                    // add informations disbaled hooks from the enity template
                    // based on the entity template, hooks can be disabled.
                    foreach (var hook in model.Hooks)
                    {
                        if (dsv != null && dsv.Dataset.EntityTemplate.DisabledHooks != null && dsv.Dataset.EntityTemplate.DisabledHooks.Contains(hook.DisplayName))
                            hook.Status = HookStatus.Disabled;
                    }

                    if (asPartial) return PartialView(model);

                    return View(model);
                }

                ModelState.AddModelError("", string.Format("The dataset with the id {0} does not exist", id));

                return View();
            }
        }

        public ActionResult Reload(long id, int version = 0)
        {
            DatasetManager dm = new DatasetManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

            try
            {
                DatasetVersion dsv;
                ShowDataModel model = new ShowDataModel();

                string title = "";
                long metadataStructureId = -1;
                long dataStructureId = -1;
                long researchPlanId = 1;
                long versionId = 0;
                string dataStructureType = DataStructureType.Structured.ToString();
                bool downloadAccess = false;
                bool requestExist = false;
                bool requestAble = false;
                bool latestVersion = false;
                long latestVersionId = 0;
                long latestVersionNr = 0;
                string isValid = "no";

                XmlDocument metadata = new XmlDocument();

                if (dm.IsDatasetCheckedIn(id))
                {
                    //get latest version
                    if (version == 0)
                    {
                        versionId = dm.GetDatasetLatestVersionId(id); // check for zero value
                        //get current version number
                        version = dm.GetDatasetVersions(id).OrderBy(d => d.Timestamp).Count();

                        latestVersion = true;
                    }
                    // get specific version
                    else
                    {
                        versionId = dm.GetDatasetVersions(id).OrderBy(d => d.Timestamp).Skip(version - 1).Take(1).Select(d => d.Id).FirstOrDefault();
                        latestVersionId = dm.GetDatasetLatestVersionId(id);
                        latestVersionNr = dm.GetDatasetVersionNr(latestVersionId);
                        latestVersion = versionId == dm.GetDatasetLatestVersionId(id);
                    }

                    dsv = dm.DatasetVersionRepo.Get(versionId); // this is needed to allow dsv to access to an open session that is available via the repo

                    metadataStructureId = dsv.Dataset.MetadataStructure.Id;

                    //MetadataStructureManager msm = new MetadataStructureManager();
                    //dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

                    title = dsv.Title; // this function only needs metadata and extra fields, there is no need to pass the version to it.
                    dataStructureId = dsv.Dataset.DataStructure.Id;
                    researchPlanId = dsv.Dataset.ResearchPlan.Id;
                    metadata = dsv.Metadata;

                    if (dsv.StateInfo != null)
                    {
                        isValid = DatasetStateInfo.Valid.ToString().Equals(dsv.StateInfo.State) ? "yes" : "no";
                    }

                    // check if the user has download rights
                    downloadAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), id, RightType.Read);

                    // check if a reuqest of this dataset exist
                    if (!downloadAccess)
                    {
                        requestExist = HasOpenRequest(id);

                        if (UserExist() && HasRequestMapping(id)) requestAble = true;
                    }

                    if (dsv.Dataset.DataStructure.Self.GetType().Equals(typeof(StructuredDataStructure)))
                    {
                        dataStructureType = DataStructureType.Structured.ToString();
                        ViewData["gridTotal"] = dm.RowCount(dsv.Dataset.Id, null);
                    }
                    else
                    {
                        dataStructureType = DataStructureType.Unstructured.ToString();
                    }

                    ViewBag.Title = PresentationModel.GetViewTitleForTenant("Show " + typeof(Dataset).Name + ": " + title, this.Session.GetTenant());
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Dataset is just in processing.");
                }

                model = new ShowDataModel()
                {
                    Id = id,
                    Version = version,
                    VersionSelect = version,
                    VersionId = versionId,
                    LatestVersion = latestVersion,
                    LatestVersionNumber = latestVersionNr,
                    Title = title,
                    MetadataStructureId = metadataStructureId,
                    DataStructureId = dataStructureId,
                    ResearchPlanId = researchPlanId,
                    ViewAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), id, RightType.Read),
                    GrantAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), id, RightType.Grant),
                    DataStructureType = dataStructureType,
                    DownloadAccess = downloadAccess,
                    RequestExist = requestExist,
                    RequestAble = requestAble
                };

                //set metadata in session
                Session["ShowDataMetadata"] = metadata;
                ViewData["VersionSelect"] = getVersionsSelectList(id, dm);
                ViewData["isValid"] = isValid;
                ViewData["datasetSettings"] = getSettingsDataset();

                return PartialView("ShowData", model);
            }
            finally
            {
                dm.Dispose();
                entityPermissionManager.Dispose();
            }
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult DownloadZip(long id, string format, long version = -1)
        {
            if (this.IsAccessible("DIM", "Export", "GenerateZip"))
            {
                var actionresult = this.Run("DIM", "Export", "GenerateZip", new RouteValueDictionary() { { "id", id }, { "versionid", version }, { "format", format } });

                return actionresult;
            }

            return Json(false);
        }

        #region metadata external sources

        // <summary>Retrieve the content of a JavaScript file, which is stored in the data folder, which is not accessible from the IIS. The content of the
        // JavaScript file is meant to manipulate the metadata edit form and view (e.g.add disabled fields, set default values, remove or add additional UI elements)
        // based on special project needs.The files are attached to the MetadataStructure id. Files to be included have to be located under the
        // folder "[DataFolder]/]MetadataStructure"/[id]/ext.js. If no file is deposited an empty file is created and returned.</summary>
        public FileResult GetFile(long id = -1, long metadataStructureId = -1)
        {
            DatasetManager dm = null;
            try
            {
                dm = new DatasetManager();

                //use dataset ID instead of metdataStructureId
                if (metadataStructureId == -1)
                {
                    metadataStructureId = dm.DatasetRepo.Get(id).MetadataStructure.Id;
                }

                string filename = "ext.js";
                string datapath = @"C:\Data";
                if (AppConfiguration.DataPath != null || !string.IsNullOrEmpty(AppConfiguration.DataPath)) datapath = AppConfiguration.DataPath;

                string path = Path.Combine(AppConfiguration.DataPath, "MetadataStructures", metadataStructureId.ToString(), filename);

                if (!FileHelper.FileExist(path))
                {
                    // Create new folder and empty file if not exists
                    Directory.CreateDirectory(Path.Combine(AppConfiguration.DataPath, "MetadataStructures"));
                    System.IO.File.Create(Path.Combine(AppConfiguration.DataPath, "MetadataStructures", filename)).Dispose();
                    path = Path.Combine(AppConfiguration.DataPath, "MetadataStructures", filename); // set path to empty file location
                }
                return File(path, MimeMapping.GetMimeMapping(filename), filename);
            }
            finally
            {
                dm.Dispose();
            }
        }

        #endregion metadata external sources

        #region metadata

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="datasetID"></param>
        /// <returns>model</returns>
        public ActionResult ShowMetaData(long entityId, string title, long metadatastructureId, long datastructureId, long researchplanId, string sessionKeyForMetadata, bool latest, string isValid = "yes")
        {
            var result = this.Run("DCM", "Form", "SetCopyFunctionForView", new RouteValueDictionary() { { "actionName", "Copy" }, { "controllerName", "CreateDataset" }, { "area", "DCM" }, { "type", "copy" } });

            var view = this.Render("DCM", "Form", "LoadMetadataFromExternal", new RouteValueDictionary()
            {
                { "entityId", entityId },
                { "title", title },
                { "metadatastructureId", metadatastructureId },
                { "datastructureId", datastructureId },
                { "researchplanId", researchplanId },
                { "sessionKeyForMetadata", sessionKeyForMetadata },
                { "resetTaskManager", false },
                { "latest", latest },
                { "isValid", isValid }
            });

            return Content(view.ToHtmlString(), "text/html");
        }

        private BaseModelElement GetModelFromElement(XElement element)
        {
            string name = element.Attribute("name").Value;
            string displayName = "";
            BExIS.Xml.Helpers.XmlNodeType type;

            // get Type
            type = BExIS.Xml.Helpers.XmlMetadataWriter.GetXmlNodeType(element.Attribute("type").Value);

            // get DisplayName
            if (type.Equals(BExIS.Xml.Helpers.XmlNodeType.MetadataAttribute))
                displayName = element.Parent.Attribute("name").Value;
            else
                displayName = element.Attribute("name").Value;

            if (XmlUtility.HasChildren(element))
            {
                CompundAttributeModel model = new CompundAttributeModel();
                model.Name = name;
                model.Type = type;
                model.DisplayName = displayName;

                List<XElement> childrens = XmlUtility.GetChildren(element).ToList();
                foreach (XElement child in childrens)
                {
                    model.Childrens.Add(GetModelFromElement(child));
                }

                return model;
            }
            else
            {
                return new SimpleAttributeModel()
                {
                    Name = name,
                    DisplayName = displayName,
                    Value = element.Value,
                    Type = type
                };
            }
        }

        #endregion metadata

        #region primary data

        public ActionResult SetGridCommand(string filters, string orders, string columns)
        {
            Session["Columns"] = columns.Replace("ID", "").Split(',');

            //Session["Filter"] = TelerikGridHelper.ConvertToGridCommand(filters, orders);

            return null;
        }

        //[MeasurePerformance
        [BExISEntityAuthorize(typeof(Dataset), "datasetID", RightType.Read)]
        public ActionResult ShowPrimaryData(long datasetID, int versionId)
        {
            Session["Filter"] = null;
            Session["Columns"] = null;
            Session["DownloadFullDataset"] = false;
            ViewData["DownloadOptions"] = null;
            IOUtility iOUtility = new IOUtility();
            DatasetManager dm = new DatasetManager();
            DataStructureManager dsm = new DataStructureManager();
            //permission download
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

            try
            {
                if (dm.IsDatasetCheckedIn(datasetID))
                {
                    // get latest or other datasetversion
                    DatasetVersion dsv = dm.GetDatasetVersion(versionId);
                    bool latestVersion = versionId == dm.GetDatasetLatestVersionId(datasetID);
                    //TITLE
                    string title = dsv.Title;

                    // TODO: refactor Download Right not existing, so i set it to read
                    bool downloadAccess = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), datasetID, RightType.Read);

                    if (dsv.Dataset.DataStructure != null)
                    {
                        StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);
                        DataStructure ds = dsm.AllTypesDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);

                    
                        if (ds.Self.GetType() == typeof(StructuredDataStructure))
                        {
                            DataTable table = null;
                            // Count "Data" changes between current and latest version
                            int numberNewerDataVersions = dm.GetDatasetVersions(datasetID).Where(x => x.Id > versionId).Count(x => x.ModificationInfo.Comment.Contains("Data"));

                            // use MV for latest version and all older versions, if no data has been changed (only metadata)
                            if (latestVersion || numberNewerDataVersions == 0)
                            {
                                try
                                {
                                    long count = dm.RowCount(datasetID, null);
                                    if (count > 0) table = dm.GetLatestDatasetVersionTuples(datasetID, null, null, null,"", 0, 10);
                                    else ModelState.AddModelError(string.Empty, "<span style=\"color: black;\"> There is no primary data available/uploaded. </span><br/><br/> <span style=\"font-weight: normal;color: black;\">Please note that the data may have been uploaded to another repository and is referenced here in the metadata.</span>");
                                }
                                catch
                                {
                                    ModelState.AddModelError(string.Empty, "The data is not available, please ask the administrator for a synchronization.");
                                }

                                ViewData["gridTotal"] = dm.RowCount(dsv.Dataset.Id, null);
                            }
                            else
                            {
                                table = dm.GetDatasetVersionTuples(versionId, 0, 10);
                                ViewData["gridTotal"] = dm.GetDatasetVersionEffectiveTuples(dsv).Count;
                            }

                            sds.Variables = sds.Variables.OrderBy(v => v.OrderNo).ToList();

                            return PartialView(ShowPrimaryDataModel.Convert(
                                datasetID,
                                versionId,
                                title,
                                sds,
                                table,
                                downloadAccess,
                                iOUtility.GetSupportedAsciiFiles(),
                                latestVersion,
                                hasUserRights(datasetID, RightType.Write)
                                ));
                        }

                    }
                    else
                    {
                        if (this.IsAccessible("MMM", "ShowMultimediaData", "multimediaData") && GeneralSettings.UseMultiMediaModule)
                            return RedirectToAction("multimediaData", "ShowMultimediaData", new RouteValueDictionary { { "area", "MMM" }, { "datasetID", datasetID }, { "versionId", versionId } });
                        else
                            return
                                PartialView(ShowPrimaryDataModel.Convert(datasetID,
                                versionId,
                                title,
                                null,
                                SearchUIHelper.GetContantDescriptorFromKey(dsv, "unstructuredData"),
                                downloadAccess,
                                iOUtility.GetSupportedAsciiFiles(),
                                latestVersion,
                                hasUserRights(datasetID, RightType.Write)
                                ));
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "The dataset is currently being processed and is therefore locked.");
                }

                return PartialView(null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dm.Dispose();
                dsm.Dispose();
                entityPermissionManager.Dispose();
            }
        }

        #region server side

        [GridAction(EnableCustomBinding = true)]
        public ActionResult _CustomPrimaryDataBinding(GridCommand command, string columns, int datasetId, int versionId)
        {
            GridModel model = new GridModel();
            ViewData["Filter"] = command;
            DatasetManager dm = new DatasetManager();

            try
            {
                if (dm.IsDatasetCheckedIn(datasetId))
                {
                    DatasetVersion dsv = dm.GetDatasetVersion(versionId);
                    long latestDatasetVersionId = dm.GetDatasetLatestVersionId(datasetId);

                    DataTable table = null;

                    // get primarydata from latest version with table
                    if (versionId == latestDatasetVersionId)
                    {
                        FilterExpression filter = TelerikGridHelper.Convert(command.FilterDescriptors.ToList());
                        OrderByExpression orderBy = TelerikGridHelper.Convert(command.SortDescriptors.ToList());

                        table = dm.GetLatestDatasetVersionTuples(datasetId, filter, orderBy, null,"", command.Page - 1, command.PageSize);
                        ViewData["gridTotal"] = dm.RowCount(datasetId, filter);
                    }
                    // get primarydata from other version with tuples
                    else
                    {
                        table = dm.GetDatasetVersionTuples(versionId, command.Page - 1, command.PageSize);
                        ViewData["gridTotal"] = dm.GetDatasetVersionEffectiveTuples(dsv).Count;
                    }

                    model = new GridModel(table);
                    model.Total = Convert.ToInt32(ViewData["gridTotal"]); // (int)Session["gridTotal"];
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Dataset is just in processing.");
                }

                return View(model);
            }
            finally
            {
                dm.Dispose();
            }
        }

        #endregion server side

        #region download

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public JsonResult PrepareAscii(long id, string ext, long versionid, bool latest, bool withUnits)
        {
            if (hasUserRights(id, RightType.Read))
            {
                DatasetManager datasetManager = new DatasetManager();
                string mimetype = MimeMapping.GetMimeMapping(ext);
                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    AsciiWriter writer = new AsciiWriter(TextSeperator.comma);
                    OutputDataManager ioOutputDataManager = new OutputDataManager();
                    string title = getTitle(writer.GetTitle(id));
                    string path = "";

                    string message = string.Format("dataset {0} version {1} was downloaded as {2}.", id,
                                                    datasetVersion.Id, ext);
                    //create a history dátaset
                    if (!latest)
                    {
                        DataTable datatable = getHistoryData(versionid);
                        path = ioOutputDataManager.GenerateAsciiFile("temp", datatable, title, mimetype, datasetVersion.Dataset.DataStructure.Id);
                    }
                    else
                    // or if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        DataTable datatable = getFilteredData(id);
                        path = ioOutputDataManager.GenerateAsciiFile("temp", datatable, title, mimetype, datasetVersion.Dataset.DataStructure.Id, withUnits);

                        LoggerFactory.LogCustom(message);

                        #endregion generate a subset of a dataset
                    }
                    //full dataset
                    else
                    {
                        path = ioOutputDataManager.GenerateAsciiFile(id, versionid, mimetype, withUnits);

                        LoggerFactory.LogCustom(message);
                    }

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
                finally
                {
                    datasetManager.Dispose();
                }
            }
            else
            {
                return Json("User has no rights.", JsonRequestBehavior.AllowGet);
            }
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult DownloadAscii(long id, string ext, long versionid, bool latest, bool withUnits, bool download = false)
        {
            if (hasUserRights(id, RightType.Read))
            {
                DatasetManager datasetManager = new DatasetManager();
                int versionNumber = getVersionNumber(id, versionid);
                string mimetype = MimeMapping.GetMimeMapping(ext);

                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    AsciiWriter writer = new AsciiWriter(TextSeperator.comma);
                    OutputDataManager ioOutputDataManager = new OutputDataManager();
                    string title = getTitle(writer.GetTitle(id));
                    string path = "";
                    long versionNr = datasetManager.GetDatasetVersionNr(datasetVersion);
                    string message = string.Format("dataset {0} version {1} was downloaded as {2}.", id,
                        versionNr, ext);

                    //create a history dátaset
                    if (!latest)
                    {
                        DataTable datatable = getHistoryData(versionid);
                        path = ioOutputDataManager.GenerateAsciiFile("temp", datatable, title, mimetype, datasetVersion.Dataset.DataStructure.Id);

                        return File(path, mimetype, title + "_v" + versionNumber + ext);
                    }
                    else
                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        DataTable datatable = getFilteredData(id);
                        path = ioOutputDataManager.GenerateAsciiFile("temp", datatable, title, mimetype, datasetVersion.Dataset.DataStructure.Id, withUnits);

                        LoggerFactory.LogCustom(message);

                        return File(path, mimetype, title + ext);

                        #endregion generate a subset of a dataset
                    }
                    else
                    {
                        path = ioOutputDataManager.GenerateAsciiFile(id, versionid, mimetype, withUnits);

                        //only log and send mail once
                        if (download)
                        {
                            LoggerFactory.LogCustom(message);

                            var es = new EmailService();
                            es.Send(MessageHelper.GetDownloadDatasetHeader(id, versionNr),
                            MessageHelper.GetDownloadDatasetMessage(id, title, getPartyNameOrDefault(), ext, versionNr),
                                GeneralSettings.SystemEmail
                                );
                        }

                        return File(path, mimetype, title + ext);
                    }
                }
                catch (Exception ex)
                {
                    var es = new EmailService();
                    es.Send(MessageHelper.GetDownloadDatasetHeader(id, versionNumber),
                        ex.Message,
                        GeneralSettings.SystemEmail
                        );

                    throw ex;
                }
                finally
                {
                    datasetManager.Dispose();
                    //OutputDataManager.ClearTempDirectory();
                }
            }
            else
            {
                return Content("User has no rights.");
            }
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public JsonResult PrepareExcelData(long id, long versionid, bool latest, bool withUnits)
        {
            if (hasUserRights(id, RightType.Read))
            {
                string ext = ".xlsx";

                DatasetManager datasetManager = new DatasetManager();

                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    ExcelWriter writer = new ExcelWriter();

                    string title = getTitle(writer.GetTitle(id));

                    string path = "";

                    //string message = string.Format("dataset {0} version {1} was downloaded as excel.", id, datasetVersion.Id);

                    OutputDataManager outputDataManager = new OutputDataManager();

                    //create a history dátaset
                    if (!latest)
                    {
                        DataTable datatable = getHistoryData(versionid);
                        path = outputDataManager.GenerateExcelFile("temp", datatable, title, datasetVersion.Dataset.DataStructure.Id, ext, withUnits);
                    }
                    else
                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        DataTable datatable = getFilteredData(id);
                        path = outputDataManager.GenerateExcelFile("temp", datatable, title + "_filtered", datasetVersion.Dataset.DataStructure.Id, ext, withUnits);

                        //LoggerFactory.LogCustom(message);

                        #endregion generate a subset of a dataset
                    }
                    //filter not in use
                    else
                    {
                        path = outputDataManager.GenerateExcelFile(id, versionid, false, null, withUnits);
                        //LoggerFactory.LogCustom(message);
                    }

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
                finally
                {
                    datasetManager.Dispose();
                }
            }
            else
            {
                return Json("User has no rights.", JsonRequestBehavior.AllowGet);
            }
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult DownloadAsExcelData(long id, long versionid, bool latest, bool withUnits)
        {
            if (hasUserRights(id, RightType.Read))
            {
                string ext = ".xlsx";
                int versionNumber = getVersionNumber(id, versionid);
                string mimetype = MimeMapping.GetMimeMapping(ext);

                DatasetManager datasetManager = new DatasetManager();

                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    ExcelWriter writer = new ExcelWriter();

                    string title = getTitle(writer.GetTitle(id));

                    string path = "";
                    long versionNr = datasetManager.GetDatasetVersionNr(datasetVersion);
                    string message = string.Format("dataset {0} version {1} was downloaded as excel.", id,
                        versionNr);

                    OutputDataManager outputDataManager = new OutputDataManager();

                    //create a history dátaset
                    if (!latest)
                    {
                        DataTable datatable = getHistoryData(versionid);
                        path = outputDataManager.GenerateExcelFile("temp", datatable, title, datasetVersion.Dataset.DataStructure.Id, ext, withUnits);

                        return File(path, mimetype, title + "_v" + versionNumber + ext);
                    }
                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        //ToDo filter datatuples

                        DataTable datatable = getFilteredData(id);
                        path = outputDataManager.GenerateExcelFile("temp", datatable, title + "_filtered", datasetVersion.Dataset.DataStructure.Id, ext, withUnits);

                        LoggerFactory.LogCustom(message);

                        return File(path, mimetype, title + ext);

                        #endregion generate a subset of a dataset
                    }

                    //filter not in use
                    else
                    {
                        path = outputDataManager.GenerateExcelFile(id, versionid, false, null, withUnits);
                        LoggerFactory.LogCustom(message);

                        var es = new EmailService();
                        es.Send(MessageHelper.GetDownloadDatasetHeader(id, versionNr),
                            MessageHelper.GetDownloadDatasetMessage(id, title, getPartyNameOrDefault(), ext, versionNr),
                            GeneralSettings.SystemEmail
                            );

                        return File(Path.Combine(AppConfiguration.DataPath, path), mimetype, title + ext);
                    }
                }
                catch (Exception ex)
                {
                    var es = new EmailService();
                    es.Send(MessageHelper.GetUpdateDatasetHeader(id),
                        ex.Message,
                        GeneralSettings.SystemEmail
                        );

                    throw ex;
                }
                finally
                {
                    datasetManager.Dispose();
                    //OutputDataManager.ClearTempDirectory();
                }
            }
            else
            {
                return Content("User has no rights.");
            }
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public JsonResult PrepareExcelTemplateData(long id, long versionid, bool latest)
        {
            if (hasUserRights(id, RightType.Read))
            {
                string ext = ".xlsm";
                int versionNumber = getVersionNumber(id, versionid);
                string mimetype = MimeMapping.GetMimeMapping(ext);

                DatasetManager datasetManager = new DatasetManager();

                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    ExcelWriter writer = new ExcelWriter(true);

                    string title = getTitle(writer.GetTitle(id));

                    string path = "";

                    string message = string.Format("dataset {0} version {1} was downloaded as excel.", id,
                        datasetVersion.Id);

                    OutputDataManager outputDataManager = new OutputDataManager();

                    //create a history dátaset
                    if (!latest)
                    {
                        path = outputDataManager.GenerateExcelFile(id, versionid, true);

                        //return File(path, mimetype, title + "_v" + versionNumber + ext);
                    }
                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        DataTable datatable = getFilteredData(id);
                        path = outputDataManager.GenerateExcelFile(id, versionid, true, datatable);

                        LoggerFactory.LogCustom(message);

                        //return File(path, "text/csv", title + ext);

                        #endregion generate a subset of a dataset
                    }

                    //filter not in use
                    else
                    {
                        path = outputDataManager.GenerateExcelFile(id, versionid, true);
                        LoggerFactory.LogCustom(message);
                    }

                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
                finally
                {
                    datasetManager.Dispose();
                }
            }
            else
            {
                return Json("User has no rights.", JsonRequestBehavior.AllowGet);
            }
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult DownloadAsExcelTemplateData(long id, long versionid, bool latest)
        {
            if (hasUserRights(id, RightType.Read))
            {
                string ext = ".xlsm";
                int versionNumber = getVersionNumber(id, versionid);
                string mimetype = MimeMapping.GetMimeMapping(ext);

                DatasetManager datasetManager = new DatasetManager();

                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                    ExcelWriter writer = new ExcelWriter(true);

                    string title = getTitle(writer.GetTitle(id));

                    string path = "";
                    long versionNr = datasetManager.GetDatasetVersionNr(datasetVersion);
                    string message = string.Format("dataset {0} version {1} was downloaded as excel.", id,
                        versionNr);

                    OutputDataManager outputDataManager = new OutputDataManager();
                    string mimitype = MimeMapping.GetMimeMapping(ext);

                    //create a history dátaset
                    if (!latest)
                    {
                        path = outputDataManager.GenerateExcelFile(id, versionid, true, null);

                        return File(path, mimetype, title + "_v" + versionNumber + ext);
                    }
                    // if filter selected
                    if (filterInUse())
                    {
                        #region generate a subset of a dataset

                        //ToDo filter datatuples

                        DataTable datatable = getFilteredData(id);
                        path = outputDataManager.GenerateExcelFile(id, versionid, true, datatable);

                        //LoggerFactory.LogCustom(message);

                        return File(path, mimitype, title + ext);

                        #endregion generate a subset of a dataset
                    }

                    //filter not in use
                    else
                    {
                        path = outputDataManager.GenerateExcelFile(id, versionid, true);
                        LoggerFactory.LogCustom(message);

                        var es = new EmailService();
                        es.Send(MessageHelper.GetDownloadDatasetHeader(id, versionNr),
                            MessageHelper.GetDownloadDatasetMessage(id, title, getPartyNameOrDefault(), ext, versionNr),
                            GeneralSettings.SystemEmail
                            );

                        return File(Path.Combine(AppConfiguration.DataPath, path), mimitype, title + ext);
                    }
                }
                catch (Exception ex)
                {
                    var es = new EmailService();
                    es.Send(MessageHelper.GetUpdateDatasetHeader(id),
                        ex.Message,
                        GeneralSettings.SystemEmail
                        );

                    throw ex;
                }
                finally
                {
                    datasetManager.Dispose();
                    //OutputDataManager.ClearTempDirectory();
                }
            }
            else
            {
                return Content("User has no rights.");
            }
        }

        public ActionResult SetFullDatasetDownload(bool subset)
        {
            Session["DownloadFullDataset"] = subset;

            return Content("changed");
        }

        #region helper

        public void SetCommand(string filters, string orders)
        {
            Session["Filter"] = TelerikGridHelper.ConvertToGridCommand(filters, orders);
        }

        private bool filterInUse()
        {
            if ((Session["Filter"] != null || Session["Columns"] != null) && !(bool)Session["DownloadFullDataset"])
            {
                GridCommand command = (GridCommand)Session["Filter"];
                string[] columns = (string[])Session["Columns"];

                if (columns != null)
                {
                    if ((command != null && (command.FilterDescriptors.Count > 0 || command.SortDescriptors.Count > 0)) || columns.Count() > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private DataTable getFilteredData(long datasetId)
        {
            DatasetManager datasetManager = new DatasetManager();
            try
            {
                GridCommand command = null;
                FilterExpression filter = null;
                OrderByExpression orderBy = null;
                string[] columns = null;

                if (Session["Filter"] != null) command = (GridCommand)Session["Filter"];

                if (Session["Columns"] != null) columns = (string[])Session["Columns"];

                if (command != null)
                {
                    filter = TelerikGridHelper.Convert(command.FilterDescriptors.ToList());
                    orderBy = TelerikGridHelper.Convert(command.SortDescriptors.ToList());
                }

                ProjectionExpression projection = TelerikGridHelper.Convert(columns);

                long count = datasetManager.RowCount(datasetId, filter);

                DataTable table = datasetManager.GetLatestDatasetVersionTuples(datasetId, filter, orderBy, projection, "", 0, (int)count);

                if (projection == null) table.Strip();

                return table;
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        private DataTable getHistoryData(long versionId)
        {
            DatasetManager dm = new DatasetManager();

            try
            {
                DatasetVersion dsv = dm.GetDatasetVersion(versionId);
                DataTable table = null;
                long rowCount = dm.GetDatasetVersionEffectiveTuples(dsv).Count;
                table = dm.GetDatasetVersionTuples(versionId, 0, (int)rowCount);

                return table;
            }
            finally
            {
                dm.Dispose();
            }
        }

        private string getTitle(string title)
        {
            if (Session["Filter"] != null)
            {
                GridCommand command = (GridCommand)Session["Filter"];
                if (command.FilterDescriptors.Count > 0 || command.SortDescriptors.Count > 0)
                {
                    return title + "-Filtered";
                }
            }

            return title;
        }

        private int getVersionNumber(long datasetId, long versionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                return uow.GetReadOnlyRepository<DatasetVersion>().Query()
                    .Where(dsv => dsv.Dataset.Id.Equals(datasetId))
                    .OrderBy(d => d.Timestamp)
                    .Select(d => d.Id).ToList().IndexOf(versionId) + 1;
            }
        }

        #endregion helper

        #endregion download

        #region download FileStream

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult DownloadAllFiles(long id)
        {
            if (hasUserRights(id, RightType.Read))
            {

                DatasetManager datasetManager = new DatasetManager();
                string title = "";
                try
                {
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);

                    //TITLE
                    title = datasetVersion.Title;
                    title = String.IsNullOrEmpty(title) ? "unknown" : title;

                    string zipPath = Path.Combine(AppConfiguration.DataPath, "Datasets", id.ToString(), title + ".zip");

                    if (FileHelper.FileExist(zipPath))
                    {
                        if (FileHelper.WaitForFile(zipPath))
                        {
                            FileHelper.Delete(zipPath);
                        }
                    }

                    using (ZipFile zip = new ZipFile())
                    {
                        foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                        {
                            string path = Path.Combine(AppConfiguration.DataPath, cd.URI);
                            string name = cd.URI.Split('\\').Last();

                            zip.AddFile(path, "");
                        }

                        zip.Save(zipPath);
                    }

                    long versionNr = datasetManager.GetDatasetVersionNr(datasetVersion);
                    string message = string.Format("all files from dataset {0} version {1} was downloaded as zip.", datasetVersion.Dataset.Id,
                            versionNr);
                    LoggerFactory.LogCustom(message);

                    var es = new EmailService();
                    es.Send(MessageHelper.GetDownloadDatasetHeader(id, versionNr),
                        MessageHelper.GetDownloadDatasetMessage(id, title, getPartyNameOrDefault(), "zip", versionNr),
                        GeneralSettings.SystemEmail
                        );

                    return File(zipPath, "application/zip", title + ".zip");
                }
                catch (Exception ex)
                {
                    var es = new EmailService();
                    es.Send(MessageHelper.GetUpdateDatasetHeader(id),
                        ex.Message,
                        GeneralSettings.SystemEmail
                        );

                    throw ex;
                }
                finally
                {
                    datasetManager.Dispose();
                }
            }

            return Content("User has no rights.");
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult DownloadFile(long id, long version, string path, string mimeType)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            {
                if (hasUserRights(id, RightType.Read))
                {
                    string title = id + "_" + version + "_" + path.Split('\\').Last();
                    long versionNr = datasetManager.GetDatasetVersionNr(version);
                    string message = string.Format("dataset {0} version {1} was downloaded as excel.", id, versionNr);
                    LoggerFactory.LogCustom(message);

                    var es = new EmailService();
                    es.Send(MessageHelper.GetDownloadDatasetHeader(id, versionNr),
                        MessageHelper.GetDownloadDatasetMessage(id, title, getPartyNameOrDefault(), mimeType, versionNr),
                        GeneralSettings.SystemEmail
                        );

                    return File(Path.Combine(AppConfiguration.DataPath, path), mimeType, title);
                }
            }

            return Content("User has no rights.");
        }

        #endregion download FileStream

        #endregion primary data

        #region datastructure

        [GridAction]
        public ActionResult _CustomDataStructureBinding(GridCommand command, long datasetID)
        {
            DataStructureManager dsm = new DataStructureManager();
            DatasetManager dm = new DatasetManager();

            try
            {
                long id = datasetID;
                if (dm.IsDatasetCheckedIn(id))
                {
                    DatasetVersion ds = dm.GetDatasetLatestVersion(id);
                    if (ds != null)
                    {
                        StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(ds.Dataset.DataStructure.Id);
                        dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);
                        //StructuredDataStructure sds = (StructuredDataStructure)(ds.Dataset.DataStructure.Self);
                        SearchUIHelper suh = new SearchUIHelper();
                        DataTable table = suh.ConvertStructuredDataStructureToDataTable(sds);

                        return View(new GridModel(table));
                    }
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Dataset is just in processing.");
                }
            }
            finally
            {
                dm.Dispose();
                dsm.Dispose();
            }

            return View(new GridModel(new DataTable()));
        }

        public ActionResult ShowPreviewDataStructure(long datasetID, string entityType = "Dataset")
        {
            using (DatasetManager dm = new DatasetManager())
            using (DataStructureManager dsm = new DataStructureManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (OperationManager operationManager = new OperationManager())
            using (FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager())
            using (SubjectManager subjectManager = new SubjectManager())
            {
                using (var uow = this.GetUnitOfWork())
                {
                    Dataset dataset = dm.GetDataset(datasetID);
       
                    DataStructure dataStructure = null;
                    long id = (long)dataset.Id;
                    string DSlink = null;

                    if (dataset.DataStructure != null)
                    {
                        long dataStructureId = dataset.DataStructure.Id;

                        if (this.IsAccessible("RPM", "DataStructureEdit", "Index"))
                        {
                            dataStructure = uow.GetReadOnlyRepository<StructuredDataStructure>().Get(dataStructureId);
                            bool structured = false;
                            if (dataStructure != null)
                                structured = true;
                            else
                                dataStructure = uow.GetReadOnlyRepository<UnStructuredDataStructure>().Get(dataStructureId);

                            if (structured)
                            {
                                if (entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), id, RightType.Write))
                                {
                                    Feature feature = operationManager.OperationRepository.Query().Where(o => o.Module.ToLower().Equals("rpm") && o.Controller.ToLower().Equals("datastructureedit")).FirstOrDefault().Feature;
                                    Subject subject = subjectManager.SubjectRepository.Query().Where(s => s.Name.Equals(HttpContext.User.Identity.Name)).FirstOrDefault();

                                    if (featurePermissionManager.HasAccess(subject.Id, feature.Id))
                                        DSlink = "/RPM/DataStructureEdit/?dataStructureId=" + dataStructureId;
                                }
                            }
                        }
                        else
                        {
                            dataStructure = uow.GetReadOnlyRepository<DataStructure>().Get(dataStructureId);
                        }
                    }

                    Tuple<DataStructure, long, string> m = new Tuple<DataStructure, long, string>(
                        dataStructure,
                        id,
                        DSlink
                        );

                    return PartialView("_previewDatastructure", m);
                    
                }
            }
        }

        #endregion datastructure

        #region entity references

        //[BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult ShowReferences(long id, int version)
        {
            var sourceTypeId = 0;

            //get the researchobject (cuurently called dataset) to get the id of a metadata structure
            Dataset researcobject = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(id);
            long metadataStrutcureId = researcobject.MetadataStructure.Id;

            using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
            {
                string entityName = xmlDatasetHelper.GetEntityNameFromMetadatStructure(metadataStrutcureId, metadataStructureManager);
                string entityType = xmlDatasetHelper.GetEntityTypeFromMetadatStructure(metadataStrutcureId, metadataStructureManager);

                //ToDo in the entity table there must be the information
                using (EntityManager entityManager = new EntityManager())
                {
                    var entity = entityManager.Entities.Where(e => e.Name.Equals(entityName)).FirstOrDefault();

                    var view = this.Render("DCM", "EntityReference", "Show", new RouteValueDictionary()
                {
                    { "sourceId", id },
                    { "sourceTypeId", entity.Id },
                    { "sourceVersion", version }
                });

                    return Content(view.ToHtmlString(), "text/html");
                }
            }
        }

        #endregion entity references

        #region helper

        private List<DropDownItem> GetDownloadOptions()
        {
            List<DropDownItem> options = new List<DropDownItem>();

            options.Add(new DropDownItem()
            {
                Text = "Excel",
                Value = "0"
            });

            options.Add(new DropDownItem()
            {
                Text = "Excel (filtered)",
                Value = "1"
            });

            options.Add(new DropDownItem()
            {
                Text = "Csv",
                Value = "2"
            });

            options.Add(new DropDownItem()
            {
                Text = "Csv (filtered)",
                Value = "3"
            });

            options.Add(new DropDownItem()
            {
                Text = "Text",
                Value = "4"
            });

            options.Add(new DropDownItem()
            {
                Text = "Text (filtered)",
                Value = "5"
            });

            return options;
        }

        private SelectList getVersionsSelectList(long id, DatasetManager datasetManager)
        {
            List<SelectListItem> tmp = new List<SelectListItem>();
            List<DatasetVersion> datasetVersionsAllowed = new List<DatasetVersion>();
            List<DatasetVersion> datasetVersions = datasetManager.GetDatasetVersions(id).OrderByDescending(d => d.Id).ToList();

            SettingsHelper helper = new SettingsHelper();

            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            {

                bool hasEditPermission = false;

                if (GetUsernameOrDefault() != "DEFAULT")
                {
                    hasEditPermission = entityPermissionManager.HasEffectiveRight(HttpContext.User.Identity.Name, typeof(Dataset), id, RightType.Write);
                }

                // user has edit permission and can see all versions -> show full list
                if (hasEditPermission || helper.GetValue("reduce_versions_select_logged_in").ToString() == "false")
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
                    new SelectListItem()
                    {
                        Text = CreateVersionNumber(d, datasetVersions) + " " + getVersionInfo(d),
                        Value = "" + (datasetVersions.Count - datasetVersions.IndexOf(d))
                    }
                    ));

                return new SelectList(tmp, "Value", "Text");
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
            using (PartyManager partyManager = new PartyManager())
            using (var identityUserService = new IdentityUserService())
            {
                var user_performer = identityUserService.FindByNameAsync(performer);

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

                    // both exits - needs seperator
                    if (d.ModificationInfo != null &&
                        string.IsNullOrEmpty(d.ModificationInfo.Performer) &&
                        !string.IsNullOrEmpty(d.ModificationInfo.Comment) &&
                        !string.IsNullOrEmpty(d.ChangeDescription))
                    {
                        sb.Append(" : ");
                    }

                    //changedescription is not null or empty
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

        private long getVersionId(long datasetId, int version ,string versionName, List<DatasetVersion> datasetVersions, DatasetStatus datasetStatus)
        {
            long versionId = 0;
            SettingsHelper helper = new SettingsHelper();


            using (DatasetManager dm = new DatasetManager())
            {

                List<DatasetVersion> datasetVersionsAllowed = dm.GetDatasetVersionsAllowed(datasetId, true, false, datasetVersions, datasetStatus);

                // User is not logged in
                if (GetUsernameOrDefault() == "DEFAULT")
                {

                    // No version or version name -> use latest allowed version
                    if (version == 0 && versionName.Length == 0)
                    {
                        if (datasetVersionsAllowed.Count > 0)
                        {
                            versionId = datasetVersionsAllowed.OrderByDescending(d => d.Timestamp).Select(d => d.Id).FirstOrDefault();
                        }
                    }
                    // Version name -> check if requested version is allowed
                    else if (versionName.Length > 0)
                    {
                        if (datasetVersionsAllowed.Where(d => d.VersionName == versionName).Count() == 1)
                        {
                            versionId = datasetVersionsAllowed.OrderByDescending(d => d.Timestamp).Where(d => d.VersionName == versionName).Select(d => d.Id).FirstOrDefault();
                        }
                    }
                    // Version number -> check if requested version is allowed
                    else if (version != 0)
                    {
                        var idTemp = datasetVersions.OrderBy(d => d.Timestamp).Skip(version - 1).Take(1).Select(d => d.Id).FirstOrDefault();

                        if (idTemp > 0 && datasetVersionsAllowed.Where(d => d.Id == idTemp).Count() == 1)
                        {
                            versionId = idTemp;
                        }
                    }
                }
                // User is logged in
                else
                {
                    // Get version by version name
                    if (versionName.Length > 0 && datasetVersions.Where(d => d.VersionName == versionName).Select(d => d.Id).FirstOrDefault() > 0)
                    {
                        versionId = datasetVersions.Where(d => d.VersionName == versionName).Select(d => d.Id).FirstOrDefault();
                    }
                    // Get latest version
                    else if (version == 0)
                    {
                        // Use latest public, if exists or latest without restriction
                        if (datasetVersionsAllowed.Count > 0 && helper.GetValue("restrict_latest_version_logged_in").ToString() == "true")
                        {
                            versionId = datasetVersionsAllowed.OrderByDescending(d => d.Timestamp).Select(d => d.Id).FirstOrDefault();
                        }
                        else
                        {
                            versionId = dm.GetDatasetLatestVersionId(datasetId, datasetStatus); 
                        }
                    }
                    // Get specific version number
                    else
                    {
                        versionId = dm.GetDatasetVersions(datasetId).OrderBy(d => d.Timestamp).Skip(version - 1).Take(1).Select(d => d.Id).FirstOrDefault();
                    }
                }
            }

            return versionId;
        }

        public bool UserExist()
        {
            if (HttpContext.User != null && HttpContext.User.Identity != null && !string.IsNullOrEmpty(HttpContext.User.Identity.Name)) return true;

            return false;
        }

        private static string storeGeneratedFilePathToContentDiscriptor(long datasetId, DatasetVersion datasetVersion, string title, string ext)
        {
            string name = "";
            string mimeType = "";

            if (ext.Contains("csv"))
            {
                name = "datastructure";
                mimeType = "text/comma-separated-values";
            }

            // create the generated FileStream and determine its location
            string dynamicPath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId, datasetVersion.Id, title, ext);
            //Register the generated data FileStream as a resource of the current dataset version
            //ContentDescriptor generatedDescriptor = new ContentDescriptor()
            //{
            //    OrderNo = 1,
            //    Name = name,
            //    MimeType = mimeType,
            //    URI = dynamicPath,
            //    DatasetVersion = datasetVersion,
            //};

            DatasetManager dm = new DatasetManager();

            try
            {
                if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(name)) > 0)
                {   // remove the one contentdesciptor
                    foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                    {
                        if (cd.Name == name)
                        {
                            cd.URI = dynamicPath;
                            dm.UpdateContentDescriptor(cd);
                        }
                    }
                }
                else
                {
                    // add current contentdesciptor to list
                    //datasetVersion.ContentDescriptors.Add(generatedDescriptor);
                    dm.CreateContentDescriptor(name, mimeType, dynamicPath, 1, datasetVersion);
                }

                //dm.EditDatasetVersion(datasetVersion, null, null, null);
                return dynamicPath;
            }
            finally
            {
                dm.Dispose();
            }
        }

        private bool hasUserRights(long entityId, RightType rightType)
        {
            #region security permissions and authorisations check

            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
                return entityPermissionManager.HasEffectiveRight(GetUsernameOrDefault(), typeof(Dataset), entityId, rightType);

            #endregion security permissions and authorisations check
        }

        private bool hasUserRequestRight()
        {
            using (var userManager = new UserManager())
            using (var featurePermissionManager = new FeaturePermissionManager())
            using (var operationManager = new OperationManager())
            {
                var operation = operationManager.Find("DDM", "RequestsSend", "*");
                if (operation != null)
                {
                    var feature = operation.Feature;

                    if (feature != null)
                    {
                        var result = userManager.FindByNameAsync(GetUsernameOrDefault());

                        if (featurePermissionManager.HasAccess(result.Result?.Id, feature.Id)) return true;
                    }
                }
            }

            return false;
        }

        private bool HasOpenRequest(long datasetId)
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

        private bool HasRequestMapping(long datasetId)
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

        private Dictionary<string, string> getSettingsDataset()
        {
            if (Session["SettingsDataset"] != null)
            {
                return (Dictionary<string, string>)Session["SettingsDataset"];
            }

            var dataset_settings_list = new Dictionary<string, string>();
            dataset_settings_list.Add("show_primary_data_tab", "true");
            dataset_settings_list.Add("show_data_structure_tab", "true");
            dataset_settings_list.Add("show_link_tab", "true");
            dataset_settings_list.Add("show_permission_tab", "true");
            dataset_settings_list.Add("show_publish_tab", "true");
            dataset_settings_list.Add("show_attachments_tab", "true");

            dataset_settings_list.Add("show_tabs_deactivated", "true");
            dataset_settings_list.Add("check_public_metadata", "false");


            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");

            foreach (var item in dataset_settings_list.ToList())
            {
                try
                {
                    var value = moduleSettings.GetValueByKey(item.Key);

                    if (value != null)
                    {
                        dataset_settings_list[item.Key] = value.ToString();
                    }
                }
                catch (Exception e)
                {
                    // do nothing
                }
            }

            Session["SettingsDataset"] = dataset_settings_list;
            return dataset_settings_list;
        }

        #endregion helper
    }
}