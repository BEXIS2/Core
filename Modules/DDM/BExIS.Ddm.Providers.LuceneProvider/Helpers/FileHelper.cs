using System.IO;
using Vaiona.Util.Cfg;

namespace BExIS.Ddm.Providers.LuceneProvider.Helpers
{
    public static class FileHelper
    {
        public static string LuceneRoot
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene")); }
        }

        public static string IndexFolderPath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene", "SearchIndex")); }
        }

        public static string DataFolderPath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene", "Data")); }
        }

        public static string ConfigFilePath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene", "Config", "LuceneConfig.xml")); }
        }

        public static string ConfigBackUpFilePath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene", "Config", "BackUp", "LuceneConfig.xml")); }
        }
    }
}
