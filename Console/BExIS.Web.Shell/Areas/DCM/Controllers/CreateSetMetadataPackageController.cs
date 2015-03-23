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
        public ActionResult SetMetadataPackage(int index)
        {

            #region load

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int stepIndex = TaskManager.GetCurrentStepInfoIndex();
            long packageId = GetUsageId(stepIndex);

            //step should executed = true;
            TaskManager.Current().notExecuted = false;

            TaskManager.SetCurrent(index);

            #endregion

            //#region go next success
            
            //TaskManager.SetCurrent(index);
            //TaskManager.GoToNext();
            //Session["TaskManager"] = TaskManager;
            //ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
            //return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });

            //#endregion

            return PartialView("_metadataPackage", CreatePackageModel(index,false));

        }

        
        /// <summary>
        /// Jump to Step on position Index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SetMetadataPackage(int? index, string name=null)
        {
            #region load
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                int stepIndex = TaskManager.GetCurrentStepInfoIndex();
                long packageId = GetUsageId(stepIndex);

                //step should executed = true;
                TaskManager.Current().notExecuted = false;
                
                MetadataPackageModel model = new MetadataPackageModel();
            #endregion

            #region go next success
            
                //TaskManager.AddExecutedStep(TaskManager.Current());
                if (index.HasValue)
                    TaskManager.SetCurrent(index.Value);
                else
                    TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });

            #endregion
        }

        [HttpGet]
        public ActionResult SetMetadataPackageInstanze(int index)
        {
            #region load taskmanager and setup
            bool validateIt = true;
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //set current stepinfo based on index
            if (TaskManager != null)
            {
                // if jump to a other step like the next
                TaskManager.UpdateStepStatus(index);

                TaskManager.SetCurrent(index);

                if (TaskManager.Current().notExecuted)
                {
                    //TaskManager.Current().notExecuted = false;
                    validateIt = false;
                }
                // remove if existing
                //TaskManager.RemoveExecutedStep(TaskManager.Current());
            }

            #endregion

            return PartialView("_setMetadataPackageInstanz", CreatePackageModel(index, validateIt));
        }

        [HttpPost]
        public ActionResult SetMetadataPackageInstanze(int? index, string name = null)
        {

            #region load
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int stepIndex = TaskManager.GetCurrentStepInfoIndex();
            long usageId = GetUsageId(stepIndex);
            int number = 1;
            //step should executed = true;
            TaskManager.Current().notExecuted = false;

            #endregion

            #region validate

            List<Error> errors = validateStep(GetModelFromBus(usageId, number));

            if (errors == null || errors.Count == 0)
            {
                TaskManager.Current().SetValid(true);
                TaskManager.Current().SetStatus(StepStatus.success);
                //UpdateXml(packageId);
            }
            else
            {
                TaskManager.Current().SetValid(false);
                TaskManager.Current().SetStatus(StepStatus.error);
            }
            #endregion

            #region go next success

            //TaskManager.AddExecutedStep(TaskManager.Current());
            if (index.HasValue)
                TaskManager.SetCurrent(index.Value);
            else
                TaskManager.GoToNext();
            Session["TaskManager"] = TaskManager;
            ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
            return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });

            #endregion
        }

        [HttpGet]
        public ActionResult SetMetadataCompoundAttribute(int index)
        {
            #region load
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int stepIndex = TaskManager.GetCurrentStepInfoIndex();


            //step should executed = true;
            TaskManager.Current().notExecuted = false;
            #endregion

            TaskManager.SetCurrent(index);

            return PartialView("_setMetadataCompoundAttribute", CreateCompoundModel(index,false, TaskManager.IsParentChildfromRoot()));

        }

        [HttpPost]
        public ActionResult SetMetadataCompoundAttribute(int? index, string name = null)
        {

            #region load
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int stepIndex = TaskManager.GetCurrentStepInfoIndex();
            long packageId = GetUsageId(stepIndex);

            //step should executed = true;
            TaskManager.Current().notExecuted = false;

            #endregion

            #region go next success

            //TaskManager.AddExecutedStep(TaskManager.Current());
            if (index.HasValue)
                TaskManager.SetCurrent(index.Value);
            else
                TaskManager.GoToNext();
            Session["TaskManager"] = TaskManager;
            ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
            return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });

            #endregion

        }

        [HttpGet]
        public ActionResult SetMetadataCompoundAttributeInstanze(int index)
        {
            #region load taskmanager and setup

            bool validateIt = true;
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            //set current stepinfo based on index
            if (TaskManager != null)
            {
                // if jump to a other step like the next
                TaskManager.UpdateStepStatus(index);

                TaskManager.SetCurrent(index);

                if (TaskManager.Current().notExecuted)
                {
                    //TaskManager.Current().notExecuted = false;
                    validateIt = false;
                }
                // remove if existing
                //TaskManager.RemoveExecutedStep(TaskManager.Current());
            }

            #endregion

            return PartialView("_setMetadataCompoundAttributeInstanz", CreateCompoundModel(index, validateIt, TaskManager.IsParentChildfromRoot()));
        }

        [HttpPost]
        public ActionResult SetMetadataCompoundAttributeInstanze(int? index, string name = null)
        {

            #region load
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int stepIndex = TaskManager.GetCurrentStepInfoIndex();
            long usageId = GetUsageId(stepIndex);
            int number = 1;
            //step should executed = true;
            TaskManager.Current().notExecuted = false;

            #endregion

            #region validate

            List<Error> errors = validateStep(GetModelFromBus(usageId, number));

            if (errors == null || errors.Count == 0)
            {
                TaskManager.Current().SetValid(true);
                TaskManager.Current().SetStatus(StepStatus.success);
                //UpdateXml(packageId);
            }
            else
            {
                TaskManager.Current().SetValid(false);
                TaskManager.Current().SetStatus(StepStatus.error);
            }
            #endregion

            #region go next success

            //TaskManager.AddExecutedStep(TaskManager.Current());
            if (index.HasValue)
                TaskManager.SetCurrent(index.Value);
            else
                TaskManager.GoToNext();
            Session["TaskManager"] = TaskManager;
            ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
            return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });

            #endregion

        }
    



        #region Validation

        //XX number of index des values nötig
        [HttpPost]
        public JsonResult ValidateMetadataAttributeUsage(object value, int id, int parentid, string parentname, int number, int parentModelNumber)
        {
             TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

             int childUsageId = id;
             long parentUsageId = parentid;
             long grantParentUsageID = 0;

            if (parentModelNumber == 0) parentModelNumber = 1;

            if(TaskManager.Current().Parent.Parent!=null)
              grantParentUsageID = GetUsageId(TaskManager.Current().Parent.Parent.Id);
 
             AbstractMetadataStepModel parentModel = GetModelFromBus(parentUsageId);

             AbstractMetadataStepModel grantParentModel = new AbstractMetadataStepModel();

             if(grantParentUsageID != 0)
                grantParentModel = GetModelFromBus(grantParentUsageID);

            Tuple<BaseUsage, BaseUsage> usages = GetUsages(childUsageId, parentUsageId, grantParentUsageID);
            BaseUsage metadataAttributeUsage = usages.Item1;
            BaseUsage parentUsage = usages.Item2;

            //UpdateXml
            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
            MetadataAttributeModel model = MetadataAttributeModel.Convert(metadataAttributeUsage, parentUsage, metadataStructureId, parentModelNumber);
            model.Value = value;
            model.Number = number;

            UpdateAttribute(parentUsage, parentModelNumber, metadataAttributeUsage, number, value);

            if (ContainsAttribute(id, number, parentid, parentModelNumber))
                UpdateAttributeToPackageIntoBus(parentid, parentModelNumber, model);
            else
                AddAttributeToPackageIntoBus(parentid, parentModelNumber, model);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidateStep()
        { 
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int currentStepIndex = TaskManager.GetCurrentStepInfoIndex();
            
            long usageId = GetUsageId(currentStepIndex);
            BaseUsage usage = UsageHelper.GetMetadataAttributeUsageById(usageId);
            int number = 1;


            List<Error> errors = validateStep(GetModelFromBus(usageId, number));

 
            TaskManager.Current().notExecuted = false;

            if(errors==null || errors.Count==0)
                TaskManager.Current().SetStatus(StepStatus.success);
            else
                TaskManager.Current().SetStatus(StepStatus.error);


            if(TaskManager.IsRoot(TaskManager.Current().Parent.Parent))
                return PartialView("_setMetadataPackageInstanz", CreatePackageModel(currentStepIndex, true));
            else
                return PartialView("_setMetadataCompoundAttributeInstanz", CreateCompoundModel(currentStepIndex, true,TaskManager.IsParentChildfromRoot()));

        }

        private List<Error> validateStep(AbstractMetadataStepModel pModel)
        {
            List<Error> errorList = new List<Error>();

            if (pModel != null)
            {

                foreach (MetadataAttributeModel m in pModel.MetadataAttributeModels)
                {
                    List<Error> temp = ValidateAttribute(m);
                    if (temp != null)
                        errorList.AddRange(temp);
                }
            }

            if (errorList.Count == 0)
                return null;
            else
                return errorList;
        }

        private List<Error> ValidateAttribute(MetadataAttributeModel aModel)
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

            //if (TaskManager.IsParentChildfromRoot())
            //{
            //    #region load metadataattribute
            //    metadataAttributeUsage = UsageHelper.GetMetadataCompoundAttributeUsageById(id);
            //    parentUsage = UsageHelper.GetMetadataAttributeUsageById(parentid);
            //    #endregion

            //}
            //else
            //{
            //    metadataAttributeUsage = UsageHelper.GetMetadataCompoundAttributeUsageById(id);
            //    parentUsage = UsageHelper.GetMetadataCompoundAttributeUsageById(parentid);
            //}

            Tuple<BaseUsage, BaseUsage> usages = GetUsages(id, parentid, GetUsageId(TaskManager.Current().Parent.Parent.Id));
            BaseUsage metadataAttributeUsage = usages.Item1;
            BaseUsage parentUsage = usages.Item2;


            MetadataAttributeModel modelAttribute = MetadataAttributeModel.Convert(metadataAttributeUsage, parentUsage, metadataStructureId, parentModelNumber);
            modelAttribute.Number = ++number;

            AbstractMetadataStepModel model = AddAttributeToPackageIntoBus(parentid, parentModelNumber, modelAttribute);

            //addtoxml
            AddAttributeToXml(parentUsage, parentModelNumber, metadataAttributeUsage, number);

            model.ConvertInstance((XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML]);

            if (model != null)
            {

                string key = CreateIdentfifier(parentid, parentModelNumber);
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];


                Dictionary<string, AbstractMetadataStepModel> packageModelDic;

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                    packageModelDic[key] = model;


                    //return PartialView("_setMetadataPackageInstanz", model);
                }


                if (model is MetadataPackageModel)
                {
                    return PartialView("_setMetadataPackageInstanz", model);
                }

                if (model is MetadataCompoundAttributeModel)
                {
                    return PartialView("_setMetadataCompoundAttributeInstanz", model);
                }
            }

            return PartialView("_setMetadataCompoundAttributeInstanz", model);
            
        }

        public ActionResult RemoveMetadataAttributeUsage(object value, int id, int parentid, int number, int parentModelNumber)
        {
            AbstractMetadataStepModel model = RemoveAttributeFromPackageIntoBus(id, number, parentid, parentModelNumber);

            if (model != null)
            {

                string key = CreateIdentfifier(parentid, parentModelNumber);
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                Dictionary<string, AbstractMetadataStepModel> packageModelDic;

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                    //return PartialView("_setMetadataPackageInstanz", packageModelDic[key]);
                }

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
            AbstractMetadataStepModel model = ChangeOrderAttributeFromPackageIntoBus(id, number, parentid, parentModelNumber, true);

            if (model != null)
            {

                string key = CreateIdentfifier(parentid, parentModelNumber);
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                Dictionary<string, AbstractMetadataStepModel> packageModelDic;

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                    //return PartialView("_setMetadataPackageInstanz", packageModelDic[key]);
                }

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
            AbstractMetadataStepModel model = ChangeOrderAttributeFromPackageIntoBus(id, number, parentid, parentModelNumber, false);

            if (model != null)
            {

                string key = CreateIdentfifier(parentid, parentModelNumber);
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                Dictionary<string, AbstractMetadataStepModel> packageModelDic;

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                    //return PartialView("_setMetadataPackageInstanz", packageModelDic[key]);
                }

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

        public ActionResult AddMetadataPackageUsage(long id, int number)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
            MetadataStructureManager msm = new MetadataStructureManager();
            MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

            MetadataPackageUsage metadataPackageUsage = metadataStructure.MetadataPackageUsages.Where(m => m.Id.Equals(id)).FirstOrDefault();

            MetadataPackageModel model = MetadataPackageModel.Convert(metadataPackageUsage, number);
            model.Number = ++number;
            model.ConvertMetadataAttributeModels(metadataPackageUsage.MetadataPackage.MetadataAttributeUsages);

            //add to bus
            AddModelIntoBus(model);

            //add to xml
            AddPackageToXml(model.Source, model.Number);

            // load Instanz
            model.ConvertInstance((XDocument)(TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML]));

            TaskManager.Current().Children.AddRange(UpdateStepsBasedOnUsage(model.Source, TaskManager.Current()));

            //Add to Steps
            model.StepInfo = TaskManager.Current();

            return PartialView("_metadataPackage", model);
        }

        public ActionResult RemoveMetadataPackageUsage(int id, int number)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            AbstractMetadataStepModel model = GetModelFromBus(id, number);
            if (model != null)
            {
                // remove from bus
                RemoveModelFromBus(id, number);
                //remove from xml
                RemovePackageToXml(model.Source, number);

            }
            else
            { 
                //load parent
                long usageId = GetUsageId(id);
                UsageHelper.GetMetadataAttributeUsageById(usageId);
                RemovePackageToXml(model.Source, number);
            }

            model = CreatePackageModel(TaskManager.Current().Id, true);

            TaskManager.Remove(TaskManager.Current(), number-1);

            //Validate
            //model.ErrorList = Validates(model.Source.Id);

            return PartialView("_metadataPackage", model);
        }

        public ActionResult AddMetadataCompundAttributeUsage(int id, int number)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            // get usageid
            int parentStepId = 0;
            if (TaskManager.Current().Parent != null)
            {
                parentStepId = TaskManager.Current().Parent.Id;
            }

            MetadataCompoundAttributeModel model = MetadataCompoundAttributeModel.ConvertToModel(GetCompundAttributeUsage(id,  GetUsageId(parentStepId)), number);
            model.Number = ++number;
            model.ConvertMetadataAttributeModels();

            //add to bus
            AddModelIntoBus(model);

            //add to xml
            AddCompoundAttributeToXml(model.Source, model.Number);

            // load InstanzB
            model.ConvertInstance((XDocument)(TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML]));

            TaskManager.Current().Children.AddRange(UpdateStepsBasedOnUsage(model.Source,TaskManager.Current()));

            //Add to Steps
            model.StepInfo = TaskManager.Current();

            return PartialView("_setMetadataCompoundAttribute", model);
        }

        public ActionResult RemoveMetadataCompundAttributeUsage(int id, int number)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            AbstractMetadataStepModel model = GetModelFromBus(id, number);
            if (model != null)
            {
                // remove from bus
                RemoveModelFromBus(id, number);
                //remove from xml
                RemoveCompoundAttributeToXml(model.Source, number);

            }
            else
            {
                //load parent
                long usageId = GetUsageId(TaskManager.Current().Id);
                RemoveCompoundAttributeToXml(UsageHelper.GetMetadataAttributeUsageById(usageId), number);
            }

            model = CreateCompoundModel(TaskManager.Current().Id, true, TaskManager.IsParentChildfromRoot());

            TaskManager.Remove(TaskManager.Current(), number - 1);

            //Validate
            //model.ErrorList = Validates(model.Source.Id);

            return PartialView("_setMetadataCompoundAttribute", model);
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


                


                if (dm.IsDatasetCheckedOutFor(datasetId, GetUserNameOrDefault()) || dm.CheckOutDataset(datasetId, GetUserNameOrDefault()))
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

        #region package

        private void AddModelIntoBus(AbstractMetadataStepModel model)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            Dictionary<string, AbstractMetadataStepModel> packageModelDic;

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            {
                packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
            }
            else
            {
                packageModelDic = new Dictionary<string, AbstractMetadataStepModel>();
                TaskManager.Bus.Add(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST,packageModelDic);
            }

            string key = CreateIdentfifier(model.Source.Id, model.Number);

            if (!packageModelDic.ContainsKey(key))
                packageModelDic.Add(key, model);

        }

        private void RemoveModelFromBus(long id, int number)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            Dictionary<string, AbstractMetadataStepModel> packageModelDic;

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            {
                packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
            }
            else
            {
                packageModelDic = new Dictionary<string, AbstractMetadataStepModel>();
            }

            string key = CreateIdentfifier(id, number);

            if (packageModelDic.ContainsKey(key))
                packageModelDic.Remove(key);

            //List<AbstractMetadataStepModel> newList = GetModelsFromBus(id).Where(p => p.Number > number).ToList();

            //foreach (AbstractMetadataStepModel mpm in newList)
            //{
            //    string oldID = CreateIdentfifier(mpm.Source.Id, mpm.Number);

            //    if (packageModelDic.ContainsKey(oldID))
            //    {
            //        packageModelDic.Remove(oldID);

            //        mpm.Number = (mpm.Number - 1);
            //        mpm.MetadataAttributeModels.ForEach(a => a.ParentModelNumber = mpm.Number);

            //        string newId = CreateIdentfifier(mpm.Source.Id, mpm.Number);
            //        packageModelDic.Add(newId, mpm);
            //    }
            //}
        }

        private List<AbstractMetadataStepModel> GetModeleFromBus(long usageId)
        {
            Dictionary<string, AbstractMetadataStepModel> packageModelDic = new Dictionary<string, AbstractMetadataStepModel>();
            List<AbstractMetadataStepModel> temp = new List<AbstractMetadataStepModel>();

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            {
                packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];

                foreach (KeyValuePair<string, AbstractMetadataStepModel> keyValuePair in packageModelDic)
                {
                    if (GetPackageUsageIdFromIdentfifier(keyValuePair.Key).Equals(usageId))
                        temp.Add(keyValuePair.Value);
                }
            }
            else
            {

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST] = packageModelDic;
            }

            return temp;
        }

        private AbstractMetadataStepModel GetModelFromBus(long usageId, int number)
        {
            Dictionary<string, AbstractMetadataStepModel> packageModelDic = new Dictionary<string, AbstractMetadataStepModel>();
            List<AbstractMetadataStepModel> temp = new List<AbstractMetadataStepModel>();

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            {
                packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];

                foreach (KeyValuePair<string, AbstractMetadataStepModel> keyValuePair in packageModelDic)
                {
                    if (GetPackageUsageIdFromIdentfifier(keyValuePair.Key).Equals(usageId) &&
                        GetNumberFromIdentfifier(keyValuePair.Key).Equals(number))
                        return keyValuePair.Value;
                }
            }
            else {

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST] = packageModelDic;
            }

            return null;
        }

        private AbstractMetadataStepModel GetModelFromBus(long usageId)
        {
            Dictionary<string, AbstractMetadataStepModel> packageModelDic = new Dictionary<string, AbstractMetadataStepModel>();
            List<AbstractMetadataStepModel> temp = new List<AbstractMetadataStepModel>();

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
            {
                packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];

                foreach (KeyValuePair<string, AbstractMetadataStepModel> keyValuePair in packageModelDic)
                {
                    if (GetPackageUsageIdFromIdentfifier(keyValuePair.Key).Equals(usageId))
                    {
                        return keyValuePair.Value;
                    }
                }
            }
            else
            {

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST] = packageModelDic;
            }

            return null;
        }

        

        #endregion

        #region attribute
            private AbstractMetadataStepModel AddAttributeToPackageIntoBus(long packageId, int packageNumber, MetadataAttributeModel model)
                {
                    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                    Dictionary<string, AbstractMetadataStepModel> packageModelDic;

                    if (TaskManager !=null && TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                    {
                        packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                        string key = CreateIdentfifier(packageId, packageNumber);

                        if (packageModelDic.ContainsKey(key))
                        {
                            //attribute with the same id and get all higher or equals the number
                            List<MetadataAttributeModel> temp = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id == model.Id && a.Number>=model.Number-1).ToList();
                            // default index 0 or last
                            int index = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id == model.Id).ToList().Count();
                            if(temp.Count>0)
                                index = packageModelDic[key].MetadataAttributeModels.IndexOf(temp.FirstOrDefault())+1;
         

                            temp.Where(p => p.Number >= model.Number).ToList().ForEach(p => p.Number = p.Number + 1);

                            packageModelDic[key].MetadataAttributeModels.Insert(index, model);

                            //set first and last
                            List<MetadataAttributeModel> order = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id == model.Id).ToList();
                            order = UpdateFirstAndLast(order);

                            //set count
                            order.ForEach(a => a.NumberOfSourceInPackage = order.Count);

                            return packageModelDic[key];
                        }
                    }

                    return null;
                }

            private AbstractMetadataStepModel RemoveAttributeFromPackageIntoBus(long id, int number, long packageId, int packageNumber)
                {
                    Dictionary<string, AbstractMetadataStepModel> packageModelDic;

                    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                    {
                        packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                        string key = CreateIdentfifier(packageId, packageNumber);

                        if (packageModelDic.ContainsKey(key))
                        {
                            MetadataAttributeModel model =  packageModelDic[key].MetadataAttributeModels.Where(a=>a.Id.Equals(id) && a.Number.Equals(number)).FirstOrDefault();
                            if (model != null)
                            {
                                packageModelDic[key].MetadataAttributeModels.Remove(model);

                                //remove from xml
                                //RemoveAttributeToXml(packageModelDic[key].Source, packageNumber, (MetadataAttributeUsage)model.Source, number);

                                List<MetadataAttributeModel> temp = packageModelDic[key].MetadataAttributeModels.Where(a => a.Number > number).ToList();
                                temp.ForEach(a => a.Number = (a.Number - 1));

                                //set first and last
                                List<MetadataAttributeModel> order = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id == model.Id).ToList();
                                order = UpdateFirstAndLast(order);

                                // set Count of MetadataAttributes in Package
                                order.ForEach(a => a.NumberOfSourceInPackage = order.Count);
                            
                            }
                            return packageModelDic[key];
                        }
                    }

                    return null;
                }

            private AbstractMetadataStepModel ChangeOrderAttributeFromPackageIntoBus(long id, int number, long packageId, int packageNumber, bool ascent)
                {
                    Dictionary<string, AbstractMetadataStepModel> packageModelDic;

                    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                    {
                        packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                        string key = CreateIdentfifier(packageId, packageNumber);

                        if (packageModelDic.ContainsKey(key))
                        {
                            MetadataAttributeModel model = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id.Equals(id) && a.Number.Equals(number)).FirstOrDefault();
                   
                            if (model != null)
                            {
                                int index = packageModelDic[key].MetadataAttributeModels.IndexOf(model);
                                int newIndex = 0;

                                if(ascent)newIndex = --index;
                                else newIndex = ++index;

                                if (newIndex != -1)
                                {
                                    MetadataAttributeModel temp = packageModelDic[key].MetadataAttributeModels.ElementAt(newIndex);

                                    object data = temp.Value;
                                    temp.Value = model.Value;
                                    model.Value = data;
                                }
  
                            }
                            return packageModelDic[key];
                        }
                    }

                    return null;
                }

            private bool ContainsAttribute(long id, int number, long packageId, int packageNumber)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                Dictionary<string, AbstractMetadataStepModel> packageModelDic;

                if (TaskManager != null &&  TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                    string key = CreateIdentfifier(packageId, packageNumber);

                    if (packageModelDic.ContainsKey(key))
                    {
                        return packageModelDic[key].MetadataAttributeModels.Where(a=>a.Id.Equals(id) && a.Number.Equals(number)).Count()>0;
                    }
                }

                return false;
            }

            private void UpdateAttributeToPackageIntoBus(long packageId, int packageNumber, MetadataAttributeModel model)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                Dictionary<string, AbstractMetadataStepModel> packageModelDic;

                if (TaskManager != null &&  TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                    string key = CreateIdentfifier(packageId, packageNumber);


                    if (packageModelDic.ContainsKey(key))
                    {
                        List<MetadataAttributeModel> temp = packageModelDic[key].MetadataAttributeModels;

                        for (int i=0; i < temp.Count(); i++)
                        {
                            if (temp[i].Id.Equals(model.Id) && temp[i].Number.Equals(model.Number))
                                temp[i].Value = model.Value;
                        }
                    
                    }
                }
            }

            private int CountMetadataAttributeFromBus(long packageId, int packageNumber, MetadataAttributeModel model)
            { 
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                Dictionary<string, AbstractMetadataStepModel> packageModelDic;

                if (TaskManager != null && TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, AbstractMetadataStepModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                    string key = CreateIdentfifier(packageId, packageNumber);


                    if (packageModelDic.ContainsKey(key))
                    {
                        List<MetadataAttributeModel> temp = packageModelDic[key].MetadataAttributeModels;

                        return temp.Where(a => a.Source.Id.Equals(model.Source.Id)).Count();
                    }
                }

                return 0;
            }
            
        #endregion

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

        private long GetUsageId(int stepIndex)
        {

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATAPACKAGE_IDS))
            {

                if ((Dictionary<int, long>)TaskManager.Bus[CreateDatasetTaskmanager.METADATAPACKAGE_IDS] != null)
                {
                    Dictionary<int, long> metadataUsageDic = (Dictionary<int, long>)TaskManager.Bus[CreateDatasetTaskmanager.METADATAPACKAGE_IDS];

                    if (metadataUsageDic.ContainsKey(stepIndex)) return Convert.ToInt64(metadataUsageDic[stepIndex]);
                }
            }

            return 0;
                
        }

        private MetadataPackageModel CreatePackageModel(int index, bool validateIt)
        {

            long metadataPackageId = GetUsageId(index);
            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
            {
                metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
            }


            MetadataPackageUsage mpu = mdsManager.GetEffectivePackages(metadataStructureId).Where(p => p.Id.Equals(metadataPackageId)).FirstOrDefault();

            MetadataPackageModel model = new MetadataPackageModel();

            model = MetadataPackageModel.Convert(mpu, getInstanzNumber(index));

            model.ConvertMetadataAttributeModels(mpu.MetadataPackage.MetadataAttributeUsages);

            if (TaskManager.Current().IsInstanze == false)
            {
                //get Instance
                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
                {
                    XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                    model.ConvertInstance(xMetadata);
                }
            }
            else
            {
                if (GetModelFromBus(GetUsageId(index), getInstanzNumber(index)) != null)
                {
                    model = (MetadataPackageModel)GetModelFromBus(GetUsageId(index), getInstanzNumber(index));
                }
                else
                {
                    AddModelIntoBus(model);
                }
            }

            if (validateIt)
            {
                //validate packages
                List<Error> errors = validateStep(GetModelFromBus(model.Source.Id, model.Number));
                if (errors != null)
                    model.ErrorList = errors;
                else
                    model.ErrorList = new List<Error>();

            }

            model.StepInfo = TaskManager.Current();

            return model;
        }

        private MetadataCompoundAttributeModel CreateCompoundModel(int index,bool validateIt,bool parentIsPackage)
        {
            long Id = GetUsageId(index);
            //long parentId = GetUsageId(parentStep);
            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();

            MetadataAttributeManager mam = new MetadataAttributeManager();

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
            {
                metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
            }

            BaseUsage mca;
            if(parentIsPackage)
            {
                mca = UsageHelper.GetMetadataAttributeUsageById(Id);
            }
            else
            {
                //Id ist zu dem zeitpunktt die Id des attributes
                mca = UsageHelper.GetMetadataCompoundAttributeUsageById(Id);
            }

            long parentUsageId = 0;
            long grantParentUsageId = 0;

              if(TaskManager.Current().Parent!=null)
                  parentUsageId = GetUsageId(TaskManager.Current().Parent.Id);

              if (TaskManager.Current().Parent.Parent != null)
                  grantParentUsageId = GetUsageId(TaskManager.Current().Parent.Parent.Id);

            
            MetadataCompoundAttributeModel model = MetadataCompoundAttributeModel.ConvertToModel(mca, getInstanzNumber(index));

            // get children
            model.ConvertMetadataAttributeModels();

            if (TaskManager.Current().IsInstanze == false)
            {
                //get Instance
                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_XML))
                {
                    XDocument xMetadata = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                    model.ConvertInstance(xMetadata);
                }
            }
            else
            {
                if (GetModelFromBus(GetUsageId(index), getInstanzNumber(index)) != null)
                {
                    model = (MetadataCompoundAttributeModel)GetModelFromBus(GetUsageId(index), getInstanzNumber(index));
                }
                else
                {
                    AddModelIntoBus(model);
                } 
            }

            if (validateIt)
            {
                //validate packages
                List<Error> errors = validateStep(model);
                if (errors != null)
                    model.ErrorList = errors;

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

            private void AddPackageToXml(BaseUsage usage, int number)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);


                metadataXml = xmlMetadataWriter.AddPackage(metadataXml,usage, number, UsageHelper.GetNameOfType(usage),UsageHelper.GetIdOfType(usage), UsageHelper.GetChildren(usage),BExIS.Xml.Helpers.XmlNodeType.MetadataPackage, BExIS.Xml.Helpers.XmlNodeType.MetadataPackageUsage);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;

            }

            private void RemovePackageToXml(BaseUsage usage, int number)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                metadataXml = xmlMetadataWriter.RemovePackage(metadataXml, usage, number, UsageHelper.GetNameOfType(usage));

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;
            }

            private void AddCompoundAttributeToXml(BaseUsage usage, int number)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);


                metadataXml = xmlMetadataWriter.AddPackage(metadataXml, usage, number, UsageHelper.GetNameOfType(usage), UsageHelper.GetIdOfType(usage), UsageHelper.GetChildren(usage), BExIS.Xml.Helpers.XmlNodeType.MetadataAttribute, BExIS.Xml.Helpers.XmlNodeType.MetadataAttributeUsage);

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

            private void RemoveAttributeToXml(BaseUsage parentUsage, int packageNumber, MetadataAttributeUsage attribute, int number)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

                metadataXml = xmlMetadataWriter.RemoveAttribute(metadataXml, parentUsage, packageNumber, attribute, number, UsageHelper.GetNameOfType(parentUsage));

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

            private List<StepInfo> UpdateStepsBasedOnUsage(BaseUsage usage, StepInfo currentSelected)
            {
                // genertae action, controller base on usage
                string actionName = "";

                if (usage is MetadataPackageUsage)
                {
                    actionName = "SetMetadataPackageInstanze";
                }
                else
                {
                    actionName = "SetMetadataCompoundAttributeInstanze";
                }

                List<StepInfo> list = new List<StepInfo>();
                Dictionary<int, long> MetadataPackageDic = (Dictionary<int, long>)TaskManager.Bus[CreateDatasetTaskmanager.METADATAPACKAGE_IDS];

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

                        StepInfo current = currentSelected;
                        IEnumerable<XElement> xelements = x.Elements();

                        if (xelements.Count() > 0)
                        {
                            int counter = 0;
                            foreach (XElement element in xelements)
                            {
                                counter++;
                                string title = usage.Label + " (" + counter + ")";

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

                                    s.Children = GetChildrenSteps(usage, s);
                                    //current.Children.Add(s);
                                    list.Add(s);

                                    if (TaskManager.Root.Children.Where(z => z.title.Equals(title)).Count() == 0)
                                    {
                                        MetadataPackageDic.Add(s.Id, id);
                                    }
                                }
                            }
                        }
                    }

                    TaskManager.AddToBus(CreateDatasetTaskmanager.METADATAPACKAGE_IDS, MetadataPackageDic);
                }
                return list;
            }

            private List<StepInfo> GetChildrenSteps(BaseUsage usage, StepInfo parent)
            {
                List<StepInfo> childrenSteps = new List<StepInfo>();
                List<BaseUsage> childrenUsages = UsageHelper.GetCompoundChildrens(usage);
                Dictionary<int, long> MetadataPackageDic = (Dictionary<int, long>)TaskManager.Bus[CreateDatasetTaskmanager.METADATAPACKAGE_IDS];

                foreach (BaseUsage u in childrenUsages)
                {

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

                        s.Children = UpdateStepsBasedOnUsage(u, s).ToList();
                        childrenSteps.Add(s);

                        if (TaskManager.Root.Children.Where(z => z.title.Equals(s.title)).Count() == 0)
                        {
                            MetadataPackageDic.Add(s.Id, u.Id);
                        }
                    }

                }

                return childrenSteps;
            }

        #endregion
     
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

            private Tuple<BaseUsage, BaseUsage> GetUsages(int usageId, long parentUsageId, long grantParentUsageId)
            {

                AbstractMetadataStepModel parentModel = GetModelFromBus(parentUsageId);

                AbstractMetadataStepModel grantParentModel = new AbstractMetadataStepModel();

                if (grantParentUsageId != 0)
                    grantParentModel = GetModelFromBus(grantParentUsageId);

                BaseUsage metadataAttributeUsage = new BaseUsage();
                BaseUsage parentUsage = new BaseUsage();

                // both nested Usages
                if (parentModel is MetadataCompoundAttributeModel && grantParentModel is MetadataCompoundAttributeModel)
                {
                    metadataAttributeUsage = UsageHelper.GetMetadataCompoundAttributeUsageById(usageId);
                    parentUsage = UsageHelper.GetMetadataCompoundAttributeUsageById(parentUsageId);
                }

                // parent = nested Usage && grantParent PackageUsage
                if (parentModel is MetadataCompoundAttributeModel && grantParentModel is MetadataPackageModel)
                {
                    metadataAttributeUsage = UsageHelper.GetMetadataCompoundAttributeUsageById(usageId);
                    parentUsage = UsageHelper.GetMetadataAttributeUsageById(parentUsageId);
                }

                // parent = nested Usage && grantParent PackageUsage
                if (parentModel is MetadataCompoundAttributeModel && grantParentModel == null)
                {
                    metadataAttributeUsage = UsageHelper.GetMetadataCompoundAttributeUsageById(usageId);
                    parentUsage = UsageHelper.GetMetadataCompoundAttributeUsageById(parentUsageId);
                }

                //parent MetadataAttributeUsage grantParent PackageUsage
                if (parentModel is MetadataPackageModel && grantParentUsageId == 0)
                {
                    metadataAttributeUsage = UsageHelper.GetMetadataAttributeUsageById(usageId);
                    parentUsage = UsageHelper.GetUsageById(parentUsageId);
                }

                //parent MetadataAttributeUsage grantParent PackageUsage
                if (parentModel is MetadataPackageModel && grantParentModel is MetadataPackageModel)
                {
                    metadataAttributeUsage = UsageHelper.GetMetadataAttributeUsageById(usageId);
                    parentUsage = UsageHelper.GetUsageById(parentUsageId);
                }

                return new Tuple<BaseUsage, BaseUsage>(metadataAttributeUsage, parentUsage);
            }

            private BaseUsage GetCompundAttributeUsage(int usageId, long parentUsageId)
            {

                AbstractMetadataStepModel parentModel = GetModelFromBus(parentUsageId);

                BaseUsage metadataAttributeUsage = new BaseUsage();
                BaseUsage parentUsage = new BaseUsage();

                // both nested Usages
                if (parentModel is MetadataCompoundAttributeModel)
                {
                    metadataAttributeUsage = UsageHelper.GetMetadataCompoundAttributeUsageById(usageId);
                }

                //parent MetadataAttributeUsage grantParent PackageUsage
                if (parentModel is MetadataPackageModel)
                {
                    metadataAttributeUsage = UsageHelper.GetMetadataAttributeUsageById(usageId);
                }

                if (parentModel == null)
                {
                    metadataAttributeUsage = UsageHelper.GetMetadataAttributeUsageById(usageId);
                }


                return metadataAttributeUsage;
            }

        #endregion

    }
}
