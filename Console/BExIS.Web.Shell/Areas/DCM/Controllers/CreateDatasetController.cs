using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Ddm.Api;
using BExIS.Ddm.Providers.LuceneProvider.Indexer;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.DCM.Models;
using BExIS.Web.Shell.Areas.DCM.Models.CreateDataset;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;
using BExIS.Xml.Helpers;
using BExIS.Xml.Services;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Models;
using BExIS.Xml.Helpers.Mapping;
using BExIS.IO;
using BExIS.Security.Services.Objects;
using BExIS.Web.Shell.Models;
using BExIS.Web.Shell.Helpers;
using NHibernate.Cache.Entry;
using Vaiona.IoC;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class CreateDatasetController : Controller
    {
        private CreateDatasetTaskmanager TaskManager;
  
        #region Create a Dataset Setup Page

        //
        // GET: /DCM/CreateDataset/
        /// <summary>
        /// Load the createDataset action with different parameter type options
        /// type eg ("DataStructureId", "DatasetId", "MetadataStructureId")
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult Index(long id = -1, string type = "")
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Create Dataset");

            Session["CreateDatasetTaskmanager"] = null;
            if (TaskManager == null) TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager == null)
            {

                TaskManager = new CreateDatasetTaskmanager();

                Session["CreateDatasetTaskmanager"] = TaskManager;
                Session["MetadataStructureViewList"] = LoadMetadataStructureViewList();
                Session["DataStructureViewList"] = LoadDataStructureViewList();
                Session["DatasetViewList"] = LoadDatasetViewList();

                SetupModel Model = GetDefaultModel();

                //if id is set and its type dataset
                if (id != -1 && type.ToLower().Equals("datasetid"))
                {
                    ViewBag.Title = PresentationModel.GetViewTitle("Copy Dataset");

                    DatasetManager datasetManager = new DatasetManager();
                    Dataset dataset = datasetManager.DatasetRepo.Get(id);
                    Model.SelectedDatasetId = id;
                    Model.SelectedMetadataStructureId = dataset.MetadataStructure.Id;
                    Model.SelectedDataStructureId = dataset.DataStructure.Id;
                    Model.BlockMetadataStructureId = true;
                    Model.BlockDatasetId = true;
                }

                if (id != -1 && type.ToLower().Equals("metadatastructureid"))
                {
                    ViewBag.Title = PresentationModel.GetViewTitle("Copy Dataset");
                    Model.SelectedMetadataStructureId = id;
                }

                if (id != -1 && type.ToLower().Equals("datastructureid"))
                {
                    ViewBag.Title = PresentationModel.GetViewTitle("Copy Dataset");
                    Model.SelectedDataStructureId = id;
                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
                        Model.SelectedMetadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
                }


                return View(Model);
            }

            return View();
        }

        public ActionResult ReloadIndex(long id = -1, string type = "")
        {
            ViewBag.Title = PresentationModel.GetViewTitle("...");

            if (TaskManager == null) TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            DatasetManager datasetManager = new DatasetManager();
            SetupModel Model = GetDefaultModel();

            //if id is set and its type dataset
            if (id != -1 && type.ToLower().Equals("datasetid"))
            {
                Dataset dataset = datasetManager.DatasetRepo.Get(id);
                Model.SelectedDatasetId = id;
                Model.SelectedMetadataStructureId = dataset.MetadataStructure.Id;
                Model.SelectedDataStructureId = dataset.DataStructure.Id;
                Model.BlockMetadataStructureId = true;
                Model.BlockDatasetId = false;

                TaskManager.AddToBus(CreateDatasetTaskmanager.COPY_OF_DATASET_ID, id);

            }

            if (id != -1 && type.ToLower().Equals("metadatastructureid"))
            {
                TaskManager.AddToBus(CreateDatasetTaskmanager.METADATASTRUCTURE_ID, id);
                Model.SelectedMetadataStructureId = id;

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASTRUCTURE_ID))
                    Model.SelectedDataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASTRUCTURE_ID]);

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.COPY_OF_DATASET_ID))
                {
                    Model.SelectedDatasetId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.COPY_OF_DATASET_ID]);
                    Dataset dataset = datasetManager.DatasetRepo.Get(Model.SelectedDatasetId);
                    Model.BlockMetadataStructureId = true;
                    Model.BlockDatasetId = false;
                }

            }

            if (id != -1 && type.ToLower().Equals("datastructureid"))
            {
                TaskManager.AddToBus(CreateDatasetTaskmanager.DATASTRUCTURE_ID, id);
                Model.SelectedDataStructureId = id;

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
                    Model.SelectedMetadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.COPY_OF_DATASET_ID))
                {
                    Model.SelectedDatasetId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.COPY_OF_DATASET_ID]);

                    Dataset dataset = datasetManager.DatasetRepo.Get(Model.SelectedDatasetId);
                    Model.BlockMetadataStructureId = true;
                    Model.BlockDatasetId = false;
                }
            }

            return View("Index", Model);
        }

        public ActionResult StoreSelectedDatasetSetup(SetupModel model)
        {
            CreateDatasetTaskmanager TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            DatasetManager dm = new DatasetManager();

            if (model == null)
            {
                model = GetDefaultModel();
                return PartialView("Index", model);
            }

            model = LoadLists(model);

            if (ModelState.IsValid )
            {
                TaskManager.AddToBus(CreateDatasetTaskmanager.METADATASTRUCTURE_ID, model.SelectedMetadataStructureId);
                TaskManager.AddToBus(CreateDatasetTaskmanager.DATASTRUCTURE_ID, model.SelectedDataStructureId);

                // set datastructuretype
                TaskManager.AddToBus(CreateDatasetTaskmanager.DATASTRUCTURE_TYPE, GetDataStructureType(model.SelectedDataStructureId));

                //dataset is selected
                if (model.SelectedDatasetId != 0 && model.SelectedDatasetId != -1)
                {
                    if (dm.IsDatasetCheckedIn(model.SelectedDatasetId))
                    {
                        DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(model.SelectedDatasetId);
                        TaskManager.AddToBus(CreateDatasetTaskmanager.RESEARCHPLAN_ID,
                            datasetVersion.Dataset.ResearchPlan.Id);
                        TaskManager.AddToBus(CreateDatasetTaskmanager.DATASET_TITLE,
                            XmlDatasetHelper.GetInformation(datasetVersion, AttributeNames.title));

                        // set datastructuretype
                        TaskManager.AddToBus(CreateDatasetTaskmanager.DATASTRUCTURE_TYPE,
                            GetDataStructureType(model.SelectedDataStructureId));

                        // set MetadataXml From selected existing Dataset
                        XDocument metadata = XmlUtility.ToXDocument(datasetVersion.Metadata);
                        SetXml(metadata);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Dataset is just in processing");
                    }
                }
                else
                {
                    ResearchPlanManager rpm = new ResearchPlanManager();
                    TaskManager.AddToBus(CreateDatasetTaskmanager.RESEARCHPLAN_ID, rpm.Repo.Get().First().Id);
                    // create MetadataTemplate based on the selected MetadatStructure
                    CreateXml();
                }

                // generate all steps
                // one step for each complex type  in the metadata structure
                AdvanceTaskManager(model.SelectedMetadataStructureId);


                return RedirectToAction("StartMetadataEditor", "CreateDataset");


            }

            return View("Index", model);
        }

        [HttpPost]
        public ActionResult StoreSelectedDataset(long id)
        {
            if (TaskManager == null) TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            DatasetManager dm = new DatasetManager();
            Dataset dataset = dm.GetDataset(id);

            SetupModel Model = GetDefaultModel();

            if (id == -1)
            {

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASTRUCTURE_ID))
                    Model.SelectedDataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASTRUCTURE_ID]);

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
                    Model.SelectedMetadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            }
            else
            {
                Model.SelectedDatasetId = id;
                Model.SelectedDataStructureId = dataset.DataStructure.Id;
                Model.SelectedMetadataStructureId = dataset.MetadataStructure.Id;

                Model.BlockMetadataStructureId = true;

                //add to Bus
                TaskManager.AddToBus(CreateDatasetTaskmanager.DATASTRUCTURE_ID, dataset.DataStructure.Id);
                TaskManager.AddToBus(CreateDatasetTaskmanager.METADATASTRUCTURE_ID, dataset.MetadataStructure.Id);
                TaskManager.AddToBus(CreateDatasetTaskmanager.COPY_OF_DATASET_ID, dataset.Id);
            }

            return PartialView("Index", Model);
        }

        [HttpPost]
        public ActionResult StoreSelectedOption(long id, string type)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            string key = "";

            switch (type)
            {
                case "ms": key = CreateDatasetTaskmanager.METADATASTRUCTURE_ID; break;
                case "ds": key = CreateDatasetTaskmanager.DATASTRUCTURE_ID; break;
            }

            if (key != "")
            {
                if (TaskManager.Bus.ContainsKey(key))
                    TaskManager.Bus[key] = id;
                else
                    TaskManager.Bus.Add(key, id);
            }

            return Content("");
        }

        [HttpGet]
        public ActionResult ShowListOfDatasets()
        {
            List<ListViewItem> datasets = LoadDatasetViewList();

            EntitySelectorModel model = BexisModelManager.LoadEntitySelectorModel(
                datasets,
                new EntitySelectorModelAction("ShowListOfDatasetsReciever", "CreateDataset", "DCM"));

            model.Title = "Select a Dataset";

            return PartialView("_EntitySelectorInWindowView", model);
        }

        public ActionResult ShowListOfDatasetsReciever(long id)
        {
            return RedirectToAction("ReloadIndex", "CreateDataset", new { id = id, type = "Datasetid" });
        }


        [HttpGet]
        public ActionResult ShowListOfDataStructures()
        {
            List<ListViewItemWithType> datastructures = LoadDataStructureViewList();

            EntitySelectorModel model = BexisModelManager.LoadEntitySelectorModel(
                 datastructures,
                 new EntitySelectorModelAction("ShowListOfDataStructuresReciever", "CreateDataset", "DCM"));

            model.Title = "Select a Data Structure";


            return PartialView("_EntitySelectorInWindowView", model);
        }

        public ActionResult ShowListOfDataStructuresReciever(long id)
        {
            return RedirectToAction("ReloadIndex", "CreateDataset", new { id = id, type = "DataStructureId" });
        }

        [HttpGet]
        public ActionResult ShowListOfMetadataStructures()
        {
            List<ListViewItem> metadataStructures = LoadMetadataStructureViewList();

            EntitySelectorModel model = BexisModelManager.LoadEntitySelectorModel(
                 metadataStructures,
                 new EntitySelectorModelAction("ShowListOfMetadataStructuresReciever", "CreateDataset", "DCM"));

            model.Title = "Select a Metadata Structure";

            return PartialView("_EntitySelectorInWindowView", model);
        }

        public ActionResult ShowListOfMetadataStructuresReciever(long id)
        {
            return RedirectToAction("ReloadIndex", "CreateDataset", new { id = id, type = "MetadataStructureId" });
        }



        private SetupModel GetDefaultModel()
        {
            SetupModel model = new SetupModel();

            model = LoadLists(model);

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
                model.SelectedMetadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
                model.SelectedMetadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            model.BlockDatasetId = false;
            model.BlockDatastructureId = false;
            model.BlockMetadataStructureId = false;

            return model;
        }

        private SetupModel LoadLists(SetupModel model)
        {
            if ((List<ListViewItem>)Session["MetadataStructureViewList"] != null) model.MetadataStructureViewList = (List<ListViewItem>)Session["MetadataStructureViewList"];
            if ((List<ListViewItemWithType>)Session["DataStructureViewList"] != null) model.DataStructureViewList = (List<ListViewItemWithType>)Session["DataStructureViewList"];
            if ((List<ListViewItem>)Session["DatasetViewList"] != null) model.DatasetViewList = (List<ListViewItem>)Session["DatasetViewList"];

            return model;
        }

        #endregion

        #region Load Metadata formular actions

        public ActionResult StartMetadataEditor()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Create Dataset");

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            List<StepModelHelper> stepInfoModelHelpers = new List<StepModelHelper>();

            // foreach step and the childsteps... generate a stepModelhelper
            foreach (var stepInfo in TaskManager.StepInfos)
            {
                StepModelHelper stepModelHelper = GetStepModelhelper(stepInfo.Id);

                if (stepModelHelper.Model == null)
                {
                    if (stepModelHelper.Usage is MetadataPackageUsage)
                        stepModelHelper.Model = CreatePackageModel(stepInfo.Id, false);
   
                     if (stepModelHelper.Usage is MetadataNestedAttributeUsage)
                        stepModelHelper.Model = CreateCompoundModel(stepInfo.Id, false);

                    getChildModelsHelper(stepModelHelper);
                }

                stepInfoModelHelpers.Add(stepModelHelper);

            }

            MetadataEditorModel Model = new MetadataEditorModel();
            Model.StepModelHelpers = stepInfoModelHelpers;

            return View("MetadataEditor", Model);
        }

        public ActionResult LoadMetadata(long datasetId, bool locked = false, bool created = false, bool fromEditMode = false, bool resetTaskManager = false, XmlDocument newMetadata = null)
        {
            bool loadFromExternal = resetTaskManager;

            ViewBag.Title = PresentationModel.GetViewTitle("Create Dataset");
            ViewData["Locked"] = locked;

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager == null)
            {
                TaskManager = new CreateDatasetTaskmanager();
                loadFromExternal = true;
            }

            List<StepModelHelper> stepInfoModelHelpers = new List<StepModelHelper>();
            MetadataEditorModel Model = new MetadataEditorModel();

            if (loadFromExternal)
            {
                TaskManager = new CreateDatasetTaskmanager();
                Session["CreateDatasetTaskmanager"] = TaskManager;
                TaskManager.AddToBus(CreateDatasetTaskmanager.DATASET_ID, datasetId);

                // load metadatStructrue id
                DatasetManager dm = new DatasetManager();

                if (dm.IsDatasetCheckedIn(datasetId))
                {
                    DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetId);

                    TaskManager.AddToBus(CreateDatasetTaskmanager.METADATASTRUCTURE_ID, dsv.Dataset.MetadataStructure.Id);
                    TaskManager.AddToBus(CreateDatasetTaskmanager.RESEARCHPLAN_ID, dsv.Dataset.ResearchPlan.Id);
                    TaskManager.AddToBus(CreateDatasetTaskmanager.DATASTRUCTURE_ID, dsv.Dataset.DataStructure.Id);

                    if (newMetadata == null)
                        TaskManager.AddToBus(CreateDatasetTaskmanager.METADATA_XML, XmlUtility.ToXDocument(dsv.Metadata));
                    else
                        TaskManager.AddToBus(CreateDatasetTaskmanager.METADATA_XML, XmlUtility.ToXDocument(newMetadata));

                    TaskManager.AddToBus(CreateDatasetTaskmanager.DATASET_TITLE,
                        XmlDatasetHelper.GetInformation(dsv, AttributeNames.title));

                    ResearchPlanManager rpm = new ResearchPlanManager();
                    TaskManager.AddToBus(CreateDatasetTaskmanager.RESEARCHPLAN_ID, rpm.Repo.Get().First().Id);

                    AdvanceTaskManagerBasedOnExistingMetadata(dsv.Dataset.MetadataStructure.Id);
                    //AdvanceTaskManager(dsv.Dataset.MetadataStructure.Id);


                    foreach (var stepInfo in TaskManager.StepInfos)
                    {

                        StepModelHelper stepModelHelper = GetStepModelhelper(stepInfo.Id);

                        if (stepModelHelper.Model == null)
                        {
                            if (stepModelHelper.Usage is MetadataPackageUsage)
                            {
                                stepModelHelper.Model = CreatePackageModel(stepInfo.Id, false);
                                if (stepModelHelper.Model.StepInfo.IsInstanze)
                                    LoadSimpleAttributesForModelFromXml(stepModelHelper);
                            }

                            if (stepModelHelper.Usage is MetadataNestedAttributeUsage)
                            {
                                stepModelHelper.Model = CreateCompoundModel(stepInfo.Id, false);
                                if (stepModelHelper.Model.StepInfo.IsInstanze)
                                    LoadSimpleAttributesForModelFromXml(stepModelHelper);
                            }

                            getChildModelsHelper(stepModelHelper);
                        }

                        stepInfoModelHelpers.Add(stepModelHelper);

                    }

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
                    {
                        XDocument xMetadata = (XDocument) TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                        string title = XmlDatasetHelper.GetInformation(dsv, AttributeNames.title);
                        if (String.IsNullOrEmpty(title)) title = "No Title available.";

                        if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASET_TITLE))
                            Model.DatasetTitle = TaskManager.Bus[CreateDatasetTaskmanager.DATASET_TITLE].ToString();
                        else
                            Model.DatasetTitle = "No Title available.";

                    }
                }
                else
                {
                    ModelState.AddModelError(String.Empty, "Dataset is just in processing.");
                }
            }
            else
            {
                stepInfoModelHelpers = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];
                Model.DatasetTitle = TaskManager.Bus[CreateDatasetTaskmanager.DATASET_TITLE].ToString();
            }
            
            Model.DatasetId = datasetId;
            Model.StepModelHelpers = stepInfoModelHelpers;
            Model.Created = created;

            //FromCreateOrEditMode
            TaskManager.AddToBus(CreateDatasetTaskmanager.EDIT_MODE, fromEditMode);
            Model.FromEditMode = (bool)TaskManager.Bus[CreateDatasetTaskmanager.EDIT_MODE];

            #region security permissions and authorisations check
                // set edit rigths
                DatasetManager datasetManager = new DatasetManager();
                PermissionManager permissionManager = new PermissionManager();
                SubjectManager subjectManager = new SubjectManager();
                Security.Services.Objects.TaskManager securityTaskManager = new Security.Services.Objects.TaskManager();

                bool hasAuthorizationRights = false;
                bool hasAuthenticationRigths = false;
                long userid = subjectManager.GetUserByName(GetUsernameOrDefault()).Id;
                //User has Access to Features 
                //Area DCM
                //Controller "Create Dataset" 
                //Action "*"
                Task task = securityTaskManager.GetTask("DCM", "CreateDataset", "*");
                if (task != null)
                {
                    hasAuthorizationRights = permissionManager.HasSubjectFeatureAccess(userid, task.Feature.Id);
                }

                hasAuthenticationRigths = permissionManager.HasUserDataAccess(userid, 1, datasetId, RightType.Update);

                Model.EditRight = (hasAuthorizationRights && hasAuthenticationRigths);

            #endregion


            return PartialView("MetadataEditor", Model);
        }

        public ActionResult EditMetadata(long datasetId, bool locked= false, bool created= false)
        {

            return RedirectToAction("LoadMetadata", "CreateDataset", new { datasetId = datasetId, locked = false, created = false, fromEditMode = true});
        }

        public ActionResult ImportMetadata(long metadataStructureId)
        {

            ViewBag.Title = PresentationModel.GetViewTitle("Create Dataset");

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            XmlDocument newMetadata = (XmlDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

            List<StepModelHelper> stepInfoModelHelpers = new List<StepModelHelper>();
            MetadataEditorModel Model = new MetadataEditorModel();

            TaskManager.AddToBus(CreateDatasetTaskmanager.METADATA_XML, XmlUtility.ToXDocument(newMetadata));

            AdvanceTaskManagerBasedOnExistingMetadata(metadataStructureId);

            foreach (var stepInfo in TaskManager.StepInfos)
            {

                StepModelHelper stepModelHelper = GetStepModelhelper(stepInfo.Id);

                if (stepModelHelper.Model == null)
                {
                    if (stepModelHelper.Usage is MetadataPackageUsage)
                    {
                        stepModelHelper.Model = CreatePackageModel(stepInfo.Id, false);
                        if (stepModelHelper.Model.StepInfo.IsInstanze)
                            LoadSimpleAttributesForModelFromXml(stepModelHelper);
                    }

                    if (stepModelHelper.Usage is MetadataNestedAttributeUsage)
                    {
                        stepModelHelper.Model = CreateCompoundModel(stepInfo.Id, false);
                        if (stepModelHelper.Model.StepInfo.IsInstanze)
                            LoadSimpleAttributesForModelFromXml(stepModelHelper);
                    }

                    getChildModelsHelper(stepModelHelper);
                }

                stepInfoModelHelpers.Add(stepModelHelper);

            }

            Model.StepModelHelpers = stepInfoModelHelpers;

            return PartialView("MetadataEditor", Model);
        }

        public ActionResult ReloadMetadataEditor()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Create Dataset");

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            List<StepModelHelper> stepInfoModelHelpers = new List<StepModelHelper>();

            foreach (var stepInfo in TaskManager.StepInfos)
            {

                StepModelHelper stepModelHelper = GetStepModelhelper(stepInfo.Id);

                if (stepModelHelper.Model == null)
                {
                    if (stepModelHelper.Usage is MetadataPackageUsage)
                        stepModelHelper.Model = CreatePackageModel(stepInfo.Id, false);

                    if (stepModelHelper.Usage is MetadataNestedAttributeUsage)
                        stepModelHelper.Model = CreateCompoundModel(stepInfo.Id, false);

                    getChildModelsHelper(stepModelHelper);
                }

                stepInfoModelHelpers.Add(stepModelHelper);

            }

            MetadataEditorModel Model = new MetadataEditorModel();
            Model.StepModelHelpers = stepInfoModelHelpers;


            return PartialView("MetadataEditor", Model);
        }

        #endregion

        private StepModelHelper getChildModelsHelper(StepModelHelper stepModelHelper)
        {
            if (stepModelHelper.Model.StepInfo.Children.Count > 0)
            {
                foreach (var childStep in stepModelHelper.Model.StepInfo.Children)
                {
                    StepModelHelper childStepModelHelper = GetStepModelhelper(childStep.Id);

                    if (childStepModelHelper.Model == null)
                    {
                        if (childStepModelHelper.Usage is MetadataPackageUsage)
                            childStepModelHelper.Model = CreatePackageModel(childStep.Id, false);

                        if (childStepModelHelper.Usage is MetadataNestedAttributeUsage)
                            childStepModelHelper.Model = CreateCompoundModel(childStep.Id, false);

                        if (childStepModelHelper.Usage is MetadataAttributeUsage)
                            childStepModelHelper.Model = CreateCompoundModel(childStep.Id, false);

                        if (childStepModelHelper.Model.StepInfo.IsInstanze)
                            LoadSimpleAttributesForModelFromXml(childStepModelHelper);
                    }

                    childStepModelHelper = getChildModelsHelper(childStepModelHelper);

                    stepModelHelper.Childrens.Add(childStepModelHelper);
                }
            }

            return stepModelHelper;
        }

        private void AdvanceTaskManager(long MetadataStructureId)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            List<MetadataPackageUsage> metadataPackageList = metadataStructureManager.GetEffectivePackages(MetadataStructureId).ToList();

            List<StepModelHelper> stepModelHelperList = new List<StepModelHelper>();
            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
            }
            else
            {
                TaskManager.Bus.Add(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER, stepModelHelperList);
            }

            TaskManager.StepInfos = new List<StepInfo>();

            foreach (MetadataPackageUsage mpu in metadataPackageList)
            {
                //only add none optional usages
                StepInfo si = new StepInfo(mpu.Label)
                {
                    Id = TaskManager.GenerateStepId(),
                    parentTitle = mpu.MetadataPackage.Name,
                    Parent = TaskManager.Root,
                    IsInstanze = false,
                };

                TaskManager.StepInfos.Add(si);
                StepModelHelper stepModelHelper = new StepModelHelper(si.Id, 1, mpu, "Metadata//" + mpu.Label.Replace(" ", string.Empty) + "[1]");

                stepModelHelperList.Add(stepModelHelper);

                if (mpu.MinCardinality > 0)
                {

                    si = AddStepsBasedOnUsage(mpu, si, "Metadata//" + mpu.Label.Replace(" ", string.Empty) + "[1]");
                    TaskManager.Root.Children.Add(si);

                    TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
                }
            }

            TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
            Session["CreateDatasetTaskmanager"] = TaskManager;
        }

        private void AdvanceTaskManagerBasedOnExistingMetadata(long MetadataStructureId)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            List<MetadataPackageUsage> metadataPackageList = metadataStructureManager.GetEffectivePackages(MetadataStructureId).ToList();

            List<StepModelHelper> stepModelHelperList = new List<StepModelHelper>();
            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
            }
            else
            {
                TaskManager.Bus.Add(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER, stepModelHelperList);
            }

            TaskManager.StepInfos = new List<StepInfo>();

            foreach (MetadataPackageUsage mpu in metadataPackageList)
            {
                //only add none optional usages
                StepInfo si = new StepInfo(mpu.Label)
                {
                    Id = TaskManager.GenerateStepId(),
                    parentTitle = mpu.MetadataPackage.Name,
                    Parent = TaskManager.Root,
                    IsInstanze = false,
                };

                TaskManager.StepInfos.Add(si);
                StepModelHelper stepModelHelper = new StepModelHelper(si.Id, 1, mpu, "Metadata//" + mpu.Label.Replace(" ", string.Empty) + "[1]");

                stepModelHelperList.Add(stepModelHelper);

                if (mpu.MinCardinality > 0)
                {

                    si = LoadStepsBasedOnUsage(mpu, si, "Metadata//" + mpu.Label.Replace(" ", string.Empty) + "[1]");
                    TaskManager.Root.Children.Add(si);

                    TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
                }
            }

            TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER] = stepModelHelperList;
            Session["CreateDatasetTaskmanager"] = TaskManager;
        }

        private List<BaseUsage> GetCompoundAttributeUsages(BaseUsage usage)
        {
            List<BaseUsage> list = new List<BaseUsage>();

            if (usage is MetadataPackageUsage)
            {
                MetadataPackageUsage mpu = (MetadataPackageUsage)usage;

                foreach (MetadataAttributeUsage mau in mpu.MetadataPackage.MetadataAttributeUsages)
                {
                    list.AddRange(GetCompoundAttributeUsages(mau));
                }
            }

            if (usage is MetadataAttributeUsage)
            {
                MetadataAttributeUsage mau = (MetadataAttributeUsage)usage;

                if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                {
                    list.Add(mau);

                    MetadataCompoundAttribute mca = (MetadataCompoundAttribute)mau.MetadataAttribute.Self;

                    foreach (MetadataNestedAttributeUsage mnau in mca.MetadataNestedAttributeUsages)
                    {
                        list.AddRange(GetCompoundAttributeUsages(mnau));
                    }
                }

            }

            if (usage is MetadataNestedAttributeUsage)
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)usage;


                if (mnau.Member.Self is MetadataCompoundAttribute)
                {
                    list.Add(mnau);

                    MetadataCompoundAttribute mca = (MetadataCompoundAttribute)mnau.Member.Self;

                    foreach (MetadataNestedAttributeUsage m in mca.MetadataNestedAttributeUsages)
                    {
                        list.AddRange(GetCompoundAttributeUsages(m));
                    }
                }
            }

            return list;
        }

        private void CreateXml()
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            // load metadatastructure with all packages and attributes

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
            {
                XmlMetadataWriter xmlMetadatWriter = new XmlMetadataWriter(XmlNodeMode.xPath);


                XDocument metadataXml = xmlMetadatWriter.CreateMetadataXml(Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]));

                // locat path
                //string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");

                TaskManager.AddToBus(CreateDatasetTaskmanager.METADATA_XML, metadataXml);

                //setup loaded
                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.SETUP_LOADED))
                    TaskManager.Bus[CreateDatasetTaskmanager.SETUP_LOADED] = true;
                else
                    TaskManager.Bus.Add(CreateDatasetTaskmanager.SETUP_LOADED, true);


                //save
                //metadataXml.Save(path);
            }

        }

        private void SetXml(XDocument metadataXml)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            // load metadatastructure with all packages and attributes

            if (metadataXml != null)
            {
                // locat path
                //string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");

                TaskManager.AddToBus(CreateDatasetTaskmanager.METADATA_XML, metadataXml);

                //setup loaded
                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.SETUP_LOADED))
                    TaskManager.Bus[CreateDatasetTaskmanager.SETUP_LOADED] = true;
                else
                    TaskManager.Bus.Add(CreateDatasetTaskmanager.SETUP_LOADED, true);
            }

        }

        #region Import Metadata From external XML

        /// <summary>
        /// Selected File store din the BUS
        /// </summary>
        /// <param name="SelectFileUploader"></param>
        /// <returns></returns>
        public ActionResult SelectFileProcess(HttpPostedFileBase SelectFileUploader)
        {

            if (TaskManager == null) TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            if (SelectFileUploader != null)
            {
                //data/datasets/1/1/
                string dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                string storepath = Path.Combine(dataPath, "Temp", GetUsernameOrDefault());

                // if folder not exist
                if (!Directory.Exists(storepath))
                {
                    Directory.CreateDirectory(storepath);
                }

                string path = Path.Combine(storepath, SelectFileUploader.FileName);

                SelectFileUploader.SaveAs(path);

                TaskManager.AddToBus(CreateDatasetTaskmanager.METADATA_IMPORT_XML_FILEPATH, path);

            }

            return Content("");
        }

        public ActionResult LoadExternalXml()
        {
            if (TaskManager == null) TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager != null &&
                TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID) &&
                TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_IMPORT_XML_FILEPATH))
            {

                //xml metadata for import
                string metadataForImportPath = (string)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_IMPORT_XML_FILEPATH];

                if (FileHelper.FileExist(metadataForImportPath))
                {
                    XmlDocument metadataForImport = new XmlDocument();
                    metadataForImport.Load(metadataForImportPath);

                    // metadataStructure DI
                    long metadataStructureId = (Int64)TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID];
                    //long datasetId = (Int64)TaskManager.Bus[CreateDatasetTaskmanager.DATASET_ID];



                    // loadMapping file
                    string path_mappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), XmlMetadataImportHelper.GetMappingFileName(metadataStructureId));

                    // XML mapper + mapping file
                    XmlMapperManager xmlMapperManager = new XmlMapperManager();
                    xmlMapperManager.Load(path_mappingFile, "IDIV");

                    // generate intern metadata 
                    XmlDocument metadataResult = xmlMapperManager.Generate(metadataForImport, 1, true);

                    // generate intern template
                    XmlMetadataWriter xmlMetadatWriter = new XmlMetadataWriter(BExIS.Xml.Helpers.XmlNodeMode.xPath);
                    XDocument metadataXml = xmlMetadatWriter.CreateMetadataXml(metadataStructureId);
                    XmlDocument metadataXmlTemplate = XmlMetadataWriter.ToXmlDocument(metadataXml);

                    XmlDocument completeMetadata = XmlMetadataImportHelper.FillInXmlAttributes(metadataResult, metadataXmlTemplate);

                    TaskManager.AddToBus(CreateDatasetTaskmanager.METADATA_XML, completeMetadata);


                    //LoadMetadata(long datasetId, bool locked= false, bool created= false, bool fromEditMode = false, bool resetTaskManager = false, XmlDocument newMetadata=null)
                    return RedirectToAction("ImportMetadata", "CreateDataset", new { metadataStructureId = metadataStructureId });
                }
            }

            return null;
        }

        #endregion

        #region Models

        private MetadataPackageModel CreatePackageModel(int stepId, bool validateIt)
        {
            StepInfo stepInfo = TaskManager.Get(stepId);
            StepModelHelper stepModelHelper = GetStepModelhelper(stepId);

            long metadataPackageId = stepModelHelper.Usage.Id;
            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();
            MetadataPackageUsage mpu = (MetadataPackageUsage)LoadUsage(stepModelHelper.Usage);
            MetadataPackageModel model = new MetadataPackageModel();

            model = MetadataPackageModel.Convert(mpu, stepModelHelper.Number);
            model.ConvertMetadataAttributeModels(mpu, metadataStructureId, stepId);

            if (stepInfo.IsInstanze == false)
            {
                //get Instance
                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
                {
                    XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
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

            //if (validateIt)
            //{
            //    //validate packages
            //    List<Error> errors = validateStep(stepModelHelper.Model);
            //    if (errors != null)
            //        model.ErrorList = errors;
            //    else
            //        model.ErrorList = new List<Error>();

            //}

            model.StepInfo = stepInfo;

            return model;
        }

        private MetadataCompoundAttributeModel CreateCompoundModel(int stepId, bool validateIt)
        {
            StepInfo stepInfo = TaskManager.Get(stepId);
            StepModelHelper stepModelHelper = GetStepModelhelper(stepId);

            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
            long Id = stepModelHelper.Usage.Id;

            MetadataCompoundAttributeModel model = MetadataCompoundAttributeModel.ConvertToModel(stepModelHelper.Usage, stepModelHelper.Number);

            // get children
            model.ConvertMetadataAttributeModels(LoadUsage(stepModelHelper.Usage), metadataStructureId, stepInfo.Id);
            model.StepInfo = TaskManager.Get(stepId);

            if (stepInfo.IsInstanze == false)
            {
                //get Instance
                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
                {
                    XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
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

            //if (validateIt)
            //{
            //    //validate packages
            //    List<Error> errors = validateStep(stepModelHelper.Model);
            //    if (errors != null)
            //        model.ErrorList = errors;
            //    else
            //        model.ErrorList = new List<Error>();

            //}


            model.StepInfo = stepInfo;

            return model;
        }

        private AbstractMetadataStepModel LoadSimpleAttributesForModelFromXml(StepModelHelper stepModelHelper)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            XDocument metadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

            XElement complexElement =  XmlUtility.GetXElementByXPath(stepModelHelper.XPath,metadata);
            List<MetadataAttributeModel> additionalyMetadataAttributeModel = new List<MetadataAttributeModel>();

            foreach (MetadataAttributeModel simpleMetadataAttributeModel in stepModelHelper.Model.MetadataAttributeModels)
            {
                int numberOfSMM = 1;
                if (complexElement != null)
                {
                    //Debug.WriteLine("XXXXXXXXXXXXXXXXXXXX");
                    //Debug.WriteLine(simpleMetadataAttributeModel.Source.Label); 
                    IEnumerable<XElement> childs = XmlUtility.GetChildren(complexElement).Where(e => e.Attribute("id").Value.Equals(simpleMetadataAttributeModel.Id.ToString()));


                    if (childs.Any())
                        numberOfSMM = childs.First().Elements().Count();
                }

                for (int i = 1; i <= numberOfSMM; i++)
                {
                    string xpath = stepModelHelper.GetXPathFromSimpleAttribute(simpleMetadataAttributeModel.Id, i);
                    XElement simpleElement = XmlUtility.GetXElementByXPath(xpath, metadata);

                    if (i == 1)
                    {

                        if (simpleElement != null && !String.IsNullOrEmpty(simpleElement.Value))
                        {
                            simpleMetadataAttributeModel.Value = simpleElement.Value;
                            //Debug.WriteLine(xpath + "   :    " + simpleElement.Value);
                        }

                    }
                    else
                    {
                        MetadataAttributeModel newMetadataAttributeModel = simpleMetadataAttributeModel.Kopie(i, numberOfSMM);
                        newMetadataAttributeModel.Value = simpleElement.Value;
                        if (i == numberOfSMM) newMetadataAttributeModel.last = true;
                        additionalyMetadataAttributeModel.Add(newMetadataAttributeModel);
                    }

                    
                }
            }

            foreach (var item in additionalyMetadataAttributeModel)
            {
                List<MetadataAttributeModel> tempList = stepModelHelper.Model.MetadataAttributeModels;

                int indexOfLastSameAttribute = tempList.IndexOf(tempList.Where(a => a.Id.Equals(item.Id)).Last());
                tempList.Insert(indexOfLastSameAttribute + 1, item);
            }


            return stepModelHelper.Model;
        }

        #endregion

        #region Add and Remove

        public ActionResult AddMetadataAttributeUsage(int id, int parentid, int number, int parentModelNumber, int parentStepId)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            StepModelHelper stepModelHelperParent = list.Where(s => s.StepId.Equals(parentStepId)).FirstOrDefault();


            BaseUsage parentUsage = LoadUsage(stepModelHelperParent.Usage);
            int pNumber = stepModelHelperParent.Number;

            BaseUsage metadataAttributeUsage = UsageHelper.GetChildren(parentUsage).Where(u => u.Id.Equals(id)).FirstOrDefault();

            //BaseUsage metadataAttributeUsage = UsageHelper.GetSimpleUsageById(parentUsage, id);

            MetadataAttributeModel modelAttribute = MetadataAttributeModel.Convert(metadataAttributeUsage, parentUsage, metadataStructureId, parentModelNumber, stepModelHelperParent.StepId);
            modelAttribute.Number = ++number;

            AbstractMetadataStepModel model = stepModelHelperParent.Model;

            Insert(modelAttribute, stepModelHelperParent, number);
            UpdateChildrens(stepModelHelperParent, modelAttribute.Source.Id);


            //addtoxml
            AddAttributeToXml(parentUsage, parentModelNumber, metadataAttributeUsage, number);

            model.ConvertInstance((XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML], stepModelHelperParent.XPath + "//" + metadataAttributeUsage.Label.Replace(" ", string.Empty));

            if (model != null)
            {

                if (model is MetadataPackageModel)
                {
                    return PartialView("_metadataPackageUsageView", stepModelHelperParent);
                }

                if (model is MetadataCompoundAttributeModel)
                {
                    return PartialView("_metadataCompoundAttributeUsageView", stepModelHelperParent);
                }
            }

            return null;

        }

        public ActionResult RemoveMetadataAttributeUsage(object value, int id, int parentid, int number, int parentModelNumber, int parentStepId)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            StepModelHelper stepModelHelperParent = list.Where(s => s.StepId.Equals(parentStepId)).FirstOrDefault();
            
            //find the right element in the list
            MetadataAttributeModel removeAttributeModel = stepModelHelperParent.Model.MetadataAttributeModels.Where(m=>m.Source.Id.Equals(id) && m.Number.Equals(number)).First();
            //remove attribute
            stepModelHelperParent.Model.MetadataAttributeModels.Remove(removeAttributeModel);
            //update childrenList
            UpdateChildrens(stepModelHelperParent, id);

            AbstractMetadataStepModel model = stepModelHelperParent.Model;

            BaseUsage parentUsage = stepModelHelperParent.Usage;
            BaseUsage attrUsage = UsageHelper.GetChildren(parentUsage).Where(u => u.Id.Equals(id)).FirstOrDefault();

            //remove from xml
            RemoveAttributeToXml(stepModelHelperParent.Usage, stepModelHelperParent.Number, attrUsage, number, UsageHelper.GetNameOfType(attrUsage));
            
            if (model != null)
            {

                if (model is MetadataPackageModel)
                {
                    return PartialView("_metadataPackageUsageView", stepModelHelperParent);
                }

                if (model is MetadataCompoundAttributeModel)
                {
                    return PartialView("_metadataCompoundAttributeUsageView", stepModelHelperParent);
                }
            }

            return null;
        }

        public ActionResult UpMetadataAttributeUsage(object value, int id, int parentid, int number, int parentModelNumber, int parentStepId)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            StepModelHelper stepModelHelperParent = list.Where(s => s.StepId.Equals(parentStepId)).FirstOrDefault();

            // up in xml
            //ChangeInXml(stepModelHelperParent.Usage, parentModelNumber, id, number);

            // up in Model
            Up(stepModelHelperParent, id, number);

            UpdateChildrens(stepModelHelperParent,id);


            AbstractMetadataStepModel model = stepModelHelperParent.Model;

            if (model != null)
            {
                if (model is MetadataPackageModel)
                {
                    return PartialView("_metadataPackageUsageView", stepModelHelperParent);
                }

                if (model is MetadataCompoundAttributeModel)
                {
                    return PartialView("_metadataCompoundAttributeUsageView", stepModelHelperParent);
                }
            }

            return null;
        }

        public ActionResult DownMetadataAttributeUsage(object value, int id, int parentid, int number, int parentModelNumber, int parentStepId)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            StepModelHelper stepModelHelperParent = list.Where(s => s.StepId.Equals(parentStepId)).FirstOrDefault();

            Down(stepModelHelperParent, id, number);

            UpdateChildrens(stepModelHelperParent,id);

            AbstractMetadataStepModel model = stepModelHelperParent.Model;

            if (model != null)
            {
                if (model is MetadataPackageModel)
                {
                    return PartialView("_metadataPackageUsageView", stepModelHelperParent);
                }

                if (model is MetadataCompoundAttributeModel)
                {
                    return PartialView("_metadataCompoundAttributeUsageView", stepModelHelperParent);
                }
            }

            return null;
        }

        public ActionResult AddComplexUsage(int parentStepId, int number)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //TaskManager.SetCurrent(TaskManager.Get(parentStepId));

            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
            int position = number + 1;

            StepModelHelper parentStepModelHelper = GetStepModelhelper(parentStepId);
            BaseUsage u = LoadUsage(parentStepModelHelper.Usage);

            //Create new step
            StepInfo newStep = new StepInfo(UsageHelper.GetNameOfType(u))
            {
                Id = TaskManager.GenerateStepId(),
                parentTitle = parentStepModelHelper.Model.StepInfo.title,
                Parent = parentStepModelHelper.Model.StepInfo,
                IsInstanze = true,
            };

            string xPath = parentStepModelHelper.XPath + "//" + UsageHelper.GetNameOfType(u)+ "[" + position + "]";

            newStep.Children = GetChildrenSteps(u, newStep, xPath);

            // add to parent stepId
            parentStepModelHelper.Model.StepInfo.Children.Add(newStep);
            TaskManager.StepInfos.Add(newStep);

            // create Model
            AbstractMetadataStepModel model = null;

            if (u is MetadataAttributeUsage || u is MetadataNestedAttributeUsage)
            {
                model = MetadataCompoundAttributeModel.ConvertToModel(parentStepModelHelper.Usage, number);
                model.Number = position;
                ((MetadataCompoundAttributeModel)model).ConvertMetadataAttributeModels(LoadUsage(parentStepModelHelper.Usage), metadataStructureId, newStep.Id);

                //Update metadata xml
                //add step to metadataxml
                AddCompoundAttributeToXml(model.Source, model.Number, parentStepModelHelper.XPath);
            }

            if (u is MetadataPackageUsage)
            {
                model = MetadataPackageModel.Convert(parentStepModelHelper.Usage, number);
                model.Number = position;
                ((MetadataPackageModel)model).ConvertMetadataAttributeModels(LoadUsage(parentStepModelHelper.Usage), metadataStructureId, newStep.Id);

                //Update metadata xml
                //add step to metadataxml
                AddPackageToXml(model.Source, model.Number, parentStepModelHelper.XPath);
            }

            // create StepModel for new step
            StepModelHelper newStepModelhelper = new StepModelHelper
            {
                StepId = newStep.Id,
                Usage = parentStepModelHelper.Usage,
                Number = position,
                Model = model,
                XPath = xPath,
            };

            newStepModelhelper.Model.StepInfo = newStep;
            newStepModelhelper = getChildModelsHelper(newStepModelhelper);


            // add stepmodel to dictionary
            AddStepModelhelper(newStepModelhelper);

            //add stepModel to parentStepModel
            parentStepModelHelper.Childrens.Insert(newStepModelhelper.Number - 1, newStepModelhelper);

            //update childrens of the parent step based on number
            for (int i = 0; i < parentStepModelHelper.Childrens.Count; i++)
            {
                StepModelHelper smh = parentStepModelHelper.Childrens.ElementAt(i);
                smh.Number = i + 1;
            }

            // add step to parent and update title of steps
            //parentStepModelHelper.Model.StepInfo.Children.Insert(newStepModelhelper.Number - 1, newStep);
            for (int i = 0; i < parentStepModelHelper.Model.StepInfo.Children.Count; i++)
            {
                StepInfo si = parentStepModelHelper.Model.StepInfo.Children.ElementAt(i);
                si.title = (i + 1).ToString();
            }

            //TaskManager.SetCurrent(newStep);
            ////add childsteps
            ////newStep.Children = GetChildrenSteps(u, parentStepModelHelper.Model.StepInfo, parentStepModelHelper.XPath);
            ////TaskManager.StepInfos.Add(newStep);

            //// add step to parent and update title of steps
            //parentStepModelHelper.Model.StepInfo.Children.Insert(newStepModelhelper.Number-1,newStep);
            //for (int i = 0; i < parentStepModelHelper.Model.StepInfo.Children.Count; i++)
            //{
            //    StepInfo si = parentStepModelHelper.Model.StepInfo.Children.ElementAt(i);
            //    si.title = ""+i+1;
            //}


            //newStepModelhelper = GenerateModelsForChildrens(newStep, metadataStructureId);

            //////add stepModel to parentStepModel
            //parentStepModelHelper.Childrens.Insert(newStepModelhelper.Number - 1, newStepModelhelper);

            ////update childrens of the parent step based on number
            //for (int i = 0; i < parentStepModelHelper.Childrens.Count; i++)
            //{
            //    StepModelHelper smh = parentStepModelHelper.Childrens.ElementAt(i);
            //    smh.UpdatePosition(i + 1);
            //}

            //// load InstanzB for parentmodel
            parentStepModelHelper.Model.ConvertInstance((XDocument)(TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML]), parentStepModelHelper.XPath);

            ////Add to Steps
            ////model.StepInfo = newStep;



            if (u is MetadataAttributeUsage || u is MetadataNestedAttributeUsage)
            {
                return PartialView("_metadataCompoundAttributeView", parentStepModelHelper);
            }

            if (u is MetadataPackageUsage)
            {
                return PartialView("_metadataPackageView", parentStepModelHelper);
            }

            return null;
        }

        public ActionResult RemoveComplexUsage(int parentStepId, int number)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            TaskManager.SetCurrent(TaskManager.Get(parentStepId));

            StepModelHelper stepModelHelper = GetStepModelhelper(parentStepId);
            RemoveFromXml(stepModelHelper.XPath + "//" + UsageHelper.GetNameOfType(stepModelHelper.Usage).Replace(" ", string.Empty) + "[" + number + "]");

            BaseUsage u = LoadUsage(stepModelHelper.Usage);

            if (u is MetadataAttributeUsage || u is MetadataNestedAttributeUsage)
            {
                CreateCompoundModel(TaskManager.Current().Id, true);
            }

            if(u is MetadataPackageUsage)
            {
                stepModelHelper.Model = CreatePackageModel(TaskManager.Current().Id, true);
            }

            stepModelHelper.Childrens.RemoveAt(number-1);

            //add stepModel to parentStepModel
            for (int i = 0; i < stepModelHelper.Childrens.Count; i++)
            {
                stepModelHelper.Childrens.ElementAt(i).Number = i + 1;
            }
            
            TaskManager.Remove(TaskManager.Current(), number - 1);

            if (u is MetadataAttributeUsage || u is MetadataNestedAttributeUsage)
            {
                return PartialView("_metadataCompoundAttributeView", stepModelHelper);

            }
            else if (u is MetadataPackageUsage)
            {
                return PartialView("_metadataPackageView", stepModelHelper);
            }

            return null;
        }

        public ActionResult UpComplexUsage(int parentStepId, int number)
        {
            int newIndex = number - 2;
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            TaskManager.SetCurrent(TaskManager.Get(parentStepId));

            StepModelHelper stepModelHelper = GetStepModelhelper(parentStepId);
            BaseUsage u = LoadUsage(stepModelHelper.Usage);

            if (newIndex >= 0)
            {

                string xPathOfSelectedElement = stepModelHelper.XPath + "//" + UsageHelper.GetNameOfType(stepModelHelper.Usage).Replace(" ", string.Empty) + "[" + number + "]";
                string destinationXPathElement = stepModelHelper.XPath + "//" + UsageHelper.GetNameOfType(stepModelHelper.Usage).Replace(" ", string.Empty) + "[" + (number - 1) + "]";

                ChangeInXml(xPathOfSelectedElement, destinationXPathElement);

                if (u is MetadataAttributeUsage || u is MetadataNestedAttributeUsage)
                {
                    CreateCompoundModel(TaskManager.Current().Id, true);
                }

                if (u is MetadataPackageUsage)
                {
                    stepModelHelper.Model = CreatePackageModel(TaskManager.Current().Id, true);
                }

                StepModelHelper selectedStepModelHelper = stepModelHelper.Childrens.ElementAt(number - 1);
                stepModelHelper.Childrens.Remove(selectedStepModelHelper);

                stepModelHelper.Childrens.Insert(newIndex, selectedStepModelHelper);

                //update childrens of the parent step based on number
                for (int i = 0; i < stepModelHelper.Childrens.Count; i++)
                {
                    StepModelHelper smh = stepModelHelper.Childrens.ElementAt(i);
                    smh.Number = i + 1;
                    smh.Model.Number = i + 1;
                }

                StepInfo selectedStepInfo = stepModelHelper.Model.StepInfo.Children.ElementAt(number - 1);
                stepModelHelper.Model.StepInfo.Children.Remove(selectedStepInfo);
                stepModelHelper.Model.StepInfo.Children.Insert(newIndex, selectedStepInfo);

                for (int i = 0; i < stepModelHelper.Model.StepInfo.Children.Count; i++)
                {
                    StepInfo si = stepModelHelper.Model.StepInfo.Children.ElementAt(i);
                    si.title = (i + 1).ToString();
                }

               stepModelHelper.Model.ConvertInstance((XDocument)(TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML]), stepModelHelper.XPath);

            }

            if (u is MetadataAttributeUsage || u is MetadataNestedAttributeUsage)
            {
                return PartialView("_metadataCompoundAttributeView", stepModelHelper);

            }
            else if (u is MetadataPackageUsage)
            {
                return PartialView("_metadataPackageView", stepModelHelper);
            }

            return null;
        }

        public ActionResult DownComplexUsage(int parentStepId, int number)
        {
            int newIndex = number;
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            TaskManager.SetCurrent(TaskManager.Get(parentStepId));

            StepModelHelper stepModelHelper = GetStepModelhelper(parentStepId);
            BaseUsage u = LoadUsage(stepModelHelper.Usage);

            if (newIndex <= stepModelHelper.Childrens.Count-1)
            {

                string xPathOfSelectedElement = stepModelHelper.XPath + "//" + UsageHelper.GetNameOfType(stepModelHelper.Usage).Replace(" ", string.Empty) + "[" + number + "]";
                string destinationXPathElement = stepModelHelper.XPath + "//" + UsageHelper.GetNameOfType(stepModelHelper.Usage).Replace(" ", string.Empty) + "[" + (number +1) + "]";

                ChangeInXml(xPathOfSelectedElement, destinationXPathElement);

                if (u is MetadataAttributeUsage || u is MetadataNestedAttributeUsage)
                {
                    CreateCompoundModel(TaskManager.Current().Id, true);
                }

                if (u is MetadataPackageUsage)
                {
                    stepModelHelper.Model = CreatePackageModel(TaskManager.Current().Id, true);
                }

                StepModelHelper selectedStepModelHelper = stepModelHelper.Childrens.ElementAt(number - 1);
                stepModelHelper.Childrens.Remove(selectedStepModelHelper);

                stepModelHelper.Childrens.Insert(newIndex, selectedStepModelHelper);

                //update childrens of the parent step based on number
                for (int i = 0; i < stepModelHelper.Childrens.Count; i++)
                {
                    StepModelHelper smh = stepModelHelper.Childrens.ElementAt(i);
                    smh.Number = i + 1;
                    smh.Model.Number = i + 1;
                }

                StepInfo selectedStepInfo = stepModelHelper.Model.StepInfo.Children.ElementAt(number - 1);
                stepModelHelper.Model.StepInfo.Children.Remove(selectedStepInfo);
                stepModelHelper.Model.StepInfo.Children.Insert(newIndex, selectedStepInfo);

                for (int i = 0; i < stepModelHelper.Model.StepInfo.Children.Count; i++)
                {
                    StepInfo si = stepModelHelper.Model.StepInfo.Children.ElementAt(i);
                    si.title = (i + 1).ToString();
                }

                stepModelHelper.Model.ConvertInstance((XDocument)(TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML]), stepModelHelper.XPath);

            }

            if (u is MetadataAttributeUsage || u is MetadataNestedAttributeUsage)
            {
                return PartialView("_metadataCompoundAttributeView", stepModelHelper);

            }
            else if (u is MetadataPackageUsage)
            {
                return PartialView("_metadataPackageView", stepModelHelper);
            }

            return null;
        }

        #endregion

        #region Submit And Create And Finish And Cancel and Reset

        public ActionResult Submit()
        {
            long datasetId = SubmitDataset();
            bool editMode = false;

            if (TaskManager == null) TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.EDIT_MODE))
                editMode = (bool)TaskManager.Bus[CreateDatasetTaskmanager.EDIT_MODE];

            if(editMode)
                return RedirectToAction("LoadMetadata", "CreateDataset", new { datasetId = datasetId, locked = true, created = false, fromEditMode = true});
            else
                return RedirectToAction("LoadMetadata", "CreateDataset", new { datasetId = datasetId, locked = true, created = true });

        }
            
        public long SubmitDataset()
        {
            #region create dataset

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASTRUCTURE_ID)
                && TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.RESEARCHPLAN_ID)
                && TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
            {
                DatasetManager dm = new DatasetManager();
                long datasetId = 0;
                // for e new dataset
                if (!TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASET_ID))
                {
                    long datastructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASTRUCTURE_ID]);
                    long researchPlanId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.RESEARCHPLAN_ID]);
                    long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

                    DataStructureManager dsm = new DataStructureManager();

                    DataStructure dataStructure = dsm.StructuredDataStructureRepo.Get(datastructureId);
                    //if datastructure is not a structured one
                    if (dataStructure == null) dataStructure = dsm.UnStructuredDataStructureRepo.Get(datastructureId);

                    ResearchPlanManager rpm = new ResearchPlanManager();
                    ResearchPlan rp = rpm.Repo.Get(researchPlanId);

                    MetadataStructureManager msm = new MetadataStructureManager();
                    MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                    var ds = dm.CreateEmptyDataset(dataStructure, rp, metadataStructure);
                    datasetId = ds.Id;

                    // add security
                    if (GetUsernameOrDefault() != "DEFAULT")
                    {
                        PermissionManager pm = new PermissionManager();
                        SubjectManager sm = new SubjectManager();

                        BExIS.Security.Entities.Subjects.User user = sm.GetUserByName(GetUsernameOrDefault());

                        foreach (RightType rightType in Enum.GetValues(typeof(RightType)).Cast<RightType>())
                        {
                            pm.CreateDataPermission(user.Id, 1, ds.Id, rightType);
                        }
                    }

                }
                else
                {
                    datasetId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASET_ID]);
                }

                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                if (dm.IsDatasetCheckedOutFor(datasetId, GetUsernameOrDefault()) || dm.CheckOutDataset(datasetId, GetUsernameOrDefault()))
                {
                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(datasetId);

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
                    {
                        XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                        workingCopy.Metadata = XmlMetadataWriter.ToXmlDocument(xMetadata);
                    }

                    string title = XmlDatasetHelper.GetInformation(workingCopy, AttributeNames.title);
                    if(String.IsNullOrEmpty(title)) title = "No Title available.";

                    TaskManager.AddToBus(CreateDatasetTaskmanager.DATASET_TITLE, title );//workingCopy.Metadata.SelectNodes("Metadata/Description/Description/Title/Title")[0].InnerText);
                    TaskManager.AddToBus(CreateDatasetTaskmanager.DATASET_ID, datasetId);

                    dm.EditDatasetVersion(workingCopy, null, null, null);
                    dm.CheckInDataset(datasetId, "Metadata was submited.", GetUsernameOrDefault());

                    //add to index
                    // ToDo check which SearchProvider it is, default luceneprovider
                    ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
                    provider?.UpdateSingleDatasetIndex(datasetId, IndexingAction.CREATE);

                }

                return datasetId;
            }

            #endregion

            return -1;
        }

        public ActionResult StartUploadWizard()
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            BExIS.Dcm.UploadWizard.DataStructureType type = new BExIS.Dcm.UploadWizard.DataStructureType();

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASTRUCTURE_TYPE))
            {
                type = (BExIS.Dcm.UploadWizard.DataStructureType)TaskManager.Bus[CreateDatasetTaskmanager.DATASTRUCTURE_TYPE];
            }

            long datasetid = 0;
            // set parameters for upload process to pass it with the action
            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASET_ID))
            {
                datasetid = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASET_ID]);
            }

            Session["CreateDatasetTaskmanager"] = null;
            TaskManager = null;

            return RedirectToAction("UploadWizard", "Submit", new { area = "DCM", type = type , datasetid = datasetid });
  
        }

        public ActionResult ShowData(long id)
        {
            return RedirectToAction("ShowData", "Data", new { area="DDM" , id = id } );
        }

        private DataStructureType GetDataStructureType(long id)
        {
            DataStructureManager dataStructuremanager = new DataStructureManager();
            DataStructure dataStructure = dataStructuremanager.AllTypesDataStructureRepo.Get(id);

            if (dataStructure is StructuredDataStructure)
            {
                return DataStructureType.Structured;
            }

            if (dataStructure is UnStructuredDataStructure)
            {
                return DataStructureType.Unstructured;
            }

            return DataStructureType.Structured;
        }

        public ActionResult Cancel()
        {
            //public ActionResult LoadMetadata(long datasetId, bool locked = false, bool created = false, bool fromEditMode = false, bool resetTaskManager = false, XmlDocument newMetadata = null)

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager != null)
            {
                DatasetManager dm = new DatasetManager();
                long datasetid = -1;
                bool resetTaskManager = true;
                XmlDocument metadata = null;
                bool editmode = false;
                bool created = false;

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASET_ID))
                {
                    datasetid = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASET_ID]);
                }

                if (datasetid > -1 && dm.IsDatasetCheckedIn(datasetid))
                {
                    metadata = dm.GetDatasetLatestMetadataVersion(datasetid);
                    editmode = true;
                    created = true;
                }

                return RedirectToAction("LoadMetadata", "CreateDataset", new { area = "DDM", datasetid = datasetid, created = created, locked = true, fromEditMode = editmode, resetTaskManager = resetTaskManager, newMetadata = metadata });
            }

            return RedirectToAction("StartMetadataEditor", "CreateDataset");
        }

        public ActionResult Reset()
        {
            //public ActionResult LoadMetadata(long datasetId, bool locked = false, bool created = false, bool fromEditMode = false, bool resetTaskManager = false, XmlDocument newMetadata = null)

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager != null)
            {
                DatasetManager dm = new DatasetManager();
                long datasetid = -1;
                bool resetTaskManager = true;
                XmlDocument metadata = null;
                bool editmode = false;
                bool created = false;

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASET_ID))
                {
                    datasetid = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASET_ID]);
                }

                if (datasetid > -1 && dm.IsDatasetCheckedIn(datasetid))
                {
                    metadata = dm.GetDatasetLatestMetadataVersion(datasetid);
                    editmode = true;
                    created = true;
                }

                return RedirectToAction("LoadMetadata", "CreateDataset", new { area = "DDM", datasetid = datasetid, locked = false, created = created, fromEditMode = editmode, resetTaskManager = resetTaskManager, newMetadata = metadata });
            }

            return RedirectToAction("StartMetadataEditor", "CreateDataset");
        }

        #endregion

        #region Validation

        public ActionResult Validate()
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager != null && TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                List<StepModelHelper> stepInfoModelHelpers = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];
                ValidateModels(stepInfoModelHelpers);
            }


            return RedirectToAction("ReloadMetadataEditor", "CreateDataset");
        }

        private void ValidateModels(List<StepModelHelper> stepModelHelpers)
        {
            foreach (StepModelHelper stepModeHelper in stepModelHelpers)
            {
                // if model exist then validate attributes
                if (stepModeHelper.Model != null)
                {
                    foreach (var metadataAttrModel in stepModeHelper.Model.MetadataAttributeModels)
                    {

                        metadataAttrModel.Errors = validateAttribute(metadataAttrModel);
                        //if (metadataAttrModel.Errors.Count > 0)
                        //    step.stepStatus = StepStatus.error;
                    }
                }
                // else check for required elements 
                else
                {
                    stepModeHelper.Usage = LoadUsage(stepModeHelper.Usage);
                    if (UsageHelper.HasUsagesWithSimpleType(stepModeHelper.Usage))
                    {
                        //foreach (var metadataAttrModel in stepModeHelper.Model.MetadataAttributeModels)
                        //{
                        //    metadataAttrModel.Errors = validateAttribute(metadataAttrModel);
                        //    if (metadataAttrModel.Errors.Count>0)
                        //        step.stepStatus = StepStatus.error;
                        //}

                        //if(UsageHelper.HasRequiredSimpleTypes(stepModeHelper.Usage))
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

        //XX number of index des values nötig
        [HttpPost]
        public ActionResult ValidateMetadataAttributeUsage(string value, int id, int parentid, string parentname, int number, int parentModelNumber, int parentStepId)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            StepModelHelper stepModelHelper = GetStepModelhelper(parentStepId);

            long ParentUsageId = stepModelHelper.Usage.Id;
            BaseUsage parentUsage = LoadUsage(stepModelHelper.Usage);
            int pNumber = stepModelHelper.Number;

            BaseUsage metadataAttributeUsage = UsageHelper.GetChildren(parentUsage).Where(u => u.Id.Equals(id)).FirstOrDefault();

            //UpdateXml
            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
            MetadataAttributeModel model = MetadataAttributeModel.Convert(metadataAttributeUsage, parentUsage, metadataStructureId, parentModelNumber, stepModelHelper.StepId);
            model.Value = value;
            model.Number = number;

            UpdateAttribute(parentUsage, parentModelNumber, metadataAttributeUsage, number, value);

            if (stepModelHelper.Model.MetadataAttributeModels.Where(a => a.Id.Equals(id) && a.Number.Equals(number)).Count() > 0)
            {
                MetadataAttributeModel selectedMetadatAttributeModel = stepModelHelper.Model.MetadataAttributeModels.Where(a => a.Id.Equals(id) && a.Number.Equals(number)).FirstOrDefault();
                // select the attributeModel and change the value
                selectedMetadatAttributeModel.Value = model.Value;
                selectedMetadatAttributeModel.Errors = validateAttribute(selectedMetadatAttributeModel);
                return PartialView("_metadataAttributeView", selectedMetadatAttributeModel);
            }
            else
            {
                stepModelHelper.Model.MetadataAttributeModels.Add(model);
                return PartialView("_metadataAttributeView", model);
            }

            return null;

        }

        private List<Error> validateStep(AbstractMetadataStepModel pModel)
        {
            List<Error> errorList = new List<Error>();

            if (pModel != null)
            {
                foreach (MetadataAttributeModel m in pModel.MetadataAttributeModels)
                {
                    List<Error> temp = validateAttribute(m);
                    if (temp != null)
                        errorList.AddRange(temp);
                }
            }

            if (errorList.Count == 0)
                return null;
            else
                return errorList;
        }

        private List<Error> validateAttribute(MetadataAttributeModel aModel)
        {

            List<Error> errors = new List<Error>();
            //optional check
            if (aModel.MinCardinality > 0 && aModel.Value == null)
                errors.Add(new Error(ErrorType.MetadataAttribute, "is not optional", new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));
            else
                if (aModel.MinCardinality > 0 && String.IsNullOrEmpty(aModel.Value.ToString()))
                    errors.Add(new Error(ErrorType.MetadataAttribute, "is not optional", new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));

            //check datatype
            if (aModel.Value != null && !String.IsNullOrEmpty(aModel.Value.ToString()))
            {
                if (!DataTypeUtility.IsTypeOf(aModel.Value, aModel.SystemType))
                {
                    errors.Add(new Error(ErrorType.MetadataAttribute, "Value can´t convert to the type: " + aModel.SystemType + ".", new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));
                }
                else
                {
                    Type type = Type.GetType("System." + aModel.SystemType);
                    object value = Convert.ChangeType(aModel.Value, type);

                    // check Constraints
                    foreach (Constraint constraint in aModel.GetMetadataAttribute().Constraints)
                    {
                        if (value != null && !constraint.IsSatisfied(value))
                        {
                            errors.Add(new Error(ErrorType.MetadataAttribute, constraint.ErrorMessage, new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));
                        }
                    }
                }

            }

            
            if (errors.Count == 0)
                return null;
            else
                return errors;
        }

        #endregion

        #region Xml

        #region Xml Add / Remove / Update

            private void AddPackageToXml(BaseUsage usage, int number, string xpath)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);


                metadataXml = xmlMetadataWriter.AddPackage(
                    metadataXml,
                    usage,
                    number,
                    UsageHelper.GetNameOfType(usage),
                    UsageHelper.GetIdOfType(usage),
                    UsageHelper.GetChildren(usage),
                    BExIS.Xml.Helpers.XmlNodeType.MetadataPackage,
                    BExIS.Xml.Helpers.XmlNodeType.MetadataPackageUsage,
                    xpath);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;

            }

            private void AddCompoundAttributeToXml(BaseUsage usage, int number, string xpath)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);


                metadataXml = xmlMetadataWriter.AddPackage(metadataXml, usage, number, UsageHelper.GetNameOfType(usage), UsageHelper.GetIdOfType(usage), UsageHelper.GetChildren(usage), BExIS.Xml.Helpers.XmlNodeType.MetadataAttribute, BExIS.Xml.Helpers.XmlNodeType.MetadataAttributeUsage, xpath);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;

            }

            private void UpdateCompoundAttributeToXml(BaseUsage usage, int number, string xpath)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);


                metadataXml = xmlMetadataWriter.AddPackage(metadataXml, usage, number, UsageHelper.GetNameOfType(usage), UsageHelper.GetIdOfType(usage), UsageHelper.GetChildren(usage), BExIS.Xml.Helpers.XmlNodeType.MetadataAttribute, BExIS.Xml.Helpers.XmlNodeType.MetadataAttributeUsage, xpath);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;

            }

            private void RemoveCompoundAttributeToXml(BaseUsage usage, int number)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                metadataXml = xmlMetadataWriter.RemovePackage(metadataXml, usage, number, UsageHelper.GetNameOfType(usage));

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;
            }

            private void RemoveFromXml(string xpath)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                metadataXml = xmlMetadataWriter.Remove(metadataXml, xpath);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;
            }

            private void AddAttributeToXml(BaseUsage parentUsage, int parentNumber, BaseUsage attribute, int number)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                metadataXml = xmlMetadataWriter.AddAttribute(metadataXml, parentUsage, parentNumber, attribute, number, UsageHelper.GetNameOfType(parentUsage), UsageHelper.GetNameOfType(attribute), UsageHelper.GetIdOfType(attribute).ToString());

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;

                // locat path
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
                //metadataXml.Save
            }

            private void RemoveAttributeToXml(BaseUsage parentUsage, int packageNumber, BaseUsage attribute, int number, string metadataAttributeName)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

                metadataXml = xmlMetadataWriter.RemoveAttribute(metadataXml, parentUsage, packageNumber, attribute, number, UsageHelper.GetNameOfType(parentUsage), metadataAttributeName);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;
                // locat path
                //string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
                //metadataXml.Save
            }

            private void UpdateAttribute(BaseUsage parentUsage, int packageNumber, BaseUsage attribute, int number, object value)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

                metadataXml = xmlMetadataWriter.Update(metadataXml, parentUsage, packageNumber, attribute, number, value, UsageHelper.GetNameOfType(parentUsage), UsageHelper.GetNameOfType(attribute));

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;
                // locat path
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
                metadataXml.Save(path);
            }

            private void ChangeInXml(string selectedXPath, string destinationXPath)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

                metadataXml = xmlMetadataWriter.Change(metadataXml, selectedXPath, destinationXPath);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;
                // locat path
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
                metadataXml.Save(path);
            }   

        #endregion
        
        #region create Models from xml

        #endregion

        #endregion

        #region Attribute

        private StepModelHelper UpdateChildrens(StepModelHelper stepModelHelper)
        {
            int count = stepModelHelper.Model.MetadataAttributeModels.Count;

            for (int i = 0; i < count; i++)
            {
                MetadataAttributeModel child = stepModelHelper.Model.MetadataAttributeModels.ElementAt(i);
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
            IEnumerable<MetadataAttributeModel> mams = stepModelHelper.Model.MetadataAttributeModels.Where(m=>m.Source.Id.Equals(usageId));

            for (int i = 0; i < mams.Count(); i++)
            {
                MetadataAttributeModel child = mams.ElementAt(i);
                child.NumberOfSourceInPackage = mams.Count();
                child.Number = i + 1;
            }

            mams = UpdateFirstAndLast(mams.ToList()).AsEnumerable();

            return stepModelHelper;
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
            MetadataAttributeModel childrensWithSameUsage = stepModelHelperParent.Model.MetadataAttributeModels.Where(m => m.Source.Id.Equals(childModel.Source.Id)).First();
            int indexOfFirstUsage = stepModelHelperParent.Model.MetadataAttributeModels.IndexOf(childrensWithSameUsage);

            stepModelHelperParent.Model.MetadataAttributeModels.Insert(indexOfFirstUsage + number - 1, childModel);

            return stepModelHelperParent;
        }

        private StepModelHelper Up(StepModelHelper stepModelHelperParent, long id, int number)
        {
            List<MetadataAttributeModel> list = stepModelHelperParent.Model.MetadataAttributeModels;

            MetadataAttributeModel temp = list.Where(m => m.Id.Equals(id) && m.Number.Equals(number)).FirstOrDefault();
            int index = list.IndexOf(temp);

            list.RemoveAt(index);
            list.Insert(index -1, temp);

            return stepModelHelperParent;
        }

        private StepModelHelper Down(StepModelHelper stepModelHelperParent, long id, int number)
        {
            List<MetadataAttributeModel> list = stepModelHelperParent.Model.MetadataAttributeModels;

            MetadataAttributeModel temp = list.Where(m => m.Id.Equals(id) && m.Number.Equals(number)).FirstOrDefault();
            int index = list.IndexOf(temp);

            list.RemoveAt(index);
            list.Insert(index+number, temp);

            return stepModelHelperParent;
        }

        private StepModelHelper UpComplexModel(StepModelHelper stepModelHelperParent, long id, int number)
        {
            List<StepModelHelper> list = stepModelHelperParent.Childrens;


            return stepModelHelperParent;
        }

        private StepModelHelper DownComplexModel(StepModelHelper stepModelHelperParent, long id, int number)
        {
            List<StepModelHelper> list = stepModelHelperParent.Childrens;

            return stepModelHelperParent;
        }

        #endregion

        #region Helper

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
        public string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        private StepModelHelper AddStepModelhelper(StepModelHelper stepModelHelper)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                ((List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER]).Add(stepModelHelper);

                return stepModelHelper;
            }

            return stepModelHelper;
        }

        private StepModelHelper GetStepModelhelper(int stepId)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                return ((List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER]).Where(s => s.StepId.Equals(stepId)).FirstOrDefault();
            }

            return null;
        }

        private StepModelHelper GetStepModelhelper(long usageId, int number)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                return ((List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER]).Where(s => s.Usage.Id.Equals(usageId) && s.Number.Equals(number)).FirstOrDefault();
            }

            return null;
        }

        private int GetNumberOfUsageInStepModelHelper(long usageId)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                return ((List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER]).Where(s => s.Usage.Id.Equals(usageId)).Count()-1;
            }

            return 0;
        }

        private void GenerateModelsForChildrens(StepModelHelper modelHelper, long metadataStructureId)
        {
            foreach (StepModelHelper item in modelHelper.Childrens)
            {
                if (item.Childrens.Count() > 0)
                {
                    GenerateModelsForChildrens(item, metadataStructureId);
                }

                if (item.Model == null)
                {
                    BaseUsage u = LoadUsage(item.Usage);
                    if (u is MetadataAttributeUsage || u is MetadataNestedAttributeUsage)
                    {
                        item.Model = MetadataCompoundAttributeModel.ConvertToModel(u, item.Number);
                        ((MetadataCompoundAttributeModel)item.Model).ConvertMetadataAttributeModels(u, metadataStructureId, item.StepId);
                    }

                    if (u is MetadataPackageUsage)
                    {
                        item.Model = MetadataPackageModel.Convert(u, item.Number);
                        ((MetadataPackageModel)item.Model).ConvertMetadataAttributeModels(u, metadataStructureId, item.StepId);
                    }
                }
            }
            
        }

        private StepModelHelper GenerateModelsForChildrens(StepInfo stepInfo, long metadataStructureId)
        {
            StepModelHelper stepModelHelper = GetStepModelhelper(stepInfo.Id);

            if (stepModelHelper.Model == null)
            {
                if (stepModelHelper.Usage is MetadataPackageUsage)
                    stepModelHelper.Model = CreatePackageModel(stepInfo.Id, false);

                if (stepModelHelper.Usage is MetadataNestedAttributeUsage)
                    stepModelHelper.Model = CreateCompoundModel(stepInfo.Id, false);

                getChildModelsHelper(stepModelHelper);
            }

            return stepModelHelper;
        }

        public List<ListViewItem> LoadMetadataStructureViewList()
        {
            MetadataStructureManager msm = new MetadataStructureManager();
            List<ListViewItem> temp = new List<ListViewItem>();

            foreach (MetadataStructure metadataStructure in msm.Repo.Get())
            {
                string title = metadataStructure.Name;

                temp.Add(new ListViewItem(metadataStructure.Id, title, metadataStructure.Description));
            }

            return temp.OrderBy(p => p.Title).ToList();
        }

        public List<ListViewItemWithType> LoadDataStructureViewList()
        {
            DataStructureManager dsm = new DataStructureManager();
            List<ListViewItemWithType> temp = new List<ListViewItemWithType>();

            foreach (DataStructure dataStructure in dsm.AllTypesDataStructureRepo.Get())
            {
                string title = dataStructure.Name;
                string type = "";
                if (dataStructure is StructuredDataStructure)
                {
                    type = "structured";
                }

                if (dataStructure is UnStructuredDataStructure)
                {
                    type = "unstructured";
                }

                temp.Add(new ListViewItemWithType(dataStructure.Id, title, dataStructure.Description, type));
            }

            return temp.OrderBy(p => p.Title).ToList();
        }

        public List<ListViewItem> LoadDatasetViewList()
        {
            PermissionManager pm = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();
            DatasetManager datasetManager = new DatasetManager();
            List<ListViewItem> temp = new List<ListViewItem>();

            //get all datasetsid where the current userer has access to
            long userid = -1;
            if (subjectManager.ExistsUsername(GetUsernameOrDefault()))
                userid = subjectManager.GetUserByName(GetUsernameOrDefault()).Id;

            if (userid != -1)
            {
                foreach (long id in pm.GetAllDataIds(userid, 1, RightType.Update))
                {
                    if (datasetManager.IsDatasetCheckedIn(id))
                    {
                        string title = XmlDatasetHelper.GetInformation(id, AttributeNames.title);
                        string description = XmlDatasetHelper.GetInformation(id, AttributeNames.description);

                        temp.Add(new ListViewItem(id, title, description));
                    }
                }
            }

            return temp.OrderBy(p => p.Title).ToList();
        }

        private BaseUsage LoadUsage(BaseUsage usage)
        {
            if (usage is MetadataPackageUsage)
            {
                MetadataStructureManager msm = new MetadataStructureManager();
                return msm.PackageUsageRepo.Get(usage.Id);
            }

            if (usage is MetadataNestedAttributeUsage)
            {
                MetadataAttributeManager mam = new MetadataAttributeManager();

                var x = from c in mam.MetadataCompoundAttributeRepo.Get()
                        from u in c.Self.MetadataNestedAttributeUsages
                        where u.Id == usage.Id //&& c.Id.Equals(parentId)
                        select u;

                return x.FirstOrDefault();
            }

            if (usage is MetadataAttributeUsage)
            {
                MetadataPackageManager mpm = new MetadataPackageManager();

                var q = from p in mpm.MetadataPackageRepo.Get()
                        from u in p.MetadataAttributeUsages
                        where u.Id == usage.Id // && p.Id.Equals(parentId)
                        select u;

                return q.FirstOrDefault();
            }


            return usage;
        }

        #endregion

        #region Load & Update advanced steps

        private StepInfo AddStepsBasedOnUsage(BaseUsage usage, StepInfo current, string parentXpath)
        {
            // genertae action, controller base on usage
            string actionName = "";
            string childName = "";
            int min = usage.MinCardinality;

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

            List<StepInfo> list = new List<StepInfo>();
            List<StepModelHelper> stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
            {
                XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                var x = new XElement("null");
                List<XElement> elements = new List<XElement>();

                Dictionary<string, string> keyValueDic = new Dictionary<string, string>();
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
                    IEnumerable<XElement> xelements = x.Elements();

                    if (xelements.Count() > 0)
                    {
                        int counter = 0;

                        foreach (XElement element in xelements)
                        {
                            counter++;
                            string title = counter.ToString(); //usage.Label+" (" + counter + ")";
                            long id = Convert.ToInt64((element.Attribute("roleId")).Value.ToString());

                            StepInfo s = new StepInfo(title)
                            {
                                Id = TaskManager.GenerateStepId(),
                                Parent = current,
                                IsInstanze = true,
                                HasContent = UsageHelper.HasUsagesWithSimpleType(usage),
                             };

                            string xPath = parentXpath + "//" + childName.Replace(" ", string.Empty) + "[" + counter + "]";

                            s.Children = GetChildrenSteps(usage, s, xPath);

                            if (s.Children.Count == 0)
                            {

                            }

                            current.Children.Add(s);

                            if (TaskManager.Root.Children.Where(z => z.title.Equals(title)).Count() == 0)
                            {
                                stepHelperModelList.Add(new StepModelHelper(s.Id, counter, usage, xPath));
                            }

                        }
                    }
                }

                //TaskManager.AddToBus(CreateDatasetTaskmanager.METADATAPACKAGE_IDS, MetadataPackageDic);
            }
            return current;
        }

        private List<StepInfo> GetChildrenSteps(BaseUsage usage, StepInfo parent, string parentXpath)
        {
            List<StepInfo> childrenSteps = new List<StepInfo>();
            List<BaseUsage> childrenUsages = UsageHelper.GetCompoundChildrens(usage);
            List<StepModelHelper> stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            if (childrenUsages.Count > 0)
            {
                foreach (BaseUsage u in childrenUsages)
                {

                    string xPath = parentXpath + "//" + u.Label.Replace(" ", string.Empty) + "[1]";

                    bool complex = false;

                    string actionName = "";
                    string attrName = "";

                    if (u is MetadataPackageUsage)
                    {
                        actionName = "SetMetadataPackage";
                    }
                    else
                    {
                        actionName = "SetMetadataCompoundAttribute";

                        if (u is MetadataAttributeUsage)
                        {
                            MetadataAttributeUsage mau = (MetadataAttributeUsage)u;
                            if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                            {
                                complex = true;
                                attrName = mau.MetadataAttribute.Self.Name;
                            }
                        }

                        if (u is MetadataNestedAttributeUsage)
                        {
                            MetadataNestedAttributeUsage mau = (MetadataNestedAttributeUsage)u;
                            if (mau.Member.Self is MetadataCompoundAttribute)
                            {
                                complex = true;
                                attrName = mau.Member.Self.Name;
                            }
                        }

                    }

                    if (complex)
                    {
                        StepInfo s = new StepInfo(u.Label)
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
                        s = AddStepsBasedOnUsage(u, s, xPath);
                        childrenSteps.Add(s);

                        if (TaskManager.StepInfos.Where(z => z.Id.Equals(s.Id)).Count() == 0)
                        {
                            // MetadataPackageDic.Add(s.Id, u.Id);
                            stepHelperModelList.Add(new StepModelHelper(s.Id, 1, u, xPath));
                        }
                    }
                    //}

                }
            }


            return childrenSteps;
        }

        private StepInfo LoadStepsBasedOnUsage(BaseUsage usage, StepInfo current, string parentXpath)
        {
            // genertae action, controller base on usage
            string actionName = "";
            string childName = "";
            int min = usage.MinCardinality;

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

            List<StepInfo> list = new List<StepInfo>();
            List<StepModelHelper> stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
            {
                XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                //var x = new XElement("null");
                List<XElement> elements = new List<XElement>();

                Dictionary<string, string> keyValueDic = new Dictionary<string, string>();
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

                foreach (var x in elements)
                {
                    
                
                if (x != null && !x.Name.Equals("null"))
                {
                    IEnumerable<XElement> xelements = x.Elements();

                    if (xelements.Count() > 0)
                    {
                        int counter = 0;

                        XElement last = null;

                        foreach (XElement element in xelements)
                        {
                            // if the last has not the same name reset count
                            if (last != null && !last.Name.Equals(element.Name))
                            {
                                counter = 0;
                            }

                            last = element;
                            counter++;
                            string title = counter.ToString(); //usage.Label+" (" + counter + ")";
                            long id = Convert.ToInt64((element.Attribute("roleId")).Value.ToString());

                            StepInfo s = new StepInfo(title)
                            {
                                Id = TaskManager.GenerateStepId(),
                                Parent = current,
                                IsInstanze = true,
                                HasContent = UsageHelper.HasUsagesWithSimpleType(usage),
                                
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

                            string xPath = parentXpath + "//" + childName.Replace(" ", string.Empty) + "[" + counter + "]";

                            s.Children = GetChildrenStepsFromMetadata(usage, s, xPath);

                            if (s.Children.Count == 0)
                            {

                            }

                            current.Children.Add(s);

                            if (TaskManager.Root.Children.Where(z => z.title.Equals(title)).Count() == 0)
                            {
                                stepHelperModelList.Add(new StepModelHelper(s.Id, counter, usage, xPath));
                                //Debug.WriteLine(xPath + " : " + s.Id + " c:" + counter);
                            }

                        }
                    }
                }
            }

                //TaskManager.AddToBus(CreateDatasetTaskmanager.METADATAPACKAGE_IDS, MetadataPackageDic);
            }
            return current;
        }

        private List<StepInfo> GetChildrenStepsFromMetadata(BaseUsage usage, StepInfo parent, string parentXpath)
        {
            List<StepInfo> childrenSteps = new List<StepInfo>();
            List<BaseUsage> childrenUsages = UsageHelper.GetCompoundChildrens(usage);
            List<StepModelHelper> stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            if (childrenUsages.Count > 0)
            {

                foreach (BaseUsage u in childrenUsages)
                {

                    int number = 1;//childrenUsages.IndexOf(u) + 1;
                    string xPath = parentXpath + "//" + u.Label.Replace(" ", string.Empty) + "["+ number +"]";

                    bool complex = false;

                    string actionName = "";
                    string attrName = "";

                    if (u is MetadataPackageUsage)
                    {
                        actionName = "SetMetadataPackage";
                    }
                    else
                    {
                        actionName = "SetMetadataCompoundAttribute";

                        if (u is MetadataAttributeUsage)
                        {
                            MetadataAttributeUsage mau = (MetadataAttributeUsage)u;
                            if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                            {
                                complex = true;
                                attrName = mau.MetadataAttribute.Self.Name;
                            }
                        }

                        if (u is MetadataNestedAttributeUsage)
                        {
                            MetadataNestedAttributeUsage mau = (MetadataNestedAttributeUsage)u;
                            if (mau.Member.Self is MetadataCompoundAttribute)
                            {
                                complex = true;
                                attrName = mau.Member.Self.Name;
                            }
                        }

                    }

                    if (complex)
                    {
                        StepInfo s = new StepInfo(u.Label)
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
                        s = LoadStepsBasedOnUsage(u, s, xPath);
                        childrenSteps.Add(s);

                        if (TaskManager.StepInfos.Where(z => z.Id.Equals(s.Id)).Count() == 0)
                        {
                            // MetadataPackageDic.Add(s.Id, u.Id);
                            stepHelperModelList.Add(new StepModelHelper(s.Id, 1, u, xPath));
                        }
                    }
                    //}

                }
            }


            return childrenSteps;
        }

        private List<StepInfo> UpdateStepsBasedOnUsage(BaseUsage usage, StepInfo currentSelected, string parentXpath)
        {

            // genertae action, controller base on usage
            string actionName = "";
            string childName = "";
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

            List<StepInfo> list = new List<StepInfo>();
            List<StepModelHelper> stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
            {
                XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                var x = XmlUtility.GetXElementByXPath(parentXpath, xMetadata);

                if (x != null && !x.Name.Equals("null"))
                {

                    StepInfo current = currentSelected;
                    IEnumerable<XElement> xelements = x.Elements();

                    if (xelements.Count() > 0)
                    {
                        int counter = 0;
                        foreach (XElement element in xelements)
                        {
                            counter++;
                            string title = counter.ToString();

                            if(current.Children.Where(s=>s.title.Equals(title)).Count()==0)
                            {
                                long id = Convert.ToInt64((element.Attribute("roleId")).Value.ToString());

                                StepInfo s = new StepInfo(title)
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

                                string xPath = parentXpath + "//" + childName.Replace(" ",string.Empty) + "[" + counter + "]";

                                s.Children = GetChildrenStepsUpdated(usage, s, xPath);
                                list.Add(s);

                                if (TaskManager.Root.Children.Where(z => z.Id.Equals(s.Id)).Count() == 0)
                                {
                                    StepModelHelper newStepModelHelper = new StepModelHelper(s.Id, counter, usage, xPath);

                                    stepHelperModelList.Add(newStepModelHelper);
                                }
                            }//end if
                        }//end foreach
                    }//end if
                }

            }
            return list;
        }

        private List<StepInfo> GetChildrenStepsUpdated(BaseUsage usage, StepInfo parent, string parentXpath)
            {
                List<StepInfo> childrenSteps = new List<StepInfo>();
                List<BaseUsage> childrenUsages = UsageHelper.GetCompoundChildrens(usage);
                List<StepModelHelper> stepHelperModelList = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

                foreach (BaseUsage u in childrenUsages)
                {
                    string label = u.Label.Replace(" ", string.Empty);
                    string xPath = parentXpath + "//" + label + "[1]";
                    bool complex = false;

                    string actionName = "";

                    if (u is MetadataPackageUsage)
                    {
                        actionName = "SetMetadataPackage";
                    }
                    else
                    {
                        actionName = "SetMetadataCompoundAttribute";

                        if (u is MetadataAttributeUsage)
                        {
                            MetadataAttributeUsage mau = (MetadataAttributeUsage)u;
                            if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                                complex = true;
                        }

                        if (u is MetadataNestedAttributeUsage)
                        {
                            MetadataNestedAttributeUsage mau = (MetadataNestedAttributeUsage)u;
                            if (mau.Member.Self is MetadataCompoundAttribute)
                                complex = true;
                        }

                    }

                    if (complex)
                    {
                        StepInfo s = new StepInfo(u.Label)
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
                            stepHelperModelList.Add(new StepModelHelper(s.Id, 1, u, xPath));
                        }
                    }

                }

                return childrenSteps;
            }

        #endregion

        #region Bus Functions

        private List<MetadataAttributeModel> UpdateFirstAndLast(List<MetadataAttributeModel> list)
        {
            foreach (MetadataAttributeModel x in list)
            {
                if (list.First().Equals(x)) x.first = true;
                else x.first = false;

                if (list.Last().Equals(x)) x.last = true;
                else x.last = false;
            }


            return list;
        }

        private long GetUsageId(int stepId)
        {

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER))
            {
                List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];
                return list.Where(s => s.StepId.Equals(stepId)).FirstOrDefault().Usage.Id;

            }

            return 0;

        }

        private BaseUsage GetMetadataCompoundAttributeUsage(long id)
        {
            //return UsageHelper.GetChildrenUsageById(Id);
            return UsageHelper.GetMetadataAttributeUsageById(id);
        }

        private BaseUsage GetPackageUsage(long Id)
        {
            MetadataStructureManager mpm = new MetadataStructureManager();
            return mpm.PackageUsageRepo.Get(Id);
        }

        #endregion

    }
}
