using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using BExIS.DCM.Transform.Input;
using BExIS.DCM.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Web.Shell.Areas.DCM.Models;
using BExIS.DCM.UploadWizard;
using Vaiona.Util.Cfg;
using System.Diagnostics;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Collect/Home/

        List<string> ids = new List<string>();
        private TaskManager TaskManager;
        private FileStream Stream;

        public ActionResult Index()
        {
            return View();
        }

        #region Upload Wizard

        public ActionResult UploadWizard()
        {
            Session["TaskManager"] = null;

            if (TaskManager == null) TaskManager = (TaskManager)Session["TaskManager"];

            if (TaskManager == null)
            {
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "TaskInfo.xml");
                XmlDocument xmlTaskInfo = new XmlDocument();
                xmlTaskInfo.Load(path);

                Session["TaskManager"] = TaskManager.Bind(xmlTaskInfo);
                Session["Filestream"] = Stream;

                TaskManager = (TaskManager)Session["TaskManager"];

                // get Lists of Dataset and Datastructure
                Session["DatasetVersionViewList"] = LoadDatasetVersionViewList();
                Session["DataStructureViewList"] = LoadDataStructureViewList();
                Session["ResearchPlanViewList"] = LoadResearchPlanViewList();
                
            }


            return View((TaskManager)Session["TaskManager"]);
        }

        public ActionResult ReloadUploadWizard(bool restart)
        {
            if (restart) Session["TaskManager"] = null;

            if (TaskManager == null) TaskManager = (TaskManager)Session["TaskManager"];

            if (TaskManager == null)
            {
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "TaskInfo.xml");
                XmlDocument xmlTaskInfo = new XmlDocument();
                xmlTaskInfo.Load(path);

                Session["TaskManager"] = TaskManager.Bind(xmlTaskInfo);
                Session["Filestream"] = Stream;

                TaskManager = (TaskManager)Session["TaskManager"];

                // get Lists of Dataset and Datastructure
                //Session["DatasetVersionViewList"] = LoadDatasetVersionViewList();
                Session["DataStructureViewList"] = LoadDataStructureViewList();
                Session["ResearchPlanViewList"] = LoadResearchPlanViewList();
            }


            return View("UploadWizard",(TaskManager)Session["TaskManager"]);
        }


        #region UploadNavigation

        [HttpPost]
        public ActionResult RefreshNavigation()
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            return PartialView("_uploadWizardNav", TaskManager);
        }

        [HttpPost]
        public ActionResult RefreshTaskList()
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            return PartialView("_taskListView", TaskManager.GetStatusOfStepInfos());
        }

        #endregion

        #region Steps

        [HttpGet]
        public ActionResult Step1(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            //set current stepinfo based on index
            if (TaskManager != null)
                TaskManager.SetCurrent(index);

            //Get Bus infomations
            SelectFileViewModel model = new SelectFileViewModel();
            if (TaskManager.Bus.ContainsKey(TaskManager.FILENAME))
            {
                model.SelectedFileName = TaskManager.Bus[TaskManager.FILENAME].ToString();
            }

            //Get StepInfo
            model.StepInfo = TaskManager.Current();

           
            DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "ServerFiles"));
            model.serverFileList = dirInfo.GetFiles().Select(i => i.Name).ToList();

            return PartialView(model);
        }
       
        [HttpPost]
        public ActionResult Step1(object[] data)
        {

            SelectFileViewModel model = new SelectFileViewModel();

            TaskManager = (TaskManager)Session["TaskManager"];

            if (data!=null)TaskManager.AddToBus(data);

            model.StepInfo = TaskManager.Current();

            if (TaskManager != null)
            {
                // is path of file exist
                if (TaskManager.Bus.ContainsKey(TaskManager.FILEPATH))
                {
                    if (IsSupportedExtention(TaskManager))
                    {
                        try
                        {
                            //try save file
                            string filePath = TaskManager.Bus[TaskManager.FILEPATH].ToString();
                            //if extention like a makro excel file
                            if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".xlsm"))
                            {
                                // open file
                                ExcelReader reader = new ExcelReader();
                                Stream = reader.Open(filePath);
                                //Session["Stream"] = Stream;

                                //check is it template

                                if (reader.IsTemplate(Stream))
                                {
                                    TaskManager.Current().SetValid(true);
                                    TaskManager.AddToBus(TaskManager.IS_TEMPLATE, "true");
                                }
                                else
                                {
                                    model.ErrorList.Add(new Error(ErrorType.Other, "File is not a Template"));
                                    TaskManager.AddToBus(TaskManager.IS_TEMPLATE, "false");
                                }

                                Stream.Close();
                            }
                            else
                            {
                                TaskManager.AddToBus(TaskManager.IS_TEMPLATE, "false");
                                // excel file
                                if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".xls"))
                                {
                                   
                                    // open file
                                    ExcelReader reader = new ExcelReader();
                                    Stream = reader.Open(filePath);
                                    //Session["Stream"] = Stream;
                                    TaskManager.Current().SetValid(true);

                                    Stream.Close();
                                }

                                // text ór csv file
                                if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".csv") || TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".txt"))
                                {
                                    // open file
                                    AsciiReader reader = new AsciiReader();
                                    Stream = reader.Open(filePath);
                                    //Session["Stream"] = Stream;
                                    TaskManager.Current().SetValid(true);

                                    Stream.Close();
                                }
                            
                            }
                            
                        }
                        catch
                        {
                            model.ErrorList.Add(new Error(ErrorType.Other,"File can not open."));
                        }
                    }
                    else
                    { 
                        model.ErrorList.Add(new Error(ErrorType.Other,"File is not supported."));
                    }


                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Other,"File not submited or selected."));
                }

                if (TaskManager.Current().IsValid())
                {
                    TaskManager.SetPrev(TaskManager.Current());
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
            }

            return PartialView(model);
        }

        [HttpGet]
        public ActionResult Step2(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            //set current stepinfo based on index
            if (TaskManager != null)
                TaskManager.SetCurrent(index);


            //if its a template jumping direkt to the next step
            if (TaskManager.Bus[TaskManager.IS_TEMPLATE].ToString().Equals("true"))
            {
                TaskManager.Current().SetValid(true);
                if (TaskManager.Current().IsValid())
                {
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
            }
            else
            {
                GetFileInformationModel model = new GetFileInformationModel();
                model.StepInfo = TaskManager.Current();
                model.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();

                if (TaskManager.Bus.ContainsKey(TaskManager.FILE_READER_INFO))
                {
                    model.FileInfoModel = GetFileInfoModel((AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], TaskManager.Bus[TaskManager.EXTENTION].ToString());
                }

                    
                model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();

                if (model.Extention.Equals(".txt") || model.Extention.Equals(".csv"))
                    TaskManager.Bus[TaskManager.FILE_READER_INFO] = new AsciiFileReaderInfo();

                if (model.Extention.Equals(".xls"))
                    TaskManager.Bus[TaskManager.FILE_READER_INFO] = new ExcelFileReaderInfo();


                return PartialView(model);
            }

            return PartialView(new GetFileInformationModel());
        }

        [HttpPost]
        public ActionResult Step2()
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            if (TaskManager.Bus[TaskManager.IS_TEMPLATE].ToString().Equals("true"))
                TaskManager.Current().SetValid(true);

            if (TaskManager.Bus.ContainsKey(TaskManager.FILE_READER_INFO))
            {
                TaskManager.Current().SetValid(true);
            }
            else
            {
                GetFileInformationModel model = new GetFileInformationModel();
                model.StepInfo = TaskManager.Current();
                model.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
                model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
                model.ErrorList.Add(new Error(ErrorType.Other, "File Infomartion not saved."));

                if (TaskManager.Bus.ContainsKey(TaskManager.FILE_READER_INFO))
                {
                    model.FileInfoModel = GetFileInfoModel((AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], TaskManager.Bus[TaskManager.EXTENTION].ToString());
                }

                model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();

                return PartialView(model);
            }

            if (TaskManager.Current().IsValid())
            {
                TaskManager.SetPrev(TaskManager.Current());
                TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
            }

            return PartialView();
        }

        [HttpGet]
        public ActionResult Step3(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
                TaskManager.SetCurrent(index);

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


            model.DatasetViewModel = new CreateDatasetViewModel();
            if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatasetViewModel.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
            if ((List<ListViewItem>)Session["DatasetVersionViewList"] != null) model.DatasetsViewList = (List<ListViewItem>)Session["DatasetVersionViewList"];
            if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.DatasetViewModel.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];

            return PartialView(model);

        }

        [HttpPost]
        public ActionResult Step3(object[] data)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            ChooseDatasetViewModel model = new ChooseDatasetViewModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager != null)
            {
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
                        model.ErrorList.Add(new Error(ErrorType.Dataset, "Dataset not exist."));
                    }
                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Dataset, "Dataset not exist."));
                }

                if (TaskManager.Current().valid == true)
                {
                    TaskManager.SetPrev(TaskManager.Current());
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
                    model.DatasetViewModel = new CreateDatasetViewModel();
                    //load datastructure ids
                    if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatasetViewModel.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
                    if ((List<ListViewItem>)Session["DatasetVersionViewList"] != null) model.DatasetsViewList = (List<ListViewItem>)Session["DatasetVersionViewList"];
                }
            }

            return PartialView(model);
        }

        [HttpGet]
        public ActionResult Step4(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
                TaskManager.SetCurrent(index);

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_STATUS))
            {
                if (TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("new"))
                {
                    TaskManager.Current().SetValid(true);
                    if (TaskManager.Current().IsValid())
                    {
                        TaskManager.GoToNext();
                        Session["TaskManager"] = TaskManager;
                        ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                        return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                    }
                }
            }

            PrimaryKeyViewModel model = new PrimaryKeyViewModel();
            model.StepInfo = TaskManager.Current();

            model.VariableLableList = LoadVariableLableList();
            Session["VariableLableList"] = model.VariableLableList;

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Step4(object[] data)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            PrimaryKeyViewModel model = new PrimaryKeyViewModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(TaskManager.PRIMARY_KEYS))
            {
                TaskManager.Current().SetValid(true);
            }

            if (TaskManager.Current().IsValid())
            {
                TaskManager.SetPrev(TaskManager.Current());
                TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
            }

            return PartialView(model);
        }

        [HttpGet]
        public ActionResult Step5(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
                TaskManager.SetCurrent(index);

            ValidationModel model = new ValidationModel();
            model.StepInfo = TaskManager.Current();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Step5(object[] data)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            ValidationModel model = new ValidationModel();

            if (TaskManager != null)
            {
                if (data != null) TaskManager.AddToBus(data);

                if (TaskManager.Bus.ContainsKey(TaskManager.VALID))
                {
                    bool valid = Convert.ToBoolean(TaskManager.Bus[TaskManager.VALID]);

                    if (valid)
                    {
                        TaskManager.Current().SetValid(true);
                    }
                    else
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other, "Not Valid."));
                    }

                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "Validation key not exist."));
                    model.StepInfo = TaskManager.Current();
                }

                if (TaskManager.Current().valid == true)
                {
                    TaskManager.SetPrev(TaskManager.Current());
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
            }

            return PartialView(model);
        }

        [HttpGet]
        public ActionResult Step6(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
                TaskManager.SetCurrent(index);

            SummaryModel model = new SummaryModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID))
            {
                model.DatasetId = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]);
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_TITLE))
            {
                model.DatasetTitle = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_ID))
            {
                model.DataStructureId = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_TITLE))
            {
                model.DataStructureTitle = TaskManager.Bus[TaskManager.DATASTRUCTURE_TITLE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.RESEARCHPLAN_ID))
            {
                model.ResearchPlanId = Convert.ToInt32(TaskManager.Bus[TaskManager.RESEARCHPLAN_ID].ToString());
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.RESEARCHPLAN_TITLE))
            {
                model.ResearchPlanTitle = TaskManager.Bus[TaskManager.RESEARCHPLAN_TITLE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.TITLE))
            {
                model.DatasetTitle = TaskManager.Bus[TaskManager.TITLE].ToString();
            }


            if (TaskManager.Bus.ContainsKey(TaskManager.AUTHOR))
            {
                model.Author = TaskManager.Bus[TaskManager.AUTHOR].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.OWNER))
            {
                model.Owner = TaskManager.Bus[TaskManager.OWNER].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.PROJECTNAME))
            {
                model.ProjectName = TaskManager.Bus[TaskManager.PROJECTNAME].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.INSTITUTE))
            {
                model.ProjectInstitute = TaskManager.Bus[TaskManager.INSTITUTE].ToString();
            }

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Step6(object[] data)
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            if (TaskManager != null)
            {
                if (TaskManager.Current().valid == false)
                {
                    TaskManager.SetPrev(TaskManager.Current());
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
            }

            return PartialView();
        }

        
        #endregion
        
        #region Navigation options

        public ActionResult FinishUpload()
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];
            ValidationModel model = new ValidationModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID) && TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_ID))
            {
                try
                {
                    long id = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]);
                    DataStructureManager dsm = new DataStructureManager();
                    long iddsd = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);
                    StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(iddsd);
                    dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);

                    List<DataTuple> rows;
                    if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".xlsm"))
                    {
                        // open file
                        ExcelReader reader = new ExcelReader();
                        Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                        rows = reader.ReadFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), sds, (int)id);

                        if (reader.errorMessages.Count > 0)
                        {
                            //model.ErrorList = reader.errorMessages;
                        }
                        else
                        {

                            DatasetManager dm = new DatasetManager();
                            Dataset ds = dm.GetDataset(id);

                            //XXX Add packagesize to excel read function
                            if (dm.IsDatasetCheckedOutFor(ds.Id, "David") || dm.CheckOutDataset(ds.Id, "David"))
                            {
                                if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_STATUS))
                                {
                                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(ds.Id);

                                    if (TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("new"))
                                    {
                                        dm.EditDatasetVersion(workingCopy, rows, null, null);
                                        dm.CheckInDataset(ds.Id, "upload data from upload wizard", "David");
                                    }
                                    else
                                    {
                                        if (rows.Count > 0)
                                        {
                                            Dictionary<string, List<DataTuple>> splittedDatatuples = new Dictionary<string, List<DataTuple>>();
                                            splittedDatatuples = UploadWizardHelper.GetSplitDatatuples(rows, (List<long>)TaskManager.Bus[TaskManager.PRIMARY_KEYS], workingCopy);
                                            dm.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                                        }
                                    }
                                }
                            }
                        }

                        Stream.Close();
                    }

                    if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".csv") ||
                        TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".txt"))
                    {
                        // open file
                        AsciiReader reader = new AsciiReader();
                        //Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());

                        DatasetManager dm = new DatasetManager();
                        Dataset ds = dm.GetDataset(id);

                        Stopwatch totalTime = Stopwatch.StartNew();

                        if (dm.IsDatasetCheckedOutFor(ds.Id, "David") || dm.CheckOutDataset(ds.Id, "David"))
                        {
                            DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(ds.Id);
                            int packageSize = 100;
                            //schleife
                            do
                            {
                                Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                                rows = reader.ReadFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), (AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], sds, id, packageSize);
                                Stream.Close();

                                if (reader.errorMessages.Count > 0)
                                {
                                    //model.ErrorList = reader.errorMessages;
                                }
                                else
                                {
                                    //model.Validated = true;
                                    Stopwatch dbTimer = Stopwatch.StartNew();

                                    if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_STATUS))
                                    {
                                        if (TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("new"))
                                        {

                                            dm.EditDatasetVersion(workingCopy, rows, null, null);
                                        }
                                    }
                                    else
                                    {
                                        if (rows.Count > 0)
                                        {
                                            Dictionary<string, List<DataTuple>> splittedDatatuples = new Dictionary<string, List<DataTuple>>();
                                            splittedDatatuples = UploadWizardHelper.GetSplitDatatuples(rows, (List<long>)TaskManager.Bus[TaskManager.PRIMARY_KEYS], workingCopy);
                                            dm.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                                        }
                                    }

                                    dbTimer.Stop();
                                    Debug.WriteLine(" db time" + dbTimer.Elapsed.TotalSeconds.ToString());

                                }
                            } while (rows.Count > 0);

                            dm.CheckInDataset(ds.Id, "upload data from upload wizard", "David");

                            totalTime.Stop();
                            Debug.WriteLine(" Total Time "+totalTime.Elapsed.TotalSeconds.ToString());

                           
                        }

                        //Stream.Close();
                    
                    }
                }
                catch
                {

                    model.ErrorList.Add(new Error(ErrorType.Other, "Can not valid."));
                }
            }
            else
            {
                model.ErrorList.Add(new Error(ErrorType.Dataset, "Dataset is not selected."));
            }

            if (model.ErrorList.Count > 0)
            {
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                
            }
            else return View();
        }

        public ActionResult CloseUpload()
        {
            Session["TaskManager"] = null;
            TaskManager = null;

            return RedirectToAction("UploadWizard");
        }

        #endregion

        #region Step Logic

        /// <summary>
        /// Selected File store din the BUS
        /// </summary>
        /// <param name="SelectFileUploader"></param>
        /// <returns></returns>
        public ActionResult SelectFileProcess(HttpPostedFileBase SelectFileUploader)
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];
            if (SelectFileUploader != null)
            {
                // store file in archive
                //string filename = "D:\\" + DateTime.Now.Millisecond.ToString() + SelectFileUploader.FileName;
                //SelectFileUploader.SaveAs(filename);

                string path = Path.Combine(AppConfiguration.WorkspaceRootPath,"temp","DCM", DateTime.Now.Millisecond.ToString() + SelectFileUploader.FileName);
                SelectFileUploader.SaveAs(path);
                
                TaskManager.AddToBus(TaskManager.FILEPATH, path);
                
                TaskManager.AddToBus(TaskManager.FILENAME, SelectFileUploader.FileName);
                TaskManager.AddToBus(TaskManager.EXTENTION, SelectFileUploader.FileName.Split('.').Last());
                Session["TaskManager"] = TaskManager;
            }

            //return RedirectToAction("UploadWizard");
            return Content("");
        }

        /// <summary>
        /// Selected File from server and store into BUS
        /// </summary>
        /// <param name="SelectFileUploader"></param>
        /// <returns></returns>
        public ActionResult SelectFileFromServerProcess(string fileName)
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];
            if (fileName != null)
            {
                string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "ServerFiles",fileName);

                TaskManager.AddToBus(TaskManager.FILEPATH, path);

                TaskManager.AddToBus(TaskManager.FILENAME, fileName);
                TaskManager.AddToBus(TaskManager.EXTENTION, "."+fileName.Split('.').Last());
                Session["TaskManager"] = TaskManager;
            }

            //return RedirectToAction("UploadWizard");
            return Content("");
        }

        /// <summary>
        /// Load PartialView to create a Dataset GET
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CreateDataset()
        {
            CreateDatasetViewModel model = new CreateDatasetViewModel();
            if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
            if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];

            return PartialView("_createDataset", model);
        }

        /// <summary>
        /// POST of Create Dataset to create a dataset
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateDataset(CreateDatasetViewModel model)
        {
            if (model == null)
            {
                model = new CreateDatasetViewModel();
                if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
                if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];

                return PartialView("_createDataset", model);
            }

            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            if (ModelState.IsValid)
            {
                XmlDocument metadata = new XmlDocument();
                metadata.Load(Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "emptymetadata.xml"));

                metadata.GetElementsByTagName("bgc:title")[0].InnerText = model.Title;
                metadata.GetElementsByTagName("bgc:owner")[0].InnerText = model.Owner;
                metadata.GetElementsByTagName("bgc:author")[0].InnerText = model.DatasetAuthor;
                metadata.GetElementsByTagName("bgc:projectName")[0].InnerText = model.ProjectName;
                metadata.GetElementsByTagName("bgc:institute")[0].InnerText = model.ProjectInstitute;

                //Add Metadata to Bus
                TaskManager.AddToBus(TaskManager.DATASET_TITLE, model.Title);
                TaskManager.AddToBus(TaskManager.OWNER, model.Owner);
                TaskManager.AddToBus(TaskManager.AUTHOR, model.DatasetAuthor);
                TaskManager.AddToBus(TaskManager.PROJECTNAME, model.ProjectName);
                TaskManager.AddToBus(TaskManager.INSTITUTE, model.ProjectInstitute);

                DatasetManager dm = new DatasetManager();
                DataStructureManager dsm = new DataStructureManager();
                DataStructure dataStructure = dsm.StructuredDataStructureRepo.Get(model.DataStructureId);

                ResearchPlanManager rpm = new ResearchPlanManager();
                ResearchPlan rp = rpm.Repo.Get(model.ResearchPlanId);

                Dataset ds = dm.CreateEmptyDataset(dataStructure, rp);

                TaskManager.AddToBus(TaskManager.DATASET_TITLE, model.Title);
                TaskManager.AddToBus(TaskManager.RESEARCHPLAN_ID, model.ResearchPlanId);

                if (dm.IsDatasetCheckedOutFor(ds.Id, "David") || dm.CheckOutDataset(ds.Id, "David"))
                {
                    metadata.GetElementsByTagName("bgc:id")[0].InnerText = ds.Id.ToString();

                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(ds.Id);
                    workingCopy.Metadata = metadata;
                    TaskManager.AddToBus(TaskManager.DATASET_ID,ds.Id);
                    TaskManager.AddToBus(TaskManager.RESEARCHPLAN_TITLE, rp.Title);
                    dm.EditDatasetVersion(workingCopy, null, null, null);
                }

                //DatasetStatus if new or selected
                TaskManager.AddToBus("DatasetStatus", "new");

                Session["createDatasetWindowVisible"] = false;
                Session["TaskManager"] = TaskManager;

                return Json(new { success = true, title = model.Title });
            }
            else
            {
                Session["createDatasetWindowVisible"] = true;

                // put lists to model
            }

            if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
            if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];

                
            return PartialView("_createDataset", model);
        }

        /// <summary>
        /// Add Selected Dataset to Bus
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddSelectedDatasetToBus(string id)
        {

            long datasetId = Convert.ToInt64(id);

            DatasetManager datasetManager = new DatasetManager();
            DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            TaskManager.AddToBus(TaskManager.DATASET_ID, datasetId);
            //Add Metadata to Bus
            TaskManager.AddToBus(TaskManager.DATASET_TITLE, datasetVersion.Metadata.GetElementsByTagName("bgc:title")[0].InnerText);
            TaskManager.AddToBus(TaskManager.OWNER, datasetVersion.Metadata.GetElementsByTagName("bgc:owner")[0].InnerText);
            TaskManager.AddToBus(TaskManager.AUTHOR, datasetVersion.Metadata.GetElementsByTagName("bgc:author")[0].InnerText);
            TaskManager.AddToBus(TaskManager.PROJECTNAME, datasetVersion.Metadata.GetElementsByTagName("bgc:projectName")[0].InnerText);
            TaskManager.AddToBus(TaskManager.INSTITUTE, datasetVersion.Metadata.GetElementsByTagName("bgc:institute")[0].InnerText);

            ResearchPlanManager rpm = new ResearchPlanManager();
            ResearchPlan rp = rpm.Repo.Get(datasetVersion.Dataset.ResearchPlan.Id);
            TaskManager.AddToBus(TaskManager.RESEARCHPLAN_ID, rp.Id);
            TaskManager.AddToBus(TaskManager.RESEARCHPLAN_TITLE, rp.Title);


            Session["TaskManager"] = TaskManager;

            return Content("");
        }

        /// <summary>
        /// returns true if Extention in the Bus will supported
        /// </summary>
        /// <param name="taskManager"></param>
        /// <returns></returns>
        private bool IsSupportedExtention(TaskManager taskManager)
        { 
            if(taskManager.Bus.ContainsKey(TaskManager.EXTENTION))
            {
                string ext = taskManager.Bus[TaskManager.EXTENTION].ToString();

                if (ext.Equals(".xls") || ext.Equals(".xlsm") || ext.Equals(".xlsx") || ext.Equals(".csv") || ext.Equals(".txt")) return true;
            }

            return false;
        }

        public ActionResult CheckPrimaryKeys(object[] data)
        { 
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            PrimaryKeyViewModel model = new PrimaryKeyViewModel();
            model.StepInfo = TaskManager.Current();

            model.VariableLableList = (List<ListViewItem>)Session["VariableLableList"];
            
            // check Identifier
            List<string> identifiersLables =  data.ToList().ConvertAll<string>(delegate(object i) { return i.ToString(); });
            List<long> identifiers = new List<long>();
            List<ListViewItem> pks = new List<ListViewItem>();

            foreach(string value in identifiersLables)
            {
                identifiers.Add(
                        model.VariableLableList.Where(p=>p.Title.Equals(value)).First().Id
                    );

                pks.Add(model.VariableLableList.Where(p => p.Title.Equals(value)).First());
            }


            List<string> tempDataset = UploadWizardHelper.GetIdentifierList(Convert.ToInt64(TaskManager.Bus[TaskManager.DATASET_ID].ToString()), identifiers);

            List<string> tempFromFile = UploadWizardHelper.GetIdentifierList(TaskManager, Convert.ToInt64(TaskManager.Bus[TaskManager.DATASET_ID].ToString()), identifiers, TaskManager.Bus[TaskManager.EXTENTION].ToString(), TaskManager.Bus[TaskManager.FILENAME].ToString());

            if (UploadWizardHelper.CheckDuplicates(tempDataset) || UploadWizardHelper.CheckDuplicates(tempFromFile))
            {
                model.IsUnique = false;
                model.PrimaryKeysList = pks;
                model.ErrorList.Add(new Error(ErrorType.Other, "Selection is not unique"));

            }
            else
            {
                model.IsUnique = true;
                TaskManager.Bus[TaskManager.PRIMARY_KEYS] = identifiers;
                Session["TaskManager"] = TaskManager;
                model.PrimaryKeysList = pks;
            }

            return PartialView(TaskManager.Current().GetActionInfo.ActionName, model);
        }

        [HttpPost]
        public ActionResult ValidateFile()
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];
            ValidationModel model = new ValidationModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID) && TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_ID))
            {
                try
                {
                    long id = (long)Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]);
                    DataStructureManager dsm = new DataStructureManager();
                    long iddsd = (long)Convert.ToInt32(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);
                    StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(iddsd);
                    dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);

                    if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".xlsm"))
                    {
                        // open file
                        ExcelReader reader = new ExcelReader();
                        Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                        reader.ValidateFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), sds, id);
                        Stream.Close();
                        model.ErrorList = reader.errorMessages;
                    }

                    if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".csv") ||
                        TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".txt"))
                    {
                        AsciiReader reader = new AsciiReader();
                        Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                        reader.ValidateFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), (AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], sds, id);
                        Stream.Close();
                        model.ErrorList = reader.errorMessages;
                    }
                }
                catch
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "Can not valid."));
                    TaskManager.AddToBus(TaskManager.VALID, false);
                }
            }
            else
            {
                model.ErrorList.Add(new Error(ErrorType.Dataset, "Dataset is not selected."));
                TaskManager.AddToBus(TaskManager.VALID, false);
            }

            if (model.ErrorList.Count() == 0)
            {
                model.Validated = true;
                TaskManager.AddToBus(TaskManager.VALID, true);
            }

            return PartialView(TaskManager.Current().GetActionInfo.ActionName, model);
        }

        #region ascii file info
        [HttpPost]
        public ActionResult SaveAsciiFileInfos(FileInfoModel  info)
        {

                TaskManager TaskManager = (TaskManager)Session["TaskManager"];

                AsciiFileReaderInfo asciiFileReaderInfo = new AsciiFileReaderInfo();

                asciiFileReaderInfo.Data = info.Data;
                asciiFileReaderInfo.Dateformat = "";//info.Dateformat;
                asciiFileReaderInfo.Decimal = info.Decimal;
                asciiFileReaderInfo.Offset = info.Offset;
                asciiFileReaderInfo.Orientation = info.Orientation;
                asciiFileReaderInfo.Seperator = info.Seperator;
                asciiFileReaderInfo.Variables = info.Variables;

                TaskManager.AddToBus(TaskManager.FILE_READER_INFO, asciiFileReaderInfo);

                GetFileInformationModel model = new GetFileInformationModel();
                model.StepInfo = TaskManager.Current();
                model.StepInfo.SetValid(true);
                model.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
                model.FileInfoModel = info;
                model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
                model.IsSaved = true;

                return RedirectToAction("ReloadUploadWizard", new { restart = false });
        }

        public ActionResult ChangeAsciiFileInfo(string name, string value)
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".txt") ||
                TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".csv"))
            {

                AsciiFileReaderInfo info = (AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO];

                switch (name)
                {
                    case "Seperator": { info.Seperator = AsciiFileReaderInfo.GetSeperator(value); break; }
                    case "Decimal": { info.Decimal = AsciiFileReaderInfo.GetDecimalCharacter(value); break; }
                    case "Orientation": { info.Orientation = AsciiFileReaderInfo.GetOrientation(value); break; }
                    case "Variables": { info.Variables = Convert.ToInt32(value); break; }
                    case "Data": { info.Data = Convert.ToInt32(value); break; }
                    case "Offset": { info.Offset = Convert.ToInt32(value); break; }
                }

                TaskManager.Bus[TaskManager.FILE_READER_INFO] = info;
                Session["TaskManager"] = TaskManager;
            }


            return Content("");
        }

        #endregion

        #region excel file info

            public ActionResult SaveExcelFileInfos(FileInfoModel info)
            {
                TaskManager TaskManager = (TaskManager)Session["TaskManager"];

                ExcelFileReaderInfo excelFileReaderInfo = new ExcelFileReaderInfo();

                excelFileReaderInfo.Data = info.Data;
                excelFileReaderInfo.Dateformat = "";//info.Dateformat;
                excelFileReaderInfo.Decimal = info.Decimal;
                excelFileReaderInfo.Offset = info.Offset;
                excelFileReaderInfo.Orientation = info.Orientation;
                excelFileReaderInfo.Variables = info.Variables;

                TaskManager.AddToBus(TaskManager.FILE_READER_INFO, excelFileReaderInfo);

                GetFileInformationModel model = new GetFileInformationModel();
                model.StepInfo = TaskManager.Current();
                model.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
                model.FileInfoModel = info;
                model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
                model.IsSaved = true;

                return RedirectToAction("ReloadUploadWizard", new { restart = false });

            }

        #endregion

        #endregion

        #region Helper functions

            public List<ListViewItem> LoadDatasetVersionViewList()
            {
                DatasetManager dm = new DatasetManager();
                Dictionary<long, XmlDocument> dmtemp = new Dictionary<long, XmlDocument>();
                dmtemp = dm.GetDatasetLatestMetadataVersions();
                List<ListViewItem> temp = new List<ListViewItem>();

                foreach (long datasetid in dmtemp.Keys)
                {
                    if (dmtemp[datasetid] != null)
                    {
                        XmlNodeList xnl = dmtemp[datasetid].GetElementsByTagName("bgc:title");
                        string title = "";

                        if (xnl.Count > 0)
                        {
                            title = xnl[0].InnerText;
                        }

                        temp.Add(new ListViewItem(datasetid, title));
                    }
                }

                return temp;
            }

            public List<ListViewItem> LoadDataStructureViewList()
            {
                DataStructureManager dsm = new DataStructureManager();
                List<ListViewItem> temp = new List<ListViewItem>();

                foreach (DataStructure datasStructure in dsm.StructuredDataStructureRepo.Get())
                {
                    string title = datasStructure.Name;

                    temp.Add(new ListViewItem(datasStructure.Id, title));
                }

                return temp;
            }

            public List<ListViewItem> LoadResearchPlanViewList()
            {
                ResearchPlanManager rpm = new ResearchPlanManager();
                List<ListViewItem> temp = new List<ListViewItem>();

                foreach (ResearchPlan researchPlan in rpm.Repo.Get())
                {
                    string title = researchPlan.Title;

                    temp.Add(new ListViewItem(researchPlan.Id, title));
                }

                return temp;
            }

            private FileInfoModel GetFileInfoModel(AsciiFileReaderInfo info, string extention)
            {
                FileInfoModel FileReaderInfo = new FileInfoModel();

                FileReaderInfo.Data = info.Data;
                FileReaderInfo.Dateformat = info.Dateformat;
                FileReaderInfo.Decimal = info.Decimal;
                FileReaderInfo.Offset = info.Offset;
                FileReaderInfo.Orientation = info.Orientation;
                FileReaderInfo.Variables = info.Variables;
                FileReaderInfo.Seperator = info.Seperator;

                return FileReaderInfo;
            }

            private List<ListViewItem> LoadVariableLableList()
            { 
                DataStructureManager datastructureManager = new DataStructureManager();
                StructuredDataStructure structuredDatastructure = datastructureManager.StructuredDataStructureRepo.Get(Convert.ToInt64(TaskManager.Bus["DataStructureId"]));

                return (from var in structuredDatastructure.Variables
                            select new ListViewItem{
                                    Id = var.Id,
                                    Title = var.Label
                            } ).ToList();
            }

        #endregion

        #endregion

        public ActionResult Help()
        {
            return View();
        }

    }

    public class UpdateNameModel
    {
        public string Name { get; set; }
        public IEnumerable<int> Numbers { get; set; }
    } 

}
