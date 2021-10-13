using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.DQM.UI.Models;
using BExIS.Security.Services.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Party;
using BExIS.Security.Services.Subjects;
using System.Data;
using System.Xml;
using System.IO;
using Vaiona.Utils.Cfg;
using IDIV.Modules.Mmm.UI.Models;
using System.Net;
using System.Text;

namespace BExIS.Modules.DQM.UI.Controllers
{
    public class DQController : Controller
    {
        public class datasetInformation
        {
            public string type;
            public long datasetId;
            public string title;
            public int isPublic;
            public int readable; //has read permission or is public
            public int metadataValidation;
            public int metadataComplition;
            public int descriptionLength;
            public int structureDescriptionLength;
            public int structureUsage;
            public int datasetSizeTabular;
            public int columnNumber;
            public int rowNumber;
            public double datasetSizeFile;
            public int fileNumber;
            public List<string> performerNames = new List<string>();
        }
        public class datasetInfo
        {
            public long Id;
            public string title;
            public string description;
            public string type;
            public int isPublic;
        }

        public class datasetDescriptionLength
        {
            public int minDescriptionLength;
            public int currentDescriptionLength;
            public int maxDescriptionLength;
            public double medianDescriptionLength;
        }
        public class dataStrDescriptionLength
        {
            public int minDescriptionLength;
            public int currentDescriptionLength;
            public int maxDescriptionLength;
            public double medianDescriptionLength;
        }
        public class dataStrUsage
        {
            public int minDataStrUsage;
            public int currentDataStrUsage;
            public int maxDataStrUsage;
            public double medianDataStrUsage;
        }

        public class datasetTotalSize
        {
            public int minSizeTabular;
            public int maxSizeTabular;
            public double medianSizeTabular;
            public double minSizeFile;
            public double maxSizeFile;
            public double medianSizeFile;
            public double currentTotalSize;
        }

        public class datasetRowNumber
        {
            public int minRowNumber;
            public int currentRowNumber;
            public int maxRowNumber;
            public double medianRowNumber;
        }
        public class datasetColNumber
        {
            public int minColNumber;
            public int currentColNumber;
            public int maxColNumber;
            public double medianColNumber;
        }
        public class datasetFileNumber
        {
            public int minFileNumber;
            public int currentFileNumber;
            public int maxFileNumber;
            public double medianFileNumber;
        }
        public class performer
        {
            public string performerName;
            public List<long> DatasetIds;
            public int performerRate;
        }
        public class performersActivity
        {
            public int minActivity;
            public int maxActivity;
            public double medianActivity;
        }

        public class metadataComplition
        {
            public int totalFields;
            public int requiredFields;
            public int minRate;
            public int maxRate;
            public double medianRate;
        }

        public class fileInformation
        {
            public string fileName;
            public string fileDescription;
            public string fileFormat;
            public double fileSize;
        }
        public class varVariable
        {
            public string varLabel;
            public string varDescription;
            public string varType;
            public int varUsage;
            public double min;
            public double max;
            public int missing = 0;
            public bool uniqueValue = false;
            public int uniqueValueNumber;
            public string mostFrequent;
        }


