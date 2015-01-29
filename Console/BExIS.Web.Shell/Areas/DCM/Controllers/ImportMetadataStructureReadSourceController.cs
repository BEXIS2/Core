using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BExIS.Dcm.ImportMetadataStructureWizard;
using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Web.Shell.Areas.DCM.Models.ImportMetadata;
using BExIS.Xml.Helpers.Mapping;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class ImportMetadataStructureReadSourceController : Controller
    {

        private ImportMetadataStructureTaskManager TaskManager;

        //
        // GET: /DCM/ImportMetadataStructureReadSource/
        [HttpGet]
        public ActionResult ReadSource(int index)
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.SetCurrent(index);

            }

            ReadSourceModel model = new ReadSourceModel(TaskManager.Current());

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult ReadSource(object[] data)
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            TaskManager.Current().SetValid(true);

            if (TaskManager.Current().IsValid())
            {
                TaskManager.AddExecutedStep(TaskManager.Current());
                TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
            }

            ReadSourceModel model = new ReadSourceModel(TaskManager.Current());

            return PartialView(model);
        }


        public ActionResult SetRootNode(string name)
        {
             TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

             if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ROOT_NODE))
                 TaskManager.Bus[ImportMetadataStructureTaskManager.ROOT_NODE] = name;
             else
                 TaskManager.Bus.Add(ImportMetadataStructureTaskManager.ROOT_NODE, name);


             return Content("");
        }

        public ActionResult SetSchemaName(string name)
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SCHEMA_NAME))
                TaskManager.Bus[ImportMetadataStructureTaskManager.SCHEMA_NAME] = name;
            else
                TaskManager.Bus.Add(ImportMetadataStructureTaskManager.SCHEMA_NAME, name);

            return Content("");
        }

        public ActionResult GenerateMS()
        {
            string root = "";
            string schemaName = "";

            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ROOT_NODE))
                root = TaskManager.Bus[ImportMetadataStructureTaskManager.ROOT_NODE].ToString();

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SCHEMA_NAME))
                schemaName = TaskManager.Bus[ImportMetadataStructureTaskManager.SCHEMA_NAME].ToString();

            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            string path = TaskManager.Bus[ImportMetadataStructureTaskManager.FILEPATH].ToString();
            //path = @"https://code.ecoinformatics.org/code/eml/tags/RELEASE_EML_2_1_1/eml.xsd";

            ReadSourceModel model = new ReadSourceModel(TaskManager.Current());
            model.SchemaName = schemaName;
            model.RootNode = root;

            //open schema
            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();
            try
            {
                xmlSchemaManager.Load(path);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.ErrorList.Add(new Error(ErrorType.Other, "Can not find any dependent files to the selected schema."));
            }

            if (model.ErrorList.Count == 0)
            {
                try
                {
                    xmlSchemaManager.GenerateMetadataStructure(root, schemaName);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    model.ErrorList.Add(new Error(ErrorType.Other, "Can not create metadatastructure."));
                }
            }

            model.StepInfo.notExecuted = false;

            return PartialView("ReadSource",model);
        }
    }
}
