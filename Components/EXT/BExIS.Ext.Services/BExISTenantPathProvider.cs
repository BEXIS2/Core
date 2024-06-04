using System.IO;
using Vaiona.MultiTenancy.Services;
using Vaiona.Utils.Cfg;

namespace BExIS.Ext.Services
{
    public class BExISTenantPathProvider : DefaultTenantPathProvider
    {
        protected override string GetImagePathFromFallbackTenant(string fallbackTenantId, string imageFileName)
        {
            // Check if the resouce exists in the fallback tenant's folder, if not check the application folders
            string basePath = base.GetImagePathFromFallbackTenant(fallbackTenantId, imageFileName);
            if (File.Exists(basePath))
                return basePath;
            return Path.Combine(AppConfiguration.AppRoot, "Content", "Images", imageFileName);
        }

        protected override string GetThemePathFromFallbackTenant(string fallbackTenantId, string themeName)
        {
            // Check if the resouce exists in the fallback tenant's folder, if not check the application folders
            string basePath = base.GetThemePathFromFallbackTenant(fallbackTenantId, themeName);
            if (Directory.Exists(basePath))
                return basePath;
            return Path.Combine(AppConfiguration.AppRoot, "Themes", themeName);
            //return Path.Combine(AppConfiguration.ThemesPath, themeName);
        }

        protected override string GetContentFilePathFromFallbackTenant(string fallbackTenantId, string contentFileName)
        {
            // Check if the resouce exists in the fallback tenant's folder, if not check the application folders
            string basePath = base.GetContentFilePathFromFallbackTenant(fallbackTenantId, contentFileName);
            if (File.Exists(basePath))
                return basePath;
            return Path.Combine(AppConfiguration.AppRoot, "Content", "Static", contentFileName);
        }
    }
}