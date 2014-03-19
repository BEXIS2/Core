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
using BExIS.Web.Shell.Areas.DCM.Models.Metadata;
using BExIS.Xml.Services;
using Vaiona.Util.Cfg;

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

            return PartialView(Create(index, validateIt));
        }

        [HttpPost]
        public ActionResult SetMetadataPackage()
        {
            #region load
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                int stepIndex = TaskManager.GetCurrentStepInfoIndex();
                long packageId = GetPackageId(stepIndex);

                //step should executed = true;
                TaskManager.Current().notExecuted = false;
                
                MetadataPackageContainerModel model = new MetadataPackageContainerModel();
            #endregion

            #region validate

                List<Error> errors = ValidatePackageContainer(packageId);
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
                TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });

            #endregion


                //return PartialView(Create(stepIndex));
        }

        #region Validation

        //XX number of index des values nötig
        [HttpPost]
        public JsonResult ValidateMetadataAttributeUsage(object value, int id, int parentid, string parentname, int metadataStructureId, int number, int packageModelNumber)
        {
            #region load metadataattribute
                MetadataStructureManager msm = new MetadataStructureManager();
                MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                MetadataPackageUsage metadataPackageUsage = metadataStructure.MetadataPackageUsages.Where(m => m.Id.Equals(parentid)).FirstOrDefault();
                MetadataAttributeUsage metadataAttributeUsage = metadataPackageUsage.MetadataPackage.MetadataAttributeUsages.Where(m => m.Id.Equals(id)).FirstOrDefault();

            #endregion

            MetadataAttributeModel model = MetadataAttributeModel.Convert(metadataAttributeUsage, metadataPackageUsage, metadataStructure.Id, packageModelNumber);
            model.Value = value;
            model.Number = number;

            //UpdateXml
            UpdateAttribute(metadataPackageUsage, packageModelNumber, metadataAttributeUsage, number, value);

            if (ContainsAttribute(id, number, parentid, packageModelNumber))
                UpdateAttributeToPackageIntoBus(parentid, packageModelNumber, model);
            else
                AddAttributeToPackageIntoBus(parentid, packageModelNumber, model);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ValidateStep()
        { 
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int currentStepIndex = TaskManager.GetCurrentStepInfoIndex();
            long packageId = GetPackageId(currentStepIndex);
            
            List<Error> errors = ValidatePackageContainer(packageId);

            TaskManager.Current().notExecuted = false;

            if(errors==null || errors.Count==0)
                TaskManager.Current().SetStatus(StepStatus.success);
            else
                TaskManager.Current().SetStatus(StepStatus.error);

            return PartialView("SetMetadataPackage", Create(currentStepIndex, true));
        }

        private List<Error> ValidatePackageContainer(long pId)
        {
            List<Error> errorList = new List<Error>();
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

            List<MetadataPackageModel> metadataPackageModelList = new List<MetadataPackageModel>();
            // get stored MetadataPackages from Bus
            metadataPackageModelList = GetMetadataPackageModelsFromBus(pId);

            foreach (MetadataPackageModel m in metadataPackageModelList)
            {
                List<Error> temp = ValidatePackage(m);
                if(temp!=null)
                    errorList.AddRange(temp);
            }

            if(errorList.Count==0)
                return null;
            else
                return errorList;
        
        }

        private List<Error> ValidatePackage(MetadataPackageModel pModel)
        {
            List<Error> errorList = new List<Error>();

            foreach (MetadataAttributeModel m in pModel.MetadataAttributeModels)
            {
                List<Error> temp = ValidateAttribute(m);
                if (temp != null)
                    errorList.AddRange(temp);
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
            if(aModel.MinCardinality>0 && aModel.Value == null)
                errors.Add(new Error(ErrorType.MetadataAttribute,"is not optional",new object[] { aModel.DisplayName, aModel.Value, aModel.Number, aModel.Source.Label }));
            if(errors.Count==0)
                return null;
            else
                return errors;
        }


        #endregion

        #region Add and Remove
            public ActionResult AddMetadataAttributeUsage(int id, int parentid, int metadataStructureId, int number, int packageModelNumber)
            {
                MetadataStructureManager msm = new MetadataStructureManager();
                MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                MetadataPackageUsage metadataPackageUsage = metadataStructure.MetadataPackageUsages.Where(m => m.Id.Equals(parentid)).FirstOrDefault();
                MetadataAttributeUsage metadataAttributeUsage = metadataPackageUsage.MetadataPackage.MetadataAttributeUsages.Where(m => m.Id.Equals(id)).FirstOrDefault();

                MetadataAttributeModel modelAttribute = MetadataAttributeModel.Convert(metadataAttributeUsage, metadataPackageUsage, metadataStructure.Id, packageModelNumber);
                modelAttribute.Number = ++number;

                MetadataPackageModel model = AddAttributeToPackageIntoBus(parentid, packageModelNumber, modelAttribute);

                //addtoxml
                AddAttributeToXml(metadataPackageUsage, packageModelNumber, metadataAttributeUsage, number);

                if (model != null)
                {

                    string key = CreateIdentfifier(parentid, packageModelNumber);
                    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];


                    Dictionary<string, MetadataPackageModel> packageModelDic;

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                    {
                        packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                        return PartialView("_metadataPackage", packageModelDic[key]);
                    }
                }


                return PartialView("_metadataPackage", model);
            }

            public ActionResult RemoveMetadataAttributeUsage(object value, int id, int parentid, long metadataStructureId, int number,int packageModelNumber)
            {
                MetadataPackageModel model = RemoveAttributeFromPackageIntoBus(id,number,parentid, packageModelNumber);

                

                if (model != null)
                {

                    string key = CreateIdentfifier(parentid, packageModelNumber);
                    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                    Dictionary<string, MetadataPackageModel> packageModelDic;

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                    {
                        packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                        return PartialView("_metadataPackage", packageModelDic[key]);
                    }
                }

                return null;
            }

            public ActionResult UpMetadataAttributeUsage(object value, int id, int parentid, long metadataStructureId, int number, int packageModelNumber)
            {
                MetadataPackageModel model = ChangeOrderAttributeFromPackageIntoBus(id, number, parentid, packageModelNumber,true);

                if (model != null)
                {

                    string key = CreateIdentfifier(parentid, packageModelNumber);
                    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                    Dictionary<string, MetadataPackageModel> packageModelDic;

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                    {
                        packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                        return PartialView("_metadataPackage", packageModelDic[key]);
                    }
                }

                return null;
            }

            public ActionResult DownMetadataAttributeUsage(object value, int id, int parentid, long metadataStructureId, int number, int packageModelNumber)
            {
                MetadataPackageModel model = ChangeOrderAttributeFromPackageIntoBus(id, number, parentid, packageModelNumber, false);

                if (model != null)
                {

                    string key = CreateIdentfifier(parentid, packageModelNumber);
                    TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                    Dictionary<string, MetadataPackageModel> packageModelDic;

                    if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                    {
                        packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                        return PartialView("_metadataPackage", packageModelDic[key]);
                    }
                }

                return null;
            }

            public ActionResult AddMetadataPackageUsage(int id, int metadataStructureId, int number)
            {
                MetadataStructureManager msm = new MetadataStructureManager();
                MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                MetadataPackageUsage metadataPackageUsage = metadataStructure.MetadataPackageUsages.Where(m => m.Id.Equals(id)).FirstOrDefault();

                MetadataPackageModel model = MetadataPackageModel.Convert(metadataPackageUsage);
                model.Number = ++number;

                //add to bus
                AddPackageIntoBus(model);

                //add to xml
                AddPackageToXml(model.Source, model.Number);

                foreach (MetadataAttributeUsage mau in model.Source.MetadataPackage.MetadataAttributeUsages)
                {
                    model.MetadataAttributeModels.Add(MetadataAttributeModel.Convert(mau, metadataPackageUsage, metadataStructureId, model.Number));
                }

                return PartialView("_metadataPackage", model);
            }

            public ActionResult RemoveMetadataPackageUsage(int id, int number)
            { 
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                 
                 MetadataPackageContainerModel model = Create(TaskManager.GetCurrentStepInfoIndex(),true);

                 // remove from bus
                 model.MetadataPackageModel = RemovePackageFromBus(id, number);

                 //remove from xml
                 RemovePackageToXml(model.Source, number);

                 return PartialView("SetMetadataPackage", model);
            }
        #endregion

        #region save

        public ActionResult Save()
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            int currentStepIndex = TaskManager.GetCurrentStepInfoIndex();
            long packageId = GetPackageId(currentStepIndex);

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


            return PartialView("SetMetadataPackage", Create(currentStepIndex, true));
        }
        #endregion

        #region bus functions

        #region package
        private void AddPackageIntoBus(MetadataPackageModel model)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                Dictionary<string, MetadataPackageModel> packageModelDic;

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                }
                else
                {
                    packageModelDic = new Dictionary<string, MetadataPackageModel>();
                }

                string key = CreateIdentfifier(model.Source.Id, model.Number);

                if (!packageModelDic.ContainsKey(key))
                    packageModelDic.Add(key, model);

                    
            }

            private List<MetadataPackageModel> RemovePackageFromBus(long id, int number)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                Dictionary<string, MetadataPackageModel> packageModelDic;

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                }
                else
                {
                    packageModelDic = new Dictionary<string, MetadataPackageModel>();
                }

                string key = CreateIdentfifier(id, number);

                if (packageModelDic.ContainsKey(key))
                    packageModelDic.Remove(key);

                List<MetadataPackageModel> newList = GetMetadataPackageModelsFromBus(id).Where(p => p.Number > number).ToList();

                foreach (MetadataPackageModel mpm in newList)
                {
                    string oldID = CreateIdentfifier(mpm.Source.Id, mpm.Number);

                    if (packageModelDic.ContainsKey(oldID))
                    {
                        packageModelDic.Remove(oldID);

                        mpm.Number = (mpm.Number - 1);
                        mpm.MetadataAttributeModels.ForEach(a => a.PackageModelNumber = mpm.Number);

                        string newId = CreateIdentfifier(mpm.Source.Id, mpm.Number);
                        packageModelDic.Add(newId, mpm);
                    }
                }
                return GetMetadataPackageModelsFromBus(id);
    
            }

            private List<MetadataPackageModel> GetMetadataPackageModelsFromBus(long metadataPackageUsageId)
            {
                Dictionary<string, MetadataPackageModel> packageModelDic = new Dictionary<string,MetadataPackageModel>();
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
                else {

                    TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST] = packageModelDic;
                }

                return temp;
            }
        #endregion

        #region attribute
            private MetadataPackageModel AddAttributeToPackageIntoBus(long packageId, int packageNumber, MetadataAttributeModel model)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                Dictionary<string, MetadataPackageModel> packageModelDic;

                if (TaskManager !=null && TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                    string key = CreateIdentfifier(packageId, packageNumber);

                    if (packageModelDic.ContainsKey(key))
                    {
                        //attribute with the same id and get all higher or equals the number
                        List<MetadataAttributeModel> temp = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id == model.Id && a.Number>=model.Number).ToList();
                        // default index 0 or last
                        int index = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id == model.Id).ToList().Count();
                        if(temp.Count>0)
                            index = packageModelDic[key].MetadataAttributeModels.IndexOf(temp.FirstOrDefault());
         

                        temp.Where(p => p.Number >= model.Number).ToList().ForEach(p => p.Number = p.Number + 1);

                        packageModelDic[key].MetadataAttributeModels.Insert(index, model);

                        //set first and last
                        List<MetadataAttributeModel> order = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id == model.Id).ToList();
                        order = UpdateFirstAndLast(order);
          

                        return packageModelDic[key];
                    }
                }

                return null;
            }

            private MetadataPackageModel RemoveAttributeFromPackageIntoBus(long id,int number, long packageId, int packageNumber)
            {
                Dictionary<string, MetadataPackageModel> packageModelDic;

                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
                    string key = CreateIdentfifier(packageId, packageNumber);

                    if (packageModelDic.ContainsKey(key))
                    {
                        MetadataAttributeModel model =  packageModelDic[key].MetadataAttributeModels.Where(a=>a.Id.Equals(id) && a.Number.Equals(number)).FirstOrDefault();
                        if (model != null)
                        {
                            packageModelDic[key].MetadataAttributeModels.Remove(model);

                            //remove from xml
                            RemoveAttributeToXml(packageModelDic[key].Source, packageNumber, model.Source, number);

                            List<MetadataAttributeModel> temp = packageModelDic[key].MetadataAttributeModels.Where(a => a.Number > number).ToList();
                            temp.ForEach(a => a.Number = (a.Number - 1));

                            //set first and last
                            List<MetadataAttributeModel> order = packageModelDic[key].MetadataAttributeModels.Where(a => a.Id == model.Id).ToList();
                            order = UpdateFirstAndLast(order);
                            
                        }
                        return packageModelDic[key];
                    }
                }

                return null;
            }

            private MetadataPackageModel ChangeOrderAttributeFromPackageIntoBus(long id, int number, long packageId, int packageNumber, bool ascent)
            {
                Dictionary<string, MetadataPackageModel> packageModelDic;

                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
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

                Dictionary<string, MetadataPackageModel> packageModelDic;

                if (TaskManager != null &&  TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
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

                Dictionary<string, MetadataPackageModel> packageModelDic;

                if (TaskManager != null &&  TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST))
                {
                    packageModelDic = (Dictionary<string, MetadataPackageModel>)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_PACKAGE_MODEL_LIST];
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


        private long GetPackageId(int stepIndex)
        {

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATAPACKAGE_IDS))
            {

                if ((Dictionary<int, long>)TaskManager.Bus[CreateDatasetTaskmanager.METADATAPACKAGE_IDS] != null)
                {
                    Dictionary<int, long> metadataPackageDic = (Dictionary<int, long>)TaskManager.Bus[CreateDatasetTaskmanager.METADATAPACKAGE_IDS];

                    if (metadataPackageDic.ContainsKey(stepIndex)) return Convert.ToInt64(metadataPackageDic[stepIndex]);
                }
            }

            return 0;
                
        }

        private MetadataPackageContainerModel Create(int index, bool validateIt)
        {
            List<MetadataPackageModel> metadataPackageModelList = new List<MetadataPackageModel>();
            long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
            long metadataPackageId = GetPackageId(index) ;

            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataPackageManager mdpManager = new MetadataPackageManager();

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID))
            {
                metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);
            }

            MetadataPackageModel modelchild = new MetadataPackageModel();


            //get PackageIdFrom Bus based on stepindex
            metadataPackageId = GetPackageId(index);

            // get stored MetadataPackages from Bus
            metadataPackageModelList = GetMetadataPackageModelsFromBus(metadataPackageId);

            //create Model
            MetadataPackageUsage mpu = mdsManager.GetEffectivePackages(metadataStructureId).Where(p => p.Id.Equals(metadataPackageId)).FirstOrDefault();
            MetadataPackageContainerModel model = MetadataPackageContainerModel.Convert(mpu);

            if (metadataPackageModelList == null || metadataPackageModelList.Count() == 0)
            {
                modelchild = MetadataPackageModel.Convert(mpu);
                modelchild.ConvertMetadataAttributeModels(mpu.MetadataPackage.MetadataAttributeUsages);
                //modelchild = CompareWithBus(modelchild);
                AddPackageIntoBus(modelchild);
                model.MetadataPackageModel.Add(modelchild);
            }
            else
            {
                model.MetadataPackageModel = metadataPackageModelList;

            }

            if (validateIt)
            {
                //validate packages
                List<Error> errors = ValidatePackageContainer(model.Source.Id);
                if (errors != null)
                    model.ErrorList = errors;

            }

            model.StepInfo = TaskManager.Current();

            return model;
        }

    #endregion

        #region xml

            private void AddPackageToXml(MetadataPackageUsage packageUsage, int number)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                metadataXml = xmlMetadataWriter.AddPackage(metadataXml,packageUsage, number);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;

                 // locat path
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
                metadataXml.Save(path);
            }

            private void RemovePackageToXml(MetadataPackageUsage packageUsage, int number)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                metadataXml = xmlMetadataWriter.RemovePackage(metadataXml, packageUsage, number);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;

                // locat path
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
                metadataXml.Save(path);
            }

            private void AddAttributeToXml(MetadataPackageUsage packageUsage, int packageNumber, MetadataAttributeUsage attribute, int number)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];

                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                metadataXml = xmlMetadataWriter.AddAttribute(metadataXml, packageUsage, packageNumber, attribute, number);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;

                // locat path
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
                metadataXml.Save(path);
            }

            private void RemoveAttributeToXml(MetadataPackageUsage packageUsage, int packageNumber, MetadataAttributeUsage attribute, int number)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

                metadataXml = xmlMetadataWriter.RemoveAttribute(metadataXml, packageUsage, packageNumber, attribute, number);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;
                // locat path
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
                metadataXml.Save(path);
            }

            private void UpdateAttribute(MetadataPackageUsage packageUsage, int packageNumber, MetadataAttributeUsage attribute, int number, object value)
            {
                TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
                XDocument metadataXml = (XDocument)TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML];
                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

                metadataXml = xmlMetadataWriter.Update(metadataXml, packageUsage, packageNumber, attribute, number, value);

                TaskManager.Bus[CreateDatasetTaskmanager.METADATA_XML] = metadataXml;
                // locat path
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "metadataTemp.Xml");
                metadataXml.Save(path);
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

        #endregion


    }
}
