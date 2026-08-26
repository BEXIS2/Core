using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dim.Entities.Export.GBIF;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.SpeciesMatching;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.SpeciesMatching;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Modules.Smm.UI.Helpers;
using BExIS.Modules.Smm.UI.Helpers.MatchingAPIs;
using BExIS.Modules.Smm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using BExIS.Utils.Config;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;


namespace BExIS.Modules.Smm.UI.Controllers
{
    public class SpeciesController : Controller
    {
        // GET: Species

        // Provides access to file based matching Api functions (file creation, - reading, matching via request, ...)
        MatchingApiProvider matchingApiProvider = new Helpers.MatchingAPIs.MatchingApiProvider();

        public ActionResult Index()
        {
            string module = "SMM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        [JsonNetFilter]
        [HttpGet]
        public async Task<JsonResult> StartDownloadResultFile(long datasetId, long versionId, int stepId)
        {
            var user = ResolveUserAndRights(datasetId, out ActionResult errorResult);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Authentification error." }, HttpStatusCode.Unauthorized, JsonRequestBehavior.AllowGet);
            }

            var matchingProgressLocal = ProgressHelper.LoadMatchingProgress(datasetId, versionId);
            if (matchingProgressLocal == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "No matching progress found." }, HttpStatusCode.Conflict, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var stepLocal = matchingProgressLocal.GetStepById(stepId);
                var apiIdentifier = stepLocal.ApiIdentifier;

                MatchingApiBase apiBase = matchingApiProvider.GetApi(apiIdentifier);
                Debug.WriteLine("Starting DownloadResultFile function with apiIdentifier: ", apiIdentifier);
                var downloadedFilepath = await apiBase.DownloadResultFile(datasetId, versionId, stepId, matchingProgressLocal);

                if (string.IsNullOrWhiteSpace(downloadedFilepath))
                {
                    return JsonWithStatus(new { success = false, id = datasetId, message = "Failed to download or extract matching result file." }, HttpStatusCode.Conflict, JsonRequestBehavior.AllowGet);
                }

                // TODO: this should never fail
                // update matching progress entry
                if (stepLocal != null)
                {
                    stepLocal.ResultFileName = Path.GetFileName(downloadedFilepath);
                    matchingProgressLocal.UpdateStep(stepLocal);
                    ProgressHelper.SaveMatchingProgress(matchingProgressLocal, datasetId, versionId);
                }

                // return filepath for client to use (or filename)
                return Json(new { success = true, id = datasetId }, JsonRequestBehavior.AllowGet);
            }
            catch (KeyNotFoundException)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Matching API not found for the given identifier." }, HttpStatusCode.Conflict, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Unexpected error while downloading matching result file: " + ex.Message }, HttpStatusCode.InternalServerError, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpGet]
        // Returns status information about a matching result file (exists, downloading, matching progress coherence)
        public JsonResult GetMatchingFileStatus(long datasetId, long versionId, int stepId)
        {
            var user = ResolveUserAndRights(datasetId, out ActionResult errorResult);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Authentification error." }, HttpStatusCode.Unauthorized, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // locate versioned matching directory (may be null if folder missing)
                var directory = ProgressHelper.GetVersionedMatchingPath(datasetId, versionId);
                bool directoryExists = !string.IsNullOrEmpty(directory);

                // expected final filepath for matched csv
                var filename = ProgressHelper.GenMatchingFileName(true, datasetId, stepId);
                string filepath = directoryExists ? Path.Combine(directory, filename) : Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString(), ProgressHelper.MatchingFolderName, versionId.ToString(), filename);

                bool fileExists = System.IO.File.Exists(filepath);

                // marker file indicates active download
                var markerPath = filepath + ".downloading";
                bool markerExists = System.IO.File.Exists(markerPath);

                // inspect marker for potential staleness
                bool markerStale = false;
                DateTime? markerStart = null;
                if (markerExists)
                {
                    try
                    {
                        var content = System.IO.File.ReadAllText(markerPath);
                        // look for a line starting with download_start=
                        foreach (var line in content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var trimmed = line.Trim();
                            if (trimmed.StartsWith("download_start=", StringComparison.OrdinalIgnoreCase))
                            {
                                var ts = trimmed.Substring("download_start=".Length);
                                if (DateTime.TryParse(ts, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
                                {
                                    markerStart = dt.ToUniversalTime();
                                }
                                break;
                            }
                        }

                        if (markerStart.HasValue)
                        {
                            markerStale = (DateTime.UtcNow - markerStart.Value) > TimeSpan.FromHours(24);
                        }
                    }
                    catch { /* ignore parsing errors */ }
                }

                // matching progress and step info
                var matchingProgress = ProgressHelper.LoadMatchingProgress(datasetId, versionId);
                bool matchingProgressExists = matchingProgress != null;
                var step = matchingProgress?.GetStepById(stepId);
                bool stepExists = step != null;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        DirectoryExists = directoryExists,
                        FileExists = fileExists,
                        MarkerExists = markerExists,
                        MarkerStale = markerStale,
                        MarkerStart = markerStart,
                        MatchingProgressExists = matchingProgressExists,
                        StepExists = stepExists,
                        StepCompleted = step?.IsCompleted() ?? false,
                        DownloadLinkPresent = !string.IsNullOrWhiteSpace(step?.DownloadLink),
                        JobKeyPresent = !string.IsNullOrWhiteSpace(step?.JobKey)
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while checking matching file status: " + ex.Message);
                return JsonWithStatus(new { success = false, id = datasetId, message = "Error while checking matching file status." }, HttpStatusCode.Conflict, JsonRequestBehavior.AllowGet);
            }
        }



        private static readonly HttpClient _httpClient = new HttpClient();

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetMyDatasetsJson()
        {
            var result = new List<object>();
            const string EntityName = "Dataset";
            const RightType RightTypeCondition = RightType.Write;

            var user = ResolveRouteUser(out ActionResult userError);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, message = "User could not be resolved from route." }, HttpStatusCode.Unauthorized, JsonRequestBehavior.AllowGet);
            }

            string username = user.Name;
            // TODO: - CHANGE - JUST FOR TESTING
            if (string.IsNullOrWhiteSpace(username)) username = "erik";

            using (var datasetManager = new DatasetManager())
            using (var entityPermissionManager = new EntityPermissionManager())
            using (var entityManager = new EntityManager())
            using (var speciesMatchingResultManager = new SpeciesMatchingResultManager())
            {
                // Find entity (defensive)
                var entity = entityManager.FindByName(EntityName);
                if (entity == null)
                {
                    return JsonWithStatus(new { error = $"Entity '{EntityName}' not found." }, HttpStatusCode.Conflict, JsonRequestBehavior.AllowGet);
                }

                // collect dataset ids the current user has the requested right for
                List<long> datasetIds = entityPermissionManager.GetKeys(username, EntityName, typeof(Dataset), RightTypeCondition).Result ?? new List<long>();

                var smrmRepo = speciesMatchingResultManager.GetBulkUnitOfWork().GetReadOnlyRepository<SpeciesMatchingResult>();

                // For each dataset id retrieve all versions (and working copy if present) and include version id and version number
                foreach (var dsId in datasetIds)
                {
                    try
                    {
                        var versions = datasetManager.GetDatasetVersions(dsId) ?? new List<DatasetVersion>();

                        if (versions == null || versions.Count == 0) continue;

                        // choose a representative version for dataset-level info (prefer latest by timestamp)
                        var representative = versions.OrderByDescending(v => v.Timestamp).First();

                        bool isTabular = representative.Dataset.DataStructure?.Self is StructuredDataStructure;

                        // we only consider tabular datasets
                        if (!isTabular) continue;

                        bool metadataComplete = false;
                        if (representative.StateInfo != null)
                        {
                            metadataComplete = string.Equals(representative.StateInfo.State, DatasetStateInfo.Valid.ToString(), StringComparison.OrdinalIgnoreCase);
                        }

                        bool hasSpeciesMatches = smrmRepo.Query().Any(r => r.Dataset.Id == dsId);

                        // build versions info list
                        var orderedVersions = versions.OrderBy(v => v.Timestamp).ToList();

                        List<object> versionsInfo;

                        if (!hasSpeciesMatches)
                        {
                            // If there are no species matches for this dataset at all, only include the latest version info
                            var v = orderedVersions.Last();
                            versionsInfo = new List<object>
                            {
                                new
                                {
                                    VersionId = v.Id,
                                    VersionNr = datasetManager.GetDatasetVersionNr(v),
                                    Timestamp = v.Timestamp,
                                    Status = v.Status.ToString(),
                                    VersionName = v.VersionName ?? string.Empty,
                                    HasMatchingProgress = false
                                }
                            };
                        }
                        else
                        {
                            // Iterate from latest to oldest and apply header-mapping rules
                            var descVersions = orderedVersions.OrderByDescending(v => v.Timestamp).ToList();
                            var filtered = new List<object>();

                            for (int i = 0; i < descVersions.Count; i++)
                            {
                                var v = descVersions[i];
                                bool isLatest = (i == 0);

                                bool hasHeader = ProgressHelper.HasHeaderMappings(dsId, v.Id);

                                if (!hasHeader)
                                {
                                    if (isLatest)
                                    {
                                        // keep latest but mark matching progress as false
                                        filtered.Add(new
                                        {
                                            VersionId = v.Id,
                                            VersionNr = datasetManager.GetDatasetVersionNr(v),
                                            Timestamp = v.Timestamp,
                                            Status = v.Status.ToString(),
                                            VersionName = v.VersionName ?? string.Empty,
                                            HasMatchingProgress = false
                                        });
                                    }
                                    else
                                    {
                                        // drop this older version without header mappings
                                        continue;
                                    }
                                }
                                else
                                {
                                    // keep version and mark that header mappings exist
                                    filtered.Add(new
                                    {
                                        VersionId = v.Id,
                                        VersionNr = datasetManager.GetDatasetVersionNr(v),
                                        Timestamp = v.Timestamp,
                                        Status = v.Status.ToString(),
                                        VersionName = v.VersionName ?? string.Empty,
                                        HasMatchingProgress = true
                                    });
                                }
                            }

                            // filtered currently in descending order (latest first). Return ascending to keep previous ordering.
                            versionsInfo = filtered.OrderBy(v => ((DateTime)((dynamic)v).Timestamp)).ToList<object>();
                        }

                        result.Add(new
                        {
                            Id = representative.Dataset.Id,
                            Title = representative.Title ?? string.Empty,
                            Abstract = representative.Description ?? string.Empty,
                            IsTabular = isTabular,
                            MetadataComplete = metadataComplete,
                            HasMatchingProgress = hasSpeciesMatches,
                            DataStructureId = representative.Dataset.DataStructure?.Id,
                            Versions = versionsInfo
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error while retrieving versions for dataset " + dsId + ": " + ex.Message);
                        // ignore dataset on error and continue with others
                        continue;
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult SubmitHeaderMappings(SubmitHeaderMappingsRequest request)
        {
            // basic model binding validation: ensure payload present and DatasetId provided (>0)
            if (request.Data == null)
            {
                return JsonWithStatus(new { success = false, message = "Request body missing or invalid." }, HttpStatusCode.BadRequest);
            }

            var data = request.Data;
            var datasetId = data.DatasetId;
            var versionId = request.VersionId;

            if (!ModelState.IsValid)
            {
                // var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
                // if (!errors.Any()) errors.Add("Invalid request payload.");
                return JsonWithStatus(new { success = false, message = "Validation failed." }, HttpStatusCode.BadRequest);
            }

            var user = ResolveUserAndRights(datasetId, out ActionResult errorResult);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Authentification error." }, HttpStatusCode.Unauthorized);
            }

            if (ProgressHelper.HasHeaderMappings(datasetId, versionId))
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Header Mappings already exist and cannot be directly overwritten." }, HttpStatusCode.InternalServerError);
            }

            var folderSuccess = ProgressHelper.CreateMatchingFolder(datasetId, versionId);
            if (!folderSuccess)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Failed to create matching folder." }, HttpStatusCode.InternalServerError);
            }

            var success = ProgressHelper.CreateHeaderMappingsFile(data, datasetId, versionId, out string errorMessage);

            if (success)
            {
                return Json(new { success = true, id = datasetId });
            } else
            {
                return JsonWithStatus(new
                {
                    success = false,
                    message = errorMessage
                }, HttpStatusCode.BadRequest);
            }
        }

        [JsonNetFilter]
        [HttpPost]
        // Calls Datastatistic API with the given dataset id and variable id, creates a SpeciesMatchingResult for each unique name and saves to database.
        public async Task<ActionResult> Tailor(long datasetId, long versionId)
        {
            /* 
             This method is supposed to be called after the user has set up the header mappings and wants to start the matching process. 
            It should then call the DataStatistic API with the given dataset id and variable id and create a SpeciesMatchingResult for each
            individual name. The API call needs to be authenticated with a JWT token, which we can generate here with a custom method. 
             */

            var user = ResolveUserAndRights(datasetId, out ActionResult errorResult);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Authentification error." }, HttpStatusCode.Unauthorized);
            }

            // load header mappings for this dataset
            var headerMappings = ProgressHelper.LoadHeaderMappings(datasetId, versionId);
            if (headerMappings == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Header mappings not found for this dataset." }, HttpStatusCode.Conflict);
            }

            // find variable id for scientific name field
            long? targetVariableId = headerMappings.GetVariableIdForScientificName();
            if (targetVariableId == null) {
                return JsonWithStatus(new { success = false, id = datasetId, message = "No variable mapped for scientific name found in header mappings." }, HttpStatusCode.Conflict);
            }

            // check matching progress
            if (ProgressHelper.HasMatchingProgress(datasetId, versionId))
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Matching progress file already exists for this dataset. Please complete or reset existing matching progress before starting a new tailoring process." }, HttpStatusCode.Conflict);
            }

            // generate token for local api call
            string jwtToken = GenerateCustomJwtToken();
            if (jwtToken == null)
            {
                Debug.WriteLine("JWT token generation failed.");
                return JsonWithStatus(new { success = false, id = datasetId, message = "Could not call internal API. Please try again later." }, HttpStatusCode.Conflict);
            }

            var result = await TailorDataset(datasetId, targetVariableId.Value, jwtToken);
            var list_result = JsonConvert.DeserializeObject<List<ApiDataStatisticModel>>(result.Content);
            ApiDataStatisticModel json_result = list_result.FirstOrDefault();

            if (!result.IsSuccess)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Api call failed " + result.Content }, HttpStatusCode.BadRequest);
            }

            using (var speciesMatchingResultManager = new SpeciesMatchingResultManager())
            using (var uow = speciesMatchingResultManager.GetBulkUnitOfWork())
            using (var datasetManager = new DatasetManager())
            {
                try
                {
                    // check if rows present in API result
                    if (json_result.uniqueValues == null || json_result.uniqueValues.Rows.Count <= 0)
                    {
                        return JsonWithStatus(new { success = false, id = datasetId, message = "DataStatisticApi returned no meaningful result." }, HttpStatusCode.Conflict);
                    }

                    // check if expected column "var" is present in the API result
                    if (!json_result.uniqueValues.Columns.Contains("var"))
                    {
                        return JsonWithStatus(new { success = false, id = datasetId, message = "Column variable 'var' was missing on the Api result." }, HttpStatusCode.Conflict);
                    }

                    var repo = uow.GetRepository<SpeciesMatchingResult>();
                    var dataset = datasetManager.DatasetRepo.Get(datasetId);
                    var placeHolderTimeStamp = DateTime.Now;

                    // load existing species matching results for this dataset (if any)
                    var existingMatchesQuery = repo.Query().Where(r => r.Dataset.Id == datasetId);
                    bool hasMatchesForThisVersion = existingMatchesQuery.Any(r => r.DatasetVersionId == versionId);

                    // If there are already matches for the same dataset version, do not allow re-tailoring the same version
                    if (hasMatchesForThisVersion)
                    {
                        return JsonWithStatus(new { success = false, id = datasetId, message = "Species matching results already exist for this dataset version. Please complete or reset existing matching progress before starting a new tailoring process." }, HttpStatusCode.BadRequest);
                    }

                    // Build a set of already processed original names for the dataset (across all versions)
                    var existingOriginalNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var em in existingMatchesQuery.Select(r => r.OriginalName))
                    {
                        if (!string.IsNullOrWhiteSpace(em)) existingOriginalNames.Add(em);
                    }

                    int rowCount = 0;

                    // create one row in SpeciesMatchingResult per unique row from DataStatistic Api
                    foreach (DataRow row in json_result.uniqueValues.Rows)
                    {
                        if (row["var"] == DBNull.Value) continue;
                        string varValue = row["var"].ToString();

                        // skip names that were already processed for this dataset (from previous tailoring runs)
                        if (existingOriginalNames.Contains(varValue)) continue;

                        // create row for newly encountered original name
                        var matchingResult = new SpeciesMatchingResult
                        {
                            OriginalName = varValue,
                            EditedName = "",
                            MatchedName = "",
                            Status = "",
                            MatchType = "",
                            TimestampMatch = placeHolderTimeStamp,
                            MatchSource = "",
                            MatchSourceVersion = "",
                            ConfirmedByUser = false,
                            Dataset = dataset,
                            DatasetVersionId = versionId
                        };

                        repo.Put(matchingResult);
                        rowCount++;
                        existingOriginalNames.Add(varValue);
                    }

                    // TODO: check success (but in general should be built in a way that it never fails)
                    ProgressHelper.CreateMatchingProgressFile(datasetId, versionId, rowCount);

                    // batch commit
                    uow.Commit();
                }
                catch (Exception ex)
                {
                    // ignore on failure to avoid partial commits and inconsistent state; the user can then try again after fixing the underlying issue
                    uow.Ignore();
                    Debug.WriteLine("Custom exception catch in Tailor.");
                    return JsonWithStatus(new { success = false, id = datasetId, message = "An error occured while processing the Api result: " + ex.Message }, HttpStatusCode.InternalServerError);
                }
            }

            return Json(new { success = true, id = datasetId, message = json_result });
        }

        [JsonNetFilter]
        [HttpGet]
        // Get ALL SpeciesMatchingResults for a given dataset.
        // Used to display the overall state of the matching results in the frontend, and to allow users to filter and edit.
        public JsonResult ViewTailored(long datasetId, long versionId)
        {
            var user = ResolveUserAndRights(datasetId, out ActionResult errorResult);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Authentification error." }, HttpStatusCode.Unauthorized, JsonRequestBehavior.AllowGet);
            }

            var result = MatchingResultHelper.GetAll(datasetId, versionId);

            if (result == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "No matching results found for this dataset." }, HttpStatusCode.NotFound, JsonRequestBehavior.AllowGet);
            } else
            {
                return Json(new
                {
                    succes = true,
                    id = datasetId,
                    message = result
                }, JsonRequestBehavior.AllowGet
                );
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult ViewProgress(long datasetId, long versionId)
        {
            var user = ResolveUserAndRights(datasetId, out ActionResult errorResult);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Authentification error." }, HttpStatusCode.Unauthorized, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // header mappings
                bool hasHeaderMappings = ProgressHelper.HasHeaderMappings(datasetId, versionId);
                var headerMappings = hasHeaderMappings ? ProgressHelper.LoadHeaderMappings(datasetId, versionId) : null;

                // tailored check: any SpeciesMatchingResult entries for this dataset and version?
                bool isTailored = false;
                using (var smrm = new SpeciesMatchingResultManager())
                {
                    var repo = smrm.GetBulkUnitOfWork().GetReadOnlyRepository<SpeciesMatchingResult>();
                    isTailored = repo.Query().Any(r => r.Dataset.Id == datasetId && r.DatasetVersionId == versionId);
                }

                // matching progress
                bool hasMatchingProgress = ProgressHelper.HasMatchingProgress(datasetId, versionId);
                var matchingProgress = hasMatchingProgress ? ProgressHelper.LoadMatchingProgress(datasetId, versionId) : null;
                ExternalApiMetadata externalApiMetadata = ModuleManager.GetModuleSettings("SMM").GetValueByKey<ExternalApiMetadata>("externalApiMetadata");


                return Json(new
                {
                    success = true,
                    hasHeaderMappings,
                    headerMappings,
                    isTailored,
                    hasMatchingProgress,
                    matchingProgress,
                    externalApiMetadata
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while building progress view: " + ex);
                return JsonWithStatus(new { success = false, id = datasetId, message = "Error while retrieving progress information." }, HttpStatusCode.Conflict, JsonRequestBehavior.AllowGet);
            }

        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult GenNewMatchInputFile(long datasetId, long versionId, string apiIdentifier)
        {
            var user = ResolveUserAndRights(datasetId, out ActionResult errorResult);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Authentification error." }, HttpStatusCode.Unauthorized);
            }

            var matchingProgress = ProgressHelper.LoadMatchingProgress(datasetId, versionId);
            if (matchingProgress == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "No matching progress found." }, HttpStatusCode.Unauthorized);
            }

            if (!matchingProgress.AllStepsCompleted())
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Not all matching steps are completed yet. Please complete existing steps before generating a new matching input file." }, HttpStatusCode.Conflict);
            }

            var newStepId = matchingProgress.GetNewId();
            var datastructureId = GetDatastructureIdFromDatasetId(datasetId);
            if (datastructureId == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Dataset or datastructure not found." }, HttpStatusCode.NotFound);
            }

            try
            {
                MatchingApiBase apiBase =  matchingApiProvider.GetApi(apiIdentifier);
                var (FilePath, RowCount) = apiBase.GenerateInputFile(datasetId, datastructureId.Value, versionId, newStepId);
                string filepath = FilePath;
                int rows = RowCount;
                if (filepath == null)
                {
                    return JsonWithStatus(new { success = false, id = datasetId, message = "Could not generate MatchingInput file." }, HttpStatusCode.Conflict);
                }

                // this is double generated, but simpler
                var filename = ProgressHelper.GenMatchingFileName(false, datasetId, newStepId);

                matchingProgress.AddStep(newStepId, rows, filename, apiIdentifier);
                ProgressHelper.SaveMatchingProgress(matchingProgress, datasetId, versionId);
            } catch (KeyNotFoundException ex)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Matching API not found for the given identifier." }, HttpStatusCode.Conflict);
            } catch (Exception ex)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Unexpected error while generating matching input file: " + ex.Message  }, HttpStatusCode.InternalServerError);
            }

            return Json(new { success = true, data = new { id = datasetId, stepId = newStepId }, message = "Matching input file generated." });
        }

        [JsonNetFilter]
        [HttpPost]
        public async Task<JsonResult> MatchFileByStepId(long datasetId, long versionId, int stepId)
        {
            Debug.WriteLine("EXECUTING MatchNextFile");
            var user = ResolveUserAndRights(datasetId, out ActionResult errorResult);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Authentification error." }, HttpStatusCode.Unauthorized);
            }

            var matchingProgress = ProgressHelper.LoadMatchingProgress(datasetId, versionId);
            if (matchingProgress == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "No matching progress found." }, HttpStatusCode.Unauthorized);
            }

            StepEntry step = matchingProgress.GetStepById(stepId);
            if (step == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Step not found." }, HttpStatusCode.Conflict);
            }

            if (!step.IsReadyToMatch())
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Step is not ready to match. Either no input file available or step is already completed." }, HttpStatusCode.Conflict);
            }

            string nextFileName = step.InputFileName;
            string apiIdentifier = step.ApiIdentifier;

            // TODO: - pfad logik vereinfachen
            string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString(), ProgressHelper.MatchingFolderName, versionId.ToString());

            if (!Directory.Exists(directory))
            {
                Debug.WriteLine("SaveMatchingProgress: dataset directory does not exist: " + directory);
                return JsonWithStatus(new { success = false, id = datasetId, message = "No matching input file found on disk." }, HttpStatusCode.Conflict);
            }

            string filepath = Path.Combine(directory, nextFileName);

            try
            {
                // Read raw request body and try to parse options as JSON object
                JObject options = null;
                try
                {
                    Request.InputStream.Position = 0;
                    using (var sr = new StreamReader(Request.InputStream, Encoding.UTF8))
                    {
                        var body = sr.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            try
                            {
                                options = JObject.Parse(body);
                            }
                            catch (JsonException)
                            {
                                // try to extract nested property named "options"
                                try
                                {
                                    var parsed = JObject.Parse(body);
                                    if (parsed["options"] != null && parsed["options"].Type == JTokenType.Object)
                                    {
                                        options = (JObject)parsed["options"];
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                catch { /* ignore read errors and keep options null */ }

                IApiOptions apiOptions = matchingApiProvider.ResolveOptions(apiIdentifier, options);

                Debug.WriteLine(apiOptions);

                MatchingApiBase apiBase = matchingApiProvider.GetApi(apiIdentifier);
                MatchingApiResponse response = await apiBase.MatchAsync(datasetId, versionId, filepath, matchingProgress, apiOptions);
                response.StepId = step.Id;

                return Json(new { success = true, data = response.Message });
            }
            catch (ArgumentException ex)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = ex.Message }, HttpStatusCode.Conflict);
            }
            catch (KeyNotFoundException ex)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Matching API not found for the given identifier." }, HttpStatusCode.Conflict);
            }
            catch (Exception ex)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Unexpected error while generating matching input file: " + ex.Message }, HttpStatusCode.InternalServerError);
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult ViewMatchingResult(long datasetId, long versionId, int stepId)
        {
            var user = ResolveUserAndRights(datasetId, out ActionResult errorResult);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Authentification error." }, HttpStatusCode.Unauthorized, JsonRequestBehavior.AllowGet);
            }

            var matchingProgress = ProgressHelper.LoadMatchingProgress(datasetId, versionId);
            if (matchingProgress == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "No matching progress found under the given datasetId." }, HttpStatusCode.Conflict, JsonRequestBehavior.AllowGet);
            }

            if (!matchingProgress.IsCompletedById(stepId))
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "No valid matching job found in the matching progress data for the given stepId." }, HttpStatusCode.Conflict, JsonRequestBehavior.AllowGet);
            }

            var filepath = ProgressHelper.GetMatchedFilepath(datasetId, versionId, stepId);

            if (filepath == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "No result file found." }, HttpStatusCode.Conflict, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var apiIdentifier = matchingProgress.GetApiIdentifierById(stepId);
                Debug.WriteLine("SEARCHING MATCHING PROGRESS FOR: " + datasetId.ToString() + " " + versionId.ToString());
                Debug.WriteLine("API IDENTIFIER: " + apiIdentifier);
                MatchingApiBase apiBase = matchingApiProvider.GetApi(apiIdentifier);
                var matchingResults = apiBase.ReadResultFile(filepath);

                var acceptableMatchTypes = apiBase.GetAcceptableMatchTypes();

                // If matching results were read, build a set of IDs from the file and then
                // query the SpeciesMatchingResult table for the subset of rows that belong
                // to this dataset/version and whose IDs are present in the matching result file.
                List<SpeciesMatchingResult> dbResults = null;

                if (matchingResults != null && matchingResults.Count > 0)
                {
                    try
                    {
                        // collect ids from matching result rows
                        var idsFromFile = new HashSet<long>();
                        foreach (var m in matchingResults)
                        {
                            if (string.IsNullOrWhiteSpace(m.Original_ID)) continue;
                            if (long.TryParse(m.Original_ID, out long mid)) idsFromFile.Add(mid);
                        }

                        Debug.WriteLine("Creating HashSet<long> from file MatchingResultRows:");
                        Debug.WriteLine(idsFromFile.Count);

                        using (var smrm = new SpeciesMatchingResultManager())
                        {
                            var repo = smrm.GetBulkUnitOfWork().GetReadOnlyRepository<SpeciesMatchingResult>();

                            // query only the subset matching dataset/version and the ids from file
                            dbResults = repo.Query()
                                .Where(r => r.Dataset.Id == datasetId && r.DatasetVersionId == versionId && idsFromFile.Contains(r.Id))
                                .ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error while querying SpeciesMatchingResult subset: " + ex.Message);
                        dbResults = null; // fall back to null on error
                    }
                }

                return Json(new { success = true, matchingResults, acceptableMatchTypes, speciesMatchingResults = dbResults }, JsonRequestBehavior.AllowGet);
            }
            catch (ArgumentException ex)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = ex.Message }, HttpStatusCode.Conflict, JsonRequestBehavior.AllowGet);
            }
            catch (KeyNotFoundException ex)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Matching API not found for the given identifier." }, HttpStatusCode.Conflict, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Unexpected error while generating matching input file: " + ex.Message }, HttpStatusCode.InternalServerError, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult AcceptMatches(AcceptMatchesRequestModel request)
        {
            if (request == null) return JsonWithStatus(new { success = false, message = "Request body missing or invalid." }, HttpStatusCode.BadRequest);

            if (!ModelState.IsValid) return JsonWithStatus(new { success = false, message = "Validation failed." }, HttpStatusCode.BadRequest);
            
            var datasetId = request.DatasetId;
            var versionId = request.VersionId;
            var stepId = request.StepId;

            var user = ResolveUserAndRights(datasetId, out ActionResult errorResult);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Authentification error." }, HttpStatusCode.Unauthorized);
            }

            var matchingProgress = ProgressHelper.LoadMatchingProgress(datasetId, versionId);
            if (matchingProgress == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "No matching progress found under the given datasetId." }, HttpStatusCode.Conflict);
            }

            if (!matchingProgress.IsCompletedById(stepId))
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "No valid matching job found in the matching progress data for the given stepId." }, HttpStatusCode.Conflict);
            }

            try
            {
                var step = matchingProgress.GetStepById(stepId);
                var apiIdentifier = step.ApiIdentifier;
                MatchingApiBase apiBase = matchingApiProvider.GetApi(apiIdentifier);

                // Convert incoming List<string> MatchIds into a HashSet<long> for fast lookups
                var matchIdsSet = ConversionHelper.ConvertStringListToLongHashSet(request.MatchIds);

                apiBase.AcceptMatches(datasetId, versionId, step, matchIdsSet);
            } catch (Exception ex)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Error: " + ex.Message }, HttpStatusCode.BadRequest);
            }

            // matchIdsSet is now available for efficient contains checks further down the method
            return Json(new { success = true, id = request.DatasetId });
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult ApplyTailorEdits(long datasetId, long versionId, TailorEdit[] edits)
        {
            // Debug.WriteLine("Received request to apply tailor edits...");

            if (edits == null) return JsonWithStatus(new { success = false, message = "Request body missing or invalid." }, HttpStatusCode.BadRequest);

            if (!ModelState.IsValid) return JsonWithStatus(new { success = false, message = "Validation failed." }, HttpStatusCode.BadRequest);

            var user = ResolveUserAndRights(datasetId, out ActionResult errorResult);
            if (user == null)
            {
                return JsonWithStatus(new { success = false, id = datasetId, message = "Authentification error." }, HttpStatusCode.Unauthorized);
            }

            // Debug.WriteLine("Received request to apply tailor edits for dataset " + datasetId + " version " + versionId);

            try
            {
                var result = MatchingResultHelper.ApplyTailorEdits(datasetId, versionId, edits.ToList());
                if (result == false)
                {
                    return JsonWithStatus(new { success = false, id = datasetId, message = "Failed to apply edits." }, HttpStatusCode.InternalServerError);
                }

                // Debug.WriteLine("Successfully applied tailor edits for dataset " + datasetId + " version " + versionId);

                return Json(new { success = true, id = datasetId });
            }
            catch (Exception ex)
            {
                // Debug.WriteLine("Error while applying tailor edits for dataset " + datasetId + " version " + versionId + ": " + ex);
                return JsonWithStatus(new { success = false, id = datasetId, message = "Error while applying edits: " + ex.Message }, HttpStatusCode.BadRequest);
            }
        }

        public async Task<(bool IsSuccess, string Content)> TailorDataset(long datasetId, long variableId, string jwtToken)
        {
            string url = "http://localhost:44345/api/DataStatistic/" + datasetId.ToString() + "/" + variableId.ToString();

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                using (HttpResponseMessage response = await _httpClient.SendAsync(request))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponseContent = await response.Content.ReadAsStringAsync();

                        // since response codes are ambiguous, we need to check with this workaround
                        if (apiResponseContent.StartsWith("["))
                        {
                            // SUCCESS
                            return (true, apiResponseContent);
                        }
                        else
                        {
                            // API SOFT FAILURE
                            return (false, apiResponseContent);
                        }

                    }
                    else
                    {
                        // API HARD FAILURE
                        Debug.WriteLine("Error: " + response.StatusCode.ToString());
                        return (false, "Response status code was not ok." + response.StatusCode.ToString());
                    }
                }
            }
        }


        // Returns the datastructure id for the given dataset id, or null if not found or on error
        private long? GetDatastructureIdFromDatasetId(long datasetId)
        {
            try
            {
                using (var datasetManager = new DatasetManager())
                {
                    var datastructureId = datasetManager.DatasetRepo.Query()
                                 .Where(d => d.Id == datasetId)
                                 .Select(d => d.DataStructure != null ? (long?)d.DataStructure.Id : null)
                                 .FirstOrDefault();

                    return datastructureId;


                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error getting datastructure id: " + ex);
                return null;
            }
        }

        // Returns a custom JWT token for authenticating internal API calls
        private string GenerateCustomJwtToken()
        {
            try
            {
                var jwtConfiguration = GeneralSettings.JwtConfiguration;

                using (var userManager = new UserManager())
                {
                    // var user = BExISAuthorizeHelper.GetUserFromAuthorizationAsync(HttpContext).Result;
                    var user = ResolveRouteUser(out ActionResult userError);

                    Debug.WriteLine(user.DisplayName, " ", user.Email, " ", user.Id);

                    if (user != null)
                    {

                        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.IssuerSigningKey));
                        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);


                        //Create a List of Claims, Keep claims name short
                        var permClaims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName)
                    };


                        //Create Security Token object by giving required parameters
                        var token = new JwtSecurityToken(jwtConfiguration.ValidIssuer,
                            jwtConfiguration.ValidAudience,
                            permClaims,
                            notBefore: DateTime.Now,
                            expires: jwtConfiguration.ValidLifetime > 0 ? DateTime.Now.AddHours(jwtConfiguration.ValidLifetime) : DateTime.MaxValue,
                            signingCredentials: credentials);

                        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
                        return jwtToken;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Resolves user from route and checks write rights for the given datasetId. Returns the user if successful, otherwise null
        private User ResolveUserAndRights(long datasetId, out ActionResult errorResult)
        {
            errorResult = null;
            var user = ResolveRouteUser(out ActionResult userError);
            if (user == null)
            {
                errorResult = Json(new { success = false, id = datasetId, message = "User could not be resolved from route." });
                return null;
            }
            if (!EnsureUserHasWriteRights(user.UserName, datasetId, out ActionResult rightsError))
            {
                errorResult = Json(new { success = false, id = datasetId, message = "User has no write rights for this dataset." });
                return null;
            }
            return user;
        }

        // Resolves and returns user from the current HttpContext, or null if not possible
        private User ResolveRouteUser(out ActionResult errorResult)
        {
            errorResult = null;
            try
            {
                using (var userManager = new UserManager())
                {
                    // try token-based resolution first
                    var user = BExISAuthorizeHelper.GetUserFromAuthorizationAsync(HttpContext).Result;
                    if (user != null)
                    {
                        Debug.WriteLine("User resolved from token: " + user.Name);
                        return user;
                    }

                    // fallback: try to find a user named 'erik'
                    var fallback = userManager.Users.FirstOrDefault(u => u.Name == "erik");
                    if (fallback != null)
                    {
                        Debug.WriteLine("User 'erik' found and used as fallback.");
                        return fallback;
                    }

                    // final fallback: any available user (default)
                    var any = userManager.Users.FirstOrDefault();
                    if (any != null)
                    {
                        Debug.WriteLine("No specific user found; using any available user: " + any.Name);
                        return any;
                    }

                    Debug.WriteLine("Resolving Route User failed.");
                    errorResult = Json(new { success = false, message = "User not found in route data." });
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error resolving user: " + ex.ToString());
                errorResult = Json(new { success = false, message = "Error while resolving user." });
                return null;
            }
        }

        // Returns TRUE IF the given username has Write rights on the specified datasetId, ELSE FALSE
        private bool EnsureUserHasWriteRights(string username, long datasetId, out ActionResult errorResult)
        {
            errorResult = null;

            if (string.IsNullOrWhiteSpace(username))
            {
                errorResult = Json(new { success = false, id = datasetId, message = "Username is missing." });
                return false;
            }

            var entityPermissionManager = new EntityPermissionManager();
            try
            {
                bool hasRights = entityPermissionManager.HasEffectiveRightsAsync(username, typeof(Dataset), datasetId, RightType.Read).Result;
                if (!hasRights)
                {
                    errorResult = Json(new { success = false, id = datasetId, message = "User has no rights to read the given dataset." });
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while checking permissions: " + ex.ToString());
                errorResult = Json(new { success = false, id = datasetId, message = "Error while checking permissions." });
                return false;
            }
            finally
            {
                entityPermissionManager.Dispose();
            }
        }

        // Helper overloads to return a JsonResult and set a custom HTTP status code in a consistent way.
        // Use the no-behavior overload for POST (will use DenyGet), and the overload with behavior for GET responses.
        // GET usage (one-liner): return JsonWithStatus(new { success = false, id = datasetId, message = "..." }, HttpStatusCode.BadRequest, JsonRequestBehavior.AllowGet);
        // POST usage (one-liner): return JsonWithStatus(new { success = false, id = datasetId, message = "..." }, HttpStatusCode.BadRequest);
        private JsonResult JsonWithStatus(object data, HttpStatusCode statusCode, JsonRequestBehavior behavior)
        {
            Response.StatusCode = (int)statusCode;
            Response.TrySkipIisCustomErrors = true; // ensure IIS does not override the response body
            return Json(data, behavior);
        }

        private JsonResult JsonWithStatus(object data, HttpStatusCode statusCode)
        {
            Response.StatusCode = (int)statusCode;
            Response.TrySkipIisCustomErrors = true; // ensure IIS does not override the response body
            return Json(data, JsonRequestBehavior.DenyGet);
        }

    }
}