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
    public class ShowDataModel
    {
        public DataTable Data { get; set; }

        public long DatasetId { get; set; }

        public String DatasetTitle { get; set; }

        public List<BasicFileInfo> FileList { get; set; }

        public DataStructure DataStructure { get; set; }

        public DataStructureType DataStructureType{ get; set; }

        public static ShowDataModel Convert(long datasetId, string title, DataStructure dataStructure, DataTable data)
        {
            ShowDataModel model = new ShowDataModel();
            model.Data = data;
            model.DatasetId = datasetId;
            model.DataStructure = dataStructure;
            model.DataStructureType = DataStructureType.Structured;
            model.DatasetTitle = title;

            return model;
        }

        public static ShowDataModel Convert(long datasetId, string title, DataStructure dataStructure, List<ContentDescriptor> dataFileList)
        {
            ShowDataModel model = new ShowDataModel();
            model.FileList = ConvertContentDiscriptorsToFileInfos(dataFileList);
            model.DatasetId = datasetId;
            model.DataStructure = dataStructure;
            model.DataStructureType = DataStructureType.Unstructured;
            model.DatasetTitle = title;

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
    }

    public enum DataStructureType
    { 
        Structured,
        Unstructured
    }


}
