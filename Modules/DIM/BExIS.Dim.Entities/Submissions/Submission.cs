using BExIS.Dlm.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Submissions
{
    public class Submission : BaseEntity
    {
        public Agent Agent { get; set; }
        public DatasetVersion DatasetVersion { get; set; }
        public SubmissionStatus Status { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset LastModificationDate { get; set; }
    }

    public enum SubmissionStatus
    {
        Pending,
        Denied,
        Accepted
    }
}
