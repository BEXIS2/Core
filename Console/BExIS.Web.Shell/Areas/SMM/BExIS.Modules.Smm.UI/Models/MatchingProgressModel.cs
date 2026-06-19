using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Smm.UI.Models
{
    public class MatchingProgressModel
    {

        public List<StepEntry> Steps { get; set; } = new List<StepEntry>();

        // total number of rows in the original data, should be set at the beginning of the matching process
        public int NumRowsGlobal { get; set; }

        // identifier for the dataset being matched, should be set at the beginning of the matching process
        public long DatasetId { get; set; }

        // identifier for the specific version of the dataset being matched, should be set at the beginning of the matching process
        public long VersionId { get; set; }

        public int GetNewId()
        {
            return Steps.Count;
        }

        public StepEntry GetLatestStep()
        {
            // Return the last step in the list or null when there are no steps
            if (Steps == null || Steps.Count == 0) return null;

            return Steps.Last();
        }

        public void AddStep(int id, int numRows, string inputFileName, string apiIdentifier)
        {
            var entry = new StepEntry
            {
                Id = id,
                NumRows = numRows,
                InputFileName = inputFileName,
                ResultFileName = string.Empty,
                ApiIdentifier = apiIdentifier,
                DownloadLink = string.Empty,
                JobKey = string.Empty,
                MatchSource = string.Empty,
                TimeStamp = DateTime.MinValue,
                Done = false
            };

            Steps.Add(entry);
        }

        public bool AreAllStepsDone()
        {
            // Return true when there are no unfinished steps (i.e. no step with Done == false)
            return Steps == null || Steps.All(s => s.Done);
        }

        public string GetNextPendingInputFileName()
        {
            if (Steps == null || Steps.Count == 0) return null;

            var entry = Steps.FirstOrDefault(s => s.Done == false
                                                 && string.IsNullOrEmpty(s.DownloadLink)
                                                 && string.IsNullOrEmpty(s.JobKey));

            return entry?.InputFileName;
        }

        public StepEntry GetNextPendingStepEntry()
        {
            if (Steps == null || Steps.Count == 0) return null;

            var entry = Steps.FirstOrDefault(s => s.Done == false
                                                 && string.IsNullOrEmpty(s.DownloadLink)
                                                 && string.IsNullOrEmpty(s.JobKey));

            return entry;
        }

        public bool IsIdValidAndMatched(int stepId)
        {
            // Return false when there are no steps
            if (Steps == null || Steps.Count == 0) return false;

            var entry = Steps.FirstOrDefault(s => s.Id == stepId);

            // Valid and matched when the step exists and has a non-empty ResultFileName
            return entry != null && !string.IsNullOrEmpty(entry.ResultFileName);
        }

        public string GetApiIdentifier(int stepId)
        {
            // Return null when there are no steps
            if (Steps == null || Steps.Count == 0) return null;

            var entry = Steps.FirstOrDefault(s => s.Id == stepId);

            return entry?.ApiIdentifier;
        }

        public StepEntry GetStepById(int stepId)
        {
            if (Steps == null || Steps.Count == 0) return null;

            return Steps.FirstOrDefault(s => s.Id == stepId);
        }

        public bool UpdateStep(StepEntry updatedStep)
        {
            // Validate input and existing steps
            if (updatedStep == null) return false;
            if (Steps == null || Steps.Count == 0) return false;

            var existing = Steps.FirstOrDefault(s => s.Id == updatedStep.Id);
            if (existing == null) return false;

            // Update fields of the existing entry
            existing.NumRows = updatedStep.NumRows;
            existing.InputFileName = updatedStep.InputFileName;
            existing.ResultFileName = updatedStep.ResultFileName;
            existing.ApiIdentifier = updatedStep.ApiIdentifier;
            existing.DownloadLink = updatedStep.DownloadLink;
            existing.MatchSource = updatedStep.MatchSource;
            existing.JobKey = updatedStep.JobKey;
            existing.Done = updatedStep.Done;

            return true;
        }
    }

    // Represents a single step in the matching process
    // Each step corresponds to a matching operation, which involves an input file, result file and an API call
    // to a file based matching service (e.g. CheckListBank). The step is considered completed when the result file is available and the API call is done.
    public class StepEntry
    {
        // identifier for this step, should be unique within the context of a MatchingProgressModel
        public int Id { get; set; }
        
        // number of rows in the input file
        public int NumRows { get; set; }

        // name of the input file for this step
        public string InputFileName { get; set; }

        // name of the result file for this step, should be non-empty when the step is completed
        public string ResultFileName { get; set; }

        // identifier for the API call associated with this step, should be non-empty when the step is completed
        public string ApiIdentifier { get; set; }

        // download link for the result file
        public string DownloadLink { get; set; }

        // source of the matching results (e.g. string of dataset sourceKey in CheckListBank)
        public string MatchSource { get; set; }

        // timestamp when the match request is sent
        public DateTime TimeStamp { get; set; }

        // job key for tracking the matching job (if asynchronous)
        public string JobKey { get; set; }

        // indicates whether the matching step is completed (completed when the result file is available and the API call is done)
        public bool Done { get; set; }
    }
}