        // GET: DQ
        public ActionResult ShowDQ(long datasetId, long versionId)
        {
            DQModels dqModel = new DQModels();
            Dictionary<string, string> datasetInfo = new Dictionary<string, string>();
            List<performer> performers = new List<performer>();
            List<varVariable> varVariables = new List<varVariable>();
            Dictionary<string, double> datasetSize = new Dictionary<string, double>();
            DatasetManager dm = new DatasetManager();
            DataStructureManager dsm = new DataStructureManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            PartyManager pm = new PartyManager();
            UserManager um = new UserManager();
            DatasetVersion dsv = new DatasetVersion();
            UserManager userManager = new UserManager();
            // data quality files
            try
            {                
                string pathPerformerDataset = @"C:\Data\DatasetQualities\PerformerDataset.csv";
                StreamReader readerPerformerDataset = new StreamReader(pathPerformerDataset);

            }
            catch (Exception ex)
            { }

            //////////////////////////////////////////////////////////////////////////
            DatasetVersion currentDatasetVersion = dm.GetDatasetVersion(versionId); //Current dataset version 
            DataStructure currentDataStr = dsm.AllTypesDataStructureRepo.Get(currentDatasetVersion.Dataset.DataStructure.Id); //current data structure
            var currentUser = userManager.FindByNameAsync(GetUsernameOrDefault()).Result; //Find current user

            //Find the dataset Type
            string currentDatasetType = "file";
            if (currentDataStr.Self.GetType() == typeof(StructuredDataStructure)) { currentDatasetType = "tabular"; }
            dqModel.type = currentDatasetType;

            #region performers

            #region dataset's performers
            try
            {
                string pathPerformerDataset = @"C:\Data\DatasetQualities\PerformerDataset.csv";
                StreamReader readerPerformerDataset = new StreamReader(pathPerformerDataset);
                string pathPerformers = @"C:\Data\DatasetQualities\Performers.csv";
                StreamReader readerPerformers = new StreamReader(pathPerformers);
                string performerLine;
                List<string> pfs = new List<string>();
                List<performer> ps = new List<performer>();

                while ((performerLine = readerPerformerDataset.ReadLine()) != null)
                {
                    string[] s = performerLine.Split(',');
                    if (long.Parse(s[1]) == datasetId)
                    {
                        pfs.Add(s[0]);
                    }                    
                }
                while ((performerLine = readerPerformers.ReadLine()) != null)
                {
                    string[] s = performerLine.Split(',');
                    if(pfs.Contains(s[0]))
                    {
                        performer p = new performer();
                        p.performerName = FindPerformerNameFromUsername(um, s[0]); //find performer name
                        p.performerRate = int.Parse(s[1]);
                        List<long> pfIds = FindDatasetsFromPerformerUsername(dm, um, s[0]); //Find all datasets in wich the username is involved.
                        p.DatasetIds = pfIds;
                        ps.Add(p);
                    }
                }
                dqModel.performers = ps;
                readerPerformerDataset.Close();
                readerPerformers.Close();
            }
            catch (Exception ex)
            {
                
            }
            #endregion

            #endregion //performers

            //dqModel.isPublic = entityPermissionManager.GetRights(null, 1, datasetId); //check if dataset is public
            //check the read permission for current dataset
            bool rPermission = entityPermissionManager.HasEffectiveRight(currentUser.UserName, typeof(Dataset), datasetId, Security.Entities.Authorization.RightType.Read); //find if user has read permission
            if (rPermission == true) //has read permission or public = readable
            { dqModel.readable = 1; }
            else { dqModel.readable = 0; } //cannot read

            //Check if the current metadata is valid
            if (currentDatasetVersion.StateInfo != null)
            {
                dqModel.isValid = DatasetStateInfo.Valid.ToString().Equals(currentDatasetVersion.StateInfo.State) ? 1 : 0; //1:valid; 0:invalid.
            }
            else { dqModel.isValid = 0; }


            List<long> datasetIds = dm.GetDatasetLatestIds();
            dqModel.allDatasets = datasetIds.Count;
            List<int> metadataRates = new List<int>();
            List<int> dsDescLength = new List<int>();
            List<int> dstrDescLength = new List<int>();
            List<int> dstrUsage = new List<int>();
            List<int> datasetSizeTabular = new List<int>();
            List<int> datasetRows = new List<int>();
            List<int> datasetCols = new List<int>();
            List<double> datasetSizeFiles = new List<double>();
            double datasetSizeFile = new double();
            List<int> datasetFileNumber = new List<int>();
            List<int> restrictions = new List<int>();
            int fileNumber = 0;
            List<int> sizeTabular = new List<int>(); //collect size, column number, and row number for one dataset
            int publicDatasets = 0;
            int restrictedDatasets = 0;
            int fileDatasets = 0;
            int tabularDatasets = 0;
            int rpTrue = 0;
            int rp;
            int validMetadata = 0;
            int allValidMetadas = 0;

            
            foreach (long Id in datasetIds) //for each dataset
            {
                if (dm.IsDatasetCheckedIn(Id))
                {
                            DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(Id);  //get last dataset versions

                    //If user has read permission
                    rPermission = entityPermissionManager.HasEffectiveRight(currentUser.UserName, typeof(Dataset), Id, Security.Entities.Authorization.RightType.Read);
                    if (rPermission == true) //has read permission or public = readable
                    {
                        rp = 1;
                        rpTrue += 1;
                    }
                    else { rp = 0; } //cannot read

                }
            }
            dqModel.allReadables = rpTrue;

            string pathDatasetInfo = @"C:\Data\DatasetQualities\datasetInfo.csv";
            StreamReader readerDatasetInfo = new StreamReader(pathDatasetInfo);
            List<datasetInformation> datasetsInformation = new List<datasetInformation>();
            try
            {

                string lineDatasetInfo;
                while ((lineDatasetInfo = readerDatasetInfo.ReadLine()) != null)
                {
                    string[] dsInf = lineDatasetInfo.Split(';');
                    datasetInformation datasetInformation = new datasetInformation();

                    long id = long.Parse(dsInf[0]);
                    datasetInformation.datasetId = id;
                    datasetInformation.title = dm.GetDatasetLatestVersion(id).Title;

                    rPermission = entityPermissionManager.HasEffectiveRight(currentUser.UserName, typeof(Dataset), id, Security.Entities.Authorization.RightType.Read);
                    if(rPermission == true) { datasetInformation.readable = 1;  }
                    if (rPermission == false) { datasetInformation.readable = 0; }
                    datasetInformation.type = dsInf[1];
                    datasetInformation.metadataValidation = int.Parse(dsInf[2]);                     
                    datasetInformation.metadataComplition = int.Parse(dsInf[3]);                   
                    datasetInformation.descriptionLength = int.Parse(dsInf[4]);                     
                    datasetInformation.structureDescriptionLength = int.Parse(dsInf[5]);                     
                    datasetInformation.structureUsage = int.Parse(dsInf[6]);                     
                    datasetInformation.columnNumber = int.Parse(dsInf[7]);
                    datasetInformation.rowNumber = int.Parse(dsInf[8]);
                    datasetInformation.fileNumber = int.Parse(dsInf[9]);
                    datasetInformation.datasetSizeFile = double.Parse(dsInf[10]);
                    string[] pfrms = dsInf[11].Split(',');
                    List<string> performerNames = new List<string>();
                    foreach(string p in pfrms)
                    {
                        performerNames.Add(p);
                    }
                    datasetInformation.performerNames = performerNames;

                    datasetsInformation.Add(datasetInformation);

                    if (datasetId == id)
                    {
                        dqModel.metadataComplition.requiredFields = int.Parse(dsInf[2]);
                        dqModel.metadataComplition.totalFields = int.Parse(dsInf[3]);
                        dqModel.datasetDescriptionLength.currentDescriptionLength = int.Parse(dsInf[4]);
                        dqModel.dataStrDescriptionLength.currentDescriptionLength = int.Parse(dsInf[5]);
                        dqModel.dataStrUsage.currentDataStrUsage = int.Parse(dsInf[6]);

                        dqModel.columnNumber = datasetInformation.columnNumber;
                        dqModel.rowNumber = datasetInformation.rowNumber;
                        dqModel.fileNumber = datasetInformation.fileNumber;
                        dqModel.datasetTotalSize.currentTotalSize = datasetInformation.datasetSizeFile;
                    }
                }
            }
            catch
            {

            }

            dqModel.datasetsInformation = datasetsInformation;
            readerDatasetInfo.Close();

            //CURRENT DATASET VERSION
            //dqModel.metadataComplition.totalFields = GetMetadataRate(currentDatasetVersion); //current dataset version: metadata rate
            //dqModel.metadataComplition.requiredFields = 100; //Need to calculate: metadataStructureId = dsv.Dataset.MetadataStructure.Id;
            //dqModel.datasetDescriptionLength.currentDescriptionLength = currentDatasetVersion.Description.Length; // Current dataset vesion: dataset description length
            //dqModel.dataStrDescriptionLength.currentDescriptionLength = currentDatasetVersion.Dataset.DataStructure.Description.Length; // Current dataset version: data structure description length
            //dqModel.dataStrUsage.currentDataStrUsage = currentDataStr.Datasets.Count() - 1; // Current dataset version: how many times the data structure is used in other datasets

            #region comparision
            try
            {
                string pathComparison = @"C:\Data\DatasetQualities\Comparison.csv";
                StreamReader readerComparison = new StreamReader(pathComparison);
                string infoline;
                List<int> infos = new List<int>();
                while ((infoline = readerComparison.ReadLine()) != null)
                {
                    string[] s = infoline.Split(',');
                    if(s[0] == "performersActivity")
                    {
                        dqModel.performersActivity.minActivity = int.Parse(s[1]);
                        dqModel.performersActivity.medianActivity = int.Parse(s[2]);
                        dqModel.performersActivity.maxActivity = int.Parse(s[3]);
                    }
                    else if (s[0] == "type")
                    {
                        dqModel.allDatasets = int.Parse(s[1]);
                        dqModel.tabularDatasets = int.Parse(s[2]);
                        dqModel.fileDatasets = int.Parse(s[3]);

                    }
                    else if (s[0] == "metadataRates")
                    {
                        dqModel.metadataComplition.minRate = int.Parse(s[1]);
                        dqModel.metadataComplition.medianRate = int.Parse(s[2]);
                        dqModel.metadataComplition.maxRate = int.Parse(s[3]);
                    }
                    else if (s[0] == "allValidMetadas")
                    {
                        dqModel.allValids = int.Parse(s[1]);
                    }
                    else if (s[0] == "datasetDescriptionLength")
                    {
                        dqModel.datasetDescriptionLength.minDescriptionLength = int.Parse(s[1]);
                        dqModel.datasetDescriptionLength.medianDescriptionLength = int.Parse(s[2]);
                        dqModel.datasetDescriptionLength.maxDescriptionLength = int.Parse(s[3]);
                    }
                    else if (s[0] == "dataStrDescriptionLength")
                    {
                        dqModel.dataStrDescriptionLength.minDescriptionLength = int.Parse(s[1]);
                        dqModel.dataStrDescriptionLength.medianDescriptionLength = int.Parse(s[2]);
                        dqModel.dataStrDescriptionLength.maxDescriptionLength = int.Parse(s[3]);
                    }
                    else if (s[0] == "dataStrUsage")
                    {
                        dqModel.dataStrUsage.minDataStrUsage = int.Parse(s[1]);
                        dqModel.dataStrUsage.medianDataStrUsage = int.Parse(s[2]);
                        dqModel.dataStrUsage.maxDataStrUsage = int.Parse(s[3]);
                    }
                    else if (s[0] == "datasetColNumber")
                    {
                        dqModel.datasetColNumber.minColNumber = int.Parse(s[1]);
                        dqModel.datasetColNumber.medianColNumber = int.Parse(s[2]);
                        dqModel.datasetColNumber.maxColNumber = int.Parse(s[3]);
                    }
                    else if (s[0] == "datasetRowNumber")
                    {
                        dqModel.datasetRowNumber.minRowNumber = int.Parse(s[1]);
                        dqModel.datasetRowNumber.medianRowNumber = int.Parse(s[2]);
                        dqModel.datasetRowNumber.maxRowNumber = int.Parse(s[3]);
                    }
                    else if (s[0] == "datasetFileNumber")
                    {
                        dqModel.datasetFileNumber.minFileNumber = int.Parse(s[1]);
                        dqModel.datasetFileNumber.medianFileNumber = int.Parse(s[2]);
                        dqModel.datasetFileNumber.maxFileNumber = int.Parse(s[3]);
                    }
                    else if (s[0] == "datasetTotalSizeFiles")
                    {
                        dqModel.datasetTotalSize.minSizeFile = double.Parse(s[1]);
                        dqModel.datasetTotalSize.medianSizeFile = double.Parse(s[2]);
                        dqModel.datasetTotalSize.maxSizeFile = double.Parse(s[3]);
                    }

                }
                readerComparison.Close();
            }
            catch(Exception ex)
            {

            }

            #endregion

            ///////////////////////////////////////////////////////////////////////

            #region TABULAR FORMAT DATASET      
            //If it is a tabular format dataset
            if (currentDatasetType == "tabular")
            {
                string pathVariables = @"C:\Data\DatasetQualities\Variables.csv";
                StreamReader readerVariables = new StreamReader(pathVariables);

                string varLine;
                while ((varLine = readerVariables.ReadLine()) != null)
                {
                    string[] varDetail = varLine.Split(',');
                    if (varDetail[0] == datasetId.ToString())
                    {
                        varVariable v = new varVariable();
                        v.varLabel = varDetail[1];
                        v.varType = varDetail[2];
                        v.varDescription = varDetail[3];
                        v.varUsage = int.Parse(varDetail[4]);
                        v.missing = int.Parse(varDetail[5]);

                        varVariables.Add(v);
                    }

                }
                dqModel.varVariables = varVariables;
                readerVariables.Close();
            }

                //    string serverName = "http://localhost:5412";

                //    //HttpWebRequest request = WebRequest.Create(link) as HttpWebRequest;                

                //    //HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                //    bool data = false;


                //    //using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                //    //{
                //    //    // Get the response stream  
                //    //    StreamReader reader = new StreamReader(response.GetResponseStream());
                //    //    if (reader.ReadLine().Count() > 0)
                //    //        data = true;

                //    //    response.Close();
                //    //}


                //int count = 0;
                //try
                //{
                //    count = dm.GetDatasetVersionEffectiveTupleIds(currentDatasetVersion).Count(); //count dataset rows
                //}
                //catch
                //{

                //}
                //StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(currentDatasetVersion.Dataset.DataStructure.Id); //get data structure
                //var variables = sds.Variables; //get variables
                //dqModel.columnNumber = variables.Count();

                //    //// Specify the URL to receive the request.
                //    ////string link = serverName + "/api/DataStatistic/" + datasetId + "/" + variable.Id;
                //    ////string link = serverName + "/api/data/" + datasetId; // Elli
                //    //string link = serverName + "/api/DataStatistic/" + datasetId; // Franzi
                //    //try
                //    //{
                //    //    HttpWebRequest request = WebRequest.Create(link) as HttpWebRequest;
                //    //    //request.Headers;
                //    //    request.UseDefaultCredentials = true;
                //    //    request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                //    //    //request.HaveResponse = true;
                //    //    //request.Headers.Add("Authorization", "Basic dchZ2VudDM6cGFdGVzC5zc3dvmQ=");
                //    //    request.Headers.Add("Authorization", "token fMZHJbS9j2PRrquXSuy4gvEB5YVrC6LwzbnwwCncFT5oZrG86LhSZdprZCzU6Znh");

                //    //    //request.Headers.Add("token", "fMZHJbS9j2PRrquXSuy4gvEB5YVrC6LwzbnwwCncFT5oZrG86LhSZdprZCzU6Znh");

                //    //    //request.GetResponse();
                //    //    using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                //    //    {
                //    //        //request.Credentials = CredentialCache.DefaultCredentials;
                //    //        //HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //    //        // Get the stream associated with the response.
                //    //        Stream receiveStream = response.GetResponseStream();

                //    //        //// Get the response stream  -- Elli
                //    //        //StreamReader readerStream = new StreamReader(response.GetResponseStream());
                //    //        //if (readerStream.ReadLine().Count() > 0)
                //    //        //    data = true;

                //    //        // Get the response stream
                //    //        StreamReader streamReader = new StreamReader(response.GetResponseStream());

                //    //        response.Close();
                //    //    }
                //    //}
                //    //catch (WebException e)
                //    //{
                //    //    string t = "t";
                //    //}

                //    int columnNumber = -1; //First four columns are added from system.
                //if (variables.Count() > 0)
                //{
                //        foreach (var variable in variables)
                //        {                       
                //            columnNumber += 1;
                //            //string missingValue = variable.MissingValue; //MISSING VALUE
                //            List<string> missingValues = new List<string>(); //creat a list contains missing values
                //            DataTable missTable = new DataTable();
                //            foreach (var missValue in variable.MissingValues) //if data is equal missing value
                //            {
                //                missingValues.Add(missValue.Placeholder);

                //            }
                //            //List<string> missingValues = variable.MissingValues;
                //            varVariable varV = new varVariable();
                //            varV.varLabel = variable.Label; // variable name
                //            varV.varDescription = variable.Description; //variable description
                //            varV.varUsage = variable.DataAttribute.UsagesAsVariable.Count() - 1; //How many other data structures are using the same variable template (except current one)
                //            varV.varType = variable.DataAttribute.DataType.SystemType; // What is the system type?
                //            varV.missing = 100; //suppose 100% is completed
                //try
                //{
                //    DataTable table = dm.GetLatestDatasetVersionTuples(datasetId, true); //data tuples
                //    DataColumnCollection columns = table.Columns;
                //    DataRowCollection rows = table.Rows;

                //                dqModel.datasetTotalSize.currentTotalSize = columns.Count * rows.Count;
                //dqModel.rowNumber = rows.Count;
                //dqModel.columnNumber = columns.Count - 4;
                //                double min = 0;
                //                double max = 0;
                //                int missing = rows.Count;
                //                bool b = true; //first value
                //                Dictionary<string, int> frequency = new Dictionary<string, int>();
                //                foreach (DataRow row in rows)
                //                {
                //                    var value = row.ItemArray[columnNumber];//.ToString();
                //                    if (value == null || missingValues.Contains(value.ToString())) //check if cell is emty or contains a missing value
                //                    {
                //                        missing -= 1;
                //                    }

                //                    //if value is numeric and it is in the first row
                //                    else if (varV.varType != "String" && varV.varType != "DateTime" && varV.varType != "Boolean") //&& varV.varType != "DateTime"
                //                    {
                //                        if (b == true)
                //                        {
                //                            min = Convert.ToDouble(value);
                //                            max = Convert.ToDouble(value);
                //                            b = false;
                //                        }
                //                        else
                //                        {
                //                            if (Convert.ToDouble(value) < min) { min = Convert.ToDouble(value); }
                //                            if (Convert.ToDouble(value) > max) { max = Convert.ToDouble(value); }
                //                        }
                //                    }

                //                    else //if data type is string or date or bool
                //                    {
                //                        if (frequency.ContainsKey(value.ToString()))
                //                        {
                //                            frequency[value.ToString()] += 1;
                //                        }
                //                        else
                //                        {
                //                            frequency[value.ToString()] = 1;
                //                        }
                //                    }
                //                }
                //                varV.min = min;
                //                varV.max = max;
                //                if (rows.Count > 0) { varV.missing = 100 * missing / rows.Count; } //% of existing values
                //                if (frequency.Count() > 0)
                //                {
                //                    var sortedDict = from entry in frequency orderby entry.Value ascending select entry;
                //                    if (sortedDict.First().Value == 1)
                //                    {
                //                        varV.uniqueValue = true;
                //                        varV.uniqueValueNumber = sortedDict.Count();
                //                    }
                //                    else
                //                    {
                //                        varV.uniqueValue = false;
                //                        varV.uniqueValueNumber = 0;
                //                        varV.mostFrequent = sortedDict.First().Key;
                //                    }
                //                }
                //                varVariables.Add(varV);
            //}
            //        catch
            //        {
            //            //dqModel.datasetTotalSize.currentTotalSize = -1;
            //        }
            //    }
            //}

            #endregion

            #region file format dataset
            // If it is a file format dataset
            else
            {
                List<fileInformation> filesInformations = new List<fileInformation>();
                string fileLine;
                fileInformation fileInformation = new fileInformation();
                string pathFiles = @"C:\Data\DatasetQualities\Files.csv";
                try
                {
                    StreamReader readerFiles = new StreamReader(pathFiles);
                    while ((fileLine = readerFiles.ReadLine()) != null)
                    {
                        string[] fileDetail = fileLine.Split(',');
                        if (fileDetail[0] == datasetId.ToString())
                        {
                            fileInformation f = new fileInformation();
                            f.fileName = fileDetail[1];
                            f.fileFormat = fileDetail[2];
                            double d = Convert.ToDouble(fileDetail[3]);
                            f.fileSize = d;
                            filesInformations.Add(f);
                        }
                    }
                    readerFiles.Close();
                }
                catch
                {

                }
                dqModel.filesInformation = filesInformations;                
            } 

                //if (currentDatasetVersion != null)
                //{
                //    List<ContentDescriptor> contentDescriptors = currentDatasetVersion.ContentDescriptors.ToList();
                //    double totalSize = 0;
                //    if (contentDescriptors.Count > 0)
                //    {
                //        foreach (ContentDescriptor cd in contentDescriptors)
                //        {
                //            if (cd.Name.ToLower().Equals("unstructureddata"))
                //            {
                //                fileInformation fileInformation = new fileInformation();
                //                string uri = cd.URI;

                //                //get the file path
                //                try
                //                {
                //                    String path = Server.UrlDecode(uri);
                //                    path = Path.Combine(AppConfiguration.DataPath, path);
                //                    Stream fileStream = System.IO.File.OpenRead(path);

                //                    if (fileStream != null)
                //                    {
                //                        FileStream fs = fileStream as FileStream;
                //                        if (fs != null)
                //                        {
                //                            //get file information
                //                            FileInformation fileInfo = new FileInformation(fs.Name.Split('\\').LastOrDefault(), MimeMapping.GetMimeMapping(fs.Name), (uint)fs.Length, uri);
                //                            fileInformation.fileName = fileInfo.Name.Split('.')[0]; //file name
                //                            fileInformation.fileFormat = fileInfo.Name.Split('.')[1].ToLower(); //file extension
                //                            fileInformation.fileSize = fileInfo.Size; //file size
                //                            totalSize += fileInfo.Size;
                //                        }
                //                    }
                //                }
                //                catch
                //                {

                //                }
                        //        filesInformation.Add(fileInformation);
                        //    }

                        //}
                    //}
                    //dqModel.fileNumber = contentDescriptors.Count;
                    //dqModel.datasetTotalSize.currentTotalSize = totalSize;
                //}
                //dqModel.filesInformation = filesInformation;
            //}
            #endregion

            
            return PartialView(dqModel);
        }

