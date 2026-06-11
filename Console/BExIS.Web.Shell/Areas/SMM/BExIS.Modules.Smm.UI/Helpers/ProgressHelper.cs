using BExIS.Modules.Smm.UI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Smm.UI.Helpers
{
    public class ProgressHelper
    {
        public const string MappingFilename = "header_mappings.json";
        public const string MatchingFilename = "matching_progress.json";
        public const string MatchedPrefix = "species_matched";
        public const string UnmatchedPrefix = "species_unmatched";
        public const string MatchingFolderName = "Matching";

        public static MatchingProgressModel LoadMatchingProgress(long datasetId, long versionId)
        {
            try
            {
                string directory = GetVersionedMatchingPath(datasetId, versionId);
                if (directory == null)
                {
                    Debug.WriteLine("LoadMatchingProgress: dataset directory does not exist.");
                    return null;
                }

                string filepath = Path.Combine(directory, MatchingFilename);

                if (!System.IO.File.Exists(filepath))
                {
                    Debug.WriteLine($"Matching Progress file not found: {filepath}");
                    return null;
                }

                string content = System.IO.File.ReadAllText(filepath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    Debug.WriteLine($"Matching progress file empty: {filepath}");
                    return null;
                }

                var model = JsonConvert.DeserializeObject<MatchingProgressModel>(content);
                return model;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load matching progress: " + ex);
                return null;
            }
        }


        // Loads the header mappings JSON file for the given dataset id.
        // Returns the deserialized HeaderMappingsModel or null when the file
        // does not exist, is empty or cannot be parsed.
        public static HeaderMappingsModel LoadHeaderMappings(long datasetId, long versionId)
        {
            try
            {
                string directory = GetVersionedMatchingPath(datasetId, versionId);
                if (directory == null)
                {
                    Debug.WriteLine("LoadHeaderMappings: dataset directory does not exist.");
                    return null;
                }

                string filepath = Path.Combine(directory, MappingFilename);

                if (!System.IO.File.Exists(filepath))
                {
                    Debug.WriteLine($"Header mappings file not found: {filepath}");
                    return null;
                }

                string content = System.IO.File.ReadAllText(filepath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    Debug.WriteLine($"Header mappings file empty: {filepath}");
                    return null;
                }

                var model = JsonConvert.DeserializeObject<HeaderMappingsModel>(content);
                return model;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load header mappings: " + ex);
                return null;
            }
        }

        public static bool CreateMatchingFolder(long datasetId, long versionId)
        {
            try
            {
                string subdirectory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString());
                if (!Directory.Exists(subdirectory))
                {
                    Debug.WriteLine("CreateMatchingFolder: dataset directory does not exist: " + subdirectory);
                    return false;
                }

                string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString(), MatchingFolderName, versionId.ToString());
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                else
                {
                    Debug.WriteLine("Matching folder already exists: " + directory);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to create matching folder: " + ex);
                return false;
            }
        }

        // Creates a matching_progress.json file for the given dataset with an empty Steps list.
        // Returns true when the file was created successfully, false on error.
        public static bool CreateMatchingProgressFile(long datasetId, long versionId, int numRowsGlobal)
        {
            try
            {
                string directory = GetVersionedMatchingPath(datasetId, versionId);

                if (directory == null)
                {
                    Debug.WriteLine("CreateMatchingProgressFile: dataset directory does not exist.");
                    return false;
                }

                string filepath = Path.Combine(directory, MatchingFilename);

                var model = new MatchingProgressModel
                {
                    DatasetId = datasetId,
                    NumRowsGlobal = numRowsGlobal,
                    Steps = new List<StepEntry>()
                };

                string json = JsonConvert.SerializeObject(model, Formatting.Indented);
                System.IO.File.WriteAllText(filepath, json);

                Debug.WriteLine("Created matching progress file: " + filepath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to create matching progress file: " + ex);
                return false;
            }
        }

        public static bool CreateHeaderMappingsFile(HeaderMappingsModel data, long datasetId, long versionId, out string errorMessage)
        {
            foreach (var entry in data.Mappings)
            {
                if (!MappingValidator.IsValid(entry.HeaderMapping))
                {
                    errorMessage = "The selected HeaderMapping " + entry.HeaderMapping + " does not exist.";
                    return false;
                }
            }

            string directory = GetVersionedMatchingPath(datasetId, versionId);
            string filepath = Path.Combine(directory, MappingFilename);

            if (directory == null)
            {
                errorMessage = "The dataset folder with id " + datasetId + " does not exist.";
                return false;
            }
            else
            {
                System.IO.File.WriteAllText(filepath, JsonConvert.SerializeObject(data));
                errorMessage = null;
                return true;
            }
        }

        // Persist the provided MatchingProgressModel to the dataset's matching_progress.json file.
        // This method will overwrite the file regardless of whether it already exists.
        // Returns true on success, false on failure.
        public static bool SaveMatchingProgress(MatchingProgressModel model, long datasetId, long versionId)
        {
            if (model == null)
            {
                Debug.WriteLine("SaveMatchingProgress: model is null.");
                return false;
            }

            try
            {
                string directory = GetVersionedMatchingPath(datasetId, versionId);

                if (directory == null)
                {
                    Debug.WriteLine("SaveMatchingProgress: dataset directory does not exist.");
                    return false;
                }

                string filepath = Path.Combine(directory, MatchingFilename);

                string json = JsonConvert.SerializeObject(model, Formatting.Indented);

                // Overwrite the file (or create it if missing)
                System.IO.File.WriteAllText(filepath, json, Encoding.UTF8);

                Debug.WriteLine("Saved matching progress file: " + filepath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to save matching progress: " + ex);
                return false;
            }
        }

        public static string GenMatchingFileName(bool matched, long datasetId, int suffixId, bool withFileEnding = true)
        {
            string prefix = matched ? MatchedPrefix : UnmatchedPrefix;
            if (withFileEnding)
            {
                return $"{prefix}_{datasetId}_{suffixId}.csv";
            }
            else
            {
                return $"{prefix}_{datasetId}_{suffixId}";
            }
        }

        public static string GetMatchedFilepath(long datasetId, long versionId, int stepId, bool exists = true)
        {
            string directory = GetVersionedMatchingPath(datasetId, versionId);
            if (directory == null) {
                return null;
            }

            string filename = GenMatchingFileName(true, datasetId, stepId);
            string filepath = Path.Combine(directory, filename);
            if (System.IO.File.Exists(filepath))
            {
                return filepath;
            }
            else
            {
                if (exists)
                {
                    return null;
                } else
                {
                    return filepath;
                }
            }

        }

        public static string GetVersionedMatchingPath(long datasetId, long versionId)
        {
            string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString(), MatchingFolderName, versionId.ToString());
            if (Directory.Exists(directory))
            {
                return directory;
            }
            else
            {
                return null;
            }
        }

        public static bool HasMatchingProgress(long datasetId, long versionId)
        {
            string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString(), MatchingFolderName, versionId.ToString());
            string filepath = Path.Combine(directory, MatchingFilename);
            return System.IO.File.Exists(filepath);
        }

        public static bool HasHeaderMappings(long datasetId, long versionId)
        {
            string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString(), MatchingFolderName, versionId.ToString());
            string filepath = Path.Combine(directory, MappingFilename);
            return System.IO.File.Exists(filepath);
        }
    }
}