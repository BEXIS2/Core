using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.SAM.Models
{
    public class TenantCreateModel
    {
        public IEnumerable<HttpPostedFileBase> Files { get; set; }

        public TenantCreateModel()
        {
            Files = new List<HttpPostedFileBase>();
        }
    }

    public class TenantViewModel
    {
        
    }

    public class TenantGridRowModel
    {
        
    }
}