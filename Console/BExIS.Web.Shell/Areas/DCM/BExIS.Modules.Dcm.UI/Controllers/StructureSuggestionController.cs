using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.Modules.Dcm.UI.Models.StructureSuggestion;
using BExIS.Security.Entities.Authorization;
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
                DecimalCharacter decimalCharacter = structureAnalyser.SuggestDecimal(model.Preview.First(), model.Preview.Last(), textSeperator);
                model.Decimal = AsciiFileReaderInfo.GetDecimalCharacter(decimalCharacter);

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

            // get default missing values
            return Json(model, JsonRequestBehavior.AllowGet);
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

            // missingvalues
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
                VariableModel var = new VariableModel();

                var.Name = getValueFromMarkedRow(markerRows, model.Markers, "variable", (char)model.Delimeter, i);
                var.Description = getValueFromMarkedRow(markerRows, model.Markers, "description", (char)model.Delimeter, i);
                

                // check and get datatype
                if(systemTypes.ContainsKey(i))
                  var.SystemType = systemTypes[i].Name;

                // get list of possible datatypes
                var.DataType = strutcureAnalyzer.SuggestDataType(var.SystemType).Select(d=> new KvP(d.Id,d.Name)).FirstOrDefault();

                // get list of possible units
                var unitInput = getValueFromMarkedRow(markerRows, model.Markers, "unit", (char)model.Delimeter, i);
                strutcureAnalyzer.SuggestUnit(unitInput, var.DataType.Text).ForEach(u => var.PossibleUnits.Add(new KvP(u.Id, u.Name)));
                var.Unit = var.PossibleUnits.FirstOrDefault();

                model.Variables.Add(var);

            }

            // get default missing values
            return Json(model);
        }

        private List<KvP> getDelimeters()
        {
            List<KvP> list = new List<KvP>();

            // tab
            char c = AsciiFileReaderInfo.GetSeperator(TextSeperator.tab);
            KvP kvP = new KvP();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetSeperatorAsString(TextSeperator.tab);

            list.Add(kvP);

            //comma

            c = AsciiFileReaderInfo.GetSeperator(TextSeperator.comma);
            kvP = new KvP();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetSeperatorAsString(TextSeperator.comma);

            list.Add(kvP);

            // semicolon

            c = AsciiFileReaderInfo.GetSeperator(TextSeperator.semicolon);
            kvP = new KvP();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetSeperatorAsString(TextSeperator.semicolon);

            list.Add(kvP);

            // space
            c = AsciiFileReaderInfo.GetSeperator(TextSeperator.space);
            kvP = new KvP();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetSeperatorAsString(TextSeperator.space);

            list.Add(kvP);


            return list;
        }

        private List<KvP> getDecimals()
        {

            List<KvP> list = new List<KvP>();

            // point
            char c = AsciiFileReaderInfo.GetDecimalCharacter(DecimalCharacter.point);
            KvP kvP = new KvP();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetDecimal(DecimalCharacter.point);

            list.Add(kvP);

            // comma
            c = AsciiFileReaderInfo.GetDecimalCharacter(DecimalCharacter.comma);
            kvP = new KvP();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetDecimal(DecimalCharacter.comma);

            list.Add(kvP);

            return list;
        }

        private List<KvP> getTextMarkers()
        {
            List<KvP> list = new List<KvP>();

            // quotes
            char c = AsciiFileReaderInfo.GetTextMarker(TextMarker.quotes);
            KvP kvP = new KvP();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetTextMarkerAsString(TextMarker.quotes);

            list.Add(kvP);

            // double quotes
            c = AsciiFileReaderInfo.GetTextMarker(TextMarker.doubleQuotes);
            kvP = new KvP();
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
    }
}