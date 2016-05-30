using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Models
{
    public class UiTestModel
    {
        [Required]
        public string Name { get; set; }
        public DataTable DataTable { get; set; }
        public DataTable DataTable2 { get; set; }
    }
}