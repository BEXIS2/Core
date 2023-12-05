using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.IO;
using BExIS.IO.DataType.DisplayPattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class ShowPrimaryDataModel
    {
        public DataTable Data { get; set; }

        public long DatasetId { get; set; }

        public int VersionId { get; set; }

        public bool LatestVersion { get; set; }

        public String DatasetTitle { get; set; }

        public List<BasicFileInfo> FileList { get; set; }

        public DataStructure DataStructure { get; set; }

        public DataStructureType DataStructureType { get; set; }

        public List<string> CompareValuesOfDataTypes { get; set; }

        public List<DisplayFormatObject> DisplayFormats { get; set; }

        public bool DownloadAccess { get; set; }

        public bool HasEditRight { get; set; }

        public Dictionary<string, string> AsciiFileDownloadSupport { get; set; }

        public static ShowPrimaryDataModel Convert(long datasetId, int versionId, string title, DataStructure dataStructure, DataTable data, bool downloadAccess, Dictionary<string, string> supportedAsciiFileTypes, bool latestVersion, bool hasEditRights)
        {
            ShowPrimaryDataModel model = new ShowPrimaryDataModel();
            model.Data = data;
            model.DatasetId = datasetId;
            model.VersionId = versionId;
            model.DataStructure = dataStructure;
            model.DataStructureType = DataStructureType.Structured;
            model.DatasetTitle = title;
            model.CompareValuesOfDataTypes = CompareValues(dataStructure as StructuredDataStructure);
            model.DownloadAccess = downloadAccess;
            model.DisplayFormats = getDisplayFormatObjects(dataStructure as StructuredDataStructure);
            model.AsciiFileDownloadSupport = supportedAsciiFileTypes;
            model.LatestVersion = latestVersion;
            model.HasEditRight = hasEditRights;

            return model;
        }

        public static ShowPrimaryDataModel Convert(long datasetId, int versionId, string title, DataStructure dataStructure, List<ContentDescriptor> dataFileList, bool downloadAccess, Dictionary<string, string> asciiFileDownloadSupport, bool latestVersion, bool hasEditRights)
        {
            ShowPrimaryDataModel model = new ShowPrimaryDataModel();
            model.FileList = ConvertContentDiscriptorsToFileInfos(dataFileList);
            model.DatasetId = datasetId;
            model.VersionId = versionId;
            model.DataStructure = dataStructure;
            model.DataStructureType = DataStructureType.Unstructured;
            model.DatasetTitle = title;
            model.CompareValuesOfDataTypes = CompareValues(dataStructure as StructuredDataStructure);
            model.DownloadAccess = downloadAccess;
            model.AsciiFileDownloadSupport = asciiFileDownloadSupport;
            model.LatestVersion = latestVersion;
            model.HasEditRight = hasEditRights;


            return model;
        }

        private static List<BasicFileInfo> ConvertContentDiscriptorsToFileInfos(List<ContentDescriptor> cds)
        {
            List<BasicFileInfo> list = new List<BasicFileInfo>();

            foreach (ContentDescriptor cd in cds)
            {
                string filePath = Path.Combine(AppConfiguration.DataPath, cd.URI);

                if (cd.Name.Equals("unstructuredData") && FileHelper.FileExist(filePath))
                {
                    FileInfo fileInfo = new FileInfo(filePath);

                    list.Add(
                        new BasicFileInfo(fileInfo.Name, cd.URI, cd.MimeType, fileInfo.Extension, fileInfo.Length)
                        );
                }
            }

            return list;
        }

        /// <summary>
        /// List of used datatypes and
        /// the maxvalue of the datatypes
        /// </summary>
        /// <returns></returns>
        private static List<string> CompareValues(StructuredDataStructure dataStructure)
        {
            List<string> cv = new List<string>();


            if (dataStructure != null)
            {
                foreach (var variable in dataStructure.Variables)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var missingValue in variable.MissingValues)
                    {

                        if (DataTypeUtility.GetTypeCode(variable.DataType.SystemType) == DataTypeCode.DateTime && DataTypeDisplayPattern.Materialize(variable.DataType.Extra) != null)
                        {
                            DataTypeDisplayPattern ddp = DataTypeDisplayPattern.Materialize(variable.DataType.Extra);
                            DateTime dateTime;
                            if (DateTime.TryParse(missingValue.Placeholder, new CultureInfo("en-US", false), DateTimeStyles.NoCurrentDateDefault, out dateTime))
                            {
                                sb.Append(missingValue.DisplayName + "|" + dateTime.ToString(ddp.StringPattern) + "#%#"); ;
                            }

                        }
                        else
                        {
                            sb.Append(missingValue.DisplayName + "|" + missingValue.Placeholder + "#%#");
                        }
                    }

                    //add also the case of the optional field
                    //replace the value with EMPTY String
                    sb.Append(" |" + getMaxvalueOfType(variable.DataType.SystemType));

                    cv.Add(sb.ToString());
                }
            }

            return cv;
        }

        private static string getMaxvalueOfType(string datatype)
        {
            switch (datatype)
            {
                case "System.Int16": return Int16.MaxValue.ToString();
                case "System.Int32": return Int32.MaxValue.ToString();
                case "System.Int64": return Int64.MaxValue.ToString();
                case "System.UInt16": return UInt16.MaxValue.ToString();
                case "System.UInt32": return UInt32.MaxValue.ToString();
                case "System.UInt64": return UInt64.MaxValue.ToString();
                case "System.Double": return double.MaxValue.ToString();
                case "System.Float": return float.MaxValue.ToString();
                case "System.Decimal": return decimal.MaxValue.ToString();
                case "System.DateTime": return DateTime.MaxValue.ToString();
                default: return "";
            }
        }

        private static List<DisplayFormatObject> getDisplayFormatObjects(StructuredDataStructure dataStructure)
        {
            List<DisplayFormatObject> tmp = new List<DisplayFormatObject>();

            foreach (var variable in dataStructure.Variables)
            {
                string format = "";
                string unit = "";
                string column = variable.Label;

                DataType dt = variable.DataType;

                // add display pattern to DisplayFormatObject;
                DataTypeDisplayPattern ddp = DataTypeDisplayPattern.Get(variable.DisplayPatternId);
                if (ddp != null)
                {
                    format = ddp.StringPattern;
                }

                // add unit abbr if exist do DisplayFormatObject
                // first variable, second dataattribute

                if (variable.Unit != null && !string.IsNullOrEmpty(variable.Unit.Abbreviation))
                {
                    unit = variable.Unit.Abbreviation;
                }
                else
                {
                    if (variable.VariableTemplate.Unit != null &&
                        !string.IsNullOrEmpty(variable.VariableTemplate.Unit.Abbreviation) &&
                        !variable.VariableTemplate.Unit.Name.Equals("None"))
                    {
                        unit = variable.Unit.Abbreviation;
                    }
                }

                if (!string.IsNullOrEmpty(column) && (!string.IsNullOrEmpty(format) || !string.IsNullOrEmpty(unit)))
                    tmp.Add(new DisplayFormatObject(column, format, unit));
            }

            return tmp;
        }


    }

    public enum DataStructureType
    {
        Structured,
        Unstructured
    }

    public class DisplayFormatObject
    {
        public string Format { get; set; }
        public string Unit { get; set; }
        public string Column { get; set; }

        public DisplayFormatObject(string column, string format, string unit)
        {
            Column = column;
            Format = format;
            Unit = unit;
        }
    }
}