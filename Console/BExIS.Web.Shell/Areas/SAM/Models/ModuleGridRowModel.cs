using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Sam.UI.Models
{
    public class ModuleGridRowModel
    {
        [Display(Name = "Order")]
        public int Order { get; set; }

        public string Id { get; set; }
        [Display(Name = "Status")]
        public string Status { get; set; }
        [Display(Name = "Version")]
        public string Version { get; set; }

        [Display(Name = "Is Loaded")]
        public bool Loaded { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }
    }

}