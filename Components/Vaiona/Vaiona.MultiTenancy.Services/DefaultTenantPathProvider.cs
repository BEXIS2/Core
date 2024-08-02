using System.IO;
using Vaiona.Model.MTnt;
using Vaiona.Utils.Cfg;

namespace Vaiona.MultiTenancy.Services
{
    public class DefaultTenantPathProvider : ITenantPathProvider
    {
        // write two overridable functions per public method, and keep the logic here. So that sub classes in client applications can customize the path determination logic for the resouces
        public string GetImagePath(string tenantId, string logoFileName, string fallbackTenantId)
        {
            string path = GetImagePathFromTenant(tenantId, logoFileName);
            if (File.Exists(path))
                return path;
            if (fallbackTenantId != null)
            {
                path = GetImagePathFromFallbackTenant(fallbackTenantId, logoFileName);
                if (File.Exists(path))
                    return path;
            }
            return string.Empty;
        }

        protected virtual string GetImagePathFromTenant(string tenantId, string imageFileName)
        {
            return Path.Combine(AppConfiguration.WorkspaceTenantsRoot, tenantId, "images", imageFileName);
        }

        protected virtual string GetImagePathFromFallbackTenant(string fallbackTenantId, string imageFileName)
        {
            return Path.Combine(AppConfiguration.WorkspaceTenantsRoot, fallbackTenantId, "images", imageFileName);
        }

        public string GetThemePath(string tenantId, string themeName, string fallbackTenantId)
        {
            string path = GetThemePathFromTenant(tenantId, themeName);
            if (Directory.Exists(path))
                return path;
            if (fallbackTenantId != null)
            {
                path = GetThemePathFromFallbackTenant(fallbackTenantId, themeName);
                if (Directory.Exists(path))
                    return path;
            }
            return string.Empty;
        }

        protected virtual string GetThemePathFromTenant(string tenantId, string themeName)
        {
            return Path.Combine(AppConfiguration.WorkspaceTenantsRoot, tenantId, "themes", themeName);
        }

        protected virtual string GetThemePathFromFallbackTenant(string fallbackTenantId, string themeName)
        {
            return Path.Combine(AppConfiguration.WorkspaceTenantsRoot, fallbackTenantId, "themes", themeName);
        }

        public virtual string GetContentFilePath(string tenantId, string contentFileName, string fallbackTenantId)
        {
            string path = GetContentFilePathFromTenant(tenantId, contentFileName);
            if (File.Exists(path))
                return path;
            if (fallbackTenantId != null)
            {
                path = GetContentFilePathFromFallbackTenant(fallbackTenantId, contentFileName);
                if (File.Exists(path))
                    return path;
            }
            return string.Empty;
        }

        protected virtual string GetContentFilePathFromTenant(string tenantId, string contentFileName)
        {
            return Path.Combine(AppConfiguration.WorkspaceTenantsRoot, tenantId, "contents", contentFileName);
        }

        protected virtual string GetContentFilePathFromFallbackTenant(string fallbackTenantId, string contentFileName)
        {
            return Path.Combine(AppConfiguration.WorkspaceTenantsRoot, fallbackTenantId, "contents", contentFileName);
        }
    }
}