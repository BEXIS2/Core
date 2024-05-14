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
        public long DatasetVersionId { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset LastModificationDate { get; set; }
        public string Response { get; set; }
    }

    public class ReadGridRowSubmissionModel
    {
        public SubmissionStatus Status { get; set; }
        public long DatasetVersionId { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset LastModificationDate { get; set; }
        public string Response { get; set; }

        public static ReadGridRowSubmissionModel Convert(Submission submission)
        {
            return new ReadGridRowSubmissionModel()
            {
                Status = submission.Status,
                DatasetVersionId = submission.DatasetVersion.Id,
                CreationDate = submission.CreationDate,
                LastModificationDate = submission.LastModificationDate,
                Response = submission.Response
            };
        }
    }

    public class CreateSubmissionModel
    {
        public SubmissionStatus Status { get; set; }
    }

    public class UpdateSubmissionModel
    {

    }
}