using BExIS.Security.Entities.Subjects;
using System;
using Vaiona.Entities.Common;

namespace BExIS.Security.Entities.Requests
{
    public enum DecisionStatus
    {
        Open = 0,
        Accepted = 1,
        Rejected = 2,
        Withdrawn = 3
    }

    public class Decision : BaseEntity
    {
        public virtual DateTime DecisionDate { get; set; }
        public virtual User DecisionMaker { get; set; }
        public virtual string Reason { get; set; }
        public virtual Request Request { get; set; }
        public virtual DecisionStatus Status { get; set; }
    }
}