using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Create
{
    public class CreateModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<MetadataInputField> InputFields { get; set; }

        public List<string> Datastructures { get; set; }

        public List<string> FileTypes { get; set; }

        public CreateModel()
        {
            Id = 0;
            Name = "";
            Description = "";
            InputFields = new List<MetadataInputField>();
            Datastructures = new List<string>();
            FileTypes = new List<string>();
        }
    }

    public class MetadataInputField
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public MetadataInputField()
        {
            Index = 0;
            Type = "";
            Value = "";
            Name = "";
        }
    }
}