        /// <summary>
        /// Get username of the current user
        /// </summary>
        /// <returns>username/DEFAULT</returns>
        private string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }
            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        //private Stream getFileStream(string uri)
        //{
        //    String path = Server.UrlDecode(uri);
        //    path = Path.Combine(AppConfiguration.DataPath, path);
        //    return System.IO.File.OpenRead(path);
        //}

        /// <summary>
        /// Get total size of a file format dataset
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <returns>double size</returns>
        private double GetFileDatasetSize(DatasetVersion datasetVersion)
        {
            List<ContentDescriptor> contentDescriptors = datasetVersion.ContentDescriptors.ToList();
            double totalSize = 0;
            if (contentDescriptors.Count > 0)
            {
                foreach (ContentDescriptor cd in contentDescriptors)
                {
                    if (cd.Name.ToLower().Equals("unstructureddata"))
                    {
                        fileInformation fileInformation = new fileInformation();
                        string uri = cd.URI;
                        String path = Server.UrlDecode(uri);
                        path = Path.Combine(AppConfiguration.DataPath, path);
                        Stream fileStream = System.IO.File.OpenRead(path);

                        if (fileStream != null)
                        {
                            FileStream fs = fileStream as FileStream;
                            if (fs != null)
                            {
                                FileInformation fileInfo = new FileInformation(fs.Name.Split('\\').LastOrDefault(), MimeMapping.GetMimeMapping(fs.Name), (uint)fs.Length, uri);
                                fileInformation.fileSize = fileInfo.Size;
                                totalSize += fileInfo.Size;
                            }
                        }

                    }
                }
            }
            return (totalSize);
        }

