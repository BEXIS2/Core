using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;
using BExIS.Web.Shell.Areas.DCM.Models.Create;
using System.Web.Routing;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using System.Xml.Linq;
using BExIS.Xml.Services;
using BExIS.Dcm.Wizard;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Security.Services.Objects;
using BExIS.Security.Entities.Security;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class CreateSummaryController : Controller
    {
        CreateDatasetTaskmanager TaskManager;

        //
        // GET: /DCM/CreateSummary/
        [HttpGet]
        public ActionResult Summary(int index)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            CreateSummaryModel model = new CreateSummaryModel();
            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.UpdateStepStatus(index);
                TaskManager.SetCurrent(index);
                Session["CreateDatasetTaskmanager"] = TaskManager;


                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    Dictionary<string, MetadataPackageModel> list = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                    model = CreateSummaryModel.Convert(list, TaskManager.Current());
                    model.ErrorList = ValidatePackageModels();
                    model.PageStatus = Models.PageStatus.FirstLoad;
                    return PartialView(model);
                }
                else
                {
                    TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST] = new Dictionary<string, MetadataPackageModel>();
                }

                model.ErrorList = ValidatePackageModels();
            }

           model.StepInfo = TaskManager.Current();
           return PartialView(model);
        }

        [HttpPost]
        public ActionResult Summary(int? index, string name = null)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //set current stepinfo based on index
            if (TaskManager != null)
            {

                Session["CreateDatasetTaskmanager"] = TaskManager;
            }

         
            CreateSummaryModel model = new CreateSummaryModel();
            // prepare model
            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            {
                model.StepInfo = TaskManager.Current();

                model.StepInfo.SetValid(false);
                Dictionary<string, MetadataPackageModel> list = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                model = CreateSummaryModel.Convert(list, TaskManager.Current());

                if (ValidatePackageModels().Count==0)
                {
                    Submit();
                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASET_TITLE))
                        model.DatasetTitle = TaskManager.Bus[CreateDatasetTaskmanager.DATASET_TITLE].ToString();

                    model.StepInfo.SetStatus(StepStatus.exit);
                    model.PageStatus = Models.PageStatus.LastAndSubmitted;
                }
                else
                {
                    model.PageStatus = Models.PageStatus.Error;
                    model.ErrorList = ValidatePackageModels();
                }
            }

            
            //jump to a other step
            if (index.HasValue)
            {
                TaskManager.SetCurrent(index.Value);
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });

            }
            else
                return PartialView(model);

             
        }

        public void Submit()
        {
            #region create dataset

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

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
                    //if datastructure is not a structured one
                    if (dataStructure == null) dataStructure = dsm.UnStructuredDataStructureRepo.Get(datastructureId);

                    ResearchPlanManager rpm = new ResearchPlanManager();
                    ResearchPlan rp = rpm.Repo.Get(researchPlanId);

                    MetadataStructureManager msm = new MetadataStructureManager();
                    MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                    ds = dm.CreateEmptyDataset(dataStructure, rp, metadataStructure);
                    datasetId = ds.Id;

                    // add security
                    if (GetUserNameOrDefault() != "DEFAULT")
                    {
                        PermissionManager pm = new PermissionManager();
                        pm.CreateDataPermission(ds.Id, 1, GetUserNameOrDefault(), RightType.Read);
                        pm.CreateDataPermission(ds.Id, 1, GetUserNameOrDefault(), RightType.Update);
                        pm.CreateDataPermission(ds.Id, 1, GetUserNameOrDefault(), RightType.Delete);
                    }

                }
                else
                {
                    datasetId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASET_ID]);
                }

                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];





                if (dm.IsDatasetCheckedOutFor(datasetId, GetUserNameOrDefault()) || dm.CheckOutDataset(datasetId, GetUserNameOrDefault()))
                {
                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(datasetId);

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
                    {
                        XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                        workingCopy.Metadata = XmlMetadataWriter.ToXmlDocument(xMetadata);
                    }

                    //XXX TITLE
                    TaskManager.AddToBus(CreateDatasetTaskmanager.DATASET_TITLE, "Title missing");//workingCopy.Metadata.SelectNodes("Metadata/Description/Description/Title/Title")[0].InnerText);


                    TaskManager.AddToBus(CreateDatasetTaskmanager.DATASET_ID, datasetId);

                    dm.EditDatasetVersion(workingCopy, null, null, null);

                    dm.CheckInDataset(datasetId, "Metadata was submited.", GetUserNameOrDefault());
                }
            }

            #endregion
        }

        #region helper
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

        // Check if existing packageModels have errors or not
        // is valid == ture mean no errors
        private bool IsValid()
        {
            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            {
                Dictionary<string, MetadataPackageModel> list = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];

                foreach (MetadataPackageModel packageModel in list.Values)
                {
                    if (packageModel.ErrorList != null)
                    {
                        if (packageModel.ErrorList.Count > 0) return false;
                    }
                }

                return true;
            }

            return false;
        }

        private List<Error> ValidatePackageModels()
        {
            List<Error> errors = new List<Error>();


            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            {
                if(TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
                {
                    MetadataStructureManager msm = new MetadataStructureManager();
                    long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
                    List<MetadataPackageUsage> metadataPackageUsageList = msm.GetEffectivePackages(metadataStructureId);

                    List<MetadataPackageModel> tempModels = new List<MetadataPackageModel>();
                    foreach(MetadataPackageUsage mpu in metadataPackageUsageList)
                    {
                        tempModels = GetMetadataPackageModelsFromBus(mpu.Id);

                        //if tempModel count > 0 , model exist , add if errors exist to error list
                        if (tempModels.Count > 0)
                        {
                            foreach (MetadataPackageModel model in tempModels)
                            {
                                if (model.ErrorList != null)
                                {
                                    if (model.ErrorList.Count() > 0)
                                    {
                                        errors.AddRange(model.ErrorList);
                                    }
                                }
                            }
                        }
                        //mopdel not exist
                        else {

                            if (hasRequiredMetadataAttributeUsage(mpu))
                            {
                                List<MetadataAttributeUsage> listOfRequiredAttributes = GetRequiredMetadataAttributeUsage(mpu);
                                foreach (MetadataAttributeUsage metadataAttributeUsage in listOfRequiredAttributes)
                                {
                                    errors.Add(new Error(ErrorType.MetadataAttribute, "is not optional", new object[] { metadataAttributeUsage.Label, null, 1, 1, mpu.Label }));
                                }
                            }

                        }
                    }
                }
            
            }


            return errors;
        }

        private bool hasRequiredMetadataAttributeUsage(MetadataPackageUsage mpu)
        {
            foreach (MetadataAttributeUsage metadataAttributeUsage in mpu.MetadataPackage.MetadataAttributeUsages)
            {
                if (metadataAttributeUsage.MinCardinality > 0) return true;
            }

            return false;
        }

        private List<MetadataAttributeUsage> GetRequiredMetadataAttributeUsage(MetadataPackageUsage mpu)
        {
            List<MetadataAttributeUsage> list = new List<MetadataAttributeUsage>();

            foreach (MetadataAttributeUsage metadataAttributeUsage in mpu.MetadataPackage.MetadataAttributeUsages)
            {
                if (metadataAttributeUsage.MinCardinality > 0) list.Add(metadataAttributeUsage);
            }

            return list;
        }

        private List<MetadataPackageModel> GetMetadataPackageModelsFromBus(long metadataPackageUsageId)
        {
            Dictionary<string, MetadataPackageModel> packageModelDic = new Dictionary<string, MetadataPackageModel>();
            List<MetadataPackageModel> temp = new List<MetadataPackageModel>();

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            {
                packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];

                foreach (KeyValuePair<string, MetadataPackageModel> keyValuePair in packageModelDic)
                {
                    if (GetPackageUsageIdFromIdentfifier(keyValuePair.Key).Equals(metadataPackageUsageId))
                        temp.Add(keyValuePair.Value);
                }
            }
            else
            {

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST] = packageModelDic;
            }

            return temp;
        }

        #region identifier
        private string CreateIdentfifier(long usageId, int number)
        {
            return usageId + "_" + number;
        }

        private long GetPackageUsageIdFromIdentfifier(string key)
        {
            string[] keyArray = key.Split('_');
            long Id = Convert.ToInt64(keyArray[0]);
            return Id;
        }

        private long GetNumberFromIdentfifier(string key)
        {
            string[] keyArray = key.Split('_');
            long Id = Convert.ToInt64(keyArray[1]);
            return Id;
        }
        #endregion


    }
}
