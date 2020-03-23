﻿using BExIS.Dcm.UploadWizard;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Output;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Upload;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Vaiona.Entities.Common;
using Vaiona.Logging.Aspects;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class SubmitSummaryController : BaseController
    {
        private TaskManager TaskManager;
        private FileStream Stream;

        private static IDictionary<Guid, int> tasks = new Dictionary<Guid, int>();

        private UploadHelper uploadWizardHelper = new UploadHelper();
        //
        // GET: /DCM/Summary/

        [HttpGet]
        public ActionResult Summary(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.SetCurrent(index);
                // remove if existing
                TaskManager.RemoveExecutedStep(TaskManager.Current());
            }

            SummaryModel model = new SummaryModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID))
            {
                model.DatasetId = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]);
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_TITLE) && TaskManager.Bus[TaskManager.DATASET_TITLE]  != null)
            {
                model.DatasetTitle = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_ID))
            {
                model.DataStructureId = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_TITLE))
            {
                model.DataStructureTitle = TaskManager.Bus[TaskManager.DATASTRUCTURE_TITLE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.RESEARCHPLAN_ID))
            {
                model.ResearchPlanId = Convert.ToInt32(TaskManager.Bus[TaskManager.RESEARCHPLAN_ID].ToString());
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.RESEARCHPLAN_TITLE))
            {
                model.ResearchPlanTitle = TaskManager.Bus[TaskManager.RESEARCHPLAN_TITLE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.TITLE))
            {
                model.DatasetTitle = TaskManager.Bus[TaskManager.TITLE].ToString();
            }

            /*
            if (TaskManager.Bus.ContainsKey(TaskManager.AUTHOR))
            {
                model.Author = TaskManager.Bus[TaskManager.AUTHOR].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.OWNER))
            {
                model.Owner = TaskManager.Bus[TaskManager.OWNER].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.PROJECTNAME))
            {
                model.ProjectName = TaskManager.Bus[TaskManager.PROJECTNAME].ToString();
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.INSTITUTE))
            {
                model.ProjectInstitute = TaskManager.Bus[TaskManager.INSTITUTE].ToString();
            }*/

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Summary(object[] data)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            SummaryModel model = new SummaryModel();

            model.StepInfo = TaskManager.Current();
            model.ErrorList = FinishUpload(TaskManager);

            if (model.ErrorList.Count > 0)
            {
                #region set summary

                if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID))
                {
                    model.DatasetId = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]);
                }

                if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_TITLE) && TaskManager.Bus[TaskManager.DATASET_TITLE] != null)
                {
                    model.DatasetTitle = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();
                }

                if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_ID))
                {
                    model.DataStructureId = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);
                }

                if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_TITLE))
                {
                    model.DataStructureTitle = TaskManager.Bus[TaskManager.DATASTRUCTURE_TITLE].ToString();
                }

                if (TaskManager.Bus.ContainsKey(TaskManager.RESEARCHPLAN_ID))
                {
                    model.ResearchPlanId = Convert.ToInt32(TaskManager.Bus[TaskManager.RESEARCHPLAN_ID].ToString());
                }

                if (TaskManager.Bus.ContainsKey(TaskManager.RESEARCHPLAN_TITLE))
                {
                    model.ResearchPlanTitle = TaskManager.Bus[TaskManager.RESEARCHPLAN_TITLE].ToString();
                }

                #endregion set summary

                //ToDo: remove all changed from dataset and version
                return PartialView(model);
            }
            else
            {
                //ToDo: remove all changed from dataset and version
                return null;
            }
        }

        //temporary solution: norman :FinishUpload2
        public List<Error> FinishUpload(TaskManager taskManager)
        {
            DataStructureManager dsm = new DataStructureManager();
            DatasetManager dm = new DatasetManager();
            IOUtility iOUtility = new IOUtility();

            try
            {
                List<Error> temp = new List<Error>();
                DatasetVersion workingCopy = new DatasetVersion();
                //datatuple list
                List<DataTuple> rows = new List<DataTuple>();
                Dataset ds = null;
                bool inputWasAltered = false;

                if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID) && TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_ID))
                {
                    long id = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]);
                    long iddsd = Convert.ToInt32(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);

                    ds = dm.GetDataset(id);
                    // Javad: Please check if the dataset does exists!!

                    //GetValues from the previus version
                    // Status
                    DatasetVersion latestVersion = dm.GetDatasetLatestVersion(ds);
                    string status = DatasetStateInfo.NotValid.ToString();
                    if (latestVersion.StateInfo != null) status = latestVersion.StateInfo.State;

                    #region Progress Informations

                    if (TaskManager.Bus.ContainsKey(TaskManager.CURRENTPACKAGESIZE))
                    {
                        TaskManager.Bus[TaskManager.CURRENTPACKAGESIZE] = 0;
                    }
                    else
                    {
                        TaskManager.Bus.Add(TaskManager.CURRENTPACKAGESIZE, 0);
                    }

                    if (TaskManager.Bus.ContainsKey(TaskManager.CURRENTPACKAGE))
                    {
                        TaskManager.Bus[TaskManager.CURRENTPACKAGE] = 0;
                    }
                    else
                    {
                        TaskManager.Bus.Add(TaskManager.CURRENTPACKAGE, 0);
                    }

                    #endregion Progress Informations

                    #region structured data

                    if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_TYPE) && TaskManager.Bus[TaskManager.DATASTRUCTURE_TYPE].Equals(DataStructureType.Structured))
                    {
                        string title = "";
                        long datasetid = ds.Id;
                        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                        title = xmlDatasetHelper.GetInformation(ds.Id, NameAttributeValues.title);

                        int numberOfRows = 0;

                        try
                        {
                            //Stopwatch fullTime = Stopwatch.StartNew();

                            //Stopwatch loadDT = Stopwatch.StartNew();
                            List<long> datatupleFromDatabaseIds = dm.GetDatasetVersionEffectiveTupleIds(dm.GetDatasetLatestVersion(ds.Id));
                            //loadDT.Stop();
                            //Debug.WriteLine("Load DT From Db Time " + loadDT.Elapsed.TotalSeconds.ToString());

                            StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(iddsd);
                            dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);

                            #region excel reader

                            if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".xlsm") ||
                                iOUtility.IsSupportedExcelFile(TaskManager.Bus[TaskManager.EXTENTION].ToString()))
                            {
                                int packageSize = 100000;

                                TaskManager.Bus[TaskManager.CURRENTPACKAGESIZE] = packageSize;

                                int counter = 0;

                                //schleife
                                dm.CheckOutDatasetIfNot(ds.Id, GetUsernameOrDefault()); // there are cases, the dataset does not get checked out!!
                                if (!dm.IsDatasetCheckedOutFor(ds.Id, GetUsernameOrDefault()))
                                    throw new Exception(string.Format("Not able to checkout dataset '{0}' for  user '{1}'!", ds.Id, GetUsernameOrDefault()));

                                workingCopy = dm.GetDatasetWorkingCopy(ds.Id);

                                //set StateInfo of the previus version
                                if (workingCopy.StateInfo == null)
                                {
                                    workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo()
                                    {
                                        State = status
                                    };
                                }
                                else
                                {
                                    workingCopy.StateInfo.State = status;
                                }

                                ExcelReader reader = null;
                                ExcelFileReaderInfo excelFileReaderInfo = null;

                                if (iOUtility.IsSupportedExcelFile(TaskManager.Bus[TaskManager.EXTENTION].ToString()))
                                    excelFileReaderInfo = (ExcelFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO];

                                reader = new ExcelReader(sds, excelFileReaderInfo);

                                do
                                {
                                    counter++;
                                    TaskManager.Bus[TaskManager.CURRENTPACKAGE] = counter;

                                    //open stream
                                    Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());

                                    if (iOUtility.IsSupportedExcelFile(TaskManager.Bus[TaskManager.EXTENTION].ToString()))
                                    {
                                        rows = reader.ReadFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), (int)id, packageSize);
                                    }
                                    else
                                    {
                                        rows = reader.ReadTemplateFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), (int)id, packageSize);
                                    }

                                    //Debug.WriteLine("ReadFile: " + counter + "  Time " + upload.Elapsed.TotalSeconds.ToString());

                                    if (reader.ErrorMessages.Count > 0)
                                    {
                                        //model.ErrorList = reader.errorMessages;
                                    }
                                    else
                                    {
                                        //XXX Add packagesize to excel read function
                                        if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_STATUS))
                                        {
                                            if (TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("new") || ((UploadMethod)TaskManager.Bus[TaskManager.UPLOAD_METHOD]).Equals(UploadMethod.Append))
                                            {
                                                dm.EditDatasetVersion(workingCopy, rows, null, null);
                                                //Debug.WriteLine("EditDatasetVersion: " + counter + "  Time " + upload.Elapsed.TotalSeconds.ToString());
                                                //Debug.WriteLine("----");
                                            }
                                            else
                                            if (TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("edit"))
                                            {
                                                if (rows.Count() > 0)
                                                {
                                                    Dictionary<string, List<DataTuple>> splittedDatatuples = new Dictionary<string, List<DataTuple>>();
                                                    splittedDatatuples = uploadWizardHelper.GetSplitDatatuples(rows, (List<long>)TaskManager.Bus[TaskManager.PRIMARY_KEYS], workingCopy, ref datatupleFromDatabaseIds);

                                                    dm.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                                                }
                                            }
                                        }
                                        else
                                        {
                                        }
                                    }
                                    Stream?.Close();

                                    //count rows
                                    numberOfRows += rows.Count();
                                } while (rows.Count() > 0 && rows.Count() == packageSize);
                            }

                            #endregion excel reader

                            #region ascii reader

                            if (iOUtility.IsSupportedAsciiFile(TaskManager.Bus[TaskManager.EXTENTION].ToString()))
                            {
                                // open file
                                AsciiReader reader = new AsciiReader(sds, (AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO]);
                                //Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());

                                //DatasetManager dm = new DatasetManager();
                                //Dataset ds = dm.GetDataset(id);

                                Stopwatch totalTime = Stopwatch.StartNew();

                                if (dm.IsDatasetCheckedOutFor(ds.Id, GetUsernameOrDefault()) || dm.CheckOutDataset(ds.Id, GetUsernameOrDefault()))
                                {
                                    workingCopy = dm.GetDatasetWorkingCopy(ds.Id);
                                    int packageSize = 100000;
                                    TaskManager.Bus[TaskManager.CURRENTPACKAGESIZE] = packageSize;
                                    //schleife
                                    int counter = 0;

                                    //set StateInfo of the previus version
                                    if (workingCopy.StateInfo == null)
                                    {
                                        workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo()
                                        {
                                            State = status
                                        };
                                    }
                                    else
                                    {
                                        workingCopy.StateInfo.State = status;
                                    }

                                    do
                                    {
                                        counter++;
                                        inputWasAltered = false;
                                        TaskManager.Bus[TaskManager.CURRENTPACKAGE] = counter;

                                        Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                                        rows = reader.ReadFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), id, packageSize);
                                        Stream.Close();

                                        if (reader.ErrorMessages.Count > 0)
                                        {
                                            foreach (var err in reader.ErrorMessages)
                                            {
                                                temp.Add(new Error(ErrorType.Dataset, err.GetMessage()));
                                            }
                                            //return temp;
                                        }

                                        if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_STATUS))
                                        {
                                            if (TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("new") || ((UploadMethod)TaskManager.Bus[TaskManager.UPLOAD_METHOD]).Equals(UploadMethod.Append))
                                            {
                                                dm.EditDatasetVersion(workingCopy, rows, null, null);
                                            }
                                            else
                                            if (TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("edit"))
                                            {
                                                if (rows.Count() > 0)
                                                {
                                                    //Dictionary<string, List<DataTuple>> splittedDatatuples = new Dictionary<string, List<AbstractTuple>>();
                                                    var splittedDatatuples = uploadWizardHelper.GetSplitDatatuples(rows, (List<long>)TaskManager.Bus[TaskManager.PRIMARY_KEYS], workingCopy, ref datatupleFromDatabaseIds);
                                                    dm.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                                                    inputWasAltered = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (rows.Count() > 0)
                                            {
                                                Dictionary<string, List<DataTuple>> splittedDatatuples = new Dictionary<string, List<DataTuple>>();
                                                splittedDatatuples = uploadWizardHelper.GetSplitDatatuples(rows, (List<long>)TaskManager.Bus[TaskManager.PRIMARY_KEYS], workingCopy, ref datatupleFromDatabaseIds);
                                                dm.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                                                inputWasAltered = true;
                                            }
                                        }

                                        //count rows
                                        numberOfRows += rows.Count();
                                    } while ((rows.Count() > 0 && rows.Count() == packageSize) || inputWasAltered == true);

                                    totalTime.Stop();
                                    Debug.WriteLine(" Total Time " + totalTime.Elapsed.TotalSeconds.ToString());
                                }

                                //Stream.Close();
                            }

                            #endregion ascii reader

                            #region contentdescriptors

                            //remove all contentdescriptors from the old version
                            //generatedTXT
                            if (workingCopy.ContentDescriptors.Any(c => c.Name.Equals("generatedTXT")))
                            {
                                ContentDescriptor tmp =
                                    workingCopy.ContentDescriptors.Where(c => c.Name.Equals("generatedTXT"))
                                        .FirstOrDefault();
                                dm.DeleteContentDescriptor(tmp);
                            }

                            //generatedCSV
                            if (workingCopy.ContentDescriptors.Any(c => c.Name.Equals("generatedCSV")))
                            {
                                ContentDescriptor tmp =
                                    workingCopy.ContentDescriptors.Where(c => c.Name.Equals("generatedCSV"))
                                        .FirstOrDefault();
                                dm.DeleteContentDescriptor(tmp);
                            }
                            //generated
                            if (workingCopy.ContentDescriptors.Any(c => c.Name.Equals("generated")))
                            {
                                ContentDescriptor tmp =
                                    workingCopy.ContentDescriptors.Where(c => c.Name.Equals("generated"))
                                        .FirstOrDefault();
                                dm.DeleteContentDescriptor(tmp);
                            }

                            #endregion contentdescriptors

                            #region set System value into metadata

                            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_STATUS))
                            {
                                bool newdataset = TaskManager.Bus[TaskManager.DATASET_STATUS].ToString().Equals("new");
                                int v = 1;
                                if (workingCopy.Dataset.Versions != null && workingCopy.Dataset.Versions.Count > 1) v = workingCopy.Dataset.Versions.Count();

                                //set modification
                                workingCopy.ModificationInfo = new EntityAuditInfo()
                                {
                                    Performer = GetUsernameOrDefault(),
                                    Comment = "Data",
                                    ActionType = newdataset ? AuditActionType.Create : AuditActionType.Edit
                                };

                                setSystemValuesToMetadata(id, v, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata, newdataset);
                                dm.EditDatasetVersion(workingCopy, null, null, null);
                            }

                            #endregion set System value into metadata

                            // ToDo: Get Comment from ui and users
                            MoveAndSaveOriginalFileInContentDiscriptor(workingCopy);
                            dm.CheckInDataset(ds.Id, numberOfRows + " rows", GetUsernameOrDefault());

                            //send email
                            var es = new EmailService();
                            es.Send(MessageHelper.GetUpdateDatasetHeader(),
                                MessageHelper.GetUpdateDatasetMessage(datasetid, title, GetUsernameOrDefault()),
                                ConfigurationManager.AppSettings["SystemEmail"]
                                );
                        }
                        catch (Exception e)
                        {
                            temp.Add(new Error(ErrorType.Other, "Can not upload. : " + e.Message));
                            var es = new EmailService();
                            es.Send(MessageHelper.GetErrorHeader(),
                                "Can not upload. : " + e.Message,
                                ConfigurationManager.AppSettings["SystemEmail"]
                                );
                        }
                        finally
                        {
                        }
                    }

                    #endregion structured data

                    #region unstructured data

                    if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_TYPE) && TaskManager.Bus[TaskManager.DATASTRUCTURE_TYPE].Equals(DataStructureType.Unstructured))
                    {
                        // checkout the dataset, apply the changes, and check it in.
                        if (dm.IsDatasetCheckedOutFor(ds.Id, GetUsernameOrDefault()) || dm.CheckOutDataset(ds.Id, GetUsernameOrDefault()))
                        {
                            try
                            {
                                workingCopy = dm.GetDatasetWorkingCopy(ds.Id);

                                using (var unitOfWork = this.GetUnitOfWork())
                                {
                                    workingCopy = unitOfWork.GetReadOnlyRepository<DatasetVersion>().Get(workingCopy.Id);

                                    //set StateInfo of the previus version
                                    if (workingCopy.StateInfo == null)
                                    {
                                        workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo()
                                        {
                                            State = status
                                        };
                                    }
                                    else
                                    {
                                        workingCopy.StateInfo.State = status;
                                    }

                                    unitOfWork.GetReadOnlyRepository<DatasetVersion>().Load(workingCopy.ContentDescriptors);

                                    SaveFileInContentDiscriptor(workingCopy);
                                }

                                //set modification
                                workingCopy.ModificationInfo = new EntityAuditInfo()
                                {
                                    Performer = GetUsernameOrDefault(),
                                    Comment = "File",
                                    ActionType = AuditActionType.Create
                                };

                                dm.EditDatasetVersion(workingCopy, null, null, null);

                                //filename
                                string filename = "";
                                if (TaskManager.Bus.ContainsKey(TaskManager.FILENAME))
                                {
                                    filename = TaskManager.Bus[TaskManager.FILENAME]?.ToString();
                                }

                                // ToDo: Get Comment from ui and users
                                dm.CheckInDataset(ds.Id, filename, GetUsernameOrDefault(), ViewCreationBehavior.None);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                    }

                    #endregion unstructured data
                }
                else
                {
                    temp.Add(new Error(ErrorType.Dataset, "Dataset is not selected."));
                }

                if (temp.Count <= 0)
                {
                    dm.CheckInDataset(ds.Id, "no update on data tuples", GetUsernameOrDefault(), ViewCreationBehavior.None);
                }
                else
                {
                    dm.UndoCheckoutDataset(ds.Id, GetUsernameOrDefault());
                }
                return temp;
            }
            finally
            {
                dm.Dispose();
                dsm.Dispose();
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

        private string SaveFileInContentDiscriptor(DatasetVersion datasetVersion)
        {
            try
            {
                //XXX put function like GetStorePathOriginalFile or GetDynamicStorePathOriginalFile
                // the function is available in the abstract class datawriter
                ExcelWriter excelWriter = new ExcelWriter();
                // Move Original File to its permanent location
                String tempPath = TaskManager.Bus[TaskManager.FILEPATH].ToString();
                string originalFileName = TaskManager.Bus[TaskManager.FILENAME].ToString();
                string storePath = excelWriter.GetFullStorePathOriginalFile(datasetVersion.Dataset.Id, datasetVersion.VersionNo, originalFileName);
                string dynamicStorePath = excelWriter.GetDynamicStorePathOriginalFile(datasetVersion.Dataset.Id, datasetVersion.VersionNo, originalFileName);
                string extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();

                Debug.WriteLine("extention : " + extention);

                //Why using the excel writer, isn't any function available in System.IO.File/ Directory, etc. Javad
                FileHelper.MoveFile(tempPath, storePath);

                string mimeType = MimeMapping.GetMimeMapping(originalFileName);

                //Register the original data as a resource of the current dataset version
                ContentDescriptor originalDescriptor = new ContentDescriptor()
                {
                    OrderNo = 1,
                    Name = "unstructuredData",
                    MimeType = mimeType,
                    URI = dynamicStorePath,
                    DatasetVersion = datasetVersion,
                };

                // add current contentdesciptor to list
                datasetVersion.ContentDescriptors.Add(originalDescriptor);

                return storePath;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        [MeasurePerformance]
        private string MoveAndSaveOriginalFileInContentDiscriptor(DatasetVersion datasetVersion)
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            //dataset id and data structure id are available via datasetVersion properties,why you are passing them via the BUS? Javad
            long datasetId = Convert.ToInt64(TaskManager.Bus[TaskManager.DATASET_ID]);
            long dataStructureId = Convert.ToInt64(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);

            string title = "";

            if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_TITLE) && TaskManager.Bus[TaskManager.DATASET_TITLE] != null)
            {
                title = TaskManager.Bus[TaskManager.DATASET_TITLE].ToString();
            }
            string ext = ".xlsm";// TaskManager.Bus[TaskManager.EXTENTION].ToString();

            ExcelWriter excelWriter = new ExcelWriter();

            // Move Original File to its permanent location
            String tempPath = TaskManager.Bus[TaskManager.FILEPATH].ToString();
            string originalFileName = TaskManager.Bus[TaskManager.FILENAME].ToString();
            string storePath = excelWriter.GetFullStorePathOriginalFile(datasetId, datasetVersion.Id, originalFileName);
            string dynamicStorePath = excelWriter.GetDynamicStorePathOriginalFile(datasetId, datasetVersion.VersionNo, originalFileName);

            //Why using the excel writer, isn't any function available in System.IO.File/ Directory, etc. Javad
            FileHelper.MoveFile(tempPath, storePath);

            //Register the original data as a resource of the current dataset version
            ContentDescriptor originalDescriptor = new ContentDescriptor()
            {
                OrderNo = 1,
                Name = "original",
                MimeType = "application/xlsm",
                URI = dynamicStorePath,
                DatasetVersion = datasetVersion,
            };

            if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(originalDescriptor.Name)) > 0)
            {   // remove the one contentdesciptor
                foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                {
                    if (cd.Name == originalDescriptor.Name)
                    {
                        cd.URI = originalDescriptor.URI;
                    }
                }
            }
            else
            {
                // add current contentdesciptor to list
                datasetVersion.ContentDescriptors.Add(originalDescriptor);
            }

            return storePath;
        }

        private XmlDocument setSystemValuesToMetadata(long datasetid, long version, long metadataStructureId, XmlDocument metadata, bool newDataset)
        {
            SystemMetadataHelper SystemMetadataHelper = new SystemMetadataHelper();

            Key[] myObjArray = { };

            if (newDataset) myObjArray = new Key[] { Key.Id, Key.Version, Key.DateOfVersion, Key.DataCreationDate, Key.DataLastModified };
            else myObjArray = new Key[] { Key.Id, Key.Version, Key.DateOfVersion, Key.DataLastModified };

            metadata = SystemMetadataHelper.SetSystemValuesToMetadata(metadataStructureId, version, metadataStructureId, metadata, myObjArray);

            return metadata;
        }

        #endregion private methods
    }
}