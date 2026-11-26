using BEXIS2.App.Logging;
using BEXIS2.App.Logging.Loggers;
using BEXIS2.App.MultiTenancy.Api;
using BEXIS2.App.MultiTenancy.Services;
using BEXIS2.DB.Persistence.Api;
using BEXIS2.DB.PersistenceProviders.NH;
using BEXIS2.Security.Entities.Subjects;
using BExIS.Utils.Config;
using Microsoft.AspNet.Identity;
using Vaiona.Web.Mvc.Modularity;
using BEXIS2.Utils.Data.Helpers;
using BExIS.Security.Services.Subjects;

namespace BEXIS2.Web.Shell.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIoCConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Persistence
            services.AddSingleton<IPersistenceManager, NHibernatePersistenceManager>();

            // Logging
            //services.AddSingleton<ILogger>(sp => 
            //    ActivatorUtilities.CreateInstance<DatabaseLogger>(sp));

            // Multi-Tenancy
            services.AddSingleton<ITenantRegistrar, TenantRegistrar>();
            services.AddSingleton<ITenantResolver, TenantResolver>();

            // Module Seed Data
            services.AddTransient<IModuleSeedDataGenerator, ShellSeedDataGenerator>();

            // General Settings
            services.AddSingleton<GeneralSettings>();

            // Identity - User Manager & Stores
            services.AddTransient<UserManager>();
            services.AddTransient<IUserStore<User, long>, UserStore>();
            services.AddTransient<UserStore>();

            // Identity - Group/Role Manager & Stores
            services.AddTransient<GroupManager>();
            services.AddTransient<IRoleStore<Group, long>, GroupStore>();
            services.AddTransient<GroupStore>();

            return services;
        }
    }
}
