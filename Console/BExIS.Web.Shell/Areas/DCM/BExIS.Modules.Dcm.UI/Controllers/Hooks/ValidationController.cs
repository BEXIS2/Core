using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Hooks.Logs;
using BExIS.Utils.Helpers;
using BExIS.Utils.Upload;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ValidationController : Controller
    {
        private FileStream Stream;
        private UploadHelper uploadWizardHelper = new UploadHelper();


        // GET: Validation
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// validation view need to load the svelte builded script
        /// </summary>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        [JsonNetFilter]
        public ActionResult Start(long id, int version = 0)
        {
            return RedirectToAction("Validate", new { id, version });
        }

        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Write)]
        [JsonNetFilter]
        public JsonResult Validate(long id, int version = 0)
        {
            ValidationModel model = new ValidationModel();
            HookManager hookManager = new HookManager();
            IOUtility iOUtility = new IOUtility();
            List<Error> errors = new List<Error>();

            // load cache to get informations about the current upload workflow
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id );
            EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);
            if(log==null) log = new EditDatasetDetailsLog();

            var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);
            Hashtable primaryKeyHashTable = new Hashtable();

            //2. Files
            if (cache.Files == null || cache.Files.Any() == false)
                errors.Add(new Error(ErrorType.File, "The validation cannot be executed because no files are exiting."));

            if (!errors.Any())
            {
                using (var datasetManager = new DatasetManager())
                using (var dataStructureManager = new DataStructureManager())
                {
                    // check if id is a valid value
                    if (id <= 0)
                        errors.Add(new Error(ErrorType.Other, "The validation cannot be executed because a dataset with id " + id + " not exist."));

                    //check dataset and datastructure
                    var dataset = datasetManager.GetDataset(id); ;
                    if (dataset == null)
                    {
                        errors.Add(new Error(ErrorType.Dataset, "The validation cannot be executed because a dataset with id " + id + " not exist.","Dataset"));
                    }
                    else // dataset exist
                    {

                        #region file error
                        // if datastructue is null
                        if (dataset.DataStructure == null)
                        {
                            errors.Add(new Error(ErrorType.Datastructure, "The validation cannot be executed because a dataset has no structure.","Datastructure"));
                        }
                        else // datastrutcure exist
                        {
                            long datastructureId = dataset.DataStructure.Id;

                            StructuredDataStructure sds = dataStructureManager.StructuredDataStructureRepo.Get(datastructureId);
                            dataStructureManager.StructuredDataStructureRepo.LoadIfNot(sds.Variables);

                            // Add Number of Variables to the cache
                            if (sds != null)
                            {
                                if (cache.UpdateSetup == null) cache.UpdateSetup = new UpdateSetup();
                                cache.UpdateSetup.VariablesCount = sds.Variables.Count;
                            }

                            // check data and primary key usecases before reading files
                            // 1. no data, no pk -> (check for duplicates, all vars are the primary key) - is checked by UploadHelper.IsUnique fn
                            // 2. exist data & no pk -> force pk - info : change data or structure pk
                            bool hasData = false;
                            bool hasPrimaryKey = false;
                            hasData = datasetManager.RowCount(id) > 0?true:false;
                            hasPrimaryKey = sds.Variables.Where(v => v.IsKey.Equals(true)).Any();

                            if(hasData && !hasPrimaryKey)
                            errors.Add(new Error(ErrorType.Datastructure, "Updating data is only possible with a primary key. Please set the primary in the data structure.", "Datastructure"));

                            // in file:
                            // 3. exist data & pk but not unique -> info : change data or structure pk - is checked by uploadWizardHelper.IsUnique fn



                            // check against primary key in db
                            // this check happens outside of the files,
                            var unique = false;
                            if(cache.Files.Any()) uploadWizardHelper.IsUnique(id, ref primaryKeyHashTable);


                            // read all files
                            foreach (var file in cache.Files)
                            {
                                // get extention, name & path
                                var ext = Path.GetExtension(file.Name);
                                var fileName = Path.GetFileName(file.Name);
                                var filePath = Path.Combine(AppConfiguration.DataPath, "Datasets", id.ToString(), "Temp", file.Name);
                                List<Error> fileErrors = new List<Error>(); // collection of errors 

                                try
                                {
                                    
                                        //check file reader info
                                        bool existReaderInfo = true;
                                        if (cache.AsciiFileReaderInfo == null)
                                        {
                                            existReaderInfo = false;
                                            fileErrors.Add(new Error(ErrorType.FileReader, "File reader informations missing.", "FileReader"));
                                        }
                                        else
                                        {

                                            if (System.IO.File.Exists(filePath))
                                            {

                                                // check if hash of file has changed or not exist
                                                // if so, then valdiate and overide hash if not set results to model
                                                // the hash value need to be abot: name, lenght, structure id, ascci reader info;
                                                // if something has changed also validation need to repeat
                                                string readerInfo = cache.AsciiFileReaderInfo != null ? cache.AsciiFileReaderInfo.ToJson() : "";
                                                string incomingHash = HashHelper.CreateMD5Hash(file.Name, file.Lenght.ToString(), datastructureId.ToString(), readerInfo, cache.Files.Count.ToString(), sds.Variables.Where(v => v.IsKey.Equals(true)));

                                            // if a validation is allready run and the file has not changed, skip validation
                                            if (file.ValidationHash != incomingHash)
                                            {
                                                if (ext.Equals(".xlsm")) // excel Template
                                                {
                                                    //ExcelReader reader = new ExcelReader(sds, new ExcelFileReaderInfo());
                                                    //Stream = reader.Open(filePath);
                                                    //reader.ValidateTemplateFile(Stream, fileName, id);
                                                    //file.Errors = reader.ErrorMessages;
                                                    //cache.UpdateSetup.RowsCount = reader.NumberOfRows;
                                                    //throw new NotImplementedException("validation with .xlsm is not supported yet");
                                                    fileErrors.Add(new Error(ErrorType.File, "Validation with .xlsm is not supported yet.", "File"));
                                                }
                                                else
                                                if (iOUtility.IsSupportedExcelFile(ext)) // Excel
                                                {
                                                    //ExcelReader reader = new ExcelReader(sds, (ExcelFileReaderInfo)cache.ExcelFileReaderInfo);
                                                    //Stream = reader.Open(filePath);
                                                    //reader.ValidateFile(Stream, fileName, id);
                                                    //file.Errors = reader.ErrorMessages;
                                                    //cache.UpdateSetup.RowsCount = reader.NumberOfRows;
                                                    //throw new NotImplementedException("validation with .xlsx is not supported yet");
                                                    fileErrors.Add(new Error(ErrorType.File, "Validation with .xlsm is not supported yet.", "File"));
                                                }
                                                else
                                                if (iOUtility.IsSupportedAsciiFile(ext)) // asccii
                                                {
                                                    AsciiReader reader = new AsciiReader(sds, (AsciiFileReaderInfo)cache.AsciiFileReaderInfo);

                                                    // current validation direction
                                                    // check data structure
                                                    // check values
                                                    // check primary key

                                                    using (Stream = reader.Open(filePath))
                                                    {
                                                        //validate
                                                        reader.ValidateFile(Stream, fileName, id); // structure and values
                                                        fileErrors = reader.ErrorMessages;
                                                        cache.UpdateSetup.RowsCount = reader.NumberOfRows;
                                                    }

                                                    if (fileErrors == null || fileErrors.Count == 0)
                                                    {
                                                        
                                                        //check against primary key local file
                                                        unique = uploadWizardHelper.IsUnique(id, ext, fileName, filePath, (AsciiFileReaderInfo)cache.AsciiFileReaderInfo, datastructureId, ref primaryKeyHashTable);
                                                        if (!unique)
                                                        {
                                                            fileErrors.Add(new Error(ErrorType.PrimaryKey, "the data in the file violate the primary key set.", "Primary Key"));
                                                        }
                                                    }
                                                }

                                                file.ValidationHash = incomingHash;
                                            }
                                            else
                                            {
                                                var cacheFile = cache.Files.Where(f => f.Name.Equals(file.Name)).FirstOrDefault();
                                                if (cacheFile != null && cacheFile.Errors.Any())
                                                {
                                                    fileErrors.AddRange(cacheFile.Errors); 
                                                }
                                            }
                                                

                                            }
                                            else
                                            {
                                                fileErrors.Add(new Error(ErrorType.File, "File is missing.", "File"));
                                            }
                                            
                                        }
                                    
                                }
                                catch (Exception ex)
                                {
                                    fileErrors?.Add(new Error(ErrorType.Dataset, ex.Message, "Dataset"));
                                }
                                finally
                                {

                                    if (fileErrors!=null) file.Errors = fileErrors;

                                    FileValidationResult result = new FileValidationResult();
                                    result.File = file.Name;

                                    if (file.Errors.Any())
                                    { 
                                        file.Errors.ForEach(e => result.Errors.Add(e.ToHtmlString()));
                                        result.SortedErrors = EditHelper.SortFileErrors(file.Errors);
                                        errors.AddRange(file.Errors); // set to global error list
                                    }

                                    model.FileResults.Add(result);
                                }
                                

                            }
                        }

                        #endregion
                    }

                    // if the validation is done, prepare the model 
                    if (cache.Files.Any()) //if any file exits update model 
                    {
                        // set this flags as default, if a error exist it will change
                        cache.IsDataValid = true;
                        model.IsValid = true;

                        List<string> e = new List<string>(); //overall collection of errors

                        foreach (var result in model.FileResults)
                        {

                            // if there are errors
                            if (result.Errors.Any())
                            {
                                cache.IsDataValid = false;
                                model.IsValid = cache.IsDataValid;
                                result.Errors.ForEach(error=> e.Add(result.File+" : "+error)); // add file name to each message
                            }
                        }

                        if(e.Any())
                            log.Messages.Add(new LogMessage(DateTime.Now, e, username, "Validation", "validate")); // add message for the history

                    }
                    else
                    {
                        cache.IsDataValid = false;
                        model.IsValid = false;
                    }
                }
            }

            // if model is valid , add a message 
            if (model.IsValid)
            {
                log.Messages.Add(new LogMessage(DateTime.Now, new List<string>() { "The validation was successful." }, username, "Validation", "validate")); // add message for the history
            }


            // save cache
            hookManager.Save(cache,log, "dataset", "details", HookMode.edit, id);

            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}