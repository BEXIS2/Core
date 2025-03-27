using System.Collections.Generic;

namespace BExIS.Modules.Dim.UI.Models
{
    public class AdminModel
    {
        public Dictionary<long, string> MetadataStructuresDic { get; set; }

        public AdminModel()
        {
            MetadataStructuresDic = new Dictionary<long, string>();
        }

        public void Add(long metadataStructureId, string metadataStrutcureName)
        {
            this.MetadataStructuresDic.Add(metadataStructureId,
                                         metadataStrutcureName);
        }
    }
}