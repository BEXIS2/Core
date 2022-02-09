using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vaiona.Model.MTnt
{
    public interface ITenantPathProvider
    {
        string GetImagePath(string tenantId, string logoFileName, string fallbackTenantId);
        string GetThemePath(string tenantId, string logoFileName, string fallbackTenantId);
        string GetContentFilePath(string tenantId, string logoFileName, string fallbackTenantId);
    }
}
