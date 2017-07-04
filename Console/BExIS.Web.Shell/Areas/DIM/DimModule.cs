using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using BExIS.Modules.Dim.UI.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dim.UI
{
    public class DimModule : ModuleBase
    {
        public DimModule(): base("dim")
        {
        }

        public override void Start()
        {
            base.Start();
            if (context != null && context.State != null)
            {
                HttpConfiguration config = (HttpConfiguration)this.context.State;
                //config.Formatters.Insert(0, new DatasetModelCsvFormatter()); // should also work
                config.Formatters.Insert(0, new DatasetModelCsvFormatter(new QueryStringMapping("format", "csv", "text/csv")));
            }
        }
    }
}
