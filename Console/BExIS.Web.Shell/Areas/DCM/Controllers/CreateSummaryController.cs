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
using BExIS.Xml.Helpers;
using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Security.Services.Objects;
using System.Xml;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Xml.Services;
using BExIS.Dlm.Entities.Common;
using BExIS.Web.Shell.Areas.DCM.Models;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class CreateSummaryController : Controller
    {
        CreateDatasetTaskmanager TaskManager;

        //
        // GET: /DCM/CreateSummary/
        [HttpGet]
        public ActionResult Summary(int stepId)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            CreateSummaryModel model = new CreateSummaryModel();
            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.UpdateStepStatus(stepId);
                TaskManager.SetCurrent(stepId);
                Session["CreateDatasetTaskmanager"] = TaskManager;

                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];


                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER))
                {
                    List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];
                    model = CreateSummaryModel.Convert(list, TaskManager.Current());
                    model.AllErrors = ValidatePackageModels();
                    model.PageStatus = Models.PageStatus.FirstLoad;
                    return PartialView(model);
                }
                else
                {
                    TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST] = new Dictionary<string, MetadataPackageModel>();
                }

                model.AllErrors = ValidatePackageModels();
            }

           model.StepInfo = TaskManager.Current();
           return PartialView(model);
        }

        [HttpPost]
        public ActionResult Summary(int? stepId, string name = null)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //set current stepinfo based on index
            if (TaskManager != null)
            {

                Session["CreateDatasetTaskmanager"] = TaskManager;
            }

         
            CreateSummaryModel model = new CreateSummaryModel();
            // prepare model
             if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER))
             {
                model.StepInfo = TaskManager.Current();

                model.StepInfo.SetValid(false);

                List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];
                model = CreateSummaryModel.Convert(list, TaskManager.Current());

                //if (ValidatePackageModels().Count==0)
                //{

                    Submit();

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASET_TITLE))
                    {
                        model.DatasetTitle = TaskManager.Bus[CreateDatasetTaskmanager.DATASET_TITLE].ToString();
                        if (String.IsNullOrEmpty(model.DatasetTitle))
                        {
                            model.DatasetTitle = "No title available";
                            TaskManager.Bus[CreateDatasetTaskmanager.DATASET_TITLE] = model.DatasetTitle; 
                        }
                    }
                    else
                    {
                        TaskManager.Bus[CreateDatasetTaskmanager.DATASET_TITLE] = "No title available";
                        model.DatasetTitle = TaskManager.Bus[CreateDatasetTaskmanager.DATASET_TITLE].ToString();
                    }

                    
                    model.DatasetId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.DATASET_ID]);
                    model.StepInfo.SetStatus(StepStatus.exit);
                    model.PageStatus = Models.PageStatus.LastAndSubmitted;
                //}
                //else
                //{
                //    model.PageStatus = Models.PageStatus.Error;
                //    model.ErrorList = ValidatePackageModels();
                //}
                }

            
            //jump to a other step
            if (stepId.HasValue)
            {
                TaskManager.SetCurrent(stepId.Value);
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "stepId", TaskManager.GetCurrentStepInfoIndex() } });

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
                        SubjectManager sm = new SubjectManager();

                        BExIS.Security.Entities.Subjects.User user = sm.GetUserByName(GetUserNameOrDefault());

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

                if (dm.IsDatasetCheckedOutFor(datasetId, GetUserNameOrDefault()) || dm.CheckOutDataset(datasetId, GetUserNameOrDefault()))
                {
                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(datasetId);

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
                    {
                        XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                        workingCopy.Metadata = XmlMetadataWriter.ToXmlDocument(xMetadata);
                    }

                    TaskManager.AddToBus(CreateDatasetTaskmanager.DATASET_TITLE, XmlDatasetHelper.GetInformation(workingCopy, AttributeNames.title));//workingCopy.Metadata.SelectNodes("Metadata/Description/Description/Title/Title")[0].InnerText);

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

        private List<Tuple<StepInfo,List<Error>>> ValidatePackageModels()
        {
            List<Tuple<StepInfo,List<Error>>> errors = new List<Tuple<StepInfo,List<Error>>>();;

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            foreach(StepModelHelper stepModeHelper in list)
            {
                // if model exist then validate attributes
                if (stepModeHelper.Model != null)
                {
                    if (stepModeHelper.Model.StepInfo.stepStatus == StepStatus.error)
                        errors.Add(new Tuple<StepInfo,List<Error>>(stepModeHelper.Model.StepInfo,stepModeHelper.Model.ErrorList));
                }
                // else check for required elements 
                else
                { 
                    stepModeHelper.Usage = LoadUsage(stepModeHelper.Usage);
                    if(UsageHelper.HasUsagesWithSimpleType(stepModeHelper.Usage))
                    {
                        if(UsageHelper.HasRequiredSimpleTypes(stepModeHelper.Usage))
                        {
                            StepInfo step = TaskManager.Get(stepModeHelper.StepId);
                            if (step != null && step.IsInstanze)
                            {
                                Error error = new Error(ErrorType.Other, String.Format("{0} : {1} {2}", "Step: ", stepModeHelper.Usage.Label, "is not valid. There are fields that are required and not yet completed are."));
                                List<Error> tempErrors = new List<Error>();
                                tempErrors.Add(error);
                                
                                errors.Add(new Tuple<StepInfo,List<Error>>(step,tempErrors));

                                step.stepStatus = StepStatus.error;
                            }
                        }
                    }
                }


            }

            return errors;
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
    }
}
