using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Web.Shell.Areas.DCM.Models;
using Vaiona.Util.Cfg;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class SubmitSpecifyDatasetController : Controller
    {
        private TaskManager TaskManager;

        //
        // GET: /DCM/SpecifyDataset/

        [HttpGet]
        public ActionResult SpecifyDataset(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.SetCurrent(index);

                // remove if existing
                TaskManager.RemoveExecutedStep(TaskManager.Current());
            }

            ChooseDatasetViewModel model = new ChooseDatasetViewModel();

            // jump back to this step
            // check if dataset selected
            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID))
                if (Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]) > 0)
                    model.DatasetTitle = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();


            // load datasetids
            //DatasetManager dm = new DatasetManager();
            //IList<DatasetVersion> dv = dm.DatasetVersionRepo.Get();

            //model.Datasets = (from datasets in dm.DatasetRepo.Get() select datasets.Id).ToList();
            model.StepInfo = TaskManager.Current();


            //model.DatasetViewModel = new CreateDatasetViewModel();
            model.DatasetViewModel = new List<MetadataPackageModel>();
            //if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatasetViewModel.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
            if ((List<ListViewItem>)Session["DatasetVersionViewList"] != null) model.DatasetsViewList = (List<ListViewItem>)Session["DatasetVersionViewList"];
            //if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.DatasetViewModel.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];

            return PartialView(model);

        }

        [HttpPost]
        public ActionResult SpecifyDataset(object[] data)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            ChooseDatasetViewModel model = new ChooseDatasetViewModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager != null)
            {
                TaskManager.Current().SetValid(false);

                if (data != null) TaskManager.AddToBus(data);

                if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID))
                {
                    DatasetManager dm = new DatasetManager();
                    Dataset ds = new Dataset();
                    try
                    {
                        dm = new DatasetManager();
                        ds = dm.GetDataset((long)Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]));

                        TaskManager.AddToBus(TaskManager.DATASTRUCTURE_ID, ((StructuredDataStructure)(ds.DataStructure.Self)).Id);
                        TaskManager.AddToBus(TaskManager.DATASTRUCTURE_TITLE, ((StructuredDataStructure)(ds.DataStructure.Self)).Name);

                        TaskManager.Current().SetValid(true);

                    }
                    catch
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other, "Dataset not exist."));
                    }
                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "Dataset not exist."));
                }

                if (TaskManager.Current().valid == true)
                {
                    TaskManager.AddExecutedStep(TaskManager.Current());
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
                else
                {
                    TaskManager.Current().SetStatus(StepStatus.error);

                    //reload model
                    DatasetManager dm = new DatasetManager();
                    //IList<DatasetVersion> dv = dm.DatasetVersionRepo.Get();

                    model.Datasets = (from datasets in dm.DatasetRepo.Get() select datasets.Id).ToList();
                    model.StepInfo = TaskManager.Current();
                    //model.DatasetViewModel = new CreateDatasetViewModel();                    
                    model.DatasetViewModel = new List<MetadataPackageModel>();
                    ////load datastructure ids
                    //if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatasetViewModel.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
                    //if ((List<ListViewItem>)Session["DatasetVersionViewList"] != null) model.DatasetsViewList = (List<ListViewItem>)Session["DatasetVersionViewList"];
                }
            }

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult AddSelectedDatasetToBus(string id)
        {


            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            DatasetManager datasetManager = new DatasetManager();

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_STATUS))
            {
                if (TaskManager.Bus[TaskManager.DATASET_STATUS].Equals("new"))
                {
                    long newid = Convert.ToInt64(TaskManager.Bus[TaskManager.DATASET_ID]);
                    datasetManager.PurgeDataset(newid);

                }

                TaskManager.Bus[TaskManager.DATASET_STATUS] = "edit";
            }
            else
            {
                TaskManager.AddToBus("DatasetStatus", "edit");
            }


            long datasetId = Convert.ToInt64(id);

            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);



            TaskManager.AddToBus(TaskManager.DATASET_ID, datasetId);
            //Add Metadata to Bus
            TaskManager.AddToBus(TaskManager.DATASET_TITLE, datasetVersion.Metadata.SelectNodes("Metadata/Description/Description/Title/Title")[0].InnerText);
            /*TaskManager.AddToBus(TaskManager.OWNER, datasetVersion.Metadata.GetElementsByTagName("bgc:owner")[0].InnerText);
            TaskManager.AddToBus(TaskManager.AUTHOR, datasetVersion.Metadata.GetElementsByTagName("bgc:author")[0].InnerText);
            TaskManager.AddToBus(TaskManager.PROJECTNAME, datasetVersion.Metadata.GetElementsByTagName("bgc:projectName")[0].InnerText);
            TaskManager.AddToBus(TaskManager.INSTITUTE, datasetVersion.Metadata.GetElementsByTagName("bgc:institute")[0].InnerText);*/

            ResearchPlanManager rpm = new ResearchPlanManager();
            ResearchPlan rp = rpm.Repo.Get(datasetVersion.Dataset.ResearchPlan.Id);
            TaskManager.AddToBus(TaskManager.RESEARCHPLAN_ID, rp.Id);
            TaskManager.AddToBus(TaskManager.RESEARCHPLAN_TITLE, rp.Title);


            Session["TaskManager"] = TaskManager;

            return Content("");
        }

        [HttpGet]
        public ActionResult CreateDataset()
        {
            CreateDatasetViewModel model = new CreateDatasetViewModel();

            model.MetadataPackageModel = GetMetadataStructureModel(1);
            if (Session["MetadataStructure"] == null)
            { 
                 Session["MetadataStructure"] = GetMetadataStructureModel(1);
            }

            if ((List<MetadataPackageModel>)Session["MetadataStructure"] != null) model.MetadataPackageModel = (List<MetadataPackageModel>)Session["MetadataStructure"];
            if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
            if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];

            return PartialView("_createDataset", model);
        }

        /// <summary>
        /// POST of Create Dataset to create a dataset
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[HttpPost]
        //public ActionResult CreateDataset(CreateDatasetViewModel model)
        //{

        //    if (model == null)
        //    {
        //        model = new CreateDatasetViewModel();
        //        if ((List<MetadataPackageModel>)Session["MetadataStructure"] != null) model.MetadataPackageModel = (List<MetadataPackageModel>)Session["MetadataStructure"];
        //        if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
        //        if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];

        //        return PartialView("_createDataset", model);
        //    }

        //    TaskManager TaskManager = (TaskManager)Session["TaskManager"];

        //    if (ModelState.IsValid)
        //    {
        //        XmlDocument metadata = new XmlDocument();
        //        metadata.Load(Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "emptymetadata.xml"));

        //        metadata.GetElementsByTagName("bgc:title")[0].InnerText = model.Title;
        //        metadata.GetElementsByTagName("bgc:owner")[0].InnerText = model.Owner;
        //        metadata.GetElementsByTagName("bgc:author")[0].InnerText = model.DatasetAuthor;
        //        metadata.GetElementsByTagName("bgc:projectName")[0].InnerText = model.ProjectName;
        //        metadata.GetElementsByTagName("bgc:institute")[0].InnerText = model.ProjectInstitute;

        //        //Add Metadata to Bus
        //        TaskManager.AddToBus(TaskManager.DATASET_TITLE, model.Title);
        //        TaskManager.AddToBus(TaskManager.OWNER, model.Owner);
        //        TaskManager.AddToBus(TaskManager.AUTHOR, model.DatasetAuthor);
        //        TaskManager.AddToBus(TaskManager.PROJECTNAME, model.ProjectName);
        //        TaskManager.AddToBus(TaskManager.INSTITUTE, model.ProjectInstitute);

        //        DatasetManager dm = new DatasetManager();
        //        DataStructureManager dsm = new DataStructureManager();
        //        DataStructure dataStructure = dsm.StructuredDataStructureRepo.Get(model.DataStructureId);

        //        ResearchPlanManager rpm = new ResearchPlanManager();
        //        ResearchPlan rp = rpm.Repo.Get(model.ResearchPlanId);

        //        Dataset ds = dm.CreateEmptyDataset(dataStructure, rp, null);

        //        TaskManager.AddToBus(TaskManager.DATASET_TITLE, model.Title);
        //        TaskManager.AddToBus(TaskManager.RESEARCHPLAN_ID, model.ResearchPlanId);



        //        if (dm.IsDatasetCheckedOutFor(ds.Id, GetUserNameOrDefault()) || dm.CheckOutDataset(ds.Id, GetUserNameOrDefault()))
        //        {
        //            metadata.GetElementsByTagName("bgc:id")[0].InnerText = ds.Id.ToString();

        //            DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(ds.Id);
        //            workingCopy.Metadata = metadata;
        //            TaskManager.AddToBus(TaskManager.DATASET_ID, ds.Id);
        //            TaskManager.AddToBus(TaskManager.RESEARCHPLAN_TITLE, rp.Title);
        //            dm.EditDatasetVersion(workingCopy, null, null, null);
        //        }

        //        //DatasetStatus if new or selected
        //        if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_STATUS))
        //        {
        //            TaskManager.Bus[TaskManager.DATASET_STATUS] = "new";
        //        }
        //        else
        //        {
        //            TaskManager.AddToBus("DatasetStatus", "new");
        //        }

        //        Session["createDatasetWindowVisible"] = false;
        //        Session["TaskManager"] = TaskManager;

        //        return Json(new { success = true, title = model.Title });
        //    }
        //    else
        //    {
        //        Session["createDatasetWindowVisible"] = true;

        //        // put lists to model
        //    }

        //    if ((List<MetadataPackageModel>)Session["MetadataStructure"] != null) model.MetadataPackageModel = (List<MetadataPackageModel>)Session["MetadataStructure"];
        //    if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
        //    if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];


        //    return PartialView("_createDataset", model);
        //}

        
        public ActionResult AjaxLoadPackage(int Id)
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();
            MetadataAttributeManager mdaManager = new MetadataAttributeManager();

            MetadataStructure metadataStructure = mdsManager.Repo.Get(1);

            MetadataPackageUsage mpu = metadataStructure.MetadataPackageUsages.Where(p => p.Id.Equals(Id)).FirstOrDefault();

            MetadataPackageModel model = MetadataPackageModel.Convert(mpu);
            model.ConvertMetadataAttributeModels(mpu.MetadataPackage.MetadataAttributeUsages);

            return PartialView("_defineMetadataPackage", model);
        }

        public ActionResult DefineMetadataStructure()
        {
 
            return PartialView("_defineMetadataStructure",  GetMetadataStructureModel(1));
        }

        private List<MetadataPackageModel> GetMetadataStructureModel(long id)
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();
            MetadataAttributeManager mdaManager = new MetadataAttributeManager();

            MetadataStructure metadatStructure = mdsManager.Repo.Get(id);

            List<MetadataPackageModel> model = new List<MetadataPackageModel>();

            if (metadatStructure.MetadataPackageUsages.Count > 0)
            {
                foreach (MetadataPackageUsage mpu in metadatStructure.MetadataPackageUsages)
                {
                    MetadataPackageModel mpm = MetadataPackageModel.Convert(mpu);
                    mpm.ConvertMetadataAttributeModels(mpu.MetadataPackage.MetadataAttributeUsages);
                    model.Add(mpm);
                }
            }

            return model;
        }


        #region validation

        [HttpPost]
        public JsonResult ValidateMetadataAttributeUsage(object value, int id, int parentid, string parentname)
        {
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult ValidateMetadataAttributeUsage(object Value, long id)
        //{
        //        if (String.IsNullOrEmpty(Value.ToString()))
        //        {
        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            string error = "XXX : " + Value.ToString();

        //            return Json(error, JsonRequestBehavior.AllowGet);
        //        }
            
        //}

        //public JsonResult Test(object Value)
        //{
        //    if (String.IsNullOrEmpty(Value.ToString()))
        //    {
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        string error = "YYY";

        //        return Json(error, JsonRequestBehavior.AllowGet);
        //    }

        //}


        #endregion

        #region private methods

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


        #endregion
    }
}
