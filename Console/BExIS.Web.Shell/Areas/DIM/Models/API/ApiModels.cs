using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Modules.Dim.UI.Models.Api
{
    public class ApiSimpleDatasetModel
    {
        public long Id { get; set; }
        public List<ApiSimpleDatasetVersionModel> Versions { get; set; }

        public ApiSimpleDatasetModel()
        {
            Versions = new List<ApiSimpleDatasetVersionModel>();
        }
    }

    public class ApiSimpleDatasetVersionModel
    {
        public long Id { get; set; }
        public long Number { get; set; }
    }

    public class ApiDatasetModel
    {
        public long Id { get; set; }
        public long Version { get; set; }
        public long VersionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long DataStructureId { get; set; }
        public long MetadataStructureId { get; set; }
        public Dictionary<string, string> AdditionalInformations { get; set; }
        public Dictionary<string, Dictionary<string, string>> Parties { get; set; }
        public bool IsPublic { get; set; }
        public string PublicationDate { get; set; }
        public string VersionName { get; set; }
        public bool VersionPublicAccess { get; set; }
        public string VersionPublicAccessDate { get; set; }

        public ApiDatasetModel()
        {
            AdditionalInformations = new Dictionary<string, string>();
            Parties = new Dictionary<string, Dictionary<string, string>>();
        }
    }

    public class ApiDatasetAttachmentsModel
    {
        public long DatasetId { get; set; }
        public List<ApiSimpleAttachmentModel> Attachments { get; set; }

        public ApiDatasetAttachmentsModel()
        {
            DatasetId = 0;
            Attachments = new List<ApiSimpleAttachmentModel>();
        }
    }

    public class ApiSimpleAttachmentModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
    }

    public class ApiDataStatisticModel
    {
        public long VariableId { get; set; }
        public string VariableName { get; set; }
        public string VariableDescription { get; set; }
        public string DataTypeName { get; set; }
        public string DataTypeSystemType { get; set; }
        public string DataTypeDisplayPattern { get; set; }
        public string Unit { get; set; }
        public DataTable uniqueValues { get; set; }
        public string count { get; set; }
        public string max { get; set; }
        public string min { get; set; }
        public string maxLength { get; set; }
        public string minLength { get; set; }
        public DataTable missingValues { get; set; }

    }
}