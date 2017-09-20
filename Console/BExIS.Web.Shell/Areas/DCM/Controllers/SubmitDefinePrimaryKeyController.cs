using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class SubmitDefinePrimaryKeyController : BaseController
    {
        private TaskManager TaskManager;
        private UploadWizardHelper uploadWizardHelper = new UploadWizardHelper();

        // GET: /DCM/DefinePrimaryKey/
        [HttpGet]
        public ActionResult DefinePrimaryKey(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.SetCurrent(index);
                // remove if existing
                TaskManager.RemoveExecutedStep(TaskManager.Current());

                if (TaskManager.Bus.ContainsKey(TaskManager.PRIMARY_KEYS_UNIQUE))
                {
                    TaskManager.Bus[TaskManager.PRIMARY_KEYS_UNIQUE] = false;
                }
                else
                {
                    TaskManager.AddToBus(TaskManager.PRIMARY_KEYS_UNIQUE, false);
                }

            }

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

            // load maybe selected primary keys
            if (TaskManager.Bus.ContainsKey(TaskManager.PRIMARY_KEYS))
            {
                model.PK_Id_List = (List<long>)TaskManager.Bus[TaskManager.PRIMARY_KEYS];
            }

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult DefinePrimaryKey(object[] data)
        {
            TaskManager = (BExIS.Dcm.UploadWizard.TaskManager)Session["TaskManager"];
            PrimaryKeyViewModel model = new PrimaryKeyViewModel();
            model.StepInfo = TaskManager.Current();

            TaskManager.Current().SetValid(false);

            if (TaskManager.Bus.ContainsKey(TaskManager.PRIMARY_KEYS_UNIQUE))
            {
                TaskManager.Current().SetValid((bool)TaskManager.Bus[TaskManager.PRIMARY_KEYS_UNIQUE]);
            }

            if (TaskManager.Current().IsValid())
            {
                TaskManager.AddExecutedStep(TaskManager.Current());
                TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
            }

            // if step not valid load data if its not exist in the session for the model
            if (Session["VariableLableList"] == null)
            {
                model.VariableLableList = LoadVariableLableList();
                Session["VariableLableList"] = model.VariableLableList;
            }
            else
            {
                model.VariableLableList = (List<ListViewItem>)Session["VariableLableList"];
            }

            // load maybe selected primary keys
            if (TaskManager.Bus.ContainsKey(TaskManager.PRIMARY_KEYS))
            {
                model.PK_Id_List = (List<long>)TaskManager.Bus[TaskManager.PRIMARY_KEYS];
            }

            // and add error to model
            model.ErrorList.Add(new Error(ErrorType.Other, "Define a primary key combination and check it please."));

            return PartialView(model);
        }

        //[MeasurePerformance]
        public ActionResult CheckPrimaryKeys(object[] data)
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            PrimaryKeyViewModel model = new PrimaryKeyViewModel();
            model.StepInfo = TaskManager.Current();

            model.VariableLableList = (List<ListViewItem>)Session["VariableLableList"];

            try
            {

                if (data != null)
                {
                    // check Identifier
                    List<string> identifiersLables = data.ToList().ConvertAll<string>(delegate (object i) { return i.ToString(); });
                    List<long> identifiers = new List<long>();
                    List<ListViewItem> pks = new List<ListViewItem>();

                    foreach (string value in identifiersLables)
                    {
                        identifiers.Add(
                                    model.VariableLableList.Where(p => p.Title.Equals(value)).First().Id
                            );

                        pks.Add(model.VariableLableList.Where(p => p.Title.Equals(value)).First());
                    }

                    List<string> tempDataset = new List<string>();
                    List<string> tempFromFile = new List<string>();

                    //bool IsUniqueInDb = UploadWizardHelper.IsUnique(Convert.ToInt64(TaskManager.Bus[TaskManager.DATASET_ID].ToString()), identifiers);

                    // temporary solution to make it a little bit faster
                    // direct link to xml is no t allow, need to call functions from dlm
                    bool IsUniqueInDb = uploadWizardHelper.IsUnique2(Convert.ToInt64(TaskManager.Bus[TaskManager.DATASET_ID].ToString()), identifiers);

                    bool IsUniqueInFile = uploadWizardHelper.IsUnique(TaskManager, Convert.ToInt64(TaskManager.Bus[TaskManager.DATASET_ID].ToString()), identifiers, TaskManager.Bus[TaskManager.EXTENTION].ToString(), TaskManager.Bus[TaskManager.FILENAME].ToString());

                    if (IsUniqueInDb && IsUniqueInFile)
                    {
                        model.IsUnique = true;

                        if (TaskManager.Bus.ContainsKey(TaskManager.PRIMARY_KEYS_UNIQUE))
                        {
                            TaskManager.Bus[TaskManager.PRIMARY_KEYS_UNIQUE] = true;
                        }
                        else
                        {
                            TaskManager.AddToBus(TaskManager.PRIMARY_KEYS_UNIQUE, true);
                        }

                        TaskManager.Bus[TaskManager.PRIMARY_KEYS] = identifiers;
                        Session["TaskManager"] = TaskManager;
                        model.PrimaryKeysList = pks;
                        model.PK_Id_List = identifiers;
                    }
                    else
                    {
                        model.IsUnique = false;
                        model.PrimaryKeysList = pks;

                        if (TaskManager.Bus.ContainsKey(TaskManager.PRIMARY_KEYS_UNIQUE))
                        {
                            TaskManager.Bus[TaskManager.PRIMARY_KEYS_UNIQUE] = false;
                        }
                        else
                        {
                            TaskManager.AddToBus(TaskManager.PRIMARY_KEYS_UNIQUE, false);
                        }

                        if (TaskManager.Bus.ContainsKey(TaskManager.PRIMARY_KEYS))
                        {
                            TaskManager.Bus.Remove(TaskManager.PRIMARY_KEYS);
                        }
                        model.ErrorList.Add(new Error(ErrorType.Other, "Selection is not unique"));

                    }


                }
                else
                {
                    model.IsUnique = false;

                    if (TaskManager.Bus.ContainsKey(TaskManager.PRIMARY_KEYS_UNIQUE))
                    {
                        TaskManager.Bus[TaskManager.PRIMARY_KEYS_UNIQUE] = false;
                    }
                    else
                    {
                        TaskManager.AddToBus(TaskManager.PRIMARY_KEYS_UNIQUE, false);
                    }

                    if (TaskManager.Bus.ContainsKey(TaskManager.PRIMARY_KEYS))
                    {
                        TaskManager.Bus.Remove(TaskManager.PRIMARY_KEYS);
                    }
                    model.ErrorList.Add(new Error(ErrorType.Other, "Please select one or a combination of variables to define a primary key."));
                }

            }
            catch (Exception e)
            {
                model.ErrorList.Add(new Error(ErrorType.Other, e.Message));
            }

            return PartialView(TaskManager.Current().GetActionInfo.ActionName, model);
        }

        #region private methods

        private List<ListViewItem> LoadVariableLableList()
        {
            DataStructureManager datastructureManager = new DataStructureManager();

            try
            {

                StructuredDataStructure structuredDatastructure = datastructureManager.StructuredDataStructureRepo.Get(Convert.ToInt64(TaskManager.Bus["DataStructureId"]));

                return (from var in structuredDatastructure.Variables
                        select new ListViewItem
                        {
                            Id = var.Id,
                            Title = var.Label
                        }).ToList();
            }
            finally
            {
                datastructureManager.Dispose();
            }
        }

        #endregion
    }
}
