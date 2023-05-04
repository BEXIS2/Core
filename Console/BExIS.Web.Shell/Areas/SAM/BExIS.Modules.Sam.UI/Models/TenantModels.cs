using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Vaiona.Model.MTnt;

namespace BExIS.Modules.Sam.UI.Models
{
    public class TenantCreateModel
    {
        public TenantCreateModel()
        {
            Files = new List<HttpPostedFileBase>();
        }

        public IEnumerable<HttpPostedFileBase> Files { get; set; }
    }

    public class TenantEditModel
    {
        public string Description { get; set; }
        public string Id { get; set; }

        [Display(Name = "Is Default")]
        public bool IsDefault { get; set; }

        [Display(Name = "Short Name")]
        public string ShortName { get; set; }

        [Display(Name = "Is Active")]
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

    public class TenantGridRowModel
    {
        public string Description { get; set; }
        public string Id { get; set; }

        [Display(Name = "Is Default")]
        public bool IsDefault { get; set; }

        public bool IsDeletable { get; set; }
        public string ShortName { get; set; }

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

    public class TenantViewModel
    {
    }
}