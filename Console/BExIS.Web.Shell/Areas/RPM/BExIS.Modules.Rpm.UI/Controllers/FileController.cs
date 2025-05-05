using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.IO.Transform.Input;
using BExIS.IO;
using BExIS.Modules.Rpm.UI.Models.DataStructure;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Hooks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Data;
using BExIS.UI.Models;
using Vaiona.Web.Mvc.Modularity;
using BExIS.UI.Hooks.Logs;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class FileController : Controller
    {
        // GET: File
        public ActionResult Index()
        {
            return View();
        }

        [JsonNetFilter]
        [DoesNotNeedDataAccess]
        public JsonResult Load(string file, EncodingType encoding = EncodingType.UTF8, long entityId = 0, long version = 0)
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
            model.Preview = AsciiReader.GetRows(filepath, AsciiFileReaderInfo.GetEncoding(encoding), 10);
            model.Total = AsciiReader.Count(filepath);
            model.Skipped = AsciiReader.Skipped(filepath);

            if (cache == null || cache.AsciiFileReaderInfo == null) // file reader infos not exit, suggest it
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

                    model.FileEncoding = (int)encoding;
                }
            }
            else // allready exist, set it
            {
                model.Decimal = (int)cache.AsciiFileReaderInfo.Decimal;
                model.Delimeter = (int)cache.AsciiFileReaderInfo.Seperator;
                model.TextMarker = (int)cache.AsciiFileReaderInfo.TextMarker;
                model.FileEncoding = (int)cache.AsciiFileReaderInfo.EncodingType;

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
            model.Encodings = getEncodings();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [DoesNotNeedDataAccess]
        public JsonResult Store(DataStructureCreationModel model)
        {
            #region update cache

            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, model.EntityId);
            EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, model.EntityId);

            var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

            // update Data description hook

            // set file reading information
            if (cache.AsciiFileReaderInfo == null)
                cache.AsciiFileReaderInfo = new AsciiFileReaderInfo();

            cache.AsciiFileReaderInfo.Decimal = (DecimalCharacter)model.Decimal;
            cache.AsciiFileReaderInfo.Seperator = (TextSeperator)model.Delimeter;
            cache.AsciiFileReaderInfo.TextMarker = (TextMarker)model.TextMarker;
            cache.AsciiFileReaderInfo.Data = model.Markers.Where(m => m.Type.Equals("data")).FirstOrDefault().Row + 1; // add 1 to store not the index but the row
            cache.AsciiFileReaderInfo.Variables = model.Markers.Where(m => m.Type.Equals("variable")).FirstOrDefault().Row + 1;// add 1 to store not the index but the row
            cache.AsciiFileReaderInfo.Cells = model.Markers.Where(m => m.Type.Equals("variable")).FirstOrDefault().Cells;
            cache.AsciiFileReaderInfo.EncodingType = (EncodingType)model.FileEncoding;

            // additional information
            // description
            var descriptionMarker = model.Markers.Where(m => m.Type.Equals("description")).FirstOrDefault();
            if (descriptionMarker != null) cache.AsciiFileReaderInfo.Description = descriptionMarker.Row + 1;// add 1 to store nit the index but the row
            else cache.AsciiFileReaderInfo.Description = 0;
            // units
            var unitMarker = model.Markers.Where(m => m.Type.Equals("unit")).FirstOrDefault();
            if (unitMarker != null) cache.AsciiFileReaderInfo.Unit = unitMarker.Row + 1;// add 1 to store nit the index but the row
            else cache.AsciiFileReaderInfo.Unit = 0;

            // update modification date
            //cache.UpdateLastModificarion(typeof(DataDescriptionHook));

            // store in messages
            string message = String.Format("the structure {0} was successfully created and attached to the dataset {1}.", model.Title, model.EntityId);
            log.Messages.Add(new LogMessage(DateTime.Now, new List<string>() { message }, username, "Structure suggestion", "store"));

            // save cache
            hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, model.EntityId);

            #endregion update cache

            return Json(true, JsonRequestBehavior.AllowGet);
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

            // double quotes
            c = AsciiFileReaderInfo.GetTextMarker(TextMarker.none);
            kvP = new ListItem();
            kvP.Id = Convert.ToInt32(c);
            kvP.Text = AsciiFileReaderInfo.GetTextMarkerAsString(TextMarker.none);


            list.Add(kvP);

            return list;
        }

        private List<ListItem> getEncodings()
        {
            List<ListItem> list = new List<ListItem>();

            foreach (EncodingType type in Enum.GetValues(typeof(EncodingType)))
            {
                ListItem kvP = new ListItem();
                kvP.Id = (int)type;
                kvP.Text = type.ToString();
                list.Add(kvP);
            }

            return list;
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

            return list;
        }
    }
}