using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Smm.UI.Models
{
    public class TailorEdit
    {
        [Range(1, long.MaxValue, ErrorMessage = "Id must be provided and greater than 0.")]
        public long Id { get; set; }

        public string OriginalName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string EditedName { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string CleanedName { get; set; }
    }
}