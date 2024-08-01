using BExIS.App.Bootstrap.Attributes;
using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.IO;
using BExIS.IO.Transform.Output;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models.CreateDataset;
using BExIS.Modules.Dcm.UI.Models.Metadata;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Utils.Data.MetadataStructure;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class FormController : BaseController
    {
        private CreateTaskmanager TaskManager;
        private MetadataStructureUsageHelper metadataStructureUsageHelper = new MetadataStructureUsageHelper();
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        #region Load Metadata formular actions

        [BExISEntityAuthorize(typeof(Dataset), "datasetId", RightType.Write)]
        public ActionResult EditMetadata(long datasetId, bool locked = false, bool created = false)
        {
            return RedirectToAction("LoadMetadata", "Form", new { entityId = datasetId, locked = false, created = false, fromEditMode = true });
        }

        public ActionResult ImportMetadata(long metadataStructureId, bool edit = true, bool created = false, bool locked = false)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Dataset", this.Session.GetTenant());

            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            FormHelper.ClearCache();
            //var newMetadata = TaskManager.Bus[CreateTaskmanager.METADATA_XML];

            var stepInfoModelHelpers = new List<StepModelHelper>();
            var Model = new MetadataEditorModel();

            //TaskManager.AddToBus(CreateTaskmanager.METADATA_XML, newMetadata);

            TaskManager = AdvanceTaskManagerBasedOnExistingMetadata(metadataStructureId, TaskManager);

            foreach (var stepInfo in TaskManager.StepInfos)
            {
                var stepModelHelper = GetStepModelhelper(stepInfo.Id, TaskManager);

                if (stepModelHelper.Model == null)
                {
                    if (stepModelHelper.UsageType.Equals(typeof(MetadataPackageUsage)))
                    {
                        stepModelHelper.Model = createPackageModel(stepInfo.Id, false);
                        if (stepModelHelper.Model.StepInfo.IsInstanze)
                            LoadSimpleAttributesForModelFromXml(stepModelHelper, TaskManager);
                    }

                    if (stepModelHelper.UsageType.Equals(typeof(MetadataNestedAttributeUsage)))
                    {
                        stepModelHelper.Model = createCompoundModel(stepInfo.Id, false);
                        if (stepModelHelper.Model.StepInfo.IsInstanze)
                            LoadSimpleAttributesForModelFromXml(stepModelHelper, TaskManager);
                    }

                    getChildModelsHelper(stepModelHelper, TaskManager);
                }

                stepInfoModelHelpers.Add(stepModelHelper);
            }

            Model.StepModelHelpers = stepInfoModelHelpers;
            Model.Import = IsImportAvavilable(metadataStructureId);
            //set addtionaly functions
            Model.Actions = getAddtionalActions(TaskManager);
            Model.FromEditMode = edit;
            Model.Created = created;

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_TITLE))
            {
                if (TaskManager.Bus[CreateTaskmanager.ENTITY_TITLE] != null)
                    Model.DatasetTitle = TaskManager.Bus[CreateTaskmanager.ENTITY_TITLE].ToString();
            }
            else
                Model.DatasetTitle = "No Title available.";

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
            {
                var entityId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
                Model.EditRight = hasUserEditRights(entityId);
                Model.EditAccessRight = hasUserEditAccessRights(entityId);
                Model.DatasetId = entityId;
            }
            else
            {
                Model.DatasetTitle = "No Title available.";
                Model.EditRight = false;
                Model.EditAccessRight = false;
                Model.DatasetId = -1;
            }

            // set latest version to true, as this view is only called from edit actions, which are only possible for the latest version
            Model.LatestVersion = true;

            ViewData["Locked"] = locked;

            return PartialView("MetadataEditor", Model);
        }

        public ActionResult LoadMetadata(long entityId, bool locked = false, bool created = false, bool fromEditMode = false, bool resetTaskManager = false, XmlDocument newMetadata = null, bool asPartial = true)
        {
            var loadFromExternal = resetTaskManager;
            long metadataStructureId = -1;
            long dataStructureId = -1;

            var Model = new MetadataEditorModel();

            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            FormHelper.ClearCache();

            // if dataset exist load metadata and metadata sturtcure id
            if (entityId > -1)
            {
                using (var datasetManager = new DatasetManager())
                {
                    var dataset = datasetManager.GetDataset(entityId);
                    var metadata = datasetManager.GetDatasetLatestMetadataVersion(entityId);
                    metadataStructureId = dataset.MetadataStructure.Id;
                    if (dataset.DataStructure != null)
                        dataStructureId = dataset.DataStructure.Id;

                    if (TaskManager == null)
                    {
                        TaskManager = new CreateTaskmanager();
                        // set button functions
                        setDefaultAdditionalFunctions(TaskManager);
                    }

                    if (dataset.EntityTemplate != null)
                    {
                        TaskManager.AddToBus(CreateTaskmanager.SAVE_WITH_ERRORS, dataset.EntityTemplate.MetadataInvalidSaveMode);
                    }

                    //load taskmanager based onb metadata structure and maybe existing metadata
                    TaskManager = loadTaskManager(metadataStructureId, dataStructureId, -1, metadata, "", TaskManager, ref Model);

                    TaskManager.AddToBus(CreateTaskmanager.ENTITY_ID, entityId);

                    Session["ViewDatasetTaskmanager"] = TaskManager;
                }
            }

            // adds to taskmanager
            TaskManager.AddToBus(CreateTaskmanager.EDIT_MODE, fromEditMode);

            #region prepare model & View Data

            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Dataset", this.Session.GetTenant());
            ViewData["Locked"] = locked;
            ViewData["ShowOptional"] = false;
            ViewData["EntityId"] = entityId;

            // Set dataset Title to Model
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_TITLE))
            {
                if (TaskManager.Bus[CreateTaskmanager.ENTITY_TITLE] != null)
                    Model.DatasetTitle = TaskManager.Bus[CreateTaskmanager.ENTITY_TITLE].ToString();
            }
            else
                Model.DatasetTitle = "No Title available.";

            Model.DatasetId = entityId;
            Model.Created = created;

            //check if a metadatastructure has a import mapping
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);

            if (metadataStructureId != -1)
                Model.Import = IsImportAvavilable(metadataStructureId);

            //FromCreateOrEditMode
            Model.FromEditMode = (bool)TaskManager.Bus[CreateTaskmanager.EDIT_MODE];

            Model.EditRight = hasUserEditRights(entityId);
            Model.EditAccessRight = hasUserEditAccessRights(entityId);
            Model.LatestVersion = true;

            //set addtionaly functions
            Model.Actions = getAddtionalActions(TaskManager);

            //save with errors?
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.SAVE_WITH_ERRORS))
            {
                Model.SaveWithErrors = (bool)TaskManager.Bus[CreateTaskmanager.SAVE_WITH_ERRORS];
            }

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.NO_IMPORT_ACTION))
            {
                Model.Import = !(bool)TaskManager.Bus[CreateTaskmanager.NO_IMPORT_ACTION];
            }

            //Replace the title of the info box on top
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.INFO_ON_TOP_TITLE))
            {
                ViewBag.Title = PresentationModel.GetViewTitleForTenant(Convert.ToString(TaskManager.Bus[CreateTaskmanager.INFO_ON_TOP_TITLE]), this.Session.GetTenant());
            }

            //Replace the description in the info box on top
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.INFO_ON_TOP_DESCRIPTION))
            {
                Model.HeaderHelp = Convert.ToString(TaskManager.Bus[CreateTaskmanager.INFO_ON_TOP_DESCRIPTION]);
            }

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.LOCKED))
            {
                ViewData["Locked"] = (bool)TaskManager.Bus[CreateTaskmanager.LOCKED];
            }

            ViewData["MetadataStructureID"] = TaskManager.Bus["MetadataStructureId"];

            Session["CreateDatasetTaskmanager"] = TaskManager;

            #endregion prepare model & View Data

            if (asPartial) return PartialView("MetadataEditor", Model);

            return View("MetadataEditor", Model);
        }

        public ActionResult LoadMetadataFromExternal(long entityId, string title, long metadatastructureId, long datastructureId = -1, long researchplanId = -1, string sessionKeyForMetadata = "", bool resetTaskManager = false, bool latest = true, string isValid = "yes")
        {
            var loadFromExternal = true;
            long metadataStructureId = -1;

            FormHelper.ClearCache();

            //load metadata from session if exist
            var metadata = Session[sessionKeyForMetadata] != null
                ? (XmlDocument)Session[sessionKeyForMetadata]
                : new XmlDocument();

            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Dataset", this.Session.GetTenant()); ;
            ViewData["Locked"] = true;
            ViewData["ShowOptional"] = false;
            ViewData["isValid"] = isValid;
            ViewData["EntityId"] = entityId;

            TaskManager = (CreateTaskmanager)Session["ViewDatasetTaskmanager"];
            if (TaskManager == null || resetTaskManager)
            {
                TaskManager = new CreateTaskmanager();
            }

            var Model = new MetadataEditorModel();

            using (var dm = new DatasetManager())
            using (var rpm = new ResearchPlanManager())
            {
                if (loadFromExternal)
                {
                    var entityClassPath = "";
                    Session["ViewDatasetTaskmanager"] = TaskManager;
                    TaskManager.AddToBus(CreateTaskmanager.ENTITY_ID, entityId);

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_CLASS_PATH))
                        entityClassPath = TaskManager.Bus[CreateTaskmanager.ENTITY_CLASS_PATH].ToString();

                    var ready = true;

                    // todo i case of entity "BExIS.Dlm.Entities.Data.Dataset" we need to have a check if the dataset is checked in later all enitities should support such functions over webapis
                    if (entityClassPath.Equals("BExIS.Dlm.Entities.Data.Dataset"))
                    {
                        //todo need a check if entity is in use
                        if (!dm.IsDatasetCheckedIn(entityId))
                        {
                            ready = false;
                        }
                    }

                    if (ready)
                    {
                        TaskManager = loadTaskManager(metadatastructureId, datastructureId, researchplanId, metadata, title, TaskManager, ref Model);
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Dataset is just in processing.");
                    }
                }

                Model.DatasetId = entityId;
                Model.Created = false;

                //check if a metadatastructure has a import mapping
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                    metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);

                if (metadataStructureId != -1)
                    Model.Import = IsImportAvavilable(metadataStructureId);

                //FromCreateOrEditMode
                TaskManager.AddToBus(CreateTaskmanager.EDIT_MODE, false);
                Model.FromEditMode = (bool)TaskManager.Bus[CreateTaskmanager.EDIT_MODE];

                // set edit rights
                Model.EditRight = hasUserEditRights(entityId);
                Model.EditAccessRight = hasUserEditAccessRights(entityId);
                Model.LatestVersion = latest;

                //set addtionaly functions
                Model.Actions = getAddtionalActions(TaskManager);

                //save with errors?
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.SAVE_WITH_ERRORS))
                {
                    Model.SaveWithErrors = (bool)TaskManager.Bus[CreateTaskmanager.SAVE_WITH_ERRORS];
                }

                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.NO_IMPORT_ACTION))
                {
                    Model.Import = !(bool)TaskManager.Bus[CreateTaskmanager.NO_IMPORT_ACTION];
                }

                //Replace the title of the info box on top
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.INFO_ON_TOP_TITLE))
                {
                    ViewBag.Title = PresentationModel.GetViewTitleForTenant(Convert.ToString(TaskManager.Bus[CreateTaskmanager.INFO_ON_TOP_TITLE]), this.Session.GetTenant());
                }

                //Replace the description in the info box on top
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.INFO_ON_TOP_DESCRIPTION))
                {
                    Model.HeaderHelp = Convert.ToString(TaskManager.Bus[CreateTaskmanager.INFO_ON_TOP_DESCRIPTION]);
                }
            }

            ViewData["MetadataStructureID"] = TaskManager.Bus["MetadataStructureId"];
            return PartialView("MetadataEditor", Model);
        }

        public ActionResult LoadMetadataOfflineVersion(long entityId, string title, long metadatastructureId, long datastructureId = -1, long researchplanId = -1, string sessionKeyForMetadata = "", bool resetTaskManager = false)
        {
            using (var dm = new DatasetManager())
            using (var rpm = new ResearchPlanManager())
            {
                var loadFromExternal = true;
                long metadataStructureId = -1;

                //load metadata from session if exist
                var metadata = new XmlDocument();

                if (Session[sessionKeyForMetadata] != null)
                {
                    metadata = (XmlDocument)Session[sessionKeyForMetadata];
                }
                else
                {
                    //load metadata from latest version
                    metadata = dm.GetDatasetLatestMetadataVersion(entityId);
                }

                ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Dataset", this.Session.GetTenant()); ;
                ViewData["Locked"] = true;
                ViewData["ShowOptional"] = false;
                ViewData["EntityId"] = entityId;

                TaskManager = (CreateTaskmanager)Session["ViewDatasetTaskmanager"];
                if (TaskManager == null || resetTaskManager)
                {
                    TaskManager = new CreateTaskmanager();
                }

                var stepInfoModelHelpers = new List<StepModelHelper>();
                var Model = new MetadataEditorModel();

                if (loadFromExternal)
                {
                    var entityClassPath = "";
                    //TaskManager = new CreateTaskmanager();
                    Session["DownloadDatasetTaskmanager"] = TaskManager;
                    TaskManager.AddToBus(CreateTaskmanager.ENTITY_ID, entityId);

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_CLASS_PATH))
                        entityClassPath = TaskManager.Bus[CreateTaskmanager.ENTITY_CLASS_PATH].ToString();

                    var ready = true;

                    // todo i case of entity "BExIS.Dlm.Entities.Data.Dataset" we need to have a check if the dataset is checked in later all enitities should support such functions over webapis
                    if (entityClassPath.Equals("BExIS.Dlm.Entities.Data.Dataset"))
                    {
                        //todo need a check if entity is in use
                        if (!dm.IsDatasetCheckedIn(entityId))
                        {
                            ready = false;
                        }
                    }

                    if (ready)
                    {
                        TaskManager.AddToBus(CreateTaskmanager.METADATASTRUCTURE_ID, metadatastructureId);
                        if (researchplanId != -1) TaskManager.AddToBus(CreateTaskmanager.RESEARCHPLAN_ID, researchplanId);
                        if (datastructureId != -1) TaskManager.AddToBus(CreateTaskmanager.DATASTRUCTURE_ID, datastructureId);

                        if (metadata != null && metadata.DocumentElement != null)
                            TaskManager.AddToBus(CreateTaskmanager.METADATA_XML, XmlUtility.ToXDocument(metadata));

                        TaskManager.AddToBus(CreateTaskmanager.ENTITY_TITLE, title);

                        TaskManager.AddToBus(CreateTaskmanager.RESEARCHPLAN_ID, rpm.Repo.Get().First().Id);

                        TaskManager = AdvanceTaskManagerBasedOnExistingMetadata(metadatastructureId, TaskManager);

                        foreach (var stepInfo in TaskManager.StepInfos)
                        {
                            var stepModelHelper = GetStepModelhelper(stepInfo.Id, TaskManager);

                            if (stepModelHelper.Model == null)
                            {
                                if (stepModelHelper.UsageType.Equals(typeof(MetadataPackageUsage)))
                                {
                                    stepModelHelper.Model = createPackageModel(stepInfo.Id, false);
                                    if (stepModelHelper.Model.StepInfo.IsInstanze)
                                        LoadSimpleAttributesForModelFromXml(stepModelHelper, TaskManager);
                                }

                                if (stepModelHelper.UsageType.Equals(typeof(MetadataNestedAttributeUsage)))
                                {
                                    stepModelHelper.Model = createCompoundModel(stepInfo.Id, false);
                                    if (stepModelHelper.Model.StepInfo.IsInstanze)
                                        LoadSimpleAttributesForModelFromXml(stepModelHelper, TaskManager);
                                }

                                getChildModelsHelper(stepModelHelper, TaskManager);
                            }

                            stepInfoModelHelpers.Add(stepModelHelper);
                        }

                        if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_XML))
                        {
                            var xMetadata = getMetadata(TaskManager);

                            if (String.IsNullOrEmpty(title)) title = "No Title available.";

                            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_TITLE))
                            {
                                if (TaskManager.Bus[CreateTaskmanager.ENTITY_TITLE] != null)
                                    Model.DatasetTitle = TaskManager.Bus[CreateTaskmanager.ENTITY_TITLE].ToString();
                            }
                            else
                                Model.DatasetTitle = "No Title available.";
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(String.Empty, "Dataset is just in processing.");
                    }
                }

                Model.DatasetId = entityId;
                Model.StepModelHelpers = stepInfoModelHelpers;
                Model.Created = false;

                //check if a metadatastructure has a import mapping
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                    metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);

                if (metadataStructureId != -1)
                    Model.Import = IsImportAvavilable(metadataStructureId);

                //FromCreateOrEditMode
                TaskManager.AddToBus(CreateTaskmanager.EDIT_MODE, false);
                Model.FromEditMode = (bool)TaskManager.Bus[CreateTaskmanager.EDIT_MODE];

                // set edit rights
                Model.EditRight = hasUserEditRights(entityId);
                Model.EditAccessRight = hasUserEditAccessRights(entityId);

                //set addtionaly functions
                Model.Actions = getAddtionalActions(TaskManager);

                return PartialView("MetadataEditorOffline", Model);
            }
        }

        public ActionResult ReloadMetadataEditor(
            bool locked = false,
            bool show = false,
            bool created = false,
            long entityId = -1,
            bool fromEditMode = false,
            bool latestVersion = false
            )
        {
            ViewData["Locked"] = locked;
            ViewData["ShowOptional"] = show;
            ViewData["EntityId"] = entityId;

            FormHelper.ClearCache();

            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Dataset", this.Session.GetTenant());
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            //TaskManager?.AddToBus(CreateTaskmanager.SAVE_WITH_ERRORS, true);

            var stepInfoModelHelpers = new List<StepModelHelper>();

            foreach (var stepInfo in TaskManager.StepInfos)
            {
                var stepModelHelper = GetStepModelhelper(stepInfo.Id, TaskManager);

                if (stepModelHelper.Model == null)
                {
                    if (stepModelHelper.UsageType.Equals(typeof(MetadataPackageUsage)))
                        stepModelHelper.Model = createPackageModel(stepInfo.Id, false);

                    if (stepModelHelper.UsageType.Equals(typeof(MetadataNestedAttributeUsage)))
                        stepModelHelper.Model = createCompoundModel(stepInfo.Id, false);

                    getChildModelsHelper(stepModelHelper, TaskManager);
                }

                stepInfoModelHelpers.Add(stepModelHelper);
            }

            var Model = new MetadataEditorModel();
            Model.StepModelHelpers = stepInfoModelHelpers;
            Model.SaveWithErrors = Model.StepModelHelpers.Any(s => s.Model.ErrorList.Count() > 0);

            #region security permissions and authorisations check

            // set edit rigths

            bool hasAuthorizationRights = false;
            bool hasAuthenticationRigths = false;

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
            {
                entityId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
                Model.EditRight = hasUserEditRights(entityId);
                Model.EditAccessRight = hasUserEditAccessRights(entityId);
            }
            else
            {
                Model.EditRight = false;
                Model.EditAccessRight = false;
            }

            //Model.FromEditMode = true;

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
            {
                long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);
                Model.Import = IsImportAvavilable(metadataStructureId);
            }

            #endregion security permissions and authorisations check

            //set addtionaly functions
            Model.Actions = getAddtionalActions(TaskManager);

            //save with errors?
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.SAVE_WITH_ERRORS))
            {
                Model.SaveWithErrors = (bool)TaskManager.Bus[CreateTaskmanager.SAVE_WITH_ERRORS];
            }

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.NO_IMPORT_ACTION))
            {
                Model.Import = !(bool)TaskManager.Bus[CreateTaskmanager.NO_IMPORT_ACTION];
            }

            //Replace the title of the info box on top
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.INFO_ON_TOP_TITLE))
            {
                ViewBag.Title = PresentationModel.GetViewTitleForTenant(Convert.ToString(TaskManager.Bus[CreateTaskmanager.INFO_ON_TOP_TITLE]), this.Session.GetTenant());
            }

            //Replace the description in the info box on top
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.INFO_ON_TOP_DESCRIPTION))
            {
                Model.HeaderHelp = Convert.ToString(TaskManager.Bus[CreateTaskmanager.INFO_ON_TOP_DESCRIPTION]);
            }

            Model.Created = created;
            Model.FromEditMode = fromEditMode;
            Model.DatasetId = entityId;
            Model.LatestVersion = latestVersion;

            //set title
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_TITLE))
            {
                if (TaskManager.Bus[CreateTaskmanager.ENTITY_TITLE] != null)
                    Model.DatasetTitle = TaskManager.Bus[CreateTaskmanager.ENTITY_TITLE].ToString();
            }
            else
                Model.DatasetTitle = "No Title available.";

            ViewData["MetadataStructureID"] = TaskManager.Bus["MetadataStructureId"];
            return PartialView("MetadataEditor", Model);
        }

        public ActionResult StartMetadataEditor()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Dataset", this.Session.GetTenant());
            ViewData["ShowOptional"] = true;
            ViewData["EntityId"] = (long)-1;

            FormHelper.ClearCache();

            var Model = new MetadataEditorModel();

            if (TaskManager == null) TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager != null)
            {
                //load empty metadata xml if needed
                if (!TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_XML))
                {
                    CreateXml(TaskManager);
                }

                var loaded = false;
                //check if formsteps are loaded
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.FORM_STEPS_LOADED))
                {
                    loaded = (bool)TaskManager.Bus[CreateTaskmanager.FORM_STEPS_LOADED];
                }

                // load form steps
                if (loaded == false && TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                {
                    var metadataStrutureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);
                    // generate all steps
                    // one step for each complex type  in the metadata structure
                    AdvanceTaskManager(metadataStrutureId);
                }

                var stepInfoModelHelpers = new List<StepModelHelper>();

                // foreach step and the childsteps... generate a stepModelhelper
                foreach (var stepInfo in TaskManager.StepInfos)
                {
                    var stepModelHelper = GetStepModelhelper(stepInfo.Id, TaskManager);

                    if (stepModelHelper.Model == null)
                    {
                        if (stepModelHelper.UsageType.Equals(typeof(MetadataPackageUsage)))
                            stepModelHelper.Model = createPackageModel(stepInfo.Id, false);

                        if (stepModelHelper.UsageType.Equals(typeof(MetadataNestedAttributeUsage)))
                            stepModelHelper.Model = createCompoundModel(stepInfo.Id, false);

                        getChildModelsHelper(stepModelHelper, TaskManager);
                    }

                    stepInfoModelHelpers.Add(stepModelHelper);
                }

                Model.StepModelHelpers = stepInfoModelHelpers;

                //set import
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                {
                    var id = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);
                    Model.Import = IsImportAvavilable(id);
                }

                //set addtionaly functions
                Model.Actions = getAddtionalActions(TaskManager);

                //save with errors?
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.SAVE_WITH_ERRORS))
                {
                    Model.SaveWithErrors = (bool)TaskManager.Bus[CreateTaskmanager.SAVE_WITH_ERRORS];
                }

                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.NO_IMPORT_ACTION))
                {
                    Model.Import = !(bool)TaskManager.Bus[CreateTaskmanager.NO_IMPORT_ACTION];
                }

                //Replace the title of the info box on top
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.INFO_ON_TOP_TITLE))
                {
                    ViewBag.Title = PresentationModel.GetViewTitleForTenant(Convert.ToString(TaskManager.Bus[CreateTaskmanager.INFO_ON_TOP_TITLE]), this.Session.GetTenant());
                }

                //Replace the description in the info box on top
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.INFO_ON_TOP_DESCRIPTION))
                {
                    Model.HeaderHelp = Convert.ToString(TaskManager.Bus[CreateTaskmanager.INFO_ON_TOP_DESCRIPTION]);
                }
            }

            ViewData["MetadataStructureID"] = TaskManager.Bus["MetadataStructureId"];
            return View("MetadataEditor", Model);
        }

        public ActionResult SwitchVisibilityOfOptionalElements(bool show, bool created, long entityId, bool fromEditMode, bool latestVersion)
        {
            return RedirectToAction("ReloadMetadataEditor", new
            {
                locked = true,
                show = !show,
                created,
                entityId,
                fromEditMode,
                latestVersion
            });
        }

        private Dictionary<string, ActionInfo> getAddtionalActions(CreateTaskmanager taskmanager)
        {
            if (taskmanager.Actions.Any())
            {
                return taskmanager.Actions;
            }

            return new Dictionary<string, ActionInfo>();
        }

        #endregion Load Metadata formular actions

        #region Import Metadata From external XML

        public ActionResult LoadExternalXml()
        {
            var validationMessage = "";

            if (TaskManager == null) TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager != null &&
                TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID) &&
                TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_IMPORT_XML_FILEPATH))
            {
                //xml metadata for import
                var metadataForImportPath = (string)TaskManager.Bus[CreateTaskmanager.METADATA_IMPORT_XML_FILEPATH];

                if (FileHelper.FileExist(metadataForImportPath))
                {
                    var metadataForImport = new XmlDocument();
                    metadataForImport.Load(metadataForImportPath);

                    // metadataStructure ID
                    var metadataStructureId = (Int64)TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID];
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
                    var completeMetadata = XmlMetadataImportHelper.FillInXmlValues(metadataResult,
                        metadataXmlTemplate);

                    TaskManager.AddToBus(CreateTaskmanager.METADATA_XML, XmlUtility.ToXDocument(completeMetadata));

                    //LoadMetadata(long datasetId, bool locked= false, bool created= false, bool fromEditMode = false, bool resetTaskManager = false, XmlDocument newMetadata=null)
                    return RedirectToAction("ImportMetadata", "Form",
                        new { metadataStructureId = metadataStructureId });
                }
            }

            return Content("Error Message :" + validationMessage);
        }

        /// <summary>
        /// Selected File store din the BUS
        /// </summary>
        /// <param name="SelectFileUploader"></param>
        /// <returns></returns>
        public ActionResult SelectFileProcess(HttpPostedFileBase SelectFileUploader)
        {
            if (TaskManager == null) TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            if (SelectFileUploader != null)
            {
                //data/datasets/1/1/
                var dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                var storepath = Path.Combine(dataPath, "Temp", GetUsernameOrDefault());

                // if folder not exist
                if (!Directory.Exists(storepath))
                {
                    Directory.CreateDirectory(storepath);
                }

                var path = Path.Combine(storepath, SelectFileUploader.FileName);

                SelectFileUploader.SaveAs(path);

                TaskManager.AddToBus(CreateTaskmanager.METADATA_IMPORT_XML_FILEPATH, path);
            }

            return Content("");
        }

        public ActionResult ValidateExternalXml()
        {
            var validationMessage = "";

            if (TaskManager == null) TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager != null &&
                TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID) &&
                TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_IMPORT_XML_FILEPATH))
            {
                using (var metadataStructureManager = new MetadataStructureManager())
                {
                    //xml metadata for import
                    var metadataForImportPath = (string)TaskManager.Bus[CreateTaskmanager.METADATA_IMPORT_XML_FILEPATH];

                    if (FileHelper.FileExist(metadataForImportPath))
                    {
                        var metadataForImport = new XmlDocument();
                        metadataForImport.Load(metadataForImportPath);

                        // metadataStructure DI
                        var metadataStructureId = (Int64)TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID];
                        var metadataStructrueName = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId).Name;

                        // loadMapping file
                        var path_mappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), XmlMetadataImportHelper.GetMappingFileName(metadataStructureId, TransmissionType.mappingFileImport, metadataStructrueName));

                        // XML mapper + mapping file
                        var xmlMapperManager = new XmlMapperManager(TransactionDirection.ExternToIntern);
                        xmlMapperManager.Load(path_mappingFile, "IDIV");

                        validationMessage = xmlMapperManager.Validate(metadataForImport);
                    }
                }
            }

            return Content(validationMessage);
        }

        #endregion Import Metadata From external XML

        #region Add and Remove and Activate and Update

        public ActionResult ActivateComplexUsage(int id)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            var stepModelHelper = GetStepModelhelper(id, TaskManager);

            var active = stepModelHelper.Activated ? false : true;
            stepModelHelper.Activated = active;

            if (stepModelHelper.Parent != null)
            {
                var pStepModelHelper = GetStepModelhelper(stepModelHelper.Parent.StepId, TaskManager);
                if (pStepModelHelper != null)
                    pStepModelHelper.Activated = active;
            }

            // Set entity id
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
            {
                if (TaskManager.Bus[CreateTaskmanager.ENTITY_ID] != null)
                    ViewData["EntityId"] = (long)TaskManager.Bus[CreateTaskmanager.ENTITY_ID];
            }
            else
                ViewData["EntityId"] = (long)-1;

            return PartialView("_metadataCompoundAttributeUsageView", stepModelHelper);
        }

        public ActionResult ActivateComplexUsageInAChoice(int parentid, int id)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            var stepModelHelper = GetStepModelhelper(id, TaskManager);

            var active = stepModelHelper.Activated ? false : true;
            stepModelHelper.Activated = active;
            stepModelHelper.Parent.Activated = active;

            var firstOrDefault = stepModelHelper.Childrens.FirstOrDefault();
            if (firstOrDefault != null)
                firstOrDefault.Activated = active;

            var pStepModelHelper = GetStepModelhelper(parentid, TaskManager);
            pStepModelHelper.Activated = active;

            //update stepModel to parentStepModel
            for (var i = 0; i < pStepModelHelper.Childrens.Count; i++)
            {
                var usage = pStepModelHelper.Childrens.ElementAt(i);
                var childStepModelHelper = GetStepModelhelper(usage.StepId, TaskManager);
                usage.Activated = usage.StepId.Equals(id);
                childStepModelHelper.Activated = usage.StepId.Equals(id);

                var type = usage.Childrens.FirstOrDefault();
                if (type != null)
                {
                    type.Activated = usage.Activated;

                    if (!usage.Activated) ClearXml(type.XPath);
                }
            }

            return PartialView("_metadataCompoundAttributeUsageView", pStepModelHelper);
        }

        public ActionResult AddComplexUsage(int parentStepId, int number)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            ViewData["ShowOptional"] = true;
            //TaskManager.SetCurrent(TaskManager.Get(parentStepId));

            var metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);
            var position = number + 1;

            var parentStepModelHelper = GetStepModelhelper(parentStepId, TaskManager);

            //Create new step
            var newStep = new StepInfo(parentStepModelHelper.UsageName)
            {
                Id = TaskManager.GenerateStepId(),
                parentTitle = parentStepModelHelper.Model.StepInfo.title,
                Parent = parentStepModelHelper.Model.StepInfo,
                IsInstanze = true,
            };

            string xPath = parentStepModelHelper.XPath + "//" + parentStepModelHelper.UsageAttrName + "[" + position + "]";
            // add to parent stepId
            parentStepModelHelper.Model.StepInfo.Children.Add(newStep);
            TaskManager.StepInfos.Add(newStep);

            // create Model
            AbstractMetadataStepModel model = null;

            if (parentStepModelHelper.UsageType.Equals(typeof(MetadataAttributeUsage)) ||
                parentStepModelHelper.UsageType.Equals(typeof(MetadataNestedAttributeUsage)))
            {
                BaseUsage usage = loadUsage(parentStepModelHelper.UsageId, parentStepModelHelper.UsageType);
                model = FormHelper.CreateMetadataCompoundAttributeModel(usage, number);
                model.Number = position;

                ((MetadataCompoundAttributeModel)model).ConvertMetadataAttributeModels(usage, metadataStructureId, newStep.Id);
                ((MetadataCompoundAttributeModel)model).ConvertMetadataParameterModels(usage, metadataStructureId, newStep.Id);

                //Update metadata xml
                //add step to metadataxml
                AddCompoundAttributeToXml(model.Source, model.Number, parentStepModelHelper.XPath);
            }

            if (parentStepModelHelper.UsageType.Equals(typeof(MetadataPackageUsage)))
            {
                BaseUsage usage = loadUsage(parentStepModelHelper.UsageId, parentStepModelHelper.UsageType);
                model = FormHelper.CreateMetadataPackageModel(usage, number);
                model.Number = position;
                ((MetadataPackageModel)model).ConvertMetadataAttributeModels(usage, metadataStructureId, newStep.Id);

                //Update metadata xml
                //add step to metadataxml
                AddPackageToXml(model.Source, model.Number, parentStepModelHelper.XPath);
            }

            // create StepModel for new step
            var newStepModelhelper = new StepModelHelper
            {
                StepId = newStep.Id,
                UsageId = parentStepModelHelper.UsageId,
                UsageName = parentStepModelHelper.UsageName,
                UsageType = parentStepModelHelper.UsageType,
                UsageAttrName = parentStepModelHelper.UsageAttrName,
                Number = position,
                Model = model,
                XPath = xPath,
                Level = parentStepModelHelper.Level + 1,
                Activated = true
            };

            newStep.Children = GetChildrenSteps(newStepModelhelper.UsageId, newStepModelhelper.UsageType, newStep, xPath, newStepModelhelper);
            newStepModelhelper.Model.StepInfo = newStep;
            newStepModelhelper = getChildModelsHelper(newStepModelhelper, TaskManager);

            // add stepmodel to dictionary
            AddStepModelhelper(newStepModelhelper);

            //add stepModel to parentStepModel
            parentStepModelHelper.Childrens.Insert(newStepModelhelper.Number - 1, newStepModelhelper);

            //update childrens of the parent step based on number
            for (var i = 0; i < parentStepModelHelper.Childrens.Count; i++)
            {
                var smh = parentStepModelHelper.Childrens.ElementAt(i);
                smh.Number = i + 1;
            }

            // add step to parent and update title of steps
            //parentStepModelHelper.Model.StepInfo.Children.Insert(newStepModelhelper.Number - 1, newStep);
            for (var i = 0; i < parentStepModelHelper.Model.StepInfo.Children.Count; i++)
            {
                var si = parentStepModelHelper.Model.StepInfo.Children.ElementAt(i);
                si.title = (i + 1).ToString();
            }

            //// load InstanzB for parentmodel
            parentStepModelHelper.Model.ConvertInstance(getMetadata(TaskManager), parentStepModelHelper.XPath);

            return PartialView("_metadataCompoundAttributeView", parentStepModelHelper);
        }

        public ActionResult AddMetadataAttributeUsage(int id, int parentid, int number, int parentModelNumber, int parentStepId)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            var metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);

            var list = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];

            var stepModelHelperParent = list.Where(s => s.StepId.Equals(parentStepId)).FirstOrDefault();

            var parentUsage = loadUsage(stepModelHelperParent.UsageId, stepModelHelperParent.UsageType);
            var pNumber = stepModelHelperParent.Number;

            var metadataAttributeUsage = metadataStructureUsageHelper.GetChildren(parentUsage.Id, parentUsage.GetType()).Where(u => u.Id.Equals(id)).FirstOrDefault();

            //BaseUsage metadataAttributeUsage = MetadataStructureUsageHelper.GetSimpleUsageById(parentUsage, id);

            var modelAttribute = FormHelper.CreateMetadataAttributeModel(metadataAttributeUsage, parentUsage, metadataStructureId, parentModelNumber, stepModelHelperParent.StepId);
            modelAttribute.Number = ++number;
            modelAttribute.Parameters.ForEach(p => p.AttributeNumber = modelAttribute.Number);

            var model = stepModelHelperParent.Model;

            Insert(modelAttribute, stepModelHelperParent, number);
            UpdateChildrens(stepModelHelperParent, modelAttribute.Source.Id);

            //addtoxml
            AddAttributeToXml(parentUsage, parentModelNumber, metadataAttributeUsage, number, stepModelHelperParent.XPath);

            model.ConvertInstance(getMetadata(TaskManager), stepModelHelperParent.XPath + "//" + metadataAttributeUsage.Label.Replace(" ", string.Empty));

            if (model != null)
            {
                return PartialView("_metadataCompoundAttributeUsageView", stepModelHelperParent);
            }

            return null;
        }

        public ActionResult DownComplexUsage(int parentStepId, int number)
        {
            var newIndex = number;
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            TaskManager.SetCurrent(TaskManager.Get(parentStepId));

            var stepModelHelper = GetStepModelhelper(parentStepId, TaskManager);
            //var u = LoadUsage(stepModelHelper.Usage);

            if (newIndex <= stepModelHelper.Childrens.Count - 1)
            {
                var xPathOfSelectedElement = stepModelHelper.XPath + "//" + stepModelHelper.UsageAttrName.Replace(" ", string.Empty) + "[" + number + "]";
                var destinationXPathElement = stepModelHelper.XPath + "//" + stepModelHelper.UsageAttrName.Replace(" ", string.Empty) + "[" + (number + 1) + "]";

                ChangeInXml(xPathOfSelectedElement, destinationXPathElement);

                stepModelHelper.Model = createModel(TaskManager.Current().Id, true, stepModelHelper.UsageType);

                var selectedStepModelHelper = stepModelHelper.Childrens.ElementAt(number - 1);
                stepModelHelper.Childrens.Remove(selectedStepModelHelper);

                stepModelHelper.Childrens.Insert(newIndex, selectedStepModelHelper);

                //update childrens of the parent step based on number
                for (var i = 0; i < stepModelHelper.Childrens.Count; i++)
                {
                    var smh = stepModelHelper.Childrens.ElementAt(i);
                    smh.Number = i + 1;
                    smh.Model.Number = i + 1;
                }

                var selectedStepInfo = stepModelHelper.Model.StepInfo.Children.ElementAt(number - 1);
                stepModelHelper.Model.StepInfo.Children.Remove(selectedStepInfo);
                stepModelHelper.Model.StepInfo.Children.Insert(newIndex, selectedStepInfo);

                for (var i = 0; i < stepModelHelper.Model.StepInfo.Children.Count; i++)
                {
                    var si = stepModelHelper.Model.StepInfo.Children.ElementAt(i);
                    si.title = (i + 1).ToString();
                }

                stepModelHelper.Model.ConvertInstance(getMetadata(TaskManager), stepModelHelper.XPath);
            }

            return PartialView("_metadataCompoundAttributeView", stepModelHelper);
        }

        public ActionResult DownMetadataAttributeUsage(object value, int id, int parentid, int number, int parentModelNumber, int parentStepId)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            var list = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];

            var stepModelHelperParent = list.Where(s => s.StepId.Equals(parentStepId)).FirstOrDefault();
            var parentUsage = loadUsage(stepModelHelperParent.UsageId, stepModelHelperParent.UsageType);
            var attrUsage = metadataStructureUsageHelper.GetChildren(parentUsage.Id, parentUsage.GetType()).Where(u => u.Id.Equals(id)).FirstOrDefault();

            GetUsageAttrName(attrUsage);

            // up in xml

            var xPathOfSelectedElement = stepModelHelperParent.XPath + "//" + attrUsage.Label + "//" + GetUsageAttrName(attrUsage).Replace(" ", string.Empty) + "[" + number + "]";
            var destinationXPathElement = stepModelHelperParent.XPath + "//" + attrUsage.Label + "//" + GetUsageAttrName(attrUsage).Replace(" ", string.Empty) + "[" + (number + 1) + "]";

            ChangeInXml(xPathOfSelectedElement, destinationXPathElement);

            Down(stepModelHelperParent, id, number);

            UpdateChildrens(stepModelHelperParent, id);

            var model = stepModelHelperParent.Model;

            if (model != null)
            {
                return PartialView("_metadataCompoundAttributeUsageView", stepModelHelperParent);
            }

            return null;
        }

        public ActionResult RemoveComplexUsage(int parentStepId, int number)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            // if you are able to remove a complex usage from the ui you are in the edit mode
            // in the edit mode all optional fields should be visible
            ViewData["ShowOptional"] = true;

            // Set Current Step of the Taskmanger
            // maybee its not needed anymore but the base idea was that the taskmanager knows where we are
            TaskManager.SetCurrent(TaskManager.Get(parentStepId));

            // Each step has a stepModelHelp with basic informations in the Taskmanager
            // load the parent Model helper based on the parentstep id to remove the child with the number that comes in
            var stepModelHelper = GetStepModelhelper(parentStepId, TaskManager);

            // remove the Complex Usage from the XML
            RemoveFromXml(stepModelHelper.XPath + "//" + stepModelHelper.UsageAttrName.Replace(" ", string.Empty) + "[" + number + "]");

            // Create the parent model for the ui
            stepModelHelper.Model = createModel(TaskManager.Current().Id, true, stepModelHelper.UsageType);
            // Remove the child with the given number from the list
            stepModelHelper.Childrens.RemoveAt(number - 1);

            //Update the position and the xpath from all other childrens
            for (var i = 0; i < stepModelHelper.Childrens.Count; i++)
            {
                var child = stepModelHelper.Childrens.ElementAt(i);
                // update new position in the stepmodel helper and in dthe view model
                child.Number = i + 1;
                child.Model.Number = child.Number;
                // update the xpath path
                var newXPath = stepModelHelper.XPath + "//" + stepModelHelper.UsageAttrName.Replace(" ", string.Empty) + "[" + child.Number + "]";
                child.XPath = newXPath;
            }

            // remove the stepinfo of the removed complex usage
            TaskManager.Remove(TaskManager.Current(), number - 1);

            return PartialView("_metadataCompoundAttributeView", stepModelHelper);
        }

        public ActionResult RemoveMetadataAttributeUsage(object value, int id, int parentid, int number, int parentModelNumber, int parentStepId)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            var list = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];

            var stepModelHelperParent = list.Where(s => s.StepId.Equals(parentStepId)).FirstOrDefault();

            //find the right element in the list
            var removeAttributeModel = stepModelHelperParent.Model.MetadataAttributeModels.Where(m => m.Source.Id.Equals(id) && m.Number.Equals(number)).First();
            //remove attribute
            stepModelHelperParent.Model.MetadataAttributeModels.Remove(removeAttributeModel);
            //update childrenList
            UpdateChildrens(stepModelHelperParent, id);

            var model = stepModelHelperParent.Model;

            var parentUsage = loadUsage(stepModelHelperParent.UsageId, stepModelHelperParent.UsageType);
            var attrUsage = metadataStructureUsageHelper.GetChildren(parentUsage.Id, parentUsage.GetType()).Where(u => u.Id.Equals(id)).FirstOrDefault();

            //remove from xml
            RemoveAttributeToXml(stepModelHelperParent.Number, attrUsage, number, metadataStructureUsageHelper.GetNameOfType(attrUsage), stepModelHelperParent.XPath);

            if (model != null)
            {
                return PartialView("_metadataCompoundAttributeUsageView", stepModelHelperParent);
            }

            return null;
        }

        public ActionResult UpComplexUsage(int parentStepId, int number)
        {
            var newIndex = number - 2;
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            TaskManager.SetCurrent(TaskManager.Get(parentStepId));

            var stepModelHelper = GetStepModelhelper(parentStepId, TaskManager);

            if (newIndex >= 0)
            {
                var xPathOfSelectedElement = stepModelHelper.XPath + "//" + stepModelHelper.UsageAttrName.Replace(" ", string.Empty) + "[" + number + "]";
                var destinationXPathElement = stepModelHelper.XPath + "//" + stepModelHelper.UsageAttrName.Replace(" ", string.Empty) + "[" + (number - 1) + "]";

                ChangeInXml(xPathOfSelectedElement, destinationXPathElement);

                stepModelHelper.Model = createModel(TaskManager.Current().Id, true, stepModelHelper.UsageType);

                var selectedStepModelHelper = stepModelHelper.Childrens.ElementAt(number - 1);
                stepModelHelper.Childrens.Remove(selectedStepModelHelper);

                stepModelHelper.Childrens.Insert(newIndex, selectedStepModelHelper);

                //update childrens of the parent step based on number
                for (var i = 0; i < stepModelHelper.Childrens.Count; i++)
                {
                    var smh = stepModelHelper.Childrens.ElementAt(i);
                    smh.Number = i + 1;
                    smh.Model.Number = i + 1;
                }

                var selectedStepInfo = stepModelHelper.Model.StepInfo.Children.ElementAt(number - 1);
                stepModelHelper.Model.StepInfo.Children.Remove(selectedStepInfo);
                stepModelHelper.Model.StepInfo.Children.Insert(newIndex, selectedStepInfo);

                for (var i = 0; i < stepModelHelper.Model.StepInfo.Children.Count; i++)
                {
                    var si = stepModelHelper.Model.StepInfo.Children.ElementAt(i);
                    si.title = (i + 1).ToString();
                }

                stepModelHelper.Model.ConvertInstance(getMetadata(TaskManager), stepModelHelper.XPath);
            }

            return PartialView("_metadataCompoundAttributeView", stepModelHelper);
        }

        public ActionResult UpdateComplexUsageWithEntity(int stepId, int number, int inputattrid, int inputAttrNumber, long entityId, long entityTypeId, string value)
        {
            ViewData["ShowOptional"] = true;

            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            TaskManager.SetCurrent(TaskManager.Get(stepId));

            var stepModelHelper = GetStepModelhelper(stepId, TaskManager);
            stepModelHelper.Model = createModel(stepId, true, stepModelHelper.UsageType);
            var usage = loadUsage(stepModelHelper.UsageId, stepModelHelper.UsageType);

            metadataStructureUsageHelper = new MetadataStructureUsageHelper();

            foreach (var attrModel in stepModelHelper.Model.MetadataAttributeModels.Where(a => a.Id.Equals(inputattrid) && a.Number.Equals(inputAttrNumber)))
            {
                var metadataAttributeUsage = metadataStructureUsageHelper.GetChildren(usage.Id, usage.GetType()).Where(u => u.Id.Equals(attrModel.Id)).FirstOrDefault();

                if (entityId > 0 && attrModel.Id.Equals(inputattrid))
                {
                    if (MappingUtils.ExistMappingWithEntity(attrModel.Id, LinkElementType.MetadataNestedAttributeUsage))
                    {
                        attrModel.Value = MappingUtils.GetAllMatchesInEntities(attrModel.Id, LinkElementType.MetadataNestedAttributeUsage, value).Where(e => e.EntityId.Equals(entityId)).FirstOrDefault().Value;
                        attrModel.Locked = false;
                        int version = 0;

                        // if an enity mapping exists, the question is whether the entity has a version or not. The url created may have to be created with version.
                        // Get version of a entity with the entity store

                        #region entity

                        try
                        {
                            if (entityTypeId > 0)
                            {
                                using (EntityManager entityManager = new EntityManager())
                                {
                                    Entity entity = entityManager.Entities.FirstOrDefault(e => e.Id.Equals(entityTypeId));

                                    if (entity != null)
                                    {
                                        var instanceStore = (IEntityStore)Activator.CreateInstance(entity.EntityStoreType);
                                        if (instanceStore != null)
                                        {
                                            version = instanceStore.CountVersions(entityId);
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            version = 0;
                        }

                        #endregion entity

                        //create dic for the xml attr
                        Dictionary<string, string> xmlAttr = new Dictionary<string, string>();
                        xmlAttr.Add(EntityReferenceXmlAttribute.entityid.ToString(), entityId.ToString());
                        xmlAttr.Add(EntityReferenceXmlAttribute.entitytype.ToString(), entityTypeId.ToString());
                        if (version > 0) xmlAttr.Add(EntityReferenceXmlAttribute.entityversion.ToString(), version.ToString());

                        UpdateAttribute(
                        usage,
                        number,
                        metadataAttributeUsage,
                        Convert.ToInt32(inputAttrNumber),
                        attrModel.Value,
                        stepModelHelper.XPath,
                        xmlAttr);
                    }
                }
            }

            return PartialView("_metadataCompoundAttributeUsageView", stepModelHelper);
        }

        public JsonResult UpdateSimpleUsageWithParty(string xpath, long partyId)
        {
            try
            {
                AddXmlAttribute(xpath, "partyid", partyId.ToString());

                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateComplexUsageWithParty(int stepId, int number, long partyId)
        {
            ViewData["ShowOptional"] = true;

            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            TaskManager.SetCurrent(TaskManager.Get(stepId));

            var stepModelHelper = GetStepModelhelper(stepId, TaskManager);
            stepModelHelper.Model = createModel(stepId, true, stepModelHelper.UsageType);
            stepModelHelper.Model.PartyId = partyId;

            var usage = loadUsage(stepModelHelper.UsageId, stepModelHelper.UsageType);

            metadataStructureUsageHelper = new MetadataStructureUsageHelper();

            foreach (var attrModel in stepModelHelper.Model.MetadataAttributeModels)
            {
                var metadataAttributeUsage = metadataStructureUsageHelper.GetChildren(usage.Id, usage.GetType()).Where(u => u.Id.Equals(attrModel.Id)).FirstOrDefault();

                if (partyId > 0)
                {
                    LinkElementType let;
                    if (MappingUtils.ExistMappingWithParty(attrModel.Id, LinkElementType.MetadataNestedAttributeUsage))
                        let = LinkElementType.MetadataNestedAttributeUsage;
                    else
                        let = LinkElementType.MetadataAttributeUsage;

                    attrModel.Value = MappingUtils.GetValueFromSystem(partyId, attrModel.Id, let);
                    attrModel.MappingSelectionField = MappingUtils.PartyAttrIsMain(attrModel.Id, let);
                    attrModel.ParentPartyId = partyId;

                    // in case the parent was mapped as a complex object,
                    // you have to check which of the simple fields is the selection field.
                    // If it is not and there is a mapping for the field, it must be blocked.
                    // OR if its allready locked because of a system mapping then let it locked.
                    if (attrModel.Locked == false && (!attrModel.MappingSelectionField && attrModel.PartyComplexMappingExist && !attrModel.PartySimpleMappingExist))
                    {
                        attrModel.Locked = true;
                    }
                }
                else
                {
                    if (MappingUtils.ExistMappingWithParty(attrModel.Id, LinkElementType.MetadataAttributeUsage) ||
                        MappingUtils.ExistMappingWithParty(attrModel.Id, LinkElementType.MetadataNestedAttributeUsage))
                    {
                        if (attrModel.MappingSelectionField != true) attrModel.Value = "";
                        attrModel.Locked = false;
                    }

                    attrModel.Locked = false;
                    attrModel.ParentPartyId = 0;
                }

                UpdateAttribute(
                   usage,
                   number,
                   metadataAttributeUsage,
                   Convert.ToInt32(attrModel.Number),
                   attrModel.Value,
                   stepModelHelper.XPath);

                AddXmlAttribute(stepModelHelper.XPath, "partyid", partyId.ToString());
            }

            return PartialView("_metadataCompoundAttributeUsageView", stepModelHelper);
        }

        public ActionResult UpMetadataAttributeUsage(object value, int id, int parentid, int number, int parentModelNumber, int parentStepId)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            var list = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];

            var stepModelHelperParent = list.Where(s => s.StepId.Equals(parentStepId)).FirstOrDefault();
            var parentUsage = loadUsage(stepModelHelperParent.UsageId, stepModelHelperParent.UsageType);
            var attrUsage = metadataStructureUsageHelper.GetChildren(parentUsage.Id, parentUsage.GetType()).Where(u => u.Id.Equals(id)).FirstOrDefault();

            GetUsageAttrName(attrUsage);

            // up in xml

            var xPathOfSelectedElement = stepModelHelperParent.XPath + "//" + attrUsage.Label + "//" + GetUsageAttrName(attrUsage).Replace(" ", string.Empty) + "[" + number + "]";
            var destinationXPathElement = stepModelHelperParent.XPath + "//" + attrUsage.Label + "//" + GetUsageAttrName(attrUsage).Replace(" ", string.Empty) + "[" + (number - 1) + "]";
            ChangeInXml(xPathOfSelectedElement, destinationXPathElement);
            // up in Model
            Up(stepModelHelperParent, id, number);

            UpdateChildrens(stepModelHelperParent, id);

            var model = stepModelHelperParent.Model;

            if (model != null)
            {
                return PartialView("_metadataCompoundAttributeUsageView", stepModelHelperParent);
            }

            return null;
        }

        #endregion Add and Remove and Activate and Update

        #region Load & Update advanced steps

        private StepInfo AddStepsBasedOnUsage(BaseUsage usage, StepInfo current, string parentXpath, StepModelHelper parent)
        {
            // genertae action, controller base on usage
            var actionName = "";
            var childName = "";
            var min = usage.MinCardinality;

            if (usage is MetadataPackageUsage)
            {
                actionName = "SetMetadataPackageInstanze";
                childName = ((MetadataPackageUsage)usage).MetadataPackage.Name;
            }
            else
            {
                actionName = "SetMetadataCompoundAttributeInstanze";

                if (usage is MetadataNestedAttributeUsage)
                    childName = ((MetadataNestedAttributeUsage)usage).Member.Name;

                if (usage is MetadataAttributeUsage)
                    childName = ((MetadataAttributeUsage)usage).MetadataAttribute.Name;
            }

            var list = new List<StepInfo>();
            var stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_XML))
            {
                var xMetadata = getMetadata(TaskManager);

                var x = new XElement("null");
                var elements = new List<XElement>();

                var keyValueDic = new Dictionary<string, string>();
                keyValueDic.Add("id", usage.Id.ToString());

                if (usage is MetadataPackageUsage)
                {
                    keyValueDic.Add("type", BExIS.Xml.Helpers.XmlNodeType.MetadataPackageUsage.ToString());
                    elements = XmlUtility.GetXElementsByAttribute(usage.Label, keyValueDic, xMetadata).ToList();
                }
                else
                {
                    keyValueDic.Add("type", BExIS.Xml.Helpers.XmlNodeType.MetadataAttributeUsage.ToString());
                    elements = XmlUtility.GetXElementsByAttribute(usage.Label, keyValueDic, xMetadata).ToList();
                }

                x = elements.FirstOrDefault();

                if (x != null && !x.Name.Equals("null"))
                {
                    var xelements = x.Elements();

                    if (xelements.Count() > 0)
                    {
                        var counter = 0;
                        var title = "";
                        Int64 id = 0;
                        var xPath = "";
                        StepInfo s;
                        StepModelHelper newStepModelHelper = new StepModelHelper();

                        foreach (var element in xelements)
                        {
                            counter++;
                            title = counter.ToString(); //usage.Label+" (" + counter + ")";
                            id = Convert.ToInt64((element.Attribute("roleId")).Value.ToString());

                            s = new StepInfo(title)
                            {
                                Id = TaskManager.GenerateStepId(),
                                Parent = current,
                                IsInstanze = true,
                                HasContent = metadataStructureUsageHelper.HasUsagesWithSimpleType(usage.Id, usage.GetType()),
                            };

                            xPath = parentXpath + "//" + childName.Replace(" ", string.Empty) + "[" + counter + "]";

                            if (TaskManager.Root.Children.Where(z => z.title.Equals(title)).Count() == 0)
                            {
                                newStepModelHelper = new StepModelHelper(s.Id, counter, usage.Id, usage.Label, GetUsageAttrName(usage), usage.GetType(), xPath, parent, usage.Extra);
                                stepHelperModelList.Add(newStepModelHelper);

                                s.Children = GetChildrenSteps(usage.Id, usage.GetType(), s, xPath, newStepModelHelper);

                                current.Children.Add(s);
                            }
                        }
                    }
                }

                //TaskManager.AddToBus(CreateDatasetTaskmanager.METADATAPACKAGE_IDS, MetadataPackageDic);
            }
            return current;
        }

        private List<StepInfo> GetChildrenSteps(long usageId, Type type, StepInfo parent, string parentXpath, StepModelHelper parentStepModelHelper)
        {
            var childrenSteps = new List<StepInfo>();
            var childrenUsages = metadataStructureUsageHelper.GetCompoundChildrens(usageId, type);
            var stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];

            if (childrenUsages.Count() > 0)
            {
                var xPath = "";
                var complex = false;
                var actionName = "";
                var attrName = "";

                StepInfo s;
                var newStepModelHelper = new StepModelHelper();

                foreach (var u in childrenUsages)
                {
                    //var u = loadUsage(id, type);
                    xPath = parentXpath + "//" + u.Label.Replace(" ", string.Empty) + "[1]";
                    complex = false;
                    actionName = "";
                    attrName = "";

                    if (u is MetadataPackageUsage)
                    {
                        actionName = "SetMetadataPackage";
                    }
                    else
                    {
                        actionName = "SetMetadataCompoundAttribute";

                        if (u is MetadataAttributeUsage)
                        {
                            var mau = (MetadataAttributeUsage)u;
                            if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                            {
                                complex = true;
                                attrName = mau.MetadataAttribute.Self.Name;
                            }
                        }

                        if (u is MetadataNestedAttributeUsage)
                        {
                            var mau = (MetadataNestedAttributeUsage)u;
                            if (mau.Member.Self is MetadataCompoundAttribute)
                            {
                                complex = true;
                                attrName = mau.Member.Self.Name;
                            }
                        }
                    }

                    if (complex)
                    {
                        s = new StepInfo(u.Label)
                        {
                            Id = TaskManager.GenerateStepId(),
                            parentTitle = attrName,
                            Parent = parent,
                            IsInstanze = false,
                            GetActionInfo = new ActionInfo
                            {
                                ActionName = actionName,
                                ControllerName = "CreateSetMetadataPackage",
                                AreaName = "DCM"
                            },

                            PostActionInfo = new ActionInfo
                            {
                                ActionName = actionName,
                                ControllerName = "CreateSetMetadataPackage",
                                AreaName = "DCM"
                            }
                        };

                        if (TaskManager.StepInfos.Where(z => z.Id.Equals(s.Id)).Count() == 0)
                        {
                            newStepModelHelper = new StepModelHelper(s.Id, 1, u.Id, u.Label, GetUsageAttrName(u), u.GetType(), xPath,
                                parentStepModelHelper, u.Extra);
                            stepHelperModelList.Add(newStepModelHelper);

                            s = AddStepsBasedOnUsage(u, s, xPath, newStepModelHelper);
                            childrenSteps.Add(s);
                        }
                    }
                }
            }

            return childrenSteps;
        }

        private List<StepInfo> GetChildrenStepsFromMetadata(long usageId, Type type, StepInfo parent, string parentXpath, StepModelHelper parentStepModelHelper)
        {
            var childrenSteps = new List<StepInfo>();
            var childrenUsages = metadataStructureUsageHelper.GetCompoundChildrens(usageId, type);
            var stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];

            if (childrenUsages.Count > 0)
            {
                foreach (var u in childrenUsages)
                {
                    var number = 1;//childrenUsages.IndexOf(u) + 1;
                    var xPath = parentXpath + "//" + u.Label.Replace(" ", string.Empty) + "[" + number + "]";

                    var complex = false;

                    var actionName = "";
                    var attrName = "";

                    if (u is MetadataPackageUsage)
                    {
                        actionName = "SetMetadataPackage";
                    }
                    else
                    {
                        actionName = "SetMetadataCompoundAttribute";

                        if (u is MetadataAttributeUsage)
                        {
                            var mau = (MetadataAttributeUsage)u;
                            if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                            {
                                complex = true;
                                attrName = mau.MetadataAttribute.Self.Name;
                            }
                        }

                        if (u is MetadataNestedAttributeUsage)
                        {
                            var mau = (MetadataNestedAttributeUsage)u;
                            if (mau.Member.Self is MetadataCompoundAttribute)
                            {
                                complex = true;
                                attrName = mau.Member.Self.Name;
                            }
                        }
                    }

                    if (complex)
                    {
                        var s = new StepInfo(u.Label)
                        {
                            Id = TaskManager.GenerateStepId(),
                            parentTitle = attrName,
                            Parent = parent,
                            IsInstanze = false,
                            GetActionInfo = new ActionInfo
                            {
                                ActionName = actionName,
                                ControllerName = "CreateSetMetadataPackage",
                                AreaName = "DCM"
                            },

                            PostActionInfo = new ActionInfo
                            {
                                ActionName = actionName,
                                ControllerName = "CreateSetMetadataPackage",
                                AreaName = "DCM"
                            }
                        };

                        //only not optional

                        if (TaskManager.StepInfos.Where(z => z.Id.Equals(s.Id)).Count() == 0)
                        {
                            var newStepModelHelper = new StepModelHelper(s.Id, 1, u.Id, u.Label, GetUsageAttrName(u), u.GetType(), xPath, parentStepModelHelper, u.Extra);
                            stepHelperModelList.Add(newStepModelHelper);
                            s = LoadStepsBasedOnUsage(u, s, xPath, newStepModelHelper);
                            childrenSteps.Add(s);
                        }
                    }
                    //}
                }
            }

            return childrenSteps;
        }

        private List<StepInfo> GetChildrenStepsUpdated(long usageId, Type type, StepInfo parent, string parentXpath)
        {
            var childrenSteps = new List<StepInfo>();
            var childrenUsageIds = metadataStructureUsageHelper.GetCompoundChildrens(usageId, type).Select(u => u.Id);
            var stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];

            foreach (var id in childrenUsageIds)
            {
                var u = loadUsage(id, type);
                var label = u.Label.Replace(" ", string.Empty);
                var xPath = parentXpath + "//" + label + "[1]";
                var complex = false;

                var actionName = "";

                if (u is MetadataPackageUsage)
                {
                    actionName = "SetMetadataPackage";
                }
                else
                {
                    actionName = "SetMetadataCompoundAttribute";

                    if (u is MetadataAttributeUsage)
                    {
                        var mau = (MetadataAttributeUsage)u;
                        if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                            complex = true;
                    }

                    if (u is MetadataNestedAttributeUsage)
                    {
                        var mau = (MetadataNestedAttributeUsage)u;
                        if (mau.Member.Self is MetadataCompoundAttribute)
                            complex = true;
                    }
                }

                if (complex)
                {
                    var s = new StepInfo(u.Label)
                    {
                        Id = TaskManager.GenerateStepId(),
                        Parent = parent,
                        IsInstanze = false,
                        //GetActionInfo = new ActionInfo
                        //{
                        //    ActionName = actionName,
                        //    ControllerName = "CreateSetMetadataPackage",
                        //    AreaName = "DCM"
                        //},

                        //PostActionInfo = new ActionInfo
                        //{
                        //    ActionName = actionName,
                        //    ControllerName = "CreateSetMetadataPackage",
                        //    AreaName = "DCM"
                        //}
                    };

                    s.Children = UpdateStepsBasedOnUsage(u, s, xPath).ToList();
                    childrenSteps.Add(s);

                    if (TaskManager.Root.Children.Where(z => z.title.Equals(s.title)).Count() == 0)
                    {
                        var p = GetStepModelhelper(parent.Id, TaskManager);
                        stepHelperModelList.Add(new StepModelHelper(s.Id, 1, u.Id, u.Label, GetUsageAttrName(u), u.GetType(), xPath, p, u.Extra));
                    }
                }
            }

            return childrenSteps;
        }

        private StepInfo LoadStepsBasedOnUsage(BaseUsage usage, StepInfo current, string parentXpath, StepModelHelper parent)
        {
            // genertae action, controller base on usage
            var actionName = "";
            var childName = "";
            var min = usage.MinCardinality;

            if (usage is MetadataPackageUsage)
            {
                actionName = "SetMetadataPackageInstanze";
                childName = ((MetadataPackageUsage)usage).MetadataPackage.Name;
            }
            else
            {
                actionName = "SetMetadataCompoundAttributeInstanze";

                if (usage is MetadataNestedAttributeUsage)
                    childName = ((MetadataNestedAttributeUsage)usage).Member.Name;

                if (usage is MetadataAttributeUsage)
                    childName = ((MetadataAttributeUsage)usage).MetadataAttribute.Name;
            }

            var list = new List<StepInfo>();
            var stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_XML))
            {
                var xMetadata = getMetadata(TaskManager);

                //var x = new XElement("null");
                var parentXElement = new XElement("tmp");

                var keyValueDic = new Dictionary<string, string>();
                keyValueDic.Add("id", usage.Id.ToString());

                if (usage is MetadataPackageUsage)
                {
                    keyValueDic.Add("type", BExIS.Xml.Helpers.XmlNodeType.MetadataPackageUsage.ToString());
                    //elements = XmlUtility.GetXElementsByAttribute(usage.Label, keyValueDic, xMetadata).ToList();
                    parentXElement = XmlUtility.GetXElementByXPath(parent.XPath, xMetadata);
                }
                else
                {
                    keyValueDic.Add("type", BExIS.Xml.Helpers.XmlNodeType.MetadataAttributeUsage.ToString());
                    //elements = XmlUtility.GetXElementsByAttribute(usage.Label, keyValueDic, xMetadata, parentXpath).ToList();
                    parentXElement = XmlUtility.GetXElementByXPath(parent.XPath, xMetadata);
                }

                //foreach (var x in elements)
                //{
                var x = parentXElement;

                if (x != null && !x.Name.Equals("null"))
                {
                    var xelements = x.Elements();

                    if (xelements.Count() > 0)
                    {
                        var counter = 0;

                        XElement last = null;

                        foreach (var element in xelements)
                        {
                            // if the last has not the same name reset count
                            if (last != null && !last.Name.Equals(element.Name))
                            {
                                counter = 0;
                            }

                            last = element;
                            counter++;
                            var title = counter.ToString(); //usage.Label+" (" + counter + ")";
                            var id = Convert.ToInt64((element.Attribute("roleId")).Value.ToString());

                            var s = new StepInfo(title)
                            {
                                Id = TaskManager.GenerateStepId(),
                                Parent = current,
                                IsInstanze = true,
                                HasContent = metadataStructureUsageHelper.HasUsagesWithSimpleType(usage.Id, usage.GetType()),

                                //GetActionInfo = new ActionInfo
                                //{
                                //    ActionName = actionName,
                                //    ControllerName = "CreateSetMetadataPackage",
                                //    AreaName = "DCM"
                                //},

                                //PostActionInfo = new ActionInfo
                                //{
                                //    ActionName = actionName,
                                //    ControllerName = "CreateSetMetadataPackage",
                                //    AreaName = "DCM"
                                //}
                            };

                            var xPath = parentXpath + "//" + childName.Replace(" ", string.Empty) + "[" + counter + "]";

                            if (TaskManager.Root.Children.Where(z => z.title.Equals(title)).Count() == 0)
                            {
                                var newStepModelHelper = new StepModelHelper(s.Id, counter, usage.Id, usage.Label, GetUsageAttrName(usage), usage.GetType(), xPath,
                                    parent, usage.Extra);
                                stepHelperModelList.Add(newStepModelHelper);
                                s.Children = GetChildrenStepsFromMetadata(usage.Id, usage.GetType(), s, xPath, newStepModelHelper);

                                current.Children.Add(s);
                            }
                        }
                    }
                }
                //}

                //TaskManager.AddToBus(CreateDatasetTaskmanager.METADATAPACKAGE_IDS, MetadataPackageDic);
            }
            return current;
        }

        private List<StepInfo> UpdateStepsBasedOnUsage(BaseUsage usage, StepInfo currentSelected, string parentXpath)
        {
            // genertae action, controller base on usage
            var actionName = "";
            var childName = "";
            if (usage is MetadataPackageUsage)
            {
                actionName = "SetMetadataPackageInstanze";
                childName = ((MetadataPackageUsage)usage).MetadataPackage.Name;
            }
            else
            {
                actionName = "SetMetadataCompoundAttributeInstanze";

                if (usage is MetadataNestedAttributeUsage)
                    childName = ((MetadataNestedAttributeUsage)usage).Member.Name;

                if (usage is MetadataAttributeUsage)
                    childName = ((MetadataAttributeUsage)usage).MetadataAttribute.Name;
            }

            var list = new List<StepInfo>();
            var stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_XML))
            {
                var xMetadata = getMetadata(TaskManager);

                var x = XmlUtility.GetXElementByXPath(parentXpath, xMetadata);

                if (x != null && !x.Name.Equals("null"))
                {
                    var current = currentSelected;
                    var xelements = x.Elements();

                    if (xelements.Count() > 0)
                    {
                        var counter = 0;
                        foreach (var element in xelements)
                        {
                            counter++;
                            var title = counter.ToString();

                            if (current.Children.Where(s => s.title.Equals(title)).Count() == 0)
                            {
                                var id = Convert.ToInt64((element.Attribute("roleId")).Value.ToString());

                                var s = new StepInfo(title)
                                {
                                    Id = TaskManager.GenerateStepId(),
                                    Parent = current,
                                    IsInstanze = true,
                                    //GetActionInfo = new ActionInfo
                                    //{
                                    //    ActionName = actionName,
                                    //    ControllerName = "CreateSetMetadataPackage",
                                    //    AreaName = "DCM"
                                    //},

                                    //PostActionInfo = new ActionInfo
                                    //{
                                    //    ActionName = actionName,
                                    //    ControllerName = "CreateSetMetadataPackage",
                                    //    AreaName = "DCM"
                                    //}
                                };

                                var xPath = parentXpath + "//" + childName.Replace(" ", string.Empty) + "[" + counter + "]";

                                s.Children = GetChildrenStepsUpdated(usage.Id, usage.GetType(), s, xPath);
                                list.Add(s);

                                if (TaskManager.Root.Children.Where(z => z.Id.Equals(s.Id)).Count() == 0)
                                {
                                    var parent = GetStepModelhelper(current.Id, TaskManager);
                                    var newStepModelHelper = new StepModelHelper(s.Id, counter, usage.Id, usage.Label, GetUsageAttrName(usage), usage.GetType(), xPath, parent, usage.Extra);

                                    stepHelperModelList.Add(newStepModelHelper);
                                }
                            }//end if
                        }//end foreach
                    }//end if
                }
            }
            return list;
        }

        #endregion Load & Update advanced steps

        #region Helper

        //load Taskamanger
        private CreateTaskmanager loadTaskManager(
            long metadatastructureId,
            long datastructureId,
            long researchplanId,
            XmlDocument metadata,
            string title,
            CreateTaskmanager taskManager,
            ref MetadataEditorModel model
            )
        {
            var stepInfoModelHelpers = new List<StepModelHelper>();

            using (var dm = new DatasetManager())
            using (var rpm = new ResearchPlanManager())
            {
                taskManager.AddToBus(CreateTaskmanager.METADATASTRUCTURE_ID, metadatastructureId);
                if (researchplanId != -1) taskManager.AddToBus(CreateTaskmanager.RESEARCHPLAN_ID, researchplanId);
                if (datastructureId != -1) taskManager.AddToBus(CreateTaskmanager.DATASTRUCTURE_ID, datastructureId);

                if (metadata != null && metadata.DocumentElement != null)
                    taskManager.AddToBus(CreateTaskmanager.METADATA_XML, convertMetadata(metadata));

                taskManager.AddToBus(CreateTaskmanager.ENTITY_TITLE, title);

                taskManager.AddToBus(CreateTaskmanager.RESEARCHPLAN_ID, rpm.Repo.Get().First().Id);

                taskManager = AdvanceTaskManagerBasedOnExistingMetadata(metadatastructureId, taskManager);

                foreach (var stepInfo in taskManager.StepInfos)
                {
                    var stepModelHelper = GetStepModelhelper(stepInfo.Id, taskManager);

                    if (stepModelHelper.Model == null)
                    {
                        if (stepModelHelper.UsageType.Equals(typeof(MetadataPackageUsage)))
                        {
                            stepModelHelper.Model = createPackageModel(stepInfo.Id, false);
                            if (stepModelHelper.Model.StepInfo.IsInstanze)
                                LoadSimpleAttributesForModelFromXml(stepModelHelper, taskManager);
                        }

                        if (stepModelHelper.UsageType.Equals(typeof(MetadataNestedAttributeUsage)))
                        {
                            stepModelHelper.Model = createCompoundModel(stepInfo.Id, false);
                            if (stepModelHelper.Model.StepInfo.IsInstanze)
                                LoadSimpleAttributesForModelFromXml(stepModelHelper, taskManager);
                        }

                        getChildModelsHelper(stepModelHelper, taskManager);
                    }

                    stepInfoModelHelpers.Add(stepModelHelper);
                }

                model.StepModelHelpers = stepInfoModelHelpers;
            }

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_XML))
            {
                if (String.IsNullOrEmpty(title)) title = "No Title available.";

                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_TITLE))
                {
                    if (TaskManager.Bus[CreateTaskmanager.ENTITY_TITLE] != null)
                        model.DatasetTitle = TaskManager.Bus[CreateTaskmanager.ENTITY_TITLE].ToString();
                }
                else
                    model.DatasetTitle = "No Title available.";
            }

            return taskManager;
        }

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
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

        private StepModelHelper AddStepModelhelper(StepModelHelper stepModelHelper)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                ((List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER]).Add(stepModelHelper);

                return stepModelHelper;
            }

            return stepModelHelper;
        }

        private void AdvanceTaskManager(long MetadataStructureId)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            var metadataStructureManager = new MetadataStructureManager();

            try
            {
                var metadataPackageList = metadataStructureManager.GetEffectivePackages(MetadataStructureId).ToList();

                var stepModelHelperList = new List<StepModelHelper>();
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_STEP_MODEL_HELPER))
                {
                    TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
                }
                else
                {
                    TaskManager.Bus.Add(CreateTaskmanager.METADATA_STEP_MODEL_HELPER, stepModelHelperList);
                }

                TaskManager.StepInfos = new List<StepInfo>();

                StepModelHelper stepModelHelper;
                StepInfo si;
                foreach (var mpuId in metadataPackageList.Select(p => p.Id))
                {
                    MetadataPackageUsage mpu = metadataStructureManager.PackageUsageRepo.Get(mpuId);

                    //only add none optional usages
                    si = new StepInfo(mpu.Label)
                    {
                        Id = TaskManager.GenerateStepId(),
                        parentTitle = mpu.MetadataPackage.Name,
                        Parent = TaskManager.Root,
                        IsInstanze = false,
                    };

                    TaskManager.StepInfos.Add(si);
                    stepModelHelper = new StepModelHelper(si.Id, 1, mpu.Id, mpu.Label, GetUsageAttrName(mpu), mpu.GetType(), "Metadata//" + mpu.Label.Replace(" ", string.Empty) + "[1]", null, mpu.Extra);

                    stepModelHelperList.Add(stepModelHelper);

                    si = AddStepsBasedOnUsage(mpu, si, "Metadata//" + mpu.Label.Replace(" ", string.Empty) + "[1]", stepModelHelper);
                    TaskManager.Root.Children.Add(si);

                    //TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
                }

                TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
                Session["CreateDatasetTaskmanager"] = TaskManager;
            }
            finally
            {
                metadataStructureManager.Dispose();
            }
        }

        private CreateTaskmanager AdvanceTaskManagerBasedOnExistingMetadata(long MetadataStructureId, CreateTaskmanager taskManager)
        {
            TaskManager = taskManager;
            var metadataStructureManager = new MetadataStructureManager();
            try
            {
                var metadataPackageList = metadataStructureManager.GetEffectivePackages(MetadataStructureId).ToList();

                var stepModelHelperList = new List<StepModelHelper>();
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_STEP_MODEL_HELPER))
                {
                    TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
                }
                else
                {
                    TaskManager.Bus.Add(CreateTaskmanager.METADATA_STEP_MODEL_HELPER, stepModelHelperList);
                }

                TaskManager.StepInfos = new List<StepInfo>();

                foreach (var mpuId in metadataPackageList.Select(p => p.Id))
                {
                    MetadataPackageUsage mpu = metadataStructureManager.PackageUsageRepo.Get(mpuId);

                    //only add none optional usages
                    var si = new StepInfo(mpu.Label)
                    {
                        Id = TaskManager.GenerateStepId(),
                        parentTitle = mpu.MetadataPackage.Name,
                        Parent = TaskManager.Root,
                        IsInstanze = false,
                    };

                    TaskManager.StepInfos.Add(si);
                    var stepModelHelper = new StepModelHelper(si.Id, 1, mpu.Id, mpu.Label, GetUsageAttrName(mpu), mpu.GetType(), "Metadata//" + mpu.Label.Replace(" ", string.Empty) + "[1]", null, mpu.Extra);

                    stepModelHelperList.Add(stepModelHelper);

                    si = LoadStepsBasedOnUsage(mpu, si, "Metadata//" + mpu.Label.Replace(" ", string.Empty) + "[1]", stepModelHelper);
                    TaskManager.Root.Children.Add(si);

                    TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
                }

                TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
                return TaskManager;
            }
            finally
            {
                metadataStructureManager.Dispose();
            }
        }

        private void CreateXml(CreateTaskmanager taskManager)
        {
            // load metadatastructure with all packages and attributes

            if (taskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
            {
                var xmlMetadatWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

                var metadataXml = xmlMetadatWriter.CreateMetadataXml(Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]));

                //local path
                //string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
                taskManager.AddToBus(CreateTaskmanager.METADATA_XML, metadataXml);

                //setup loaded
                if (taskManager.Bus.ContainsKey(CreateTaskmanager.SETUP_LOADED))
                    taskManager.Bus[CreateTaskmanager.SETUP_LOADED] = true;
                else
                    taskManager.Bus.Add(CreateTaskmanager.SETUP_LOADED, true);
            }
        }

        private StepModelHelper getChildModelsHelper(StepModelHelper stepModelHelper, CreateTaskmanager taskManager)
        {
            StepInfo currentStepInfo = stepModelHelper.Model.StepInfo;

            if (currentStepInfo.Children.Count > 0)
            {
                StepModelHelper childStepModelHelper;

                foreach (var childStep in currentStepInfo.Children)
                {
                    childStepModelHelper = GetStepModelhelper(childStep.Id, taskManager);

                    if (childStepModelHelper.Model == null)
                    {
                        childStepModelHelper.Model = createModel(childStep.Id, false, childStepModelHelper.UsageType);

                        if (childStepModelHelper.Model.StepInfo.IsInstanze)
                            LoadSimpleAttributesForModelFromXml(childStepModelHelper, taskManager);
                    }

                    childStepModelHelper = getChildModelsHelper(childStepModelHelper, taskManager);

                    stepModelHelper.Childrens.Add(childStepModelHelper);
                }
            }

            return stepModelHelper;
        }

        private List<BaseUsage> GetCompoundAttributeUsages(BaseUsage usage)
        {
            var list = new List<BaseUsage>();

            if (usage is MetadataPackageUsage)
            {
                var mpu = (MetadataPackageUsage)usage;

                foreach (var mau in mpu.MetadataPackage.MetadataAttributeUsages)
                {
                    list.AddRange(GetCompoundAttributeUsages(mau));
                }
            }

            if (usage is MetadataAttributeUsage)
            {
                var mau = (MetadataAttributeUsage)usage;

                if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                {
                    list.Add(mau);

                    var mca = (MetadataCompoundAttribute)mau.MetadataAttribute.Self;

                    foreach (var mnau in mca.MetadataNestedAttributeUsages)
                    {
                        list.AddRange(GetCompoundAttributeUsages(mnau));
                    }
                }
            }

            if (usage is MetadataNestedAttributeUsage)
            {
                var mnau = (MetadataNestedAttributeUsage)usage;

                if (mnau.Member.Self is MetadataCompoundAttribute)
                {
                    list.Add(mnau);

                    var mca = (MetadataCompoundAttribute)mnau.Member.Self;

                    foreach (var m in mca.MetadataNestedAttributeUsages)
                    {
                        list.AddRange(GetCompoundAttributeUsages(m));
                    }
                }
            }

            return list;
        }

        private StepModelHelper GetStepModelhelper(int stepId, CreateTaskmanager taskManager)
        {
            TaskManager = taskManager;
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                return ((List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER]).Where(s => s.StepId.Equals(stepId)).FirstOrDefault();
            }

            return null;
        }

        private bool IsImportAvavilable(long metadataStructureId)
        {
            return xmlDatasetHelper.HasExportInformation(metadataStructureId);
        }

        private BaseUsage loadUsage(long Id, Type type)
        {
            if (type.Equals(typeof(MetadataAttributeUsage)))
                return FormHelper.CachedMetadataAttributeUsages().Where(m => m.Id.Equals(Id)).FirstOrDefault();

            if (type.Equals(typeof(MetadataNestedAttributeUsage)))
                return FormHelper.CachedMetadataNestedAttributeUsages().Where(m => m.Id.Equals(Id)).FirstOrDefault();

            if (type.Equals(typeof(MetadataPackageUsage)))
                return FormHelper.CachedMetadataPackageUsages().Where(m => m.Id.Equals(Id)).FirstOrDefault();

            return null;
        }

        private string GetUsageAttrName(BaseUsage usage)
        {
            if (usage.GetType().Equals(typeof(MetadataAttributeUsage)))
            {
                var u = (MetadataAttributeUsage)usage;
                return u.MetadataAttribute.Name;
            }
            if (usage.GetType().Equals(typeof(MetadataNestedAttributeUsage)))
            {
                var u = (MetadataNestedAttributeUsage)usage;
                return u.Member.Name;
            }
            if (usage.GetType().Equals(typeof(MetadataPackageUsage)))
            {
                var u = (MetadataPackageUsage)usage;
                return u.MetadataPackage.Name;
            }

            return null;
        }

        //    return stepModelHelper;
        //}
        private void setStepModelActive(StepModelHelper model)
        {
            model.Activated = true;
            if (model.Parent != null)
                setStepModelActive(model.Parent);
        }

        private void SetXml(XDocument metadataXml)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            // load metadatastructure with all packages and attributes

            if (metadataXml != null)
            {
                // locat path
                //string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");

                TaskManager.AddToBus(CreateTaskmanager.METADATA_XML, metadataXml);

                //setup loaded
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.SETUP_LOADED))
                    TaskManager.Bus[CreateTaskmanager.SETUP_LOADED] = true;
                else
                    TaskManager.Bus.Add(CreateTaskmanager.SETUP_LOADED, true);
            }
        }

        private string storeGeneratedFilePathToContentDiscriptor(long datasetId, DatasetVersion datasetVersion,
            string title, string ext)
        {
            string name = "";
            string mimeType = "";

            if (ext.Contains("csv"))
            {
                name = "datastructure";
                mimeType = "text/comma-separated-values";
            }

            if (ext.Contains("html"))
            {
                name = title;
                mimeType = "application/html";
            }

            // create the generated FileStream and determine its location
            string dynamicPath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId, datasetVersion.Id, title,
                ext);
            //Register the generated data FileStream as a resource of the current dataset version
            //ContentDescriptor generatedDescriptor = new ContentDescriptor()
            //{
            //    OrderNo = 1,
            //    Name = name,
            //    MimeType = mimeType,
            //    URI = dynamicPath,
            //    DatasetVersion = datasetVersion,
            //};

            using (DatasetManager dm = new DatasetManager())
            {
                if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(name)) > 0)
                {
                    // remove the one contentdesciptor
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
                    dm.CreateContentDescriptor(name, mimeType, dynamicPath, 1, datasetVersion);
                }
            }

            return dynamicPath;
        }

        #endregion Helper

        #region Attribute

        /// <summary>
        /// Is called when the user write a letter in Autocomplete User Component
        /// </summary>
        [HttpPost]
        public ActionResult _AutoCompleteAjaxLoading(string text, long id, string type)
        {
            // if mapping with etities exits
            if (MappingUtils.ExistMappingWithEntity(id, LinkElementType.MetadataNestedAttributeUsage))
            {
                var y = new List<MappingEntityResultElement>();

                y = MappingUtils.GetAllMatchesInEntities(id, LinkElementType.MetadataNestedAttributeUsage, text);

                var tmp = y.Select(e => new SelectListItem() { Text = e.Value + " (" + e.EntityId + "_" + e.EntityTypeId + ")", Value = e.EntityTypeId.ToString() });

                tmp.Distinct();

                return new JsonResult
                {
                    Data = tmp,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }

            var x = new List<MappingPartyResultElemenet>();
            // if mapping with parties exits

            switch (type)
            {
                case "MetadataNestedAttributeUsage":
                    {
                        if (MappingUtils.PartyAttrIsMain(id, LinkElementType.MetadataNestedAttributeUsage))
                        {
                            x = MappingUtils.GetAllMatchesInSystem(id, LinkElementType.MetadataNestedAttributeUsage, text);
                        }
                        break;
                    }
                case "MetadataAttributeUsage":
                    {
                        if (MappingUtils.PartyAttrIsMain(id, LinkElementType.MetadataNestedAttributeUsage))
                        {
                            x = MappingUtils.GetAllMatchesInSystem(id, LinkElementType.MetadataNestedAttributeUsage, text);
                        }
                        else if (MappingUtils.PartyAttrIsMain(id, LinkElementType.MetadataAttributeUsage))
                        {
                            x = MappingUtils.GetAllMatchesInSystem(id, LinkElementType.MetadataAttributeUsage, text);
                        }

                        break;
                    }
            }

            // Create text for autocomplete list; order by name; delete dublicates;
            var list = x.Select(e => new SelectListItem() { Text = e.Value + " (" + e.PartyId + ")" }).OrderBy(y => y.Text).GroupBy(i => i.Text).Select(i => i.FirstOrDefault()).ToList();

            // BUG: invalid call to ddm method
            // TODO: mODULARITY ->Call DDM Reindex
            /*
             <Export tag="internalApi" id="freeTextSearch"
                title="Free Text Search" description="free Text Search" icon=""
                controller="SearchIndex" action="Get"
                extends="" />
             */
            //ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            //return new JsonResult { Data = new SelectList(provider.GetTextBoxSearchValues(text, "all", "new", 10).SearchComponent.TextBoxSearchValues, "Value", "Name") };

            // WORKAROUND: return always an empty list
            return new JsonResult
            {
                Data = list,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        private StepModelHelper Down(StepModelHelper stepModelHelperParent, long id, int number)
        {
            var list = stepModelHelperParent.Model.MetadataAttributeModels;

            var temp = list.Where(m => m.Id.Equals(id) && m.Number.Equals(number)).FirstOrDefault();
            var index = list.IndexOf(temp);

            list.RemoveAt(index);
            list.Insert(index + 1, temp);

            return stepModelHelperParent;
        }

        private StepModelHelper DownComplexModel(StepModelHelper stepModelHelperParent, long id, int number)
        {
            var list = stepModelHelperParent.Childrens;

            return stepModelHelperParent;
        }

        /// <summary>
        /// insert at a spezific number in the same children usages
        /// </summary>
        /// <param name="childModel"></param>
        /// <param name="stepModelHelperParent"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private StepModelHelper Insert(MetadataAttributeModel childModel, StepModelHelper stepModelHelperParent, int number)
        {
            var childrensWithSameUsage = stepModelHelperParent.Model.MetadataAttributeModels.Where(m => m.Source.Id.Equals(childModel.Source.Id)).First();
            var indexOfFirstUsage = stepModelHelperParent.Model.MetadataAttributeModels.IndexOf(childrensWithSameUsage);

            stepModelHelperParent.Model.MetadataAttributeModels.Insert(indexOfFirstUsage + number - 1, childModel);

            return stepModelHelperParent;
        }

        private StepModelHelper Up(StepModelHelper stepModelHelperParent, long id, int number)
        {
            var list = stepModelHelperParent.Model.MetadataAttributeModels;

            var temp = list.FirstOrDefault(m => m.Id.Equals(id) && m.Number.Equals(number));
            var index = list.IndexOf(temp);

            list.RemoveAt(index);
            list.Insert(index - 1, temp);

            return stepModelHelperParent;
        }

        private StepModelHelper UpComplexModel(StepModelHelper stepModelHelperParent, long id, int number)
        {
            var list = stepModelHelperParent.Childrens;

            return stepModelHelperParent;
        }

        private StepModelHelper UpdateChildrens(StepModelHelper stepModelHelper)
        {
            var count = stepModelHelper.Model.MetadataAttributeModels.Count;

            for (var i = 0; i < count; i++)
            {
                var child = stepModelHelper.Model.MetadataAttributeModels.ElementAt(i);
                child.NumberOfSourceInPackage = count;
                child.Number = i + 1;
            }

            stepModelHelper.Model.MetadataAttributeModels = UpdateFirstAndLast(stepModelHelper.Model.MetadataAttributeModels);

            return stepModelHelper;
        }

        /// <summary>
        /// Update metadataattributemodels in a parentmodel.
        /// Update number of childrens, based on the usage id
        /// </summary>
        /// <param name="stepModelHelper"></param>
        /// <param name="usageId"></param>
        /// <returns></returns>
        private StepModelHelper UpdateChildrens(StepModelHelper stepModelHelper, long usageId)
        {
            var mams = stepModelHelper.Model.MetadataAttributeModels.Where(m => m.Source.Id.Equals(usageId));

            for (var i = 0; i < mams.Count(); i++)
            {
                var child = mams.ElementAt(i);
                child.NumberOfSourceInPackage = mams.Count();
                child.Number = i + 1;
                child.Parameters.ForEach(p => p.AttributeNumber = child.Number);
            }

            mams = UpdateFirstAndLast(mams.ToList()).AsEnumerable();

            return stepModelHelper;
        }

        #endregion Attribute

        #region Xml

        #region Xml Add / Remove / Update

        private void AddAttributeToXml(BaseUsage parentUsage, int parentNumber, BaseUsage attribute, int number, string parentXPath)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            var metadataXml = getMetadata(TaskManager);

            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
            metadataXml = xmlMetadataWriter.AddAttribute(metadataXml, attribute, number, metadataStructureUsageHelper.GetNameOfType(attribute), metadataStructureUsageHelper.GetIdOfType(attribute).ToString(), parentXPath);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;

            // locat path
            var path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
            //metadataXml.Save
        }

        private void AddCompoundAttributeToXml(BaseUsage usage, int number, string xpath)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            var metadataXml = getMetadata(TaskManager);

            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

            metadataXml = xmlMetadataWriter.AddPackage(metadataXml, usage, number, metadataStructureUsageHelper.GetNameOfType(usage), metadataStructureUsageHelper.GetIdOfType(usage), metadataStructureUsageHelper.GetChildren(usage.Id, usage.GetType()), BExIS.Xml.Helpers.XmlNodeType.MetadataAttribute, BExIS.Xml.Helpers.XmlNodeType.MetadataAttributeUsage, xpath);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;
        }

        private void AddPackageToXml(BaseUsage usage, int number, string xpath)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            var metadataXml = getMetadata(TaskManager);

            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

            metadataXml = xmlMetadataWriter.AddPackage(
                metadataXml,
                usage,
                number,
                metadataStructureUsageHelper.GetNameOfType(usage),
                metadataStructureUsageHelper.GetIdOfType(usage),
                metadataStructureUsageHelper.GetChildren(usage.Id, usage.GetType()),
                BExIS.Xml.Helpers.XmlNodeType.MetadataPackage,
                BExIS.Xml.Helpers.XmlNodeType.MetadataPackageUsage,
                xpath);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;
        }

        private void ChangeInXml(string selectedXPath, string destinationXPath)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            var metadataXml = getMetadata(TaskManager);
            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

            metadataXml = xmlMetadataWriter.Change(metadataXml, selectedXPath, destinationXPath);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;
            // locat path
            var path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
            metadataXml.Save(path);
        }

        private void RemoveAttributeToXml(int packageNumber, BaseUsage attribute, int number, string metadataAttributeName, string parentXPath)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            var metadataXml = getMetadata(TaskManager);
            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

            metadataXml = xmlMetadataWriter.RemoveAttribute(metadataXml, attribute, number, metadataAttributeName, parentXPath);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;
            // locat path
            //string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
            //metadataXml.Save
        }

        private void RemoveCompoundAttributeToXml(BaseUsage usage, int number)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            var metadataXml = getMetadata(TaskManager);

            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
            metadataXml = xmlMetadataWriter.RemovePackage(metadataXml, usage, number, metadataStructureUsageHelper.GetNameOfType(usage));

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;
        }

        private void RemoveFromXml(string xpath)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            var metadataXml = getMetadata(TaskManager);

            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
            metadataXml = xmlMetadataWriter.Remove(metadataXml, xpath);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;
        }

        private void ClearXml(string xpath)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            var metadataXml = getMetadata(TaskManager);

            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
            metadataXml = xmlMetadataWriter.Clean(metadataXml, xpath);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;
        }

        private void UpdateAttribute(BaseUsage parentUsage, int packageNumber, BaseUsage attribute, int number, object value, string parentXpath)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            var metadataXml = getMetadata(TaskManager);
            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

            metadataXml = xmlMetadataWriter.Update(metadataXml, attribute, number, value, metadataStructureUsageHelper.GetNameOfType(attribute), parentXpath);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;
            // locat path
            var path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
            metadataXml.Save(path);
        }

        private void UpdateParameter(BaseUsage attribute, int attrNumber, object value, string parentXpath, KeyValuePair<string, string> parameter)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            var metadataXml = getMetadata(TaskManager);
            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(parameter.Key, parameter.Value);

            metadataXml = xmlMetadataWriter.Update(metadataXml, attribute, attrNumber, null, metadataStructureUsageHelper.GetNameOfType(attribute), parentXpath, parameters);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;
            // locat path
            var path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
            metadataXml.Save(path);
        }

        private void UpdateAttribute(BaseUsage parentUsage, int packageNumber, BaseUsage attribute, int number, object value, string parentXpath, Dictionary<string, string> xmlAttrs)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            var metadataXml = getMetadata(TaskManager);
            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

            metadataXml = xmlMetadataWriter.Update(metadataXml, attribute, number, value, metadataStructureUsageHelper.GetNameOfType(attribute), parentXpath, xmlAttrs);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;
            // locat path
            var path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
            metadataXml.Save(path);
        }

        //ToDo really said function, but cant find a other solution for now
        private void AddXmlAttribute(string xpath, string attrName, string attrValue)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            XDocument metadataXml = getMetadata(TaskManager);

            XmlDocument xmlDocument = XmlUtility.ToXmlDocument(metadataXml);

            XmlNode tmp = xmlDocument.SelectSingleNode(xpath);

            XmlAttribute xmlAttr = xmlDocument.CreateAttribute(attrName);
            xmlAttr.Value = attrValue;

            tmp.Attributes.Append(xmlAttr);

            metadataXml = XmlUtility.ToXDocument(xmlDocument);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;

            // locat path
            var path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
            metadataXml.Save(path);
        }

        private void UpdateCompoundAttributeToXml(BaseUsage usage, int number, string xpath)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            var metadataXml = getMetadata(TaskManager);

            var xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

            metadataXml = xmlMetadataWriter.AddPackage(metadataXml, usage, number,
                metadataStructureUsageHelper.GetNameOfType(usage),
                metadataStructureUsageHelper.GetIdOfType(usage),
                metadataStructureUsageHelper.GetChildren(usage.Id, usage.GetType()),
                BExIS.Xml.Helpers.XmlNodeType.MetadataAttribute,
                BExIS.Xml.Helpers.XmlNodeType.MetadataAttributeUsage, xpath);

            TaskManager.Bus[CreateTaskmanager.METADATA_XML] = metadataXml;
        }

        #endregion Xml Add / Remove / Update

        #region Xml Helper

        private XDocument getMetadata(CreateTaskmanager taskManager)
        {
            try
            {
                if (taskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_XML))
                {
                    var metadata = taskManager.Bus[CreateTaskmanager.METADATA_XML];

                    if (metadata is XDocument) return (XDocument)metadata;
                    else
                    {
                        if (metadata is XmlDocument)
                        {
                            return XmlUtility.ToXDocument((XmlDocument)metadata);
                        }
                    }
                }

                return new XDocument();
            }
            catch
            {
                return new XDocument();
            }
        }

        private XDocument convertMetadata(object metadata)
        {
            try
            {
                if (metadata is XDocument) return (XDocument)metadata;
                else
                {
                    if (metadata is XmlDocument)
                    {
                        return XmlUtility.ToXDocument((XmlDocument)metadata);
                    }
                }

                return new XDocument();
            }
            catch
            {
                return new XDocument();
            }
        }

        #endregion Xml Helper

        #endregion Xml

        #region Validation

        public ActionResult Validate()
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager != null && TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                var stepInfoModelHelpers = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];
                ValidateModels(stepInfoModelHelpers.Where(s => s.Activated && s.IsParentActive()).ToList());
            }

            return RedirectToAction("ReloadMetadataEditor", new
            {
                fromEditMode = true,
            });
        }

        //XX number of index des values nötig
        [HttpPost]
        public ActionResult ValidateMetadataAttributeUsage(string value, int id, int parentid, string parentname, int number, int parentModelNumber, int parentStepId)
        {
            //delete all white spaces from start and end
            value = value.Trim();

            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            var stepModelHelper = GetStepModelhelper(parentStepId, TaskManager);

            var ParentUsageId = stepModelHelper.UsageId;
            var parentUsage = loadUsage(stepModelHelper.UsageId, stepModelHelper.UsageType);
            var pNumber = stepModelHelper.Number;

            metadataStructureUsageHelper = new MetadataStructureUsageHelper();

            var metadataAttributeUsage = metadataStructureUsageHelper.GetChildren(parentUsage.Id, parentUsage.GetType()).Where(u => u.Id.Equals(id)).FirstOrDefault();

            //Path.Combine(AppConfiguration.GetModuleWorkspacePath("dcm"),"x","file.xml");

            //UpdateXml
            var metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);
            var model = FormHelper.CreateMetadataAttributeModel(metadataAttributeUsage, parentUsage, metadataStructureId, parentModelNumber, stepModelHelper.StepId);

            //check if datatype is a datetime then check display pattern and manipulate the incoming string
            if (model.SystemType.Equals(typeof(DateTime).Name))
            {
                if (!string.IsNullOrEmpty(model.DisplayPattern))
                {
                    var dt = DateTime.Parse(value);
                    value = dt.ToString(model.DisplayPattern);
                }
            }

            model.Value = value;
            model.Number = number;

            UpdateAttribute(parentUsage, parentModelNumber, metadataAttributeUsage, number, value, stepModelHelper.XPath);
            // update model

            ViewData["Xpath"] = stepModelHelper.XPath; // set Xpath for idbyxapth
                                                       // read temp metadata XML

            var metadata = getMetadata(TaskManager); // new getMetadata(TaskManager)

            if (stepModelHelper.Model.MetadataAttributeModels.Where(a => a.Id.Equals(id) && a.Number.Equals(number)).Count() > 0)
            {
                var selectedMetadatAttributeModel = stepModelHelper.Model.MetadataAttributeModels.Where(a => a.Id.Equals(id) && a.Number.Equals(number)).FirstOrDefault();
                // select the attributeModel and change the value
                selectedMetadatAttributeModel.Value = model.Value;
                selectedMetadatAttributeModel.Errors = validateAttribute(selectedMetadatAttributeModel);

                // get xpath for element at position x (number)
                var xpath = stepModelHelper.GetXPathFromSimpleAttribute(selectedMetadatAttributeModel.Id, number);
                // get simple element based on xpath
                var simpleElement = XmlUtility.GetXElementByXPath(xpath, metadata);

                // if this simple attr is linked to a party, add partyid to Model
                if (simpleElement.Attributes().Any(a => a.Name.LocalName.ToLowerInvariant().Equals("partyid")))
                {
                    long partyid = 0;
                    string partyidAsString = simpleElement.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLowerInvariant().Equals("partyid"))?.Value;

                    if (Int64.TryParse(partyidAsString, out partyid))
                    {
                        selectedMetadatAttributeModel.PartyId = partyid;
                    }
                }

                // reload model based on xelement value and existing attributes
                selectedMetadatAttributeModel.Update(simpleElement);

                Session["CreateDatasetTaskmanager"] = TaskManager;

                return PartialView("_metadataAttributeView", selectedMetadatAttributeModel);
            }
            else
            {
                var xpath = stepModelHelper.GetXPathFromSimpleAttribute(model.Id, number);
                // get simple element based on xpath
                var simpleElement = XmlUtility.GetXElementByXPath(xpath, metadata);

                model.Update(simpleElement);

                stepModelHelper.Model.MetadataAttributeModels.Add(model);

                return PartialView("_metadataAttributeView", model);
            }
        }

        [HttpPost]
        public ActionResult ValidateMetadataParameterUsage(string value, int id, long attrUsageId, int number, int parentModelNumber, int parentStepId)
        {
            //delete all white spaces from start and end
            value = value.Trim();

            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            var stepModelHelper = GetStepModelhelper(parentStepId, TaskManager);

            var ParentUsageId = stepModelHelper.UsageId;
            var parentUsage = loadUsage(stepModelHelper.UsageId, stepModelHelper.UsageType);
            var pNumber = stepModelHelper.Number;

            metadataStructureUsageHelper = new MetadataStructureUsageHelper();
            var metadataAttributeUsage = metadataStructureUsageHelper.GetChildren(parentUsage.Id, parentUsage.GetType()).Where(u => u.Id.Equals(attrUsageId)).FirstOrDefault();

            Type type = metadataAttributeUsage == null ? parentUsage.GetType() : metadataAttributeUsage.GetType();
            var destinationUsage = metadataAttributeUsage == null ? parentUsage : metadataAttributeUsage;

            var metadataParameterUsage = metadataStructureUsageHelper.GetParameters(attrUsageId, type).FirstOrDefault(p => p.Id.Equals(id));

            //UpdateXml
            var metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);
            var model = FormHelper.CreateMetadataParameterModel(metadataParameterUsage as BaseUsage, destinationUsage, metadataStructureId, parentModelNumber, parentStepId);

            //check if datatype is a datetime then check display pattern and manipulate the incoming string
            if (model.SystemType.Equals(typeof(DateTime).Name))
            {
                if (!string.IsNullOrEmpty(model.DisplayPattern))
                {
                    var dt = DateTime.Parse(value);
                    value = dt.ToString(model.DisplayPattern);
                }
            }

            model.Value = value;
            model.AttributeNumber = number;
            model.Errors = validateParameter(model);

            //create para
            KeyValuePair<string, string> parameter = new KeyValuePair<string, string>(metadataParameterUsage.Label, value);
            UpdateParameter(destinationUsage, number, value, stepModelHelper.XPath, parameter);

            ViewData["Xpath"] = stepModelHelper.XPath; // set Xpath for idbyxapth

            // store in stephelper
            if (metadataAttributeUsage != null)
            {
                if (stepModelHelper.Model.MetadataAttributeModels.Any()) // check if metadata Attribute models exist
                {
                    var metadataAttributeModel = stepModelHelper.Model.MetadataAttributeModels.Where(m => m.Source.Id.Equals(attrUsageId) && m.Number.Equals((long)number)).FirstOrDefault();// get metadata attribute model for this parameter
                    if (metadataAttributeModel != null)
                    {
                        if (metadataAttributeModel.Parameters.Any())
                        {
                            // get stored parameter model and replace it
                            var storedParameterModel = metadataAttributeModel.Parameters.Where(p => p.Id.Equals(model.Id)).FirstOrDefault();
                            storedParameterModel.Value = model.Value;
                            storedParameterModel.Errors = validateParameter(model);
                        }
                    }
                }
            }
            else
            {
                if (stepModelHelper.Model.MetadataParameterModels.Any()) // check if metadata Attribute models exist
                {
                    var parameters = stepModelHelper.Model.MetadataParameterModels;

                    if (parameters.Any())
                    {
                        // get stored parameter model and replace it
                        var storedParameterModel = parameters.Where(p => p.Id.Equals(model.Id)).FirstOrDefault();
                        storedParameterModel.Value = model.Value;
                        storedParameterModel.Errors = validateParameter(model);
                    }
                }
            }

            return PartialView("_metadataParameterView", model);
        }

        private List<Error> validateAttribute(MetadataAttributeModel aModel)
        {
            var errors = new List<Error>();
            //optional check
            if (aModel.MinCardinality > 0 && (aModel.Value == null || String.IsNullOrEmpty(aModel.Value.ToString())))
                errors.Add(new Error(ErrorType.MetadataAttribute, "is required", new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));
            else
                if (aModel.MinCardinality > 0 && String.IsNullOrEmpty(aModel.Value.ToString()))
                errors.Add(new Error(ErrorType.MetadataAttribute, "is required", new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));

            //check datatype
            if (aModel.Value != null && !String.IsNullOrEmpty(aModel.Value.ToString()))
            {
                if (!DataTypeUtility.IsTypeOf(aModel.Value, aModel.SystemType))
                {
                    errors.Add(new Error(ErrorType.MetadataAttribute, "Value can´t convert to the type: " + aModel.SystemType + ".", new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));
                }
                else
                {
                    var type = Type.GetType("System." + aModel.SystemType);
                    var value = Convert.ChangeType(aModel.Value, type);

                    // check Constraints
                    foreach (var constraint in aModel.GetMetadataAttribute().Constraints)
                    {
                        if (value != null && !constraint.IsSatisfied(value))
                        {
                            errors.Add(new Error(ErrorType.MetadataAttribute, constraint.ErrorMessage, new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));
                        }
                    }
                }
            }

            // check parameters
            if (aModel.Parameters.Any())
            {
                foreach (var p in aModel.Parameters)
                {
                    p.Errors = validateParameter(p);
                }
            }

            if (errors.Count == 0)
                return null;
            else
                return errors;
        }

        private List<Error> validateParameter(MetadataParameterModel aModel)
        {
            var errors = new List<Error>();
            if (aModel != null)
            {
                //optional check
                if (aModel.MinCardinality > 0 && (aModel.Value == null || String.IsNullOrEmpty(aModel.Value.ToString())))
                    errors.Add(new Error(ErrorType.MetadataAttribute, "is required", new object[] { aModel.DisplayName, aModel.Value, aModel.AttributeNumber, aModel.ParentModelNumber, aModel.Parent.Label }));
                else
                    if (aModel.MinCardinality > 0 && String.IsNullOrEmpty(aModel.Value.ToString()))
                    errors.Add(new Error(ErrorType.MetadataAttribute, "is required", new object[] { aModel.DisplayName, aModel.Value, aModel.AttributeNumber, aModel.ParentModelNumber, aModel.Parent.Label }));

                //check datatype
                if (aModel.Value != null && !String.IsNullOrEmpty(aModel.Value.ToString()))
                {
                    if (!DataTypeUtility.IsTypeOf(aModel.Value, aModel.SystemType))
                    {
                        errors.Add(new Error(ErrorType.MetadataAttribute, "Value can´t convert to the type: " + aModel.SystemType + ".", new object[] { aModel.DisplayName, aModel.Value, aModel.AttributeNumber, aModel.ParentModelNumber, aModel.Parent.Label }));
                    }
                    else
                    {
                        var type = Type.GetType("System." + aModel.SystemType);
                        var value = Convert.ChangeType(aModel.Value, type);

                        // check Constraints
                        foreach (var constraint in aModel.GetMetadataParameter().Constraints)
                        {
                            if (value != null && !constraint.IsSatisfied(value))
                            {
                                errors.Add(new Error(ErrorType.MetadataAttribute, constraint.ErrorMessage, new object[] { aModel.DisplayName, aModel.Value, aModel.AttributeNumber, aModel.ParentModelNumber, aModel.Parent.Label }));
                            }
                        }
                    }
                }
            }

            return errors;
        }

        private void ValidateModels(List<StepModelHelper> stepModelHelpers)
        {
            List<Error> tmp = new List<Error>();

            foreach (var stepModelHelper in stepModelHelpers)
            {
                //only check if step is a instance
                if (stepModelHelper.Model.StepInfo.IsInstanze)
                {
                    // if model exist then validate attributes
                    if (stepModelHelper.Model != null)
                    {
                        foreach (var metadataAttrModel in stepModelHelper.Model.MetadataAttributeModels)
                        {
                            metadataAttrModel.Errors = validateAttribute(metadataAttrModel);

                            if (metadataAttrModel.Errors != null) tmp.AddRange(metadataAttrModel.Errors);

                            //if (metadataAttrModel.Errors.Count > 0)
                            //    step.stepStatus = StepStatus.error;
                        }

                        foreach (var metadataParameterModel in stepModelHelper.Model.MetadataParameterModels)
                        {
                            if (metadataParameterModel != null)
                            {
                                metadataParameterModel.Errors = validateParameter(metadataParameterModel);

                                if (metadataParameterModel.Errors != null) tmp.AddRange(metadataParameterModel.Errors);
                            }
                            //if (metadataAttrModel.Errors.Count > 0)
                            //    step.stepStatus = StepStatus.error;
                        }
                    }
                    // else check for required elements
                    else
                    {
                        if (metadataStructureUsageHelper.HasUsagesWithSimpleType(stepModelHelper.UsageId, stepModelHelper.UsageType))
                        {
                            //foreach (var metadataAttrModel in stepModeHelper.Model.MetadataAttributeModels)
                            //{
                            //    metadataAttrModel.Errors = validateAttribute(metadataAttrModel);
                            //    if (metadataAttrModel.Errors.Count>0)
                            //        step.stepStatus = StepStatus.error;
                            //}

                            //if(MetadataStructureUsageHelper.HasRequiredSimpleTypes(stepModeHelper.Usage))
                            //{
                            //    StepInfo step = TaskManager.Get(stepModeHelper.StepId);
                            //    if (step != null && step.IsInstanze)
                            //    {
                            //        Error error = new Error(ErrorType.Other, String.Format("{0} : {1} {2}", "Step: ", stepModeHelper.Usage.Label, "is not valid. There are fields that are required and not yet completed are."));

                            //        errors.Add(new Tuple<StepInfo, List<Error>>(step, tempErrors));

                            //        step.stepStatus = StepStatus.error;
                            //    }
                            //}
                        }
                    }
                }
            }
        }

        private List<Error> validateStep(AbstractMetadataStepModel pModel)
        {
            var errorList = new List<Error>();

            if (pModel != null)
            {
                foreach (var m in pModel.MetadataAttributeModels)
                {
                    var temp = validateAttribute(m);
                    if (temp != null)
                        errorList.AddRange(temp);
                }

                foreach (var m in pModel.MetadataParameterModels)
                {
                    var temp = validateParameter(m);
                    if (temp != null)
                        errorList.AddRange(temp);
                }
            }

            if (errorList.Count == 0)
                return null;
            else
                return errorList;
        }

        #endregion Validation

        #region Bus Functions

        //private BaseUsage GetMetadataCompoundAttributeUsage(long id)
        //{
        //    //return MetadataStructureUsageHelper.GetChildrenUsageById(Id);
        //    return metadataStructureUsageHelper.GetMetadataAttributeUsageById(id);
        //}

        //private BaseUsage GetPackageUsage(long Id)
        //{
        //    var mpm = new MetadataStructureManager();
        //    return mpm.PackageUsageRepo.Get(Id);
        //}

        private long GetUsageId(int stepId)
        {
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                var list = (List<StepModelHelper>)TaskManager.Bus[CreateTaskmanager.METADATA_STEP_MODEL_HELPER];
                return list.Where(s => s.StepId.Equals(stepId)).FirstOrDefault().UsageId;
            }

            return 0;
        }

        private List<MetadataAttributeModel> UpdateFirstAndLast(List<MetadataAttributeModel> list)
        {
            foreach (var x in list)
            {
                if (list.First().Equals(x)) x.first = true;
                else x.first = false;

                if (list.Last().Equals(x)) x.last = true;
                else x.last = false;
            }

            return list;
        }

        #endregion Bus Functions

        #region Models

        private AbstractMetadataStepModel createModel(int stepId, bool validateIt, Type usageType)
        {
            if (usageType.Equals(typeof(MetadataPackageUsage)))
            {
                return createPackageModel(stepId, validateIt);
            }

            return createCompoundModel(stepId, validateIt);
        }

        private MetadataCompoundAttributeModel createCompoundModel(int stepId, bool validateIt)
        {
            var stepInfo = TaskManager.Get(stepId);
            var stepModelHelper = GetStepModelhelper(stepId, TaskManager);

            var metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);
            var Id = stepModelHelper.UsageId;

            BaseUsage usage = loadUsage(stepModelHelper.UsageId, stepModelHelper.UsageType);
            var model = FormHelper.CreateMetadataCompoundAttributeModel(usage, stepModelHelper.Number);

            // get children
            model.ConvertMetadataAttributeModels(usage, metadataStructureId, stepInfo.Id);
            model.ConvertMetadataParameterModels(usage, metadataStructureId, stepInfo.Id);
            model.StepInfo = TaskManager.Get(stepId);

            if (stepInfo.IsInstanze == false)
            {
                //get Instance
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_XML))
                {
                    var xMetadata = getMetadata(TaskManager);
                    model.ConvertInstance(xMetadata, stepModelHelper.XPath);
                }
            }
            else
            {
                if (stepModelHelper.Model != null)
                {
                    model = (MetadataCompoundAttributeModel)stepModelHelper.Model;
                }
                else
                {
                    stepModelHelper.Model = model;
                }
            }

            model.StepInfo = stepInfo;

            return model;
        }

        private MetadataPackageModel createPackageModel(int stepId, bool validateIt)
        {
            using (var mdsManager = new MetadataStructureManager())
            using (var mdpManager = new MetadataPackageManager())
            {
                var stepInfo = TaskManager.Get(stepId);
                var stepModelHelper = GetStepModelhelper(stepId, TaskManager);

                var metadataPackageId = stepModelHelper.UsageId;
                var metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);

                var mpu = (MetadataPackageUsage)loadUsage(stepModelHelper.UsageId, stepModelHelper.UsageType);
                var model = new MetadataPackageModel();

                model = FormHelper.CreateMetadataPackageModel(mpu, stepModelHelper.Number);
                model.ConvertMetadataAttributeModels(mpu, metadataStructureId, stepId);
                model.ConvertMetadataParameterModels(mpu, metadataStructureId, stepId);

                if (stepInfo.IsInstanze == false)
                {
                    //get Instance
                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_XML))
                    {
                        var xMetadata = getMetadata(TaskManager);
                        model.ConvertInstance(xMetadata, stepModelHelper.XPath);
                    }
                }
                else
                {
                    if (stepModelHelper.Model != null)
                    {
                        model = (MetadataPackageModel)stepModelHelper.Model;
                    }
                    else
                    {
                        stepModelHelper.Model = model;
                    }
                }

                model.StepInfo = stepInfo;

                return model;
            }
        }

        /// <summary>
        /// load for the complex model all simple attribute models from the global set metadata xml
        ///
        /// </summary>
        /// <param name="stepModelHelper"></param>
        /// <returns></returns>
        private AbstractMetadataStepModel LoadSimpleAttributesForModelFromXml(StepModelHelper stepModelHelper, CreateTaskmanager TaskManager)
        {
            var metadata = getMetadata(TaskManager);

            // load complex xml element from the metadata xml based on the xpath of the stepmodelhelper
            // the stepmodel helper has the model for the view part as also the path to the xml where it belongs to
            var complexElement = XmlUtility.GetXElementByXPath(stepModelHelper.XPath, metadata);

            var additionalyMetadataAttributeModel = new List<MetadataAttributeModel>();
            long parentPartyId = 0;
            // if the complex xml element has a partyid its mapped and all dependend simmple attributes must set
            bool complexIsMapped = false;
            if (complexElement.Attributes().Any(a => a.Name.LocalName.ToLowerInvariant().Equals("partyid")))
            {
                complexIsMapped = true;

                string partyidAsString = complexElement.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLowerInvariant().Equals("partyid"))?.Value;

                if (Int64.TryParse(partyidAsString, out parentPartyId))
                {
                    stepModelHelper.Model.PartyId = parentPartyId;
                }
            }

            // go throw each metadata attribute from the complex type and load them
            foreach (var simpleMetadataAttributeModel in stepModelHelper.Model.MetadataAttributeModels)
            {
                // in one complex object there is a chance that simple attributes can exist more then ones
                // get the count (numberOfSMM) to go throw all of them
                var numberOfSMM = 1;
                if (complexElement != null)
                {
                    //Debug.WriteLine("XXXXXXXXXXXXXXXXXXXX");
                    //Debug.WriteLine(simpleMetadataAttributeModel.Source.Label);
                    var childs = XmlUtility.GetChildren(complexElement).Where(e => e.Attribute("id").Value.Equals(simpleMetadataAttributeModel.Id.ToString()));

                    if (childs.Any())
                        numberOfSMM = childs.First().Elements().Count();
                }

                // go throw each count of one simple attribute
                for (var i = 1; i <= numberOfSMM; i++)
                {
                    // get the xpath of a simple attribute with index i
                    var xpath = stepModelHelper.GetXPathFromSimpleAttribute(simpleMetadataAttributeModel.Id, i);
                    //load the simple element based on the given xpath to fill the metadata attribute model
                    var simpleElement = XmlUtility.GetXElementByXPath(xpath, metadata);

                    // the first simple element model exist as default by loading the structure, if there are more in the xml
                    // they can be copied from the frist and replace the values
                    if (i == 1)
                    {
                        // lock all attributs in a complex mapping except the main attribute
                        if (simpleMetadataAttributeModel.PartyMappingExist && simpleMetadataAttributeModel.PartyComplexMappingExist)
                        {
                            simpleMetadataAttributeModel.MappingSelectionField = MappingUtils.PartyAttrIsMain(simpleMetadataAttributeModel.Id, LinkElementType.MetadataNestedAttributeUsage);

                            if (complexIsMapped && !simpleMetadataAttributeModel.Locked)
                                simpleMetadataAttributeModel.Locked = !simpleMetadataAttributeModel.MappingSelectionField;
                        }

                        if (simpleElement != null)
                        {
                            simpleMetadataAttributeModel.Update(simpleElement);

                            #region entity & Party mapping

                            // if this simple attr is linked to a enity, some attr need to get from the xelement and create a url for the model
                            if (simpleElement.Attributes().Any(a => a.Name.LocalName.ToLowerInvariant().Equals("entityid")))
                            {
                                long id = 0;
                                int version = 0;
                                string idAsString = simpleElement.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLowerInvariant().Equals("entityid"))?.Value;
                                string versionAsString = simpleElement.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLowerInvariant().Equals("entityversion"))?.Value;

                                if (Int64.TryParse(idAsString, out id) &&
                                    Int32.TryParse(versionAsString, out version))
                                {
                                    string server = this.Request.Url.GetLeftPart(UriPartial.Authority);
                                    string url = server + "/DDM/Data/Show/" + id.ToString() + "?version=" + version;

                                    simpleMetadataAttributeModel.EntityUrl = url;
                                }
                            }

                            // if this simple attr is linked to a party, add partyid to Model
                            if (simpleElement.Attributes().Any(a => a.Name.LocalName.ToLowerInvariant().Equals("partyid")))
                            {
                                long partyid = 0;
                                string partyidAsString = simpleElement.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLowerInvariant().Equals("partyid"))?.Value;

                                if (Int64.TryParse(partyidAsString, out partyid))
                                {
                                    simpleMetadataAttributeModel.PartyId = partyid;
                                }
                            }

                            // if parent is mapped and has a party id, set it to the simple metadata model
                            if (complexIsMapped)
                            {
                                simpleMetadataAttributeModel.ParentPartyId = parentPartyId;
                            }

                            #endregion entity & Party mapping

                            // if at least on item has a value, the parent should be activated
                            if (!string.IsNullOrEmpty(simpleMetadataAttributeModel.Value.ToString())) setStepModelActive(stepModelHelper);
                        }
                    }
                    else
                    {
                        var newMetadataAttributeModel = simpleMetadataAttributeModel.Copy(i, numberOfSMM);
                        newMetadataAttributeModel.Update(simpleElement);

                        // if this simple attr is linked to a party, add partyid to Model
                        if (simpleElement.Attributes().Any(a => a.Name.LocalName.ToLowerInvariant().Equals("partyid")))
                        {
                            long partyid = 0;
                            string partyidAsString = simpleElement.Attributes().FirstOrDefault(a => a.Name.LocalName.ToLowerInvariant().Equals("partyid"))?.Value;

                            if (Int64.TryParse(partyidAsString, out partyid))
                            {
                                newMetadataAttributeModel.PartyId = partyid;
                            }
                        }

                        // if parent has a party id, set it to the simple attr
                        if (complexIsMapped)
                        {
                            simpleMetadataAttributeModel.ParentPartyId = parentPartyId;
                        }

                        if (i == numberOfSMM) newMetadataAttributeModel.last = true;
                        additionalyMetadataAttributeModel.Add(newMetadataAttributeModel);
                    }
                }
            }

            foreach (var item in additionalyMetadataAttributeModel)
            {
                var tempList = stepModelHelper.Model.MetadataAttributeModels;

                var indexOfLastSameAttribute = tempList.IndexOf(tempList.Where(a => a.Id.Equals(item.Id)).Last());
                tempList.Insert(indexOfLastSameAttribute + 1, item);
            }

            // add parameters
            foreach (var item in stepModelHelper.Model.MetadataParameterModels)
            {
                if (item != null && complexElement != null && complexElement.HasAttributes)
                {
                    item.Value = complexElement.Attribute(item.DisplayName) !=null? complexElement.Attribute(item.DisplayName).Value.ToString():"";
                }
            }

            return stepModelHelper.Model;
        }

        #endregion Models

        #region Security

        /// <summary>
        /// return true if user has edit rights
        /// </summary>
        /// <returns></returns>
        private bool hasUserEditAccessRights(long entityId)
        {
            FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager();

            try
            {
                return featurePermissionManager.HasAccess<User>(GetUsernameOrDefault(), "DCM", "CreateDataset", "*");
            }
            finally
            {
                featurePermissionManager.Dispose();
            }
        }

        /// <summary>
        /// return true if user has edit rights
        /// </summary>
        /// <returns></returns>
        private bool hasUserEditRights(long entityId)
        {
            #region security permissions and authorisations check

            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

            try
            {
                return entityPermissionManager.HasEffectiveRightsAsync(GetUsernameOrDefault(), typeof(Dataset), entityId, RightType.Write).Result;
            }
            finally
            {
                entityPermissionManager.Dispose();
            }

            #endregion security permissions and authorisations check
        }

        #endregion Security

        #region overrideable Action

        private void setDefaultAdditionalFunctions(CreateTaskmanager taskManager)
        {
            //set function actions of COPY, RESET,CANCEL,SUBMIT
            ActionInfo copyAction = new ActionInfo();
            copyAction.ActionName = "Copy";
            copyAction.ControllerName = "CreateDataset";
            copyAction.AreaName = "DCM";

            ActionInfo resetAction = new ActionInfo();
            resetAction.ActionName = "Reset";
            resetAction.ControllerName = "Form";
            resetAction.AreaName = "DCM";

            ActionInfo cancelAction = new ActionInfo();
            cancelAction.ActionName = "Cancel";
            cancelAction.ControllerName = "Form";
            cancelAction.AreaName = "DCM";

            ActionInfo submitAction = new ActionInfo();
            submitAction.ActionName = "Submit";
            submitAction.ControllerName = "CreateDataset";
            submitAction.AreaName = "DCM";

            taskManager.Actions.Add(CreateTaskmanager.CANCEL_ACTION, cancelAction);
            taskManager.Actions.Add(CreateTaskmanager.COPY_ACTION, copyAction);
            taskManager.Actions.Add(CreateTaskmanager.RESET_ACTION, resetAction);
            taskManager.Actions.Add(CreateTaskmanager.SUBMIT_ACTION, submitAction);
        }

        /// <summary>
        /// Set a action in the Form
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="area"></param>
        /// <param name="type">submit,cancel,reset,copy</param>
        public ActionResult SetAdditionalFunctions(string actionName, string controllerName, string area, string type)
        {
            try
            {
                CreateTaskmanager TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"] ??
                                                new CreateTaskmanager();

                ActionInfo action = new ActionInfo();
                action.ActionName = actionName;
                action.ControllerName = controllerName;
                action.AreaName = area;
                int x = 5;

                x = x >= 5 ? x++ : x--;

                switch (type.ToLower())
                {
                    case "submit":
                        {
                            if (TaskManager.Actions.ContainsKey(CreateTaskmanager.SUBMIT_ACTION))
                                TaskManager.Actions[CreateTaskmanager.SUBMIT_ACTION] = action;
                            else
                                TaskManager.Actions.Add(CreateTaskmanager.SUBMIT_ACTION, action);

                            //TaskManager.Actions.ContainsKey(CreateTaskmanager.SUBMIT_ACTION) ? TaskManager.Actions[CreateTaskmanager.SUBMIT_ACTION] = action : TaskManager.Actions.Add(CreateTaskmanager.SUBMIT_ACTION, action);

                            break;
                        }
                    case "reset":
                        {
                            if (TaskManager.Actions.ContainsKey(CreateTaskmanager.RESET_ACTION))
                                TaskManager.Actions[CreateTaskmanager.RESET_ACTION] = action;
                            else
                                TaskManager.Actions.Add(CreateTaskmanager.RESET_ACTION, action);
                            break;
                        }
                    case "cancel":
                        {
                            if (TaskManager.Actions.ContainsKey(CreateTaskmanager.CANCEL_ACTION))
                                TaskManager.Actions[CreateTaskmanager.CANCEL_ACTION] = action;
                            else
                                TaskManager.Actions.Add(CreateTaskmanager.CANCEL_ACTION, action);
                            break;
                        }
                    case "copy":
                        {
                            if (TaskManager.Actions.ContainsKey(CreateTaskmanager.COPY_ACTION))
                                TaskManager.Actions[CreateTaskmanager.COPY_ACTION] = action;
                            else
                                TaskManager.Actions.Add(CreateTaskmanager.COPY_ACTION, action);
                            break;
                        }
                }

                Session["CreateDatasetTaskmanager"] = TaskManager;

                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        /// <summary>
        /// Set a action in the Form
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        /// <param name="area"></param>
        /// <param name="type">submit,cancel,reset,copy</param>
        public ActionResult SetCopyFunctionForView(string actionName, string controllerName, string area)
        {
            try
            {
                CreateTaskmanager TaskManager = (CreateTaskmanager)Session["ViewDatasetTaskmanager"] ??
                                                new CreateTaskmanager();

                ActionInfo action = new ActionInfo();
                action.ActionName = actionName;
                action.ControllerName = controllerName;
                action.AreaName = area;

                if (TaskManager.Actions.ContainsKey(CreateTaskmanager.COPY_ACTION))
                    TaskManager.Actions[CreateTaskmanager.COPY_ACTION] = action;
                else
                    TaskManager.Actions.Add(CreateTaskmanager.COPY_ACTION, action);

                Session["ViewDatasetTaskmanager"] = TaskManager;

                return Json(true);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public ActionResult Cancel()
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager != null)
            {
                using (var dm = new DatasetManager())
                {
                    long datasetid = -1;
                    long metadataStructureid = -1;
                    var resetTaskManager = true;
                    XmlDocument metadata = null;

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
                    {
                        datasetid = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
                    }

                    if (datasetid > -1 && dm.IsDatasetCheckedIn(datasetid))
                    {
                        var dataset = dm.GetDataset(datasetid);
                        metadataStructureid = dataset.MetadataStructure.Id;
                        metadata = dm.GetDatasetLatestMetadataVersion(datasetid);
                        TaskManager.UpdateBus(CreateTaskmanager.METADATA_XML, XmlUtility.ToXDocument(metadata));
                    }

                    return RedirectToAction("Show", "Data", new { area = "Ddm", id = datasetid });
                }
            }

            return RedirectToAction("StartMetadataEditor", "Form");
        }

        public ActionResult Reset()
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager != null)
            {
                using (var dm = new DatasetManager())
                {
                    long datasetid = -1;
                    long metadataStructureid = -1;
                    var resetTaskManager = true;
                    XmlDocument metadata = null;
                    var edit = true;
                    var created = false;

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
                    {
                        datasetid = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
                    }

                    if (datasetid > -1 && dm.IsDatasetCheckedIn(datasetid))
                    {
                        var dataset = dm.GetDataset(datasetid);
                        metadataStructureid = dataset.MetadataStructure.Id;
                        metadata = dm.GetDatasetLatestMetadataVersion(datasetid);
                        TaskManager.UpdateBus(CreateTaskmanager.METADATA_XML, XmlUtility.ToXDocument(metadata));
                    }

                    return RedirectToAction("ImportMetadata", "Form", new { area = "Dcm", metadataStructureId = metadataStructureid, edit, created });
                }
            }

            return RedirectToAction("StartMetadataEditor", "Form");
        }

        #endregion overrideable Action

        #region download

        public ActionResult DownloadAsHtml()
        {
            if (TaskManager == null) TaskManager = (CreateTaskmanager)Session["ViewDatasetTaskmanager"];

            if (TaskManager != null)
            {
                using (DatasetManager datasetManager = new DatasetManager())
                {
                    XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                    long entityId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
                    long datastructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.DATASTRUCTURE_ID]);
                    long researchplanId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.RESEARCHPLAN_ID]);
                    long metadatastructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);

                    var entityVersion = datasetManager.GetDatasetLatestVersion(entityId);

                    string title = entityVersion.Title;

                    // get the offline version of the metadata
                    var view = this.Render("DCM", "Form", "LoadMetadataOfflineVersion", new RouteValueDictionary()
                    {
                        { "entityId", entityId },
                        { "title", title },
                        { "metadatastructureId", metadatastructureId },
                        { "datastructureId", datastructureId },
                        { "researchplanId", researchplanId },
                        { "sessionKeyForMetadata", null },
                        { "resetTaskManager", false }
                    });

                    // prepare view to write it to the file
                    byte[] content = Encoding.UTF8.GetBytes(view.ToString());

                    return File(content, "application/xhtml+xml", entityId + "_metadata.htm");
                }
            }

            return Content("no metadata html file is loaded.");
        }

        public ActionResult DownloadAsXml()
        {
            var id = ViewBag.Title;
            id = TempData["EntityId"];

            try
            {
                if (TaskManager == null) TaskManager = (CreateTaskmanager)Session["ViewDatasetTaskmanager"];

                if (TaskManager != null)
                {
                    long entityId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
                    string path = OutputMetadataManager.CreateConvertedMetadata(entityId, TransmissionType.mappingFileExport);

                    path = Path.Combine(AppConfiguration.DataPath, path);

                    return File(path, "application/xml", Path.GetFileName(path));
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            return Content("no metadata xml file is loaded.");
        }

        #endregion download
    }
}