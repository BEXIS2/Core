using BExIS.Dlm.Entities.SpeciesMatching;
using BExIS.Dlm.Services.SpeciesMatching;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Smm.UI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Smm.UI.Helpers
{
    // This helper class is used for reading and parsing the matching result files. Currently only ChecklistBank (CLB) but later also other APIs.
    public class MatchingResultHelper
    {
        // returns all SpeciesMatchingResult entries for a given datasetId, or null if an error occurs
        public static List<SpeciesMatchingResult> GetAll(long datasetId)
        {
            try
            {
                using (var smrm = new SpeciesMatchingResultManager())
                {
                    var smrmRepo = smrm.GetBulkUnitOfWork().GetReadOnlyRepository<SpeciesMatchingResult>();
                    List<SpeciesMatchingResult> result = smrmRepo.Query().Where(r => r.Dataset.Id == datasetId).ToList();

                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Generate a CSV file containing all confirmed SpeciesMatchingResults for a given datasetId. Returns the file path and the number of rows written, or (null, 0) if an error occurs.
        public static (string FilePath, int RowCount) GenerateUnmatchedCsv(long datasetId, long dataStructureId, int stepId)
        {
            DataTable dt = new DataTable("SpeciesUnmatched");
            dt.Columns.Add("ID", typeof(long));
            dt.Columns.Add("scientificName", typeof(string));
            dt.Columns.Add("rank", typeof(string));
            dt.Columns.Add("kingdom", typeof(string));
            dt.Columns.Add("authorship", typeof(string));

            int writtenCount = 0;

            using (var smrm = new SpeciesMatchingResultManager())
            {
                var smrmRepo = smrm.GetBulkUnitOfWork().GetReadOnlyRepository<SpeciesMatchingResult>();
                List<SpeciesMatchingResult> result = smrmRepo.Query().Where(r => r.Dataset.Id == datasetId && r.ConfirmedByUser == false).ToList();

                // TODO: - write all columns correctly
                foreach (var item in result)
                {
                    if (item.EditedName != null && item.EditedName != "")
                    {
                        dt.Rows.Add(item.Id, item.EditedName, "species", "", "");
                        writtenCount++;
                    }
                    else if (item.CleanedName != null && item.CleanedName != "")
                    {
                        dt.Rows.Add(item.Id, item.CleanedName, "species", "", "");
                        writtenCount++;
                    }
                    else if (item.OriginalName != null && item.OriginalName != "")
                    {
                        dt.Rows.Add(item.Id, item.OriginalName, "species", "", "");
                        writtenCount++;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            var outputManager = new OutputDataManager();
            string ns = datasetId.ToString();
            string title = ProgressHelper.GenMatchingFileName(false, datasetId, stepId, false);

            string filepath = outputManager.GenerateAsciiFile(ns, dt, title, "text/csv", dataStructureId);

            if (!System.IO.File.Exists(filepath)) return (null, 0);
            return (filepath, writtenCount);
        }

        // Read a ChecklistBank matching CSV file and map rows to CLBMatchingResultFile objects
        public static List<CLBMatchingResultFile> ReadClbMatchingResultFile(string filepath)
        {
            var result = new List<CLBMatchingResultFile>();
            
            try
            {
                if (string.IsNullOrWhiteSpace(filepath) || !System.IO.File.Exists(filepath)) return result;

                using (var sr = new StreamReader(filepath, Encoding.UTF8))
                {
                    string headerLine = sr.ReadLine();
                    if (headerLine == null) return result;

                    // parse header columns
                    var headers = ParseCsvLine(headerLine).Select(h => h?.Trim()).ToList();
                    // map header name (case-insensitive) to index
                    var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < headers.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(headers[i]) && !headerIndex.ContainsKey(headers[i]))
                        {
                            headerIndex[headers[i]] = i;
                        }
                    }

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var fields = ParseCsvLine(line);

                        string GetField(string name)
                        {
                            if (!headerIndex.TryGetValue(name, out int idx)) return string.Empty;
                            if (idx < 0 || idx >= fields.Count) return string.Empty;
                            var v = fields[idx];
                            return string.IsNullOrEmpty(v) ? string.Empty : v;
                        }

                        var entry = new CLBMatchingResultFile
                        {
                            Original_ID = GetField("Original_ID"),
                            Original_scientificName = GetField("Original_scientificName"),
                            Original_rank = GetField("Original_rank"),
                            Original_kingdom = GetField("Original_kingdom"),
                            Original_authorship = GetField("Original_authorship"),
                            MatchType = GetField("MatchType"),
                            MatchIssues = GetField("MatchIssues"),
                            ID = GetField("ID"),
                            Rank = GetField("Rank"),
                            ScientificName = GetField("ScientificName"),
                            Authorship = GetField("Authorship"),
                            Status = GetField("Status"),
                            AcceptedID = GetField("AcceptedID"),
                            AcceptedScientificName = GetField("AcceptedScientificName"),
                            AcceptedAuthorship = GetField("AcceptedAuthorship"),
                            Kingdom = GetField("Kingdom"),
                            Phylum = GetField("Phylum"),
                            Class = GetField("Class"),
                            Order = GetField("Order"),
                            Family = GetField("Family"),
                            Genus = GetField("Genus"),
                            Classification = GetField("Classification")
                        };

                        result.Add(entry);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public static bool AcceptClbMatches(long datasetId, int stepId, HashSet<long> acceptedIds)
        {
            var filepath = ProgressHelper.GetMatchedFilepath(datasetId, stepId);
            if (filepath == null) return false;

            try
            {
                using (var sr = new StreamReader(filepath, Encoding.UTF8))
                {
                    string headerLine = sr.ReadLine();
                    if (headerLine == null) return false;

                    // parse header columns
                    var headers = ParseCsvLine(headerLine).Select(h => h?.Trim()).ToList();
                    // map header name (case-insensitive) to index
                    var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < headers.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(headers[i]) && !headerIndex.ContainsKey(headers[i]))
                        {
                            headerIndex[headers[i]] = i;
                        }
                    }

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var fields = ParseCsvLine(line);

                        string GetField(string name)
                        {
                            if (!headerIndex.TryGetValue(name, out int idx)) return string.Empty;
                            if (idx < 0 || idx >= fields.Count) return string.Empty;
                            var v = fields[idx];
                            return string.IsNullOrEmpty(v) ? string.Empty : v;
                        }

                        var original_id = GetField("Original_ID");
                        if (acceptedIds.Contains(long.Parse(original_id)))
                        {
                            // query for the SpeciesMatchingResult with this Original_ID and mark it as confirmed
                            
                        }

                        var entry = new CLBMatchingResultFile
                        {
                            Original_ID = GetField("Original_ID"),
                            Original_scientificName = GetField("Original_scientificName"),
                            Original_rank = GetField("Original_rank"),
                            Original_kingdom = GetField("Original_kingdom"),
                            Original_authorship = GetField("Original_authorship"),
                            MatchType = GetField("MatchType"),
                            MatchIssues = GetField("MatchIssues"),
                            ID = GetField("ID"),
                            Rank = GetField("Rank"),
                            ScientificName = GetField("ScientificName"),
                            Authorship = GetField("Authorship"),
                            Status = GetField("Status"),
                            AcceptedID = GetField("AcceptedID"),
                            AcceptedScientificName = GetField("AcceptedScientificName"),
                            AcceptedAuthorship = GetField("AcceptedAuthorship"),
                            Kingdom = GetField("Kingdom"),
                            Phylum = GetField("Phylum"),
                            Class = GetField("Class"),
                            Order = GetField("Order"),
                            Family = GetField("Family"),
                            Genus = GetField("Genus"),
                            Classification = GetField("Classification")
                        };


                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Simple CSV line parser that handles quoted fields and commas inside quotes.
        public static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            if (line == null) return fields;

            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // escaped quote
                        sb.Append('"');
                        i++; // skip next
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }

            fields.Add(sb.ToString());
            return fields;
        }

    }
}