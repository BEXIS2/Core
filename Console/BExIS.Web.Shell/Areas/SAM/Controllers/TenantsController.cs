using BExIS.Ext.Services;
using BExIS.Modules.Sam.UI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Vaiona.IoC;
using Vaiona.Model.MTnt;
using Vaiona.MultiTenancy.Api;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class TenantsController : Controller
    {
        /// <summary>
        /// If the tenat is not active, activates it and returns to the list action with updated information
        /// reports any problem, otherwise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Activate(string id)
        {
            var tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();
            tenantRegistrar.Activate(id);
            return View();
        }

        /// <summary>
        /// Should accept a tenant package in a zip form (or a handle to it),
        /// check its validity,  check for duplication, etc
        /// and install it as a new tenant if not exist, othwerise as an update to the exisiting one.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView("_Create", new TenantCreateModel());
        }

        [HttpPost]
        public ActionResult Create(TenantCreateModel model)
        {
            // The model must contain a zip file.
            // check the zip file for validity
            // copy the zip (as a zip) file into the workspace's temp folder and provide the path
            var zipFolder = "";
            // provide the filename and the folder to the Register function. the file name must be the tenant id
            var tenantId = "the file name";
            var tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();
            tenantRegistrar.Register(tenantId, zipFolder);
            return PartialView("_Create", model);
        }

        [HttpPost]
        public void Delete(string id)
        {
            if (IsDeletable(id))
            {
                var tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();
                tenantRegistrar.Unregister(id);
            }
        }

        // GET: SAM/Tenants/<id>
        /// <summary>
        /// Shows the deatils of the chosen tenant. Indicates if it is: default, active, and current
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(string id)
        {
            return View();
        }

        /// <summary>
        /// Creates a zip downloadable of the specified tenant and pushes it to the client
        /// The zip file has the predefined folder/file structure
        /// </summary>
        /// <param name="id">The ID of the tenat its branding package is requested</param>
        /// <returns></returns>
        public ActionResult Download(string id)
        {
            return View();
        }

        public ActionResult Edit(string id)
        {
            var tenantResolver = IoCFactory.Container.Resolve<ITenantResolver>();
            tenantResolver.Load(new BExISTenantPathProvider());
            var tenant = tenantResolver.Manifest.Where(x => x.Id.Equals(id)).FirstOrDefault();

            return PartialView("_Edit", TenantEditModel.Convert(tenant));
        }

        [HttpPost]
        public ActionResult Edit(TenantEditModel model)
        {
            var tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();

            // Make tenant to be the default!
            if (model.IsDefault)
            {
                tenantRegistrar.MakeDefault(model.Id);
            }

            if (model.Status)
            {
                tenantRegistrar.Activate(model.Id);
            }
            else
            {
                tenantRegistrar.Inactivate(model.Id);
            }

            return Json(new { success = true });
        }

        /// <summary>
        /// If the tenat is active, inactivates it and returns to the list action with updated information
        /// reports any problem, otherwise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>There MUST at least one active tenant remain in the list after this operation.</remarks>
        public ActionResult Inactivate(string id)
        {
            var tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();
            tenantRegistrar.Inactivate(id);
            return View();
        }

        // GET: SAM/Tenants
        /// <summary>
        /// List all the registered tenants with thier status, etc.
        /// </summary>
        /// <returns></returns>
        /// <remarks>The one that is currently serving, should be highlighted.</remarks>
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Tenants", this.Session.GetTenant());
            return View();
        }

        public ActionResult MakeDefault(string id)
        {
            var tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();
            tenantRegistrar.MakeDefault(id);
            return View();
        }

        [GridAction]
        public ActionResult Tenants_Select()
        {
            var tenantResolver = IoCFactory.Container.Resolve<ITenantResolver>();
            tenantResolver.Load(new BExISTenantPathProvider());

            var tenants = new List<TenantGridRowModel>();

            for (var i = 0; i < tenantResolver.Manifest.Count(); i++)
            {
                var t = tenantResolver.Manifest.ElementAt(i);
                tenants.Add(TenantGridRowModel.Convert(t, IsDeletable(t.Id)));
            }

            return View(new GridModel<TenantGridRowModel> { Data = tenants });
        }

        /// <summary>
        /// If the tenat is inactive and is not default, deletes it and returns to the list action with updated information
        /// reports any problem, otherwise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// 1. It is not allowed to delete an ACTIVE tenant. If needed, the chosen tenant must be inctivated first.
        /// 2. It is not allowed to delete the DEFAULT tenant. If needed, another tenant must be set as the default, before deleting the chosen one.
        /// 3.There MUST at least one active tenant remaining in the list after this operation.
        /// </remarks>
        public ActionResult Unregister(string id)
        {
            var tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();
            tenantRegistrar.Unregister(id);
            return View();
        }

        private bool IsDeletable(string id)
        {
            var tenantResolver = IoCFactory.Container.Resolve<ITenantResolver>();
            tenantResolver.Load(new BExISTenantPathProvider());

            // Get tenant
            var tenant = tenantResolver.Manifest.Where(x => x.Id.Equals(id)).FirstOrDefault();

            if (!tenant.IsDefault && tenant.Status == TenantStatus.Inactive)
            {
                // Get all tenants
                var tenants = tenantResolver.Manifest;

                if (tenants.Select(x => x.IsDefault || x.Status == TenantStatus.Active && x.Id != id).Count() >= 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}