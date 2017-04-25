using BExIS.Modules.Sam.UI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class ModulesController : Controller
    {
        public ActionResult Activate(string id)
        {
            ModuleManager.Enable(id);
            return View("Index");
        }

        public ActionResult Create()
        {
            return PartialView("_Create", null);
        }

        [HttpPost]
        public ActionResult Create(object model)
        {
            return PartialView("_Create", model);
        }

        [HttpPost]
        public void Delete(string id)
        {
        }

        public ActionResult Details(string id)
        {
            return View();
        }

        public ActionResult Download(string id)
        {
            return View();
        }

        public ActionResult Edit(string id)
        {
            return PartialView("_Edit", null);
        }

        //    return false;
        //}
        [HttpPost]
        public ActionResult Edit(object model)
        {
            return Json(new { success = true });
        }

        public ActionResult Inactivate(string id)
        {
            ModuleManager.Disable(id);
            return View("Index");
        }

        // GET: SAM/Tenants
        /// <summary>
        /// List all the registered tenants with thier status, etc.
        /// </summary>
        /// <returns></returns>
        /// <remarks>The one that is currently serving, should be highlighted.</remarks>
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Modules", this.Session.GetTenant());
            return View();
        }

        [GridAction]
        public ActionResult Modules_Select()
        {
            // load
            List<ModuleGridRowModel> modules = new List<ModuleGridRowModel>();
            var q = ModuleManager.Catalog.Elements("Module")
                        .OrderBy(p => int.Parse(p.Attribute("order").Value));
            foreach (var item in q)
            {
                ModuleGridRowModel row = new ModuleGridRowModel()
                {
                    Id = item.Attribute("id").Value,
                    Status = ModuleManager.IsActive(item),
                };
                var moduleInfo = ModuleManager.GetModuleInfo(row.Id);
                //pm.ModuleInfos
                //.Where(p => p.Manifest.Name.Equals(row.Id, StringComparison.InvariantCultureIgnoreCase))
                //.First();
                row.Description = moduleInfo.Manifest.Description;
                row.Version = moduleInfo.Manifest.Version;
                row.Loaded = moduleInfo.Plugin != null;
                row.Order = int.Parse(item.Attribute("order").Value);
                modules.Add(row);
            }

            return View(new GridModel<ModuleGridRowModel> { Data = modules });
        }

        //private bool IsDeletable(string id)
        //{
        //    ITenantResolver tenantResolver = IoCFactory.Container.Resolve<ITenantResolver>();
        //    tenantResolver.Load(new BExISTenantPathProvider());

        //    // Get tenant
        //    Tenant tenant = tenantResolver.Manifest.Where(x => x.Id.Equals(id)).FirstOrDefault();

        //    if (!tenant.IsDefault && tenant.Status == TenantStatus.Inactive)
        //    {
        //        // Get all tenants
        //        List<Tenant> tenants = tenantResolver.Manifest;

        //        if (tenants.Select(x => x.IsDefault || x.Status == TenantStatus.Active && x.Id != id).Count() >= 1)
        //        {
        //            return true;
        //        }
        //    }
        public ActionResult Unregister(string id)
        {
            return View();
        }
    }
}