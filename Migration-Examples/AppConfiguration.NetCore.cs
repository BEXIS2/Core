using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Threading;

namespace Vaiona.Utils.Cfg
{
    public class Constants
    {
        public static readonly string AnonymousUser = @"Anonymous";
        public static readonly string EveryoneRole = "Everyone";
    }

    /// <summary>
    /// .NET Core version of AppConfiguration using dependency injection instead of static access.
    /// Replaces System.Web dependencies with ASP.NET Core equivalents.
    /// </summary>
    public class AppConfiguration
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public AppConfiguration(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        public bool IsWebContext => _httpContextAccessor.HttpContext != null;

        public bool IsPostBack(HttpRequest request)
        {
            if (request.Headers.Referer.Count == 0) return false;
            if (!Uri.TryCreate(request.Headers.Referer.ToString(), UriKind.Absolute, out Uri refererUri)) return false;

            bool isPost = "POST".Equals(request.Method, StringComparison.CurrentCultureIgnoreCase);
            bool isSameUrl = request.Path.Equals(refererUri.AbsolutePath, StringComparison.CurrentCultureIgnoreCase);

            return isPost && isSameUrl;
        }

        public string DefaultApplicationConnection => _configuration.GetConnectionString("ApplicationServices");

        public string DefaultCulture => _configuration["DefaultCulture"] ?? "en-US";

        public string DatabaseMappingFile => _configuration["DatabaseMappingFile"] ?? string.Empty;

        public string DatabaseDialect => _configuration["DatabaseDialect"] ?? "DB2Dialect";

        public bool AutoCommitTransactions => _configuration.GetValue<bool>("AutoCommitTransactions", false);

        public string IoCProviderTypeInfo => _configuration["IoCProviderTypeInfo"] ?? string.Empty;

        public string AppRoot
        {
            get
            {
                // Try configuration first, then fall back to content root
                string path = _configuration["ApplicationRoot"];
                return string.IsNullOrWhiteSpace(path) ? _environment.ContentRootPath : path;
            }
        }

        public string AreasPath
        {
            get
            {
                // Use WebRootPath instead of HostingEnvironment.MapPath
                string path = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "Areas");
                if (Directory.Exists(path))
                {
                    return path;
                }
                return Path.Combine(AppRoot, "Areas");
            }
        }

        /// <summary>
        /// DataPath shows the root folder containing business data.
        /// Uses IConfiguration instead of ConfigurationManager.AppSettings
        /// </summary>
        public string DataPath
        {
            get
            {
                string path = _configuration["DataPath"];
                return string.IsNullOrWhiteSpace(path) ? Path.Combine(AppRoot, "Data") : path;
            }
        }

        private string _workspaceRootPath = string.Empty;

