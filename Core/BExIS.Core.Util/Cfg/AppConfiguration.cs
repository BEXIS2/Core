using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;

namespace BExIS.Core.Util.Cfg
{
    public class AppConfiguration
    {
        public static ConnectionStringSettings DefaultApplicationConnection
        { 
            get 
            {
                return (ConfigurationManager.ConnectionStrings["ApplicationServices"]);
            } 
        }

        public static string IoCProviderTypeInfo
        {
            get
            {
                try
                {
                    return (ConfigurationManager.AppSettings["IoCProviderTypeInfo"]);
                }
                catch { return (string.Empty); }
            }
        }

        //public static string DbMappingInfoPath
        //{
        //    get
        //    {
        //        try
        //        {
        //            return (ConfigurationManager.AppSettings["DbMappingInfoPath"]);
        //        }
        //        catch { return (string.Empty); }
        //    }
        //}

        public static string AppRoot
        {
            get
            {
                return (AppDomain.CurrentDomain.BaseDirectory);
            }
        }
        public static string WorkspaceRootPath
        {
            get
            {
                string path = string.Empty;
                try
                {
                    path = ConfigurationManager.AppSettings["WorkspacePath"];
                }
                catch { path = string.Empty; }
                if (!string.IsNullOrWhiteSpace(path))
                    return (path);
                // use a default location: the same level with the app root not beneath it
                DirectoryInfo di = new DirectoryInfo(AppRoot);
                while (di.GetFiles("Web.config").Count() >= 1)
                    di = di.Parent;
                return (Path.Combine(di.Parent.FullName, "Workspace"));
            }
        }

        public static string WorkspaceComponentRoot
        {
            get
            {
                try
                {
                    return (Path.Combine(WorkspaceRootPath, "Components"));
                }
                catch { return (string.Empty); }
            }
        }

        public static string WorkspaceModulesRoot
        {
            get
            {
                try
                {
                    return (Path.Combine(WorkspaceRootPath, "Modules"));
                }
                catch { return (string.Empty); }
            }
        }

        public static string WorkspaceGeneralRoot
        {
            get
            {
                try
                {
                    return (Path.Combine(WorkspaceRootPath, "General"));
                }
                catch { return (string.Empty); }
            }
        }

        public static string GetModuleWorkspacePath(string moduleName)
        {
            return (Path.Combine(WorkspaceModulesRoot, moduleName));
        }

        public static string GetComponentWorkspacePath(string componentName)
        {
            return (Path.Combine(WorkspaceComponentRoot, componentName));
        }

        public static bool UseSchemaInDatabaseGeneration 
        { 
            get 
            {
                try
                {
                    return (bool.Parse(ConfigurationManager.AppSettings["UseSchemaInDatabaseGeneration"]));
                }
                catch { return (false); }
            } 
        }

        public static bool CreateDatabase
        {
            get
            {
                try
                {
                    return (bool.Parse(ConfigurationManager.AppSettings["CreateDatabase"]));
                }
                catch { return (false); }
            }
        }

        public static string ThemesPath
        {
            get
            {
                try
                {
                    return (ConfigurationManager.AppSettings["ThemesPath"]);
                }
                catch { return ("~/Themes"); }
            }
        }

        public static string DefaultThemeName
        {
            get
            {
                try
                {
                    return (ConfigurationManager.AppSettings["DefaultThemeName"]);
                }
                catch { return ("Default"); }
            }
        }

        public static string ActiveLayoutName
        {
            get
            {
                try
                {
                    return (ConfigurationManager.AppSettings["ActiveLayoutName"]);
                }
                catch { return ("_Layout"); }
            }
        }

        public static bool ThrowErrorWhenParialContentNotFound 
        { 
            get 
            {
                try
                {
                    return (bool.Parse(ConfigurationManager.AppSettings["ThrowErrorWhenParialContentNotFound"]));
                }
                catch { return (false); }
            } 
        }
        
    }
}
