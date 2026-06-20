using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Model.MTnt;

namespace BExIS.Web.Shell.Helpers
{
    /// <summary>
    /// .NET Core session management that replaces System.Web.SessionState functionality.
    /// Provides strongly-typed session access and cross-platform compatibility.
    /// </summary>
    public interface ISessionManager
    {
        // Basic session operations
        T Get<T>(string key);
        void Set<T>(string key, T value);
        void Remove(string key);
        void Clear();
        bool Exists(string key);
        IEnumerable<string> Keys { get; }

        // BExIS-specific session methods
        void SetTenant(Tenant tenant);
        Tenant GetTenant();
        void SetUser(string userName, bool isAuthenticated);
        (string UserName, bool IsAuthenticated) GetUser();
        void ApplyCulture(string culture);
        string GetCulture();
        
        // Session lifecycle
        Task InitializeAsync();
        Task RefreshAsync();
        bool IsNewSession { get; }
    }

    /// <summary>
    /// Implementation of session management for ASP.NET Core.
    /// Replaces the static session access patterns from System.Web.
    /// </summary>
    public class SessionManager : ISessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SessionManager> _logger;

        public SessionManager(IHttpContextAccessor httpContextAccessor, ILogger<SessionManager> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private ISession Session => _httpContextAccessor.HttpContext?.Session;

        public bool IsNewSession
        {
            get
            {
                var session = Session;
                return session != null && !session.Keys.Any();
            }
        }

        public IEnumerable<string> Keys => Session?.Keys ?? new List<string>();

        /// <summary>
        /// Gets a strongly-typed value from session.
        /// Replaces: HttpContext.Current.Session["key"]
        /// </summary>
        public T Get<T>(string key)
        {
            try
            {
                var session = Session;
                if (session == null) return default(T);

                var value = session.GetString(key);
                if (string.IsNullOrEmpty(value))
                    return default(T);

                if (typeof(T) == typeof(string))
                    return (T)(object)value;

                if (typeof(T).IsValueType)
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session value for key: {Key}", key);
                return default(T);
            }
        }

        /// <summary>
        /// Sets a strongly-typed value in session.
        /// Replaces: HttpContext.Current.Session["key"] = value
        /// </summary>
        public void Set<T>(string key, T value)
        {
            try
            {
                var session = Session;
                if (session == null) return;

                if (value == null)
                {
                    session.Remove(key);
                    return;
                }

                string serializedValue;
                if (typeof(T) == typeof(string))
                {
                    serializedValue = value.ToString();
                }
                else if (typeof(T).IsValueType)
                {
                    serializedValue = value.ToString();
                }
                else
                {
                    serializedValue = JsonConvert.SerializeObject(value);
                }

                session.SetString(key, serializedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting session value for key: {Key}", key);
            }
        }

        /// <summary>
        /// Removes a value from session.
        /// Replaces: HttpContext.Current.Session.Remove("key")
        /// </summary>
        public void Remove(string key)
        {
            try
            {
                Session?.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing session value for key: {Key}", key);
            }
        }

        /// <summary>
        /// Clears all session values.
        /// Replaces: HttpContext.Current.Session.Clear()
        /// </summary>
        public void Clear()
        {
            try
            {
                Session?.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing session");
            }
        }

        /// <summary>
        /// Checks if a session key exists.
        /// Replaces: HttpContext.Current.Session["key"] != null
        /// </summary>
        public bool Exists(string key)
        {
            try
            {
                var session = Session;
                return session != null && session.Keys.Contains(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking session key existence: {Key}", key);
                return false;
            }
        }

        #region BExIS-Specific Session Methods

        /// <summary>
        /// Sets the current tenant in session.
        /// Replaces: Session["Tenant"] = tenant
        /// </summary>
        public void SetTenant(Tenant tenant)
        {
            const string key = "CurrentTenant";
            Set(key, tenant);
            _logger.LogDebug("Tenant set in session: {TenantId}", tenant?.Id);
        }

        /// <summary>
        /// Gets the current tenant from session.
        /// Replaces: (Tenant)Session["Tenant"]
        /// </summary>
        public Tenant GetTenant()
        {
            const string key = "CurrentTenant";
            return Get<Tenant>(key);
        }

        /// <summary>
        /// Sets user information in session.
        /// Replaces manual session management for user state.
        /// </summary>
        public void SetUser(string userName, bool isAuthenticated)
        {
            Set("UserName", userName);
            Set("IsAuthenticated", isAuthenticated);
            _logger.LogDebug("User set in session: {UserName}, Authenticated: {IsAuthenticated}", userName, isAuthenticated);
        }

        /// <summary>
        /// Gets user information from session.
        /// </summary>
        public (string UserName, bool IsAuthenticated) GetUser()
        {
            var userName = Get<string>("UserName") ?? string.Empty;
            var isAuthenticated = Get<bool>("IsAuthenticated");
            return (userName, isAuthenticated);
        }

        /// <summary>
        /// Applies culture settings to the current thread and stores in session.
        /// Replaces: Thread.CurrentThread.CurrentCulture = culture
        /// </summary>
        public void ApplyCulture(string culture)
        {
            try
            {
                var cultureInfo = new System.Globalization.CultureInfo(culture);
                System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
                System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
                
                Set("Culture", culture);
                _logger.LogDebug("Culture applied: {Culture}", culture);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying culture: {Culture}", culture);
            }
        }

        /// <summary>
        /// Gets the current culture from session.
        /// </summary>
        public string GetCulture()
        {
            return Get<string>("Culture") ?? "en-US";
        }

        #endregion

        #region Session Lifecycle

        /// <summary>
        /// Initializes a new session with default values.
        /// Replaces: Global.asax Session_Start logic
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                if (IsNewSession)
                {
                    _logger.LogInformation("Initializing new session");

                    // Set default values
                    Set("SessionStartTime", DateTime.UtcNow);
                    Set("SessionId", Guid.NewGuid().ToString());
                    
                    // Apply default culture
                    ApplyCulture("en-US");
                    
                    _logger.LogDebug("Session initialized successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing session");
                throw;
            }
        }

        /// <summary>
        /// Refreshes session timeout.
        /// Useful for keeping active sessions alive.
        /// </summary>
        public async Task RefreshAsync()
        {
            try
            {
                var session = Session;
                if (session != null)
                {
                    Set("LastActivity", DateTime.UtcNow);
                    await session.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing session");
            }
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for ISession to provide backward compatibility
    /// with existing BExIS session usage patterns.
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Extension method that replicates the old Session.ApplyCulture functionality.
        /// Usage: context.Session.ApplyCulture("en-US")
        /// </summary>
        public static void ApplyCulture(this ISession session, string defaultCulture)
        {
            try
            {
                var culture = new System.Globalization.CultureInfo(defaultCulture);
                System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                
                session.SetString("Culture", defaultCulture);
            }
            catch (Exception)
            {
                // Fallback to en-US if culture is invalid
                var fallbackCulture = new System.Globalization.CultureInfo("en-US");
                System.Threading.Thread.CurrentThread.CurrentCulture = fallbackCulture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = fallbackCulture;
                session.SetString("Culture", "en-US");
            }
        }

        /// <summary>
        /// Extension method for setting tenant in session.
        /// Usage: context.Session.SetTenant(tenant)
        /// </summary>
        public static void SetTenant(this ISession session, Tenant tenant)
        {
            if (tenant != null)
            {
                var tenantJson = JsonConvert.SerializeObject(tenant);
                session.SetString("CurrentTenant", tenantJson);
                session.SetString("TenantId", tenant.Id.ToString());
                session.SetString("TenantName", tenant.Name ?? string.Empty);
            }
        }

        /// <summary>
        /// Extension method for getting tenant from session.
        /// Usage: var tenant = context.Session.GetTenant()
        /// </summary>
        public static Tenant GetTenant(this ISession session)
        {
            try
            {
                var tenantJson = session.GetString("CurrentTenant");
                if (!string.IsNullOrEmpty(tenantJson))
                {
                    return JsonConvert.DeserializeObject<Tenant>(tenantJson);
                }

                // Fallback: try to construct from individual properties
                var tenantIdString = session.GetString("TenantId");
                var tenantName = session.GetString("TenantName");
                
                if (!string.IsNullOrEmpty(tenantIdString) && long.TryParse(tenantIdString, out long tenantId))
                {
                    return new Tenant { Id = tenantId, Name = tenantName };
                }
            }
            catch (Exception)
            {
                // Return null if deserialization fails
            }
            
            return null;
        }

        /// <summary>
        /// Generic extension method for getting strongly-typed values from session.
        /// Usage: var value = context.Session.Get<MyType>("key")
        /// </summary>
        public static T Get<T>(this ISession session, string key)
        {
            try
            {
                var value = session.GetString(key);
                if (string.IsNullOrEmpty(value))
                    return default(T);

                if (typeof(T) == typeof(string))
                    return (T)(object)value;

                if (typeof(T).IsValueType)
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                return JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// Generic extension method for setting strongly-typed values in session.
        /// Usage: context.Session.Set("key", myObject)
        /// </summary>
        public static void Set<T>(this ISession session, string key, T value)
        {
            try
            {
                if (value == null)
                {
                    session.Remove(key);
                    return;
                }

                string serializedValue;
                if (typeof(T) == typeof(string))
                {
                    serializedValue = value.ToString();
                }
                else if (typeof(T).IsValueType)
                {
                    serializedValue = value.ToString();
                }
                else
                {
                    serializedValue = JsonConvert.SerializeObject(value);
                }

                session.SetString(key, serializedValue);
            }
            catch
            {
                // Silently fail - logging should be handled at higher level
            }
        }
    }

    /// <summary>
    /// Service collection extensions for registering session management services.
    /// </summary>
    public static class SessionManagerServiceExtensions
    {
        /// <summary>
        /// Adds session management services to the dependency injection container.
        /// Call this in Startup.ConfigureServices.
        /// </summary>
        public static IServiceCollection AddBExISSessionManagement(this IServiceCollection services)
        {
            // Add distributed memory cache for session storage
            services.AddDistributedMemoryCache();
            
            // Configure session options
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "BExIS.SessionId";
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            // Register session manager
            services.AddScoped<ISessionManager, SessionManager>();

            return services;
        }
    }
}

/*
Migration Guide: System.Web.SessionState to ASP.NET Core Sessions

OLD (.NET Framework):
===================
1. HttpContext.Current.Session["key"] = value
2. var value = (MyType)HttpContext.Current.Session["key"]
3. HttpContext.Current.Session.Remove("key")
4. HttpContext.Current.Session.Clear()
5. HttpContext.Current.Session.Abandon()

NEW (.NET Core):
===============
1. Using ISessionManager (Dependency Injection):
   - _sessionManager.Set("key", value)
   - var value = _sessionManager.Get<MyType>("key")
   - _sessionManager.Remove("key")
   - _sessionManager.Clear()

2. Using ISession directly:
   - context.Session.Set("key", value) // extension method
   - var value = context.Session.Get<MyType>("key") // extension method
   - context.Session.Remove("key")
   - context.Session.Clear()

3. In Controllers:
   - HttpContext.Session.Set("key", value)
   - var value = HttpContext.Session.Get<MyType>("key")

Key Differences:
================
1. No more static HttpContext.Current access
2. Dependency injection replaces static access
3. Sessions are async-aware (CommitAsync)
4. Better type safety with generic methods
5. JSON serialization for complex objects
6. Configurable session providers (memory, Redis, SQL Server)
7. Cross-platform compatible storage

Configuration Required:
======================
1. Add services.AddSession() in ConfigureServices
2. Add app.UseSession() in Configure pipeline
3. Register ISessionManager in DI container
4. Configure session options (timeout, cookie settings)

Benefits:
=========
1. Testable (can mock ISessionManager)
2. Type-safe with generics
3. Better error handling
4. Configurable storage backends
5. Linux/Docker compatible
6. Better security options
*/