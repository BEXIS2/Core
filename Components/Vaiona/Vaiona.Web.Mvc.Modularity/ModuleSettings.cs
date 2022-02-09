using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Utils.Cfg;

namespace Vaiona.Web.Mvc.Modularity
{
    public class ModuleSettings: Settings
    {
        public ModuleSettings(string moduleId): 
            base(moduleId, 
                Path.Combine(AppConfiguration.GetModuleWorkspacePath(moduleId), $"{moduleId}.Settings.json")
                )
        {
            
        }
    }
}
