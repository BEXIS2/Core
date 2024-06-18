using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Xml.Linq;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;
using Vaiona.Utils.IO;

namespace Vaiona.Web.Mvc.Modularity
{
    /// <summary>
    /// This class is responsible for loading the plugin assemblies before the application is started.
    /// Its Initialize method automatically fires at app_pre_init, because it is registered in the AssemblyInfo.cs
    /// The method should read the assembly names from the modules catalog, select the active ones and
    /// load their associated assemblies from the modules folder into the plugins folder.
    /// The plugins folder is resgisted as a probling folder in the web.config,
    /// so that the AppDomain knows to search there for loading assemblies.
    /// This project must be referenced from the Shell to get activated.
    /// </summary>
    /// <remarks>
    /// The main issue with this method is that it needs the application to be restarted
    /// if a new module is registered or its status has been changed!! No hot plugability yet!!!!
    /// </remarks>
    public class ModuleInitializer
    {
        /// <summary>
        /// The source folder the conatins the modules' assemblies
        /// </summary>
        /// <remarks>
        /// This folder conatins one folder per module, so that each folder conatins all the resources of the respective module
        /// </remarks>
        private static readonly DirectoryInfo areasFolder;

        static ModuleInitializer()
        {
            // this code is called by aspnet_compiler at compile time!!
            // The if protects it from running during the build
            // See: http://stackoverflow.com/questions/13642691/avoid-aspnet-compiler-running-preapplicationstart-method
            if (HostingEnvironment.InClientBuildManager)
                return;

            // Use AppConfiguration for path selection
            string areasPath = ModuleManager.DeploymentRoot;
            areasFolder = new DirectoryInfo(areasPath);
            if (areasFolder == null || !areasFolder.Exists)
                throw new DirectoryNotFoundException("Areas");
        }

        /// <summary>
        /// This method will be automatically called during application startup at pre-init phase
        /// </summary>
        public static void Initialize()
        {
            initialize();
        }

        /// <summary>
        /// This overload (of Initialize)
        /// </summary>
        /// <param name="force"></param>
        public static void Initialize(bool force)
        {
            initialize();
        }

