﻿using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dim.UI.Models
{
    public class ShowPublishDataModel
    {
        public List<string> Brokers;
        public long DatasetId;
        public bool EditRights;
        public bool DownloadRights;
        public bool MetadataIsValid;

        public List<PublicationModel> Publications;

        public ShowPublishDataModel()
        {
            Brokers = new List<string>();
            Publications = new List<PublicationModel>();
            DatasetId = 0;
            EditRights = false;
            DownloadRights = false;
            MetadataIsValid = false;
        }
    }

    public class PublicationModel
    {
        public long DatasetVersionId { get; set; }
        public int DatasetVersionNr { get; set; }

        public BrokerModel Broker { get; set; }
        public string DataRepo { get; set; }
        public string FilePath { get; set; }
        public string ExternalLink { get; set; }
        public string Status { get; set; }
        public List<string> DataRepos { get; set; }
        public DateTime CreationDate { get; set; }

        public long DatasetId { get; set; }
    }

    public class BrokerModel
    {
        public string Name { get; set; }
        public List<string> DataRepos { get; set; }
        public string Link { get; set; }

        public BrokerModel()
        {
            Name = "";
            DataRepos = new List<string>();
            Link = "";
        }

        public BrokerModel(string name, List<string> datarepos, string link)
        {
            Name = name;
            DataRepos = datarepos;
            Link = link;
        }
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