﻿using BExIS.Dcm.UploadWizard;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Output;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Hooks.Logs;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Upload;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Vaiona.Entities.Common;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class DataASyncUploadHelper
    {
        public EditDatasetDetailsCache Cache { get; set; }
        public EditDatasetDetailsLog Log { get; set; }

        private string entity { get; set; }
        public bool RunningASync { get; set; }
        public User User { get; set; }

        private FileStream Stream;

        private UploadHelper uploadWizardHelper = new UploadHelper();

        public DataASyncUploadHelper(EditDatasetDetailsCache _cache, EditDatasetDetailsLog _logs, string _entity)
        {
            Cache = _cache;
            Log = _logs;
            entity = _entity;
        }

        //temporary solution: norman :FinishUpload2
        public async Task<List<Error>> FinishUpload(long id, AuditActionType datasetStatus, long structureId = -1)
        {
            if (id <= 0) throw new ArgumentException(nameof(id), "dataset id should be greater then 0");

            DataStructureManager dsm = new DataStructureManager();
            HookManager hookManager = new HookManager();
            DatasetManager dm = new DatasetManager();
            IOUtility iOUtility = new IOUtility();
            List<Error> temp = new List<Error>();

            string title = "";
            int numberOfRows = 0;
            int numberOfSkippedRows = 0;

            try
            {
                DatasetVersion workingCopy = new DatasetVersion();
                //datatuple list
                List<DataTuple> rows = new List<DataTuple>();
                //Dataset ds = null;
                bool inputWasAltered = false;

                // checkout the dataset, apply the changes, and check it in.
                if (dm.IsDatasetCheckedOutFor(id, User.Name) || dm.CheckOutDataset(id, User.Name))
                {
                    //GetValues from the previus version
                    // Status
                    workingCopy = dm.GetDatasetWorkingCopy(id);
                    title = workingCopy.Title;
                    string status = DatasetStateInfo.NotValid.ToString();
                    if (workingCopy.StateInfo != null) status = workingCopy.StateInfo.State;

                    #region Progress Informations

                    if (Cache.UpdateSetup != null)
                    {
                        Cache.UpdateSetup.CurrentPackage = 0;
                        Cache.UpdateSetup.CurrentPackageSize = 0;
                    }

                    #endregion Progress Informations

                    string folder = "Temp"; // folder name inside dataset - temp or attachments
                    var dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                    var getpath = Path.Combine(dataPath, "Datasets", id.ToString(), folder);
                    var storepath = Path.Combine(dataPath, "Datasets", id.ToString());

                    #region structured data

                    if (structureId > 0) // structure id exist, means structured data
                    {
                        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                        try
                        {
                            // load all data tuple ids from the latest version
                            List<long> datatupleFromDatabaseIds = dm.GetDatasetVersionEffectiveTupleIds(workingCopy);

                            // load structured data structure
                            StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(structureId);
                            dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);

                            #region excel reader

                            //if (Bus[TaskManager.EXTENTION].ToString().Equals(".xlsm") ||
                            //    iOUtility.IsSupportedExcelFile(Bus[TaskManager.EXTENTION].ToString()))
                            //{
                            //    int packageSize = 100000;

                            //    Bus[TaskManager.CURRENTPACKAGESIZE] = packageSize;

                            //    int counter = 0;

                            //    //schleife
                            //    dm.CheckOutDatasetIfNot(id, User.Name); // there are cases, the dataset does not get checked out!!
                            //    if (!dm.IsDatasetCheckedOutFor(id, User.Name))
                            //        throw new Exception(string.Format("Not able to checkout dataset '{0}' for  user '{1}'!", id, User.Name));

                            //    workingCopy = dm.GetDatasetWorkingCopy(id);

                            //    //set StateInfo of the previus version
                            //    if (workingCopy.StateInfo == null)
                            //    {
                            //        workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo()
                            //        {
                            //            State = status
                            //        };
                            //    }
                            //    else
                            //    {
                            //        workingCopy.StateInfo.State = status;
                            //    }

                            //    ExcelReader reader = null;
                            //    ExcelFileReaderInfo excelFileReaderInfo = null;

                            //    if (iOUtility.IsSupportedExcelFile(Bus[TaskManager.EXTENTION].ToString()))
                            //        excelFileReaderInfo = (ExcelFileReaderInfo)Bus[TaskManager.FILE_READER_INFO];

                            //    reader = new ExcelReader(sds, excelFileReaderInfo);

                            //    do
                            //    {
                            //        counter++;
                            //        Bus[TaskManager.CURRENTPACKAGE] = counter;

                            //        //open stream
                            //        using (Stream = reader.Open(Bus[TaskManager.FILEPATH].ToString()))
                            //        {
                            //            rows = new List<DataTuple>();

                            //            if (iOUtility.IsSupportedExcelFile(Bus[TaskManager.EXTENTION].ToString()))
                            //            {
                            //                if (reader.Position < excelFileReaderInfo.DataEndRow)
                            //                    rows = reader.ReadFile(Stream, Bus[TaskManager.FILENAME].ToString(), (int)id, packageSize);
                            //            }
                            //            else
                            //            {
                            //                rows = reader.ReadTemplateFile(Stream, Bus[TaskManager.FILENAME].ToString(), (int)id, packageSize);
                            //            }

                            //            //Debug.WriteLine("ReadFile: " + counter + "  Time " + upload.Elapsed.TotalSeconds.ToString());

                            //            if (reader.ErrorMessages.Count > 0)
                            //            {
                            //                //model.ErrorList = reader.errorMessages;
                            //            }
                            //            else
                            //            {
                            //                //XXX Add packagesize to excel read function
                            //                if (Bus.ContainsKey(TaskManager.DATASET_STATUS))
                            //                {
                            //                    if (Bus[TaskManager.DATASET_STATUS].ToString().Equals("new") || ((UploadMethod)Bus[TaskManager.UPLOAD_METHOD]).Equals(UploadMethod.Append))
                            //                    {
                            //                        dm.EditDatasetVersion(workingCopy, rows, null, null);
                            //                        //Debug.WriteLine("EditDatasetVersion: " + counter + "  Time " + upload.Elapsed.TotalSeconds.ToString());
                            //                        //Debug.WriteLine("----");
                            //                    }
                            //                    else
                            //                    if (Bus[TaskManager.DATASET_STATUS].ToString().Equals("edit"))
                            //                    {
                            //                        if (rows.Count() > 0)
                            //                        {
                            //                            Dictionary<string, List<DataTuple>> splittedDatatuples = new Dictionary<string, List<DataTuple>>();
                            //                            splittedDatatuples = uploadWizardHelper.GetSplitDatatuples(rows, (List<long>)Bus[TaskManager.PRIMARY_KEYS], workingCopy, ref datatupleFromDatabaseIds);

                            //                            dm.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                            //                        }
                            //                    }
                            //                }
                            //                else
                            //                {
                            //                }
                            //            }
                            //        }

                            //        //count rows
                            //        numberOfRows += rows.Count();
                            //    } while (rows.Count() > 0 && rows.Count() <= packageSize);

                            //    numberOfSkippedRows = reader.NumberOSkippedfRows;
                            //}

                            #endregion excel reader

                            foreach (var file in Cache.Files)
                            {
                                var filepath = Path.Combine(getpath, file.Name);

                                #region ascii reader

                                if (iOUtility.IsSupportedAsciiFile(Path.GetExtension(file.Name))) // extention not mimetype. may change sometimes later
                                {
                                    // open file
                                    AsciiReader reader = new AsciiReader(sds, Cache.AsciiFileReaderInfo);

                                    //set packagsize for one loop
                                    int packageSize = 100000;
                                    Cache.UpdateSetup.CurrentPackageSize = packageSize;

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
                                        Cache.UpdateSetup.CurrentPackage = counter;

                                        using (Stream = reader.Open(filepath.ToString()))
                                        {
                                            rows = reader.ReadFile(Stream, file.Name.ToString(), id, packageSize);
                                        }

                                        if (reader.ErrorMessages.Count > 0)
                                        {
                                            foreach (var err in reader.ErrorMessages)
                                            {
                                                temp.Add(new Error(ErrorType.Dataset, err.GetMessage()));
                                            }
                                            //return temp;
                                        }

                                        // based the dataset status and/ or the upload method
                                        // 3 different cases append only the tuples.
                                        // 1. dataset staus create
                                        // 2. update method -> append (obsolete)
                                        // 3. primary keys are not set
                                        if (datasetStatus == AuditActionType.Create || Cache.UpdateSetup.UpdateMethod.Equals(UploadMethod.Append) || Cache.UpdateSetup.PrimaryKeys == null)
                                        {
                                            dm.EditDatasetVersion(workingCopy, rows, null, null); // add all datatuples to the datasetversion
                                        }
                                        else
                                        if (datasetStatus == AuditActionType.Edit) // datatuples allready exist
                                        {
                                            if (rows.Count() > 0)
                                            {
                                                //split the incoming datatuples to (new|edit) based on the primary keys
                                                var splittedDatatuples = uploadWizardHelper.GetSplitDatatuples(rows, Cache.UpdateSetup.PrimaryKeys, workingCopy, ref datatupleFromDatabaseIds);
                                                dm.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                                                inputWasAltered = true;
                                            }
                                        }

                                        //count rows
                                        numberOfRows += rows.Count();
                                    } while ((rows.Count() > 0 && rows.Count() <= packageSize) || inputWasAltered == true);

                                    numberOfSkippedRows = reader.NumberOSkippedfRows;

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

                                MoveAndSaveOriginalFileInContentDiscriptor(workingCopy, title, id, structureId, Path.Combine(getpath, file.Name), file);
                            }

                            #region set System value into metadata

                            bool newdataset = datasetStatus == AuditActionType.Create ? true : false;
                            int v = 1;
                            if (workingCopy.Dataset.Versions != null && workingCopy.Dataset.Versions.Count > 1) v = workingCopy.Dataset.Versions.Count();

                            //set modification
                            workingCopy.ModificationInfo = new EntityAuditInfo()
                            {
                                Performer = User.Name,
                                Comment = "Data",
                                ActionType = newdataset ? AuditActionType.Create : AuditActionType.Edit
                            };

                            workingCopy.Metadata = setSystemValuesToMetadata(id, v, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata, newdataset);
                            dm.EditDatasetVersion(workingCopy, null, null, null);

                            #endregion set System value into metadata

                            // ToDo: Get Comment from ui and users

                            dm.CheckInDataset(id, numberOfRows + " rows", User.Name);

                            //send email
                            var es = new EmailService();
                            es.Send(MessageHelper.GetUpdateDatasetHeader(id),
                                MessageHelper.GetUpdateDatasetMessage(id, title, User.DisplayName, typeof(Dataset).Name),
                                GeneralSettings.SystemEmail
                                );
                        }
                        catch (Exception e)
                        {
                            temp.Add(new Error(ErrorType.Other, "Can not upload. : " + e.Message));
                            var es = new EmailService();
                            es.Send(MessageHelper.GetErrorHeader(),
                                "Dataset: " + title + "(ID: " + id + ", User: " + User.DisplayName + " )" + " Can not upload. : " + e.Message,
                                ConfigurationManager.AppSettings["SystemEmail"]
                                );
                        }
                        finally
                        {
                        }
                    }

                    #endregion structured data

                    #region unstructured data

                    if (structureId <= 0)
                    {
                        try
                        {
                            using (var unitOfWork = this.GetUnitOfWork())
                            {
                                workingCopy.VersionNo += 1;

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

                                foreach (var file in Cache.Files)
                                {
                                    SaveFileInContentDiscriptor(workingCopy, file, Path.Combine(getpath, file.Name));
                                }
                            }

                            bool newdataset = datasetStatus == AuditActionType.Create ? true : false;
                            int v = 1;
                            if (workingCopy.Dataset.Versions != null && workingCopy.Dataset.Versions.Count > 1) v = workingCopy.Dataset.Versions.Count();

                            //set modification
                            workingCopy.ModificationInfo = new EntityAuditInfo()
                            {
                                Performer = User.Name,
                                Comment = "File",
                                ActionType = AuditActionType.Create
                            };

                            workingCopy.Metadata = setSystemValuesToMetadata(id, v, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata, newdataset);

                            dm.EditDatasetVersion(workingCopy, null, null, null);

                            //filenames
                            string fileNames = string.Join(",", Cache.Files.Select(f => f.Name).ToArray());

                            // ToDo: Get Comment from ui and users
                            dm.CheckInDataset(id, fileNames, User.Name, ViewCreationBehavior.None);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    #endregion unstructured data

                    if (temp.Count <= 0)
                    {
                        dm.CheckInDataset(id, "no update on data tuples", User.Name, ViewCreationBehavior.None);
                    }
                    else
                    {
                        dm.UndoCheckoutDataset(id, User.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                temp.Add(new Error(ErrorType.Dataset, ex.Message));

                //When a exception is happen, may the dataset is checkedout
                //
                if (!dm.IsDatasetCheckedIn(id))
                {
                    // revert last changed and checkin without a new version
                    dm.UndoCheckoutDataset(id, User.Name, ViewCreationBehavior.None);
                }
            }
            finally
            {
                if (RunningASync)
                {
                    var es = new EmailService();

                    var user = User;

                    if (temp.Any())
                    {
                        es.Send(MessageHelper.GetPushApiUploadFailHeader(id, title),
                            MessageHelper.GetPushApiUploadFailMessage(id, user.Name, temp.Select(e => e.ToString()).ToArray()),
                            new List<string> { user.Email }, null, new List<string> { GeneralSettings.SystemEmail });
                    }
                    else
                    {
                        // reset cache

                        es.Send(MessageHelper.GetASyncFinishUploadHeader(id, title),
                            MessageHelper.GetASyncFinishUploadMessage(id, title, numberOfRows, numberOfSkippedRows),
                            new List<string> { user.Email }, null, new List<string> { GeneralSettings.SystemEmail });
                    }

                    if (Cache.Files.Count == 1)
                        es.Send(MessageHelper.GeFileUpdatHeader(id),
                            MessageHelper.GetFileUploaddMessage(id, user.Name, Cache.Files.FirstOrDefault().Name),
                            new List<string> { user.Email }, null, new List<string> { GeneralSettings.SystemEmail });

                    if (Cache.Files.Count > 1)
                        es.Send(MessageHelper.GeFileUpdatHeader(id),
                            MessageHelper.GetFilesUploaddMessage(id, user.Name, Cache.Files.Select(f => f.Name).ToArray()),
                            new List<string> { user.Email }, null, new List<string> { GeneralSettings.SystemEmail });
                }

                dm.Dispose();
                dsm.Dispose();

                if (temp.Count == 0)// reset cache if success, no errors in temp
                {
                    Cache.Files = new List<BExIS.UI.Hooks.Caches.FileInfo>();
                    //logs.Messages.Add(new LogMessage(DateTime.Now, messages, username, "Attachment upload","upload"));
                    Log.Messages.Add(new LogMessage(DateTime.Now, "data was successfully uploaded", User.Name, "Submit", "Upload"));
                }
                else
                {
                    List<string> err = new List<string>();
                    temp.ForEach(e => err.Add(e.ToHtmlString()));

                    Log.Messages.Add(new LogMessage(DateTime.Now, err, User.Name, "Submit", "Upload")); ;
                }

                hookManager.Save(Cache, Log, entity, "details", HookMode.edit, id);
            }

            return temp;
        }

        private string SaveFileInContentDiscriptor(DatasetVersion datasetVersion, BExIS.UI.Hooks.Caches.FileInfo file, string getpath)
        {
            try
            {
                //XXX put function like GetStorePathOriginalFile or GetDynamicStorePathOriginalFile
                // the function is available in the abstract class datawriter
                ExcelWriter excelWriter = new ExcelWriter();
                // Move Original File to its permanent location
                String tempPath = getpath;
                string originalFileName = file.Name.ToString();
                //string storePath = excelWriter.GetFullStorePathOriginalFile(datasetVersion.Dataset.Id, datasetVersion.VersionNo, originalFileName);
                string storePath = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetVersion.Dataset.Id.ToString(), originalFileName);
                string dynamicStorePath = Path.Combine("Datasets", datasetVersion.Dataset.Id.ToString(), originalFileName);
                string extention = file.Type.ToString();

                Debug.WriteLine("extention : " + extention);

                //check if directory exist
                // if folder not exist
                var directory = Path.GetDirectoryName(storePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                // check if file exist allready and if yes change the name
                int count = 1;
                string fileNameOnly = Path.GetFileNameWithoutExtension(storePath);
                string extension = Path.GetExtension(storePath);

                while (File.Exists(storePath))
                {
                    string tempFileName = string.Format("{0}({1})", fileNameOnly, count++);
                    storePath = Path.Combine(directory, tempFileName + extension);
                    dynamicStorePath = Path.Combine("Datasets", datasetVersion.Dataset.Id.ToString(), tempFileName + extension);
                    file.Name = tempFileName + extension;
                }

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
                    Description = file.Description
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

        //[MeasurePerformance]
        private string MoveAndSaveOriginalFileInContentDiscriptor(DatasetVersion datasetVersion, string title, long datasetId, long dataStructureId, string originalFilePath, BExIS.UI.Hooks.Caches.FileInfo file)
        {
            string ext = ".xlsm";// Bus[TaskManager.EXTENTION].ToString();

            ExcelWriter excelWriter = new ExcelWriter();

            // Move Original File to its permanent location
            String tempPath = originalFilePath.ToString();
            string originalFileName = file.Name.ToString();
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

            var metadata_new = SystemMetadataHelper.SetSystemValuesToMetadata(datasetid, version, metadataStructureId, metadata, myObjArray);

            return metadata_new;
        }
    }
}