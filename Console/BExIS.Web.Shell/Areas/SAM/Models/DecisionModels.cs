using BExIS.Security.Entities.Requests;

namespace BExIS.Modules.Sam.UI.Models
{
    public class DecisionGridRowModel
    {
        public string Applicant { get; set; }
        public long EntityId { get; set; }
        public long InstanceId { get; set; }
        public long Id { get; set; }
        public long Key { get; set; }
        public long RequestId { get; set; }
        public short Rights { get; set; }
        public string Title { get; set; }
        public DecisionStatus Status { get; set; }
    }
}