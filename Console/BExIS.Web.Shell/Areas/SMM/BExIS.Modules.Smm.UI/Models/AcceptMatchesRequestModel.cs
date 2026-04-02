using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Smm.UI.Models
{
    public class AcceptMatchesRequestModel
    {
        [Range(1, long.MaxValue, ErrorMessage = "DatasetId must be provided and greater than 0.")]
        public long DatasetId { get; set; }

        [Required(ErrorMessage = "At least one MatchId must be provided.")]
        [MinLength(1, ErrorMessage = "At least one MatchId must be provided.")]
        public List<long> MatchIds { get; set; }
    }
}