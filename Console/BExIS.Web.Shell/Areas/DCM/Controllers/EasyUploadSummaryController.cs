using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Output;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Web.Shell.Areas.DCM.Models;
using System.Threading.Tasks;
using System.Threading;
using NHibernate.Util;
using Vaiona.Logging;
using Vaiona.Logging.Aspects;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Entities.Administration;
using System.Xml;
using BExIS.Xml.Helpers;
using System.Xml.Linq;
using BExIS.Xml.Services;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Entities.Objects;
using System.Web.Script.Serialization;
//using BExIS.Web.Shell.Areas.SAM.Models;
using BExIS.IO.Transform.Output;
using BExIS.RPM.Output;
using BExIS.IO.Transform.Validation;
using BExIS.IO.Transform.Validation.ValueCheck;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.Web.Shell.Helpers;
using BExIS.Web.Shell.Areas.DCM.Helpers;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class EasyUploadSummaryController : Controller
    {
        private EasyUploadTaskManager TaskManager;
        private FileStream Stream;

        private static IDictionary<Guid, int> tasks = new Dictionary<Guid, int>();

        [HttpGet]
        public ActionResult Summary(int index)
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
            }

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SCHEMA))
            {
                MetadataStructureManager msm = new MetadataStructureManager();
                long id = Convert.ToInt64(TaskManager.Bus[EasyUploadTaskManager.SCHEMA]);
                model.MetadataSchemaTitle = msm.Repo.Get(m => m.Id == id).FirstOrDefault().Name;
            }

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_FORMAT))
            {
                model.FileFormat = TaskManager.Bus[EasyUploadTaskManager.SHEET_FORMAT].ToString();
            }

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_HEADER_AREA))
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string selectedHeaderAreaJsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_HEADER_AREA].ToString();
                int[] areaHeaderValues = serializer.Deserialize<int[]>(selectedHeaderAreaJsonArray);

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
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string selectedDataAreaJsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA].ToString();
                int[] areaDataValues = serializer.Deserialize<int[]>(selectedDataAreaJsonArray);

                if (model.FileFormat.ToLower() == "leftright")
                {
                    model.NumberOfData = (areaDataValues[3]) - (areaDataValues[1]) + 1;
                }

                if (model.FileFormat.ToLower() == "topdown")
                {
                    model.NumberOfData = (areaDataValues[2]) - (areaDataValues[0]) + 1;
                }
            }

            return PartialView("EasyUploadSummary", model);
        }

        [HttpPost]
        public ActionResult Summary(object[] data)
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
                    MetadataStructureManager msm = new MetadataStructureManager();
                    long id = Convert.ToInt64(TaskManager.Bus[EasyUploadTaskManager.SCHEMA]);
                    model.MetadataSchemaTitle = msm.Repo.Get(m => m.Id == id).FirstOrDefault().Name;
                }

                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_FORMAT))
                {
                    model.FileFormat = TaskManager.Bus[EasyUploadTaskManager.SHEET_FORMAT].ToString();
                }

                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_HEADER_AREA))
                {
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    string selectedHeaderAreaJsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_HEADER_AREA].ToString();
                    int[] areaHeaderValues = serializer.Deserialize<int[]>(selectedHeaderAreaJsonArray);

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
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    string selectedDataAreaJsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA].ToString();
                    int[] areaDataValues = serializer.Deserialize<int[]>(selectedDataAreaJsonArray);

                    if (model.FileFormat.ToLower() == "leftright")
                    {
                        model.NumberOfData = (areaDataValues[3]) - (areaDataValues[1]) + 1;
                    }

                    if (model.FileFormat.ToLower() == "topdown")
                    {
                        model.NumberOfData = (areaDataValues[2]) - (areaDataValues[0]) + 1;
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

        //[MeasurePerformance]
        //temporary solution: norman
        //For original solution, look into Aquadiva Code
        public List<Error> FinishUpload(EasyUploadTaskManager taskManager)
        {

            List<Error> temp = new List<Error>();

            // initialize all necessary manager
            DataStructureManager dsm = new DataStructureManager();
            DatasetManager dm = new DatasetManager();
            MetadataStructureManager msm = new MetadataStructureManager();
            ResearchPlanManager rpm = new ResearchPlanManager();
            UnitManager um = new UnitManager();
            DataTypeManager dtm = new DataTypeManager();
            DataContainerManager dam = new DataContainerManager();

            DataTuple[] rows;

            //First, try to validate - if there are errors, return immediately
            string JsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_JSON_DATA].ToString();
            List<Error> ValidationErrors = ValidateRows(JsonArray);
            if (ValidationErrors.Count != 0)
            {
                temp.AddRange(ValidationErrors);
                return temp;
            }

            string timestamp = DateTime.Now.ToString("r");
            string title = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.FILENAME]);

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.DESCRIPTIONTITLE))
            {
                title = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.DESCRIPTIONTITLE]);
            }

            StructuredDataStructure sds = dsm.CreateStructuredDataStructure(title, title + " " + timestamp, "", "", DataStructureCategory.Generic);

            TaskManager.AddToBus(EasyUploadTaskManager.DATASTRUCTURE_ID, sds.Id);
            TaskManager.AddToBus(EasyUploadTaskManager.DATASTRUCTURE_TITLE, title + " " + timestamp);

            if (!TaskManager.Bus.ContainsKey(EasyUploadTaskManager.DATASET_TITLE))
            {
                TaskManager.AddToBus(EasyUploadTaskManager.DATASET_TITLE, title);
                TaskManager.AddToBus(EasyUploadTaskManager.TITLE, title);
            }

            // FIXIT
            MetadataStructure metadataStructure = null;
            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SCHEMA))
            {
                long metadataStructureId = Convert.ToInt64(TaskManager.Bus[EasyUploadTaskManager.SCHEMA]);
                metadataStructure = msm.Repo.Get(m => m.Id == metadataStructureId).FirstOrDefault();
            }
            else
            {
                //Default option but shouldn't happen because previous steps can't be finished without selecting the metadata-structure
                metadataStructure = msm.Repo.Get(m => m.Name.ToLower().Contains("eml")).FirstOrDefault();
            }
            ResearchPlan rp = rpm.Repo.Get().FirstOrDefault();
            TaskManager.AddToBus(EasyUploadTaskManager.RESEARCHPLAN_ID, rp.Id);
            TaskManager.AddToBus(EasyUploadTaskManager.RESEARCHPLAN_TITLE, rp.Title);

            //I don't see why this line is needed
            DatasetVersion workingCopy = new DatasetVersion();

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

            Dataset ds = null;
            XmlDocument xmldoc = new XmlDocument();
            XmlElement extraElement = xmldoc.CreateElement("extra");
            XmlElement orderElement = xmldoc.CreateElement("order");

            List<DataAttribute> allDataAttributes = dam.DataAttributeRepo.Get().ToList();

            List<Tuple<int, string, UnitInfo>> MappedHeaders = (List<Tuple<int, string, UnitInfo>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS];
            List<VariableIdentifier> identifiers = new List<VariableIdentifier>();
            foreach (Tuple<int, string, UnitInfo> Entry in MappedHeaders)
            {
                int i = MappedHeaders.IndexOf(Entry);

                DataType dataType = dtm.Repo.Get(Entry.Item3.SelectedDataTypeId);
                Unit CurrentSelectedUnit = um.Repo.Get(Entry.Item3.UnitId);

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
                VariableIdentifier vi = new VariableIdentifier();
                vi.name = newVariable.Label;
                vi.id = newVariable.Id;
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

            ds = dm.CreateEmptyDataset(sds, rp, metadataStructure);

            //Don't think these lines are necessary
            /*ExcelTemplateProvider etp = new ExcelTemplateProvider();
            etp.CreateTemplate(sds);*/

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

                dsv.Metadata = XmlDatasetHelper.SetInformation(dsv, metadataXml, NameAttributeValues.title, title);
                dm.EditDatasetVersion(dsv, null, null, null);
            }


            #region security
            // add security
            if (GetUsernameOrDefault() != "DEFAULT")
            {
                PermissionManager pm = new PermissionManager();
                SubjectManager sm = new SubjectManager();

                User user = sm.GetUserByName(GetUsernameOrDefault());

                //Rights-Management
                /*
                 * TODO: Use the BExIS Party API for that
                 * 
                 * */
                /*
                UserPiManager upm = new UserPiManager();
                List<long> piList = (new UserSelectListModel(GetUsernameOrDefault())).UserList.Select(x => x.Id).ToList();
                */


                foreach (RightType rightType in Enum.GetValues(typeof(RightType)).Cast<RightType>())
                {
                    //The user gets full permissions
                    pm.CreateDataPermission(user.Id, 1, ds.Id, rightType);

                    // adding the rights for the pis
                    /*foreach (long piId in piList)
                    {
                        //Each pi gets full permissions
                        pm.CreateDataPermission(piId, 1, ds.Id, rightType);

                        // get all pi members
                        List<UserPi> currentMembers = upm.GetAllPiMember(piId).ToList();

                        foreach (UserPi currentMapping in currentMembers)
                        {
                            switch (rightType)
                            {
                                //Each member of each of the pis gets view-rights
                                case RightType.View:
                                    pm.CreateDataPermission(currentMapping.UserId, 1, ds.Id, rightType);
                                    break;
                                //Each member of each of the pis gets download-rights
                                case RightType.Download:
                                    pm.CreateDataPermission(currentMapping.UserId, 1, ds.Id, rightType);
                                    break;
                                default:
                                    //No other permissions - is this call necessary?
                                    pm.CreateDataPermission(currentMapping.UserId, 0, ds.Id, rightType);
                                    break;
                            }
                        }
                    }*/
                }
            }
            #endregion security


            #region excel reader

            if (TaskManager.Bus[EasyUploadTaskManager.EXTENTION].ToString().Equals(".xls") ||
                TaskManager.Bus[EasyUploadTaskManager.EXTENTION].ToString().Equals(".xlsx"))
            {
                int packageSize = 10000;
                //HACK ?
                TaskManager.Bus[EasyUploadTaskManager.CURRENTPACKAGESIZE] = packageSize;

                int counter = 0;

                EasyUploadExcelReader reader = new EasyUploadExcelReader();

                dm.CheckOutDatasetIfNot(ds.Id, GetUsernameOrDefault()); // there are cases, the dataset does not get checked out!!
                if (!dm.IsDatasetCheckedOutFor(ds.Id, GetUsernameOrDefault()))
                {
                    throw new Exception(string.Format("Not able to checkout dataset '{0}' for  user '{1}'!", ds.Id, GetUsernameOrDefault()));
                }

                workingCopy = dm.GetDatasetWorkingCopy(ds.Id);

                counter++;
                TaskManager.Bus[EasyUploadTaskManager.CURRENTPACKAGE] = counter;

                // open file
                Stream = reader.Open(TaskManager.Bus[EasyUploadTaskManager.FILEPATH].ToString());
                //rows = reader.ReadFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), oldSds, (int)id, packageSize).ToArray();

                string selectedDataAreaJsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA].ToString();
                string selectedHeaderAreaJsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_HEADER_AREA].ToString();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                int[] areaDataValues = serializer.Deserialize<int[]>(selectedDataAreaJsonArray);
                int[] areaHeaderValues = serializer.Deserialize<int[]>(selectedHeaderAreaJsonArray);

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


                EasyUploadFileReaderInfo fri = new EasyUploadFileReaderInfo();
                fri.DataStartRow = areaDataValues[0] + 1;
                fri.DataStartColumn = areaDataValues[1] + 1;
                fri.DataEndRow = areaDataValues[2] + 1;
                fri.DataEndColumn = areaDataValues[3] + 1;

                fri.VariablesStartRow = areaHeaderValues[0] + 1;
                fri.VariablesStartColumn = areaHeaderValues[1] + 1;
                fri.VariablesEndRow = areaHeaderValues[2] + 1;
                fri.VariablesEndColumn = areaHeaderValues[3] + 1;

                fri.Offset = areaDataValues[1];
                fri.Orientation = orientation;
                
                reader.setSubmittedVariableIdentifiers(identifiers);

                rows = reader.ReadFile(Stream, TaskManager.Bus[EasyUploadTaskManager.FILENAME].ToString(), fri, sds, (int)datasetId);
                
                dm.EditDatasetVersion(workingCopy, rows.ToList(), null, null);
                
                Stream.Close();
            }

            #endregion


            dm.CheckInDataset(ds.Id, "upload data from upload wizard", GetUsernameOrDefault());

            TaskManager.AddToBus(EasyUploadTaskManager.DATASET_ID, ds.Id);

            return temp;
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
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
            var serializer = new JavaScriptSerializer();
            string[][] DeserializedJsonArray = serializer.Deserialize<string[][]>(JsonArray);

            List<Error> ErrorList = new List<Error>();
            List<Tuple<int, string, UnitInfo>> MappedHeaders = (List<Tuple<int, string, UnitInfo>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS];
            Tuple<int, string, UnitInfo>[] MappedHeadersArray = MappedHeaders.ToArray();
            DataTypeManager dtm = new DataTypeManager();


            string DataArea = TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA].ToString();
            int[] IntDataArea = serializer.Deserialize<int[]>(DataArea);

            string[,] SelectedDataArea = new string[(IntDataArea[2] - IntDataArea[0]), (IntDataArea[3] - IntDataArea[1])];

            for (int y = IntDataArea[0]; y <= IntDataArea[2]; y++)
            {
                for (int x = IntDataArea[1]; x <= IntDataArea[3]; x++)
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
                    //Workaround for the missing/incorrect implementation of the DataTypeCheck for Double and Character
                    //Should be removed as soon as this is fixed
                    Boolean skipDataTypeCheck = false;
                    if (datatypeName == "Double")
                    {
                        if (!Double.TryParse(vv, out DummyValue))
                        {
                            ErrorList.Add(new Error(ErrorType.Value, "Can not convert to:", new object[] { mappedHeader.Item2, vv, y, datatypeName }));
                            skipDataTypeCheck = true;
                        }
                    }

                    if (datatypeName == "Char")
                    {
                        skipDataTypeCheck = true;
                        char dummy;
                        if (!Char.TryParse(vv, out dummy))
                        {
                            ErrorList.Add(new Error(ErrorType.Value, "Can not convert to:", new object[] { mappedHeader.Item2, vv, y, datatypeName }));
                        }
                    }

                    if (!skipDataTypeCheck)
                    {
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
                        }
                    }
                }
            }

            return ErrorList;
        }


        #endregion
    }
}