        private static void initialize()
        {
            //#if DEBUG
            // Gets a value that indicates whether the hosting environment has access to the ASP.NET build system.
            if (HostingEnvironment.InClientBuildManager)
            {
                LoggerFactory.GetFileLogger().LogCustom(string.Format("Hosting environenment is set as 'InClientBuildManager', which prevents modules from being initialized!"));
                return;
            }
            //#endif
            LoggerFactory.GetFileLogger().LogCustom(string.Format("Preparing to initialize the registered modules..."));
            XElement catalog = null;
            List<XElement> modules = new List<XElement>();
            try
            {
                catalog = XElement.Load(Path.Combine(AppConfiguration.WorkspaceModulesRoot, "Modules.Catalog.xml"));
                List<string> validStates = new List<string>() { "Active", "Inactive", "Pending" };
                modules = catalog.Elements("Module")
                                    .Where(m => validStates.Any(p => p.Equals(m.Attribute("status").Value, StringComparison.InvariantCultureIgnoreCase)))
                                    .OrderBy(m => int.Parse(m.Attribute("order").Value))
                                    .ToList();
            }
            catch (Exception ex)
            {
                string message = string.Format("Could not load the modules' catalog file from the '{0}' folder or the catalog is not a vlaid XML file. See the detailed message: {1}.", AppConfiguration.WorkspaceModulesRoot, ex.Message);
                LoggerFactory.GetFileLogger().LogCustom(message);
                throw new Exception(message, ex);
            }

            foreach (var moduleEntry in modules)
            //foreach (var moduleDir in areasFolder.GetDirectories())
            {
                string moduleId = moduleEntry.Attribute("id").Value;
                string moduleUIPath = moduleEntry.Attribute("path")?.Value;
                LoggerFactory.GetFileLogger().LogCustom(string.Format("Initializing module '{0}'.", moduleId));
                if (string.IsNullOrWhiteSpace(moduleId))
                    break;

                DirectoryInfo moduleDir = null;

                try
                {
                    string dirPath = (string.IsNullOrEmpty(moduleUIPath)) ? moduleId : Path.Combine(moduleId, moduleUIPath);

                    moduleDir = areasFolder.GetDirectories(dirPath, SearchOption.TopDirectoryOnly).FirstOrDefault();
                    if (moduleDir == null || !moduleDir.Exists)
                        throw new DirectoryNotFoundException();
                }
                catch (Exception ex)
                {
                    var message = $"Could not find module '{moduleId}' in folder {areasFolder.FullName}. Root cause: {ex.Message}";
                    LoggerFactory.GetFileLogger().LogCustom(message);
                    throw new Exception(message);
                }

                // string manifestFileName = string.Format("{0}.Manifest.xml", moduleDir.Name);
                string manifestFileName = string.Format("{0}.Manifest.xml", moduleId.ToUpper());
                if (moduleDir.GetFiles(manifestFileName, SearchOption.TopDirectoryOnly).Count() == 1)
                {
                    var moduleBinFolder = moduleDir.GetDirectories("bin", SearchOption.TopDirectoryOnly).First(); // it is supposed to throw an exception if the bin folder does not exist.
                    LoggerFactory.GetFileLogger().LogCustom(string.Format("Initializing dependent assemblies for module '{0}'.", moduleId));
                    loadSatelliteAssemblies(moduleId, moduleDir, moduleBinFolder);

                    LoggerFactory.GetFileLogger().LogCustom(string.Format("Initializing UI assembly for module '{0}'.", moduleId));
                    loadEntryAssembly(moduleId.ToUpper(), moduleDir, moduleBinFolder);

                    LoggerFactory.GetFileLogger().LogCustom(string.Format("Module '{0}' was successfully initialized.", moduleId));
                }
                else
                {
                    string message = string.Format("Folder: {0} located at: {1} is supposed to be a module, but does not contain a valid manifest file.", moduleDir.Name, moduleDir.FullName);
                    LoggerFactory.GetFileLogger().LogCustom(message);
                    throw new Exception(message);
                }
            }
        }

        private static void loadEntryAssembly(string moduleId, DirectoryInfo moduleDir, DirectoryInfo moduleBinFolder)
        {
            try
            {
                // for each module get its main assembly, the one that contains the type inheritted from the ModuleBase
                string assemblyNamePattern = string.Format("*.{0}.UI.dll", moduleId);
                var assemblyName = moduleBinFolder.GetFiles(assemblyNamePattern, SearchOption.TopDirectoryOnly)
                                                    .Select(x => AssemblyName.GetAssemblyName(x.FullName))
                                                    .FirstOrDefault();
                if (assemblyName == null)
                {
                    string message = string.Format("Could not find assembly '{0}' for module '{1}' in '{2}'.", assemblyName.FullName, moduleId, moduleBinFolder.FullName);
                    LoggerFactory.GetFileLogger().LogCustom(message);
                    throw new Exception(message);
                }

                var asm = Assembly.Load(assemblyName);
                LoggerFactory.GetFileLogger().LogCustom(string.Format("Assembly '{0}' was loaded for module '{1}'.", assemblyName.Name, moduleDir));
                // check for for a class that inherits from the ModuleBase class
                Type type = asm.GetTypes()
                                .Where(t => typeof(ModuleBase).IsAssignableFrom(t)).FirstOrDefault();
                if (type != null)
                {
                    ModuleInfo moduleMetadata = new ModuleInfo();
                    moduleMetadata.Id = moduleId;
                    moduleMetadata.EntryType = type;
                    // instance will be created later with the area registration
                    //var plugin = (ModuleBase)Activator.CreateInstance(type);
                    // Check for duplicates
                    if (!ModuleManager.ModuleInfos.Contains(moduleMetadata))
                    {
                        //Add the plugin (module's entry assembly/type) as a reference to the application
                        // Check if the assmebly is aleary added. if not possible to check, guard
                        try
                        {
                            BuildManager.AddReferencedAssembly(asm);
                            moduleMetadata.Assembly = asm;
                            LoggerFactory.GetFileLogger().LogCustom(string.Format("Assembly '{0}' was added to the build manager for module '{1}'.", assemblyName.Name, moduleDir));
                        }
                        catch (Exception)
                        { // it is aleady added
                            string message = string.Format("Module '{0}' is already registered and loaded. It may have a strong depenedcy from another module or from the shell, Or more than one manifest files are registering it!", moduleDir);
                            LoggerFactory.GetFileLogger().LogCustom(message);
                            throw new Exception(message);
                        }
                        ModuleManager.CacheAssembly(assemblyName.Name, asm);
                        moduleMetadata.Path = moduleDir;
                        //Add the modules to the PluginManager to manage them later
                        ModuleManager.Add(moduleMetadata);
                    }
                }
                else
                {
                    string message = string.Format("Could not find the entry class for module '{0}'. The module must have a class that inherits from the ModuleBase class.", moduleDir);
                    LoggerFactory.GetFileLogger().LogCustom(message);
                    throw new Exception(message);
                }
            }
            catch (Exception ex)
            {
                string message = string.Format("Module {0} does not have the proper structure! No bin folder or no entry assembly was found.", moduleDir.Name);
                LoggerFactory.GetFileLogger().LogCustom(message);
                throw new Exception(message, ex);
            }
        }

