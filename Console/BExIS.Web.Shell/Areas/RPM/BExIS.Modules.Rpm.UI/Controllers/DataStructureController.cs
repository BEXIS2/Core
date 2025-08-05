using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
using BExIS.IO;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Rpm.UI.Helpers;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Modules.Rpm.UI.Models.DataStructure;
using BExIS.UI.Helpers;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Hooks.Logs;
using BExIS.UI.Models;
using BExIS.Utils.Models;
using BExIS.Utils.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Data;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class DataStructureController : Controller
    {
        public ActionResult Index()
        {
            string module = "rpm";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult DataStructures()
        {
            List<DataStructureModel> tmp = new List<DataStructureModel>();
            using (var dataStructureManger = new DataStructureManager())
            using (var datasetManager = new DatasetManager())
            {
                var dataStructures = dataStructureManger.StructuredDataStructureRepo.Query().Select(e => new DataStructureModel() { Id = e.Id, Description = e.Description, Title = e.Name, LinkedTo = new List<long>() }).ToList().OrderByDescending(i => i.Id);

                foreach (var entity in dataStructures)
                {
                    entity.LinkedTo = datasetManager.DatasetRepo.Query().Where(d => (d.DataStructure != null && d.DataStructure.Id.Equals(entity.Id))).Select(d => d.Id).ToList();
                    tmp.Add(entity);
                }
            }

            return Json(tmp.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Upload()
        {
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        string fname = getFileName(file);
                        //data/datasets/1/1/
                        var dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                        var user = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);
                        var storepath = Path.Combine(dataPath, "Temp", user);

                        // if folder not exist
                        if (!Directory.Exists(storepath)) Directory.CreateDirectory(storepath);

                        var path = Path.Combine(storepath, fname);

                        file.SaveAs(path);
                    }
                }
                catch (Exception ex)
                {
                    throw new FileNotFoundException(ex.Message);
                }

                this.Response.StatusCode = 200;
                return Json("File Uploaded Successfully!");
            }
            else
            {
                this.Response.StatusCode = 400;
                return Json("No files selected.");
            }
        }

        public ActionResult Create(string file, long entityId = 0, long structureId = 0, int version = 0)
        {
            string module = "rpm";

            ViewData["id"] = entityId;
            ViewData["version"] = version;
            ViewData["file"] = file;
            ViewData["structureId"] = structureId;
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            // get from settings, if template is required or not
            bool isTemplateRequired = (bool)ModuleManager.GetModuleSettings("RPM").GetValueByKey("isTemplateRequired");
            ViewData["isTemplateRequired"] = isTemplateRequired;

            bool isMeaningRequired = (bool)ModuleManager.GetModuleSettings("RPM").GetValueByKey("isMeaningRequired");
            ViewData["isMeaningRequired"] = isMeaningRequired;

            bool setByTemplate = (bool)ModuleManager.GetModuleSettings("RPM").GetValueByKey("setByTemplate");
            ViewData["setByTemplate"] = setByTemplate;

            bool changeablePrimaryKey = (bool)ModuleManager.GetModuleSettings("RPM").GetValueByKey("changeablePrimaryKey");
            ViewData["changeablePrimaryKey"] = changeablePrimaryKey;

            bool enforcePrimaryKey = (bool)ModuleManager.GetModuleSettings("RPM").GetValueByKey("enforcePrimaryKey");
            ViewData["enforcePrimaryKey"] = enforcePrimaryKey;

            return View("Create");
        }

        public ActionResult Edit(long structureId = 0)
        {
            string module = "rpm";
            DataStructureHelper structureHelper = new DataStructureHelper();

            ViewData["structureId"] = structureId;
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            // get from settings, if template is required or not
            bool isTemplateRequired = (bool)ModuleManager.GetModuleSettings("RPM").GetValueByKey("isTemplateRequired");
            ViewData["isTemplateRequired"] = isTemplateRequired;

            bool isMeaningRequired = (bool)ModuleManager.GetModuleSettings("RPM").GetValueByKey("isMeaningRequired");
            ViewData["isMeaningRequired"] = isMeaningRequired;

            bool setByTemplate = (bool)ModuleManager.GetModuleSettings("RPM").GetValueByKey("setByTemplate");
            ViewData["setByTemplate"] = setByTemplate;

            bool updateDescriptionByTemplate = (bool)ModuleManager.GetModuleSettings("RPM").GetValueByKey("updateDescriptionByTemplate");
            ViewData["updateDescriptionByTemplate"] = setByTemplate;
            

            bool changeablePrimaryKey = (bool)ModuleManager.GetModuleSettings("RPM").GetValueByKey("changeablePrimaryKey");
            ViewData["changeablePrimaryKey"] = changeablePrimaryKey;

            bool enforcePrimaryKey = (bool)ModuleManager.GetModuleSettings("RPM").GetValueByKey("enforcePrimaryKey");
            ViewData["enforcePrimaryKey"] = enforcePrimaryKey;

            ViewData["dataExist"] = structureHelper.InUseAndDataExist(structureId);

            return View("Edit");
        }

        

        [JsonNetFilter]
        [HttpPost]
        public JsonResult Create(DataStructureCreationModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            using (var structureManager = new DataStructureManager())
            using (var variableManager = new VariableManager())
            using (var unitManager = new UnitManager())
            using (var datatypeManager = new DataTypeManager())
            using (var datasetManager = new DatasetManager())
            using (var missingValueManager = new MissingValueManager())
            using (var constraintManager = new ConstraintManager())
            {
                // create strutcure
                StructuredDataStructure newStructure = structureManager.CreateStructuredDataStructure(
                        model.Title,
                        model.Description,
                        null,
                        null,
                        DataStructureCategory.Generic
                    );

                DataStructureHelper helper = new DataStructureHelper();
                VariableHelper variableHelper = new VariableHelper();

                // create variable
                foreach (var variable in model.Variables)
                {
                    // if needed gerenate units??
                    // if needed gerenate Variabe Template??

                    // get datatype
                    var dataType = datatypeManager.Repo.Get(variable.DataType.Id);
                    if (dataType == null) { }// create;

                    // get unit
                    var unit = unitManager.Repo.Get(variable.Unit.Id);
                    if (unit == null) { }// create;

                    // set displayPattern
                    int displayPattern = -1;
                    if (variable.DisplayPattern != null) displayPattern = Convert.ToInt32(variable.DisplayPattern.Id);

                    // create var and add to structure

                    // get orderNo
                    int orderNo = model.Variables.IndexOf(variable) + 1;

                    // list missing values
                    List<MissingValue> missingValues = new List<MissingValue>();
                    if(variable.MissingValues.Any()) missingValues = variableHelper.ConvertTo(variable.MissingValues);
                    else missingValues = variableHelper.ConvertTo(model.MissingValues);

                    long varTempId = variable.Template != null ? variable.Template.Id : 0;

                    // generate variables
                    var result = variableManager.CreateVariable(
                        variable.Name,
                        dataType,
                        unit,
                        newStructure.Id,
                        variable.IsOptional,
                        variable.IsKey,
                        orderNo,
                        varTempId,
                        variable.Description,
                        "",
                        "",
                        displayPattern,
                        missingValues, // add also missing values that came from varaible it self
                        variable.Constraints.Select(co => co.Id).ToList(),
                        variable.Meanings.Select(m => m.Id).ToList()
                        );

                    newStructure = structureManager.AddVariable(newStructure.Id, result.Id);
                }

                // if id == 0 that means only create the strutcure and stop here
                // otherwise the creation belongs to a dataset and the link to the dataset can be created

                // store link to entity
                if (model.EntityId > 0)
                {
                    var dataset = datasetManager.GetDataset(model.EntityId);
                    dataset.DataStructure = newStructure;
                    datasetManager.UpdateDataset(dataset);
                }
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult Save(DataStructureEditModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Id <= 0) throw new ArgumentNullException(nameof(model.Id));

            using (var structureManager = new DataStructureManager())
            using (var variableManager = new VariableManager())
            using (var meaningManager = new MeaningManager())
            using (var unitManager = new UnitManager())
            using (var datatypeManager = new DataTypeManager())
            using (var datasetManager = new DatasetManager())
            using (var missingValueManager = new MissingValueManager())
            using (var constraintsManager = new ConstraintManager())
            {
                // create strutcure
                StructuredDataStructure structure = structureManager.StructuredDataStructureRepo.Get(model.Id);
                if (structure == null) throw new NullReferenceException("Structure not exist with id: " + model.Id);

                structure.Name = model.Title;
                structure.Description = model.Description;
                //structure = structureManager.UpdateStructuredDataStructure(structure);

                // update variable
                foreach (var variable in model.Variables)
                {
                    // get datatype
                    var dataType = datatypeManager.Repo.Get(variable.DataType.Id);
                    if (dataType == null) { }// create;

                    // get unit
                    var unit = unitManager.Repo.Get(variable.Unit.Id);
                    if (unit == null) { }// create;

                    // set displayPattern
                    int displayPattern = -1;
                    if (variable.DisplayPattern != null) displayPattern = Convert.ToInt32(variable.DisplayPattern.Id);

                    // create var and add to structure

                    // get orderNo
                    int orderNo = model.Variables.IndexOf(variable) + 1;
                    VariableInstance updatedVariable = new VariableInstance();
                    VariableHelper variableHelper = new VariableHelper();

                    // update variable
                    if (variable.Id > 0)
                    {
                        updatedVariable = variableManager.GetVariable(variable.Id);
                        updatedVariable.Label = variable.Name;
                        updatedVariable.Description = variable.Description;
                        updatedVariable.DataType = dataType;
                        updatedVariable.Unit = unit;
                        updatedVariable.DisplayPatternId = displayPattern;
                        updatedVariable.OrderNo = orderNo;
                    
                        updatedVariable.IsKey = variable.IsKey;
                        updatedVariable.IsValueOptional = variable.IsOptional;
                        updatedVariable.VariableConstraints = variableHelper.ConvertTo(variable.Constraints, constraintsManager);
                        updatedVariable.Meanings = variableHelper.ConvertTo(variable.Meanings, meaningManager);

                        // template
                        if (variable.Template != null) updatedVariable.VariableTemplate = variableManager.GetVariableTemplate(variable.Template.Id);
                        else updatedVariable.VariableTemplate = null;

                        //add update of missing values
                        updatedVariable.MissingValues = variableHelper.Update(variable.MissingValues, updatedVariable.MissingValues);

                        updatedVariable =  variableManager.UpdateVariable(updatedVariable);
                    }
                    else // create
                    {
                        long templateid = variable.Template != null ? variable.Template.Id : 0;

                        updatedVariable = variableManager.CreateVariable(
                            variable.Name,
                            dataType,
                            unit,
                            structure.Id,
                            variable.IsOptional,
                            variable.IsKey,
                            orderNo,
                            templateid,
                            variable.Description,
                            "",
                            "",
                            displayPattern,
                            variableHelper.ConvertTo(variable.MissingValues),
                            variable.Constraints.Select(co => co.Id).ToList(),
                            variable.Meanings.Select(co => co.Id).ToList()
                            );

                        variable.Id = updatedVariable.Id;

                        structure = structureManager.AddVariable(structure.Id, updatedVariable.Id);
                    }
                }

                // order vars based on orderNo
                structure.Variables.OrderBy(v => v.OrderNo);

                // compare all vars from model with from db
                // delete all not existing variables from db
                var varids = model.Variables.Select(v => v.Id);
                var removeVars = structure.Variables.Where(v => !varids.Contains(v.Id));
                //removeVars.ToList().ForEach(v => variableManager.DeleteVariable(v.Id));
                removeVars.ToList().ForEach(v => structure.Variables.Remove(v));

                structureManager.UpdateStructuredDataStructure(structure);
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        

        [JsonNetFilter]
        [HttpPost]
        public JsonResult Generate(DataStructureCreationModel model)
        {

            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Markers == null || !model.Markers.Any()) throw new ArgumentNullException(nameof(model));
            if (model.File == null) throw new ArgumentNullException(nameof(model.File));

            // get similarity Threshold from settings
            var settings = ModuleManager.GetModuleSettings("Rpm");
            double similarityThreshold = Convert.ToDouble(settings.GetValueByKey("similarityThreshold")) / 100;

            string path = "";
            if (model.EntityId == 0) path = Path.Combine(AppConfiguration.DataPath, "Temp", BExISAuthorizeHelper.GetAuthorizedUserName(this.HttpContext), model.File);
            else path = Path.Combine(AppConfiguration.DataPath, "Datasets", "" + model.EntityId, "temp", model.File);

            // get variable data
            // order rows by index
            model.Markers = model.Markers.OrderBy(m => m.Row).ToList();

            // get list of alle index for the rows
            List<int> rowIndexes = model.Markers.Select(m => m.Row).ToList();

            // get first cells array- alle should be the same, so first one is ok
            // if all cells are active, set it to null, because selection of rows is after
            List<bool> activeCells = model.Markers.FirstOrDefault().Cells.Contains(false) ? model.Markers.FirstOrDefault().Cells : null;

            // contains marker rows in order of model.Markers rows index
            List<string> markerRows = AsciiReader.GetRows(path, Encoding.UTF8, rowIndexes, activeCells, AsciiFileReaderInfo.GetSeperator("" + (char)model.Delimeter));

            // missing values
            List<string> missingValues = new List<string>();
            if (model.Markers.Any(m => m.Type.Equals("missing-values")))
            {
                int mvIndex = model.Markers.FindIndex(m => m.Type.Equals("missing-values"));
                missingValues = markerRows[mvIndex].Split((char)model.Delimeter).ToList();
            }

            //add missing values from model
            missingValues.AddRange(model.MissingValues.Select(m => m.DisplayName).ToList());

            int startdataIndex = 0;

            if (model.Markers.Any(m => m.Type.Equals("data")))
            {
                startdataIndex = model.Markers.Where(m => m.Type.Equals("data")).FirstOrDefault().Row;
            }

            // get DataTypes
            Dictionary<int, Type> systemTypes = suggestSystemTypes(
                path,
                AsciiFileReaderInfo.GetSeperator((char)model.Delimeter),
                AsciiFileReaderInfo.GetDecimalCharacter((char)model.Decimal),
                missingValues,
                startdataIndex + 1
                );

            // generate variables
            // reset list
            model.Variables = new List<VariableInstanceModel>();
            int cells = AsciiReader.CountCells(model.Preview.FirstOrDefault(), (char)model.Delimeter, (char)model.TextMarker);

            var strutcureAnalyzer = new StructureAnalyser();
            VariableHelper helper = new VariableHelper();

            using (var unitManager = new UnitManager())
            using (var dataTypeManager = new DataTypeManager())
            using (var variableManager = new VariableManager())
            {
                var allUnits = unitManager.Repo.Query().ToList();
                var allDataTypes = dataTypeManager.Repo.Query().ToList();
                var allTemplates = variableManager.VariableTemplateRepo.Query().ToList();

                for (int i = 0; i < cells; i++)
                {
                    if (activeCells == null || activeCells[i]) // only create a var to the model if the cell is active or the list is null - means add everyone
                    {
                        VariableInstanceModel var = new VariableInstanceModel();

                        var.Name = getValueFromMarkedRow(markerRows, model.Markers, "variable", (char)model.Delimeter, i, (char)model.TextMarker);
                        if (string.IsNullOrEmpty(var.Name)) throw new ArgumentException($"Variable name on (" + i + ") is empty");

                        var.Description = getValueFromMarkedRow(markerRows, model.Markers, "description", (char)model.Delimeter, i, (char)model.TextMarker);

                        // check and get datatype
                        if (systemTypes.ContainsKey(i))
                            var.SystemType = systemTypes[i].Name;

                        // get example value
                        var value = getValueFromMarkedRow(markerRows, model.Markers, "data", (char)model.Delimeter, i, (char)model.TextMarker);


                        // get list of possible data types
                        var.DataType = strutcureAnalyzer.SuggestDataType(var.SystemType, value, allDataTypes).Select(d => new ListItem(d.Id, d.Name, "detect")).FirstOrDefault();

                        // get list of possible units
                        var unitInput = getValueFromMarkedRow(markerRows, model.Markers, "unit", (char)model.Delimeter, i, (char)model.TextMarker);

                        // here we need 2 workflows
                        // 1. if unit is not empty -> start from unit
                        // 2. if unit is empty start from template

                        List<VariableTemplate> templates = new List<VariableTemplate>();

                        if (!string.IsNullOrEmpty(unitInput)) // has unit input
                        {
                            strutcureAnalyzer.SuggestUnit(unitInput, var.Name, var.DataType.Text, similarityThreshold, allUnits, allDataTypes).ForEach(u => var.PossibleUnits.Add(new UnitItem(u.Id, u.Abbreviation, u.AssociatedDataTypes.Select(x => x.Name).ToList(), "detect")));
                            var.Unit = var.PossibleUnits.FirstOrDefault();
                            if (var.Unit != null) templates = strutcureAnalyzer.SuggestTemplate(var.Name, var.Unit.Id, var.DataType.Id); // unit exist
                            else // unit not exist
                            {
                                templates = strutcureAnalyzer.SuggestTemplate(var.Name, 0, var.DataType.Id, similarityThreshold, allTemplates);
                                templates.Select(t => t.Unit).Distinct().ToList().ForEach(u => var.PossibleUnits.Add(new UnitItem(u.Id, u.Abbreviation, u.AssociatedDataTypes.Select(x => x.Name).ToList(), "detect")));
                                var.Unit = var.PossibleUnits.FirstOrDefault();
                            }
                        }
                        else // no unit input
                        {

                            templates = strutcureAnalyzer.SuggestTemplate(var.Name, 0, var.DataType.Id, similarityThreshold, allTemplates);
                            templates.Select(t => t.Unit).Distinct().ToList().ForEach(u => var.PossibleUnits.Add(new UnitItem(u.Id, u.Abbreviation, u.AssociatedDataTypes.Select(x => x.Name).ToList(), "detect")));
                            var.Unit = var.PossibleUnits.FirstOrDefault();
                        }

                        // fallback if unit is null
                        if (var.Unit == null) // if suggestion return null then set to unit none
                        {
                            strutcureAnalyzer.SuggestUnit("none", var.Name, var.DataType.Text, 1, allUnits, allDataTypes).ForEach(u => var.PossibleUnits.Add(new UnitItem(u.Id, u.Abbreviation, u.AssociatedDataTypes.Select(x => x.Name).ToList(), "detect")));
                            var.Unit = var.PossibleUnits.FirstOrDefault();
                        }

                        // get suggested DisplayPattern / currently only for DateTime
                        if (var.SystemType.Equals(typeof(DateTime).Name))
                        {
                            var displayPattern = DataTypeDisplayPattern.Pattern.Where(p => p.Systemtype.ToString().Equals(var.SystemType));

                            displayPattern.ToList().ForEach(d => var.PossibleDisplayPattern.Add(new ListItem(d.Id, d.DisplayPattern)));

                            // sugest displaypattern
                            //suggest display pattern
                            var suggestedDisplayPattern = strutcureAnalyzer.SuggestDisplayPattern(value);
                            // create listitem if display pattern exist
                            ListItem dsp = null;
                            if (suggestedDisplayPattern != null)
                            {
                                dsp = new ListItem(suggestedDisplayPattern.Id, suggestedDisplayPattern.DisplayPattern);
                            }
                            // set display pattern to variable
                            var.DisplayPattern = dsp;
                        }

                        // variable template
                        templates.ForEach(t => var.PossibleTemplates.Add(helper.ConvertTo(t, "detect")));

                        if (var.PossibleTemplates.Any())
                            var.Template = var.PossibleTemplates.FirstOrDefault();

                        // set meanings,constraints and description from template
                        if (var.Template?.Id == 0) var.Template = null;
                        if (var.Template != null)
                        {
                            var t = templates.Where(tx => tx.Id.Equals(var.Template.Id)).FirstOrDefault();
                            var.Meanings = helper.ConvertTo(t.Meanings);
                            var.Constraints = helper.ConvertTo(t.VariableConstraints);
                            if (string.IsNullOrEmpty(var.Description)) var.Description = t.Description;
                        }

                        // add missing values
                        model.MissingValues.ToList().ForEach(mv => var.MissingValues.Add(new MissingValueItem(0, mv.DisplayName, mv.Description)));

                        model.Variables.Add(var);
                    }
                }
            }


            // set name if dataset belongs to
            if (model.EntityId > 0)
            {
                using (var datasetManager = new DatasetManager())
                {
                    var v = datasetManager.GetDatasetLatestVersion(model.EntityId);
                    model.Title = v.Title;
                }
            }

            return Json(model);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult Empty(long entityId = 0)
        {
            DataStructureCreationModel model = new DataStructureCreationModel();
            model.EntityId = entityId;
            // get default missing values
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult Copy(long id)
        {
            if (id <= 0) throw new NullReferenceException("id of the structure should be greater then 0");
            DataStructureCreationModel model = new DataStructureCreationModel();
            VariableHelper helper = new VariableHelper();

            using (var structureManager = new DataStructureManager())
            {
                if (id > 0)
                {
                    var structure = structureManager.StructuredDataStructureRepo.Get(id);
                    if (structure == null) throw new NullReferenceException("structure with id " + id);

                    model.Title = structure.Name + " (copy)";
                    model.Description = structure.Description;

                    if (structure.Variables.Any())
                    {
                        foreach (var variable in structure.Variables)
                        {
                            model.Variables.Add(helper.Copy(variable));
                        }
                    }
                }
            }

            // get default missing values
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult Delete(long id)
        {
            if (id <= 0) throw new NullReferenceException("id of the data structure should be greater then 0");

            using (var structureManager = new DataStructureManager())
            using (var variableManager = new VariableManager())
            {
                if (id > 0)
                {
                    var structure = structureManager.StructuredDataStructureRepo.Get(id);
                    if (structure == null) throw new Exception("Data structure with id " + id + " does not exist.");

                    structureManager.DeleteStructuredDataStructure(structure);
                }
            }

            // get default missing values
            return Json(true);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult CheckPrimaryKeySet(long id, long[] primaryKeys)
        {
            if (id <= 0) throw new ArgumentNullException("id");
            if (primaryKeys == null) primaryKeys = new long[0];

            UploadHelper helper = new UploadHelper();

            using (var datasetManager = new DatasetManager())
            {
                List<long> datasetIds = datasetManager.DatasetRepo.Query(d => d.DataStructure.Id.Equals(id))?.Select(d => d.Id).ToList();

                foreach (long dsid in datasetIds)
                {
                    if (!helper.IsUnique2(dsid, primaryKeys.ToList()))
                        return Json(false, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [JsonNetFilter]
        public JsonResult Get(long id)
        {
            if (id <= 0) throw new NullReferenceException("Id of the data structure should be greater then 0");
            DataStructureEditModel model = new DataStructureEditModel();
            VariableHelper helper = new VariableHelper();

            using (var structureManager = new DataStructureManager())
            {
                if (id > 0)
                {
                    var structure = structureManager.StructuredDataStructureRepo.Get(id);
                    if (structure == null) throw new NullReferenceException("Data structure with id " + id);
                    model.Id = structure.Id;
                    model.Title = structure.Name;
                    model.Description = structure.Description;

                    if (structure.Variables.Any())
                    {
                        foreach (var variable in structure.Variables)
                        {
                            model.Variables.Add(helper.ConvertTo(variable));
                        }
                    }
                }
            }

            // get default missing values
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        public JsonResult GetDataTypes()
        {
            using (var dataTypeManager = new DataTypeManager())
            {
                var datatypes = dataTypeManager.Repo.Get().ToList();
                List<ListItem> list = new List<ListItem>();

                if (datatypes.Any())
                {
                    foreach (var datatype in datatypes)
                    {
                        list.Add(new ListItem(datatype.Id, datatype.Name, "other"));
                    }
                }

                // get default missing values
                return Json(list.OrderBy(l => l.Text), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        public JsonResult GetStructures()
        {
            using (var structureManager = new DataStructureManager())
            {
                List<ListItem> list = new List<ListItem>();
                var structures = structureManager.GetStructuredDataStructuresAsKVP();

                if (structures.Any())
                {
                    foreach (var structure in structures)
                    {
                        list.Add(new ListItem(structure.Key, structure.Value));
                    }
                }

                // get default missing values
                return Json(list.OrderBy(l => l.Text), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        public JsonResult GetDisplayPattern()
        {
            List<ListItem> list = new List<ListItem>();
            foreach (var displayPattern in DataTypeDisplayPattern.Pattern)
            {
                list.Add(new ListItem()
                {
                    Id = displayPattern.Id,
                    Text = displayPattern.DisplayPattern,
                    Group = displayPattern.Systemtype.ToString()
                });
            }

            // get list of all display pattern
            return Json(list.OrderBy(l => l.Text), JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        public JsonResult GetUnits()
        {
            using (var unitManager = new UnitManager())
            {
                var units = unitManager.Repo.Get().ToList();
                List<UnitItem> list = new List<UnitItem>();

                if (units.Any())
                {
                    foreach (var unit in units)
                    {
                        list.Add(new UnitItem(
                            unit.Id,
                            unit.Abbreviation,
                            unit.AssociatedDataTypes.Select(x => x.Name).ToList(),
                            "other"
                            ));
                    }
                }

                // get default missing values
                return Json(list.OrderBy(l => l.Text), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        public JsonResult GetVariableTemplates()
        {
            using (var variableManager = new VariableManager())
            using (var unitManager = new UnitManager())
            {
                var _helper = new VariableHelper();
                var variableTemplates = variableManager.VariableTemplateRepo.Get().ToList();
                var units = unitManager.Repo.Get();
                List<VariableTemplateItem> list = new List<VariableTemplateItem>();

                if (variableTemplates.Any())
                {
                    foreach (var variableTemplate in variableTemplates.Where(t => t.Approved))
                    {
                        list.Add(_helper.ConvertTo(variableTemplate, "other"));
                    }
                }

                // get default missing values
                return Json(list.OrderBy(l => l.Text), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        public JsonResult GetMeanings()
        {
            VariableHelper helper = new VariableHelper();
            List<MeaningItem> list = helper.GetMeanings();

            // get default missing values
            return Json(list.OrderBy(l => l.Text), JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        public JsonResult GetConstraints()
        {
            VariableHelper helper = new VariableHelper();
            List<ListItem> list = helper.GetConstraints();

            return Json(list.OrderBy(l => l.Text), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// suggestDataTypes datatypes based on incoming file and start data row (not index)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="delimeter"></param>
        /// <param name="decimalCharacter"></param>
        /// <param name="missingValues"></param>
        /// <param name="datastart">row not index!</param>
        /// <returns></returns>
        private Dictionary<int, Type> suggestSystemTypes(string file, TextSeperator delimeter, DecimalCharacter decimalCharacter, List<string> missingValues, int datastart)
        {
            var settings = ModuleManager.GetModuleSettings("Rpm");
            int min = Convert.ToInt32(settings.GetValueByKey("minToAnalyse"));
            int max = Convert.ToInt32(settings.GetValueByKey("maxToAnalyse"));
            int percentage = Convert.ToInt32(settings.GetValueByKey("precentageToAnalyse"));

            StructureAnalyser structureAnalyser = new StructureAnalyser();

            long total = AsciiReader.Count(file);
            long skipped = AsciiReader.Skipped(file);

            // rows only with data
            var dataTotal = total - skipped - (datastart - 1);

            long selection = structureAnalyser.GetNumberOfRowsToAnalyse(min, max, percentage, dataTotal);

            List<string> rows = AsciiReader.GetRandowRows(file, total, selection, datastart);

            return structureAnalyser.SuggestSystemTypes(rows, delimeter, decimalCharacter, missingValues);
        }

        private string getValueFromMarkedRow(List<string> rows, List<Marker> markers, string type, char delimeter, int position, char textMarker)
        {
            /**here it is assumed that the index of the line and the index of the maker match.
                0 = variable marker & variable data
                1 = descrption marker & descrption data 
                2 = unit marker & unit data
                *****
            */
            var marker = markers.FirstOrDefault(m => m.Type.Equals(type));
            int markerIndex = marker != null ? markers.IndexOf(marker) : -1;

            if (markerIndex > -1)
            {
                var row = rows[markerIndex];
                var v = AsciiReader.GetCells(row,delimeter,textMarker)[position]; // get value
                //if text marker char is in the value, remove it
                if (v.Contains(textMarker)) v = v.Trim(textMarker);

                return v;
            }

            return "";
        }



        public string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        private string getFileName(HttpPostedFileBase file)
        {
            // Checking for Internet Explorer
            if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
            {
                string[] testfiles = file.FileName.Split(new char[] { '\\' });
                return testfiles[testfiles.Length - 1];
            }
            else
            {
                return file.FileName;
            }
        }

        public FileResult downloadTemplate(long id)
        {
            DataStructureHelper _h = new DataStructureHelper();
            _h.ConvertExcelToCsv();

            if (id != 0)
            {
                DataStructureManager dataStructureManager = null;
                try
                {
                    dataStructureManager = new DataStructureManager();

                    StructuredDataStructure dataStructure = new StructuredDataStructure();
                    dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);

                    if (dataStructure != null)
                    {
                        ExcelTemplateProvider provider = new ExcelTemplateProvider("BExISppTemplate_Clean.xlsx");

                        string path = Path.Combine(AppConfiguration.DataPath, provider.CreateTemplate(dataStructure));
                        return File(path, MimeMapping.GetMimeMapping(path), Path.GetFileName(path));
                    }
                }
                finally
                {
                    dataStructureManager.Dispose();
                }
            }
            return File(Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Template", "BExISppTemplate_Clean.xlsx"), "application/xlsm", "Template_" + id + "_No_Data_Structure.xlsx");
        }
    }
}