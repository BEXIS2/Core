using Microsoft.AspNetCore.Http;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Threading;

namespace BEXIS2.Utils.Cfg
{
    public static class Constants
    {
        public static readonly string AnonymousUser = @"Anonymous";
        public static readonly string EveryoneRole = "Everyone";
    }

    public class AppConfiguration
    {
        // Hinweis: IsWebContext und Request-abhängige Logik (wie IsPostBack, CurrentRequestURL, User) 
        // sollten in .NET Core / .NET Standard über DI und IHttpContextAccessor gelöst werden, 
        // da es kein globales HttpContext.Current mehr gibt. 
        // Hier als Platzhalter / Fallback implementiert:
        public static bool IsWebContext => false; // In .NET Standard standardmäßig false, es sei denn, ein Context wird injiziert.

        private static Func<object> _httpContextProvider;

        public static void Configure(Func<object> httpContextProvider)
        {
            _httpContextProvider = httpContextProvider;
        }

        /// <summary>
        /// Liefert den aktuellen HttpContext über den Accessor.
        /// </summary>
        public static HttpContext HttpContext => _httpContextProvider?.Invoke() as HttpContext;

        public static ConnectionStringSettings DefaultApplicationConnection
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["ApplicationServices"];
            }
        }

        public static string DefaultCulture
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["DefaultCulture"] ?? "en-US";
                }
                catch { return "en-US"; }
            }
        }

        public static string DatabaseMappingFile
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["DatabaseMappingFile"] ?? string.Empty;
                }
                catch { return string.Empty; }
            }
        }

        public static string DatabaseDialect
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["DatabaseDialect"] ?? "DB2Dialect";
                }
                catch { return "DB2Dialect"; }
            }
        }

        public static bool AutoCommitTransactions
        {
            get
            {
                try
                {
                    string s = ConfigurationManager.AppSettings["AutoCommitTransactions"];
                    return bool.TryParse(s, out var result) ? result : false;
                }
                catch { return false; }
            }
        }

        public static string IoCProviderTypeInfo
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["IoCProviderTypeInfo"] ?? string.Empty;
                }
                catch { return string.Empty; }
            }
        }

        public static string AppRoot
        {
            get
            {
                string path = string.Empty;
                try
                {
                    path = ConfigurationManager.AppSettings["ApplicationRoot"];
                }
                catch { }

                if (string.IsNullOrWhiteSpace(path))
                    path = AppDomain.CurrentDomain.BaseDirectory;

                return path;
            }
        }

        public static string AreasPath
        {
            get
            {
                // HostingEnvironment.MapPath existiert in .NET Standard nicht.
                // Entweder über AppRoot auflösen oder weglassen, wenn nicht benötigt.
                return Path.Combine(AppRoot, "Areas");
            }
        }

        /// <summary>
        /// DataPath shows the root folder containing business data.
        /// </summary>
        public static string DataPath
        {
            get
            {
                string path = Path.Combine(AppRoot, "Data");
                try
                {
                    string customPath = ConfigurationManager.AppSettings["DataPath"];
                    if (!string.IsNullOrWhiteSpace(customPath))
                    {
                        path = customPath;
                    }
                }
                catch { }
                return path;
            }
        }

        private static string workspaceRootPath = string.Empty;
        private static readonly object workspaceLock = new object();

        public static string WorkspaceRootPath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(workspaceRootPath))
                    return workspaceRootPath;

                lock (workspaceLock)
                {
                    if (!string.IsNullOrWhiteSpace(workspaceRootPath))
                        return workspaceRootPath;

                    string path = string.Empty;
                    try
                    {
                        path = ConfigurationManager.AppSettings["WorkspacePath"];
                    }
                    catch { path = string.Empty; }

                    int level = 0;
                    if (string.IsNullOrWhiteSpace(path))
                        level = 0;
                    else if (path.Contains(@"..\") || path.Contains("../"))
                    {
                        level = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    }
                    else
                    {
                        workspaceRootPath = path;
                        return workspaceRootPath;
                    }

                    DirectoryInfo di = new DirectoryInfo(AppRoot);
                    // Suche nach Konfigurationsdateien oder Projektdateien als Abbruchbedingung
                    while (di != null && di.GetFiles("*.config").Length >1)
                    {
                        di = di.Parent; // go to console
                    }

                    if (di == null)
                        di = new DirectoryInfo(AppRoot);

                    for (int i = 1; i < level; i++)
                    {
                        if (di.Parent != null)
                            di = di.Parent;
                    }

                    workspaceRootPath = Path.Combine(di.FullName, "Workspace");
                    return workspaceRootPath;
                }
            }
        }

        public static string WorkspaceComponentRoot => Path.Combine(WorkspaceRootPath, "Components");
        public static string WorkspaceModulesRoot => Path.Combine(WorkspaceRootPath, "Modules");
        public static string WorkspaceGeneralRoot => Path.Combine(WorkspaceRootPath, "General");
        public static string WorkspaceTenantsRoot => Path.Combine(WorkspaceRootPath, "Tenants");

        public static string GetModuleWorkspacePath(string moduleName) => Path.Combine(WorkspaceModulesRoot, moduleName);
        public static string GetComponentWorkspacePath(string componentName) => Path.Combine(WorkspaceComponentRoot, componentName);

        public static bool UseSchemaInDatabaseGeneration
        {
            get
            {
                try
                {
                    return bool.TryParse(ConfigurationManager.AppSettings["UseSchemaInDatabaseGeneration"], out var res) && res;
                }
                catch { return false; }
            }
        }

        public static bool CreateDatabase
        {
            get
            {
                try
                {
                    return bool.TryParse(ConfigurationManager.AppSettings["CreateDatabase"], out var res) && res;
                }
                catch { return false; }
            }
        }

        public static bool ShowQueries
        {
            get
            {
                try
                {
                    return bool.TryParse(ConfigurationManager.AppSettings["ShowQueries"], out var res) && res;
                }
                catch { return false; }
            }
        }

        private static bool? cacheQueryResults = null;
        public static bool CacheQueryResults
        {
            get
            {
                if (cacheQueryResults.HasValue)
                    return cacheQueryResults.Value;

                try
                {
                    string s = ConfigurationManager.AppSettings["CacheQueryResults"];
                    cacheQueryResults = string.IsNullOrEmpty(s) ? false : bool.Parse(s);
                }
                catch { cacheQueryResults = false; }
                return cacheQueryResults.Value;
            }
        }

        public static string ThemesPath
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["ThemesPath"] ?? "~/Themes";
                }
                catch { return "~/Themes"; }
            }
        }

        public static string DefaultThemeName
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["DefaultThemeName"] ?? "Default";
                }
                catch { return "Default"; }
            }
        }

        public static string ActiveLayoutName
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["ActiveLayoutName"] ?? "_Layout";
                }
                catch { return "_Layout"; }
            }
        }

        public static bool ThrowErrorWhenParialContentNotFound
        {
            get
            {
                try
                {
                    return bool.TryParse(ConfigurationManager.AppSettings["ThrowErrorWhenParialContentNotFound"], out var res) && res;
                }
                catch { return false; }
            }
        }

        public static Thread CurrentThread => Thread.CurrentThread;
        public static CultureInfo UICulture => Thread.CurrentThread.CurrentUICulture;
        public static CultureInfo Culture => Thread.CurrentThread.CurrentCulture;

        public static DateTime UTCDateTime => DateTime.UtcNow;

        public static byte[] GetUTCAsBytes()
        {
            return System.Text.Encoding.UTF8.GetBytes(UTCDateTime.ToBinary().ToString());
        }

        public static DateTime DateTimeNow => DateTime.Now;

        private static bool? isLoggingEnable = null;
        public static bool IsLoggingEnable => GetCachedBool("IsLoggingEnable", ref isLoggingEnable);

        private static bool? isPerformanceLoggingEnable = null;
        public static bool IsPerformanceLoggingEnable => GetCachedBool("IsPerformanceLoggingEnable", ref isPerformanceLoggingEnable);

        private static bool? isDiagnosticLoggingEnable = null;
        public static bool IsDiagnosticLoggingEnable => GetCachedBool("IsDiagnosticLoggingEnable", ref isDiagnosticLoggingEnable);

        private static bool? isCallLoggingEnable = null;
        public static bool IsCallLoggingEnable => GetCachedBool("IsCallLoggingEnable", ref isCallLoggingEnable);

        private static bool? isExceptionLoggingEnable = null;
        public static bool IsExceptionLoggingEnable => GetCachedBool("IsExceptionLoggingEnable", ref isExceptionLoggingEnable);

        private static bool? isDataLoggingEnable = null;
        public static bool IsDataLoggingEnable => GetCachedBool("IsDataLoggingEnable", ref isDataLoggingEnable);

        private static string tenantId = null;
        public static string TenantId
        {
            get
            {
                if (tenantId != null) return tenantId;
                try
                {
                    tenantId = ConfigurationManager.AppSettings["TenantId"] ?? string.Empty;
                }
                catch { tenantId = string.Empty; }
                return tenantId;
            }
        }

        private static int? conversationIsolationLevel = null;
        public static int ConversationIsolationLevel
        {
            get
            {
                if (conversationIsolationLevel.HasValue)
                    return conversationIsolationLevel.Value;

                try
                {
                    string s = ConfigurationManager.AppSettings["ConversationIsolationLevel"];
                    conversationIsolationLevel = int.TryParse(s, out var res) ? res : 2;
                }
                catch { conversationIsolationLevel = 2; }
                return conversationIsolationLevel.Value;
            }
        }

        private static bool GetCachedBool(string key, ref bool? cacheField)
        {
            if (cacheField.HasValue)
                return cacheField.Value;

            try
            {
                string s = ConfigurationManager.AppSettings[key];
                cacheField = !string.IsNullOrEmpty(s) && bool.Parse(s);
            }
            catch { cacheField = false; }
            return cacheField.Value;
        }

        #region user
        


        public static IPrincipal User
        {
            get
            {
                var user = HttpContext?.User;
                if (user == null || !user.Identity.IsAuthenticated)
                {
                    return createUser(Constants.AnonymousUser, Constants.EveryoneRole);
                }
                else
                {
                    return user;
                }
            }
        }

        public static bool TryGetCurrentUser(ref string userName)
        {
            try
            {
                var user = HttpContext?.User;
                if (user?.Identity != null && user.Identity.IsAuthenticated)
                {
                    userName = user.Identity.Name;
                    return true;
                }
                userName = string.Empty;
                return false;
            }
            catch
            {
                userName = string.Empty;
                return false;
            }
        }

        internal static IPrincipal createUser(string userName, string roleName)
        {
            IIdentity identity = new GenericIdentity(userName);
            // In .NET Standard / Core nutzt man meist GenericPrincipal statt RolePrincipal
            string[] roles = new string[] { roleName };
            return new GenericPrincipal(identity, roles);
        }


        #endregion

        #region request

        public static Uri CurrentRequestURL
        {
            get
            {
                try
                {
                    var request = HttpContext?.Request;
                    if (request != null)
                    {
                        // In ASP.NET Core setzen wir die URL sauber zusammen:
                        string scheme = request.Scheme;
                        string host = request.Host.Value;
                        string path = request.Path;
                        string queryString = request.QueryString.Value;
                        return new Uri($"{scheme}://{host}{path}{queryString}");
                    }
                    return new Uri("http://NotFound.htm");
                }
                catch
                {
                    return new Uri("http://NotFound.htm");
                }
            }
        }

        public static string UserAgent
        {
            get
            {
                try
                {
                    var headers = HttpContext?.Request?.Headers;
                    if (headers != null && headers.ContainsKey("User-Agent"))
                    {
                        return headers["User-Agent"].ToString();
                    }
                    return string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static string HttpMethod
        {
            get
            {
                try
                {
                    return HttpContext?.Request?.Method ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static bool IsSecureConnection
        {
            get
            {
                try
                {
                    // Prüft, ob der Request via HTTPS erfolgt ist
                    return HttpContext?.Request?.IsHttps ?? false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static string UserHostAddress
        {
            get
            {
                try
                {
                    var ipAddress = HttpContext?.Connection?.RemoteIpAddress;
                    return ipAddress?.ToString() ?? string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static bool IsLocal
        {
            get
            {
                try
                {
                    var connection = HttpContext?.Connection;
                    if (connection == null)
                        return false;

                    var remoteIp = connection.RemoteIpAddress;
                    var localIp = connection.LocalIpAddress;

                    // Für Unit Tests oder In-Memory-Server ohne Verbindungsinfos
                    if (remoteIp == null && localIp == null)
                        return true;

                    if (remoteIp != null)
                    {
                        // Wenn die Remote-IP gleich der Local-IP ist, ist es lokal
                        if (localIp != null && remoteIp.Equals(localIp))
                            return true;

                        // Ansonsten prüfen, ob es sich um eine Loopback-Adresse handelt (z.B. 127.0.0.1 oder ::1)
                        return IPAddress.IsLoopback(remoteIp);
                    }

                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static string SessionID
        {
            get
            {
                try
                {
                    // In ASP.NET Core greift man über den HttpContext auf die Session zu
                    var session = HttpContext?.Session;
                    if (session != null)
                    {
                        // Durch den Zugriff auf .IsAvailable oder einen Lesevorgang wird sichergestellt, 
                        // dass die Session-ID generiert bzw. geladen wird.
                        return session.Id;
                    }
                    return string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static bool IsPostBack()
        {
            try
            {
                var request = HttpContext?.Request;
                if (request == null) return false;

                // Prüfen, ob es ein POST-Request ist
                bool isPost = "POST".Equals(request.Method, StringComparison.OrdinalIgnoreCase);
                if (!isPost) return false;

                // In ASP.NET Core kommt der Referrer aus den Request-Headern ("Referer")
                string refererStr = request.Headers["Referer"].ToString();
                if (string.IsNullOrEmpty(refererStr)) return false;

                // Referer in eine Uri parsen, um den Pfad zu vergleichen
                if (Uri.TryCreate(refererStr, UriKind.Absolute, out var refererUri))
                {
                    bool isSameUrl = request.Path.Equals(refererUri.AbsolutePath, StringComparison.OrdinalIgnoreCase);
                    return isSameUrl;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}