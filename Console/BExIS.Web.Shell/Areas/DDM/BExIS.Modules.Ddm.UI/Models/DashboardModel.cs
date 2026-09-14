using System.Collections.Generic;
using System.Data;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class DashboardModel
    {
        public Dictionary<long, string> Entities { get; set; }
        public DataTable MyDatasets { get; set; }

        public DashboardModel()
        {
            Entities = new Dictionary<long, string>();
            MyDatasets = new DataTable();
        }
    }
    public class MyDatasetsModel
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsOwn { get; set; }

        public string IsValid { get; set; }
        public string Type { get; set; }
        public bool HasTag { get; set; }
        public double TagNr { get; set; }
        public bool HasData { get; set; }


        public MyDatasetsModel(long id, string title, string description, bool isOwn, string isValid, string type, bool hasTag = false, double tagNr = 0, bool hasData = false)
        {
            Id = id;
            Title = title;
            Description = description;
            IsOwn = isOwn;
            IsValid = isValid;
            Type = type;
            HasTag = hasTag;
            TagNr = tagNr;
            HasData = hasData;
        }
    }
}