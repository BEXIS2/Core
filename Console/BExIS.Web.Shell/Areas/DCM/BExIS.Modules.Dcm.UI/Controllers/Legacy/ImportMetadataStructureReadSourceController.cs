using BExIS.Dcm.ImportMetadataStructureWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models.ImportMetadata;
using BExIS.Utils.Helpers;
using BExIS.Xml.Helpers.Mapping;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace BExIS.Modules.Dcm.UI.Controllers
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
                TaskManager.RemoveExecutedStep(TaskManager.Current());
                TaskManager.Current().notExecuted = true;
            }

            ReadSourceModel model = new ReadSourceModel(TaskManager.Current());

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.IS_GENERATE))
                model.IsGenerated = (bool)TaskManager.Bus[ImportMetadataStructureTaskManager.IS_GENERATE];

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ROOT_NODE))
                model.RootNode = TaskManager.Bus[ImportMetadataStructureTaskManager.ROOT_NODE].ToString();

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SCHEMA_NAME))
                model.SchemaName = TaskManager.Bus[ImportMetadataStructureTaskManager.SCHEMA_NAME].ToString();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult ReadSource(object[] data)
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            ReadSourceModel model = new ReadSourceModel(TaskManager.Current());

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.IS_GENERATE))
            {
                TaskManager.Current().SetValid(true);
                TaskManager.Current().SetStatus(StepStatus.success);
            }
            else
            {
                ModelState.AddModelError("", "Please click generate button.");
            }

            if (TaskManager.Current().IsValid())
            {
                TaskManager.AddExecutedStep(TaskManager.Current());
                TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
            }

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

        public JsonResult SetSchemaName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return Json("A Metadata structure must have a name.", JsonRequestBehavior.AllowGet);
            }

            if (!RegExHelper.IsFilenameValid(name))
            {
                return Json("Name : \" " + name + " \" is invalid. These special characters are not allowed : \\/:*?\"<>|", JsonRequestBehavior.AllowGet);
            }

            if (SchemaNameExist(name))
            {
                return Json("A Metadata structure with this name already exist. Please choose a other name.", JsonRequestBehavior.AllowGet);
            }

            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SCHEMA_NAME))
                TaskManager.Bus[ImportMetadataStructureTaskManager.SCHEMA_NAME] = name;
            else
                TaskManager.Bus.Add(ImportMetadataStructureTaskManager.SCHEMA_NAME, name);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public bool SchemaNameExist(string SchemaName)
        {
            using (MetadataStructureManager msm = new MetadataStructureManager())
            {
                if (msm.Repo.Get().Where(m => m.Name.ToLower().Equals(SchemaName.ToLower())).Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public ActionResult GenerateMS()
        {
            //open schema
            XmlSchemaManager xmlSchemaManager = new XmlSchemaManager();

            string root = "";
            string schemaName = "";
            long metadataStructureid = 0;

            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ROOT_NODE))
                root = TaskManager.Bus[ImportMetadataStructureTaskManager.ROOT_NODE].ToString();

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SCHEMA_NAME))
                schemaName = TaskManager.Bus[ImportMetadataStructureTaskManager.SCHEMA_NAME].ToString();

            string path = TaskManager.Bus[ImportMetadataStructureTaskManager.FILEPATH].ToString();
            //path = @"https://code.ecoinformatics.org/code/eml/tags/RELEASE_EML_2_1_1/eml.xsd";

            ReadSourceModel model = new ReadSourceModel(TaskManager.Current());
            model.SchemaName = schemaName;
            model.RootNode = root;

            if (!RegExHelper.IsFilenameValid(schemaName))
            {
                model.ErrorList.Add(new Error(ErrorType.Other,
                        "Name : \" " + schemaName + " \" is invalid. These special characters are not allowed : \\/:*?\"<>|"));
            }
            else
            {
                try
                {
                    //file.WriteLine("check schema exist");
                    if (SchemaNameExist(schemaName))
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other,
                            "A Metadata structure with this name already exist. Please choose a other name."));
                    }
                    else
                    if (String.IsNullOrEmpty(schemaName))
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other, "A Metadata structure must have a name."));
                    }
                    else
                        xmlSchemaManager.Load(path, GetUserNameOrDefault());

                    if (!String.IsNullOrEmpty(model.RootNode) && !xmlSchemaManager.Elements.Any(e => e.Name.Equals(model.RootNode)))
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other, "Root node not exist"));
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    model.ErrorList.Add(new Error(ErrorType.Other, "Can not create metadatastructure."));
                    throw ex;
                }

                if (model.ErrorList.Count == 0)
                {
                    try
                    {
                        metadataStructureid = xmlSchemaManager.GenerateMetadataStructure(root, schemaName);
                    }
                    catch (Exception ex)
                    {
                        xmlSchemaManager.Delete(schemaName);
                        ModelState.AddModelError("", ex.Message);
                        model.ErrorList.Add(new Error(ErrorType.Other, "Can not create metadatastructure."));
                    }
                }

                TaskManager.AddToBus(ImportMetadataStructureTaskManager.MAPPING_FILE_NAME_IMPORT,
                    xmlSchemaManager.mappingFileNameImport);
                TaskManager.AddToBus(ImportMetadataStructureTaskManager.MAPPING_FILE_NAME_EXPORT,
                    xmlSchemaManager.mappingFileNameExport);

                model.StepInfo.notExecuted = false;

                if (model.ErrorList.Count == 0)
                {
                    model.IsGenerated = true;

                    if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.IS_GENERATE))
                        TaskManager.Bus[ImportMetadataStructureTaskManager.IS_GENERATE] = true;
                    else
                        TaskManager.Bus.Add(ImportMetadataStructureTaskManager.IS_GENERATE, true);

                    if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID))
                        TaskManager.Bus[ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID] = metadataStructureid;
                    else
                        TaskManager.Bus.Add(ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID, metadataStructureid);
                }
            }

            return PartialView("ReadSource", model);
        }

        // chekc if user exist
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
    }
}