using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.Site.Models
{
    public class ContactUsModel
    {
        public string content { get; set; }

        public ContactUsModel()
        {
            content = "";
        }
    }
}