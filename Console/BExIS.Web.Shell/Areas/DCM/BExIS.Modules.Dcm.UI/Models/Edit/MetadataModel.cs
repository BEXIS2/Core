using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;

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