using BExIS.Modules.Sam.UI.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;
using System.Linq;
using System.Web;
using System.IO;
using Vaiona.Utils.Cfg;
using System;

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
        /// <param name="moduleZip">The zipped bundle containg the binaries, views, workspace, and manifest.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Install(HttpPostedFileBase moduleZip)
        {
            string moduleName = "";
            string moduleVersion = "";
            string path = "";
            try
            {
                /// 1: Acquire the bundle, check if the same bundle is installing? if yes, return an error
                if (moduleZip == null || moduleZip.ContentLength <= 0)
                    throw new System.Exception(string.Format("The submited file is not valid. Operation aborted."));

                var fileName = Path.GetFileName(moduleZip.FileName);
                moduleName = fileName.Substring(0, fileName.IndexOf("."));
                moduleVersion = fileName.Substring(fileName.IndexOf(".") + 1, (fileName.LastIndexOf(".") - fileName.IndexOf(".") - 1));

                path = Path.Combine(AppConfiguration.WorkspaceModulesRoot, "installing");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                path = Path.Combine(path, fileName);
                if (System.IO.File.Exists(path)) // the module is being installed from another session
                {
                    throw new Exception(string.Format("Bundle {0} is being installed from another session or a previous installation attempt has failed. Operation aborted.", fileName));
                }
                /// 2: Save the bundle in the "installing" folder for further processing.
                moduleZip.SaveAs(path);

                /// 3: Validate the bundle
                validateBundle(moduleName, moduleVersion, path); // throws exception on validation issues

                /// 4: Distribute the content of the bundle to proper places
                /// 5: Register the module in the modules catalog
                /// 6: Delete the zipped bundle any any temprorary file.folder created during installation
                /// 7: Prepare and communicate messages to the user
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError("", ex.Message);
            }
            finally // in any case clean up the installing folder and whatever else...
            {
                cleanUpInstallation(moduleName, moduleVersion, path); // expect empty parameters
            }
            // redirect back to the index action to show the form once again and the list of installed modules
            // it also carries model errors; they should be shown on top of the submit area.
            return RedirectToAction("Index");
        }

        private void cleanUpInstallation(object moduleName, object moduleVersion, object path)
        {
            throw new NotImplementedException();
        }

        private void validateBundle(string moduleName, string moduleVersion, string path)
        {
            throw new NotImplementedException();
        }

        public ActionResult Uninstall(string id)
        {
            return View();
        }

        public ActionResult Download(string id)
        {
            return View();
        }

    }
}