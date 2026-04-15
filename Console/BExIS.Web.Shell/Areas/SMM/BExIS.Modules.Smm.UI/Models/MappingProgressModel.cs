using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Smm.UI.Models
{
    public class MappingProgressModel
    {

        public List<StepEntry> Steps { get; set; } = new List<StepEntry>();

        public int NumRowsGlobal { get; set; }

        public long DatasetId { get; set; }

        public long VersionId { get; set; }

        public int GetNewId()
        {
            return Steps.Count;
        }

        public void AddStep(int id, int numRows, string inputFileName)
        {
            var entry = new StepEntry
            {
                Id = id,
                NumRows = numRows,
                InputFileName = inputFileName ?? string.Empty,
                ResultFileName = string.Empty,
                DownloadLink = string.Empty,
                JobKey = string.Empty,
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

        public bool IsIdValidAndMatched(int stepId)
        {
            // Return false when there are no steps
            if (Steps == null || Steps.Count == 0) return false;

            var entry = Steps.FirstOrDefault(s => s.Id == stepId);

            // Valid and matched when the step exists and has a non-empty ResultFileName
            return entry != null && !string.IsNullOrEmpty(entry.ResultFileName);
        }
    }


    public class StepEntry
    {
        public int Id { get; set; }

        public int NumRows { get; set; }

        public string InputFileName { get; set; }

        public string ResultFileName { get; set; }

        public string DownloadLink { get; set; }

        public string JobKey { get; set; }

        public bool Done { get; set; }
    }
}