using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
using BExIS.IO;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.IO.Transform.Input;
using BExIS.Modules.Rpm.UI.Helpers;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Modules.Rpm.UI.Models.DataStructure;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Helpers;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Hooks.Logs;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;
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

                foreach (var entity in dataStructureManger.StructuredDataStructureRepo.Get())
                {
                    List<long> linked = datasetManager.DatasetRepo.Query().Where(d => (d.DataStructure != null && d.DataStructure.Id.Equals(entity.Id))).Select(d => d.Id).ToList();

                    tmp.Add(new DataStructureModel()
                    {
                        Id = entity.Id,
                        Description = entity.Description,
                        Title = entity.Name,
                        LinkedTo = linked,
                        
                    });
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
            return View("Create");
        }

   


        [JsonNetFilter]
        public JsonResult Load(string file, long entityId = 0, int version = 0)
        {
            EditDatasetDetailsCache cache = null;

            // there are 2 usecases
            // 1. From edit dataset
            // 2. From Data structure
            // in usecase 1 a entity exist and the file to read is find albe in AppConfiguration.DataPath, "Datasets", entityId.ToString(), "temp", file
            // in usecase 2 not entity exist and the fils is findable under user temp
            var filepath = "";
            if (entityId == 0) filepath = Path.Combine(AppConfiguration.DataPath, "Temp", BExISAuthorizeHelper.GetAuthorizedUserName(this.HttpContext), file);
            else
            {
                // if filereader info allready exist, load the data from the cache otherwise, suggest it
                HookManager hookManager = new HookManager();
                cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, entityId);

                // file can be incoming or set from editcache
                if (string.IsNullOrEmpty(file)) // incoming file ist not set
                {
                    if (cache.Files != null && cache.Files.Any()) // files added to the files list allready, 
                    {
                        // use the first one
                        file = cache.Files.First().Name;
                    }
                }

                filepath = Path.Combine(AppConfiguration.DataPath, "Datasets", entityId.ToString(), "temp", file);
            }

            if (!FileHelper.FileExist(filepath)) throw new FileNotFoundException(nameof(filepath));

            DataStructureCreationModel model = new DataStructureCreationModel();
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            model.EntityId = entityId;
            model.File = file;

            // get first rows
            model.Preview = AsciiReader.GetRows(filepath, 10);
            model.Total = AsciiReader.Count(filepath);
            model.Skipped = AsciiReader.Skipped(filepath);

       
                if (cache==null || cache.AsciiFileReaderInfo == null) // file reader infos not exit, suggest it
                {
                    if (model.Preview.Any())
                    {
                        // get delimeter
                        TextSeperator textSeperator = structureAnalyser.SuggestDelimeter(model.Preview.First(), model.Preview.Last());
                        model.Delimeter = AsciiFileReaderInfo.GetSeperator(textSeperator);

                        // get decimal
                        // the structure analyzer return a result or trigger a exception
                        // catch the exception and set a default value -1 
                        try
                        {
                            DecimalCharacter decimalCharacter = structureAnalyser.SuggestDecimal(model.Preview.First(), model.Preview.Last(), textSeperator);
                            model.Decimal = AsciiFileReaderInfo.GetDecimalCharacter(decimalCharacter);
                        }
                        catch (Exception ex)
                        {
                            model.Decimal = -1;
                        }

                        // get textmarkers
                        TextMarker textMarker = structureAnalyser.SuggestTextMarker(model.Preview.First(), model.Preview.Last());
                        model.TextMarker = AsciiFileReaderInfo.GetTextMarker(textMarker);

                    }

                }
                else // allready exist, set it
                {

                    model.Decimal = (int)cache.AsciiFileReaderInfo.Decimal;
                    model.Delimeter = (int)cache.AsciiFileReaderInfo.Seperator;
                    model.TextMarker = (int)cache.AsciiFileReaderInfo.TextMarker;

                    // variables
                    model.Markers.Add(
                        new Marker()
                        {
                            Row = cache.AsciiFileReaderInfo.Variables,
                            Type = "variable",
                            Cells = cache.AsciiFileReaderInfo.Cells

                        });

                    // Data
                    model.Markers.Add(
                        new Marker()
                        {
                            Row = cache.AsciiFileReaderInfo.Data,
                            Type = "data",
                            Cells = cache.AsciiFileReaderInfo.Cells
                        });

                    // Unit
                    model.Markers.Add(
                        new Marker()
                        {
                            Row = cache.AsciiFileReaderInfo.Unit,
                            Type = "unit",
                            Cells = cache.AsciiFileReaderInfo.Cells
                        });

                    //description
                    model.Markers.Add(
                        new Marker()
                        {
                            Row = cache.AsciiFileReaderInfo.Description,
                            Type = "description",
                            Cells = cache.AsciiFileReaderInfo.Cells
                        });

                }
            
            // get lists
            model.Decimals = getDecimals();
            model.Delimeters = getDelimeters();
            model.TextMarkers = getTextMarkers();
            model.MissingValues = getDefaultMissingValues();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        public JsonResult Save(DataStructureCreationModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            using (var structureManager = new DataStructureManager())
            using (var variableManager = new VariableManager())
            using (var unitManager = new UnitManager())
            using (var datatypeManager = new DataTypeManager())
            using (var datasetManager = new DatasetManager())
            using (var missingValueManager = new MissingValueManager())
            {
                // create strutcure
                StructuredDataStructure newStructure = structureManager.CreateStructuredDataStructure(
                        model.Title,
                        model.Description,
                        null,
                        null,
                        DataStructureCategory.Generic
                    );


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
                    int orderNo = model.Variables.IndexOf(variable)+1;

                    // generate variables
                    var result = variableManager.CreateVariable(
                        variable.Name,
                        dataType,
                        unit,
                        newStructure.Id,
                        variable.IsOptional,
                        variable.IsKey,
                        orderNo,
                        0,
                        variable.Description,
                        "",
                        displayPattern
                        );


                    // gerenate missing values and link them to each variable
                    foreach (var mv in model.MissingValues)
                    {
                        if (!string.IsNullOrEmpty(mv.DisplayName))
                        {
                            var missingValue = missingValueManager.Create(mv.DisplayName, mv.Description, result);
                        }
                    }

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
        public JsonResult Store(DataStructureCreationModel model)
        {
            #region update cache

            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, model.EntityId);
            EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, model.EntityId);

            var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

            // update Data description hook

            // set file reading informations
            if (cache.AsciiFileReaderInfo == null)
                cache.AsciiFileReaderInfo = new AsciiFileReaderInfo();

            cache.AsciiFileReaderInfo.Decimal = (DecimalCharacter)model.Decimal;
            cache.AsciiFileReaderInfo.Seperator = (TextSeperator)model.Delimeter;
            cache.AsciiFileReaderInfo.TextMarker = (TextMarker)model.TextMarker;
            cache.AsciiFileReaderInfo.Data = model.Markers.Where(m => m.Type.Equals("data")).FirstOrDefault().Row + 1; // add 1 to store not the index but the row
            cache.AsciiFileReaderInfo.Variables = model.Markers.Where(m => m.Type.Equals("variable")).FirstOrDefault().Row + 1;// add 1 to store not the index but the row
            cache.AsciiFileReaderInfo.Cells = model.Markers.Where(m => m.Type.Equals("variable")).FirstOrDefault().Cells;

            // additional infotmations
            // description
            var descriptionMarker = model.Markers.Where(m => m.Type.Equals("description")).FirstOrDefault();
            if(descriptionMarker != null) cache.AsciiFileReaderInfo.Description = descriptionMarker.Row + 1;// add 1 to store nit the index but the row
            // units
            var unitMarker = model.Markers.Where(m => m.Type.Equals("unit")).FirstOrDefault();
            if (unitMarker != null) cache.AsciiFileReaderInfo.Unit = unitMarker.Row + 1;// add 1 to store nit the index but the row

            // update modifikation date
            //cache.UpdateLastModificarion(typeof(DataDescriptionHook));


            // store in messages
            string message = String.Format("the structure {0} was successfully created and attached to the dataset {1}.", model.Title, model.EntityId);
            log.Messages.Add(new LogMessage(DateTime.Now, new List<string>() { message }, username,"Structure suggestion","store"));

            // save cache
            hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, model.EntityId);
            #endregion


            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult Generate(DataStructureCreationModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Markers == null || !model.Markers.Any()) throw new ArgumentNullException(nameof(model));
            if (model.File == null) throw new ArgumentNullException(nameof(model.File));

            string path = "";
            if (model.EntityId==0 ) path = Path.Combine(AppConfiguration.DataPath, "Temp", BExISAuthorizeHelper.GetAuthorizedUserName(this.HttpContext), model.File);
            else path = Path.Combine(AppConfiguration.DataPath, "Datasets", "" + model.EntityId, "temp", model.File);

            // get variable data
            // order rows by index
            model.Markers = model.Markers.OrderBy(m => m.Row).ToList();

            // get list of alle index for the rows
            List<int> rowIndexes = model.Markers.Select(m => m.Row).ToList();

            // get first cells array- alle should be the same, so first one is ok
            // if all cells are active, set it to null, because selection of rows is after
            List<bool> activeCells = model.Markers.FirstOrDefault().Cells.Contains(false)?model.Markers.FirstOrDefault().Cells:null;

            // contains marker rows in order of model.Markers rows index
            List<string> markerRows = AsciiReader.GetRows(path, rowIndexes, activeCells,AsciiFileReaderInfo.GetSeperator(""+(char)model.Delimeter));

            // missingvalues
            List<string> missingValues = new List<string>();
            if (model.Markers.Any(m => m.Type.Equals("missing-values")))
            {
                int mvIndex = model.Markers.FindIndex(m => m.Type.Equals("missing-values"));
                missingValues = markerRows[mvIndex].Split((char)model.Delimeter).ToList();
            }

            //add missing values from model
            missingValues.AddRange(model.MissingValues.Select(m=>m.DisplayName).ToList());

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
                startdataIndex+1
                );


            // generate variables
            // reset list
            model.Variables = new List<VariableInstanceModel>();
            int cells = markerRows.First().Split((char)model.Delimeter).Count();

            var strutcureAnalyzer = new StructureAnalyser();

            for (int i = 0; i < cells; i++)
            {
                if (activeCells == null || activeCells[i]) // only create a var to the model if the cell is active or the list is null - means add everyone
                {

                    VariableInstanceModel var = new VariableInstanceModel();

                    var.Name = getValueFromMarkedRow(markerRows, model.Markers, "variable", (char)model.Delimeter, i, AsciiFileReaderInfo.GetTextMarker((TextMarker)model.TextMarker));
                    var.Description = getValueFromMarkedRow(markerRows, model.Markers, "description", (char)model.Delimeter, i, AsciiFileReaderInfo.GetTextMarker((TextMarker)model.TextMarker));


                    // check and get datatype
                    if (systemTypes.ContainsKey(i))
                        var.SystemType = systemTypes[i].Name;

                    // get list of possible datatypes
                    var.DataType = strutcureAnalyzer.SuggestDataType(var.SystemType).Select(d => new ListItem(d.Id, d.Name, "detect")).FirstOrDefault();

                    // get list of possible units
                    var unitInput = getValueFromMarkedRow(markerRows, model.Markers, "unit", (char)model.Delimeter, i, AsciiFileReaderInfo.GetTextMarker((TextMarker)model.TextMarker));
                    strutcureAnalyzer.SuggestUnit(unitInput, var.DataType.Text).ForEach(u => var.PossibleUnits.Add(new UnitItem(u.Id, u.Name,u.AssociatedDataTypes.Select(x => x.Name).ToList(), "detect")));
                    var.Unit = var.PossibleUnits.FirstOrDefault();


                    // get suggestes DisplayPattern / currently only for DateTime
                    if (var.SystemType.Equals(typeof(DateTime).Name))
                    {
                        var.DisplayPattern = null; // here a suggesten of the display pattern is needed
                        var displayPattern = DataTypeDisplayPattern.Pattern.Where(p => p.Systemtype.ToString().Equals(var.SystemType));
                        displayPattern.ToList().ForEach(d => var.PossibleDisplayPattern.Add(new ListItem(d.Id, d.DisplayPattern)));
                    }

                    // varaible template
                    var templates = strutcureAnalyzer.SuggestTemplate(var.Name, var.Unit.Id, var.DataType.Id, 0.5);
                        templates.ForEach(t => var.PossibleTemplates.Add(new VariableTemplateItem(t.Id, t.Label,new List<string>() {t.Unit.Name}, t.Unit.AssociatedDataTypes.Select(x => x.Name).ToList(),t.Meanings.Select(x => x.Name).ToList(),null, "detect")));

                    if (var.PossibleTemplates.Any())
                        var.Template = var.PossibleTemplates.FirstOrDefault();

                    model.Variables.Add(var);
                }
            }

            // get default missing values
            return Json(model);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult Empty()
        {
            DataStructureCreationModel model = new DataStructureCreationModel();

            // get default missing values
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult Copy(long id)
        {
            if (id <= 0) throw new NullReferenceException("id of the structure should be greater then 0");
            DataStructureCreationModel model = new DataStructureCreationModel();

            using (var structureManager = new DataStructureManager())
            {
                if (id > 0)
                {
                    var structure = structureManager.StructuredDataStructureRepo.Get(id);
                    if (structure == null) throw new NullReferenceException("structure with id " + id);

                    model.Title = structure.Name +" (copy)";
                    model.Description = structure.Description;

                    if (structure.Variables.Any())
                    {
                        foreach (var variable in structure.Variables)
                        {
                            var var = new VariableInstanceModel()
                            {
                                Id = variable.Id,
                                Name = variable.Label,
                                Description = variable.Description,
                                DataType = new ListItem(variable.DataType.Id, variable.DataType.Name, "copied"),
                                SystemType = variable.DataType.SystemType,
                                Unit = new UnitItem(variable.Unit.Id, variable.Unit.Abbreviation, variable.Unit.AssociatedDataTypes.Select(x => x.Name).ToList(),"copied"),
                                IsKey = variable.IsKey,
                                IsOptional = variable.IsValueOptional

                            };

                            // get suggestes DisplayPattern / currently only for DateTime
                            if (var.SystemType.Equals(typeof(DateTime).Name))
                            {
                                var.DisplayPattern = null; // here a suggesten of the display pattern is needed
                                var displayPattern = DataTypeDisplayPattern.Pattern.Where(p => p.Systemtype.ToString().Equals(var.SystemType));
                                displayPattern.ToList().ForEach(d => var.PossibleDisplayPattern.Add(new ListItem(d.Id, d.DisplayPattern)));
                            };

                            model.Variables.Add(var);
                            
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
            if (id <= 0) throw new NullReferenceException("id of the structure should be greater then 0");


            using (var structureManager = new DataStructureManager())
            {
                if (id > 0)
                {
                    var structure = structureManager.StructuredDataStructureRepo.Get(id);
                    if (structure == null) throw new Exception("Structure with id " + id + " not exist.");
                        
                    structureManager.DeleteStructuredDataStructure(structure);
                }

            }

            // get default missing values
         
            return Json(true);
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
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        public JsonResult GetStructures()
        {
            using (var structureManager = new DataStructureManager())
            {
                List<ListItem> list = new List<ListItem>();
                var structures = structureManager.StructuredDataStructureRepo.Get();

                if (structures.Any())
                {
                    foreach (var structure in structures)
                    {
                        list.Add(new ListItem(structure.Id, structure.Name));
                    }
                }

                // get default missing values
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        public JsonResult GetDisplayPattern()
        {
            List<ListItem> list = new List<ListItem>();
            foreach(var displayPattern in DataTypeDisplayPattern.Pattern)
            {
                list.Add(new ListItem()
                {
                    Id = displayPattern.Id,
                    Text = displayPattern.DisplayPattern,
                    Group = displayPattern.Systemtype.ToString()
                });
            }

            // get list of all display pattern
            return Json(list, JsonRequestBehavior.AllowGet);
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
                return Json(list.OrderBy(i => i.Group), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        public JsonResult GetVariableTemplates()
        {
            using (var variableManager = new VariableManager())
            using (var unitManager = new UnitManager())
            {
                var variableTemplates = variableManager.VariableTemplateRepo.Get().ToList();
                var units = unitManager.Repo.Get();
                List<VariableTemplateItem> list = new List<VariableTemplateItem>();

                if (variableTemplates.Any())
                {
                    foreach (var variableTemplate in variableTemplates)
                    {
                        List<string> dataTypes = new List<string>();
               
                        var unit = units.FirstOrDefault(u=> u.Id.Equals(variableTemplate.Unit.Id));

                        // get possible datatypes
                        if (unit != null)
                            dataTypes = unit.AssociatedDataTypes.Select(x => x.Name).ToList();

                        VariableTemplateItem vti = new VariableTemplateItem();
                        vti.Id = variableTemplate.Id;
                        vti.Text = variableTemplate.Label;
                        if (unit!=null) vti.Units = new List<string>() { unit.Abbreviation };
                        vti.DataTypes = dataTypes;

                        // meanings
                        vti.Meanings = variableTemplate.Meanings.ToList().Select(m=>m.Name).ToList();

                        vti.Group = "other";
                        list.Add(vti);
                    }
                }

                // get default missing values
                return Json(list.OrderBy(i => i.Group), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        public JsonResult GetMeanings()
        {
            VariableHelper helper = new VariableHelper();
            List<MeaningItem> list = helper.GetMeanings();

            // get default missing values
            return Json(list.OrderBy(i => i.Group), JsonRequestBehavior.AllowGet);

        }

        private List<ListItem> getDelimeters()
        {
            List<ListItem> list = new List<ListItem>();

            // tab
            char c = AsciiFileReaderInfo.GetSeperator(TextSeperator.tab);
            ListItem kvP = new ListItem();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetSeperatorAsString(TextSeperator.tab);

            list.Add(kvP);

            //comma

            c = AsciiFileReaderInfo.GetSeperator(TextSeperator.comma);
            kvP = new ListItem();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetSeperatorAsString(TextSeperator.comma);

            list.Add(kvP);

            // semicolon

            c = AsciiFileReaderInfo.GetSeperator(TextSeperator.semicolon);
            kvP = new ListItem();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetSeperatorAsString(TextSeperator.semicolon);

            list.Add(kvP);

            // space
            c = AsciiFileReaderInfo.GetSeperator(TextSeperator.space);
            kvP = new ListItem();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetSeperatorAsString(TextSeperator.space);

            list.Add(kvP);


            return list;
        }

        private List<ListItem> getDecimals()
        {

            List<ListItem> list = new List<ListItem>();

            // point
            char c = AsciiFileReaderInfo.GetDecimalCharacter(DecimalCharacter.point);
            ListItem kvP = new ListItem();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetDecimal(DecimalCharacter.point);

            list.Add(kvP);

            // comma
            c = AsciiFileReaderInfo.GetDecimalCharacter(DecimalCharacter.comma);
            kvP = new ListItem();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetDecimal(DecimalCharacter.comma);

            list.Add(kvP);

            return list;
        }

        private List<ListItem> getTextMarkers()
        {
            List<ListItem> list = new List<ListItem>();

            // quotes
            char c = AsciiFileReaderInfo.GetTextMarker(TextMarker.quotes);
            ListItem kvP = new ListItem();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetTextMarkerAsString(TextMarker.quotes);

            list.Add(kvP);

            // double quotes
            c = AsciiFileReaderInfo.GetTextMarker(TextMarker.doubleQuotes);
            kvP = new ListItem();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetTextMarkerAsString(TextMarker.doubleQuotes);

            list.Add(kvP);


            return list;
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
        private Dictionary<int,Type> suggestSystemTypes(string file, TextSeperator delimeter, DecimalCharacter decimalCharacter, List<string> missingValues,int datastart)
        {
            var settings = ModuleManager.GetModuleSettings("Dcm");
            int min = Convert.ToInt32(settings.GetValueByKey("minToAnalyse"));
            int max = Convert.ToInt32(settings.GetValueByKey("maxToAnalyse"));
            int percentage = Convert.ToInt32(settings.GetValueByKey("precentageToAnalyse"));

            StructureAnalyser structureAnalyser = new StructureAnalyser();

            long total = AsciiReader.Count(file);
            long skipped = AsciiReader.Skipped(file);

            // rows only with data
            var dataTotal = total - skipped - (datastart-1);

            long selection = structureAnalyser.GetNumberOfRowsToAnalyse(min, max, percentage, dataTotal);

            List<string> rows = AsciiReader.GetRandowRows(file, total, selection, datastart);

            return structureAnalyser.SuggestSystemTypes(rows, delimeter, decimalCharacter, missingValues);
        }

        private string getValueFromMarkedRow(List<string> rows, List<Marker> markers, string type, char delimeter, int position, char textMarker)
        {
   
            var marker = markers.FirstOrDefault(m => m.Type.Equals(type));
            int markerIndex = marker != null?marker.Row:-1;

            if (markerIndex > -1)
            {
                var v = rows[markerIndex].Split(delimeter)[position]; // get value
                //if text marker char is in the value, remove it
                if (v.Contains(textMarker)) v = v.Trim(textMarker);

                return v;
            }

            return "";
        }

        private List<MissingValueModel> getDefaultMissingValues()
        {
            var list = new List<MissingValueModel>();
            // get default missing values
            var settings = ModuleManager.GetModuleSettings("rpm");
            var mv_list = settings.GetValueByKey("missingValues") as List<Vaiona.Utils.Cfg.Entry>;

            if (mv_list != null)
            {
                foreach (var mv in mv_list)
                {
                    list.Add(new MissingValueModel()
                    {
                        DisplayName = mv.Value.ToString(),
                        Description = mv.Description.ToString()
                    });
                }
            }
            else // create a empty list entry
            {
                list.Add(new MissingValueModel());
            }

            list.Add(new MissingValueModel());


            return list;
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
        }
}