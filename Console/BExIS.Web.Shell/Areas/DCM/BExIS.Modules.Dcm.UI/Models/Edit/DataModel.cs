using BExIS.UI.Hooks.Caches;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Models.Edit
{ 

    public class DataModel
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public bool HasStrutcure { get; set; }
        public List<FileInfo> ExistingFiles { get; set; }
        public List<FileInfo> DeleteFiles { get; set; }

        public DescriptionType DescriptionType { get; set; }

        public DataModel()
        {
            ExistingFiles = new List<FileInfo>();
            DeleteFiles = new List<FileInfo>();
        }

    }


}