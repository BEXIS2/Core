using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Data;
using Vaiona.Util.Cfg;
using System.IO;
using BExIS.Io;


namespace BExIS.Web.Shell.Areas.DDM.Models
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

        public bool DownloadAccess { get; set; }

        public static ShowPrimaryDataModel Convert(long datasetId, string title, DataStructure dataStructure, DataTable data, bool downloadAccess)
        {
            ShowPrimaryDataModel model = new ShowPrimaryDataModel();
            model.Data = data;
            model.DatasetId = datasetId;
            model.DataStructure = dataStructure;
            model.DataStructureType = DataStructureType.Structured;
            model.DatasetTitle = title;
            model.CompareValuesOfDataTypes = CompareValues();
            model.DownloadAccess = downloadAccess;

            return model;
        }

        public static ShowPrimaryDataModel Convert(long datasetId, string title, DataStructure dataStructure, List<ContentDescriptor> dataFileList, bool downloadAccess)
        {
            ShowPrimaryDataModel model = new ShowPrimaryDataModel();
            model.FileList = ConvertContentDiscriptorsToFileInfos(dataFileList);
            model.DatasetId = datasetId;
            model.DataStructure = dataStructure;
            model.DataStructureType = DataStructureType.Unstructured;
            model.DatasetTitle = title;
            model.CompareValuesOfDataTypes = CompareValues();
            model.DownloadAccess = downloadAccess;

            return model;
        }

        private static List<BasicFileInfo> ConvertContentDiscriptorsToFileInfos(List<ContentDescriptor> cds)
        {
            List<BasicFileInfo> list = new List<BasicFileInfo>();

            foreach (ContentDescriptor cd in cds)
            {
                
                list.Add(
                    new BasicFileInfo(cd.URI, cd.MimeType)
                    );
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

            return values;
        }

    }

    public enum DataStructureType
    { 
        Structured,
        Unstructured
    }


}
