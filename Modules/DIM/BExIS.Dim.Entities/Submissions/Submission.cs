using BExIS.Dim.Entities.Responses;
using BExIS.Dlm.Entities.Data;
using Newtonsoft.Json.Linq;
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
        public virtual Agent Agent { get; set; }
        public virtual DatasetVersion DatasetVersion { get; set; }
        public virtual SubmissionStatus Status { get; set; }
        public virtual DateTimeOffset CreationDate { get; set; }
        public virtual DateTimeOffset LastModificationDate { get; set; }
        public virtual string Response { get; set; }
    }

    public enum SubmissionStatus
    {
        Pending,
        Accepted,
        Rejected
    }
}
