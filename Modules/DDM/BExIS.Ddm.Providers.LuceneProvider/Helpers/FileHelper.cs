using System.IO;
using Vaiona.Utils.Cfg;

/// <summary>
///
/// </summary>
namespace BExIS.Ddm.Providers.LuceneProvider.Helpers
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public static class FileHelper
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public static string LuceneRoot
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene")); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public static string IndexFolderPath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene", "SearchIndex")); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public static string DataFolderPath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene", "Data")); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public static string ConfigFilePath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene", "Config", "LuceneConfig.xml")); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public static string LuceneManagerConfigFilePath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("Search"), "Lucene", "Config", "LuceneManagerConfig.xml")); }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public static string ConfigBackUpFilePath
        {
            get { return (Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene", "Config", "BackUp", "LuceneConfig.xml")); }
        }
    }
}