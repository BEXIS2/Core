using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Data;
using Vaiona.Utils.Cfg;
using System.IO;
using BExIS.IO;
using BExIS.IO.DataType.DisplayPattern;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class ShowPrimaryDataModel
    {
        public DataTable Data { get; set; }

        public long DatasetId { get; set; }

        public String DatasetTitle { get; set; }

        public List<BasicFileInfo> FileList { get; set; }

        public DataStructure DataStructure { get; set; }

        public DataStructureType DataStructureType{ get; set; }

        public List<object> CompareValuesOfDataTypes { get; set; }

        public List<DisplayFormatObject> DisplayFormats { get; set; }

        public bool DownloadAccess { get; set; }

        public Dictionary<string,string> AsciiFileDownloadSupport { get; set; }


        public static ShowPrimaryDataModel Convert(long datasetId, string title, DataStructure dataStructure, DataTable data, bool downloadAccess, Dictionary<string,string> supportedAsciiFileTypes)
        {
            ShowPrimaryDataModel model = new ShowPrimaryDataModel();
            model.Data = data;
            model.DatasetId = datasetId;
            model.DataStructure = dataStructure;
            model.DataStructureType = DataStructureType.Structured;
            model.DatasetTitle = title;
            model.CompareValuesOfDataTypes = CompareValues();
            model.DownloadAccess = downloadAccess;
            model.DisplayFormats = getDisplayFormatObjects(dataStructure as StructuredDataStructure);
            model.AsciiFileDownloadSupport = supportedAsciiFileTypes;


            return model;
        }

        public static ShowPrimaryDataModel Convert(long datasetId, string title, DataStructure dataStructure, List<ContentDescriptor> dataFileList, bool downloadAccess, Dictionary<string, string> asciiFileDownloadSupport)
        {
            ShowPrimaryDataModel model = new ShowPrimaryDataModel();
            model.FileList = ConvertContentDiscriptorsToFileInfos(dataFileList);
            model.DatasetId = datasetId;
            model.DataStructure = dataStructure;
            model.DataStructureType = DataStructureType.Unstructured;
            model.DatasetTitle = title;
            model.CompareValuesOfDataTypes = CompareValues();
            model.DownloadAccess = downloadAccess;
            model.AsciiFileDownloadSupport = asciiFileDownloadSupport;

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
        private static List<object> CompareValues()
        {
            List<object> values = new List<object>();

            values.Add(Int16.MaxValue);
            values.Add(Int32.MaxValue);
            values.Add(Int64.MaxValue);
            values.Add(UInt16.MaxValue);
            values.Add(UInt32.MaxValue);
            values.Add(UInt64.MaxValue);
            values.Add(double.MaxValue);
            values.Add(float.MaxValue);
            values.Add(decimal.MaxValue);
            values.Add(DateTime.MaxValue);
            values.Add(-99999);

            return values;
        }

        private static List<DisplayFormatObject> getDisplayFormatObjects(StructuredDataStructure dataStructure)
        {
            List<DisplayFormatObject> tmp = new List<DisplayFormatObject>();

            foreach (var variable in  dataStructure.Variables)
            {
                string format = "";
                string unit = "";
                string column = variable.Label;

                DataType dt = variable.DataAttribute.DataType;

                // add display pattern to DisplayFormatObject;
                DataTypeDisplayPattern ddp = DataTypeDisplayPattern.Materialize(dt.Extra);
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
                    if (variable.DataAttribute.Unit != null &&
                        !string.IsNullOrEmpty(variable.DataAttribute.Unit.Abbreviation) &&
                        !variable.DataAttribute.Unit.Name.Equals("None"))
                    {
                        unit = variable.DataAttribute.Unit.Abbreviation;
                    }
                }
                
                if(!string.IsNullOrEmpty(column) && (!string.IsNullOrEmpty(format) || !string.IsNullOrEmpty(unit)) )
                    tmp.Add(new DisplayFormatObject(column,format,unit));

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
