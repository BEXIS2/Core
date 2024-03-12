using BExIS.Dim.Entities.Submissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dim.UI.Models.Submissions
{
    public class ReadSubmissionModel
    {
        public SubmissionStatus Status { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset LastModificationDate { get; set; }
        public string AgentName { get; set; }
        public long DatasetVersionId { get; set; }
        public long Id { get; set; }

        public static ReadSubmissionModel Convert(Submission submission)
        {
            return new ReadSubmissionModel()
            {
                CreationDate = submission.CreationDate,
                LastModificationDate = submission.LastModificationDate,
                Status = submission.Status,
                DatasetVersionId = submission.DatasetVersion.Id,
                Id = submission.Id,
                AgentName = submission.Agent.Name
            };
        }
    }

    public class WriteSubmissionModel
    {

    }
}