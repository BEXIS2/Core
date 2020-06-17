using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Requests
{
    public enum RequestStatus
    {
        Open = 0,
        Accepted = 1,
        Rejected = 2,
        Withdrawn = 3,
    }

    public class Request : BaseEntity
    {
        public Request()
        {
            Decisions = new List<Decision>();
        }

        public virtual User Applicant { get; set; }
        public virtual ICollection<Decision> Decisions { get; set; }
        public virtual Entity Entity { get; set; }
        public virtual string Intention { get; set; }
        public virtual long Key { get; set; }
        public virtual DateTime RequestDate { get; set; }
        public virtual short Rights { get; set; }
        public virtual RequestStatus Status { get; set; }
    }
}