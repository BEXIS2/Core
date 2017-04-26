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
using Ionic.Zip;

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
            // save the zip file and paas the path to the ModuleManager.Install method. the whole process should be there.
            string moduleName = "";
            string moduleVersion = "";
            string installationPath = Path.Combine(AppConfiguration.WorkspaceModulesRoot, "installing");
            string path = "";
            try
            {
                /// 1: Acquire the bundle, check if the same bundle is installing? if yes, return an error
                if (moduleZip == null || moduleZip.ContentLength <= 0)
                    throw new System.Exception(string.Format("The submited file is not valid. Operation aborted."));

                var fileName = Path.GetFileName(moduleZip.FileName);
                moduleName = fileName.Substring(0, fileName.IndexOf("."));
                moduleVersion = fileName.Substring(fileName.IndexOf(".") + 1, (fileName.LastIndexOf(".") - fileName.IndexOf(".") - 1));

                path = installationPath;
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
                validateBundle(moduleName, moduleVersion, installationPath); // throws exception on validation issues

                /// 4: Distribute the content of the bundle to proper places
                copyBundleElements(moduleName, moduleVersion, installationPath);

                /// 5: Register the module in the modules catalog
                registerModule(moduleName, moduleVersion, installationPath);
            }
            catch (Exception ex)
            {
                this.ModelState.AddModelError("", ex.Message);
            }
            finally // in any case clean up the installing folder and whatever else...
            {
                /// 6: Delete the zipped bundle any any temprorary file.folder created during installation
                cleanUpInstallation(moduleName, moduleVersion, installationPath); // expect empty parameters

                /// 7: Prepare and communicate messages to the user
                if (this.ModelState.IsValid)
                {
                    // send a success message back. explain what should be done next.
                }
            }
            // redirect back to the index action to show the form once again and the list of installed modules
            // it also carries model errors; they should be shown on top of the submit area.
            return RedirectToAction("Index");
        }

        private void registerModule(string moduleName, string moduleVersion, string installationPath)
        {
            // check the catalog, if the module is already there, update its version and set it to pending.
            // if not, create an entry and set it to pending
            var existingModule = ModuleManager.GetModuleInfo(moduleName);
            if (existingModule != null)
            {

            }
            else
            {

            }
        }

        private void copyBundleElements(string moduleName, string moduleVersion, string installationPath)
        {
            //throw new NotImplementedException();
            string zipPath = Path.Combine(installationPath, moduleName + "." + moduleVersion + ".zip");
            var zip = ZipFile.Read(zipPath);
            string tempPath = Path.Combine(installationPath, moduleName, moduleVersion);
            try
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath);
            }catch(Exception ex)
            {
                throw new Exception(string.Format("Could not delete directoty {0}. {1}", tempPath, ex.Message));
            }
            try
            {
                Directory.CreateDirectory(tempPath);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not create directoty {0}. {1}", tempPath, ex.Message));
            }
            try
            {
                zip.ExtractAll(tempPath);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not unzip bundle {0} to directory {1}. {2}", moduleName, tempPath, ex.Message));
            }

            // in debug mode, there is no need to copy the files because they are already in the right place and more importantly the bundle will overwite the sources!
            bool installForProduction = true;
            if (installForProduction)
            {
                //move the workspace, if exists in the bundle
                string moduleWorkspace = Path.Combine(AppConfiguration.WorkspaceModulesRoot, moduleName);
                try
                {
                    if (Directory.Exists(moduleWorkspace))
                    {
                        // remove the module's workspace and its content. 
                        // It may remove existing module specific settings. So better guard it with a user selected switch (UI)
                        //Directory.Delete(moduleWorkspace, true);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Could not delete directoty {0}. {1}", moduleWorkspace, ex.Message));
                }
                try
                {
                    MoveAndReplace(Path.Combine(tempPath, "Workspace"), moduleWorkspace);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Could not move the bundle's worspace to {0}. {1}", moduleWorkspace, ex.Message));
                }
                // move whatever else that needs to go to a place other than the module's root

                // move whatever is remained to the module's root. This should include bin, Content, Scripts, and Views folder plus the manifest file.
                string moduleDepolymentPath = Path.Combine(ModuleManager.DeploymentRoot, moduleName, moduleVersion); // the version is ONLY for testing purpose. must be removed!
                try
                {
                    if (Directory.Exists(moduleDepolymentPath))
                        Directory.Delete(moduleDepolymentPath, true);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Could not delete directoty {0}. {1}", moduleDepolymentPath, ex.Message));
                }
                try
                {
                    // remove the bin folder's assemblies that are not listed in the manifest

                    // move the reamining 
                    Directory.Move(tempPath, moduleDepolymentPath); // in case use MoveAndReplace
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Could not move the bundle's binaries to {0}. {1}", moduleDepolymentPath, ex.Message));
                }
            }
            //end if
        }

        private void cleanUpInstallation(string moduleName, string moduleVersion, string installationPath)
        {
            //throw new NotImplementedException();
        }

        private void validateBundle(string moduleName, string moduleVersion, string installationPath)
        {
            // check the module version: zip, manifest, UI assembly.
            // check if an equal or a higher version is already installed
            var existingModule = ModuleManager.GetModuleInfo(moduleName);
            if(existingModule != null)
            {
                Version exisitingVersion = new Version(existingModule.Manifest.Version);
                Version incomingVersion = new Version(moduleVersion);
                if(exisitingVersion >= incomingVersion)
                {
                    throw new Exception(string.Format("A higher version of the '{0}' module is already installed.", moduleName));
                }
            }
            // check if the declaraed dependecies are already available. module/version

        }

        public ActionResult Uninstall(string id)
        {
            return View();
        }

        // Move this to Viona.Utils
        public static void MoveAndReplace(string sourceDirectoty, string targetDirectory)
        {
            var sourcePath = sourceDirectoty.TrimEnd('\\', ' ');
            var targetPath = targetDirectory.TrimEnd('\\', ' ');
            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(s => Path.GetDirectoryName(s));
            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (System.IO.File.Exists(targetFile))
                        System.IO.File.Delete(targetFile);
                    System.IO.File.Move(file, targetFile);
                }
            }
            Directory.Delete(sourceDirectoty, true);
        }

        public ActionResult Download(string id)
        {
            return View();
        }

    }
}