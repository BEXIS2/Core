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
using BExIS.Web.Shell.Areas.DCM.Models;
using BExIS.Web.Shell.Areas.DCM.Models.Create;
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class CreateSetMetadataPackageController : Controller
    {
        //
        // GET: /DCM/CreateSetMetadataPackage/
        private CreateDatasetTaskmanager TaskManager;

        [HttpGet]
        public ActionResult SetMetadataPackage(int stepId)
        {

            #region load

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int stepIndex = TaskManager.GetCurrentStepInfoIndex();
            long packageId = GetUsageId(stepIndex);

            //step should executed = true;
            TaskManager.Current().notExecuted = false;

            TaskManager.SetCurrent(stepId);

            #endregion

            return PartialView("_metadataPackage", CreatePackageModel(stepId, false));

        }

        
        /// <summary>
        /// Jump to Step on position Index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetMetadataPackage(int? stepId, string name = null)
        {
            #region load

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int stepIndex = TaskManager.GetCurrentStepInfoIndex();

            StepModelHelper stepModelHelper = GetStepModelhelper(stepIndex);

            //step should executed = true;
            TaskManager.Current().notExecuted = false;

            #endregion

            #region validate

            Validate(stepModelHelper);

            #endregion

            #region go next success
            
                //TaskManager.AddExecutedStep(TaskManager.Current());
                if (stepId.HasValue)
                    TaskManager.SetCurrent(stepId.Value);
                else
                    TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "stepId", TaskManager.GetCurrentStepInfoIndex() } });

            #endregion
        }

        [HttpGet]
        public ActionResult SetMetadataPackageInstanze(int stepId)
        {
            #region load taskmanager and setup
            bool validateIt = true;
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //set current stepinfo based on index
            if (TaskManager != null)
            {
                // if jump to a other step like the next
                TaskManager.UpdateStepStatus(stepId);

                TaskManager.SetCurrent(stepId);

                if (TaskManager.Current().notExecuted)
                {
                    //TaskManager.Current().notExecuted = false;
                    validateIt = false;
                }
                // remove if existing
                //TaskManager.RemoveExecutedStep(TaskManager.Current());
            }

            #endregion

            return PartialView("_setMetadataPackageInstanz", CreatePackageModel(stepId, validateIt));
        }

        [HttpPost]
        public ActionResult SetMetadataPackageInstanze(int? stepId, string name = null)
        {

            #region load

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int stepIndex = TaskManager.GetCurrentStepInfoIndex();
            StepModelHelper stepModelHelper = GetStepModelhelper(stepIndex);

            //step should executed = true;
            TaskManager.Current().notExecuted = false;

            #endregion

            #region validate

            Validate(stepModelHelper);

            #endregion

            #region go next success

            //TaskManager.AddExecutedStep(TaskManager.Current());
            if (stepId.HasValue)
                TaskManager.SetCurrent(stepId.Value);
            else
                TaskManager.GoToNext();
            Session["TaskManager"] = TaskManager;
            ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
            return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "stepId", TaskManager.GetCurrentStepInfoIndex() } });

            #endregion
        }

        [HttpGet]
        public ActionResult SetMetadataCompoundAttribute(int stepId)
        {
            #region load
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int stepIndex = TaskManager.GetCurrentStepInfoIndex();

            //step should executed = true;
            TaskManager.Current().notExecuted = false;
            #endregion

            TaskManager.SetCurrent(stepId);

            return PartialView("_setMetadataCompoundAttribute", CreateCompoundModel(stepId, false));

        }

        [HttpPost]
        public ActionResult SetMetadataCompoundAttribute(int? stepId, string name = null)
        {

            #region load

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int stepIndex = TaskManager.GetCurrentStepInfoIndex();

            StepModelHelper stepModelHelper = GetStepModelhelper(stepIndex);

            //step should executed = true;
            TaskManager.Current().notExecuted = false;

            #endregion

            #region validate

            Validate(stepModelHelper);

            #endregion

            #region go next success

            //TaskManager.AddExecutedStep(TaskManager.Current());
            if (stepId.HasValue)
                TaskManager.SetCurrent(stepId.Value);
            else
                TaskManager.GoToNext();
            Session["TaskManager"] = TaskManager;
            ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
            return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "stepId", TaskManager.GetCurrentStepInfoIndex() } });

            #endregion

        }

        [HttpGet]
        public ActionResult SetMetadataCompoundAttributeInstanze(int stepId)
        {
            #region load taskmanager and setup

            bool validateIt = true;
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //set current stepinfo based on index
            if (TaskManager != null)
            {
                // if jump to a other step like the next
                TaskManager.UpdateStepStatus(stepId);

                TaskManager.SetCurrent(stepId);

                if (TaskManager.Current().notExecuted)
                {
                    //TaskManager.Current().notExecuted = false;
                    validateIt = false;
                }
                // remove if existing
                //TaskManager.RemoveExecutedStep(TaskManager.Current());
            }

            #endregion

            return PartialView("_setMetadataCompoundAttributeInstanz", CreateCompoundModel(stepId, validateIt));
        }

        [HttpPost]
        public ActionResult SetMetadataCompoundAttributeInstanze(int? stepId, string name = null)
        {

            #region load
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int stepIndex = TaskManager.GetCurrentStepInfoIndex();

            StepModelHelper stepModelHelper = GetStepModelhelper(stepIndex);
            long usageId = stepModelHelper.Usage.Id;
            int number = stepModelHelper.Number;
            //step should executed = true;
            TaskManager.Current().notExecuted = false;

            #endregion

            #region validate
            
            Validate(stepModelHelper);
            
            #endregion

            #region go next success

            //TaskManager.AddExecutedStep(TaskManager.Current());
            if (stepId.HasValue)
                TaskManager.SetCurrent(stepId.Value);
            else
                TaskManager.GoToNext();
            Session["TaskManager"] = TaskManager;
            ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
            return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "stepId", TaskManager.GetCurrentStepInfoIndex() } });

            #endregion

        }
    



        #region Validation

        //XX number of index des values nötig
        [HttpPost]
        public JsonResult ValidateMetadataAttributeUsage(object value, int id, int parentid, string parentname, int number, int parentModelNumber)
        {
             TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

             int currentStepIndex = TaskManager.GetCurrentStepInfoIndex();

             StepModelHelper stepModelHelper = GetStepModelhelper(currentStepIndex);

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
                // select the attributeModel and change the value
                stepModelHelper.Model.MetadataAttributeModels.Where(a => a.Id.Equals(id) && a.Number.Equals(number)).FirstOrDefault().Value = model.Value;

            }
            else
            {
                stepModelHelper.Model.MetadataAttributeModels.Add(model);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidateStep()
        { 
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int currentStepIndex = TaskManager.GetCurrentStepInfoIndex();

            StepModelHelper stepModelHelper = GetStepModelhelper(currentStepIndex);

            long usageId = stepModelHelper.Usage.Id;
            BaseUsage usage = LoadUsage(stepModelHelper.Usage);
            int number = stepModelHelper.Number;


            List<Error> errors = validateStep(stepModelHelper.Model);

            TaskManager.Current().notExecuted = false;

            if(errors==null || errors.Count==0)
                TaskManager.Current().SetStatus(StepStatus.success);
            else
                TaskManager.Current().SetStatus(StepStatus.error);


            if(TaskManager.IsRoot(TaskManager.Current().Parent.Parent))
                return PartialView("_setMetadataPackageInstanz", CreatePackageModel(currentStepIndex, true));
            else
                return PartialView("_setMetadataCompoundAttributeInstanz", CreateCompoundModel(currentStepIndex, true));

        }

        private void Validate(StepModelHelper stepModelHelper)
        {

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            if (stepModelHelper != null)
            {
                List<Error> errors = validateStep(stepModelHelper.Model);

                if (errors == null || errors.Count == 0)
                {
                    TaskManager.Current().SetValid(true);
                    TaskManager.Current().SetStatus(StepStatus.success);
                    //UpdateXml(packageId);
                }
                else
                {
                    stepModelHelper.Model.ErrorList = errors;
                    TaskManager.Current().SetValid(false);
                    TaskManager.Current().SetStatus(StepStatus.error);
                }
            }
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
            if(aModel.MinCardinality>0 && aModel.Value==null)
                errors.Add(new Error(ErrorType.MetadataAttribute,"is not optional",new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));
            else
                if (aModel.MinCardinality > 0 && String.IsNullOrEmpty(aModel.Value.ToString()))
                    errors.Add(new Error(ErrorType.MetadataAttribute, "is not optional", new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));
            
            //check datatype
            if (aModel.Value!=null)
            {
                if (!DataTypeUtility.IsTypeOf(aModel.Value, aModel.SystemType))
                {
                    errors.Add(new Error(ErrorType.MetadataAttribute, "Value can´t convert to the type: " + aModel.SystemType + ".", new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));
                }
            }
            
            // check Constraints
            foreach (Constraint constraint in aModel.GetMetadataAttribute().Constraints)
            {
                if (aModel.Value != null)
                {
                    if (!constraint.IsSatisfied(aModel.Value))
                    {
                        errors.Add(new Error(ErrorType.MetadataAttribute, constraint.ErrorMessage, new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.ParentModelNumber, aModel.Parent.Label }));
                    }
                }
            }


            if(errors.Count==0)
                return null;
            else
                return errors;
        }


        #endregion

        #region Add and Remove

        public ActionResult AddMetadataAttributeUsage(int id, int parentid, int number, int parentModelNumber)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            StepModelHelper stepModelHelperParent = list.Where(s => s.StepId.Equals(TaskManager.Current().Id)).FirstOrDefault();

            BaseUsage parentUsage = LoadUsage(stepModelHelperParent.Usage);
            BaseUsage metadataAttributeUsage = UsageHelper.GetSimpleUsageById(parentUsage, id);

            MetadataAttributeModel modelAttribute = MetadataAttributeModel.Convert(metadataAttributeUsage, parentUsage, metadataStructureId, parentModelNumber, stepModelHelperParent.StepId);
            modelAttribute.Number = ++number;

            AbstractMetadataStepModel model = stepModelHelperParent.Model;

            model.MetadataAttributeModels.Insert(number - 1, modelAttribute);
            UpdateChildrens(stepModelHelperParent);
            

            //addtoxml
            AddAttributeToXml(parentUsage, parentModelNumber, metadataAttributeUsage, number);

            model.ConvertInstance((XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML], stepModelHelperParent.XPath + "//" + metadataAttributeUsage.Label.Replace(" ",string.Empty));
            
            if (model != null)
            {

                if (model is MetadataPackageModel)
                {
                    return PartialView("_setMetadataPackageInstanz", model);
                }

                if (model is MetadataCompoundAttributeModel)
                {
                    return PartialView("_setMetadataCompoundAttributeInstanz", model);
                }
            }

            return PartialView("_setMetadataCompoundAttributeInstanz", new AbstractMetadataStepModel());
            
        }

        public ActionResult RemoveMetadataAttributeUsage(object value, int id, int parentid, int number, int parentModelNumber)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            StepModelHelper stepModelHelperParent = list.Where(s => s.StepId.Equals(TaskManager.Current().Id)).FirstOrDefault();
            stepModelHelperParent.Model.MetadataAttributeModels.RemoveAt(number-1);
            UpdateChildrens(stepModelHelperParent);

            AbstractMetadataStepModel model = stepModelHelperParent.Model;

            BaseUsage parentUsage = stepModelHelperParent.Usage;
            BaseUsage attrUsage = UsageHelper.GetSimpleUsageById(parentUsage, id);

            //remove from xml
            RemoveAttributeToXml(stepModelHelperParent.Usage, stepModelHelperParent.Number, attrUsage, number, UsageHelper.GetNameOfType(attrUsage));

            if (model != null)
            {

                if (model is MetadataPackageModel)
                {
                    return PartialView("_setMetadataPackageInstanz", model);
                }

                if (model is MetadataCompoundAttributeModel)
                {
                    return PartialView("_setMetadataCompoundAttributeInstanz", model);
                }
            }

            return null;
        }

        public ActionResult UpMetadataAttributeUsage(object value, int id, int parentid, int number, int parentModelNumber)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            StepModelHelper stepModelHelperParent = list.Where(s => s.StepId.Equals(TaskManager.Current().Id)).FirstOrDefault();

            Up(stepModelHelperParent, id, number);

            UpdateChildrens(stepModelHelperParent);


            AbstractMetadataStepModel model = stepModelHelperParent.Model;

            if (model != null)
            {
                if (model is MetadataPackageModel)
                {
                    return PartialView("_setMetadataPackageInstanz", model);
                }

                if (model is MetadataCompoundAttributeModel)
                {
                    return PartialView("_setMetadataCompoundAttributeInstanz", model);
                }
            }

            return null;
        }

        public ActionResult DownMetadataAttributeUsage(object value, int id, int parentid, int number, int parentModelNumber)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            List<StepModelHelper> list = (List<StepModelHelper>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_STEP_MODEL_HELPER];

            StepModelHelper stepModelHelperParent = list.Where(s => s.StepId.Equals(TaskManager.Current().Id)).FirstOrDefault();

            Down(stepModelHelperParent, id, number);

            UpdateChildrens(stepModelHelperParent);

            AbstractMetadataStepModel model = stepModelHelperParent.Model;

            if (model != null)
            {
                if (model is MetadataPackageModel)
                {
                    return PartialView("_setMetadataPackageInstanz", model);
                }

                if (model is MetadataCompoundAttributeModel)
                {
                    return PartialView("_setMetadataCompoundAttributeInstanz", model);
                }
            }

            return null;
        }

        public ActionResult AddMetadataPackageUsage(int stepId, int number)
        {

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            StepModelHelper stepModelHelper = GetStepModelhelper(stepId);
            // get usageid
            int parentStepId = 0;
            if (TaskManager.Current().Parent != null)
            {
                parentStepId = TaskManager.Current().Parent.Id;
            }

            MetadataPackageModel model = MetadataPackageModel.Convert(stepModelHelper.Usage, number);
            model.Number = ++number;
            model.ConvertMetadataAttributeModels(LoadUsage(stepModelHelper.Usage), metadataStructureId,stepId);

            BaseUsage u = LoadUsage(stepModelHelper.Usage);

            string label = "";
            if (u is MetadataAttributeUsage)
                label = ((MetadataAttributeUsage)u).MetadataAttribute.Name;

            if (u is MetadataNestedAttributeUsage)
                label = ((MetadataNestedAttributeUsage)u).Member.Name;

            if (u is MetadataPackageUsage)
                label = ((MetadataPackageUsage)u).MetadataPackage.Name;

            StepModelHelper newStepModelhelper = new StepModelHelper
            {
                StepId = stepModelHelper.StepId,
                Usage = stepModelHelper.Usage,
                Number = model.Number,
                Model = model,
                XPath = stepModelHelper.XPath + "//" + label.Replace(" ",string.Empty) + "[" + model.Number + "]"
            };

            AddStepModelhelper(newStepModelhelper);

            //add to bus
            //AddModelIntoBus(model);

            //add to xml
            AddCompoundAttributeToXml(model.Source, model.Number, stepModelHelper.XPath);

            // load InstanzB
            model.ConvertInstance((XDocument)(TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML]), stepModelHelper.XPath);

            TaskManager.Current().Children.AddRange(UpdateStepsBasedOnUsage(newStepModelhelper.Usage, TaskManager.Current(), stepModelHelper.XPath));

            //Add to Steps
            model.StepInfo = TaskManager.Current();

            return PartialView("_metadataPackage", model);
        }

        public ActionResult RemoveMetadataPackageUsage(int stepId, int number)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            StepModelHelper stepModelHelper = GetStepModelhelper(stepId);
            RemoveFromXml(stepModelHelper.XPath + "//" + UsageHelper.GetNameOfType(stepModelHelper.Usage).Replace(" ",string.Empty) + "[" + number + "]");


            stepModelHelper.Model = CreatePackageModel(TaskManager.Current().Id, true);

            TaskManager.Remove(TaskManager.Current(), number - 1);

            return PartialView("_metadataPackage", stepModelHelper.Model);
        }

        public ActionResult AddMetadataCompundAttributeUsage(int stepId, int number)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            StepModelHelper stepModelHelper = GetStepModelhelper(stepId);
            // get usageid
            int parentStepId = 0;
            if (TaskManager.Current().Parent != null)
            {
                parentStepId = TaskManager.Current().Parent.Id;
            }

            MetadataCompoundAttributeModel model = MetadataCompoundAttributeModel.ConvertToModel(stepModelHelper.Usage, number);
            model.Number = ++number;
            model.ConvertMetadataAttributeModels(LoadUsage(stepModelHelper.Usage), metadataStructureId,stepId);

            BaseUsage u = LoadUsage(stepModelHelper.Usage);

            string label = "";
            if(u is MetadataAttributeUsage)
                label = ((MetadataAttributeUsage)u).MetadataAttribute.Name;

            if(u is MetadataNestedAttributeUsage)
                label = ((MetadataNestedAttributeUsage)u).Member.Name;


            StepModelHelper newStepModelhelper = new StepModelHelper
            {
                StepId = stepModelHelper.StepId,
                Usage = stepModelHelper.Usage,
                Number = model.Number,
                Model = model,
                XPath = stepModelHelper.XPath + "//" + label.Replace(" ",string.Empty) + "[" + model.Number + "]"
            };

            AddStepModelhelper(newStepModelhelper);

            //add to bus
            //AddModelIntoBus(model);

            //add to xml
            AddCompoundAttributeToXml(model.Source, model.Number, stepModelHelper.XPath);

            // load InstanzB
            model.ConvertInstance((XDocument)(TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML]), stepModelHelper.XPath);

            TaskManager.Current().Children.AddRange(UpdateStepsBasedOnUsage(newStepModelhelper.Usage, TaskManager.Current(), stepModelHelper.XPath));

            //Add to Steps
            model.StepInfo = TaskManager.Current();

            return PartialView("_setMetadataCompoundAttribute", model);
        }

        public ActionResult RemoveMetadataCompundAttributeUsage(int stepId, int number)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            StepModelHelper stepModelHelper = GetStepModelhelper(stepId);
            RemoveFromXml(stepModelHelper.XPath+"//"+UsageHelper.GetNameOfType(stepModelHelper.Usage)+"["+number+"]");


            stepModelHelper.Model = CreateCompoundModel(TaskManager.Current().Id, true);

            TaskManager.Remove(TaskManager.Current(), number - 1);

            return PartialView("_setMetadataCompoundAttribute", stepModelHelper.Model);
        }


        #endregion

        #region save

        public ActionResult Save()
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int currentStepIndex = TaskManager.GetCurrentStepInfoIndex();
            long packageId = GetUsageId(currentStepIndex);

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


                


                if (dm.IsDatasetCheckedOutFor(datasetId, GetUsernameOrDefault()) || dm.CheckOutDataset(datasetId, GetUsernameOrDefault()))
                {
                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(datasetId);

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
                    {
                        XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                        workingCopy.Metadata = XmlMetadataWriter.ToXmlDocument(xMetadata);
                    }

                    TaskManager.AddToBus(CreateDatasetTaskmanager.DATASET_ID, datasetId);

                    dm.EditDatasetVersion(workingCopy, null, null, null);
                }
            }

            #endregion


            return PartialView("SetMetadataPackage", CreatePackageModel(currentStepIndex, true));
        }
        #endregion

        #region bus functions

        #region attribute

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

            private StepModelHelper Up(StepModelHelper stepModelHelperParent, long id, int number)
            {
                List<MetadataAttributeModel> list = stepModelHelperParent.Model.MetadataAttributeModels;

                MetadataAttributeModel temp = list.Where(m => m.Id.Equals(id) && m.Number.Equals(number)).FirstOrDefault();
                int index = list.IndexOf(temp);

                list.RemoveAt(index);
                list.Insert(number-2, temp);

                return stepModelHelperParent;
            }

            private StepModelHelper Down(StepModelHelper stepModelHelperParent, long id, int number)
            {
                List<MetadataAttributeModel> list = stepModelHelperParent.Model.MetadataAttributeModels;

                MetadataAttributeModel temp = list.Where(m => m.Id.Equals(id) && m.Number.Equals(number)).FirstOrDefault();
                int index = list.IndexOf(temp);

                list.RemoveAt(index);
                list.Insert(number, temp);

                return stepModelHelperParent;
            }
            //private AbstractMetadataStepModel AddAttributeToPackageIntoBus(StepModelHelper parent, MetadataAttributeModel model)
            //{
            //    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //    if (TaskManager != null )
            //    {
            //        parent.Model.MetadataAttributeModels.Add(model);
                   

            //        //if (packageModelDic.ContainsKey(key))
            //        //{
            //        //    //attribute with the same id and get all higher or equals the number
            //        //    List<MetadataAttributeModel> temp = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id == model.Id && a.Number >= model.Number - 1).ToList();
            //        //    // default index 0 or last
            //        //    int index = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id == model.Id).ToList().Count();
            //        //    if (temp.Count > 0)
            //        //        index = packageModelDic[key].MetadataAttributeModels.IndexOf(temp.FirstOrDefault()) + 1;


            //        //    temp.Where(p => p.Number >= model.Number).ToList().ForEach(p => p.Number = p.Number + 1);

            //        //    packageModelDic[key].MetadataAttributeModels.Insert(index, model);

            //        //    //set first and last
            //        //    List<MetadataAttributeModel> order = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id == model.Id).ToList();
            //        //    order = UpdateFirstAndLast(order);

            //        //    //set count
            //        //    order.ForEach(a => a.NumberOfSourceInPackage = order.Count);

            //        //    return packageModelDic[key];
            //        //}
            //    }

            //    return null;
            //}

            //private AbstractMetadataStepModel RemoveAttributeFromPackageIntoBus(long id, int number, long packageId, int packageNumber)
            //{
                

            //    return null;
            //}

            //private AbstractMetadataStepModel ChangeOrderAttributeFromPackageIntoBus(long id, int number, long packageId, int packageNumber, bool ascent)
            //{
            //    Dictionary<string, AbstractMetadataStepModel> packageModelDic;

            //    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            //    {
            //        packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
            //        string key = CreateIdentfifier(packageId, packageNumber);

            //        if (packageModelDic.ContainsKey(key))
            //        {
            //            MetadataAttributeModel model = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id.Equals(id) && a.Number.Equals(number)).FirstOrDefault();
                   
            //            if (model != null)
            //            {
            //                int index = packageModelDic[key].MetadataAttributeModels.IndexOf(model);
            //                int newIndex = 0;

            //                if(ascent)newIndex = --index;
            //                else newIndex = ++index;

            //                if (newIndex != -1)
            //                {
            //                    MetadataAttributeModel temp = packageModelDic[key].MetadataAttributeModels.ElementAt(newIndex);

            //                    object data = temp.Value;
            //                    temp.Value = model.Value;
            //                    model.Value = data;
            //                }
  
            //            }
            //            return packageModelDic[key];
            //        }
            //    }

            //    return null;
            //}

            //private bool ContainsAttribute(long id, int number, long packageId, int packageNumber)
            //{
            //    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //    Dictionary<string, AbstractMetadataStepModel> packageModelDic;

            //    if (TaskManager != null &&  TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            //    {
            //        packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
            //        string key = CreateIdentfifier(packageId, packageNumber);

            //        if (packageModelDic.ContainsKey(key))
            //        {
            //            return packageModelDic[key].MetadataAttributeModels.Where(a=>a.Id.Equals(id) && a.Number.Equals(number)).Count()>0;
            //        }
            //    }

            //    return false;
            //}

            //private void UpdateAttributeToPackageIntoBus(long packageId, int packageNumber, MetadataAttributeModel model)
            //{
            //    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //    Dictionary<string, AbstractMetadataStepModel> packageModelDic;

            //    if (TaskManager != null &&  TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            //    {
            //        packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
            //        string key = CreateIdentfifier(packageId, packageNumber);


            //        if (packageModelDic.ContainsKey(key))
            //        {
            //            List<MetadataAttributeModel> temp = packageModelDic[key].MetadataAttributeModels;

            //            for (int i=0; i < temp.Count(); i++)
            //            {
            //                if (temp[i].Id.Equals(model.Id) && temp[i].Number.Equals(model.Number))
            //                    temp[i].Value = model.Value;
            //            }
                    
            //        }
            //    }
            //}

            //private int CountMetadataAttributeFromBus(long packageId, int packageNumber, MetadataAttributeModel model)
            //{ 
            //    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //    Dictionary<string, AbstractMetadataStepModel> packageModelDic;

            //    if (TaskManager != null && TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            //    {
            //        packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
            //        string key = CreateIdentfifier(packageId, packageNumber);


            //        if (packageModelDic.ContainsKey(key))
            //        {
            //            List<MetadataAttributeModel> temp = packageModelDic[key].MetadataAttributeModels;

            //            return temp.Where(a => a.Source.Id.Equals(model.Source.Id)).Count();
            //        }
            //    }

            //    return 0;
            //}
            
        #endregion


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

        private MetadataPackageModel CreatePackageModel(int stepId, bool validateIt)
        {

            StepModelHelper stepModelHelper = GetStepModelhelper(stepId);

            long metadataPackageId = stepModelHelper.Usage.Id;
            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();
            MetadataPackageUsage mpu = (MetadataPackageUsage)LoadUsage(stepModelHelper.Usage);
            MetadataPackageModel model = new MetadataPackageModel();

            model = MetadataPackageModel.Convert(mpu, stepModelHelper.Number);
            model.ConvertMetadataAttributeModels(mpu,metadataStructureId, stepId);

            if (TaskManager.Current().IsInstanze == false)
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

            if (validateIt)
            {
                //validate packages
                List<Error> errors = validateStep(stepModelHelper.Model);
                if (errors != null)
                    model.ErrorList = errors;
                else
                    model.ErrorList = new List<Error>();

            }

            model.StepInfo = TaskManager.Current();

            return model;
        }

        private MetadataCompoundAttributeModel CreateCompoundModel(int stepId,bool validateIt)
        {

            StepModelHelper stepModelHelper = GetStepModelhelper(stepId);

            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
            long Id = stepModelHelper.Usage.Id;

            MetadataCompoundAttributeModel model = MetadataCompoundAttributeModel.ConvertToModel(stepModelHelper.Usage, stepModelHelper.Number);

            // get children
            model.ConvertMetadataAttributeModels(LoadUsage(stepModelHelper.Usage), metadataStructureId,stepId);

            if (TaskManager.Current().IsInstanze == false)
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

            if (validateIt)
            {
                //validate packages
                List<Error> errors = validateStep(stepModelHelper.Model);
                if (errors != null)
                    model.ErrorList = errors;
                else
                    model.ErrorList = new List<Error>();

            }
            

            model.StepInfo = TaskManager.Current();

            return model;
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

        #region xml

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
                metadataXml = xmlMetadataWriter.AddAttribute(metadataXml, parentUsage, parentNumber, attribute, number, UsageHelper.GetNameOfType(parentUsage),UsageHelper.GetNameOfType(attribute), UsageHelper.GetIdOfType(attribute).ToString());

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


        #endregion

        #region steps

            private int getInstanzNumber(int stepId)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                Dictionary<int, long> StepUsageRefs = new Dictionary<int, long>();
                if(TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATAPACKAGE_IDS))
                {
                    StepUsageRefs = (Dictionary<int, long>)TaskManager.Bus[CreateDatasetTaskmanager.METADATAPACKAGE_IDS];

                    var temp = from stepUsageRef in StepUsageRefs
                                     where stepUsageRef.Value.Equals(GetUsageId(stepId))
                                     select stepUsageRef.Key;

                    if (temp.Contains(stepId))
                    {
                        List<int> resultList = temp.ToList();
                        resultList.Sort();

                        return resultList.IndexOf(stepId);
                    }
                }
                return 0;
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

                                    string xPath = parentXpath + "//" + childName.Replace(" ",string.Empty) + "[" + counter + "]";

                                    s.Children = GetChildrenSteps(usage, s, xPath);
                                    //current.Children.Add(s);
                                    list.Add(s);

                                    if (TaskManager.Root.Children.Where(z => z.title.Equals(title)).Count() == 0)
                                    {
                                        stepHelperModelList.Add(new StepModelHelper(s.Id, counter, usage, xPath));
                                    }
                                }
                            }
                        }
                    }

                }
                return list;
            }

            private List<StepInfo> GetChildrenSteps(BaseUsage usage, StepInfo parent, string parentXpath)
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
     
        #region helper

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

    }
}
