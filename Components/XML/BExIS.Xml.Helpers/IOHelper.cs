using System.IO;

namespace BExIS.Xml.Helpers
{
    public class IOHelper
    {
        public static string GetDynamicStorePath(long datasetId, long datasetVersionOrderNr, string title, string extention)
        {
            string storePath = Path.Combine("Datasets", datasetId.ToString(), "DatasetVersions");

            return Path.Combine(storePath, datasetId + "_" + datasetVersionOrderNr + "_" + title + extention);
        }
    }
}