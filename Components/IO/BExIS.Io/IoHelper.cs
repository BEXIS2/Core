using BExIS.Utils.Config;
using System;
using System.IO;

namespace BExIS.IO
{
    public class IOHelper
    {
        public static string GetDynamicStorePath(long datasetId, long datasetVersionOrderNr, string title, string extention)
        {
            if (datasetId < 1) throw new Exception("Dataset id can not be less then 1.");
            if (datasetVersionOrderNr < 1) throw new Exception("Dataset version number can not be less then 1.");
            if (string.IsNullOrEmpty(title)) throw new Exception("Title should not be Empty.");
            if (string.IsNullOrEmpty(extention)) throw new Exception("Extention should not be Empty.");
            if (extention.IndexOf('.') == -1) throw new Exception("Extention should start with '.' .");

            string storePath = Path.Combine("Datasets", datasetId.ToString(), "DatasetVersions");
            string fileName = datasetId + "_" + datasetVersionOrderNr + "_" + title + extention;//GetFileName(FileType.None, datasetId, (int)datasetVersionOrderNr, 0, title);

            return Path.Combine(storePath, fileName);
        }

        public static string GetDynamicStoreFilePath(long datasetId, long datasetVersionOrderNr, string name, string extention)
        {
            if (datasetId < 1) throw new Exception("Dataset id can not be less then 1.");
            if (datasetVersionOrderNr < 1) throw new Exception("Dataset version number can not be less then 1.");
            if (string.IsNullOrEmpty(name)) throw new Exception("Title should not be Empty.");
            if (string.IsNullOrEmpty(extention)) throw new Exception("Extention should not be Empty.");
            if (extention.IndexOf('.') == -1) throw new Exception("Extention should start with '.' .");

            string storePath = Path.Combine("Datasets", datasetId.ToString(), "DatasetVersions");
            string fileName = GetFileName(FileType.None, datasetId, (int)datasetVersionOrderNr, 0, name);

            return Path.Combine(storePath, fileName + extention);
        }

        public static string GetFileName(FileType type, long datasetId, int versionNr, long datastructureId, string title = "")
        {
            string appName = GeneralSettings.ApplicationName;
            if (string.IsNullOrEmpty(appName)) appName = "BEXIS2";

            string downloadName = "no title available";
            string downloadDate = DateTime.Now.ToString("yyyyMMdd");

            string downloadTitle = title.Replace(" ", "");

            switch (type)
            {
                case FileType.Metadata:
                    // filename should contain: application name, dataset ID, and version ID
                    downloadName = string.Format("{0}_{1}_v{2}_metadata", appName, datasetId, versionNr);
                    break;
                case FileType.MetadataExport:
                    // filename should contain: application name, dataset ID, and version ID
                    downloadName = string.Format("{0}_{1}_v{2}_metadata_{3}", appName, datasetId, versionNr, title);
                    break;
                case FileType.DataStructure:
                    downloadName = string.Format("{0}_{1}_datastructure_{2}", appName, datasetId, datastructureId);
                    break;
                case FileType.PrimaryData:
                    downloadName = string.Format("{0}_{1}_v{2}_data", appName, datasetId, versionNr);

                    break;
                case FileType.PrimaryDataFiles:
                    downloadName = string.Format("{0}_{1}_v{2}_data_{3}", appName, datasetId, versionNr, title);
                    break;
                case FileType.Attachments:
                    downloadName = string.Format("{0}_{1}_v{2}_attachment_{3}", appName, datasetId, versionNr, title);
                    break;
                case FileType.Bundle:
                    downloadName = string.Format("{0}_{1}_v{2}_{3}_{4}", appName, datasetId, versionNr, downloadTitle, downloadDate);
                    break;
                case FileType.Manifest:
                    downloadName = string.Format("{0}_{1}_v{2}_general-metadata", appName, datasetId, versionNr);
                    break;
                default:
                    downloadName = string.Format("{0}_{1}_v{2}_{3}", appName, datasetId, versionNr, title);
                    break;
            }


            return downloadName;
        }

    }

    public enum FileType
    {
        Metadata,
        MetadataExport,
        PrimaryData,
        PrimaryDataFiles,
        DataStructure,
        Attachments,
        Bundle,
        Manifest,
        None
    }

    public enum DecimalCharacter
    {
        point = 46,
        comma = 44,
    }

    public enum Orientation
    {
        columnwise,
        rowwise
    }

    /// <summary>
    /// TextSeperator is a list of different characters which used as seperator in ascii files
    /// </summary>
    /// <remarks></remarks>
    public enum TextSeperator
    {
        tab = 9,
        comma = 44,
        semicolon = 59,
        space = 32
    }

    /// <summary>
    /// TextMarker is a list of different characters which used as textmarker in ascii files
    /// </summary>
    /// <remarks></remarks>
    public enum TextMarker
    {
        quotes = 39,
        doubleQuotes = 34,
        none = 0,
    }
}