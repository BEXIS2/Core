using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vaiona.Web.Mvc.Modularity
{
    public class ModuleInfo
    {
        public ModuleManifest Manifest { get; set; }
        public DirectoryInfo Path { get; set; }

        public string Id { get; set; }
        public Type EntryType { get; set; }

        public ModuleBase Plugin { get; set; }

        public Assembly Assembly { get; set; }

    }
}
