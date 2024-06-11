namespace BExIS.Modules.Dcm.UI.Models.API
{
    public class DataApiModel
    {
        public long DatasetId { get; set; }

        //public DecimalCharacter DecimalCharacter { get; set; }
        public string[] Columns { get; set; }

        public string[][] Data { get; set; }
    }

    //public class PushDataApiModel : DataApiModel
    //{
    //}

    //public class PutDataApiModel : DataApiModel
    //{
    //    public string[] PrimaryKeys { get; set; }
    //}

    public class PostApiDatasetModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public long DataStructureId { get; set; }
        public long MetadataStructureId { get; set; }
        public long EntityTemplateId { get; set; }
    }
}