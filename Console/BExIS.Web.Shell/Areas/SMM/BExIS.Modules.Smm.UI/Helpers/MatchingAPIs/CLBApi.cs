using BExIS.Dim.Entities.Export.GBIF;
using BExIS.Dlm.Entities.SpeciesMatching;
using BExIS.Dlm.Services.SpeciesMatching;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Smm.UI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc.Modularity;


namespace BExIS.Modules.Smm.UI.Helpers.MatchingAPIs
{
    public class CLBApi : MatchingApiBase
    {
        public CLBApi(HttpClient http) : base(http) { }

        public override string Identifier => "CLB";
        
        public override string BaseUrl => "https://api.checklistbank.org/dataset/3LR/match/nameusage/job";

        public override HashSet<string> SupportedMatchTypes => new HashSet<string>
        {
            "ambiguous",
            "exact",
            "none"
        };

        public override string GenMatchingUrl()
        {
            return $"{BaseUrl}?format=csv";
        }

        public override (string FilePath, int RowCount) GenerateInputFile(long datasetId, long dataStructureId, long versionId, int stepId)
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
            // put matching files under Datasets/<datasetId>/Matching/<versionId>/
            string ns = Path.Combine(datasetId.ToString(), "Matching", versionId.ToString());
            string title = ProgressHelper.GenMatchingFileName(false, datasetId, stepId, false);

            string filepath = outputManager.GenerateAsciiFile(ns, dt, title, "text/csv", dataStructureId);

            if (!System.IO.File.Exists(filepath)) return (null, 0);
            return (filepath, writtenCount);
        }

        public override List<MatchingResultRow> ReadResultFile(string filepath)
        {
            var result = new List<MatchingResultRow>();

            try
            {
                if (string.IsNullOrWhiteSpace(filepath) || !System.IO.File.Exists(filepath)) return result;

                using (var sr = new StreamReader(filepath, Encoding.UTF8))
                {
                    string headerLine = sr.ReadLine();
                    if (headerLine == null) return result;

                    // parse header columns
                    var headers = TabularFileHelper.ParseCsvLine(headerLine).Select(h => h?.Trim()).ToList();
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
                        var fields = TabularFileHelper.ParseCsvLine(line);

                        string GetField(string name)
                        {
                            if (!headerIndex.TryGetValue(name, out int idx)) return string.Empty;
                            if (idx < 0 || idx >= fields.Count) return string.Empty;
                            var v = fields[idx];
                            return string.IsNullOrEmpty(v) ? string.Empty : v;
                        }

                        var entry = new MatchingResultRow
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

        public override bool AcceptMatches(long datasetId, long versionId, int stepId, HashSet<long> acceptedIds)
        {

            var filepath = ProgressHelper.GetMatchedFilepath(datasetId, versionId, stepId);
            if (filepath == null) return false;

            using (var speciesMatchingResultManager = new SpeciesMatchingResultManager())
            // Use a regular (stateful) unit of work here so that modified entities are tracked by NHibernate.
            // The bulk unit of work uses a stateless session which does not track changes to entities,
            // therefore modifications made to retrieved objects would not be persisted on Commit.
            using (var uow = speciesMatchingResultManager.GetUnitOfWork())
            using (var sr = new StreamReader(filepath, Encoding.UTF8))
            {
                try
                {
                    string headerLine = sr.ReadLine();
                    if (headerLine == null) return false;

                    // parse header columns
                    var headers = TabularFileHelper.ParseCsvLine(headerLine).Select(h => h?.Trim()).ToList();
                    // map header name (case-insensitive) to index
                    var headerIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < headers.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(headers[i]) && !headerIndex.ContainsKey(headers[i]))
                        {
                            headerIndex[headers[i]] = i;
                        }
                    }

                    // pre-filter the species matching results for this dataset/version so we only operate on this subset
                    var repo = uow.GetRepository<SpeciesMatchingResult>();
                    var subsetIds = repo.Query().Where(r => r.Dataset.Id == datasetId && r.DatasetVersionId == versionId);

                    Debug.WriteLine("Read file header and mapped columns. Now processing lines...");

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var fields = TabularFileHelper.ParseCsvLine(line);

                        string GetField(string name)
                        {
                            if (!headerIndex.TryGetValue(name, out int idx)) return string.Empty;
                            if (idx < 0 || idx >= fields.Count) return string.Empty;
                            var v = fields[idx];
                            return string.IsNullOrEmpty(v) ? string.Empty : v;
                        }

                        var original_id = GetField("Original_ID");

                        var entry = new CLBMatchingResultFile
                        {
                            Original_ID = GetField("Original_ID"),
                            //Original_scientificName = GetField("Original_scientificName"),
                            //Original_rank = GetField("Original_rank"),
                            //Original_kingdom = GetField("Original_kingdom"),
                            //Original_authorship = GetField("Original_authorship"),
                            MatchType = GetField("MatchType"),
                            //MatchIssues = GetField("MatchIssues"),
                            //ID = GetField("ID"),
                            //Rank = GetField("Rank"),
                            ScientificName = GetField("ScientificName"),
                            //Authorship = GetField("Authorship"),
                            Status = GetField("Status"),
                            //AcceptedID = GetField("AcceptedID"),
                            //AcceptedScientificName = GetField("AcceptedScientificName"),
                            //AcceptedAuthorship = GetField("AcceptedAuthorship"),
                            //Kingdom = GetField("Kingdom"),
                            //Phylum = GetField("Phylum"),
                            //Class = GetField("Class"),
                            //Order = GetField("Order"),
                            //Family = GetField("Family"),
                            //Genus = GetField("Genus"),
                            //Classification = GetField("Classification")
                        };

                        if (acceptedIds.Contains(long.Parse(original_id)))
                        {
                            // query for the SpeciesMatchingResult with this Original_ID and mark it as confirmed
                            var result = subsetIds.FirstOrDefault(r => r.Id == long.Parse(original_id));
                            if (result != null)
                            {
                                result.ConfirmedByUser = true;
                                result.MatchedName = entry.ScientificName;
                                result.MatchType = entry.MatchType;
                                result.Status = entry.Status;
                            }
                        }

                        Debug.WriteLine("Processed line with Original_ID=" + original_id);
                    }

                    Debug.WriteLine("Commiting  results to database...");
                    uow.Commit();
                    Debug.WriteLine("Finished commiting results.");
                    return true;
                }
                catch (Exception ex)
                {
                    uow.Ignore();
                    return false;
                }
            }
        }

