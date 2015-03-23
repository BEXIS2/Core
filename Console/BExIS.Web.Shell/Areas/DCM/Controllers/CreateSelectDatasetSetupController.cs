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
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Web.Shell.Areas.DCM.Models;
using BExIS.Web.Shell.Areas.DCM.Models.Create;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;

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

            Session["CreateDatasetTaskmanager"] = TaskManager;


            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SelectDatasetSetup(int? index, string name = null)
        {

            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            SelectDatasetSetupModel model = GetDefaultModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.DATASTRUCTURE_ID) && 
                TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.METADATASTRUCTURE_ID) && 
                TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.RESEARCHPLAN_ID)
                )
            {
                bool ready = false;
                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.SETUP_LOADED))
                {
                    if ((bool)TaskManager.Bus[CreateDatasetTaskmanager.SETUP_LOADED] == true)
                        ready = true;
                }
                else
                {
                    CreateXml();
                    AdvanceTaskManager((long)TaskManager.Bus[CreateDatasetTaskmanager.METADATASTRUCTURE_ID]);

                    ready = true;
                }

                if (ready)
                {
                    TaskManager.Current().SetValid(true);
                }
                else
                {
                    TaskManager.Current().SetValid(false);
                    model.ErrorList.Add(new Error(ErrorType.Dataset, "Select the Button to load the setup."));
                }
               

            }
            else
            {
                 TaskManager.Current().SetValid(false);
                 model = GetDefaultModel();
                 model.StepInfo = TaskManager.Current();

                 model.ErrorList.Add(new Error(ErrorType.Dataset, "Select a research plan, data structure and a metadata structure please and store the selections."));

            }

            

            if (TaskManager.Current().IsValid())
            {
                if (index.HasValue)
                    TaskManager.SetCurrent(index.Value);
                else
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
                TaskManager.AddToBus(CreateDatasetTaskmanager.DATASTRUCTURE_TYPE, GetDataStructureType(model.SelectedDatastructureId));
                // creat a new dataset
                //CreateANewDataset(model.SelectedDatastructureId, model.SelectedResearchPlanId, model.SelectedMetadatStructureId);

                CreateXml();

                AdvanceTaskManager(model.SelectedMetadatStructureId);


                model.IsLoaded = (bool)TaskManager.Bus[CreateDatasetTaskmanager.SETUP_LOADED];

                TaskManager.Current().SetStatus(StepStatus.success);
                TaskManager.GoToNext();
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
         
            }
            else
            {
                TaskManager.Current().SetStatus(StepStatus.error);

                model.ErrorList.Add(new Error(ErrorType.Dataset, "Can not create dataset."));

                //setup loaded
                if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.SETUP_LOADED))
                    TaskManager.Bus[CreateDatasetTaskmanager.SETUP_LOADED] = true;
                else
                    TaskManager.Bus.Add(CreateDatasetTaskmanager.SETUP_LOADED, true);

                model.IsLoaded = (bool)TaskManager.Bus[CreateDatasetTaskmanager.SETUP_LOADED];
            }

            return PartialView("SelectDatasetSetup", model);
        }

        [HttpPost]
        public ActionResult StoreSelectedOption(long id, string type)
        {
            TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];
            string key = "";

            switch (type)
            {
                case "rp": key = CreateDatasetTaskmanager.RESEARCHPLAN_ID; break;
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
           TaskManager = (CreateDatasetTaskmanager)Session["CreateDatasetTaskmanager"];

           MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

           List<MetadataPackageUsage> metadataPackageList = metadataStructureManager.GetEffectivePackages(MetadataStructureId).ToList();

           Dictionary<int, long> MetadataPackageDic = new Dictionary<int, long>();
           TaskManager.AddToBus(CreateDatasetTaskmanager.METADATAPACKAGE_IDS, MetadataPackageDic);

           StepInfo summary = TaskManager.StepInfos.Last();
           //TaskManager.StepInfos.Remove(TaskManager.StepInfos.Last());
           StepInfo start = TaskManager.Root.Children.First();
           TaskManager.StepInfos.Clear();
           TaskManager.Root.Children.Clear();
           TaskManager.StepInfos.Add(start);
           TaskManager.Root.Children.Add(start);

           foreach (MetadataPackageUsage mpu in metadataPackageList)
           {
               // only add none optional usages
               if (mpu.MinCardinality > 0)
               { 
                   StepInfo si = new StepInfo(mpu.Label)
                   {
                       Id = TaskManager.GenerateStepId(),
                       Parent = TaskManager.Root,
                       IsInstanze = false,
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
                   MetadataPackageDic.Add(si.Id, mpu.Id);
                   si = AddStepsBasedOnUsage(mpu,si);
                   TaskManager.Root.Children.Add(si);

                   TaskManager.Bus[CreateDatasetTaskmanager.METADATAPACKAGE_IDS] = MetadataPackageDic;
               }
           }

           TaskManager.StepInfos.Add(summary);
           TaskManager.Root.Children.Add(summary);
           TaskManager.Bus[CreateDatasetTaskmanager.METADATAPACKAGE_IDS] = MetadataPackageDic;
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
                    TaskManager.Bus.Add(CreateDatasetTaskmanager.SETUP_LOADED,true);


                //save
                //metadataXml.Save(path);
            }

        }
            
        // check if user exist
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

            if (TaskManager.Bus.ContainsKey(CreateDatasetTaskmanager.SETUP_LOADED))
                model.IsLoaded = (bool)TaskManager.Bus[CreateDatasetTaskmanager.SETUP_LOADED];

            return model;
        }

        private SelectDatasetSetupModel LoadLists(SelectDatasetSetupModel model)
        {

            if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
            if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];
            if ((List<ListViewItem>)Session["MetadataStructureViewList"] != null) model.MetadataStructureViewList = (List<ListViewItem>)Session["MetadataStructureViewList"];

            return model;
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


        #region load advanced steps

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

        private StepInfo AddStepsBasedOnUsage(BaseUsage usage, StepInfo current)
        {
            // genertae action, controller base on usage
            string actionName = "";
            int min = usage.MinCardinality;

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

                Dictionary<string,string> keyValueDic= new Dictionary<string,string>();
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
                            string title = usage.Label + " (" + counter + ")";
                            long id = Convert.ToInt64((element.Attribute("roleId")).Value.ToString());

                            StepInfo s = new StepInfo(title)
                            {
                                Id = TaskManager.GenerateStepId(),
                                Parent = current,
                                IsInstanze = true,
                                HasContent = UsageHelper.HasUsagesWithSimpleType(usage),
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
                            current.Children.Add(s);

                            if (TaskManager.Root.Children.Where(z => z.title.Equals(title)).Count() == 0)
                            {
                                MetadataPackageDic.Add(s.Id, id);
                            }

                        }
                    }
                }

                TaskManager.AddToBus(CreateDatasetTaskmanager.METADATAPACKAGE_IDS, MetadataPackageDic);
            }
            return current;
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

                        //only not optional
                        s = AddStepsBasedOnUsage(u, s);
                        childrenSteps.Add(s);

                        if (TaskManager.Root.Children.Where(z => z.title.Equals(s.title)).Count() == 0)
                        {
                            MetadataPackageDic.Add(s.Id, u.Id);
                        }
                    }
                //}

            }

            return childrenSteps;
        }

        #endregion

        #endregion
    }
}
