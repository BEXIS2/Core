using BExIS.Ext.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Web.Shell.Areas.SAM.Models;
using Vaiona.IoC;
using Vaiona.MultiTenancy.Api;

namespace BExIS.Web.Shell.Areas.SAM.Controllers
{
    public class TenantsController : Controller
    {
        // GET: SAM/Tenants
        /// <summary>
        /// List all the registered tenants with thier status, etc.
        /// </summary>
        /// <returns></returns>
        /// <remarks>The one that is currently serving, should be highlighted.</remarks>
        public ActionResult Index()
        {
            ITenantResolver tenantResolver = IoCFactory.Container.Resolve<ITenantResolver>();
            tenantResolver.Load(new BExISTenantPathProvider());

            return View("Tenants", tenantResolver.Manifest);
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
        /// If the tenat is inactive and is not default, deletes it and returns to the list action with updated information
        /// reports any problem, otherwise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>       
        /// <remarks>
        /// 1. It is not allowed to delete an ACTIVE tenant. If needed, the chosen tenant must be inctivated first.
        /// 2. It is not allowed to delete the DEFAULT tenant. If needed, another tenant must be set as the default, before deleting the chosen one.
        /// 3.There MUST at least one active tenant remain in the list after this operation.
        /// </remarks>
        public ActionResult Unregister(string id)
        {
            ITenantRegistrar tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();
            tenantRegistrar.Unregister(id);
            return View();
        }

        /// <summary>
        /// If the tenat is not active, activates it and returns to the list action with updated information
        /// reports any problem, otherwise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Activate(string id)
        {
            ITenantRegistrar tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();
            tenantRegistrar.Activate(id);
            return View();
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
            ITenantRegistrar tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();
            tenantRegistrar.Inactivate(id);
            return View();
        }

        public ActionResult MakeDefault(string id)
        {
            ITenantRegistrar tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();
            tenantRegistrar.MakeDefault(id);
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
            return PartialView("_Create", model);
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

    }
}