        public override async Task<MatchingApiResponse> MatchAsync(long datasetId, long versionId, string filepath, MappingProgressModel mappingProgress)
        {
            if (string.IsNullOrWhiteSpace(filepath) || !System.IO.File.Exists(filepath))
            {
                return await Task.FromResult(new MatchingApiResponse
                {
                    Success = false,
                    StatusCode = null,
                    Message = "Export file not generated.",
                    Payload = null
                });
            }

            Debug.WriteLine("{====================FILEPATH:====================}");
            Debug.WriteLine(filepath);
            byte[] fileBytes = System.IO.File.ReadAllBytes(filepath);


            GBFICrendentials credentials = ModuleManager.GetModuleSettings("DIM").GetValueByKey<GBFICrendentials>("gbifapicredentials");
            var username = credentials.Username;
            var password = credentials.Password;

            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            var url = this.GenMatchingUrl();
            // var url = "https://api.checklistbank.org/dataset/3LR/match/nameusage/job?format=csv";

            using (var content = new ByteArrayContent(fileBytes))
            {
                //content.Headers.ContentType = new MediaTypeHeaderValue("text/tab-separated-values");
                content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Content = content;
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);

                    try
                    {
                        // HttpResponseMessage response = await _http.PostAsync(url, content);
                        var response = await _http.SendAsync(request);
                        string responseString = await response.Content.ReadAsStringAsync();

                        // try to parse the response string as JSON. If parsing fails treat the whole call as a failure
                        // because we cannot interpret the API response reliably.
                        object responseJson;
                        try
                        {
                            responseJson = JsonConvert.DeserializeObject(responseString);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Failed to deserialize ChecklistBank response as JSON: " + ex.Message);
                            Debug.WriteLine("RESPONSE STRING: ");
                            Debug.WriteLine(responseString);
                            return new MatchingApiResponse
                            {
                                Success = false,
                                StatusCode = (int?)response.StatusCode,
                                Message = "Failed to parse API response as JSON.",
                                Payload = responseString
                            };
                        }

                        if (response.IsSuccessStatusCode)
                        {
                            return new MatchingApiResponse
                            {
                                Success = true,
                                StatusCode = (int?)response.StatusCode,
                                Message = null,
                                Payload = responseJson
                            };
                        }
                        else
                        {
                            return new MatchingApiResponse
                            {
                                Success = false,
                                StatusCode = (int?)response.StatusCode,
                                Message = "API returned non-success status.",
                                Payload = responseJson
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        // failure before request sent or other network error: no HTTP status code available
                        return new MatchingApiResponse
                        {
                            Success = false,
                            StatusCode = null,
                            Message = ex.Message,
                            Payload = null
                        };
                    }
                }
            }
        }

    }
}