using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.MetadataStructure;

namespace BExIS.Modules.Dim.UI.Models
{
    public class AdminModel
    {
        public Dictionary<long,string> MetadataStructuresDic { get; set; }

        public AdminModel()
        {
            MetadataStructuresDic = new Dictionary<long, string>();
        }

        public void Add(MetadataStructure metadataStructure)
        {
            this.MetadataStructuresDic.Add(metadataStructure.Id,
                                         metadataStructure.Name);
        }
    }
}