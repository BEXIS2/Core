using System.IO;
using BEXIS2.Utils.Cfg;

namespace Vaiona.Web.Mvc.Modularity
{
    public class ModuleSettings : Settings
    {
        public ModuleSettings(string moduleId) :
            base(moduleId,
                Path.Combine(AppConfiguration.GetModuleWorkspacePath(moduleId), $"{moduleId}.Settings.json")
                )
        {
        }
    }
}