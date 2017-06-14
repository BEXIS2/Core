using BExIS.Security.Entities.Requests;

namespace BExIS.Modules.Sam.UI.Models
{
    public class DecisionGridRowModel
    {
        public string Applicant { get; set; }
        public long Id { get; set; }
        public DecisionStatus Status { get; set; }
        public string StatusName { get; set; }
    }

    public class RequestGridRowModel
    {
        public string Applicant { get; set; }
        public long Id { get; set; }
        public RequestStatus Status { get; set; }
        public string StatusName { get; set; }
    }
}