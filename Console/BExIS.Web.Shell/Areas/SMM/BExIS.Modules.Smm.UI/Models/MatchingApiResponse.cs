using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Smm.UI.Models
{
    public class MatchingApiResponse
    {
        public bool Success { get; set; }
        public int? StatusCode { get; set; }
        public string Message { get; set; }
        public object Payload { get; set; }
        public int StepId { get; set; }
    }
}