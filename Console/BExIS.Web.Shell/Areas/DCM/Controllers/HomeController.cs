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
                string path = Path.Combine("D:\\BPP\\Tech\\ModuleBranches\\DCM\\Workspace\\Modules\\DCM", "TaskInfo.xml");
                XmlDocument xmlTaskInfo = new XmlDocument();
                xmlTaskInfo.Load(path);

                Session["TaskManager"] = TaskManager.Bind(xmlTaskInfo);
                Session["Filestream"] = Stream;
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
                            TaskManager.Current().SetValid(true);
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
            
            model.Datasets = (from datasets in dm.DatasetVersionRepo.Get() select datasets.Id).ToList();
            

            //load datastructure ids
            DataStructureManager dsm = new DataStructureManager();
            model.Datastructures = (from datastructure in dsm.SdsRepo.Get() select datastructure.Id).ToList();

            model.StepInfo = TaskManager.Current();
            model.DatasetViewModel = new CreateDatasetViewModel();
            model.DatasetViewModel.DataStructureIds = model.Datastructures;

            return PartialView(model);

        }

        [HttpPost]
        public ActionResult Step2(object[] data)
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            if (TaskManager != null)
            {
                if (data != null) TaskManager.AddToBus(data);

                

                if (TaskManager.Current().valid == false)
                {
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
                else
                {
                    TaskManager.Current().SetStatus(StepStatus.error);
                }
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

            return PartialView();
        }

        [HttpPost]
        public ActionResult Step3(object[] data)
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

        [HttpGet]
        public ActionResult Step4(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
                TaskManager.SetCurrent(index);

            return PartialView();
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

        [HttpGet]
        public ActionResult Step5(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
                TaskManager.SetCurrent(index);

            return PartialView();
        }

        [HttpPost]
        public ActionResult Step5()
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
            return View();
        }

        public ActionResult CloseUpload()
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];
            TaskManager.SetCurrent(TaskManager.Start());

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
                //TaskManager.AddToBus("TempFilePath", filename);
                Session["TaskManager"] = TaskManager;
            }

            return RedirectToAction("UploadWizard");
        }

        public ActionResult CreateDataset(CreateDatasetViewModel model)
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            if (ModelState.IsValid)
            {
                Session["createDatasetWindowVisible"] = false;
            }
            else
            {
                Session["createDatasetWindowVisible"] = true;
            }

            return View("UploadWizard", TaskManager);
        }

        private bool IsSupportedExtention(TaskManager taskManager)
        { 
            if(taskManager.Bus.ContainsKey("Extention"))
            {
                string ext = taskManager.Bus["Extention"].ToString();

                if(ext.Equals("xls")||ext.Equals("xlsm")||ext.Equals("xlsx")) return true;
            }

            return false;
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
