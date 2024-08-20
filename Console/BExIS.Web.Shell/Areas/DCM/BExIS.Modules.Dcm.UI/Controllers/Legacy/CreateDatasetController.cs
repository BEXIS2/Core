using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Modules.Dcm.UI.Models.CreateDataset;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Extensions;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Entities.Common;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class CreateDatasetController : BaseController
    {
        private CreateTaskmanager TaskManager;
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

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
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Dataset", this.Session.GetTenant());

            Session["CreateDatasetTaskmanager"] = null;
            if (TaskManager == null) TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager == null)
            {
                TaskManager = new CreateTaskmanager();

                Session["CreateDatasetTaskmanager"] = TaskManager;
                Session["MetadataStructureViewList"] = LoadMetadataStructureViewList();
                var datastructureViewList = LoadDataStructureViewList();
                Session["DataStructureViewList_unstructured"] = datastructureViewList.Where(a => a.Type == "unstructured").ToList();
                Session["DataStructureViewList_structured"] = datastructureViewList.Where(a => a.Type == "structured").ToList();

                Session["DatasetViewList"] = LoadDatasetViewList();

                setAdditionalFunctions();

                //set Entity to TaskManager
                TaskManager.AddToBus(CreateTaskmanager.ENTITY_CLASS_PATH, "BExIS.Dlm.Entities.Data.Dataset");

                SetupModel Model = GetDefaultModel();

                //if id is set and its type dataset
                if (id != -1 && type.ToLower().Equals("datasetid"))
                {
                    ViewBag.Title = PresentationModel.GetViewTitleForTenant("Copy Dataset", this.Session.GetTenant());

                    Dataset dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(id);
                    Model.SelectedDatasetId = id;
                    Model.SelectedMetadataStructureId = dataset.MetadataStructure.Id;
                    Model.SelectedDataStructureId = dataset.DataStructure.Id;
                    Model.BlockMetadataStructureId = true;
                    Model.BlockDatasetId = true;
                }

                if (id != -1 && type.ToLower().Equals("metadatastructureid"))
                {
                    ViewBag.Title = PresentationModel.GetViewTitleForTenant("Copy Dataset", this.Session.GetTenant());
                    Model.SelectedMetadataStructureId = id;
                }

                if (id != -1 && type.ToLower().Equals("datastructureid"))
                {
                    ViewBag.Title = PresentationModel.GetViewTitleForTenant("Copy Dataset", this.Session.GetTenant());
                    Model.SelectedDataStructureId = id;
                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                        Model.SelectedMetadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);
                }

                return View(Model);
            }

            return View();
        }

        /// <summary>
        /// ReLoad the createDataset action with different parameter type options
        /// type eg ("DataStructureId", "DatasetId", "MetadataStructureId")
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult ReloadIndex(long id = -1, string type = "")
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("...", this.Session.GetTenant());

            if (TaskManager == null) TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            using (DatasetManager datasetManager = new DatasetManager())
            {
                SetupModel Model = GetDefaultModel();

                //if id is set and its type dataset
                if (id != -1 && type.ToLower().Equals("datasetid"))
                {
                    Dataset dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(id);
                    Model.SelectedDatasetId = id;
                    Model.SelectedMetadataStructureId = dataset.MetadataStructure.Id;
                    Model.SelectedDataStructureId = dataset.DataStructure.Id;
                    Model.BlockMetadataStructureId = true;
                    Model.BlockDatasetId = false;

                    TaskManager.AddToBus(CreateTaskmanager.COPY_OF_ENTITY_ID, id);
                }

                if (id != -1 && type.ToLower().Equals("metadatastructureid"))
                {
                    TaskManager.AddToBus(CreateTaskmanager.METADATASTRUCTURE_ID, id);
                    Model.SelectedMetadataStructureId = id;

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.DATASTRUCTURE_ID))
                        Model.SelectedDataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.DATASTRUCTURE_ID]);

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.COPY_OF_ENTITY_ID))
                    {
                        Model.SelectedDatasetId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.COPY_OF_ENTITY_ID]);

                        Dataset dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(id);
                        Model.BlockMetadataStructureId = true;
                        Model.BlockDatasetId = false;
                    }
                }

                if (id != -1 && type.ToLower().Equals("datastructureid"))
                {
                    TaskManager.AddToBus(CreateTaskmanager.DATASTRUCTURE_ID, id);
                    Model.SelectedDataStructureId = id;

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                        Model.SelectedMetadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.COPY_OF_ENTITY_ID))
                    {
                        Model.SelectedDatasetId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.COPY_OF_ENTITY_ID]);

                        Dataset dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(id);
                        Model.BlockMetadataStructureId = true;
                        Model.BlockDatasetId = false;
                    }
                }

                return View("Index", Model);
            }
        }

        [HttpPost]
        public ActionResult StoreSelectedDataset(long id)
        {
            if (TaskManager == null) TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            Dataset dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(id);

            SetupModel Model = GetDefaultModel();

            if (id == -1)
            {
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.DATASTRUCTURE_ID))
                    Model.SelectedDataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.DATASTRUCTURE_ID]);

                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                    Model.SelectedMetadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);
            }
            else
            {
                Model.SelectedDatasetId = id;
                Model.SelectedDataStructureId = dataset.DataStructure.Id;
                Model.SelectedMetadataStructureId = dataset.MetadataStructure.Id;

                Model.BlockMetadataStructureId = true;

                Model.DataStructureOptions = DataStructureOptions.CreateNewStructure;

                //add to Bus
                TaskManager.AddToBus(CreateTaskmanager.DATASTRUCTURE_ID, dataset.DataStructure.Id);
                TaskManager.AddToBus(CreateTaskmanager.METADATASTRUCTURE_ID, dataset.MetadataStructure.Id);
                TaskManager.AddToBus(CreateTaskmanager.COPY_OF_ENTITY_ID, dataset.Id);
            }

            return PartialView("Index", Model);
        }

        public ActionResult StoreSelectedDatasetSetup(SetupModel model)
        {
            CreateTaskmanager TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            using (DatasetManager datasetManager = new DatasetManager())
            using (DataStructureManager dataStructureManager = new DataStructureManager())
            using (ResearchPlanManager rpm = new ResearchPlanManager())
            {
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                string username = GetUsernameOrDefault();

                if (model.SelectedDataStructureId_ > 0)
                {
                    model.SelectedDataStructureId = model.SelectedDataStructureId_;
                }

                if (model == null)
                {
                    model = GetDefaultModel();
                    return PartialView("Index", model);
                }

                model = LoadLists(model);

                if ((model.DataStructureOptions == DataStructureOptions.Existing_structured || model.DataStructureOptions == DataStructureOptions.Existing_unstructured) && model.SelectedDataStructureId == 0)
                    ModelState.AddModelError("SelectedDataStructureId", "Please select a data structure.");

                if (ModelState.IsValid)
                {
                    // create new structure if its not exist
                    if (model.DataStructureOptions != DataStructureOptions.Existing_structured && model.DataStructureOptions != DataStructureOptions.Existing_unstructured)
                    {
                        using (PartyManager partyManager = new PartyManager())
                        using (var identityUserService = new IdentityUserService())
                        {
                            var user = identityUserService.FindByNameAsync(username);

                            var name = "New data structure_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm");

                            // Replace account name by party name if exists
                            if (user != null)
                            {
                                Party party = partyManager.GetPartyByUser(user.Result.Id);
                                if (party != null)
                                {
                                    name = "New created for " + party.Name + "_" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm");
                                }
                            }

                            //create unstructured
                            if (model.DataStructureOptions == DataStructureOptions.CreateNewFile)
                            {
                                var d = dataStructureManager.CreateUnStructuredDataStructure(name, "");
                                if (d != null) model.SelectedDataStructureId = d.Id;
                            }

                            //create structured
                            if (model.DataStructureOptions == DataStructureOptions.CreateNewStructure)
                            {
                                var d = dataStructureManager.CreateStructuredDataStructure(name, "", "", "", DataStructureCategory.Generic);
                                if (d != null) model.SelectedDataStructureId = d.Id;
                            }

                            if (model.SelectedDataStructureId <= 0)
                            {
                                ModelState.AddModelError("DataStructureOptions", "It was not possible to create a data structure");
                                return View("Index", model);
                            }
                        }
                    }

                    //check combination of datatstructure options and data structure selection
                    TaskManager.AddToBus(CreateTaskmanager.METADATASTRUCTURE_ID, model.SelectedMetadataStructureId);
                    TaskManager.AddToBus(CreateTaskmanager.DATASTRUCTURE_ID, model.SelectedDataStructureId);

                    // set datastructuretype
                    TaskManager.AddToBus(CreateTaskmanager.DATASTRUCTURE_TYPE, GetDataStructureType(model.SelectedDataStructureId));

                    //dataset is selected
                    if (model.SelectedDatasetId != 0 && model.SelectedDatasetId != -1)
                    {
                        if (datasetManager.IsDatasetCheckedIn(model.SelectedDatasetId))
                        {
                            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(model.SelectedDatasetId);
                            TaskManager.AddToBus(CreateTaskmanager.RESEARCHPLAN_ID,
                                datasetVersion.Dataset.ResearchPlan.Id);
                            TaskManager.AddToBus(CreateTaskmanager.ENTITY_TITLE, datasetVersion.Title);

                            // set datastructuretype
                            TaskManager.AddToBus(CreateTaskmanager.DATASTRUCTURE_TYPE,
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
                        TaskManager.AddToBus(CreateTaskmanager.RESEARCHPLAN_ID, rpm.Repo.Get().First().Id);
                    }

                    return RedirectToAction("StartMetadataEditor", "Form");
                }

                return View("Index", model);
            }
        }

        [HttpPost]
        public ActionResult StoreSelectedOption(long id, string type)
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            string key = "";

            switch (type)
            {
                case "ms": key = CreateTaskmanager.METADATASTRUCTURE_ID; break;
                case "ds": key = CreateTaskmanager.DATASTRUCTURE_ID; break;
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

        #region setup parameter selection actions

        [HttpGet]
        public ActionResult ShowListOfDatasets()
        {
            List<ListViewItem> datasets = new List<ListViewItem>();

            // Load list from Session, if exists
            if (Session["DatasetViewList"] != null)
            {
                datasets = (List<ListViewItem>)Session["DatasetViewList"];
            }
            else
            {
                datasets = LoadDatasetViewList();
                Session["DatasetViewList"] = datasets;
            }

            EntitySelectorModel model = BexisModelManager.LoadEntitySelectorModel(
                datasets,
                new EntitySelectorModelAction("ShowListOfDatasetsReciever", "CreateDataset", "DCM"));

            model.Title = "Select a Dataset as Template";

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
                 datastructures, new List<string> { "Id", "Title", "Description", "Type" },
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

        #endregion setup parameter selection actions

        private SetupModel GetDefaultModel()
        {
            SetupModel model = new SetupModel();

            model = LoadLists(model);

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                model.SelectedMetadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                model.SelectedMetadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);

            model.BlockDatasetId = false;
            model.BlockDatastructureId = false;
            model.BlockMetadataStructureId = false;

            return model;
        }

        /// <summary>
        /// load all existing lists for this step
        /// if there are stored in the session
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private SetupModel LoadLists(SetupModel model)
        {
            if ((List<ListViewItem>)Session["MetadataStructureViewList"] != null) model.MetadataStructureViewList = (List<ListViewItem>)Session["MetadataStructureViewList"];
            if ((List<ListViewItemWithType>)Session["DataStructureViewList_unstructured"] != null) model.DataStructureViewList_unstructured = (List<ListViewItemWithType>)Session["DataStructureViewList_unstructured"];
            if ((List<ListViewItemWithType>)Session["DataStructureViewList_structured"] != null) model.DataStructureViewList_structured = (List<ListViewItemWithType>)Session["DataStructureViewList_structured"];
            if ((List<ListViewItem>)Session["DatasetViewList"] != null) model.DatasetViewList = (List<ListViewItem>)Session["DatasetViewList"];

            return model;
        }

        #endregion Create a Dataset Setup Page

        /// <summary>
        /// Store the incoming xmldocument in the bus of the Create TaskManager with
        /// the METADATA_XML key
        /// </summary>
        /// <param name="metadataXml"></param>
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

        #region Submit And Create And Finish And Cancel and Reset

        public JsonResult Submit(bool valid, string commitMessage)
        {
            try
            {
                // create and submit Dataset

                long datasetId = SubmitDataset(valid, "Dataset", commitMessage);

                return Json(new { result = "redirect", url = Url.Action("Show", "Data", new { area = "DDM", id = datasetId }) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Submit a Dataset based on the imformations
        /// in the CreateTaskManager
        /// </summary>
        public long SubmitDataset(bool valid, string entityname, string commitMessage = "")
        {
            #region create dataset

            // the entityname can be wrong due to the mixed use from different modules. If its an update its set explicite again in #setEntitynNameNew

            using (DatasetManager dm = new DatasetManager())
            using (DataStructureManager dsm = new DataStructureManager())
            using (ResearchPlanManager rpm = new ResearchPlanManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (EntityTemplateManager entityTemplateManager = new EntityTemplateManager())
            using (MetadataStructureManager msm = new MetadataStructureManager())
            {
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                string title = "";
                long datasetId = 0;
                bool newDataset = true;

                try
                {
                    TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                    {
                        // for e new dataset
                        if (!TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
                        {
                            long datastructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.DATASTRUCTURE_ID]);
                            long researchPlanId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.RESEARCHPLAN_ID]);
                            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);

                            DataStructure dataStructure = dsm.StructuredDataStructureRepo.Get(datastructureId);
                            //if datastructure is not a structured one
                            if (dataStructure == null) dataStructure = dsm.UnStructuredDataStructureRepo.Get(datastructureId);

                            ResearchPlan rp = rpm.Repo.Get(researchPlanId);

                            MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                            // the hole class is old code, will be removed
                            EntityTemplate entityTemplate = entityTemplateManager.Repo.Get(1);

                            var ds = dm.CreateEmptyDataset(dataStructure, rp, metadataStructure, entityTemplate);
                            datasetId = ds.Id;

                            // add security
                            if (GetUsernameOrDefault() != "DEFAULT")
                            {
                                entityPermissionManager.Create<User>(GetUsernameOrDefault(), entityname, typeof(Dataset), ds.Id, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
                            }
                        }
                        // update existing dataset
                        else
                        {
                            datasetId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
                            entityname = xmlDatasetHelper.GetEntityName(datasetId); // ensure the correct entityname is used #setEntitynNameNew
                            newDataset = false;
                        }

                        TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

                        if (dm.IsDatasetCheckedOutFor(datasetId, GetUsernameOrDefault()) || dm.CheckOutDataset(datasetId, GetUsernameOrDefault()))
                        {
                            DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(datasetId);

                            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_XML))
                            {
                                XDocument xMetadata = (XDocument)TaskManager.Bus[CreateTaskmanager.METADATA_XML];
                                workingCopy.Metadata = Xml.Helpers.XmlWriter.ToXmlDocument(xMetadata);

                                workingCopy.Title = xmlDatasetHelper.GetInformation(datasetId, workingCopy.Metadata, NameAttributeValues.title);
                                workingCopy.Description = xmlDatasetHelper.GetInformation(datasetId, workingCopy.Metadata, NameAttributeValues.description);

                                //check if modul exist
                                int v = 1;
                                if (workingCopy.Dataset.Versions != null && workingCopy.Dataset.Versions.Count > 1) v = workingCopy.Dataset.Versions.Count();

                                TaskManager.Bus[CreateTaskmanager.METADATA_XML] = setSystemValuesToMetadata(datasetId, v, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata, newDataset);
                            }

                            //set status
                            workingCopy = setStateInfo(workingCopy, valid);
                            //set modifikation
                            workingCopy = setModificationInfo(workingCopy, newDataset, GetUsernameOrDefault(), "Metadata");

                            title = workingCopy.Title;
                            if (string.IsNullOrEmpty(title)) title = "No Title available.";

                            TaskManager.AddToBus(CreateTaskmanager.ENTITY_TITLE, title);//workingCopy.Metadata.SelectNodes("Metadata/Description/Description/Title/Title")[0].InnerText);
                            TaskManager.AddToBus(CreateTaskmanager.ENTITY_ID, datasetId);

                            dm.EditDatasetVersion(workingCopy, null, null, null);
                            dm.CheckInDataset(datasetId, commitMessage, GetUsernameOrDefault(), ViewCreationBehavior.None);

                            #region set releationships

                            //todo check if dim is active
                            // todo call to  a function in dim
                            setRelationships(datasetId, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata, entityname);

                            // references

                            #endregion set releationships

                            #region set references

                            setReferences(workingCopy);

                            #endregion set references

                            if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
                            {
                                var x = this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", datasetId } });
                            }

                            LoggerFactory.LogData(datasetId.ToString(), typeof(Dataset).Name, Vaiona.Entities.Logging.CrudState.Created);

                            if (newDataset)
                            {
                                var es = new EmailService();
                                es.Send(MessageHelper.GetCreateDatasetHeader(datasetId, entityname),
                                    MessageHelper.GetCreateDatasetMessage(datasetId, title, GetUsernameOrDefault(), entityname),
                                    GeneralSettings.SystemEmail
                                    );
                            }
                            else
                            {
                                var es = new EmailService();
                                es.Send(MessageHelper.GetMetadataUpdatHeader(datasetId, entityname),
                                    MessageHelper.GetUpdateDatasetMessage(datasetId, title, GetUsernameOrDefault(), entityname),
                                    GeneralSettings.SystemEmail
                                    );
                            }
                        }

                        Session["CreateDatasetTaskManager"] = null;

                        return datasetId;
                    }
                }
                catch (Exception ex)
                {
                    var es = new EmailService();
                    es.Send(MessageHelper.GetMetadataUpdatHeader(datasetId, entityname),
                        ex.Message,
                        GeneralSettings.SystemEmail
                        );

                    string message = String.Format("error appears by create/update dataset with id: {0} , error: {1} ", datasetId.ToString(), ex.Message);
                    LoggerFactory.LogCustom(message);
                }
            }

            #endregion create dataset

            return -1;
        }

        #region Options

        public ActionResult Cancel()
        {
            //public ActionResult LoadMetadata(long datasetId, bool locked = false, bool created = false, bool fromEditMode = false, bool resetTaskManager = false, XmlDocument newMetadata = null)

            //TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            //if (TaskManager != null)
            //{
            //    DatasetManager dm = new DatasetManager();
            //    long datasetid = -1;
            //    bool resetTaskManager = true;
            //    XmlDocument metadata = null;
            //    bool editmode = false;
            //    bool created = false;

            //    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
            //    {
            //        datasetid = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
            //    }

            //    if (datasetid > -1 && dm.IsDatasetCheckedIn(datasetid))
            //    {
            //        metadata = dm.GetDatasetLatestMetadataVersion(datasetid);
            //        editmode = true;
            //        created = true;
            //    }

            //    return RedirectToAction("LoadMetadata", "Form", new { area = "Dcm", entityId = datasetid, created = created, locked = true, fromEditMode = editmode, resetTaskManager = resetTaskManager, newMetadata = metadata });
            //}

            //return RedirectToAction("StartMetadataEditor", "Form");
            return null;
        }

        public ActionResult Copy()
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager != null)
            {
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
                {
                    long datasetid = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);

                    return RedirectToAction("Index", "CreateDataset", new { id = datasetid, type = "DatasetId" });
                }
            }
            //Index(long id = -1, string type = "")
            return RedirectToAction("Index", "CreateDataset", new { id = -1, type = "DatasetId" });
        }

        public ActionResult Reset()
        {
            //public ActionResult LoadMetadata(long datasetId, bool locked = false, bool created = false, bool fromEditMode = false, bool resetTaskManager = false, XmlDocument newMetadata = null)

            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager != null)
            {
                using (DatasetManager dm = new DatasetManager())
                {
                    long datasetid = -1;
                    bool resetTaskManager = true;
                    XmlDocument metadata = null;
                    bool editmode = false;
                    bool created = false;

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
                    {
                        datasetid = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
                    }

                    if (datasetid > -1 && dm.IsDatasetCheckedIn(datasetid))
                    {
                        metadata = dm.GetDatasetLatestMetadataVersion(datasetid);
                        editmode = true;
                        created = true;
                    }

                    return RedirectToAction("LoadMetadata", "Form", new { area = "Dcm", entityId = datasetid, locked = false, created = created, fromEditMode = editmode, resetTaskManager = resetTaskManager, newMetadata = metadata });
                }
            }

            return RedirectToAction("StartMetadataEditor", "Form");
        }

        /// <summary>
        /// redirect to the DDM/Data/ShowData Action
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ShowData(long id)
        {
            return this.Run("DDM", "Data", "ShowData", new RouteValueDictionary { { "id", id } });
        }

        /// <summary>
        /// Load the UploadWizard with preselected parameters
        /// and redirect to "UploadWizard", "Submit", area = "Dcm"
        /// </summary>
        /// <returns></returns>
        public ActionResult StartUploadWizard()
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            DataStructureType type = new DataStructureType();

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.DATASTRUCTURE_TYPE))
            {
                type = (DataStructureType)TaskManager.Bus[CreateTaskmanager.DATASTRUCTURE_TYPE];
            }

            long datasetid = 0;
            // set parameters for upload process to pass it with the action
            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
            {
                datasetid = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
            }

            Session["CreateDatasetTaskmanager"] = null;
            TaskManager = null;

            return RedirectToAction("UploadWizard", "Submit", new { type = type, datasetid = datasetid });
        }

        #endregion Options

        #endregion Submit And Create And Finish And Cancel and Reset

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

        public List<ListViewItem> LoadDatasetViewList()
        {
            List<ListViewItem> temp = new List<ListViewItem>();

            DatasetManager datasetManager = new DatasetManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            //get all datasetsid where the current userer has access to
            UserManager userManager = new UserManager();
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

            try
            {
                List<long> datasetIds = entityPermissionManager.GetKeys(GetUsernameOrDefault(), "Dataset",
                    typeof(Dataset), RightType.Write);

                List<DatasetVersion> datasetVersions = datasetManager.GetDatasetLatestVersions(datasetIds, false);
                foreach (var dsv in datasetVersions)
                {
                    string title = dsv.Title;
                    string description = dsv.Description;

                    temp.Add(new ListViewItem(dsv.Dataset.Id, title, description));
                }

                return temp.OrderBy(p => p.Title).ToList();
            }
            finally
            {
                datasetManager.Dispose();
                entityPermissionManager.Dispose();
                userManager.Dispose();
            }
        }

        public List<ListViewItemWithType> LoadDataStructureViewList()
        {
            DataStructureManager dsm = new DataStructureManager();
            try
            {
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
            finally
            {
                dsm.Dispose();
            }
        }

        public List<ListViewItem> LoadMetadataStructureViewList()
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            try
            {
                IEnumerable<MetadataStructure> metadataStructureList = metadataStructureManager.Repo.Get();

                List<ListViewItem> temp = new List<ListViewItem>();

                foreach (MetadataStructure metadataStructure in metadataStructureList)
                {
                    if (xmlDatasetHelper.IsActive(metadataStructure.Id) &&
                        xmlDatasetHelper.HasEntityType(metadataStructure.Id, "bexis.dlm.entities.data.dataset"))
                    {
                        string title = metadataStructure.Name;

                        temp.Add(new ListViewItem(metadataStructure.Id, title, metadataStructure.Description));
                    }
                }

                return temp.OrderBy(p => p.Title).ToList();
            }
            finally
            {
                metadataStructureManager.Dispose();
            }
        }

        private DataStructureType GetDataStructureType(long id)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();

            try
            {
                DataStructure dataStructure = dataStructureManager.AllTypesDataStructureRepo.Get(id);

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
            finally
            {
                dataStructureManager.Dispose();
            }
        }

        private void setAdditionalFunctions()
        {
            TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

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

            TaskManager.Actions.Add(CreateTaskmanager.CANCEL_ACTION, cancelAction);
            TaskManager.Actions.Add(CreateTaskmanager.COPY_ACTION, copyAction);
            TaskManager.Actions.Add(CreateTaskmanager.RESET_ACTION, resetAction);
            TaskManager.Actions.Add(CreateTaskmanager.SUBMIT_ACTION, submitAction);
        }

        //toDo this function to DIM or BAM ??
        /// <summary>
        /// this function is parsing the xmldocument to
        /// create releationships based on releationshiptypes between datasets and person parties
        /// </summary>
        /// <param name="datasetid"></param>
        /// <param name="metadataStructureId"></param>
        /// <param name="metadata"></param>
        private void setRelationships(long datasetid, long metadataStructureId, XmlDocument metadata, string entityname)
        {
            using (PartyManager partyManager = new PartyManager())
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            using (PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager())
            {
                try
                {
                    using (var uow = this.GetUnitOfWork())
                    {
                        //check if mappings exist between system/relationships and the metadatastructure/attr
                        // get all party mapped nodes
                        IEnumerable<XElement> complexElements = XmlUtility.GetXElementsByAttribute("partyid", XmlUtility.ToXDocument(metadata));

                        // get all relationshipTypes where entityname is involved
                        var relationshipTypes = uow.GetReadOnlyRepository<PartyRelationshipType>().Get().Where(
                            p => p.AssociatedPairs.Any(
                                ap => ap.SourcePartyType.Title.ToLower().Equals(entityname.ToLower()) || ap.TargetPartyType.Title.ToLower().Equals(entityname.ToLower())
                                ));

                        #region delete relationships

                        foreach (var relationshipType in relationshipTypes)
                        {
                            // go through each associated realtionship type pair (e.g. Person - Dataset, Person - Publication)
                            foreach (var partyTpePair in relationshipType.AssociatedPairs)
                            {
                                // check if entityname is source or target and delete all found party realationships
                                if (partyTpePair.SourcePartyType.Title.ToLower().Equals(entityname.ToLower()))
                                {
                                    IEnumerable<PartyRelationship> relationships = uow.GetReadOnlyRepository<PartyRelationship>().Get().Where(
                                            r =>
                                            r.SourceParty != null && r.SourceParty.Name.Equals(datasetid.ToString()) &&
                                            r.PartyTypePair != null && r.PartyTypePair.Id.Equals(partyTpePair.Id)
                                        );

                                    IEnumerable<long> partyids = complexElements.Select(i => Convert.ToInt64(i.Attribute("partyid").Value));

                                    foreach (PartyRelationship pr in relationships)
                                    {
                                        partyManager.RemovePartyRelationship(pr);
                                    }
                                }
                                else if (partyTpePair.TargetPartyType.Title.ToLower().Equals(entityname.ToLower()))
                                {
                                    IEnumerable<PartyRelationship> relationships = uow.GetReadOnlyRepository<PartyRelationship>().Get().Where(
                                            r =>
                                            r.TargetParty != null && r.TargetParty.Name.Equals(datasetid.ToString()) &&
                                            r.PartyTypePair != null && r.PartyTypePair.Id.Equals(partyTpePair.Id)
                                        );

                                    IEnumerable<long> partyids = complexElements.Select(i => Convert.ToInt64(i.Attribute("partyid").Value));

                                    foreach (PartyRelationship pr in relationships)
                                    {
                                        partyManager.RemovePartyRelationship(pr);
                                    }
                                }
                            }
                        }

                        #endregion delete relationships

                        #region add relationship

                        foreach (XElement item in complexElements)
                        {
                            if (item.HasAttributes)
                            {
                                long sourceId = Convert.ToInt64(item.Attribute("roleId").Value);
                                long id = Convert.ToInt64(item.Attribute("id").Value);
                                string type = item.Attribute("type").Value;
                                long partyid = Convert.ToInt64(item.Attribute("partyid").Value);

                                LinkElementType sourceType = LinkElementType.MetadataNestedAttributeUsage;
                                if (type.Equals("MetadataPackageUsage")) sourceType = LinkElementType.MetadataPackageUsage;

                                foreach (var relationship in relationshipTypes)
                                {
                                    // when mapping in both directions are exist
                                    if ((MappingUtils.ExistMappings(id, sourceType, relationship.Id, LinkElementType.PartyRelationshipType) &&
                                        MappingUtils.ExistMappings(relationship.Id, LinkElementType.PartyRelationshipType, id, sourceType)) ||

                                        (MappingUtils.ExistMappings(sourceId, LinkElementType.MetadataAttributeUsage, relationship.Id, LinkElementType.PartyRelationshipType) &&
                                        MappingUtils.ExistMappings(relationship.Id, LinkElementType.PartyRelationshipType, sourceId, LinkElementType.MetadataAttributeUsage)) ||

                                        (MappingUtils.ExistMappings(sourceId, LinkElementType.ComplexMetadataAttribute, relationship.Id, LinkElementType.PartyRelationshipType) &&
                                        MappingUtils.ExistMappings(relationship.Id, LinkElementType.PartyRelationshipType, sourceId, LinkElementType.ComplexMetadataAttribute)) ||

                                        (MappingUtils.ExistMappings(sourceId, LinkElementType.MetadataNestedAttributeUsage, relationship.Id, LinkElementType.PartyRelationshipType) &&
                                        MappingUtils.ExistMappings(relationship.Id, LinkElementType.PartyRelationshipType, sourceId, LinkElementType.MetadataNestedAttributeUsage)) ||

                                        (MappingUtils.ExistMappings(sourceId, LinkElementType.MetadataPackageUsage, relationship.Id, LinkElementType.PartyRelationshipType) &&
                                        MappingUtils.ExistMappings(relationship.Id, LinkElementType.PartyRelationshipType, sourceId, LinkElementType.MetadataPackageUsage)))
                                    {
                                        // create releationship

                                        // create a Party for the dataset
                                        var customAttributes = new Dictionary<String, String>();
                                        customAttributes.Add("Name", datasetid.ToString());
                                        customAttributes.Add("Id", datasetid.ToString());

                                        // get or create datasetParty if not exists
                                        Party datasetParty = partyManager.GetPartyByCustomAttributeValues(partyTypeManager.PartyTypeRepository.Get(cc => cc.Title == entityname).First(), customAttributes).FirstOrDefault();
                                        if (datasetParty == null) datasetParty = partyManager.Create(partyTypeManager.PartyTypeRepository.Get(cc => cc.Title == entityname).First(), "[description]", null, null, customAttributes);

                                        // get user party
                                        var person = partyManager.GetParty(partyid);

                                        // add party relationships
                                        foreach (var partyTpePair in relationship.AssociatedPairs)
                                        {
                                            if (partyTpePair.SourcePartyType.Title.ToLower().Equals(entityname.ToLower()) || partyTpePair.TargetPartyType.Title.ToLower().Equals(entityname.ToLower()))
                                            {
                                                if (partyTpePair != null && person != null && datasetParty != null)
                                                {
                                                    if (!uow.GetReadOnlyRepository<PartyRelationship>().Get().Any(
                                                        r =>
                                                        r.SourceParty != null && r.SourceParty.Id.Equals(person.Id) &&
                                                        r.PartyTypePair != null && r.PartyTypePair.Id.Equals(partyTpePair.Id) &&
                                                        r.TargetParty.Id.Equals(datasetParty.Id)
                                                    ))
                                                    {
                                                        partyManager.AddPartyRelationship(
                                                            person.Id,
                                                            datasetParty.Id,
                                                            relationship.Title,
                                                            "",
                                                            partyTpePair.Id

                                                            );
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        #endregion add relationship
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private XDocument setSystemValuesToMetadata(long datasetid, long version, long metadataStructureId, XmlDocument metadata, bool newDataset)
        {
            SystemMetadataHelper SystemMetadataHelper = new SystemMetadataHelper();

            Key[] myObjArray = { };

            if (newDataset) myObjArray = new Key[] { Key.Id, Key.Version, Key.DateOfVersion, Key.MetadataCreationDate, Key.MetadataLastModfied };
            else myObjArray = new Key[] { Key.Id, Key.Version, Key.DateOfVersion, Key.MetadataLastModfied };

            metadata = SystemMetadataHelper.SetSystemValuesToMetadata(datasetid, version, metadataStructureId, metadata, myObjArray);

            return XmlUtility.ToXDocument(metadata);
        }

        private void setReferences(DatasetVersion datasetVersion)
        {
            using (EntityReferenceManager entityReferenceManager = new EntityReferenceManager())
            using (EntityManager entityManager = new EntityManager())
            {
                EntityReferenceHelper helper = new EntityReferenceHelper();
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                if (datasetVersion != null)
                {
                    List<EntityReference> refs = getAllMetadataReferences(datasetVersion);

                    foreach (var singleRef in refs)
                    {
                        if (!entityReferenceManager.Exist(singleRef, true, true))
                            entityReferenceManager.Create(singleRef);
                    }
                }
            }
        }

        private List<EntityReference> getAllMetadataReferences(DatasetVersion datasetVersion)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            using (EntityManager entityManager = new EntityManager())
            using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
            {
                List<EntityReference> tmp = new List<EntityReference>();
                EntityReferenceHelper helper = new EntityReferenceHelper();
                MappingUtils mappingUtils = new MappingUtils();
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                long id = 0;
                long typeid = 0;
                int version = 0;

                if (datasetVersion != null)
                {
                    long metadataStrutcureId = datasetVersion.Dataset.MetadataStructure.Id;

                    //get entity type like dataset or sample
                    string entityName = xmlDatasetHelper.GetEntityNameFromMetadatStructure(metadataStrutcureId, metadataStructureManager);
                    Entity entityType = entityManager.Entities.Where(e => e.Name.Equals(entityName)).FirstOrDefault();

                    //get id of the entity type
                    id = datasetVersion.Dataset.Id;
                    typeid = entityType.Id;
                    version = datasetVersion.Dataset.Versions.Count();

                    // if mapping to entites type exist
                    if (MappingUtils.ExistMappingWithEntityFromRoot(
                        datasetVersion.Dataset.MetadataStructure.Id,
                        BExIS.Dim.Entities.Mappings.LinkElementType.MetadataStructure,
                        typeid))
                    {
                        //load metadata and searching for the entity Attrs
                        XDocument metadata = XmlUtility.ToXDocument(datasetVersion.Metadata);
                        IEnumerable<XElement> xelements = XmlUtility.GetXElementsByAttribute(EntityReferenceXmlAttribute.entityid.ToString(), metadata);

                        foreach (XElement e in xelements)
                        {
                            //get attributes from xml node
                            long xId = 0;
                            int xVersion = 0;
                            long xTypeId = 0;

                            if (Int64.TryParse(e.Attribute(EntityReferenceXmlAttribute.entityid.ToString()).Value.ToString(), out xId) &&
                                Int32.TryParse(e.Attribute(EntityReferenceXmlAttribute.entityversion.ToString()).Value.ToString(), out xVersion) &&
                                Int64.TryParse(e.Attribute(EntityReferenceXmlAttribute.entitytype.ToString()).Value.ToString(), out xTypeId)
                                )
                            {
                                //entityName = xmlDatasetHelper.GetEntityNameFromMetadatStructure(metadataStrutcureId, new Dlm.Services.MetadataStructure.MetadataStructureManager());
                                //entityType = entityManager.Entities.Where(e => e.Name.Equals(entityName)).FirstOrDefault();
                                string xpath = e.GetAbsoluteXPath();

                                tmp.Add(new EntityReference(
                                        id,
                                        typeid,
                                        version,
                                        xId,
                                        xTypeId,
                                        xVersion,
                                        xpath,
                                        DefaultEntitiyReferenceType.MetadataLink.GetDisplayName(),
                                        DateTime.Now
                                    ));
                            }
                        }
                    }
                }

                return tmp;
            }
        }

        private DatasetVersion setStateInfo(DatasetVersion workingCopy, bool valid)
        {
            //StateInfo
            if (workingCopy.StateInfo == null) workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo();

            if (valid)
                workingCopy.StateInfo.State = DatasetStateInfo.Valid.ToString();
            else workingCopy.StateInfo.State = DatasetStateInfo.NotValid.ToString();

            return workingCopy;
        }

        private DatasetVersion setModificationInfo(DatasetVersion workingCopy, bool newDataset, string user, string comment)
        {
            // modifikation info
            if (workingCopy.StateInfo == null) workingCopy.ModificationInfo = new EntityAuditInfo();

            if (newDataset)
                workingCopy.ModificationInfo.ActionType = AuditActionType.Create;
            else
                workingCopy.ModificationInfo.ActionType = AuditActionType.Edit;

            //set performer
            workingCopy.ModificationInfo.Performer = string.IsNullOrEmpty(user) ? "" : user;

            //set comment
            workingCopy.ModificationInfo.Comment = string.IsNullOrEmpty(comment) ? "" : comment;

            //set time
            workingCopy.ModificationInfo.Timestamp = DateTime.Now;

            return workingCopy;
        }

        #endregion Helper
    }
}