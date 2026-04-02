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
        const string HM_FILENAME = "header_mappings.json";
        const string MP_FILENAME = "mapping_progress.json";
        const string MATCHED_PREFIX = "species_matched";
        const string UNMATCHED_PREFIX = "species_unmatched";

        public static MappingProgressModel LoadMappingProgress(long datasetId)
        {
            try
            {
                string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString());
                string filepath = Path.Combine(directory, MP_FILENAME);

                if (!System.IO.File.Exists(filepath))
                {
                    Debug.WriteLine($"Mapping Progress file not found: {filepath}");
                    return null;
                }

                string content = System.IO.File.ReadAllText(filepath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    Debug.WriteLine($"Mapping progress file empty: {filepath}");
                    return null;
                }

                var model = JsonConvert.DeserializeObject<MappingProgressModel>(content);
                return model;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load mapping progress: " + ex);
                return null;
            }
        }


        // Loads the header mappings JSON file for the given dataset id.
        // Returns the deserialized HeaderMappingsModel or null when the file
        // does not exist, is empty or cannot be parsed.
        public static HeaderMappingsModel LoadHeaderMappings(long datasetId)
        {
            try
            {
                string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString());
                string filepath = Path.Combine(directory, HM_FILENAME);

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


        // Creates a mapping_progress.json file for the given dataset with an empty Steps list.
        // Returns true when the file was created successfully, false on error.
        public static bool CreateMappingProgressFile(long datasetId, int numRowsGlobal)
        {
            try
            {
                string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString());

                // TODO: auto generate if missing?
                if (!Directory.Exists(directory))
                {
                    Debug.WriteLine("CreateMappingProgressFile: dataset directory does not exist: " + directory);
                    return false;
                }

                string filepath = Path.Combine(directory, MP_FILENAME);

                var model = new MappingProgressModel
                {
                    DatasetId = datasetId,
                    NumRowsGlobal = numRowsGlobal,
                    Steps = new List<StepEntry>()
                };

                string json = JsonConvert.SerializeObject(model, Formatting.Indented);
                System.IO.File.WriteAllText(filepath, json);

                Debug.WriteLine("Created mapping progress file: " + filepath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to create mapping progress file: " + ex);
                return false;
            }
        }

        public static bool CreateHeaderMappingsFile(HeaderMappingsModel data, long datasetId, out string errorMessage)
        {
            foreach (var entry in data.Mappings)
            {
                if (!MappingValidator.IsValid(entry.HeaderMapping))
                {
                    errorMessage = "The selected HeaderMapping " + entry.HeaderMapping + " does not exist.";
                    return false;
                }
            }

            string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString());
            string filepath = Path.Combine(directory, HM_FILENAME);

            if (!Directory.Exists(directory))
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

        // Persist the provided MappingProgressModel to the dataset's mapping_progress.json file.
        // This method will overwrite the file regardless of whether it already exists.
        // Returns true on success, false on failure.
        public static bool SaveMappingProgress(MappingProgressModel model)
        {
            if (model == null)
            {
                Debug.WriteLine("SaveMappingProgress: model is null.");
                return false;
            }

            long datasetId = model.DatasetId;
            if (datasetId <= 0)
            {
                Debug.WriteLine($"SaveMappingProgress: invalid dataset id: {datasetId}");
                return false;
            }

            try
            {
                string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString());

                if (!Directory.Exists(directory))
                {
                    Debug.WriteLine("SaveMappingProgress: dataset directory does not exist: " + directory);
                    return false;
                }

                string filepath = Path.Combine(directory, MP_FILENAME);

                string json = JsonConvert.SerializeObject(model, Formatting.Indented);

                // Overwrite the file (or create it if missing)
                System.IO.File.WriteAllText(filepath, json, Encoding.UTF8);

                Debug.WriteLine("Saved mapping progress file: " + filepath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to save mapping progress: " + ex);
                return false;
            }
        }

        public static string GenMatchingFileName(bool matched, long datasetId, int suffixId, bool withFileEnding = true)
        {
            string prefix = matched ? MATCHED_PREFIX : UNMATCHED_PREFIX;
            if (withFileEnding)
            {
                return $"{prefix}_{datasetId}_{suffixId}.csv";
            }
            else
            {
                return $"{prefix}_{datasetId}_{suffixId}";
            }
        }

        public static string GetMatchedFilepath(long datasetId, int stepId)
        {
            string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString());
            string filename = GenMatchingFileName(true, datasetId, stepId);
            string filepath = Path.Combine(directory, filename);
            if (System.IO.File.Exists(filepath))
            {
                return filepath;
            }
            else
            {
                return null;
            }

        }

        public static bool HasMappingProgress(long datasetId)
        {
            string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString());
            string filepath = Path.Combine(directory, MP_FILENAME);
            return System.IO.File.Exists(filepath);
        }

        public static bool HasHeaderMappings(long datasetId)
        {
            string directory = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString());
            string filepath = Path.Combine(directory, HM_FILENAME);
            return System.IO.File.Exists(filepath);
        }
    }
}