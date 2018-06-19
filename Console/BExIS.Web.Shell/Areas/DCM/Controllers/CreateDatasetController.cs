using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
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
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Modules.Dcm.UI.Models.CreateDataset;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Web.Shell.Helpers;
using BExIS.Web.Shell.Models;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
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
        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

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
                Session["DataStructureViewList"] = LoadDataStructureViewList();
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
            DatasetManager datasetManager = new DatasetManager();
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
            DatasetManager datasetManager = new DatasetManager();
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

            try
            {


                if (model == null)
                {
                    model = GetDefaultModel();
                    return PartialView("Index", model);
                }

                model = LoadLists(model);

                if (ModelState.IsValid)
                {
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
                            TaskManager.AddToBus(CreateTaskmanager.ENTITY_TITLE,
                                xmlDatasetHelper.GetInformationFromVersion(datasetVersion.Id, NameAttributeValues.title));

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
                        ResearchPlanManager rpm = new ResearchPlanManager();
                        TaskManager.AddToBus(CreateTaskmanager.RESEARCHPLAN_ID, rpm.Repo.Get().First().Id);
                    }

                    return RedirectToAction("StartMetadataEditor", "Form");
                }

                return View("Index", model);
            }
            finally
            {
                datasetManager.Dispose();
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
            List<ListViewItem> datasets = LoadDatasetViewList();

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
            if ((List<ListViewItemWithType>)Session["DataStructureViewList"] != null) model.DataStructureViewList = (List<ListViewItemWithType>)Session["DataStructureViewList"];
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

        public ActionResult Submit(bool valid)
        {
            // create and submit Dataset
            long datasetId = SubmitDataset(valid);

            bool editMode = false;

            if (TaskManager == null) TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.EDIT_MODE))
                editMode = (bool)TaskManager.Bus[CreateTaskmanager.EDIT_MODE];

            if (editMode)
                return RedirectToAction("LoadMetadata", "Form", new { entityId = datasetId, locked = true, created = false, fromEditMode = true });
            else
                return RedirectToAction("LoadMetadata", "Form", new { entityId = datasetId, locked = true, created = true });
        }

        /// <summary>
        /// Submit a Dataset based on the imformations
        /// in the CreateTaskManager
        /// </summary>
        public long SubmitDataset(bool valid)
        {
            #region create dataset
            DatasetManager dm = new DatasetManager();
            DataStructureManager dsm = new DataStructureManager();
            ResearchPlanManager rpm = new ResearchPlanManager();
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            string title = "";
            long datasetId = 0;
            bool newDataset = true;

            try
            {
                TaskManager = (CreateTaskmanager)Session["CreateDatasetTaskmanager"];

                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.DATASTRUCTURE_ID)
                    && TaskManager.Bus.ContainsKey(CreateTaskmanager.RESEARCHPLAN_ID)
                    && TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
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

                        MetadataStructureManager msm = new MetadataStructureManager();
                        MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                        var ds = dm.CreateEmptyDataset(dataStructure, rp, metadataStructure);
                        datasetId = ds.Id;

                        // add security
                        if (GetUsernameOrDefault() != "DEFAULT")
                        {
                            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
                            entityPermissionManager.Create<User>(GetUsernameOrDefault(), "Dataset", typeof(Dataset), ds.Id, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
                        }
                    }
                    else
                    {
                        datasetId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
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
                        }

                        //set status

                        if (workingCopy.StateInfo == null) workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo();

                        if (valid)
                            workingCopy.StateInfo.State = DatasetStateInfo.Valid.ToString();
                        else workingCopy.StateInfo.State = DatasetStateInfo.NotValid.ToString();

                        title = xmlDatasetHelper.GetInformationFromVersion(workingCopy.Id, NameAttributeValues.title);
                        if (string.IsNullOrEmpty(title)) title = "No Title available.";

                        TaskManager.AddToBus(CreateTaskmanager.ENTITY_TITLE, title);//workingCopy.Metadata.SelectNodes("Metadata/Description/Description/Title/Title")[0].InnerText);
                        TaskManager.AddToBus(CreateTaskmanager.ENTITY_ID, datasetId);

                        dm.EditDatasetVersion(workingCopy, null, null, null);
                        dm.CheckInDataset(datasetId, "Metadata was submited.", GetUsernameOrDefault(), ViewCreationBehavior.None);

                        #region set releationships 


                        //todo check if dim is active
                        // todo call to  a function in dim
                        setRelationships(datasetId, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata);

                        #endregion

                        if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
                        {

                            var x = this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", datasetId } });
                        }

                        LoggerFactory.LogData(datasetId.ToString(), typeof(Dataset).Name, Vaiona.Entities.Logging.CrudState.Created);


                        if (newDataset)
                        {
                            var es = new EmailService();
                            es.Send(MessageHelper.GetCreateDatasetHeader(),
                                MessageHelper.GetCreateDatasetMessage(datasetId, title, GetUsernameOrDefault()),
                                ConfigurationManager.AppSettings["SystemEmail"]
                                );
                        }
                        else
                        {
                            var es = new EmailService();
                            es.Send(MessageHelper.GetUpdateDatasetHeader(),
                                MessageHelper.GetUpdateDatasetMessage(datasetId, title, GetUsernameOrDefault()),
                                ConfigurationManager.AppSettings["SystemEmail"]
                                );
                        }
                    }


                    return datasetId;
                }
            }
            catch (Exception ex)
            {
                var es = new EmailService();
                es.Send(MessageHelper.GetUpdateDatasetHeader(),
                    ex.Message,
                    ConfigurationManager.AppSettings["SystemEmail"]
                    );
            }
            finally
            {
                dm.Dispose();
                rpm.Dispose();
                dsm.Dispose();
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
                DatasetManager dm = new DatasetManager();
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

            BExIS.Dcm.UploadWizard.DataStructureType type = new BExIS.Dcm.UploadWizard.DataStructureType();

            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.DATASTRUCTURE_TYPE))
            {
                type = (BExIS.Dcm.UploadWizard.DataStructureType)TaskManager.Bus[CreateTaskmanager.DATASTRUCTURE_TYPE];
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

                foreach (long id in datasetIds)
                {
                    if (datasetManager.IsDatasetCheckedIn(id))
                    {
                        string title = xmlDatasetHelper.GetInformation(id, NameAttributeValues.title);
                        string description = xmlDatasetHelper.GetInformation(id, NameAttributeValues.description);

                        temp.Add(new ListViewItem(id, title, description));
                    }
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
        private void setRelationships(long datasetid, long metadataStructureId, XmlDocument metadata)
        {
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager();

            try
            {
                using (var uow = this.GetUnitOfWork())
                {

                    //check if mappings exist between system/relationships and the metadatastructure/attr
                    // get all party mapped nodes
                    IEnumerable<XElement> complexElements = XmlUtility.GetXElementsByAttribute("partyid", XmlUtility.ToXDocument(metadata));

                    // get releaionship type id for owner
                    var releationships = uow.GetReadOnlyRepository<PartyRelationshipType>().Get().Where(
                        p => p.AssociatedPairs.Any(
                            ap => ap.SourceType.Title.ToLower().Equals("dataset") || ap.TargetType.Title.ToLower().Equals("dataset")
                            ));

                    foreach (XElement item in complexElements)
                    {
                        if (item.HasAttributes)
                        {
                            long sourceId = Convert.ToInt64(item.Attribute("roleId").Value);
                            string type = item.Attribute("type").Value;
                            long partyid = Convert.ToInt64(item.Attribute("partyid").Value);

                            LinkElementType sourceType = LinkElementType.MetadataNestedAttributeUsage;
                            if (type.Equals("MetadataPackageUsage")) sourceType = LinkElementType.MetadataPackageUsage;

                            foreach (var releationship in releationships)
                            {
                                // when mapping in both directions are exist
                                if (MappingUtils.ExistMappings(sourceId, sourceType, releationship.Id, LinkElementType.PartyRelationshipType) &&
                                    MappingUtils.ExistMappings(releationship.Id, LinkElementType.PartyRelationshipType, sourceId, sourceType))
                                {
                                    // create releationship

                                    // create a Party for the dataset
                                    var customAttributes = new Dictionary<String, String>();
                                    customAttributes.Add("Name", datasetid.ToString());
                                    customAttributes.Add("Id", datasetid.ToString());

                                    // get or create datasetParty
                                    Party datasetParty = partyManager.GetPartyByCustomAttributeValues(partyTypeManager.PartyTypeRepository.Get(cc => cc.Title == "Dataset").First(), customAttributes).FirstOrDefault();
                                    if (datasetParty == null) datasetParty = partyManager.Create(partyTypeManager.PartyTypeRepository.Get(cc => cc.Title == "Dataset").First(), "[description]", null, null, customAttributes);


                                    // Get user party
                                    var person = partyManager.GetParty(partyid);

                                    var partyTpePair = releationship.AssociatedPairs.FirstOrDefault();

                                    if (partyTpePair != null && person != null && datasetParty != null)
                                    {
                                        partyManager.AddPartyRelationship(
                                            datasetParty.Id,
                                            person.Id,
                                            releationship.Title,
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
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                partyManager.Dispose();
                partyTypeManager.Dispose();
            }
        }

        #endregion Helper
    }
}