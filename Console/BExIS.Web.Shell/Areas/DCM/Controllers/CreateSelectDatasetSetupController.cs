using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Web.Shell.Areas.DCM.Models;
using BExIS.Web.Shell.Areas.DCM.Models.Create;
using BExIS.Xml.Services;
using Vaiona.Util.Cfg;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class CreateSelectDatasetSetupController : Controller
    {
        private CreateDatasetTaskmanager TaskManager;

        //
        // GET: /DCM/SelectDatasetSetup/


        [HttpGet]
        public ActionResult SelectDatasetSetup(int index)
        {

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.UpdateStepStatus(index);
                TaskManager.SetCurrent(index);
                Session["CreateDatasetTaskmanager"] = TaskManager;
               //remove if existing
               //TaskManager.RemoveExecutedStep(TaskManager.Current());
            }

            SelectDatasetSetupModel model = GetDefaultModel();
            model.StepInfo = TaskManager.Current();
            //Get StepInfo
            //model.StepInfo = TaskManager.Current();
            Session["CreateDatasetTaskmanager"] = TaskManager;


            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SelectDatasetSetup()
        {

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            SelectDatasetSetupModel model = new SelectDatasetSetupModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASTRUCTURE_ID) && 
                TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID) && 
                TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.RESEARCHPLAN_ID))
            {
                TaskManager.Current().SetValid(true);

            }
            else
            {
                 TaskManager.Current().SetValid(false);
                 model = GetDefaultModel();
                 model.StepInfo = TaskManager.Current();
                 model.ErrorList.Add(new Error(ErrorType.Dataset, "Informations are not correct"));
            }

            

            if (TaskManager.Current().IsValid())
            {
                //TaskManager.AddExecutedStep(TaskManager.Current());
                TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
            }


            return PartialView(model);
        }


        public ActionResult StoreSelectedDatasetSetup(SelectDatasetSetupModel model)
        {
            CreateDatasetTaskmanager TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            if (model == null)
            {
                model = GetDefaultModel();
                return PartialView("SelectDatasetInformation", model);
            }

            model = LoadLists(model);
            model.StepInfo = TaskManager.Current();

            if (ModelState.IsValid)
            {
                TaskManager.AddToBus(CreateDatasetTaskmanager.DATASTRUCTURE_ID, model.SelectedDatastructureId);
                TaskManager.AddToBus(CreateDatasetTaskmanager.RESEARCHPLAN_ID, model.SelectedResearchPlanId);
                TaskManager.AddToBus(CreateDatasetTaskmanager.METADATASTRUCTURE_ID, model.SelectedMetadatStructureId);

                // creat a new dataset
                //CreateANewDataset(model.SelectedDatastructureId, model.SelectedResearchPlanId, model.SelectedMetadatStructureId);

                AdvanceTaskManager(model.SelectedMetadatStructureId);

                CreateXml();

                TaskManager.Current().SetStatus(StepStatus.success);

                return PartialView("SelectDatasetSetup", model);
            }
            else
            {
                TaskManager.Current().SetStatus(StepStatus.error);

                model.ErrorList.Add(new Error(ErrorType.Dataset, "Can not create dataset."));
            }

            return PartialView("SelectDatasetSetup", model);
        }

        #region save

        public ActionResult Save()
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int currentStepIndex = TaskManager.GetCurrentStepInfoIndex();

            SelectDatasetSetupModel model = GetDefaultModel();
            model.StepInfo = TaskManager.Current();

            #region create dataset

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASTRUCTURE_ID)
                && TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.RESEARCHPLAN_ID)
                && TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
            {
                Dataset ds;
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

                    ResearchPlanManager rpm = new ResearchPlanManager();
                    ResearchPlan rp = rpm.Repo.Get(researchPlanId);

                    MetadataStructureManager msm = new MetadataStructureManager();
                    MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                    ds = dm.CreateEmptyDataset(dataStructure, rp, metadataStructure);
                    datasetId = ds.Id;
                }
                else
                {
                    datasetId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASET_ID]);
                }

                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];





                if (dm.IsDatasetCheckedOutFor(datasetId, GetUserNameOrDefault()) || dm.CheckOutDataset(datasetId, GetUserNameOrDefault()))
                {

                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(datasetId);
                    TaskManager.AddToBus(CreateDatasetTaskmanager.DATASET_ID, datasetId);

                    dm.EditDatasetVersion(workingCopy, null, null, null);
                }
            }

            #endregion

            return PartialView("SelectDataset", model );
        }

        #endregion

        #region private methods

        private void AdvanceTaskManager(long MetadataStructureId)
        {
           CreateDatasetTaskmanager TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

           MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

           List<MetadataPackageUsage> metadataPackageList = metadataStructureManager.Repo.Get(MetadataStructureId).MetadataPackageUsages.ToList();

           Dictionary<int, long> MetadataPackageDic = new Dictionary<int, long>();

           StepInfo summary = TaskManager.StepInfos.Last();
           //TaskManager.StepInfos.Remove(TaskManager.StepInfos.Last());
           StepInfo start = TaskManager.StepInfos.First();
           TaskManager.StepInfos.Clear();
           TaskManager.StepInfos.Add(start);

           foreach (MetadataPackageUsage mpu in metadataPackageList)
           {
               StepInfo si = new StepInfo(mpu.Label)
               {
                   GetActionInfo = new ActionInfo
                   {
                       ActionName = "SetMetadataPackage",
                       ControllerName = "CreateSetMetadataPackage",
                       AreaName = "DCM"
                   },

                   PostActionInfo = new ActionInfo
                   {
                       ActionName = "SetMetadataPackage",
                       ControllerName = "CreateSetMetadataPackage",
                       AreaName = "DCM"
                   }
               };

               TaskManager.StepInfos.Add(si);
               MetadataPackageDic.Add(TaskManager.StepInfos.IndexOf(si), mpu.Id);
           }

           TaskManager.StepInfos.Add(summary);
           
            
           TaskManager.AddToBus(CreateDatasetTaskmanager.METADATAPACKAGE_IDS, MetadataPackageDic);
           Session["CreateDatasetTaskmanager"] = TaskManager;
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
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");

                TaskManager.AddToBus(CreateDatasetTaskmanager.METADATA_XML, metadataXml);

                //save
                metadataXml.Save(path);
            }

        }
            

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
        public string GetUserNameOrDefault()
        {
            string userName = string.Empty;
            try
            {
                userName = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(userName) ? userName : "DEFAULT";
        }

        private SelectDatasetSetupModel GetDefaultModel()
        {
            SelectDatasetSetupModel model = new SelectDatasetSetupModel();

            model = LoadLists(model);

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASTRUCTURE_ID))
                model.SelectedDatastructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASTRUCTURE_ID]);

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.RESEARCHPLAN_ID))
                model.SelectedResearchPlanId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.RESEARCHPLAN_ID]);

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
                model.SelectedMetadatStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            return model;
        }

        private SelectDatasetSetupModel LoadLists(SelectDatasetSetupModel model)
        {

            if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
            if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];
            if ((List<ListViewItem>)Session["MetadataStructureViewList"] != null) model.MetadataStructureViewList = (List<ListViewItem>)Session["MetadataStructureViewList"];

            return model;
        }


        #endregion
    }
}
