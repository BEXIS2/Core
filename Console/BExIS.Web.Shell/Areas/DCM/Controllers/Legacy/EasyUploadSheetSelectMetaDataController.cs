﻿using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EasyUploadSheetSelectMetaDataController : BaseController
    {
        private EasyUploadTaskManager TaskManager;
        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        [HttpGet]
        public ActionResult SheetSelectMetaData(int index)
        {
            MetadataStructureManager msm = new MetadataStructureManager();
            try
            {
                TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

                //set current stepinfo based on index
                if (TaskManager != null)
                {
                    TaskManager.SetCurrent(index);

                    // remove if existing
                    TaskManager.RemoveExecutedStep(TaskManager.Current());
                }

                SelectMetaDataModel model = new SelectMetaDataModel();

                //Load available metadata structures
                IEnumerable<MetadataStructure> metadataStructureList = msm.Repo.Get();

                foreach (MetadataStructure metadataStructure in metadataStructureList)
                {
                    if (xmlDatasetHelper.IsActive(metadataStructure.Id) &&
                        xmlDatasetHelper.HasEntityType(metadataStructure.Id, "bexis.dlm.entities.data.dataset"))
                    {
                        model.AvailableMetadata.Add(new Tuple<long, string>(metadataStructure.Id, metadataStructure.Name));
                    }                        
                }
                //Sort the metadata structures
                model.AvailableMetadata.Sort((md1, md2) => md1.Item1.CompareTo(md2.Item1));

                //If there's already a selected Metadata schema, load its id into the model
                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SCHEMA))
                {
                    model.SelectedMetaDataId = Convert.ToInt64(TaskManager.Bus[EasyUploadTaskManager.SCHEMA]);
                }

                //if the title was changed at some point during the upload, load the title into the model
                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.DESCRIPTIONTITLE))
                {
                    model.DescriptionTitle = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.DESCRIPTIONTITLE]);
                }
                //if it wasn't changed yet, the default title is the filename
                else
                {
                    model.DescriptionTitle = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.FILENAME]);
                }

                model.StepInfo = TaskManager.Current();

                return PartialView(model);
            }
            finally
            {
                msm.Dispose();
            }
        }

        [HttpPost]
        public ActionResult SheetSelectMetaData(object[] data)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
            SelectMetaDataModel model = new SelectMetaDataModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager != null)
            {
                TaskManager.Current().SetValid(false);

                //SetValid only if a metadata-structure was selected
                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SCHEMA))
                {
                    if (Convert.ToInt64(TaskManager.Bus[EasyUploadTaskManager.SCHEMA]) >= 0)
                    {
                        TaskManager.Current().SetValid(true);
                    }
                    else
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other, "No valid Metadata schema is selected."));
                    }
                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "No Metadata schema is selected."));
                }

                //If the user typed in a title, the title must not be empty
                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.DESCRIPTIONTITLE))
                {
                    string tmp = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.DESCRIPTIONTITLE]);
                    if (String.IsNullOrWhiteSpace(tmp))
                    {
                        TaskManager.Current().SetValid(false);
                        model.ErrorList.Add(new Error(ErrorType.Other, "The title must not be empty."));
                    }
                }

                if (TaskManager.Current().valid == true) //Jump to next step of the upload
                {
                    TaskManager.AddExecutedStep(TaskManager.Current());
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
                else //Model isn't valid, display an error and stay on the same site
                {
                    TaskManager.Current().SetStatus(StepStatus.error);
                    MetadataStructureManager msm = new MetadataStructureManager();
                    try
                    {
                        IEnumerable<MetadataStructure> metadataStructureList = msm.Repo.Get();

                        foreach (MetadataStructure metadataStructure in metadataStructureList)
                        {
                            if (xmlDatasetHelper.IsActive(metadataStructure.Id) &&
                                xmlDatasetHelper.HasEntityType(metadataStructure.Id, "bexis.dlm.entities.data.dataset"))
                            {
                                model.AvailableMetadata.Add(new Tuple<long, string>(metadataStructure.Id, metadataStructure.Name));
                            }
                        }
                        //Sort the metadata structures
                        model.AvailableMetadata.Sort((md1, md2) => md1.Item1.CompareTo(md2.Item1));

                        //reload model
                        model.StepInfo = TaskManager.Current();

                        //if the title was changed at some point during the upload, load the title into the model
                        if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.DESCRIPTIONTITLE))
                        {
                            model.DescriptionTitle = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.DESCRIPTIONTITLE]);
                        }
                        //if it wasn't changed yet, the default title is the filename
                        else
                        {
                            model.DescriptionTitle = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.FILENAME]);
                        }
                    }
                    finally
                    {
                        msm.Dispose();
                    }
                }
            }

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SaveMetaDataSelection(object[] data)
        {
            MetadataStructureManager msm = new MetadataStructureManager();

            try
            {
                TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
                SelectMetaDataModel model = new SelectMetaDataModel();

                long metadataId = -1;

                if (TaskManager != null)
                {
                    //Load available metadata structures and store them in the model
                    IEnumerable<MetadataStructure> metadataStructureList = msm.Repo.Get();

                    foreach (MetadataStructure metadataStructure in metadataStructureList)
                    {
                        if (xmlDatasetHelper.IsActive(metadataStructure.Id) &&
                            xmlDatasetHelper.HasEntityType(metadataStructure.Id, "bexis.dlm.entities.data.dataset"))
                        {
                            model.AvailableMetadata.Add(new Tuple<long, string>(metadataStructure.Id, metadataStructure.Name));
                        }
                    }
                    //Sort the metadata structures
                    model.AvailableMetadata.Sort((md1, md2) => md1.Item1.CompareTo(md2.Item1));

                    TaskManager.Current().SetValid(false);

                    //Grabs the id of the selected metadata from the Http-Request
                    foreach (string key in Request.Form.AllKeys)
                    {
                        if ("metadataId" == key)
                        {
                            metadataId = Convert.ToInt64(Request.Form[key]);
                        }
                    }

                    //If a valid id was submitted, save its id as the currently selected model id
                    model.SelectedMetaDataId = metadataId;

                    TaskManager.AddToBus(EasyUploadTaskManager.SCHEMA, model.SelectedMetaDataId);
                    TaskManager.Current().SetValid(true);


                    //Store all other information in the model
                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.DESCRIPTIONTITLE))
                    {
                        model.DescriptionTitle = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.DESCRIPTIONTITLE]);
                    }
                    else
                    {
                        model.DescriptionTitle = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.FILENAME]);
                    }
                }

                return PartialView("SheetSelectMetaData", model);
            }
            finally
            {
                msm.Dispose();
            }
        }

        [HttpPost]
        public ActionResult SaveMetaDescription(object[] data)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
            SelectMetaDataModel model = new SelectMetaDataModel();

            if (TaskManager != null)
            {
                //Grab the description from the Http-Request and store it in the TaskManager
                foreach (string key in Request.Form.AllKeys)
                {
                    if ("DescriptionTitle" == key)
                    {
                        model.DescriptionTitle = Request.Form[key];
                        TaskManager.AddToBus(EasyUploadTaskManager.DESCRIPTIONTITLE, model.DescriptionTitle);
                    }
                }
                TaskManager.Current().SetValid(true);
            }

            //No need to update the model because the response will just be ignored in the calling javascript
            return PartialView("SheetSelectMetaData", model);
        }
    }
}