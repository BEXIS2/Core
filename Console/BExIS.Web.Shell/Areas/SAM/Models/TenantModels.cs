using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Vaiona.Model.MTnt;

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
        public string Id { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        [Display(Name = "Is Default")]
        public bool IsDefault { get; set; }
        public bool IsDeletable { get; set; }
        [Display(Name = "Is Active")]
        public bool Status { get; set; }

        public static TenantGridRowModel Convert(Tenant tenant, bool isDeletable)
        {
            return new TenantGridRowModel()
            {
                Id = tenant.Id,
                ShortName = tenant.ShortName,
                Description = tenant.Description,
                IsDefault = tenant.IsDefault,
                IsDeletable = isDeletable,
                Status = tenant.Status == TenantStatus.Active
            };
        }
    }

    public class TenantEditModel
    {
        public string Id { get; set; }

        [Display(Name = "Short Name")]
        public string ShortName { get; set; }

        public string Description { get; set; }

        [Display(Name = "Is Default")]
        public bool IsDefault { get; set; }

        [Display(Name="Is Active")]
        public bool Status { get; set; }

        public static TenantEditModel Convert(Tenant tenant)
        {
            return new TenantEditModel()
            {
                Id = tenant.Id,
                ShortName = tenant.ShortName,
                IsDefault = tenant.IsDefault,
                Description = tenant.Description,
                Status = tenant.Status == TenantStatus.Active
            };
        }
    }
}