using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Web;
using System.Globalization;
using System.Threading;
using System.Security.Principal;
using System.Web.Security;
using System.Web.Hosting;

namespace Vaiona.Utils.Cfg
{
    public class Constants
    {
        public static readonly string AnonymousUser = @"Anonymous";
        public static readonly string EveryoneRole = "Everyone";
    }

    // this class must hide the web or desktop config implementation. use VWF pattern
    public class AppConfiguration
    {
        public static bool IsWebContext
        {
            get
            {
                return (HttpContext != null && HttpContext.Current != null);
            }
        }

        public static bool IsPostBack(HttpRequestBase request)
        {
            if (request.UrlReferrer == null) return false;

            bool isPost = "POST".Equals(request.HttpMethod, StringComparison.CurrentCultureIgnoreCase);
            bool isSameUrl = request.Url.AbsolutePath.Equals(request.UrlReferrer.AbsolutePath, StringComparison.CurrentCultureIgnoreCase);

            return isPost && isSameUrl;
        }

        public static ConnectionStringSettings DefaultApplicationConnection
        {
            get
            {
                return (ConfigurationManager.ConnectionStrings["ApplicationServices"]);
            }
        }

        //public static string ApplicationName
        //{
        //    get
        //    {
        //        try
        //        {
        //            return (ConfigurationManager.AppSettings["ApplicationName"]);
        //        }
        //        catch { return (string.Empty); }
        //    }
        //}

        //public static string ApplicationVersion
        //{
        //    get
        //    {
        //        try
        //        {
        //            return (ConfigurationManager.AppSettings["ApplicationVersion"]);
        //        }
        //        catch { return (string.Empty); }
        //    }
        //}

        public static string DefaultCulture
        {
            get
            {
                try
                {
                    return (ConfigurationManager.AppSettings["DefaultCulture"]);
                }
                catch { return ("en-US"); }
            }
        }

        public static string DatabaseMappingFile
        {
            get
            {
                try
                {
                    return (ConfigurationManager.AppSettings["DatabaseMappingFile"]);
                }
                catch { return (string.Empty); }
            }
        }

        public static string DatabaseDialect
        {
            get
            {
                try
                {
                    return (ConfigurationManager.AppSettings["DatabaseDialect"]);
                }
                catch { return ("DB2Dialect"); }
            }
        }

