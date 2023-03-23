using BExIS.IO.Transform.Output;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dim.Helpers.Models
{
    public class PanageaMetadata
    {
        public List<string> AuthorIDs { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }
        public List<ReferenceID> ReferenceIDs { get; set; }
        public string ExportFilename { get; set; }
        public string EventLabel { get; set; }
        public List<VariableElement> ParameterIDs { get; set; }
        public List<string> ProjectIDs { get; set; }
        public PangaeaMetadataTopologicType TopologicTypeID { get; set; }
        public PangaeaMetadataStatus StatusID { get; set; }
        public int LoginID { get; set; }

        public PanageaMetadata()
        {
            AuthorIDs = new List<string>();
            Title = "";
            Abstract = "";
            ReferenceIDs = new List<ReferenceID>();
            ExportFilename = "";
            EventLabel = "";
            ParameterIDs = new List<VariableElement>();
            ProjectIDs = new List<string>();
            TopologicTypeID = PangaeaMetadataTopologicType.notGiven;
            StatusID = PangaeaMetadataStatus.NotValidated;
            LoginID = 0;
        }
    }

    public class ReferenceID
    {
        public long ID { get; set; }
        public long IDRelationTypeID { get; set; }
    }

    public enum PangaeaMetadataTopologicType
    {
        notGiven,
        notSpecified
    }

    public enum PangaeaMetadataStatus
    {
        NotValidated,
        Validated,
        Published
    }
}
