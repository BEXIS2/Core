using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.Data;
using static BExIS.Modules.DQM.UI.Controllers.DQController;
using static BExIS.Modules.DQM.UI.Controllers.ManageDQController;

namespace BExIS.Modules.DQM.UI.Models
{
    public class DQModels
    {
        public datasetDescriptionLength datasetDescriptionLength = new datasetDescriptionLength(); //to store min/max/median of dataset descriptions' length
        public dataStrDescriptionLength dataStrDescriptionLength = new dataStrDescriptionLength(); //to store min/max/median of data structure descriptions' length
        public datasetTotalSize datasetTotalSize = new datasetTotalSize(); //to store min/max/median of datasets' total size 
        public dataStrUsage dataStrUsage = new dataStrUsage(); //to store min/max/median of data structures' usage (shared data structure)
        public performersActivity performersActivity = new performersActivity(); //to store min/max/median of performers' activity
        public datasetColNumber datasetColNumber = new datasetColNumber(); //to store min/max/median of tabular format datasets' column number
        public datasetRowNumber datasetRowNumber = new datasetRowNumber(); //to store min/max/median of tabular format datasets' row number
        public datasetFileNumber datasetFileNumber = new datasetFileNumber(); //to store min/max/median of file format datasets' file number
        public List<fileInformation> filesInformation = new List<fileInformation>(); //a list of file information in case of a file format dataset
        public metadataComplition metadataComplition = new metadataComplition(); //to store min/max/median of metadata completion
        public List<performer> performers = new List<performer>();  //to store performer information
        public List<varVariable> varVariables = new List<varVariable>(); //to store information about each variable in case of the tabular format dataset
        public List<datasetInformation> datasetsInformation = new List<datasetInformation>(); //to store information about each dataset
        public string type { get; set; } //dataset type: tabular/file
        public int isPublic { get; set; } //1:true; 0:false;
        public int publicDatasets { get; set; } //the number of all public datasets in the repository
        public int restrictedDataset { get; set; }  //the number of all restricted datasets in the repository
        public int readable { get; set; } //if user has the read permission for the current dataset
        public int allReadables { get; set; } //to count for how many datasets has the user read permission
        public int isValid { get; set; } //the metadata validation
        public int allValids { get; set; } //the number of datasets with valid metadata in the repository
        public int allDatasets { get; set; } //the number of all datasets in the repository
        public int fileDatasets { get; set; } //the number of all file format datasets in the repository
        public int tabularDatasets { get; set; } //the number of all tabular format datasets in the repository
        public int columnNumber { get; set; } //The number of rows in case of a tabular format dataset
        public int rowNumber { get; set; } //The number of columns in case of a tabular format dataset
        public int fileNumber { get; set; } //The number of files in case of a file format dataset
        public int userNumber { get; set; } //the number of all users in the repository
        public string dStrDescription { get; set; } //data structure description
    }

    /// <summary>
    /// A model for the ShowDatasetList view
    /// </summary>
    public class ExternalLink
    {
        public List<datasetInfo> datasetInfos = new List<datasetInfo>();
    }

    public class ManageDQ
    {
        //public List<List<long>> matrixId = new List<List<long>>();
        public List<dataset> datasets = new List<dataset>();
    }
}