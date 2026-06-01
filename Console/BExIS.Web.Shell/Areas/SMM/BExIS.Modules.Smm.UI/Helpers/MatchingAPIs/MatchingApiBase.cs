using BExIS.Modules.Smm.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Smm.UI.Helpers.MatchingAPIs
{
    // Abstract base class for matching APIs (file based matching), defining the common interface and properties
    // NOTE: each new implemented (file based) API should inherit from this
    // NOTE: for now, each configuration/implementation change here needs a rebuild
    public abstract class MatchingApiBase
    {
        protected readonly HttpClient _http;

        protected MatchingApiBase(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        // used to provide access and store information on which apiBase has been used e.g. in matchingProgress steps
        // this is not optimal but works for now.
        // NOTE IMPORTANT: once set and used, an Identifier should not be changed because this would invalidate the identifiers stored in json
        public abstract string Identifier { get; }

        public abstract string BaseUrl { get; }

        public abstract HashSet<string> SupportedMatchTypes { get; }

        // Method to perform the matching based on the provided file path
        // (this actually makes the post request to the API and returns the result as a JsonResult)
        public abstract Task<MatchingApiResponse> MatchAsync(long datasetId, long versionId, string filepath, MappingProgressModel mappingProgress);

        // Method to generate the unmatched input file (source file for matching)
        // NOTE: different APIs need different file structure and input format
        public abstract (string FilePath, int RowCount) GenerateInputFile(long datasetId, long dataStructureId, long versionId, int stepId);

        public abstract Task<string> DownloadResultFile(long datasetId, long versionId, int stepId, MappingProgressModel mappingProgress);

        // Method to read the result file and return a list of matching results
        // NOTE: different APIs have different result file structure and output format
        // NOTE: Try to always parse the file into a List of MatchingResultRow
        public abstract List<MatchingResultRow> ReadResultFile(string filepath);

        // Method to iterate result file and accept a subset of results
        // NOTE: to 'accept' means updating the respective SpeciesMatchingResult object in the database
        public abstract bool AcceptMatches(long datasetId, long versionId, int stepId, HashSet<long> acceptedIds);

        public abstract string GenMatchingUrl();

    }
}