        public static bool AutoCommitTransactions
        {
            get
            {
                try
                {
                    return (bool.Parse(ConfigurationManager.AppSettings["AutoCommitTransactions"]));
                }
                catch { return (false); }
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

        public static string AppRoot
        {
            get
            {
                string path = "";
                try
                {
                    path = ConfigurationManager.AppSettings["ApplicationRoot"]; // This key appears in app.conig of unit testing projects only.
                }
                catch { }
                if (string.IsNullOrWhiteSpace(path))
                    path = AppDomain.CurrentDomain.BaseDirectory;
                return (path);
            }
        }

        public static string AreasPath
        {
            get
            {
                string path = HostingEnvironment.MapPath("~/Areas");
                if (!string.IsNullOrWhiteSpace(path))
                {
                    return path;
                }

                return (Path.Combine(AppRoot, "Areas"));
            }
        }

        /// <summary>
        /// DataPath shows the root folder containing business data.
        /// It should be defined in the web.config otherwise it returns a 'data' folder beneath the application's root folder
        /// </summary>
        public static string DataPath
        {
            get
            {
                string path = Path.Combine(AppRoot, "Data");
                try
                {
                    path = ConfigurationManager.AppSettings["DataPath"];
                }
                catch { }
                return (path);
            }
        }

        //public static string GetLoggerInfo(string logType)
        //{
        //    string loggerKey = logType + ".Logger";
        //    string loggerInfo = ""; // the logger info is the FQN of the logger class, that is instantiated by the logger factory
        //    try
        //    {
        //        loggerInfo = ConfigurationManager.AppSettings[loggerKey];
        //    }
        //    catch { loggerInfo = ""; }
        //    if (string.IsNullOrWhiteSpace(loggerInfo)) // try the general logging info attached to the General.Logger
        //    {
        //        loggerKey = "General.Logger";
        //        try
        //        {
        //            loggerInfo = ConfigurationManager.AppSettings[loggerKey];
        //        }
        //        catch { loggerInfo = ""; }
        //    }
        //    return loggerInfo;
        //}

        private static string workspaceRootPath = string.Empty;

        public static string WorkspaceRootPath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(workspaceRootPath))
                    return (workspaceRootPath);
                string path = string.Empty;
                try
                {
                    path = ConfigurationManager.AppSettings["WorkspacePath"];
                }
                catch { path = string.Empty; }
                int level = 0;
                if (string.IsNullOrWhiteSpace(path)) // its a relative path at the same level with the web.config
                    level = 0;
                else if (path.Contains(@"..\")) // its a relative path but upper than web.config. the number of ..\ patterns shows how many levels upper
                {
                    level = path.Split(@"\".ToCharArray()).Length - 1;
                }
                else // its an absolute path, just return it
                {
                    workspaceRootPath = path;
                    return (workspaceRootPath);
                }
                // use a default location: the same level with the app root not beneath it
                DirectoryInfo di = new DirectoryInfo(AppRoot);
                while (di.GetFiles("Web.config").Count() >= 1)
                    di = di.Parent;
                for (int i = 1; i < level; i++)
                {
                    di = di.Parent;
                }
                workspaceRootPath = Path.Combine(di.FullName, "Workspace");
                return (workspaceRootPath);
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

        public static string WorkspaceTenantsRoot
        {
            get
            {
                try
                {
                    return (Path.Combine(WorkspaceRootPath, "Tenants"));
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

        public static bool ShowQueries
        {
            get
            {
                try
                {
                    return (bool.Parse(ConfigurationManager.AppSettings["ShowQueries"]));
                }
                catch { return (false); }
            }
        }

        private static bool? cacheQueryResults = null;

        /// <summary>
        /// This property is accessed fequently, to reduce the web.config access, it uses a chached field.
        /// </summary>
        public static bool CacheQueryResults
        {
            get
            {
                if (cacheQueryResults.HasValue)
                    return cacheQueryResults.Value;
                try
                {
                    string s = ConfigurationManager.AppSettings["CacheQueryResults"];
                    if (string.IsNullOrEmpty(s))
                        cacheQueryResults = false;
                    else
                        cacheQueryResults = bool.Parse(s);
                }
                catch { cacheQueryResults = false; }
                return cacheQueryResults.Value;
            }
        }

        private static string themesPath;

        public static string ThemesPath
        {
            get
            {
                if (!string.IsNullOrEmpty(themesPath))
                    return themesPath;
                try
                {
                    // check if this is not the default tenant, use ~/tenants/<tenantId>/Themes
                    themesPath = ConfigurationManager.AppSettings["ThemesPath"];
                    if (string.IsNullOrEmpty(themesPath))
                        themesPath = "~/Themes";
                }
                catch { themesPath = "~/Themes"; }
                return themesPath;
            }
        }

        private static string defaultThemeName;

        public static string DefaultThemeName
        {
            get
            {
                if (!string.IsNullOrEmpty(defaultThemeName))
                    return defaultThemeName;
                try
                {
                    // check if this is not the default tenant, use ~/tenants/<tenantId>/Themes
                    defaultThemeName = ConfigurationManager.AppSettings["DefaultThemeName"];
                    if (string.IsNullOrEmpty(defaultThemeName))
                        defaultThemeName = "Default";
                }
                catch { defaultThemeName = "Default"; }
                return defaultThemeName;
            }
        }

        private static string activeLayoutName;

        public static string ActiveLayoutName
        {
            get
            {
                if (!string.IsNullOrEmpty(activeLayoutName))
                    return activeLayoutName;
                try
                {
                    // check if this is not the default tenant, use ~/tenants/<tenantId>/Themes
                    activeLayoutName = ConfigurationManager.AppSettings["ActiveLayoutName"];
                    if (string.IsNullOrEmpty(activeLayoutName))
                        activeLayoutName = "_Layout";
                }
                catch { activeLayoutName = "_Layout"; }
                return activeLayoutName;
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

        public static HttpContext HttpContext
        {
            get { return HttpContext.Current; }
        }

        public static Thread CurrentThread
        {
            get { return Thread.CurrentThread; }
        }

        public static CultureInfo UICulture // it can be done in cooperation with session management class
        {
            get { return Thread.CurrentThread.CurrentUICulture; }
        }

        public static CultureInfo Culture
        {
            get { return Thread.CurrentThread.CurrentCulture; }
        }

        public static DateTime UTCDateTime
        {
            get { return (DateTime.UtcNow); }
        }

        public static byte[] GetUTCAsBytes()
        {
            return (System.Text.Encoding.UTF8.GetBytes(AppConfiguration.UTCDateTime.ToBinary().ToString()));
        }

        public static DateTime DateTime
        {
            get { return (DateTime.Now); }
        }

        public static Uri CurrentRequestURL
        {
            get
            {
                try
                {
                    return (HttpContext.Current.Request.Url);
                }
                catch
                {
                    return new Uri("NotFound.htm");
                }
            }
        }

        public static IPrincipal User
        {
            // decide wether user must be authenticated or not
            get
            {
                if (/*Environment.HttpContext.User.Identity != null &&*/ !AppConfiguration.HttpContext.User.Identity.IsAuthenticated)
                {
                    return (createUser(Constants.AnonymousUser, Constants.EveryoneRole));
                }
                else
                    return (AppConfiguration.HttpContext.User);
            }
        }

        public static bool TryGetCurrentUser(ref string userName)
        {
            try
            {
                userName = AppConfiguration.HttpContext.User.Identity.Name; // Thread.CurrentPrincipal.Identity.Name;
                return (Thread.CurrentPrincipal.Identity.IsAuthenticated); //Thread.CurrentPrincipal.Identity.IsAuthenticated
            }
            catch { return false; }
        }

        internal static IPrincipal createUser(string userName, string roleName)
        {
            IIdentity identity = new GenericIdentity(userName);
            RolePrincipal p = new RolePrincipal(identity);
            //string[] roles = new string[1]; // get it from the role provder, take into account site and portal id
            //roles[0] = string.IsNullOrEmpty(roleName) ? Constants.GuestRole : roleName;
            //return (new GenericPrincipal(identity, roles));
            return (p);
            //JApplication.CurrentHttpContext.User.Identity.IsAuthenticated =
        }

        private static bool? isLoggingEnable = null;

        public static bool IsLoggingEnable
        {
            get
            {
                if (isLoggingEnable.HasValue)
                    return isLoggingEnable.Value;
                try
                {
                    string s = ConfigurationManager.AppSettings["IsLoggingEnable"];
                    if (string.IsNullOrEmpty(s))
                        isLoggingEnable = false;
                    else
                        isLoggingEnable = bool.Parse(s);
                }
                catch { isLoggingEnable = false; }
                return isLoggingEnable.Value;
            }
        }

        private static bool? isPerformanceLoggingEnable = null;

        public static bool IsPerformanceLoggingEnable
        {
            get
            {
                if (isPerformanceLoggingEnable.HasValue)
                    return isPerformanceLoggingEnable.Value;
                try
                {
                    string s = ConfigurationManager.AppSettings["IsPerformanceLoggingEnable"];
                    if (string.IsNullOrEmpty(s))
                        isPerformanceLoggingEnable = false;
                    else
                        isPerformanceLoggingEnable = bool.Parse(s);
                }
                catch { isPerformanceLoggingEnable = false; }
                return isPerformanceLoggingEnable.Value;
            }
        }

        private static bool? isDiagnosticLoggingEnable = null;

        public static bool IsDiagnosticLoggingEnable
        {
            get
            {
                if (isDiagnosticLoggingEnable.HasValue)
                    return isDiagnosticLoggingEnable.Value;
                try
                {
                    string s = ConfigurationManager.AppSettings["IsDiagnosticLoggingEnable"];
                    if (string.IsNullOrEmpty(s))
                        isDiagnosticLoggingEnable = false;
                    else
                        isDiagnosticLoggingEnable = bool.Parse(s);
                }
                catch { isDiagnosticLoggingEnable = false; }
                return isDiagnosticLoggingEnable.Value;
            }
        }

        private static bool? isCallLoggingEnable = null;

        public static bool IsCallLoggingEnable
        {
            get
            {
                if (isCallLoggingEnable.HasValue)
                    return isCallLoggingEnable.Value;
                try
                {
                    string s = ConfigurationManager.AppSettings["IsCallLoggingEnable"];
                    if (string.IsNullOrEmpty(s))
                        isCallLoggingEnable = false;
                    else
                        isCallLoggingEnable = bool.Parse(s);
                }
                catch { isCallLoggingEnable = false; }
                return isCallLoggingEnable.Value;
            }
        }

        private static bool? isExceptionLoggingEnable = null;

        public static bool IsExceptionLoggingEnable
        {
            get
            {
                if (isExceptionLoggingEnable.HasValue)
                    return isExceptionLoggingEnable.Value;
                try
                {
                    string s = ConfigurationManager.AppSettings["IsExceptionLoggingEnable"];
                    if (string.IsNullOrEmpty(s))
                        isExceptionLoggingEnable = false;
                    else
                        isExceptionLoggingEnable = bool.Parse(s);
                }
                catch { isExceptionLoggingEnable = false; }
                return isExceptionLoggingEnable.Value;
            }
        }

        private static bool? isDataLoggingEnable = null;

        public static bool IsDataLoggingEnable
        {
            get
            {
                if (isDataLoggingEnable.HasValue)
                    return isDataLoggingEnable.Value;
                try
                {
                    string s = ConfigurationManager.AppSettings["IsDataLoggingEnable"];
                    if (string.IsNullOrEmpty(s))
                        isDataLoggingEnable = false;
                    else
                        isDataLoggingEnable = bool.Parse(s);
                }
                catch { isDataLoggingEnable = false; }
                return isDataLoggingEnable.Value;
            }
        }

        private static string tenantId = "";

        public static string TenantId
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(tenantId))
                    return tenantId;
                try
                {
                    string s = ConfigurationManager.AppSettings["TenantId"];
                    if (string.IsNullOrEmpty(s))
                        tenantId = string.Empty;
                    else
                        tenantId = s;
                }
                catch { tenantId = string.Empty; }
                return tenantId;
            }
        }

        private static int? conversationIsolationLevel = null;

        /// <summary>
        /// Determines whether conversations (database sessions) are shared between units of work.
        /// 1: one session per unit of work, 2: shared session between units per HTTP request
        /// </summary>
        /// <remarks>Isolated Units of Work remain isolated.</remarks>
        public static int ConversationIsolationLevel
        {
            get
            {
                if (conversationIsolationLevel.HasValue)
                    return conversationIsolationLevel.Value;
                try
                {
                    string s = ConfigurationManager.AppSettings["ConversationIsolationLevel"];
                    if (string.IsNullOrEmpty(s))
                        conversationIsolationLevel = 2;
                    else
                        conversationIsolationLevel = int.Parse(s);
                }
                catch { conversationIsolationLevel = 2; }
                return conversationIsolationLevel.Value;
            }
        }

        //public static bool IsCustomLoggingEnable
        //{
        //}
    }
}