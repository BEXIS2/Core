using System;
using System.IO;
using System.Reflection;

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