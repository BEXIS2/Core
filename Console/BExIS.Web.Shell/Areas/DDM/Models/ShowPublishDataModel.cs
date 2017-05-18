using System;
using System.Collections.Generic;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class ShowPublishDataModel
    {
        public List<string> Brokers;
        public long DatasetId;
        public bool EditRights;
        public bool DownloadRights;

        public List<PublicationModel> Publications;

        public ShowPublishDataModel()
        {
            Brokers = new List<string>();
            Publications = new List<PublicationModel>();
            DatasetId = 0;
            EditRights = false;
            DownloadRights = false;
        }

    }

    public class PublicationModel
    {
        public long DatasetVersionId { get; set; }

        public string Broker { get; set; }
        public string FilePath { get; set; }
        public string ExternalLink { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class DataRepoRequirentModel
    {
        public long DatasetId;
        public long DatasetVersionId;
        public bool Exist;
        public bool IsMetadataConvertable { get; set; }
        public string metadataValidMessage { get; set; }
        public bool IsDataConvertable { get; set; }

        public DataRepoRequirentModel()
        {
            IsMetadataConvertable = false;
            metadataValidMessage = "";
            IsDataConvertable = false;
            DatasetId = 0;
            DatasetVersionId = 0;
            Exist = false;
        }
    }
}