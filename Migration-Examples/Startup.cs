using BExIS.App.Bootstrap;
using BExIS.UI.Helpers;
using BExIS.Utils.Config;
using BExIS.Web.Shell.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using Vaiona.IoC;
using Vaiona.Model.MTnt;
using Vaiona.MultiTenancy.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell
{
    /// <summary>
    /// .NET Core Startup class that replaces Global.asax.cs functionality.
    /// Handles application configuration, dependency injection, and middleware pipeline.
    /// </summary>
    public class Startup
    {
        private BExIS.App.Bootstrap.Application _app = null;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure JSON serialization (replaces Global.asax JsonConvert.DefaultSettings)
            services.Configure<MvcNewtonsoftJsonOptions>(options =>
            {
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            // Add framework services
            services.AddControllersWithViews()
                .AddNewtonsoftJson() // For compatibility with existing JSON handling
                .AddRazorRuntimeCompilation(); // For development-time view compilation

            // Add session services (replaces System.Web.SessionState)
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = "BExIS.SessionId"; // Custom session cookie name
            });

            // Add AppConfiguration with dependency injection
            services.AddAppConfiguration();

            // Add HTTP context accessor for accessing HTTP context in services
            services.AddHttpContextAccessor();

            // Add custom view location expanders (replaces Global.asax CustomViewEngine)
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new CustomViewLocationExpander());
            });

            // Add CORS services
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add IoC container integration
            ConfigureIoCContainer(services);

            // Register BExIS application services
            ConfigureBExISServices(services);

            // Add bundling and minification (replaces BundleConfig)
            services.AddWebOptimizer(pipeline =>
            {
                // Configure CSS and JS bundling here
                pipeline.AddCssBundle("/css/bundle.css", "css/**/*.css");
                pipeline.AddJavaScriptBundle("/js/bundle.js", "js/**/*.js");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            // Configure error handling (replaces Application_Error in Global.asax)
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Add custom error handling middleware
            app.UseMiddleware<CustomErrorHandlingMiddleware>();

            // Security headers (replaces Application_PreSendRequestHeaders)
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Remove("Server");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                await next();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Use bundling and minification
            app.UseWebOptimizer();

            // CORS middleware (replaces Application_BeginRequest CORS headers)
            app.UseCors();

            // Session middleware (replaces System.Web.SessionState)
            app.UseSession();

            // Custom session initialization middleware (replaces Session_Start)
            app.UseMiddleware<SessionInitializationMiddleware>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                // API routes
                endpoints.MapControllers();
            });

            // Initialize BExIS application (replaces Application_Start)
            InitializeBExISApplication();

            logger.LogInformation("BExIS application started successfully");
        }

        private void ConfigureIoCContainer(IServiceCollection services)
        {
            // Configure IoC container integration
            // This replaces the IoC initialization from Global.asax
            services.AddSingleton<ITenantResolver, DefaultTenantResolver>();
            
            // Add other IoC registrations here
        }

        private void ConfigureBExISServices(IServiceCollection services)
        {
            // Register BExIS-specific services
            services.AddScoped<IPersistenceManager, PersistenceManager>();
            
            // Add other BExIS service registrations
        }

        private void InitializeBExISApplication()
        {
            // This replaces Application_Start logic from Global.asax
            try
            {
                _app = BExIS.App.Bootstrap.Application.GetInstance(RunStage.Production);
                _app.Start(ConfigureWebApi, true);
            }
            catch (Exception ex)
            {
                // Log startup errors
                // Consider using ILogger here
                throw new ApplicationException("Failed to initialize BExIS application", ex);
            }
        }

        private void ConfigureWebApi(IServiceCollection services)
        {
            // Web API configuration - replaces WebApiConfig.Register
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });
        }

        // This replaces Application_End from Global.asax
        public void Dispose()
        {
            _app?.Stop();
        }
    }

    /// <summary>
    /// Custom view location expander that replaces the CustomViewEngine logic from Global.asax
    /// </summary>
    public class CustomViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            // Add additional view search paths
            var additionalLocations = new[]
            {
                "/Areas/{2}/Views/{1}/{0}.cshtml",
                "/Areas/{2}/Views/Shared/{0}.cshtml",
                "/Areas/{2}/{3}.{1}.UI/Views/{1}/{0}.cshtml",  // One directory lower
                "/Areas/{2}/{3}.{1}.UI/Views/Shared/{0}.cshtml"
            };

            return additionalLocations.Concat(viewLocations);
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // Can add contextual values for view location expansion
        }
    }

    /// <summary>
    /// Middleware that handles session initialization (replaces Session_Start from Global.asax)
    /// </summary>
    public class SessionInitializationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public SessionInitializationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if this is a new session
            if (!context.Session.Keys.Any())
            {
                await InitializeSession(context);
            }

            await _next(context);
        }

        private async Task InitializeSession(HttpContext context)
        {
            try
            {
                // Session timeout handling (replaces Global.asax Session_Start logic)
                var sessionCookie = context.Request.Cookies["BExIS.SessionId"];
                if (!string.IsNullOrEmpty(sessionCookie))
                {
                    // Handle session timeout scenario
                    context.Response.Redirect("/Home/SessionTimeout");
                    return;
                }

                // Initialize IoC session-level container
                using (var scope = _serviceProvider.CreateScope())
                {
                    var iocFactory = scope.ServiceProvider.GetService<IoCFactory>();
                    iocFactory?.Container?.StartSessionLevelContainer();

                    // Apply culture settings
                    var appConfig = scope.ServiceProvider.GetService<AppConfiguration>();
                    if (appConfig != null)
                    {
                        context.Session.ApplyCulture(appConfig.DefaultCulture);
                    }

                    // Resolve tenant
                    var tenantResolver = scope.ServiceProvider.GetService<ITenantResolver>();
                    if (tenantResolver != null)
                    {
                        var tenant = tenantResolver.Resolve(context.Request);
                        context.Session.SetTenant(tenant);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log session initialization errors
                // In production, consider graceful degradation
                throw new ApplicationException("Failed to initialize session", ex);
            }
        }
    }

    /// <summary>
    /// Custom error handling middleware (replaces Application_Error from Global.asax)
    /// </summary>
    public class CustomErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomErrorHandlingMiddleware> _logger;
        private readonly IConfiguration _configuration;

        public CustomErrorHandlingMiddleware(RequestDelegate next, ILogger<CustomErrorHandlingMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            bool sendExceptions = _configuration.GetValue<bool>("SendExceptions", false);
            var statusCode = 500;

            if (exception is HttpRequestException httpEx)
            {
                statusCode = 400; // Bad Request
            }

            _logger.LogError(exception, "An unhandled exception occurred");

            // Replaces the error email functionality from Global.asax
            if (sendExceptions && 
                statusCode != 404 && 
                !(exception is InvalidOperationException) && 
                !exception.Message.StartsWith("Multiple types were found that match the controller named"))
            {
                await SendErrorEmail(exception);
            }

            context.Response.StatusCode = statusCode;
            
            if (!context.Response.HasStarted)
            {
                await context.Response.WriteAsync("An error occurred while processing your request.");
            }
        }

        private async Task SendErrorEmail(Exception exception)
        {
            try
            {
                // Implement error email functionality
                // This replaces ErrorHelper.SendEmailWithErrors from Global.asax
                _logger.LogCritical(exception, "Critical error occurred - email notification should be sent");
            }
            catch (Exception emailEx)
            {
                _logger.LogError(emailEx, "Failed to send error notification email");
            }
        }
    }

    /// <summary>
    /// Extension methods for session management (replaces System.Web.SessionState extensions)
    /// </summary>
    public static class SessionExtensions
    {
        public static void ApplyCulture(this ISession session, string defaultCulture)
        {
            // Apply culture settings to current thread
            var culture = new System.Globalization.CultureInfo(defaultCulture);
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            
            // Store in session for persistence
            session.SetString("Culture", defaultCulture);
        }

        public static void SetTenant(this ISession session, Tenant tenant)
        {
            if (tenant != null)
            {
                session.SetString("TenantId", tenant.Id.ToString());
                session.SetString("TenantName", tenant.Name ?? string.Empty);
            }
        }

        public static Tenant GetTenant(this ISession session)
        {
            var tenantId = session.GetString("TenantId");
            var tenantName = session.GetString("TenantName");
            
            if (!string.IsNullOrEmpty(tenantId) && long.TryParse(tenantId, out long id))
            {
                return new Tenant { Id = id, Name = tenantName };
            }
            
            return null;
        }
    }
}