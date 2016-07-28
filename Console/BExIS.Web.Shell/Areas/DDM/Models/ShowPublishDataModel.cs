using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dim.Entities;

namespace BExIS.Web.Shell.Areas.DDM.Models
{
    public class ShowPublishDataModel
    {
        public List<DataRepository> DataRepositories;
        public long DatasetId;

        public List<publishedFileModel> RepoFilesDictionary;

        public ShowPublishDataModel()
        {
            DataRepositories = new List<DataRepository>();
            RepoFilesDictionary = new List<publishedFileModel>();
            DatasetId = 0;
        }

    }

    public class publishedFileModel
    {
        public long DatasetId { get; set; }
        public long DatasetVersionId { get; set; }
        public DateTime CreationDate { get; set; }
        public DataRepository DataRepository { get; set; }
    }

    public class DataRepoRequirentModel
    {
        public long DatasetId;
        public long DatasetVersionId;
        public DataRepository DataRepository;
        public bool Exist;
        public bool IsMetadataConvertable { get; set; }
        public bool IsDataConvertable { get; set; }

        public DataRepoRequirentModel()
        {
            IsMetadataConvertable = false;
            IsDataConvertable = false;
            DatasetId = 0;
            DatasetVersionId = 0;
            Exist = false;
            DataRepository = new DataRepository();
        }
    }
}