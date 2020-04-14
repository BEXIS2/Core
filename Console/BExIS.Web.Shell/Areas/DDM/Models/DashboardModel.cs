using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Ddm.UI.Models
{
	public class DashboardModel
	{
		public Dictionary<long,string> Entities { get; set; }
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

        public MyDatasetsModel(long id, string title, string description, bool isOwn, string isValid)
        {
            Id = id;
            Title = title;
            Description = description;
            IsOwn = isOwn;
            IsValid = isValid;
        }

    }
}