using BExIS.Dcm.UploadWizard;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.IO.Transform.Validation.ValueCheck;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EasyUploadSummaryController : BaseController
    {
        private EasyUploadTaskManager TaskManager;
        private FileStream Stream;
        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        private static IDictionary<Guid, int> tasks = new Dictionary<Guid, int>();

        [HttpGet]
        public ActionResult Summary(int index)
        {
            MetadataStructureManager msm = new MetadataStructureManager();

            try
            {
                TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

                //set current stepinfo based on index
                if (TaskManager != null)
                {
                    TaskManager.SetCurrent(index);
                    // remove if existing
                    TaskManager.RemoveExecutedStep(TaskManager.Current());
                }

                EasyUploadSummaryModel model = new EasyUploadSummaryModel();
                model.StepInfo = TaskManager.Current();

                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.FILENAME))
                {
                    model.DatasetTitle = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.FILENAME]);

                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.DESCRIPTIONTITLE))
                    {
                        string tmp = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.DESCRIPTIONTITLE]);
                        if (!String.IsNullOrWhiteSpace(tmp))
                        {
                            model.DatasetTitle = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.DESCRIPTIONTITLE]);
                        }
                    }
                }

                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SCHEMA))
                {

                    long id = Convert.ToInt64(TaskManager.Bus[EasyUploadTaskManager.SCHEMA]);
                    model.MetadataSchemaTitle = msm.Repo.Get(m => m.Id == id).FirstOrDefault().Name;
                    msm.Dispose();
                }

                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_FORMAT))
                {
                    model.FileFormat = TaskManager.Bus[EasyUploadTaskManager.SHEET_FORMAT].ToString();
                }

                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_HEADER_AREA))
                {
                    string selectedHeaderAreaJsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_HEADER_AREA].ToString();
                    int[] areaHeaderValues = JsonConvert.DeserializeObject<int[]>(selectedHeaderAreaJsonArray);

                    if (model.FileFormat.ToLower() == "topdown")
                    {
                        model.NumberOfHeaders = (areaHeaderValues[3]) - (areaHeaderValues[1]) + 1;
                    }

                    if (model.FileFormat.ToLower() == "leftright")
                    {
                        model.NumberOfHeaders = (areaHeaderValues[2]) - (areaHeaderValues[0]) + 1;
                    }
                }

                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_DATA_AREA))
                {
                    List<string> selectedDataAreaJsonArray = (List<String>)TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA];
                    List<int[]> areaDataValuesList = new List<int[]>();
                    model.NumberOfData = 0;
                    foreach (string jsonArray in selectedDataAreaJsonArray)
                    {
                        areaDataValuesList.Add(JsonConvert.DeserializeObject<int[]>(jsonArray));
                    }
                    foreach (int[] areaDataValues in areaDataValuesList)
                    {
                        if (model.FileFormat.ToLower() == "leftright")
                        {
                            model.NumberOfData += (areaDataValues[3]) - (areaDataValues[1]) + 1;
                        }

                        if (model.FileFormat.ToLower() == "topdown")
                        {
                            model.NumberOfData += (areaDataValues[2]) - (areaDataValues[0]) + 1;
                        }
                    }

                }

                return PartialView("EasyUploadSummary", model);
            }
            finally
            {
                msm.Dispose();
            }
        }

        [HttpPost]
        public ActionResult Summary(object[] data)
        {
            MetadataStructureManager msm = new MetadataStructureManager();
            try
            {

                TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
                EasyUploadSummaryModel model = new EasyUploadSummaryModel();

                model.StepInfo = TaskManager.Current();
                model.ErrorList = FinishUpload(TaskManager);


                if (model.ErrorList.Count > 0)
                {
                    #region Populate model with data from the TaskManager
                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.FILENAME))
                    {
                        model.DatasetTitle = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.FILENAME]);
                    }

                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SCHEMA))
                    {

                        long id = Convert.ToInt64(TaskManager.Bus[EasyUploadTaskManager.SCHEMA]);
                        model.MetadataSchemaTitle = msm.Repo.Get(m => m.Id == id).FirstOrDefault().Name;
                    }

                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_FORMAT))
                    {
                        model.FileFormat = TaskManager.Bus[EasyUploadTaskManager.SHEET_FORMAT].ToString();
                    }

                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_HEADER_AREA))
                    {
                        string selectedHeaderAreaJsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_HEADER_AREA].ToString();
                        int[] areaHeaderValues = JsonConvert.DeserializeObject<int[]>(selectedHeaderAreaJsonArray);

                        if (model.FileFormat.ToLower() == "topdown")
                        {
                            model.NumberOfHeaders = (areaHeaderValues[3]) - (areaHeaderValues[1]) + 1;
                        }

                        if (model.FileFormat.ToLower() == "leftright")
                        {
                            model.NumberOfHeaders = (areaHeaderValues[2]) - (areaHeaderValues[0]) + 1;
                        }
                    }

                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_DATA_AREA))
                    {
                        List<string> selectedDataAreaJsonArray = (List<string>)TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA];
                        List<int[]> areaDataValuesList = new List<int[]>();
                        foreach (string area in selectedDataAreaJsonArray)
                        {
                            areaDataValuesList.Add(JsonConvert.DeserializeObject<int[]>(area));
                        }

                        foreach (int[] areaDataValues in areaDataValuesList)
                        {
                            if (model.FileFormat.ToLower() == "leftright")
                            {
                                model.NumberOfData = (areaDataValues[3]) - (areaDataValues[1]) + 1;
                            }

                            if (model.FileFormat.ToLower() == "topdown")
                            {
                                model.NumberOfData = (areaDataValues[2]) - (areaDataValues[0]) + 1;
                            }
                        }

                    }

                    #endregion
                    return PartialView("EasyUploadSummary", model);

                }
                else
                {
                    return null;
                }
            }
            finally
            {
                msm.Dispose();
            }
        }

        //[MeasurePerformance]
        //temporary solution: norman
        //For original solution, look into Aquadiva Code
        public List<Error> FinishUpload(EasyUploadTaskManager taskManager)
        {
            DataStructureManager dsm = new DataStructureManager();
            DatasetManager dm = new DatasetManager();
            DataContainerManager dam = new DataContainerManager();
            //SubjectManager sm = new SubjectManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

            List<Error> temp = new List<Error>();
            try
            {
                using (IUnitOfWork unitOfWork = this.GetUnitOfWork())
                {

                    // initialize all necessary manager

                    DataTuple[] rows = null;

                    //First, try to validate - if there are errors, return immediately
                    string JsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_JSON_DATA].ToString();
                    List<Error> ValidationErrors = ValidateRows(JsonArray);
                    if (ValidationErrors.Count != 0)
                    {
                        temp.AddRange(ValidationErrors);
                        return temp;
                    }

                    string timestamp = DateTime.UtcNow.ToString("r");
                    string title = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.FILENAME]);

                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.DESCRIPTIONTITLE))
                    {
                        string tmp = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.DESCRIPTIONTITLE]);
                        if (!String.IsNullOrWhiteSpace(tmp))
                        {
                            title = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.DESCRIPTIONTITLE]);
                        }
                    }

                    StructuredDataStructure sds = null;
                    List<VariableIdentifier> identifiers = new List<VariableIdentifier>(); //Used in Excel reader
                    //Try to find an exact matching datastructure
                    Boolean foundReusableDataStructure = false;
                    List<Tuple<int, string, UnitInfo>> MappedHeaders = (List<Tuple<int, string, UnitInfo>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS];
                    List<StructuredDataStructure> allDatastructures = dsm.StructuredDataStructureRepo.Get().ToList();
                    foreach (StructuredDataStructure existingStructure in allDatastructures)
                    {
                        if (!foundReusableDataStructure)
                        {
                            //For now a datastructure is considered an exact match if it contains variables with 
                            //the same names (labels), datatypes and units in the correct order
                            List<Variable> variablesOfExistingStructure = existingStructure.Variables.ToList();
                            foundReusableDataStructure = true;
                            if (variablesOfExistingStructure.Count != MappedHeaders.Count)
                                foundReusableDataStructure = false;
                            else
                            {
                                for (int i = 0; i < variablesOfExistingStructure.Count; i++)
                                {
                                    Variable exVar = variablesOfExistingStructure.ElementAt(i);
                                    Tuple<int, string, UnitInfo> currentHeader = MappedHeaders.ElementAt(i);
                                    if (exVar.Label != currentHeader.Item2 ||
                                        exVar.Unit.Id != currentHeader.Item3.UnitId ||
                                        exVar.DataAttribute.DataType.Id != currentHeader.Item3.SelectedDataTypeId)
                                    {
                                        foundReusableDataStructure = false;
                                    }
                                }
                            }
                            if (foundReusableDataStructure)
                            {
                                sds = existingStructure;
                                foreach (Variable exVar in variablesOfExistingStructure)
                                {
                                    VariableIdentifier vi = new VariableIdentifier
                                    {
                                        name = exVar.Label,
                                        id = exVar.Id
                                    };
                                    identifiers.Add(vi);
                                }
                            }
                        }
                    }

                    if (!foundReusableDataStructure)
                        sds = dsm.CreateStructuredDataStructure(title, title + " " + timestamp, "", "", DataStructureCategory.Generic);

                    TaskManager.AddToBus(EasyUploadTaskManager.DATASTRUCTURE_ID, sds.Id);
                    TaskManager.AddToBus(EasyUploadTaskManager.DATASTRUCTURE_TITLE, title + " " + timestamp);

                    if (!TaskManager.Bus.ContainsKey(EasyUploadTaskManager.DATASET_TITLE))
                    {
                        TaskManager.AddToBus(EasyUploadTaskManager.DATASET_TITLE, title);
                        TaskManager.AddToBus(EasyUploadTaskManager.TITLE, title);
                    }

                    MetadataStructure metadataStructure = null;
                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SCHEMA))
                    {
                        long metadataStructureId = Convert.ToInt64(TaskManager.Bus[EasyUploadTaskManager.SCHEMA]);
                        metadataStructure = unitOfWork.GetReadOnlyRepository<MetadataStructure>()
                            .Get(m => m.Id == metadataStructureId).FirstOrDefault();
                    }
                    else
                    {
                        //Default option but shouldn't happen because previous steps can't be finished without selecting the metadata-structure
                        metadataStructure = unitOfWork.GetReadOnlyRepository<MetadataStructure>()
                            .Get(m => m.Name.ToLower().Contains("eml")).FirstOrDefault();
                    }
                    ResearchPlan rp = unitOfWork.GetReadOnlyRepository<ResearchPlan>().Get().FirstOrDefault();
                    TaskManager.AddToBus(EasyUploadTaskManager.RESEARCHPLAN_ID, rp.Id);
                    TaskManager.AddToBus(EasyUploadTaskManager.RESEARCHPLAN_TITLE, rp.Title);

                    #region Progress Information

                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.CURRENTPACKAGESIZE))
                    {
                        TaskManager.Bus[EasyUploadTaskManager.CURRENTPACKAGESIZE] = 0;
                    }
                    else
                    {
                        TaskManager.Bus.Add(EasyUploadTaskManager.CURRENTPACKAGESIZE, 0);
                    }

                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.CURRENTPACKAGE))
                    {
                        TaskManager.Bus[EasyUploadTaskManager.CURRENTPACKAGE] = 0;
                    }
                    else
                    {
                        TaskManager.Bus.Add(EasyUploadTaskManager.CURRENTPACKAGE, 0);
                    }

                    #endregion

                    if (!foundReusableDataStructure)
                    {
                        #region Set Variables and information for new DataStructure

                        XmlDocument xmldoc = new XmlDocument();
                        XmlElement extraElement = xmldoc.CreateElement("extra");
                        XmlElement orderElement = xmldoc.CreateElement("order");

                        //Sorting necessary to prevent problems when inserting the tuples
                        MappedHeaders.Sort((head1, head2) => head1.Item1.CompareTo(head2.Item1));

                        var dataTypeRepo = unitOfWork.GetReadOnlyRepository<DataType>();
                        var unitRepo = unitOfWork.GetReadOnlyRepository<Unit>();
                        var dataAttributeRepo = unitOfWork.GetReadOnlyRepository<DataAttribute>();

                        List<DataAttribute> allDataAttributes = dataAttributeRepo.Get().ToList();

                        foreach (Tuple<int, string, UnitInfo> Entry in MappedHeaders)
                        {
                            int i = MappedHeaders.IndexOf(Entry);

                            DataType dataType = dataTypeRepo.Get(Entry.Item3.SelectedDataTypeId);
                            Unit CurrentSelectedUnit = unitRepo.Get(Entry.Item3.UnitId);

                            DataAttribute CurrentDataAttribute = new DataAttribute();
                            //If possible, map the chosen variable name, unit and datatype to an existing DataAttribute (Exact match)
                            DataAttribute existingDataAttribute = allDataAttributes.Where(da => da.Name.ToLower().Equals(TrimAndLimitString(Entry.Item2).ToLower()) &&
                                                                                                da.DataType.Id == dataType.Id &&
                                                                                                da.Unit.Id == CurrentSelectedUnit.Id).FirstOrDefault();
                            if (existingDataAttribute != null)
                            {
                                CurrentDataAttribute = existingDataAttribute;
                            }
                            else
                            {
                                //No matching DataAttribute => Create a new one
                                CurrentDataAttribute = dam.CreateDataAttribute(TrimAndLimitString(Entry.Item2), Entry.Item2, "", false, false, "", MeasurementScale.Categorial, DataContainerType.ReferenceType, "", dataType, CurrentSelectedUnit, null, null, null, null, null, null);
                            }

                            Variable newVariable = dsm.AddVariableUsage(sds, CurrentDataAttribute, true, Entry.Item2, "", "", "");
                            VariableIdentifier vi = new VariableIdentifier
                            {
                                name = newVariable.Label,
                                id = newVariable.Id
                            };
                            identifiers.Add(vi);

                            XmlElement newVariableXml = xmldoc.CreateElement("variable");
                            newVariableXml.InnerText = Convert.ToString(newVariable.Id);

                            orderElement.AppendChild(newVariableXml);
                        }
                        extraElement.AppendChild(orderElement);
                        xmldoc.AppendChild(extraElement);

                        sds.Extra = xmldoc;
                        sds.Name = "generated import structure " + timestamp;
                        sds.Description = "automatically generated structured data structure by user " + GetUsernameOrDefault() + " for file " + title + " on " + timestamp;

                        #endregion
                    }

                    Dataset ds = null;
                    ds = dm.CreateEmptyDataset(sds, rp, metadataStructure);

                    long datasetId = ds.Id;
                    long sdsId = sds.Id;
                    
                    if (dm.IsDatasetCheckedOutFor(datasetId, GetUsernameOrDefault()) || dm.CheckOutDataset(datasetId, GetUsernameOrDefault()))
                    {
                        DatasetVersion dsv = dm.GetDatasetWorkingCopy(datasetId);
                        long METADATASTRUCTURE_ID = metadataStructure.Id;
                        XmlMetadataWriter xmlMetadatWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                        XDocument metadataX = xmlMetadatWriter.CreateMetadataXml(METADATASTRUCTURE_ID);
                        XmlDocument metadataXml = XmlMetadataWriter.ToXmlDocument(metadataX);
                        dsv.Metadata = metadataXml;
                        try
                        {
                            dsv.Metadata = xmlDatasetHelper.SetInformation(dsv, metadataXml, NameAttributeValues.title, title);
                        }
                        catch (NullReferenceException ex)
                        {
                            //Reference of the title node is missing
                            throw new NullReferenceException("The extra-field of this metadata-structure is missing the title-node-reference!");
                        }
                        dm.EditDatasetVersion(dsv, null, null, null);
                    }


                    #region security
                    // add security
                    if (GetUsernameOrDefault() != "DEFAULT")
                    {
                        foreach (RightType rightType in Enum.GetValues(typeof(RightType)).Cast<RightType>())
                        {
                            //The user gets full permissions
                            // add security
                            if (GetUsernameOrDefault() != "DEFAULT")
                            {
                                entityPermissionManager.Create<User>(GetUsernameOrDefault(), "Dataset", typeof(Dataset), ds.Id, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
                            }
                        }
                    }
                    #endregion security


                    #region excel reader

                    int packageSize = 10000;
                    //HACK ?
                    TaskManager.Bus[EasyUploadTaskManager.CURRENTPACKAGESIZE] = packageSize;

                    int counter = 0;

                    dm.CheckOutDatasetIfNot(ds.Id, GetUsernameOrDefault()); // there are cases, the dataset does not get checked out!!
                    if (!dm.IsDatasetCheckedOutFor(ds.Id, GetUsernameOrDefault()))
                    {
                        throw new Exception(string.Format("Not able to checkout dataset '{0}' for  user '{1}'!", ds.Id, GetUsernameOrDefault()));
                    }

                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(ds.Id);

                    counter++;
                    TaskManager.Bus[EasyUploadTaskManager.CURRENTPACKAGE] = counter;

                    //rows = reader.ReadFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), oldSds, (int)id, packageSize).ToArray();

                    List<string> selectedDataAreaJsonArray = (List<string>)TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA];
                    string selectedHeaderAreaJsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_HEADER_AREA].ToString();
                    List<int[]> areaDataValuesList = new List<int[]>();
                    foreach (string area in selectedDataAreaJsonArray)
                    {
                        areaDataValuesList.Add(JsonConvert.DeserializeObject<int[]>(area));
                    }
                    int[] areaHeaderValues = JsonConvert.DeserializeObject<int[]>(selectedHeaderAreaJsonArray);

                    Orientation orientation = 0;

                    switch (TaskManager.Bus[EasyUploadTaskManager.SHEET_FORMAT].ToString())
                    {
                        case "LeftRight":
                            orientation = Orientation.rowwise;
                            break;
                        case "Matrix":
                            //orientation = Orientation.matrix;
                            break;
                        default:
                            orientation = Orientation.columnwise;
                            break;
                    }

                    String worksheetUri = null;
                    //Get the Uri to identify the correct worksheet
                    if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.ACTIVE_WOKSHEET_URI))
                    {
                        worksheetUri = TaskManager.Bus[EasyUploadTaskManager.ACTIVE_WOKSHEET_URI].ToString();
                    }

                    int batchSize = (new Object()).GetUnitOfWork().PersistenceManager.PreferredPushSize;
                    int batchnr = 1;
                    foreach (int[] areaDataValues in areaDataValuesList)
                    {
                        //First batch starts at the start of the current data area
                        int currentBatchStartRow = areaDataValues[0] + 1;
                        while (currentBatchStartRow <= areaDataValues[2] + 1) //While the end of the current data area has not yet been reached
                        {
                            //Create a new reader each time because the reader saves ALL tuples it read and therefore the batch processing wouldn't work
                            EasyUploadExcelReader reader = new EasyUploadExcelReader();
                            // open file
                            Stream = reader.Open(TaskManager.Bus[EasyUploadTaskManager.FILEPATH].ToString());

                            //End row is start row plus batch size
                            int currentBatchEndRow = currentBatchStartRow + batchSize;

                            //Set the indices for the reader
                            EasyUploadFileReaderInfo fri = new EasyUploadFileReaderInfo
                            {
                                DataStartRow = currentBatchStartRow,
                                //End row is either at the end of the batch or the end of the marked area
                                //DataEndRow = (currentBatchEndRow > areaDataValues[2] + 1) ? areaDataValues[2] + 1 : currentBatchEndRow,
                                DataEndRow = Math.Min(currentBatchEndRow, areaDataValues[2] + 1),
                                //Column indices as marked in a previous step
                                DataStartColumn = areaDataValues[1] + 1,
                                DataEndColumn = areaDataValues[3] + 1,

                                //Header area as marked in a previous step
                                VariablesStartRow = areaHeaderValues[0] + 1,
                                VariablesStartColumn = areaHeaderValues[1] + 1,
                                VariablesEndRow = areaHeaderValues[2] + 1,
                                VariablesEndColumn = areaHeaderValues[3] + 1,

                                Offset = areaDataValues[1],
                                Orientation = orientation
                            };

                            //Set variable identifiers because they might differ from the variable names in the file
                            reader.setSubmittedVariableIdentifiers(identifiers);

                            //Read the rows and convert them to DataTuples
                            rows = reader.ReadFile(Stream, TaskManager.Bus[EasyUploadTaskManager.FILENAME].ToString(), fri, sds, (int)datasetId, worksheetUri);

                            //After reading the rows, add them to the dataset
                            if (rows != null)
                                dm.EditDatasetVersion(workingCopy, rows.ToList(), null, null);

                            //Close the Stream so the next ExcelReader can open it again
                            Stream.Close();

                            //Debug information
                            int lines = (areaDataValues[2] + 1) - (areaDataValues[0] + 1);
                            int batches = lines / batchSize;
                            batchnr++;

                            //Next batch starts after the current one
                            currentBatchStartRow = currentBatchEndRow + 1;
                        }

                    }

                    #endregion


                    dm.CheckInDataset(ds.Id, "upload data from upload wizard", GetUsernameOrDefault());

                    //Reindex search
                    if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
                    {

                        this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", datasetId } });
                    }

                    TaskManager.AddToBus(EasyUploadTaskManager.DATASET_ID, ds.Id);

                    return temp;
                }
            }
            catch (Exception ex)
            {
                temp.Add(new Error(ErrorType.Other, "An error occured during the upload. " +
                    "Please try again later. If this problem keeps occuring, please contact your administrator."));
                return temp;
            }
            finally
            {
                dsm.Dispose();
                dm.Dispose();
                dam.Dispose();
                //sm.Dispose();
                entityPermissionManager.Dispose();
            }
        }

        #region private methods

        public string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        /*
         * Removes whitespaces at the beginning and end of the string
         * and limits the string to 255 characters
         * */
        private string TrimAndLimitString(string str, int limit = 255)
        {
            if (str != "" && str != null)
            {
                str = str.Trim();
                if (str.Length > limit)
                    str = str.Substring(0, limit);
            }
            return (str);
        }

        /// <summary>
        /// Determin whether the selected datatypes are suitable
        /// </summary>
        private List<Error> ValidateRows(string JsonArray)
        {
            DataTypeManager dtm = new DataTypeManager();


            try
            {
                const int maxErrorsPerColumn = 20;

                TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

                string[][] DeserializedJsonArray = JsonConvert.DeserializeObject<string[][]>(JsonArray);

                List<Error> ErrorList = new List<Error>();
                List<Tuple<int, string, UnitInfo>> MappedHeaders = (List<Tuple<int, string, UnitInfo>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS];
                Tuple<int, string, UnitInfo>[] MappedHeadersArray = MappedHeaders.ToArray();


                List<string> DataArea = (List<string>)TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA];
                List<int[]> IntDataAreaList = new List<int[]>();
                foreach (string area in DataArea)
                {
                    IntDataAreaList.Add(JsonConvert.DeserializeObject<int[]>(area));
                }

                foreach (int[] IntDataArea in IntDataAreaList)
                {
                    string[,] SelectedDataArea = new string[(IntDataArea[2] - IntDataArea[0]), (IntDataArea[3] - IntDataArea[1])];

                    for (int x = IntDataArea[1]; x <= IntDataArea[3]; x++)
                    {
                        int errorsInColumn = 0;
                        for (int y = IntDataArea[0]; y <= IntDataArea[2]; y++)
                        {
                            int SelectedY = y - (IntDataArea[0]);
                            int SelectedX = x - (IntDataArea[1]);
                            string vv = DeserializedJsonArray[y][x];

                            Tuple<int, string, UnitInfo> mappedHeader = MappedHeaders.Where(t => t.Item1 == SelectedX).FirstOrDefault();

                            DataType datatype = null;

                            if (mappedHeader.Item3.SelectedDataTypeId == -1)
                            {
                                datatype = dtm.Repo.Get(mappedHeader.Item3.DataTypeInfos.FirstOrDefault().DataTypeId);
                            }
                            else
                            {
                                datatype = dtm.Repo.Get(mappedHeader.Item3.SelectedDataTypeId);
                            }

                            string datatypeName = datatype.SystemType;


                            DataTypeCheck dtc;
                            double DummyValue = 0;
                            if (Double.TryParse(vv, out DummyValue))
                            {
                                if (vv.Contains("."))
                                {
                                    dtc = new DataTypeCheck(mappedHeader.Item2, datatypeName, DecimalCharacter.point);
                                }
                                else
                                {
                                    dtc = new DataTypeCheck(mappedHeader.Item2, datatypeName, DecimalCharacter.comma);
                                }
                            }
                            else
                            {
                                dtc = new DataTypeCheck(mappedHeader.Item2, datatypeName, DecimalCharacter.point);
                            }

                            var ValidationResult = dtc.Execute(vv, y);
                            if (ValidationResult is Error)
                            {
                                ErrorList.Add((Error)ValidationResult);
                                errorsInColumn++;
                            }

                            if (errorsInColumn >= maxErrorsPerColumn)
                            {
                                //Break inner (row) loop to jump to the next column
                                break;
                            }
                        }
                    }
                }

                return ErrorList;
            }
            finally
            {
                dtm.Dispose();
            }
        }


        #endregion
    }
}