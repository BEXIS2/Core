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

        [Range(1, long.MaxValue, ErrorMessage = "VersionId must be provided and greater than 0.")]
        public long VersionId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "StepId must be provided and at least 0.")]
        public int StepId { get; set; }

        [Required(ErrorMessage = "At least one MatchId must be provided.")]
        [MinLength(1, ErrorMessage = "At least one MatchId must be provided.")]
        // Use an array here so MinLengthAttribute can validate the collection correctly during model binding
        public string[] MatchIds { get; set; }
    }
}