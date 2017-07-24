using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Utils.Helpers;
using BExIS.Utils.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using Vaiona.Logging;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EasyUploadSelectAreasController : Controller
    {
        private EasyUploadTaskManager TaskManager;

        [HttpGet]
        public ActionResult SelectAreas(int index)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.SetCurrent(index);

                // remove if existing
                TaskManager.RemoveExecutedStep(TaskManager.Current());
            }

            /*
             * Use the given file and the given sheet format to create a json-table
             * */
            string filePath = TaskManager.Bus[EasyUploadTaskManager.FILEPATH].ToString();
            FileStream fis = null;
            string jsonTable = "{}";

            SelectAreasModel model = new SelectAreasModel();

            // When jumping back to this step
            // Check if the json-table has already been created, if yes, use it
            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_JSON_DATA))
            {
                if (!String.IsNullOrEmpty(Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.SHEET_JSON_DATA])))
                {
                    model.JsonTableData = TaskManager.Bus[EasyUploadTaskManager.SHEET_JSON_DATA].ToString();
                }
            }
            else //Create a new json-table
            {
                try
                {
                    fis = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                    string sheetFormatString = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.SHEET_FORMAT]);

                    SheetFormat CurrentSheetFormat = 0;
                    Enum.TryParse<SheetFormat>(sheetFormatString, true, out CurrentSheetFormat);

                    JsonTableGenerator EUEReader = new JsonTableGenerator();
                    EUEReader.Open(fis);
                    jsonTable = EUEReader.GenerateJsonTable(CurrentSheetFormat);

                    if (!String.IsNullOrEmpty(jsonTable))
                    {
                        TaskManager.AddToBus(EasyUploadTaskManager.SHEET_JSON_DATA, jsonTable);
                    }

                    model.JsonTableData = jsonTable;
                }
                catch (Exception ex)
                {
                    LoggerFactory.LogCustom(ex.Message);
                }
                finally
                {
                    if (fis != null)
                    {
                        fis.Close();
                    }
                }
            }




            // Check if the areas have already been selected, if yes, use them (Important when jumping back to this step)
            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_DATA_AREA))
            {
                model.DataArea = (List<string>)TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA];
            }

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_HEADER_AREA))
            {
                model.HeaderArea = TaskManager.Bus[EasyUploadTaskManager.SHEET_HEADER_AREA].ToString();
            }

            model.StepInfo = TaskManager.Current();

            return PartialView(model);

        }

        [HttpPost]
        public ActionResult SelectAreas(object[] data)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
            SelectAreasModel model = new SelectAreasModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager != null)
            {
                TaskManager.Current().SetValid(false);

                //If Header and data areas have been selected, you can proceed with the next step
                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_JSON_DATA) &&
                    TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_DATA_AREA) &&
                    TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_HEADER_AREA))
                {
                    bool isJsonDataEmpty = String.IsNullOrEmpty(Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.SHEET_JSON_DATA]));
                    bool isDataAreaEmpty = ((List<string>)TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA]).Count == 0;
                    bool isHeadAreaEmpty = String.IsNullOrEmpty(Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.SHEET_HEADER_AREA]));

                    if (!isJsonDataEmpty && !isDataAreaEmpty && !isHeadAreaEmpty)
                    {
                        TaskManager.Current().SetValid(true);
                    }
                    else
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other, "Some Areas are not selected."));
                    }
                }
                else //Else stay on the same page, display an error
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "Some Areas are not selected."));
                }

                if (TaskManager.Current().valid == true) //Redirect if the state is valid
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
                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_DATA_AREA))
                    {
                        model.DataArea = (List<string>)TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA];
                    }

                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_HEADER_AREA))
                    {
                        model.HeaderArea = TaskManager.Bus[EasyUploadTaskManager.SHEET_HEADER_AREA].ToString();
                    }

                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_JSON_DATA))
                    {
                        model.JsonTableData = TaskManager.Bus[EasyUploadTaskManager.SHEET_JSON_DATA].ToString();
                    }

                    model.StepInfo = TaskManager.Current();
                }
            }

            return PartialView(model);
        }

        /*
         * Save the selected area either as data area or as header area
         * */
        [HttpPost]
        public ActionResult SelectedAreaToBus()
        {
            string headerArea = null;
            string dataArea = null;

            foreach (string key in Request.Form.AllKeys)
            {
                if ("dataArea" == key)
                {
                    dataArea = Request.Form[key];
                }
                if ("headerArea" == key)
                {
                    headerArea = Request.Form[key];
                }
            }

            SelectAreasModel model = new SelectAreasModel();

            EasyUploadTaskManager TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            if (dataArea != null)
            {
                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_DATA_AREA))
                {
                    model.DataArea = (List<string>)TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA];
                }

                //dataArea == "" means the resetButton was clicked
                if (model.DataArea == null || dataArea == "")
                {
                    model.DataArea = new List<string>();
                }
                if (dataArea != "")
                {
                    var serializer = new JavaScriptSerializer();
                    int[] newArea = serializer.Deserialize<int[]>(dataArea);
                    Boolean contains = false;
                    foreach (string area in model.DataArea)
                    {
                        int[] oldArea = serializer.Deserialize<int[]>(area);
                        //If one of the already selected areas contains the new one, don't add the new one to the selection (prevents duplicate selection)
                        if (oldArea[0] <= newArea[0] && oldArea[2] >= newArea[2] &&
                            oldArea[1] <= newArea[1] && oldArea[3] >= newArea[3])
                        {
                            contains = true;
                        }
                    }
                    if (!contains)
                        model.DataArea.Add(dataArea);
                }


                TaskManager.AddToBus(EasyUploadTaskManager.SHEET_DATA_AREA, model.DataArea);
            }

            if (headerArea != null)
            {
                TaskManager.AddToBus(EasyUploadTaskManager.SHEET_HEADER_AREA, headerArea);
                model.HeaderArea = headerArea;
            }

            Session["TaskManager"] = TaskManager;

            model.StepInfo = TaskManager.Current();

            return PartialView("SelectAreas", model);

        }
    }


}