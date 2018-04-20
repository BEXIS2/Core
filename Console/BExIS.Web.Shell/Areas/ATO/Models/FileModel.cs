using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Ato.UI.Models
{
    public class FileModel
    {
        public string Header { get; set; }
        public string InfoText { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        
        public FileModel()
        {
            Header = "";
            InfoText = "";
            Name = "";
            FileName = "";
            
        }
    }
}