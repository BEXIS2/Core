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
        Rejected = 2
    }

    public abstract class Request : BaseEntity
    {
        public Request()
        {
            Decisions = new List<Decision>();
        }

        public virtual ICollection<Decision> Decisions { get; set; }
        public virtual DateTime RequestDate { get; set; }
        public virtual User Requester { get; set; }
        public virtual RequestStatus Status { get; set; }
    }
}