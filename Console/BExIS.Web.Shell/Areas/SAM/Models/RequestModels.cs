using BExIS.Security.Entities.Requests;
using System.Collections.Generic;

namespace BExIS.Modules.Sam.UI.Models
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
        public short Rights { get; set; }
        public string Title { get; set; }
        public long InstanceId { get; set; }
    }
}