        public string WorkspaceRootPath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_workspaceRootPath))
                    return _workspaceRootPath;

                string path = _configuration["WorkspacePath"] ?? string.Empty;
                int level = 0;

                if (string.IsNullOrWhiteSpace(path))
                    level = 0;
                else if (path.Contains(@"..\"))
                {
                    level = path.Split(@"\".ToCharArray()).Length - 1;
                }
                else
                {
                    _workspaceRootPath = path;
                    return _workspaceRootPath;
                }

                // Find directory without Web.config (use appsettings.json as indicator for web root)
                DirectoryInfo di = new DirectoryInfo(AppRoot);
                while (di.GetFiles("appsettings.json").Length >= 1)
                    di = di.Parent;

                for (int i = 1; i < level; i++)
                {
                    di = di.Parent;
                }

                _workspaceRootPath = Path.Combine(di.FullName, "Workspace");
                return _workspaceRootPath;
            }
        }

        public string WorkspaceComponentRoot => Path.Combine(WorkspaceRootPath, "Components");
        public string WorkspaceModulesRoot => Path.Combine(WorkspaceRootPath, "Modules");
        public string WorkspaceGeneralRoot => Path.Combine(WorkspaceRootPath, "General");
        public string WorkspaceTenantsRoot => Path.Combine(WorkspaceRootPath, "Tenants");

        public string GetModuleWorkspacePath(string moduleName) => Path.Combine(WorkspaceModulesRoot, moduleName);
        public string GetComponentWorkspacePath(string componentName) => Path.Combine(WorkspaceComponentRoot, componentName);

        public bool UseSchemaInDatabaseGeneration => _configuration.GetValue<bool>("UseSchemaInDatabaseGeneration", false);
        public bool CreateDatabase => _configuration.GetValue<bool>("CreateDatabase", false);
        public bool ShowQueries => _configuration.GetValue<bool>("ShowQueries", false);

        private bool? _cacheQueryResults = null;
        public bool CacheQueryResults
        {
            get
            {
                if (_cacheQueryResults.HasValue)
                    return _cacheQueryResults.Value;

                _cacheQueryResults = _configuration.GetValue<bool>("CacheQueryResults", false);
                return _cacheQueryResults.Value;
            }
        }

        private string _themesPath;
        public string ThemesPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_themesPath))
                    return _themesPath;

                _themesPath = _configuration["ThemesPath"] ?? "~/Themes";
                return _themesPath;
            }
        }

        private string _defaultThemeName;
        public string DefaultThemeName
        {
            get
            {
                if (!string.IsNullOrEmpty(_defaultThemeName))
                    return _defaultThemeName;

                _defaultThemeName = _configuration["DefaultThemeName"] ?? "Default";
                return _defaultThemeName;
            }
        }

        private string _activeLayoutName;
        public string ActiveLayoutName
        {
            get
            {
                if (!string.IsNullOrEmpty(_activeLayoutName))
                    return _activeLayoutName;

                _activeLayoutName = _configuration["ActiveLayoutName"] ?? "_Layout";
                return _activeLayoutName;
            }
        }

        public bool ThrowErrorWhenParialContentNotFound => _configuration.GetValue<bool>("ThrowErrorWhenParialContentNotFound", false);

        // Replace HttpContext.Current with IHttpContextAccessor
        public HttpContext HttpContext => _httpContextAccessor.HttpContext;

        public Thread CurrentThread => Thread.CurrentThread;
        public CultureInfo UICulture => Thread.CurrentThread.CurrentUICulture;
        public CultureInfo Culture => Thread.CurrentThread.CurrentCulture;
        public DateTime UTCDateTime => DateTime.UtcNow;
        public DateTime DateTime => System.DateTime.Now;

        public byte[] GetUTCAsBytes() => System.Text.Encoding.UTF8.GetBytes(UTCDateTime.ToBinary().ToString());

        public Uri CurrentRequestURL
        {
            get
            {
                try
                {
                    var request = _httpContextAccessor.HttpContext?.Request;
                    if (request != null)
                    {
                        return new Uri($"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}");
                    }
                    return new Uri("http://NotFound.htm");
                }
                catch
                {
                    return new Uri("http://NotFound.htm");
                }
            }
        }

        public IPrincipal User
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity == null || !httpContext.User.Identity.IsAuthenticated)
                {
                    return CreateUser(Constants.AnonymousUser, Constants.EveryoneRole);
                }
                return httpContext.User;
            }
        }

        public bool TryGetCurrentUser(ref string userName)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                userName = httpContext?.User?.Identity?.Name ?? string.Empty;
                return httpContext?.User?.Identity?.IsAuthenticated ?? false;
            }
            catch
            {
                return false;
            }
        }

        internal static IPrincipal CreateUser(string userName, string roleName)
        {
            // Simplified user creation - in real implementation, integrate with ASP.NET Core Identity
            return new GenericPrincipal(new GenericIdentity(userName), new[] { roleName });
        }

        // Logging properties with caching
        private bool? _isLoggingEnable = null;
        public bool IsLoggingEnable
        {
            get
            {
                if (_isLoggingEnable.HasValue)
                    return _isLoggingEnable.Value;

                _isLoggingEnable = _configuration.GetValue<bool>("IsLoggingEnable", false);
                return _isLoggingEnable.Value;
            }
        }

        private bool? _isPerformanceLoggingEnable = null;
        public bool IsPerformanceLoggingEnable
        {
            get
            {
                if (_isPerformanceLoggingEnable.HasValue)
                    return _isPerformanceLoggingEnable.Value;

                _isPerformanceLoggingEnable = _configuration.GetValue<bool>("IsPerformanceLoggingEnable", false);
                return _isPerformanceLoggingEnable.Value;
            }
        }

        private bool? _isDiagnosticLoggingEnable = null;
        public bool IsDiagnosticLoggingEnable
        {
            get
            {
                if (_isDiagnosticLoggingEnable.HasValue)
                    return _isDiagnosticLoggingEnable.Value;

                _isDiagnosticLoggingEnable = _configuration.GetValue<bool>("IsDiagnosticLoggingEnable", false);
                return _isDiagnosticLoggingEnable.Value;
            }
        }

        private bool? _isCallLoggingEnable = null;
        public bool IsCallLoggingEnable
        {
            get
            {
                if (_isCallLoggingEnable.HasValue)
                    return _isCallLoggingEnable.Value;

                _isCallLoggingEnable = _configuration.GetValue<bool>("IsCallLoggingEnable", false);
                return _isCallLoggingEnable.Value;
            }
        }

        private bool? _isExceptionLoggingEnable = null;
        public bool IsExceptionLoggingEnable
        {
            get
            {
                if (_isExceptionLoggingEnable.HasValue)
                    return _isExceptionLoggingEnable.Value;

                _isExceptionLoggingEnable = _configuration.GetValue<bool>("IsExceptionLoggingEnable", false);
                return _isExceptionLoggingEnable.Value;
            }
        }

        private bool? _isDataLoggingEnable = null;
        public bool IsDataLoggingEnable
        {
            get
            {
                if (_isDataLoggingEnable.HasValue)
                    return _isDataLoggingEnable.Value;

                _isDataLoggingEnable = _configuration.GetValue<bool>("IsDataLoggingEnable", false);
                return _isDataLoggingEnable.Value;
            }
        }

        private string _tenantId = "";
        public string TenantId
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_tenantId))
                    return _tenantId;

                _tenantId = _configuration["TenantId"] ?? string.Empty;
                return _tenantId;
            }
        }

        private int? _conversationIsolationLevel = null;
        public int ConversationIsolationLevel
        {
            get
            {
                if (_conversationIsolationLevel.HasValue)
                    return _conversationIsolationLevel.Value;

                _conversationIsolationLevel = _configuration.GetValue<int>("ConversationIsolationLevel", 2);
                return _conversationIsolationLevel.Value;
            }
        }
    }

    /// <summary>
    /// Extension methods for registering AppConfiguration in .NET Core DI container
    /// </summary>
    public static class AppConfigurationExtensions
    {
        public static IServiceCollection AddAppConfiguration(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<AppConfiguration>();
            return services;
        }
    }
}