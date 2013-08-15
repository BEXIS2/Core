using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Vaiona.Util.Cfg;

namespace BExIS.Search.Providers.LuceneProvider.Helpers
{
    public static class FileHelper
    {
        public static string LuceneRoot
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("Search"), "Lucene")); }
        }

        public static string IndexFolderPath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("Search"), "Lucene", "SearchIndex")); }
        }

        public static string DataFolderPath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("Search"), "Lucene", "Data")); }
        }

        public static string ConfigFilePath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("Search"), "Lucene", "Config", "LuceneConfig.xml")); }
        }
    }
}
