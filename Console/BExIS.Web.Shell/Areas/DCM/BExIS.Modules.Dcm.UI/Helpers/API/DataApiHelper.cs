using BExIS.Dim.Entities.Mapping;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Output;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models.API;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Upload;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Vaiona.Entities.Common;
using Vaiona.Utils.Cfg;
using BExIS.Modules.Dcm.UI.Helpers;

namespace BExIS.Modules.Dcm.UI.Helper.API
{
    public class DataApiHelper
    {
        private DatasetManager datasetManager = new DatasetManager();
        private UserManager userManager = new UserManager();
        private EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
        private DataStructureManager dataStructureManager = new DataStructureManager();
        private FileStream Stream = null;
        private AsciiReader reader = null;
        private AsciiWriter asciiWriter = null;
        private UploadHelper uploadHelper = new UploadHelper();

        private int packageSize = 10000;
        private string _filepath = "";
        private Dataset _dataset;
        private StructuredDataStructure _dataStructure;
        private User _user;
        private DataApiModel _data = null;
        private string _title = "";
        private List<long> variableIds = new List<long>();
        //private UploadMethod _uploadMethod;

        public DataApiHelper(Dataset dataset, User user, DataApiModel data, string title, UploadMethod uploadMethod)
        {
            datasetManager = new DatasetManager();
            userManager = new UserManager();
            entityPermissionManager = new EntityPermissionManager();
            dataStructureManager = new DataStructureManager();
            uploadHelper = new UploadHelper();

            _dataset = dataset;
            _user = user;
            _data = data;
            _title = title;
            //_uploadMethod = uploadMethod;

            _dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(_dataset.DataStructure.Id);
            reader = new AsciiReader(_dataStructure, new AsciiFileReaderInfo());
        }

        /// <summary>
        /// starting the upload process
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Run()
        {
            return await Store();
        }

        public async Task<bool> Store()
        {
            try
            {
                Debug.WriteLine("start storing data");

                asciiWriter = new AsciiWriter(IO.TextSeperator.tab);
                //create a file in a user tempfolder
                _filepath = Path.Combine(AppConfiguration.DataPath, _user.UserName, "temp" + DateTime.Now.Millisecond + ".tsv");

                FileHelper.CreateDicrectoriesIfNotExist(Path.GetDirectoryName(_filepath));
                asciiWriter.CreateFile(Path.GetDirectoryName(_filepath), Path.GetFileName(_filepath));

                //store data into file without units
                asciiWriter.AddData(_data.Data, _data.Columns, _filepath, _dataset.DataStructure.Id);

                //todo send email to user
                var es = new EmailService();
                es.Send(MessageHelper.GetPushApiStoreHeader(_dataset.Id, _title),
                    MessageHelper.GetPushApiStoreMessage(_dataset.Id, _user.UserName),
                    new List<string>() { _user.Email },
                    new List<string>() { GeneralSettings.SystemEmail }
                    );
            }
            catch (Exception ex)
            {
                //
                var es = new EmailService();
                es.Send(MessageHelper.GetPushApiStoreHeader(_data.DatasetId, _title),
                    MessageHelper.GetPushApiStoreMessage(_dataset.Id, _user.UserName, new string[] { ex.Message }),
                    new List<string>() { _user.Email },
                    new List<string>() { GeneralSettings.SystemEmail }
                    );

                return false;
            }

            Debug.WriteLine("end storing data");

            return await PKCheck();


        }

        public async Task<bool> PKCheck()
        {
            List<string> errors = new List<string>();

            try
            {
                // primary Key check is only available by put api , so in this case it must be a putapiModel
                // and need to convert to it to get the primary keys lsit

                DataApiModel data = (DataApiModel)_data;
                string[] pks = null;
                //if (data != null) pks = data.PrimaryKeys;

                if (_dataStructure != null)
                {
                    bool IsUniqueInDb = uploadHelper.IsUnique2(_dataset.Id, variableIds);
                    bool IsUniqueInFile = uploadHelper.IsUnique(_dataset.Id, ".tsv", Path.GetFileName(_filepath), _filepath, new AsciiFileReaderInfo(), _dataStructure.Id);

                    if (!IsUniqueInDb || !IsUniqueInFile)
                    {
                        if (!IsUniqueInDb) errors.Add("The selected key is not unique in the data in the dataset.");
                        if (!IsUniqueInFile) errors.Add("The selected key is not unique in the received data.");
                    }
                }
                else
                {
                    errors.Add("The list of primary keys is empty.");
                }

                if (errors.Count == 0) return await Validate();
                else return false;
            }
            finally
            {
                var es = new EmailService();
                es.Send(MessageHelper.GetPushApiPKCheckHeader(_dataset.Id, _title),
                    MessageHelper.GetPushApiPKCheckMessage(_dataset.Id, _user.UserName, errors.ToArray()),
                    new List<string>() { _user.Email },
                    new List<string>() { GeneralSettings.SystemEmail }
                    );
            }
        }

