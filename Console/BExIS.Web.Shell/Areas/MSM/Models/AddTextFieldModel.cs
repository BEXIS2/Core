using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.MSM.Models
{
    public class AddTextFieldModel
    {
        public String Name { get; set; }
        public String XPath { get; set; }
        public AddTextFieldModel() {
        }
        public AddTextFieldModel(string name, string xPath) {
            Name = name;
            XPath = xPath;
        }
    }
}