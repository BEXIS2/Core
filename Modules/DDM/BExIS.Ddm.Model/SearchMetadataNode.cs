using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Ddm.Model
{
    public class SearchMetadataNode
    {
        public string MetadataStructureName { get; set; }
        public string XPath { get; set; }
        public string DisplayName { get; set; }

        public SearchMetadataNode(string metadataStructureName, string xPath)
        {
            this.MetadataStructureName = metadataStructureName;
            this.XPath = xPath;
            this.DisplayName = "( " + metadataStructureName + " ) " + xPath;
        }
    }
}
