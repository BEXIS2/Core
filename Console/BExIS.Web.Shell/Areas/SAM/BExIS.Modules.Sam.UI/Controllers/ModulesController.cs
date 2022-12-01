using BExIS.Modules.Sam.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class ModulesController : Controller
    {
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
            foreach (var catalogEntry in q)
            {
                if (catalogEntry != null)
                {
                    ModuleGridRowModel row = new ModuleGridRowModel()
                    {
                        // attributes are fetched from the catalog as well as the module's manifest
                        Id = catalogEntry.Attribute("id").Value,
                        Status = catalogEntry.Attribute("status").Value,
                        Order = int.Parse(catalogEntry.Attribute("order").Value),

                        // defulat values for the other attibutes
                        Description = "Not Loaded",
                        Version = "Not Loaded",
                        Loaded = false,
                    };
                    ModuleManifest manifest = ModuleManager.GetModuleManifest(catalogEntry.Attribute("id").Value);
                    if (manifest != null)
                    {
                        row.Description = manifest.Description;
                        row.Version = manifest.Version;
                        row.Loaded = ModuleManager.IsLoaded(row.Id);
                    }

                    modules.Add(row);
                }
            }

            return View(new GridModel<ModuleGridRowModel> { Data = modules });
        }

        [HttpPost]
        public void Delete(string id)
        {
        }

        public ActionResult Edit(string id)
        {
            return PartialView("_Edit", null);
        }

        [HttpPost]
        public ActionResult Edit(object model)
        {
            return Json(new { success = true });
        }

        public ActionResult Details(string id)
        {
            return View();
        }

        public ActionResult Activate(string id)
        {
            ModuleManager.Enable(id);
            return View("Index");
        }

        public ActionResult Inactivate(string id)
        {
            ModuleManager.Disable(id);
            return View("Index");
        }

        //public ActionResult Install()
        //{
        //    return PartialView("_Create", null);
        //}

        /// <summary>
        /// Installs a module from its zipped bundle.
        /// </summary>
        /// <param name="moduleZip">A zipped bundle containg the binaries, views, workspace, and manifest.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Install(HttpPostedFileBase moduleZip)
        {
            try
            {
                //#if DEBUG
                //                // this function writes into the Areas folder and may overwrite the module's resources!
                //                // passing false, prevents it from copying the module's resources. Only the catalog is updated.
                //                ModuleManager.Install(moduleZip, false);
                //#else
                //                // Installs the bundle for production.
                //                ModuleManager.Install(moduleZip, true);
                //#endif
                ModuleManager.Install(moduleZip, true);
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError(string.Empty, ex.Message);
            }
            finally // in any case clean up the installing folder and whatever else...
            {
                /// Prepare and communicate messages to the user
                if (this.ModelState.IsValid)
                {
                    // send a success message back. explain what should be done next.
                }
            }
            // redirect back to the index action to show the form once again and the list of installed modules
            // it also carries model errors; they should be shown on top of the submit area.
            return View("Index");
        }

        public ActionResult Uninstall(string id)
        {
            return View();
        }

        // Move this to Viona.Utils

        public ActionResult Download(string id)
        {
            return View();
        }
    }
}