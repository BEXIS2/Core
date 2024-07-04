using BExIS.Security.Entities.Requests;
using System;
using System.ComponentModel;

namespace BExIS.Modules.DDM.UI.Models
{
    public class DecisionGridRowModel
    {
        public string Applicant { get; set; }
        public string Intention { get; set; }
        public long EntityId { get; set; }
        public long InstanceId { get; set; }
        public long Id { get; set; }
        public long Key { get; set; }

        public bool EntityExist { get; set; }

        public long RequestId { get; set; }

        public string Rights { get; set; }

        public string Title { get; set; }
        public DecisionStatus Status { get; set; }

        [DisplayName("Status")]
        public string StatusAsText { get; set; }

        public DateTime DecisionDate { get; set; }
        public DateTime RequestDate { get; set; }
        public string DecisionMaker { get; internal set; }
    }
}