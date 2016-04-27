using BExIS.IO.Transform.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.Site.Models
{
    public class ShowFooterModel
    {
        public string Description { get; set; }

        public ShowFooterModel()
        {
            Description = ".";
        }
    }
}