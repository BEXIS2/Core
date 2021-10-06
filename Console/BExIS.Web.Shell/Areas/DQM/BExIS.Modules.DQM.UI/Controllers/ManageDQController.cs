using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.DQM.UI.Models;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using IDIV.Modules.Mmm.UI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.DQM.UI.Controllers
{
    public class ManageDQController : Controller
    {
        public class dataset
        {
            public long Id;
            public string title;
            public List<long> versionIds;
            public bool syncDQ;
        }

        // GET: DQManager
        public ActionResult Index()
        {
            DatasetManager dm = new DatasetManager(); //dataset manager
            ManageDQ manageModel = new ManageDQ();
            //DatasetVersion dsv = new DatasetVersion(); //dataset version manager
            List<long> datasetIds = dm.GetDatasetLatestIds(); //get latest 
            //List<List<long>> matrixId = new List<List<long>>();
            List<dataset> datasets = new List<dataset>();

            foreach (long Id in datasetIds) //for each dataset
            {
                dataset ds = new dataset();
                ds.Id = Id;
                ds.title = dm.GetDatasetLatestVersion(Id).Title;
                //List<DatasetVersion> datasetVersions = dm.GetDatasetVersions(Id);
                //List<long> versionIds = new List<long>();
                //for (int i = 0; i < datasetVersions.Count; ++i)
                //{
                //    long versionId = datasetVersions[i].Id;
                //    versionIds.Add(versionId);
                //}
                ////matrixId.Add(versions);
                //ds.versionIds = versionIds;
                datasets.Add(ds);
            }

            //manageModel.matrixId = matrixId;
            manageModel.datasets = datasets;
            return View(manageModel);
        }
        public ActionResult dqError()
        {
            return View();
        }
            public ActionResult dqSync()
        {

            using (var dm = new DatasetManager())
            {
                List<long> datasetIds = dm.GetDatasetLatestIds(); //get latest 
                EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
                DataStructureManager dsm = new DataStructureManager();

                try
                {
                    //datasetManager.SyncView(datasetIds, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);
                    // if the viewData has a model error, the redirect forgets about it.
                    string pathPerformers = @"C:\Data\DatasetQualities\Performers.csv"; 
                    StreamWriter writerPerformers = new StreamWriter(pathPerformers);
                    string pathPerformerDataset = @"C:\Data\DatasetQualities\PerformerDataset.csv";
                    StreamWriter writerPerformerDataset = new StreamWriter(pathPerformerDataset);
                    string pathComparison = @"C:\Data\DatasetQualities\Comparison.csv";
                    StreamWriter writerComparison = new StreamWriter(pathComparison);
                    string pathDatasets = @"C:\Data\DatasetQualities\datasetInfo.csv";
                    StreamWriter writerDatasets = new StreamWriter(pathDatasets);
                    string pathVariable = @"C:\Data\DatasetQualities\Variables.csv";
                    StreamWriter writerVariable = new StreamWriter(pathVariable);

                    string variableHeader = "datasetId,VarLabel,varType,varDescription,varUse,varMissing";
                    writerVariable.WriteLine(variableHeader);

                    string performer;
                    List<string> performerDataset = new List<string>();
                    Dictionary<string, int> performerCount = new Dictionary<string, int>();
                    List<int> metadataRates = new List<int>();
                    int allValidMetadas = 0;
                    //int publicDatasets = 0; //could not get result
                    //int restrictedDatasets = 0; //could not get result
                    List<int> dsDescLength = new List<int>();
                    List<int> dstrDescLength = new List<int>();
                    List<int> dstrUsage = new List<int>();
                    List<int> datasetSizeTabular = new List<int>();
                    List<int> datasetRows = new List<int>();
                    List<int> datasetCols = new List<int>();
                    List<double> datasetSizeFiles = new List<double>(); //all files in all datasets
                    List<int> datasetFileNumber = new List<int>();
                    int fileDatasets = 0;
                    int tabularDatasets = 0;
                    List<int> sizeTabular = new List<int>(); //collect size, column number, and row number for one dataset
                    int fileNumber = 0;
                    List<double> datasetTotalSize = new List<double>(); //total file size of each dataset
                    List<double> sizeFile = new List<double>();


                    foreach (long datasetId in datasetIds)
                    {
                        DatasetVersion datasetLatestVersion = dm.GetDatasetLatestVersion(datasetId);  //get last dataset versions
                        DataStructure dataStr = dsm.AllTypesDataStructureRepo.Get(datasetLatestVersion.Dataset.DataStructure.Id); //get data structure
                        

                        #region performers
                        List<string> pers = new List<string>();
                        var dsvs = dm.GetDatasetVersions(datasetId);
                        foreach (var d in dsvs)
                        {
                            performer = d.ModificationInfo.Performer;
                            if (performer != null && !pers.Contains(performer))
                            {
                                pers.Add(performer);  //a list of performers

                            }
                        }
                        foreach(var p in pers) 
                        {
                            writerPerformerDataset.WriteLine(p + "," + datasetId); //fill the file PerformerDataset with a list of 'performer,datasetId'
                            if (performerCount.ContainsKey(p))
                            {
                                performerCount[p] += 1;
                            }
                            else
                            {
                                performerCount.Add(p, 1);
                            }
                        }
                        #endregion

                        #region allValidmetadatas
                        long metadataStructureId = dm.DatasetRepo.Get(datasetId).MetadataStructure.Id;
                        int validMetadata = 0;
                        if (datasetLatestVersion.StateInfo != null)
                        {
                            validMetadata = DatasetStateInfo.Valid.ToString().Equals(datasetLatestVersion.StateInfo.State) ? 1 : 0; //1:valid; 0:invalid.
                        }
                        else
                        {
                            validMetadata = 0;
                        }
                        if (validMetadata == 1)  //count how many datasets have valid metadata
                        {
                            allValidMetadas += 1;
                        }

                        #endregion

                        #region metadataRates
                        XmlDocument metadata = datasetLatestVersion.Metadata;
                        string xmlFrag = metadata.OuterXml;
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
                        int rate = (countMetaComplition * 100) / countMetaAttr; //percentage of all metadata fields contains information            
                                                                                
                        metadataRates.Add(rate);
                        #endregion

                        ////find how many public dataset exist
                        //int publicRights = entityPermissionManager.GetRights(null, 1, datasetId); //1:public; 0:restricted                        
                        //if (publicRights == 1) { publicDatasets += 1; }
                        //if (publicRights == 0) { restrictedDatasets += 1; }

                        dsDescLength.Add(datasetLatestVersion.Description.Length); //get dataset description length
                        dstrDescLength.Add(datasetLatestVersion.Dataset.DataStructure.Description.Length); //get data structure description length
                        dstrUsage.Add(dataStr.Datasets.Count() - 1); //data structure is used in how many other datasets (doesn't contain the current one)

                        string type = "file";
                        if (dataStr.Self.GetType() == typeof(StructuredDataStructure)) { type = "tabular"; } //get dataset type

                        #region tabular dataset
                        if (type == "tabular")
                        {                            
                            tabularDatasets += 1;
                            try
                            {
                                DataTable table = dm.GetLatestDatasetVersionTuples(datasetId, true);
                                DataRowCollection rowss = table.Rows;
                                DataColumnCollection columns = table.Columns;
                                sizeTabular[1] = columns.Count - 4;
                                if (sizeTabular[1] < 0) //if data structure has not been designed.
                                {
                                    sizeTabular[1] = 0;
                                }
                                sizeTabular[2] = rowss.Count;
                                sizeTabular[0] = sizeTabular[1] * sizeTabular[2];

                                StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(datasetLatestVersion.Dataset.DataStructure.Id); //get data structure

                                #region variables
                                var variables = sds.Variables; //get variables
                                int columnNumber = -1; //First four columns are added from system.
                                if (variables.Count() > 0)
                                {
                                    foreach (var variable in variables)
                                    {
                                        columnNumber += 1;
                                        //string missingValue = variable.MissingValue; //MISSING VALUE
                                        List<string> missingValues = new List<string>(); //creat a list contains missing values
                                        DataTable missTable = new DataTable();
                                        foreach (var missValue in variable.MissingValues) //if data is equal missing value
                                        {
                                            missingValues.Add(missValue.Placeholder);

                                        }
                                        var varUse = variable.DataAttribute.UsagesAsVariable.Count() - 1;
                                        string varType = variable.DataAttribute.DataType.SystemType;                                        

                                        int varMissing = 100; //suppose 100% is completed
                                        try
                                        {                                            
                                            int missing = rowss.Count;
                                            foreach (DataRow row in rowss)
                                            {
                                                var value = row.ItemArray[columnNumber];//.ToString();
                                                if (value == null || missingValues.Contains(value.ToString())) //check if cell is emty or contains a missing value
                                                {
                                                    missing -= 1;
                                                }                                                
                                            }
                                            if (rowss.Count > 0) 
                                            { 
                                                varMissing = 100 * missing / rowss.Count; //% of existing values  
                                            }                                           
                                        }
                                        catch
                                        {
                                            
                                        }
                                        string variableLine = datasetId + ","               //0: dataset Id
                                            + variable.Label + ","                          //1: variable name
                                            + varType + ","                                 //2: data type
                                            + variable.Description.Count() + ","            //3: variable description length
                                            + varUse + ","                                  //4: variable usage                                            
                                            + varMissing;                                   //5: missing values
                                        writerVariable.WriteLine(variableLine);
                                    }
                                }
                                    #endregion
                            }
                            catch
                            {
                                sizeTabular.Add(0);
                                sizeTabular.Add(0);
                                sizeTabular.Add(0);
                            }



                            datasetSizeTabular.Add(sizeTabular[0]);
                            datasetCols.Add(sizeTabular[1]); //column number
                            datasetRows.Add(sizeTabular[2]); //row number                              
                        }

                        #endregion

                        #region file dataset
                        else if (type == "file")
                        {
                            fileDatasets += 1;
                            List<ContentDescriptor> contentDescriptors = datasetLatestVersion.ContentDescriptors.ToList();
                            fileNumber = contentDescriptors.Count;
                            //datasetFileNumber.Add(fileNumber);
                            //sizeFile.Add(fileNumber);
                            int fileNum = 0;
                            double totalSize = 0;

                            if (contentDescriptors.Count > 0)
                            {
                                foreach (ContentDescriptor cd in contentDescriptors)
                                {
                                    if (cd.Name.ToLower().Equals("unstructureddata"))
                                    {
                                        fileNum += 1;
                                        
                                            string uri = cd.URI;
                                            String path = Server.UrlDecode(uri);
                                            path = Path.Combine(AppConfiguration.DataPath, path);
                                        try
                                        {
                                            Stream fileStream = System.IO.File.OpenRead(path);
                                                FileStream fs = fileStream as FileStream;
                                                if (fs != null)
                                                {
                                                    FileInformation fileInfo = new FileInformation(fs.Name.Split('\\').LastOrDefault(), MimeMapping.GetMimeMapping(fs.Name), (uint)fs.Length, uri);
                                                    datasetSizeFiles.Add(fileInfo.Size); //file size
                                                    totalSize += fileInfo.Size;
                                                }
                                        }
                                        catch
                                        {
                                            datasetSizeFiles.Add(0); //file size                                            
                                        }
                                    }
                                }

                                datasetTotalSize.Add(totalSize);
                                sizeFile.Add(fileNum);
                                sizeFile.Add(totalSize);

                            }


                        }
                        #endregion

                        //[0]datasetId, [1]dataType, [2]IsValid, [3]metadataComplitionRate, 
                        //[4]datasetDescLength, [5]dataStrDescrLength, [6]DataStrUsage, 
                        //[7]columns, [8]rows, [9]file numbers, [10]file sizes, [11]performers
                        
                        string datasetInfo = datasetId + ";" + type + ";" + validMetadata + ";" + rate + ";"
                            + datasetLatestVersion.Description.Length + ";"
                            + datasetLatestVersion.Dataset.DataStructure.Description.Length + ";"
                            + (dataStr.Datasets.Count() - 1);
                        if (type == "tabular")
                        {
                            datasetInfo = datasetInfo + ";" + sizeTabular[1]    //column number
                                + ";" + sizeTabular[2]                          //row number
                                + ";0;0";                                         //file number and size
                        }
                        if (type == "file")
                        {
                            datasetInfo = datasetInfo + ";0;0"    //column and row number
                                + ";" + sizeFile[0]             //file number
                                + ";" + sizeFile[1];            //total size
                        }
                        string prfmrs = "";
                        foreach (string p in pers)
                        {
                            prfmrs = prfmrs + FindPerformerNameFromUsername(p) + ",";
                        }
                        prfmrs.Remove(prfmrs.Length-1,1);
                        datasetInfo = datasetInfo + ";" + prfmrs;
                        writerDatasets.WriteLine(datasetInfo);
                    }
                    writerDatasets.Close();
                    #region performersInFile                    
                    //write a list of 'performer,activity' in Performers.csv
                    foreach (string p in performerCount.Keys)
                    {
                        string l = p + "," + performerCount[p];
                        writerPerformers.WriteLine(l);
                    }
                   
                    // performer activities
                    int performerMin = performerCount.Values.Min();
                    int performerMax = performerCount.Values.Max();
                    List<int> performerActivities = new List<int>();
                    foreach (int s in performerCount.Values)
                    {
                        performerActivities.Add(s);
                    }
                    double performerMedian = medianCalc(performerActivities);
                    string performerCompare = "performersActivity," + performerMin + "," + performerMedian + "," + performerMax;
                    writerComparison.WriteLine(performerCompare); //performersActivity
                    writerPerformers.Close();
                    writerPerformerDataset.Close();
                    #endregion

                    #region datasetInfo in file

                    #endregion //datasetInfo in file

                    #region compare in file                  
                    string m = "metadataRates," + metadataRates.Min() +"," + medianCalc(metadataRates) + "," + metadataRates.Max();
                    writerComparison.WriteLine(m); 
                    string allValids = "allValidMetadas," + allValidMetadas;
                    writerComparison.WriteLine(allValids);
                    //string pd = "publicDatasets," + publicDatasets;
                    //string rd = "restrictedDatasets," + restrictedDatasets;
                    //writerComparison.WriteLine(pd);
                    //writerComparison.WriteLine(rd);
                    string datasetDescriptionLength = "datasetDescriptionLength," + dsDescLength.Min() + "," + medianCalc(dsDescLength) + "," + dsDescLength.Max();
                    string dataStrDescriptionLength = "dataStrDescriptionLength," + dstrDescLength.Min() + "," + medianCalc(dstrDescLength) + "," + dstrDescLength.Max();
                    string dataStrUsage = "dataStrUsage," + dstrUsage.Min() + "," + medianCalc(dstrUsage) + "," + dstrUsage.Max();
                    writerComparison.WriteLine(datasetDescriptionLength);
                    writerComparison.WriteLine(dataStrDescriptionLength);
                    writerComparison.WriteLine(dataStrUsage);

                    string typeDataset = "type," + (tabularDatasets + fileDatasets) + "," + tabularDatasets + "," + fileDatasets;
                    writerComparison.WriteLine(typeDataset);

                    string cols = "datasetColNumber," + datasetCols.Min() + "," + medianCalc(datasetCols) + "," + datasetCols.Max();
                    string rows = "datasetRowNumber," + datasetRows.Min() + "," + medianCalc(datasetRows) + "," + datasetRows.Max();
                    string fileNums = "";
                    string fileSizes = "";
                    string totalFileSize = "";
                    if (datasetFileNumber.Count > 0)
                    {
                        fileNums = "datasetFileNumber," + datasetFileNumber.Min() + "," + medianCalc(datasetFileNumber) + "," + datasetFileNumber.Max();
                        fileSizes = "datasetSizeFiles," + datasetSizeFiles.Min() + "," + medianCalc(datasetSizeFiles) + "," + datasetSizeFiles.Max();
                        totalFileSize = "datasetTotalSizeFiles," + datasetTotalSize.Min() + "," + medianCalc(datasetTotalSize) + "," + datasetTotalSize.Max();
                    }
                    else
                    {
                        fileNums = "datasetFileNumber," + 0 + "," + 0 + "," + 0;
                        fileSizes = "datasetSizeFiles," + 0 + "," + 0 + "," + 0;
                        totalFileSize = "datasetTotalSizeFiles," + 0 + "," + 0 + "," + 0;

                    }
                    writerComparison.WriteLine(cols);
                    writerComparison.WriteLine(rows);
                    writerComparison.WriteLine(fileNums);
                    writerComparison.WriteLine(fileSizes);
                    writerComparison.WriteLine(totalFileSize);
                    #endregion


                    writerComparison.Close();
                    //writerDatasets.Close();
                    writerVariable.Close();
                    return View(); 
                    //return RedirectToAction("Index", new { area = "dqm" });

                }
                catch (Exception ex)
                {
                    ViewData.ModelState.AddModelError("", $@"'{ex.Message}'");
                    return RedirectToAction("dqError", new { area = "dqm" });
                }          
            }
        }
        private int medianCalc(List<int> intList)
        {
            List<int> sortedList = intList.OrderBy(i => i).ToList();

            //get the median
            int size = sortedList.Count();
            int mid = size / 2;
            int median = (size % 2 != 0) ? (int)sortedList[mid] : ((int)sortedList[mid] + (int)sortedList[mid - 1]) / 2;

            return (median);
        }
        private double medianCalc(List<double> doubleList)
        {
            List<double> sortedList = doubleList.OrderBy(i => i).ToList();

            //get the median
            int size = sortedList.Count();
            int mid = size / 2;
            double median = (size % 2 != 0) ? (double)sortedList[mid] : ((double)sortedList[mid] + (double)sortedList[mid - 1]) / 2;

            return (median);
        }

        /// <summary>
        /// Find the full name of a performer from the username
        /// </summary>
        /// <param name="um"></param>
        /// <param name="userName"></param>
        /// <returns>The full name</returns>
        private string FindPerformerNameFromUsername(string userName)
        {
            UserManager um = new UserManager();
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
    }
}