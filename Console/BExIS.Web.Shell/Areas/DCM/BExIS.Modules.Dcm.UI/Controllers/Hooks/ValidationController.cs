using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.Security.Entities.Authorization;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.Utils.Helpers;
using BExIS.Utils.Upload;
using System;
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
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);

            //ceck needed informtions from the cache
            //1. Filereader informations
            if (cache.ExcelFileReaderInfo == null && cache.AsciiFileReaderInfo == null)
                errors.Add(new Error(ErrorType.Other, "The validation cannot be executed because file informations are missing."));

            //2. Files
            if (cache.Files == null || cache.Files.Any() == false)
                errors.Add(new Error(ErrorType.Other, "The validation cannot be executed because no files are exiting."));

            using (var datasetManager = new DatasetManager())
            using (var dataStructureManager = new DataStructureManager())
            {
                try
                {
                    // check if id is a valid value
                    if (id <= 0)
                        errors.Add(new Error(ErrorType.Other, "The validation cannot be executed because a dataset with id " + id + " not exist."));

                    //check dataset and datastructure
                    var dataset = datasetManager.GetDataset(id); ;
                    if (dataset == null)
                    {

                        errors.Add(new Error(ErrorType.Other, "The validation cannot be executed because a dataset with id " + id + " not exist."));
                    }
                    else // dataset exist
                    {
                        // if datastructue is null
                        if (dataset.DataStructure == null)
                        {
                            errors.Add(new Error(ErrorType.Other, "The validation cannot be executed because a dataset has no structure."));
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

                            // read all files
                            foreach (var file in cache.Files)
                            {
                                // get extention, name & path
                                var ext = Path.GetExtension(file.Name);
                                var fileName = Path.GetFileName(file.Name);
                                var filePath = Path.Combine(AppConfiguration.DataPath, "Datasets", id.ToString(), "Temp", file.Name);

                                if (System.IO.File.Exists(filePath))
                                {
                                    // check if hash of file has changed or not exist
                                    // if so, then valdiate and overide hash if not set results to model

                                    string incomingHash = HashHelper.CreateMD5Hash(file.Name, file.Lenght.ToString());

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
                                            throw new NotImplementedException("validation with .xlsm is not supported yet");
                                        }
                                        else
                                        if (iOUtility.IsSupportedExcelFile(ext)) // Excel
                                        {
                                            //ExcelReader reader = new ExcelReader(sds, (ExcelFileReaderInfo)cache.ExcelFileReaderInfo);
                                            //Stream = reader.Open(filePath);
                                            //reader.ValidateFile(Stream, fileName, id);
                                            //file.Errors = reader.ErrorMessages;
                                            //cache.UpdateSetup.RowsCount = reader.NumberOfRows;
                                            throw new NotImplementedException("validation with .xlsx is not supported yet");
                                        }
                                        else
                                        if (iOUtility.IsSupportedAsciiFile(ext)) // asccii
                                        {
                                            AsciiReader reader = new AsciiReader(sds, (AsciiFileReaderInfo)cache.AsciiFileReaderInfo);
                                            Stream = reader.Open(filePath);
                                            //check against primary key
                                            var unique = uploadWizardHelper.IsUnique(id, ext, fileName, filePath, (AsciiFileReaderInfo)cache.AsciiFileReaderInfo, datastructureId);
                                            if (!unique)
                                            {
                                                file.Errors.Add(new Error(ErrorType.Datastructure, "the data in the file violate the primary key set."));
                                            }
                                            //validate
                                            reader.ValidateFile(Stream, fileName, id);
                                            file.Errors = reader.ErrorMessages;
                                            cache.UpdateSetup.RowsCount = reader.NumberOfRows;
                                        }

                                        file.ValidationHash = incomingHash;
                                    }
                                }

                                if (file.Errors.Any())
                                {
                                    FileErrors fileErrors = new FileErrors();
                                    fileErrors.File = file.Name;
                                    file.Errors.ForEach(e => fileErrors.Errors.Add(e.ToHtmlString()));
                                    fileErrors.SortedErrors = SortErrors(file.Errors);
                                    model.FileErrors.Add(fileErrors);

                                    errors.AddRange(file.Errors); // set to global error list
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new Error(ErrorType.Other, ex.Message));
                    FileErrors fileErrors = new FileErrors();
                    fileErrors.SortedErrors = SortErrors(errors);
                    model.FileErrors.Add(fileErrors);

                }


                // if the validation is done, prepare the model 
                if (cache.Files.Any()) //if any file exits update model 
                {
                    // set this flags as default, if a error exist it will change
                    cache.IsDataValid = true;
                    model.IsValid = true;

                    foreach (var fileErrors in model.FileErrors)
                    {
                        // if there are errors
                        if (fileErrors.Errors.Any())
                        {
                            cache.IsDataValid = false; 
                            model.IsValid = cache.IsDataValid;
                            cache.Messages.Add(new ResultMessage(DateTime.Now, fileErrors.Errors)); // add message for the history
                        }

                    }

                    // if model is valid , add a message 
                    if (model.IsValid)
                    {
                        cache.Messages.Add(new ResultMessage(DateTime.Now, new List<string>() { "The validation was successful." })); // add message for the history
                    }
                }
                else
                {
                    cache.IsDataValid = false;
                    model.IsValid = false;
                }

                // save cache
                hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, id);

                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        private List<Tuple<string, int, string>> SortErrors(List<Error> errors)
        {
            if (errors.Count > 0)
            {
                // split up the error messages for a btter overview-- >
                // set all value error with the same var name, datatypoe and issue-- >
                // create a dictionary for error messages

                // variable issues
                var varNames = errors.Where(e => e.GetType().Equals(ErrorType.Value)).Select(e => e.getName()).Distinct();
                var varIssues = errors.Where(e => e.GetType().Equals(ErrorType.Value)).Select(e => e.GetMessage()).Distinct();

                List<Tuple<string, int, string>> sortedErrors = new List<Tuple<string, int, string>>();

                foreach (string vn in varNames)
                {
                    foreach (string i in varIssues)
                    {
                        int c = errors.Where(e => e.getName().Equals(vn) && e.GetMessage().Equals(i)).Count();

                        if (c > 0)
                        {
                            sortedErrors.Add(new Tuple<string, int, string>(vn, c, i));
                        }
                    }
                }

                if (sortedErrors.Count > 0)
                {
                    return sortedErrors;
                }
            }

            return new List<Tuple<string, int, string>>();
        }
    }
}