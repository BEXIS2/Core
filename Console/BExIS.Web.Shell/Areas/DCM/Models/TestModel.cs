using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class TestModel
    {
        [Remote("RemoteTest", "SubmitSpecifyDataset")]
        [Required]
        [StringLength(50, ErrorMessage = "The user name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string UserName { get; set; }
    }
}