        /// <summary>
        /// get the size of a tabular dataset contains rows and columns
        /// </summary>
        /// <param name="id">dataset id</param>
        /// <returns>list[0]:rows*cols, [1]:cols, [2]:rows</returns>
        private List<int> GetTabularSize(long id)
        {
            List<int> sizeTabular = new List<int>();
            DatasetManager dm = new DatasetManager();
            try
            {
                DataTable table = dm.GetLatestDatasetVersionTuples(id, true);
                DataRowCollection rows = table.Rows;
                DataColumnCollection columns = table.Columns;
                sizeTabular.Add(rows.Count * columns.Count);
                sizeTabular.Add(columns.Count);
                sizeTabular.Add(rows.Count);
            }
            catch
            {
                sizeTabular.Add(0);
                sizeTabular.Add(0);
                sizeTabular.Add(0);
            }
            return (sizeTabular);
        }

        /// <summary>
        /// This function calculates the median of a list
        /// </summary>
        /// <param name="intList">a list of integers or doubles</param>
        /// <returns>median number in double</returns>
        private double medianCalc(List<int> intList)
        {
            List<int> sortedList = intList.OrderBy(i => i).ToList();

            //get the median
            int size = sortedList.Count();
            int mid = size / 2;
            double median = (size % 2 != 0) ? (double)sortedList[mid] : ((double)sortedList[mid] + (double)sortedList[mid - 1]) / 2;

            return (median);
        }
        private double medianCalc(List<double> doubleList)
        {
            List<int> intList = new List<int>();
            foreach (double d in doubleList)
            {
                intList.Add((int)Math.Round(d));
            }
            List<int> sortedList = intList.OrderBy(i => i).ToList();

            //get the median
            int size = sortedList.Count();
            int mid = size / 2;
            double median = (size % 2 != 0) ? (double)sortedList[mid] : ((double)sortedList[mid] + (double)sortedList[mid - 1]) / 2;

            return (median);
        }

