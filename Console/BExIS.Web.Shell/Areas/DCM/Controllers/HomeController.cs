using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.DCM.Transform.Input;
using BExIS.DCM.Transform.Validation.Exceptions;
using BExIS.Web.Shell.Areas.DCM.Models;
using Vaiona.Util.Cfg;
using System.Web.Routing;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Services.Administration;

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


            return View((TaskManager)Session["TaskManager"]);
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
            if (TaskManager.Bus.ContainsKey("FileName"))
            {
                model.SelectedFileName = TaskManager.Bus["FileName"].ToString();
            }

            if (TaskManager.Bus.ContainsKey("FileBase"))
            {
                model.file = (HttpPostedFileBase)TaskManager.Bus["FileBase"];
            }


            //Get StepInfo
            model.StepInfo = TaskManager.Current();


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
                if (TaskManager.Bus.ContainsKey("FileBase"))
                {
                    if (IsSupportedExtention(TaskManager))
                    {
                        try
                        {
                            //try save file
                            HttpPostedFileBase SelectedFile = (HttpPostedFileBase)TaskManager.Bus["FileBase"];
                            string path = "C:\\Temp\\" + DateTime.Now.Millisecond.ToString() + SelectedFile.FileName;
                            SelectedFile.SaveAs(path);

                            // open file
                            ExcelReader reader = new ExcelReader();
                            Stream = reader.Open(path);
                            Session["Stream"] = Stream;

                            //check is it template

                            if (reader.IsTemplate(Stream))
                            {
                            TaskManager.Current().SetValid(true);
                        }
                            else
                            {
                                model.ErrorList.Add(new Error(ErrorType.Other, "File is not a Template"));
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

            ChooseDatasetViewModel model = new ChooseDatasetViewModel();

            // load datasetids
            DatasetManager dm = new DatasetManager();
            IList<DatasetVersion> dv = dm.DatasetVersionRepo.Get();
            
            model.Datasets = (from datasets in dm.DatasetRepo.Get() select datasets.Id).ToList();
            model.StepInfo = TaskManager.Current();


            model.DatasetViewModel = new CreateDatasetViewModel();
            if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatasetViewModel.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
            // if ((List<ListViewItem>)Session["DatasetVersionViewList"] != null) model.DatasetsViewList = (List<ListViewItem>)Session["DatasetVersionViewList"];
            if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.DatasetViewModel.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];

            return PartialView(model);

        }

        [HttpPost]
        public ActionResult Step2(object[] data)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            ChooseDatasetViewModel model = new ChooseDatasetViewModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager != null)
            {
                if (data != null) TaskManager.AddToBus(data);

                if (TaskManager.Bus.ContainsKey("DatasetId"))
                {
                    DatasetManager dm = new DatasetManager();
                    Dataset ds = new Dataset();
                    try
                    {
                        dm = new DatasetManager();
                        ds = dm.GetDataset((long)Convert.ToInt32(TaskManager.Bus["DatasetId"]));

                        TaskManager.AddToBus("DataStructureId", ((StructuredDataStructure)(ds.DataStructure.Self)).Id);
                        TaskManager.AddToBus("DataStructureTitle", ((StructuredDataStructure)(ds.DataStructure.Self)).Name);

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
        public ActionResult Step3(int index)
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
        public ActionResult Step3(object[] data)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            ValidationModel model = new ValidationModel();

            if (TaskManager != null)
            {
                if (data != null) TaskManager.AddToBus(data);

                if (TaskManager.Bus.ContainsKey("Valid"))
                {
                    bool valid = Convert.ToBoolean(TaskManager.Bus["Valid"]);

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
                }

                if (TaskManager.Current().valid == true)
                {
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
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

            SummaryModel model = new SummaryModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey("DatasetId"))
            {
                model.DatasetId = Convert.ToInt32(TaskManager.Bus["DatasetId"]);
        }

            if (TaskManager.Bus.ContainsKey("DatasetTitle"))
        {
                model.DatasetTitle = TaskManager.Bus["DatasetTitle"].ToString();
            }

            if (TaskManager.Bus.ContainsKey("DataStructureId"))
            {
                model.DataStructureId = Convert.ToInt32(TaskManager.Bus["DataStructureId"]);
            }

            if (TaskManager.Bus.ContainsKey("DataStructureTitle"))
                {
                model.DataStructureTitle = TaskManager.Bus["DataStructureTitle"].ToString();
                }

            if (TaskManager.Bus.ContainsKey("ResearchPlanId"))
            {
                model.ResearchPlanId = Convert.ToInt32(TaskManager.Bus["ResearchPlanId"].ToString());
        }

            if (TaskManager.Bus.ContainsKey("ResearchPlanTitle"))
        {
                model.ResearchPlanTitle = TaskManager.Bus["ResearchPlanTitle"].ToString();
            }

            if (TaskManager.Bus.ContainsKey("Title"))
            {
                model.DatasetTitle = TaskManager.Bus["Title"].ToString();
            }


            if (TaskManager.Bus.ContainsKey("Author"))
            {
                model.Author = TaskManager.Bus["Author"].ToString();
            }

            if (TaskManager.Bus.ContainsKey("Owner"))
            {
                model.Owner = TaskManager.Bus["Owner"].ToString();
            }

            if (TaskManager.Bus.ContainsKey("ProjectName"))
            {
                model.ProjectName = TaskManager.Bus["ProjectName"].ToString();
            }

            if (TaskManager.Bus.ContainsKey("Institute"))
            {
                model.ProjectInstitute = TaskManager.Bus["Institute"].ToString();
            }

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Step4(object[] data)
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            if (TaskManager != null)
            {
                if (TaskManager.Current().valid == false)
                {
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

            if (TaskManager.Bus.ContainsKey("DatasetId") && TaskManager.Bus.ContainsKey("DataStructureId"))
            {
                try
                {
                    long id = Convert.ToInt32(TaskManager.Bus["DatasetId"]);
                    DataStructureManager dsm = new DataStructureManager();
                    long iddsd = Convert.ToInt32(TaskManager.Bus["DataStructureId"]);
                    StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(iddsd);
                    dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);

                    // open file
                    ExcelReader reader = new ExcelReader();
                    Stream = (FileStream)Session["Stream"];

                    List<DataTuple> rows = reader.ReadFile(Stream, TaskManager.Bus["FileName"].ToString(), sds, (int)id);

                    if (reader.errorMessages.Count > 0)
                    {
                        //model.ErrorList = reader.errorMessages;
                    }
                    else
                    {
                        //model.Validated = true;

                        DatasetManager dm = new DatasetManager();
                        Dataset ds = dm.GetDataset(id);

                        if (dm.IsDatasetCheckedOutFor(ds.Id, "David") || dm.CheckOutDataset(ds.Id, "David"))
                        {
                            DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(ds.Id);
                            dm.EditDatasetVersion(workingCopy, rows, null, null);
                            dm.CheckInDataset(ds.Id, "upload data from upload wizard", "David");
                        }
                    }
                }
                catch
                {

                    //model.ErrorList.Add(new Error(ErrorType.Other, "Can not valid."));
                }
            }
            else
            {
                //model.ErrorList.Add(new Error(ErrorType.Dataset, "Dataset is not selected."));
            }

            return View();
        }

        public ActionResult CloseUpload()
        {
            Session["TaskManager"] = null;
            TaskManager = null;

            return RedirectToAction("UploadWizard");
        }

        #endregion

        #region Step Logic

        public ActionResult SelectFileProcess(HttpPostedFileBase SelectFileUploader)
        {

            TaskManager TaskManager = (TaskManager)Session["TaskManager"];
            if (SelectFileUploader != null)
            {
                // store file in archive
                //string filename = "D:\\" + DateTime.Now.Millisecond.ToString() + SelectFileUploader.FileName;
                //SelectFileUploader.SaveAs(filename);

                TaskManager.AddToBus("FileBase", SelectFileUploader);
                
                TaskManager.AddToBus("FileName", SelectFileUploader.FileName);
                TaskManager.AddToBus("Extention", SelectFileUploader.FileName.Split('.').Last());
                Session["TaskManager"] = TaskManager;
            }

            //return RedirectToAction("UploadWizard");
            return Content("");
        }

        public ActionResult Save(IEnumerable<HttpPostedFileBase> SelectFileUploader)
        {
            // The Name of the Upload component is "attachments" 
            foreach (var file in SelectFileUploader)
            {
                // Some browsers send file names with full path. This needs to be stripped.


                // The files are not actually saved in this demo
                // file.SaveAs(physicalPath);
        }
            // Return an empty string to signify success
            return Content("");
        }

        [HttpGet]
        public ActionResult CreateDataset()
        {
            CreateDatasetViewModel model = new CreateDatasetViewModel();
            if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
            if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];

            return PartialView("_createDataset", model);
        }

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
                XmlDocument emptyMetadata = new XmlDocument();
                emptyMetadata.Load(Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "emptymetadata.xml"));

                emptyMetadata.GetElementsByTagName("bgc:title")[0].InnerText = model.Title;
                emptyMetadata.GetElementsByTagName("bgc:owner")[0].InnerText = model.Owner;
                emptyMetadata.GetElementsByTagName("bgc:author")[0].InnerText = model.DatasetAuthor;
                emptyMetadata.GetElementsByTagName("bgc:projectName")[0].InnerText = model.ProjectName;
                emptyMetadata.GetElementsByTagName("bgc:institute")[0].InnerText = model.ProjectInstitute;

                //Add Metadata to Bus
                TaskManager.AddToBus("Title", model.Title);
                TaskManager.AddToBus("Owner", model.Owner);
                TaskManager.AddToBus("Author", model.DatasetAuthor);
                TaskManager.AddToBus("ProjectName", model.ProjectName);
                TaskManager.AddToBus("Institute", model.ProjectInstitute);

                DatasetManager dm = new DatasetManager();
                DataStructureManager dsm = new DataStructureManager();
                DataStructure dataStructure = dsm.StructuredDataStructureRepo.Get(model.DataStructureId);

                ResearchPlanManager rpm = new ResearchPlanManager();
                ResearchPlan rp = rpm.Repo.Get(model.ResearchPlanId);

                Dataset ds = dm.CreateEmptyDataset(dataStructure, rp);

                TaskManager.AddToBus("DatasetTitle", model.Title);
                TaskManager.AddToBus("ResearchPlanId", model.ResearchPlanId);

                if (dm.IsDatasetCheckedOutFor(ds.Id, "David") || dm.CheckOutDataset(ds.Id, "David"))
                {
                    emptyMetadata.GetElementsByTagName("bgc:id")[0].InnerText = ds.Id.ToString();

                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(ds.Id);
                    workingCopy.Metadata = emptyMetadata;
                    TaskManager.AddToBus("DatasetId",ds.Id);
                    TaskManager.AddToBus("ResearchPlanTitle", rp.Title);
                    dm.EditDatasetVersion(workingCopy, null, null, null);
                }


                Session["createDatasetWindowVisible"] = false;
            }
            else
            {
                Session["createDatasetWindowVisible"] = true;

                // put lists to model
            }

            if ((List<ListViewItem>)Session["DataStructureViewList"] != null) model.DatastructuresViewList = (List<ListViewItem>)Session["DataStructureViewList"];
            if ((List<ListViewItem>)Session["ResearchPlanViewList"] != null) model.ResearchPlanViewList = (List<ListViewItem>)Session["ResearchPlanViewList"];

                
            return PartialView("_createDataset", model);
            //return View("UploadWizard", TaskManager);
        }

        private bool IsSupportedExtention(TaskManager taskManager)
        { 
            if(taskManager.Bus.ContainsKey("Extention"))
            {
                string ext = taskManager.Bus["Extention"].ToString();

                if(ext.Equals(".xls")||ext.Equals(".xlsm")||ext.Equals(".xlsx")) return true;
            }

            return false;
        }

        [HttpPost]
        public ActionResult ValidateFile()
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];
            ValidationModel model = new ValidationModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey("DatasetId") && TaskManager.Bus.ContainsKey("DataStructureId"))
            {
                try
                {
                    long id = (long)Convert.ToInt32(TaskManager.Bus["DatasetId"]);
                    DataStructureManager dsm = new DataStructureManager();
                    long iddsd = (long)Convert.ToInt32(TaskManager.Bus["DataStructureId"]);
                    StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(iddsd);
                    dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);

                    // open file
                    ExcelReader reader = new ExcelReader();
                    Stream = (FileStream)Session["Stream"];

                    reader.ValidateFile(Stream, TaskManager.Bus["FileName"].ToString(), sds, id);

                    if (reader.errorMessages.Count > 0)
                    {
                        model.ErrorList = reader.errorMessages;
                        TaskManager.AddToBus("Valid", false);
                    }
                    else
                    {
                        model.Validated = true;
                        TaskManager.AddToBus("Valid", true);
                    }
                }
                catch
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "Can not valid."));
                    TaskManager.AddToBus("Valid", false);
                }
            }
            else
            {
                model.ErrorList.Add(new Error(ErrorType.Dataset, "Dataset is not selected."));
                TaskManager.AddToBus("Valid", false);
            }

            return PartialView("Step3", model);
        }

        #endregion
        
        #region Helper functions

        public List<ListViewItem> LoadDatasetVersionViewList()
        {
            DatasetManager dm = new DatasetManager();
            List<ListViewItem> temp = new List<ListViewItem>();
            List<long> datasetIdList = (from dataset in dm.DatasetRepo.Get()
                                      select dataset.Id).ToList();

            foreach (long datasetid in datasetIdList)
            {
                if (datasetid < 88)
                {
                    DatasetVersion dv = dm.GetDatasetLatestVersion(datasetid);
                    XmlNodeList xnl = dv.Metadata.GetElementsByTagName("bgc:title");
                    string title = "";

                    if (xnl.Count > 0)
                    {
                        title = xnl[0].InnerText;
                    }

                    temp.Add(new ListViewItem(dv.Id, title));
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

        #endregion

        #endregion

        #region upload Data

        public ActionResult UploadData()
        {
            
            DatasetManager dm = new DatasetManager();

            var idTempList = from ds in dm.DatasetRepo.Get()
                             select ds.Id.ToString();


            Session["DatasetIds"] = idTempList.ToList();

            return View();
        }

        [HttpPost]
        public ActionResult UploadDataProcess(HttpPostedFileBase UploadData, string SelectDataset, bool templateUsed, string submit)
        {
            if (UploadData != null)
            {
                string extension = ((string)Session["Extension"]).ToLower();
                ViewData["startdate"] = DateTime.Now;
                // create excel file reader information


                // store file in archive
                string filename = "D:\\" + DateTime.Now.Millisecond.ToString() + UploadData.FileName;
                UploadData.SaveAs(filename);

                // get datastructure
                DatasetManager dm = new DatasetManager();

                //XXX dataseid check
                DatasetVersion ds = dm.GetDatasetLatestVersion(Convert.ToInt64(SelectDataset));//Convert.ToInt16(SelectDataset));
                StructuredDataStructure sds = (StructuredDataStructure)(ds.Dataset.DataStructure.Self);

                // create a list of Datatuples 
                // result for reader return
                List<DataTuple> listDt = new List<DataTuple>();

                // create filestream
                FileStream stream;

                //Convert Selected ID from string to integer 
                int id = Convert.ToInt32(SelectDataset);

                #region ascii

                if (extension.Equals(".txt") | extension.Equals(".csv"))
                {
                    AsciiReader reader = new AsciiReader();
                    AsciiFileReaderInfo asciiFileReaderInfo = new AsciiFileReaderInfo();
                    try 
                    {
                        stream = reader.Open(filename);

                       

                            // if the user dont use the template
                            // Get all inputs from formular
                            // 
                        if (!templateUsed)
                        { 

                        #region without using template

                            for (int i = 0; i < this.Request.Form.AllKeys.Count(); i++)
                            {
                                string name = this.Request.Form.AllKeys[i];
                                string[] temp = this.Request.Form.GetValues(i);
                                string value = temp[0];
                                if (!String.IsNullOrEmpty(value))
                                {
                                    switch (name)
                                    {
                                        case "orientationDropDownList":
                                            {
                                                foreach (Orientation ms in Enum.GetValues(typeof(Orientation)))
                                                {
                                                    string selectedString = this.Request.Form.GetValues(i).First().ToString();
                                                    if (selectedString == ms.ToString()) asciiFileReaderInfo.Orientation = ms;
                                                }
                                                break;
                                            }
                                        case "seperatorDropDownList":
                                            {
                                                foreach (TextSeperator ms in Enum.GetValues(typeof(TextSeperator)))
                                                {
                                                    string selectedString = this.Request.Form.GetValues(i).First().ToString();
                                                    if (selectedString == ms.ToString()) asciiFileReaderInfo.Seperator = ms;
                                                }
                                                break;
                                            }

                                        case "decimalDropDownList":
                                            {
                                                foreach (DecimalCharacter ms in Enum.GetValues(typeof(DecimalCharacter)))
                                                {
                                                    string selectedString = this.Request.Form.GetValues(i).First().ToString();
                                                    if (selectedString == ms.ToString()) asciiFileReaderInfo.Decimal = ms;
                                                }
                                                break;
                                            }

                                        case "offset": asciiFileReaderInfo.Offset = Convert.ToInt16(value); break;
                                        case "variableStart": asciiFileReaderInfo.Variables = Convert.ToInt16(value); break;
                                        case "dataStart": asciiFileReaderInfo.Data = Convert.ToInt16(value); break;
                                        case "dataFormat": asciiFileReaderInfo.Dateformat = value; break;

                                    }
                                }
                            }
                       

                        

                        if (submit.Equals("Validate")) reader.ValidateFile(stream, UploadData.FileName, asciiFileReaderInfo, sds, id);
                        if (submit.Equals("Upload")) listDt = reader.ReadFile(stream, UploadData.FileName, asciiFileReaderInfo, sds, id);

                        ViewData["UploadedData"] = listDt;
                        stream.Close();

                #endregion
                        }
                        else
                        {
                            stream = null;//reader.Open(filename);
                            reader.errorMessages.Add(new Error(ErrorType.Other, "Not supported"));
                        }
                    }
                    catch (Exception ex)
                    {
                        reader.errorMessages.Add(new Error(ErrorType.Other, ex.Message.ToString()));
                    }

                    if (reader.errorMessages.Count == 0) ViewData["ErrorMessages"] = null;
                    else ViewData["ErrorMessages"] = reader.errorMessages;
                }

                #endregion 

                #region excel
                
                if (extension.Equals(".xls") | extension.Equals(".xlsx") | extension.Equals(".xlsm"))
                {
                    ExcelReader reader = new ExcelReader();
                    ExcelFileReaderInfo efri = new ExcelFileReaderInfo();
                    // try to open the file
                    try
                    {
                        

                        // Check if template or not, create filereaderinfo class or not
                        if (!templateUsed)
                        {
                            #region without using template

                            // if the user dont use the template
                            // Get all inputs from formular
                            // 
                            if (!templateUsed)
                            {
                                for (int i = 0; i < this.Request.Form.AllKeys.Count(); i++)
                                {
                                    string name = this.Request.Form.AllKeys[i];
                                    string[] temp = this.Request.Form.GetValues(i);
                                    string value = temp[0];
                                    if (!String.IsNullOrEmpty(value))
                                    {
                                        switch (name)
                                        {
                                            case "orientationDropDownList":
                                                {
                                                    foreach (Orientation ms in Enum.GetValues(typeof(Orientation)))
                                                    {
                                                        if (this.Request.Form.GetValues(i).ToString().Equals(ms.ToString()))
                                                            efri.Orientation = ms;
                                                    }
                                                    break;
                                                }

                                            case "decimalDropDownList":
                                                {
                                                    foreach (DecimalCharacter ms in Enum.GetValues(typeof(DecimalCharacter)))
                                                    {
                                                        if (this.Request.Form.GetValues(i).ToString().Equals(ms.ToString()))
                                                            efri.Decimal = ms;
                                                    }
                                                    break;
                                                }

                                            case "offset": efri.Offset = Convert.ToInt16(value); break;
                                            case "variableStart": efri.Variables = Convert.ToInt16(value); break;
                                            case "dataStart": efri.Data = Convert.ToInt16(value); break;
                                            case "dataFormat": efri.Dateformat = value; break;

                                        }
                                    }
                                }
                            }

                            #endregion
                            //stream = null;//
                            reader.Open(filename);
                            //reader.errorMessages.Add(new Error(ErrorType.Other, "Not supported"));
                            stream = reader.Open(filename);
                            //if (submit.Equals("Validate")) reader.ValidateFile(stream, UploadData.FileName, efri, sds, id);
                            if (submit.Equals("Upload")) listDt = reader.ReadFile(stream, UploadData.FileName,efri, sds, id);
                            //listDt = reader.ReadFile(stream, UploadData.FileName, efri ,sds,id);
                        }
                        else
                        {
                            stream = reader.Open(filename);
                            if (submit.Equals("Validate")) reader.ValidateFile(stream, UploadData.FileName, sds, id);
                            if (submit.Equals("Upload")) listDt = reader.ReadFile(stream, UploadData.FileName, sds, id);
                        }

                        ViewData["UploadedData"] = listDt;
                        if(stream!=null)stream.Close();


                    }
                    catch (Exception ex)
                    {
                        reader.errorMessages.Add(new Error(ErrorType.Other,ex.Message.ToString()));
                    }

                    if (reader.errorMessages.Count == 0) ViewData["ErrorMessages"] = null;
                    else ViewData["ErrorMessages"] = reader.errorMessages;
                } 
                #endregion

                DataTuple dt = listDt.First();
                dm.CreateDataTuple(5, dt.VariableValues, dt.Amendments, ds);
            }

            ViewData["enddate"] = DateTime.Now;

            


            return View("UploadData");
        }

        


        // is called when the user select a file
        // return a partial view with input patrameter for upload
        [HttpPost]
        public ActionResult GetDataFormatView(string extension, bool useTemplate)
        {
            string viewName = "_defaultDatatypFomularView";
            if (extension != null && extension!="")Session["Extension"] = extension;

            if (!useTemplate)
            {
                if (extension != null)
                {
                    switch (extension.ToLower())
                    {
                        case ".txt": viewName = "_txtFormularView"; break;
                        case ".csv": viewName = "_txtFormularView"; break;
                        case ".xls": viewName = "_xlsFormularView"; break;
                        case ".xlsx": viewName = "_xlsFormularView"; break;
                        case ".xlsm": viewName = "_xlsFormularView"; break;
                        default: break;
                    }
                }
            }
            else viewName = "_useTemplateView";
          

            return PartialView(viewName);
        }

        [HttpPost]
        public ActionResult GetDataFormatViewAfterSelectTemplate(bool useTemplate)
        {
            string viewName = "_defaultDatatypFomularView";
            string extension =  (string)Session["Extension"];

            if (!useTemplate)
            {
                if (extension != null)
                {
                    switch (extension.ToLower())
                    {
                        case ".txt": viewName = "_txtFormularView"; break;
                        case ".csv": viewName = "_txtFormularView"; break;
                        case ".xls": viewName = "_xlsFormularView"; break;
                        case ".xlsx": viewName = "_xlsFormularView"; break;
                        case ".xlsm": viewName = "_xlsFormularView"; break;
                        default: break;
                    }
                }
            }
            else viewName = "_useTemplateView";


            return PartialView(viewName);
        }

        [HttpPost]
        public ActionResult ResetExtensionSession()
        {
            Session["Extension"] = null;
            return PartialView();
        }

        #endregion
    }

    public class UpdateNameModel
    {
        public string Name { get; set; }
        public IEnumerable<int> Numbers { get; set; }
    } 

}
