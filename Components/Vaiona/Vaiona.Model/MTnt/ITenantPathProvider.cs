namespace Vaiona.Model.MTnt
{
    public interface ITenantPathProvider
    {
        string GetImagePath(string tenantId, string logoFileName, string fallbackTenantId);

        string GetThemePath(string tenantId, string logoFileName, string fallbackTenantId);

        string GetContentFilePath(string tenantId, string logoFileName, string fallbackTenantId);
    }
}