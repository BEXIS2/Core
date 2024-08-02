using System;

namespace BExIS.Modules.Dcm.UI.Models.Edit
{
    public class MetadataModel
    {
        public bool UseExternalMetadataForm { get; set; }

        public String ExternalMetadataFormUrl { get; set; }

        public MetadataModel()
        {
        }
    }
}