        /// <summary>
        /// This function calculates the percentage of a metadata completeness.
        /// </summary>
        /// <param name="dsv">dataset version</param>
        /// <returns>rate: percentage of the metadata completeness</returns>
        private int GetMetadataRate(DatasetVersion dsv)
        {
            XmlDocument metadata = dsv.Metadata;
            string xmlFrag = metadata.OuterXml;
            List<int> metaInfo = GetMetaInfoArray(xmlFrag);
            // [0] number of all metadata attributes, [1] number of filled metadata atributes
            // [2] number of all required metadata attributes, [3] number of filled required metadata attributes
            int rate = (metaInfo[1] * 100) / metaInfo[0]; //percentage of all metadata fields contains information            
            return (rate);
        }

        /// <summary>
        /// This function gives an xml format of a filled metadata structure, read all lines and select the lines related to fields,
        /// counts all fields and all fields that contain information.
        /// </summary>
        /// <param name="xmlFrag">A string of xml reader</param>
        /// <returns>A list of four elements</returns>
        /// <returns>element1: Number of all fields of the metadata</returns>
        /// <returns>element2: Number of all fields contains information</returns>
        /// <returns>element3: Number of all required fields of the metadata</returns>
        /// <returns>element4: Number of all required fields contains information</returns>
        private List<int> GetMetaInfoArray(string xmlFrag)
        {
            List<int> metaInfo = new List<int>();
            NameTable nt = new NameTable();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
            // Create the XmlParserContext.
            XmlParserContext context = new XmlParserContext(null, nsmgr, null, XmlSpace.None);
            // Create the reader.
            XmlTextReader reader = new XmlTextReader(xmlFrag, XmlNodeType.Element, context);

            int countMetaAttr = 0;
            int countMetaComplition = 0;

            // Parse the XML and display each node.
            while (reader.Read())
            {
                //XmlTextReader myReader = reader;
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.HasAttributes && reader.GetAttribute("type") == "MetadataAttribute")
                    {
                        countMetaAttr += 1;
                        reader.Read();
                        if (reader.NodeType == XmlNodeType.Text)
                        {
                            string text = reader.Value;
                            countMetaComplition += 1;
                        }
                    }
                }
            }

            // Close the reader.
            reader.Close();
            metaInfo.Add(countMetaAttr); // number of all metadata qttributes
            metaInfo.Add(countMetaComplition); // number of filled metadata atributes
            metaInfo.Add(100); // number of all required metadata attributes
            metaInfo.Add(100); // number of filled required metadata attributes
            return (metaInfo);
        }

        /// <summary>
        /// This funcion finds all performers of a dataset.
        /// Input can be included the dataset version id, or nothing as the latest version. 
        /// </summary>
        /// <param name="dsvs">A list of dataset versions of a dataset.</param>
        /// <param name="versionId">The current version Id of a dataset. </param>
        /// <returns>A list of performers</returns>
        private List<string> FindDatasetPerformers(DatasetManager dm, long datasetId, long versionId)
        {
            var dsvs = dm.GetDatasetVersions(datasetId);
            string performer;
            List<string> performerUsernames = new List<string>();
            foreach (var d in dsvs)
            {
                if (d.Id <= versionId)
                {
                    performer = d.ModificationInfo.Performer;
                    if (performer != null && !performerUsernames.Contains(performer))
                    {
                        performerUsernames.Add(performer);
                    }
                }
            }
            return (performerUsernames);
        }

        private List<string> FindDatasetPerformers(DatasetManager dm, long datasetId) //latest version
        {
            var dsvs = dm.GetDatasetVersions(datasetId);
            string performer;
            List<string> performerUsernames = new List<string>();
            foreach (var d in dsvs)
            {
                performer = d.ModificationInfo.Performer;
                if (performer != null && !performerUsernames.Contains(performer))
                {
                    performerUsernames.Add(performer);
                }
            }
            return (performerUsernames);
        }


        /// <summary>
        /// This funcion finds all datasets that a performer is involved.
        /// </summary>
        /// <param name="dm">A list of all datasets</param>
        /// <param name="userName">The performer's username</param>
        /// <returns>A list of dataset IDs</returns>
        private List<long> FindDatasetsFromPerformerUsername(DatasetManager dm, UserManager um, string username)
        {
            List<long> datasetIds = dm.GetDatasetLatestIds();
            List<long> Ids = new List<long>();
            foreach (long datasetId in datasetIds)
            {
                List<string> names = FindDatasetPerformers(dm, datasetId, dm.GetDatasetLatestVersionId(datasetId));
                if (names.Contains(username))
                {
                    Ids.Add(datasetId);
                }
            }
            return (Ids);
        }

        /// <summary>
        /// Find the full name of a performer from the username
        /// </summary>
        /// <param name="um"></param>
        /// <param name="userName"></param>
        /// <returns>The full name</returns>
        private string FindPerformerNameFromUsername(UserManager um, string userName)
        {
            string fullName = ""; // = dm.Select(v => v.CreationInfo.Performer).ToList();
            try
            {
                foreach (var user in um.Users)
                {
                    if (user.Name == userName)
                    {
                        fullName = user.FullName;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return (fullName);
        }



        /// <summary>
        /// THE ACTIONRESULT FOR SHOW DATASET LIST VIEW
        /// </summary>
        /// <param name="datasetIds"></param>
        /// <param name="performerName"></param>
        /// <returns></returns>
        public ActionResult ShowDatasetList()
        {
            ExternalLink dsModel = new ExternalLink();
            List<datasetInfo> datasetInfos = new List<datasetInfo>();
            DatasetManager dm = new DatasetManager();
            DataStructureManager dsm = new DataStructureManager();
            List<long> datasetIds = dm.GetDatasetLatestIds();

            foreach (long Id in datasetIds)
            {
                if (dm.IsDatasetCheckedIn(Id))
                {
                    DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(Id);  //get last dataset versions
                    datasetInfo datasetInfo = new datasetInfo();

                    datasetInfo.title = datasetVersion.Title;
                    DataStructure dataStr = dsm.AllTypesDataStructureRepo.Get(datasetVersion.Dataset.DataStructure.Id);
                    string type = "file";
                    if (dataStr.Self.GetType() == typeof(StructuredDataStructure)) { type = "tabular"; }
                    datasetInfo.type = type;
                    datasetInfo.Id = Id;
                    datasetInfos.Add(datasetInfo);

                }
            }

            dsModel.datasetInfos = datasetInfos;
            return View(dsModel);
        }
    }
}