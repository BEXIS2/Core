using BExIS.Security.Entities.Requests;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.DDM.UI.Models
{
    public class RequestGridRowModel
    {
        public RequestGridRowModel()
        {
            DecisionStatuses = new List<DecisionStatus>();
        }

        public List<DecisionStatus> DecisionStatuses { get; set; }
        public long Id { get; set; }
        public string RequestStatus { get; set; }

        public string Rights { get; set; }

        public string Title { get; set; }
        public string Intention { get; set; }
        public long InstanceId { get; set; }
        public DateTime RequestDate { get; set; }
    }
}