using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.IO.Transform.Input;
using BExIS.Modules.Dcm.UI.Hooks;
using BExIS.Modules.Dcm.UI.Models.StructureSuggestion;
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

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class StructureSuggestionController : Controller
    {
        // GET: Edit
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        public ActionResult Index(long id, string file, int version = 0)
        {
            string module = "dcm";

            ViewData["id"] = id;
            ViewData["version"] = version;
            ViewData["file"] = file;
            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        [JsonNetFilter]
        public JsonResult Load(long id, string file, int version = 0)
        {
            // check incoming values
            if (id <= 0) throw new ArgumentException(nameof(id));

            // if filereader info allready exist, load the data from the cache otherwise, suggest it
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);

            // file can be incoming or set from editcache
            if (string.IsNullOrEmpty(file)) // incoming file ist not set
            {
                if (cache.Files != null && cache.Files.Any()) // files added to the files list allready, 
                {
                    // use the first one
                    file = cache.Files.First().Name;
                }
            }

            //checi if file exist
            var filepath = Path.Combine(AppConfiguration.DataPath, "Datasets", id.ToString(), "temp", file);
            if (!FileHelper.FileExist(filepath)) throw new FileNotFoundException(nameof(filepath));

            StructureSuggestionModel model = new StructureSuggestionModel();
            StructureAnalyser structureAnalyser = new StructureAnalyser();

            model.Id = id;
            model.File = file;

            // get first rows
            model.Preview = AsciiReader.GetRows(filepath, 10);
            model.Total = AsciiReader.Count(filepath);
            model.Skipped = AsciiReader.Skipped(filepath);


            if (cache.AsciiFileReaderInfo == null) // file reader infos not exit, suggest it
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
        public JsonResult Save(StructureSuggestionModel model)
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
                        "",
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

                // store link to entity

                var dataset = datasetManager.GetDataset(model.Id);
                dataset.DataStructure = newStructure;
                datasetManager.UpdateDataset(dataset);

                

            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        public JsonResult Store(StructureSuggestionModel model)
        {
            #region update cache

            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, model.Id);
            EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, model.Id);

            var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

            // update Data description hook

            // set file reading informations
            if (cache.AsciiFileReaderInfo == null)
                cache.AsciiFileReaderInfo = new AsciiFileReaderInfo();

            cache.AsciiFileReaderInfo.Decimal = (DecimalCharacter)model.Decimal;
            cache.AsciiFileReaderInfo.Seperator = (TextSeperator)model.Delimeter;
            cache.AsciiFileReaderInfo.TextMarker = (TextMarker)model.TextMarker;
            cache.AsciiFileReaderInfo.Data = model.Markers.Where(m => m.Type.Equals("data")).FirstOrDefault().Row + 1; // add 1 to store nit the index but the row
            cache.AsciiFileReaderInfo.Variables = model.Markers.Where(m => m.Type.Equals("variable")).FirstOrDefault().Row + 1;// add 1 to store nit the index but the row
            cache.AsciiFileReaderInfo.Cells = model.Markers.Where(m => m.Type.Equals("variable")).FirstOrDefault().Cells;

            // additional infotmations
            // description
            var descriptionMarker = model.Markers.Where(m => m.Type.Equals("description")).FirstOrDefault();
            if(descriptionMarker != null) cache.AsciiFileReaderInfo.Description = descriptionMarker.Row + 1;// add 1 to store nit the index but the row
            // units
            var unitMarker = model.Markers.Where(m => m.Type.Equals("unit")).FirstOrDefault();
            if (unitMarker != null) cache.AsciiFileReaderInfo.Unit = unitMarker.Row + 1;// add 1 to store nit the index but the row

            // update modifikation date
            cache.UpdateLastModificarion(typeof(DataDescriptionHook));


            // store in messages
            string message = String.Format("the structure {0} was successfully created and attached to the dataset {1}.", model.Title, model.Id);
            log.Messages.Add(new LogMessage(DateTime.Now, new List<string>() { message }, username,"Structure suggestion","store"));

            // save cache
            hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, model.Id);
            #endregion


            return Json(true, JsonRequestBehavior.AllowGet);
        }

        /*
             const MARKER_TYPE = {
                VARIABLE: "variable",
                DESCRIPTION: "description",
                UNIT: "unit",
                MISSING_VALUES:  "missing-values"
            }
             */
        [JsonNetFilter]
        [HttpPost]
        public JsonResult Generate(StructureSuggestionModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Markers == null || !model.Markers.Any()) throw new ArgumentNullException(nameof(model));
            if (model.File == null) throw new ArgumentNullException(nameof(model.File));

            string path = Path.Combine(AppConfiguration.DataPath, "Datasets", "" + model.Id, "temp", model.File);

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
            model.Variables = new List<VariableModel>();
            int cells = markerRows.First().Split((char)model.Delimeter).Count();

            var strutcureAnalyzer = new StructureAnalyser();

            for (int i = 0; i < cells; i++)
            {
                if (activeCells == null || activeCells[i]) // only create a var to the model if the cell is active or the list is null - means add everyone
                {

                    VariableModel var = new VariableModel();

                    var.Name = getValueFromMarkedRow(markerRows, model.Markers, "variable", (char)model.Delimeter, i, AsciiFileReaderInfo.GetTextMarker((TextMarker)model.TextMarker));
                    var.Description = getValueFromMarkedRow(markerRows, model.Markers, "description", (char)model.Delimeter, i, AsciiFileReaderInfo.GetTextMarker((TextMarker)model.TextMarker));


                    // check and get datatype
                    if (systemTypes.ContainsKey(i))
                        var.SystemType = systemTypes[i].Name;

                    // get list of possible datatypes
                    var.DataType = strutcureAnalyzer.SuggestDataType(var.SystemType).Select(d => new ListItem(d.Id, d.Name, "analysis results")).FirstOrDefault();

                    // get list of possible units
                    var unitInput = getValueFromMarkedRow(markerRows, model.Markers, "unit", (char)model.Delimeter, i, AsciiFileReaderInfo.GetTextMarker((TextMarker)model.TextMarker));
                    strutcureAnalyzer.SuggestUnit(unitInput, var.DataType.Text).ForEach(u => var.PossibleUnits.Add(new ListItem(u.Id, u.Name, "analysis results")));
                    var.Unit = var.PossibleUnits.FirstOrDefault();


                    // get suggestes DisplayPattern / currently only for DateTime
                    if (var.SystemType.Equals(typeof(DateTime).Name))
                    {
                        var.DisplayPattern = null; // here a suggesten of the display pattern is needed
                        var displayPattern = DataTypeDisplayPattern.Pattern.Where(p => p.Systemtype.ToString().Equals(var.SystemType));
                        displayPattern.ToList().ForEach(d => var.PossibleDisplayPattern.Add(new ListItem(d.Id, d.DisplayPattern)));
                    }


                    model.Variables.Add(var);
                }
            }

            // get default missing values
            return Json(model);
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
                List<ListItem> list = new List<ListItem>();

                if (units.Any())
                {
                    foreach (var unit in units)
                    {
                        list.Add(new ListItem(unit.Id, unit.Abbreviation, "other"));
                    }
                }

                // get default missing values
                return Json(list.OrderBy(i=>i.Group), JsonRequestBehavior.AllowGet);
            }
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
            int min = Convert.ToInt32(settings.GetEntryValue("minToAnalyse"));
            int max = Convert.ToInt32(settings.GetEntryValue("maxToAnalyse"));
            int percentage = Convert.ToInt32(settings.GetEntryValue("precentageToAnalyse"));

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
            // check and get description
            int markerIndex = markers.FindIndex(m => m.Type.Equals(type));
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
            var mv_list = settings.GetList("missingvalues");

            if (mv_list != null)
            {
                foreach (var mv in mv_list)
                {
                    list.Add(new MissingValueModel()
                    {
                        DisplayName = mv.GetAttribute("placeholder")?.Value.ToString(),
                        Description = mv.GetAttribute("description")?.Value.ToString()
                    });
                }
            }
            else // create a empty list entry
            {
                list.Add(new MissingValueModel());
            }

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
    }
}