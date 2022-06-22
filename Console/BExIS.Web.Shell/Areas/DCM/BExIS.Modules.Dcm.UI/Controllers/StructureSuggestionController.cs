﻿using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.Modules.Dcm.UI.Hooks;
using BExIS.Modules.Dcm.UI.Models.StructureSuggestion;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
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
            ViewBag.id = id;
            ViewBag.version = version;
            ViewBag.file = file;

            return View();
        }

        [JsonNetFilter]
        public JsonResult Load(long id, string file, int version = 0)
        {
            if (id <= 0) throw new ArgumentException(nameof(id));
            if (string.IsNullOrEmpty(file)) throw new ArgumentException(nameof(file));

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

            if (model.Preview.Any())
            {

                // get delimeter
                TextSeperator textSeperator = structureAnalyser.SuggestDelimeter(model.Preview.First(), model.Preview.Last());
                model.Delimeter = AsciiFileReaderInfo.GetSeperator(textSeperator);

                model.Delimeters = getDelimeters();

                // get decimal
                // the structure analyzer return a result or trigger a exception
                // catch the exception and set a default value -1 
                try
                {
                    DecimalCharacter decimalCharacter = structureAnalyser.SuggestDecimal(model.Preview.First(), model.Preview.Last(), textSeperator);
                    model.Decimal = AsciiFileReaderInfo.GetDecimalCharacter(decimalCharacter);
                } 
                catch(Exception ex)
                {
                    model.Decimal = -1;
                }

                model.Decimals = getDecimals();

                // get textmarkers
                TextMarker textMarker = structureAnalyser.SuggestTextMarker(model.Preview.First(), model.Preview.Last());
                model.TextMarker = AsciiFileReaderInfo.GetTextMarker(textMarker);

                model.TextMarkers = getTextMarkers();
            }

            // get default missing values


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
            {
                // create strutcure
                StructuredDataStructure newStructure = structureManager.CreateStructuredDataStructure(
                        model.Title,
                        model.Description,
                        null,
                        null,
                        DataStructureCategory.Generic
                    );


                // create variables

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

                    // create var and add to structure

                    // generate variables
                    var result = variableManager.CreateVariable(
                        variable.Name,
                        dataType,
                        unit,
                        newStructure.Id,
                        variable.IsOptional,
                        variable.IsKey
                        );


                    // gerenate missing values and link them to each variable

                }

                // store link to entity

                var dataset = datasetManager.GetDataset(model.Id);
                dataset.DataStructure = newStructure;
                datasetManager.UpdateDataset(dataset);

                #region update cache

                HookManager hookManager = new HookManager();
                EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, model.Id);

                // update Data description hook

                // set file reading informations
                if (cache.AsciiFileReaderInfo == null)
                    cache.AsciiFileReaderInfo = new AsciiFileReaderInfo();

                cache.AsciiFileReaderInfo.Decimal = (DecimalCharacter)model.Decimal;
                cache.AsciiFileReaderInfo.Seperator = (TextSeperator)model.Delimeter;
                cache.AsciiFileReaderInfo.TextMarker = (TextMarker)model.TextMarker;
                cache.AsciiFileReaderInfo.Data = model.Markers.Where(m=>m.Type.Equals("data")).FirstOrDefault().Row;
                cache.AsciiFileReaderInfo.Variables = model.Markers.Where(m=>m.Type.Equals("variable")).FirstOrDefault().Row;


                // update modifikation date
                cache.UpdateLastModificarion(typeof(DataDescriptionHook));


                // store in messages
                string message = String.Format("the structure {0} was successfully created and attached to the dataset {1}.",model.Title, model.Id);
                cache.Messages.Add(new ResultMessage(DateTime.Now, new List<string>() { message }));

                // save cache
                hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, model.Id);
                #endregion

            }

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
                if (activeCells==null || activeCells[i]) // only create a var to the model if the cell is active or the list is null - means add everyone
                {

                    VariableModel var = new VariableModel();

                    var.Name = getValueFromMarkedRow(markerRows, model.Markers, "variable", (char)model.Delimeter, i);
                    var.Description = getValueFromMarkedRow(markerRows, model.Markers, "description", (char)model.Delimeter, i);


                    // check and get datatype
                    if (systemTypes.ContainsKey(i))
                        var.SystemType = systemTypes[i].Name;

                    // get list of possible datatypes
                    var.DataType = strutcureAnalyzer.SuggestDataType(var.SystemType).Select(d => new ListItem(d.Id, d.Name, "analysis results")).FirstOrDefault();

                    // get list of possible units
                    var unitInput = getValueFromMarkedRow(markerRows, model.Markers, "unit", (char)model.Delimeter, i);
                    strutcureAnalyzer.SuggestUnit(unitInput, var.DataType.Text).ForEach(u => var.PossibleUnits.Add(new ListItem(u.Id, u.Name, "analysis results")));
                    var.Unit = var.PossibleUnits.FirstOrDefault();

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
                        list.Add(new ListItem(unit.Id, unit.Name, "other"));
                    }
                }

                // get default missing values
                return Json(list, JsonRequestBehavior.AllowGet);
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
            var dataTotal = total - skipped - datastart;

            long selection = structureAnalyser.GetNumberOfRowsToAnalyse(min, max, percentage, dataTotal);

            List<string> rows = AsciiReader.GetRandowRows(file, total, selection, datastart);

            return structureAnalyser.SuggestSystemTypes(rows, delimeter, decimalCharacter, missingValues);
        }

        private string getValueFromMarkedRow(List<string> rows, List<Marker> markers, string type, char delimeter, int position)
        {
            // check and get description
            int markerIndex = markers.FindIndex(m => m.Type.Equals(type));
            if (markerIndex > -1)
            {
                return rows[markerIndex].Split(delimeter)[position];
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
    }
}