using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Smm.UI.Models
{
    public class MatchingResultRow
    {
        public string Original_ID { get; set; }
        public string Original_scientificName { get; set; }
        public string Original_rank { get; set; }
        public string Original_kingdom { get; set; }
        public string Original_authorship { get; set; }
        public string MatchType { get; set; }
        public string MatchIssues { get; set; }
        public string ID { get; set; }
        public string Rank { get; set; }
        public string ScientificName { get; set; }
        public string Authorship { get; set; }
        public string Status { get; set; }
        public string AcceptedID { get; set; }
        public string AcceptedScientificName { get; set; }
        public string AcceptedAuthorship { get; set; }
        public string Kingdom { get; set; }
        public string Phylum { get; set; }
        public string Class { get; set; }
        public string Order { get; set; }
        public string Family { get; set; }
        public string Genus { get; set; }
        public string Classification { get; set; }
    }
}