        public async Task<bool> Validate()
        {
            Debug.WriteLine("start validate data");

            string error = "";
            //load strutcured data structure

            if (_dataStructure == null)
            {
                // send email to user ("failed to load datatructure");
                return false;
            }

            // validate file
            using (Stream = reader.Open(_filepath))
            {
                reader.ValidateFile(Stream, Path.GetFileName(_filepath), _dataset.Id);
                List<Error> errors = reader.ErrorMessages;

                // if errors exist -> send messages back
                if (errors.Count > 0)
                {
                    List<string> errorArray = new List<string>();

                    foreach (var e in errors)
                    {
                        errorArray.Add(e.GetMessage());
                    }

                    var es = new EmailService();
                    es.Send(MessageHelper.GetPushApiValidateHeader(_dataset.Id, _title),
                        MessageHelper.GetPushApiValidateMessage(_dataset.Id, _user.UserName, errorArray.ToArray()),
                        new List<string>() { _user.Email },
                        new List<string>() { GeneralSettings.SystemEmail }
                        );

                    return false;
                }
                else
                {
                    var es = new EmailService();
                    es.Send(MessageHelper.GetPushApiValidateHeader(_dataset.Id, _title),
                        MessageHelper.GetPushApiValidateMessage(_dataset.Id, _user.UserName),
                        new List<string>() { _user.Email },
                        new List<string>() { GeneralSettings.SystemEmail }
                        );
                }
                Debug.WriteLine("end validate data");
            }
            

            return await Upload();
        }

        public async Task<bool> Upload()
        {
            Debug.WriteLine("start upload data");

            FileStream Stream = null;

            DatasetVersion workingCopy = new DatasetVersion();
            List<DataTuple> rows = new List<DataTuple>();

            long id = _dataset.Id;
            string userName = _user.UserName;
            var es = new EmailService();

            try
            {
                List<long> datatupleFromDatabaseIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetManager.GetDatasetLatestVersion(_dataset.Id));

                if (FileHelper.FileExist(_filepath) && (datasetManager.IsDatasetCheckedOutFor(id, userName) || datasetManager.CheckOutDataset(id, userName)))
                {
                    workingCopy = datasetManager.GetDatasetWorkingCopy(id);

                    bool isUpdatingData = false; // if data exist, set to true
                    if(datatupleFromDatabaseIds.Count>0)isUpdatingData = true;

                    // update metadata based on system keys mappings
                    workingCopy.Metadata = setSystemValuesToMetadata(workingCopy.Id, datasetManager.GetDatasetVersionCount(workingCopy.Id) + 1, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata, isUpdatingData);


                    ////set modification
                    workingCopy.ModificationInfo = new EntityAuditInfo()
                    {
                        Performer = userName,
                        Comment = "Data",
                        ActionType = AuditActionType.Edit
                    };

                    //schleife
                    int counter = 0;
                    bool inputWasAltered = false;
                    do
                    {
                        counter++;
                        inputWasAltered = false;
                        using (Stream = reader.Open(_filepath))
                        {
                            rows = reader.ReadFile(Stream, Path.GetFileName(_filepath), id, packageSize);
                        }

                        // if errors exist, send email to user and stop process
                        if (reader.ErrorMessages.Count > 0)
                        {
                            List<string> errorArray = new List<string>();

                            foreach (var e in reader.ErrorMessages)
                            {
                                errorArray.Add(e.GetMessage());
                            }

                            //send error messages
                            es.Send(MessageHelper.GetPushApiUploadFailHeader(_dataset.Id, _title),
                                MessageHelper.GetPushApiUploadFailMessage(_dataset.Id, _user.UserName, errorArray.ToArray()),
                                new List<string>() { _user.Email },
                                new List<string>() { GeneralSettings.SystemEmail }
                                );

                            return false;
                        }

                        //Update Method -- append or update
                        if (rows.Count() > 0)
                        {
                            var splittedDatatuples = uploadHelper.GetSplitDatatuples(rows, variableIds, workingCopy, ref datatupleFromDatabaseIds);
                            datasetManager.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                            inputWasAltered = true;
                        }
                        
                    } while (rows.Count() > 0 || inputWasAltered == true);

                    datasetManager.CheckInDataset(id, "via API", userName);

                    string title = workingCopy.Title;

                    //send email
                    es.Send(MessageHelper.GetUpdateDatasetHeader(id),
                        MessageHelper.GetUpdateDatasetMessage(id, title, _user.DisplayName, typeof(Dataset).Name),
                        new List<string>() { _user.Email },
                               new List<string>() { GeneralSettings.SystemEmail }
                        );
                }
                else
                {
                    //ToDo send email to user
                    es.Send(MessageHelper.GetPushApiUploadFailHeader(_dataset.Id, _title),
                               MessageHelper.GetPushApiUploadFailMessage(_dataset.Id, _user.UserName, new string[] { "The temporarily stored data could not be read or the dataset is already in checkout status." }),
                               new List<string>() { _user.Email },
                               new List<string>() { GeneralSettings.SystemEmail }
                               );
                }

                return true;
            }
            catch (Exception ex)
            {
                if (datasetManager.IsDatasetCheckedOutFor(id, userName))
                    datasetManager.UndoCheckoutDataset(id, userName);

                //ToDo send email to user
                es.Send(MessageHelper.GetPushApiUploadFailHeader(_dataset.Id, _title),
                                MessageHelper.GetPushApiUploadFailMessage(_dataset.Id, _user.UserName, new string[] { ex.Message }),
                                new List<string>() { _user.Email },
                                new List<string>() { GeneralSettings.SystemEmail }
                                );

                return false;
            }
            finally
            {
                Debug.WriteLine("end of upload");
            }
        }

        private XmlDocument setSystemValuesToMetadata(long datasetid, long version, long metadataStructureId, XmlDocument metadata, bool updateData = false)
        {
            SystemMetadataHelper SystemMetadataHelper = new SystemMetadataHelper();

            Key[] myObjArray = { };

            if (updateData) myObjArray = new Key[] { Key.Id, Key.Version, Key.DateOfVersion, Key.DataLastModified };
            else myObjArray = new Key[] { Key.Id, Key.Version, Key.DataCreationDate, Key.DataLastModified };

            metadata = SystemMetadataHelper.SetSystemValuesToMetadata(datasetid, version, metadataStructureId, metadata, myObjArray);

            return metadata;
        }
    }
}