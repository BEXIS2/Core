using System.Collections.Generic;

namespace BExIS.Modules.Sam.UI.Models
{
    public class RequestGridRowModel
    {
        public long EntityId { get; set; }
        public long Id { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}