        private static void loadSatelliteAssemblies(string moduleId, DirectoryInfo moduleDir, DirectoryInfo moduleBinFolder)
        {
            XElement manifest;
            string manifestFileName = string.Format("{0}{1}{2}.Manifest.xml", moduleDir.FullName, Path.DirectorySeparatorChar, moduleId);

            FileHelper.WaitForFile(manifestFileName);
            using (var stream = File.Open(manifestFileName, FileMode.Open, FileAccess.Read))
            {
                manifest = XElement.Load(stream);
                try
                {
                    var xAssemblies = manifest.Element("Assemblies").Elements("Assembly")
                                                .Where(x => !string.IsNullOrWhiteSpace(x.Attribute("fullName").Value))
                                                .ToList();
                    LoggerFactory.GetFileLogger().LogCustom(string.Format("Initializing {0} dependent assemblies for module '{1}'.", xAssemblies.Count(), moduleId));

                    foreach (var xAsm in xAssemblies)
                    {
                        string asmName = xAsm.Attribute("fullName").Value;

                        string assemblyNamePattern = string.Format("{0}.dll", asmName);
                        var assemblyName = moduleBinFolder.GetFiles(assemblyNamePattern, SearchOption.TopDirectoryOnly)
                                                            .Select(x => AssemblyName.GetAssemblyName(x.FullName))
                                                            .FirstOrDefault();
                        if (assemblyName == null)
                            break; // should throw an exception!
                        try
                        {
                            var asm = Assembly.Load(assemblyName);
                            LoggerFactory.GetFileLogger().LogCustom(string.Format("Assembly '{0}' was loaded for module '{1}'.", assemblyName.Name, moduleId));
                            //Add the plugin's satellite assembly as a reference to the application
                            try
                            {
                                BuildManager.AddReferencedAssembly(asm);
                                LoggerFactory.GetFileLogger().LogCustom(string.Format("Assembly '{0}' was added to the build manager for module '{1}'.", assemblyName.Name, moduleId));
                            }
                            catch (Exception)
                            { // it is aleady added
                              //string message = string.Format("Assembly '{0}' is already registered and loaded. It may have a strong depenedcy from a module or from the shell, Or more than one manifest files are registering it!", assemblyName.FullName);
                              //LoggerFactory.GetFileLogger().LogCustom(message);
                              //throw new Exception(message);
                            }
                            ModuleManager.CacheAssembly(asmName, asm);
                        }
                        catch (Exception ex)
                        {
                            LoggerFactory.GetFileLogger().LogCustom($"Could not load the assembly {asmName} for module '{moduleId}'. Roor cause: {ex.Message}");
                            throw ex;
                        }
                    }
                }
                catch (Exception ex)
                { // Do nothing
                    LoggerFactory.GetFileLogger().LogCustom($"Could not load the manifest file for module '{moduleId}'. No dependent assembly loaded!");
                    throw ex;
                }
            }
        }
    }
}