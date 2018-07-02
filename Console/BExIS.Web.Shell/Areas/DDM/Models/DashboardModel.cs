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
}