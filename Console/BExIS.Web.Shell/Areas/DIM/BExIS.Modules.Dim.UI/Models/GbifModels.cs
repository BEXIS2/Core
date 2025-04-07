using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dim.UI.Models.Export
{
    public class GbifPublicationModel
    {
        public long PublicationId { get; set; }
        public long DatasetId { get; set; }
        public long DatasetVersionId { get; set; }
        public long DatasetVersionNr { get; set; }
        public long BrokerRef { get; set; }
        public long RepositoryRef { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Response { get; set; }
        public string Link { get; set; }
        public string Type { get; set; }
    }
}