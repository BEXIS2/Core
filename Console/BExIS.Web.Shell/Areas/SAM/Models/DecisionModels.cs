using BExIS.Security.Entities.Requests;

namespace BExIS.Modules.Sam.UI.Models
{
    public class DecisionGridRowModel
    {
        public string Applicant { get; set; }
        public long Id { get; set; }
        public short Rights { get; set; }
        public DecisionStatus Status { get; set; }
    }
}