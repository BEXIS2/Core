using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Output;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dim.UI.Models.API;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dim.UI.Helper.API
{
    public class PushDataApiHelper
    {
        DatasetManager datasetManager = new DatasetManager();
        UserManager userManager = new UserManager();
        EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
        DataStructureManager dataStructureManager = new DataStructureManager();
        FileStream Stream = null;
        AsciiReader reader = null;
        AsciiWriter asciiWriter = null;


        int packageSize = 10000;
        string _filepath = "";
        Dataset _dataset;
        User _user;
        PushDataModel _data = null;


        public PushDataApiHelper(Dataset dataset, User user, PushDataModel data)
        {
            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            DataStructureManager dataStructureManager = new DataStructureManager();

            _dataset = dataset;
            _user = user;
            _data = data;
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

                //store data into file
                asciiWriter.AddData(_data.Data, _data.Columns, _filepath, _dataset.DataStructure.Id);

                //todo send email to user
                var es = new EmailService();
                es.Send(MessageHelper.GetPushApiStoreHeader(),
                    MessageHelper.GetPushApiStoreMessage(_dataset.Id, _user.UserName),
                    new List<string>() { _user.Email },
                    new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                    );
            }
            catch (Exception ex)
            {
                //
                var es = new EmailService();
                es.Send(MessageHelper.GetPushApiStoreHeader(),
                    MessageHelper.GetPushApiStoreMessage(_dataset.Id, _user.UserName, new string[] { ex.Message }),
                    new List<string>() { _user.Email },
                    new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                    );

                return false;

            }

            Debug.WriteLine("end storing data");


            return await Validate();
        }

        public async Task<bool> Validate()
        {
            Debug.WriteLine("start validate data");

            string error = "";
            //load strutcured data structure
            StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(_dataset.DataStructure.Id);

            if (dataStructure == null)
            {
                // send email to user ("failed to load datatructure");
                return false;
            }



            // validate file
            reader = new AsciiReader(dataStructure, new AsciiFileReaderInfo());
            Stream = reader.Open(_filepath);
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
                es.Send(MessageHelper.GetPushApiValidateHeader(_dataset.Id),
                    MessageHelper.GetPushApiValidateMessage(_dataset.Id, _user.UserName, errorArray.ToArray()),
                    new List<string>() { _user.Email },
                    new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                    );

                return false;

            }
            else
            {
                var es = new EmailService();
                es.Send(MessageHelper.GetPushApiValidateHeader(_dataset.Id),
                    MessageHelper.GetPushApiValidateMessage(_dataset.Id, _user.UserName),
                    new List<string>() { _user.Email },
                    new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                    );
            }
            Debug.WriteLine("end validate data");


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

                if (FileHelper.FileExist(_filepath) && (datasetManager.IsDatasetCheckedOutFor(id, userName) || datasetManager.CheckOutDataset(id, userName)))
                {
                    workingCopy = datasetManager.GetDatasetWorkingCopy(id);

                    //schleife
                    int counter = 0;
                    bool inputWasAltered = false;
                    do
                    {
                        counter++;
                        inputWasAltered = false;
                        Stream = reader.Open(_filepath);
                        rows = reader.ReadFile(Stream, Path.GetFileName(_filepath), id, packageSize);
                        Stream.Close();

                        // if errors exist, send email to user and stop process
                        if (reader.ErrorMessages.Count > 0)
                        {
                            List<string> errorArray = new List<string>();

                            foreach (var e in reader.ErrorMessages)
                            {
                                errorArray.Add(e.GetMessage());
                            }

                            //send error messages
                            es.Send(MessageHelper.GetPushApiUploadFailHeader(_dataset.Id),
                                MessageHelper.GetPushApiUploadFailMessage(_dataset.Id, _user.UserName, errorArray.ToArray()),
                                new List<string>() { _user.Email },
                                new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                                );

                            return false;
                        }

                        //Update Method -- append or update
                        if (_data.UpdateMethod == UpdateMethod.Append)
                        {
                            if (rows.Count > 0)
                            {
                                datasetManager.EditDatasetVersion(workingCopy, rows, null, null);
                                inputWasAltered = true;
                            }
                        }
                        //todo add updateMethod update 

                    } while (rows.Count() > 0 || inputWasAltered == true);

                    datasetManager.CheckInDataset(id, "upload data via api", userName);


                    XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                    string title = xmlDatasetHelper.GetInformation(id, NameAttributeValues.title);

                    //send email
                    es.Send(MessageHelper.GetUpdateDatasetHeader(),
                        MessageHelper.GetUpdateDatasetMessage(id, title, userName),
                        new List<string>() { _user.Email },
                               new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                        );

                }
                else
                {
                    //ToDo send email to user
                    es.Send(MessageHelper.GetPushApiUploadFailHeader(_dataset.Id),
                               MessageHelper.GetPushApiUploadFailMessage(_dataset.Id, _user.UserName, new string[] { "The temporarily stored data could not be read or the dataset is already in checkout status." }),
                               new List<string>() { _user.Email },
                               new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                               );

                }

                return true;


            }
            catch (Exception ex)
            {
                if (datasetManager.IsDatasetCheckedOutFor(id, userName))
                    datasetManager.UndoCheckoutDataset(id, userName);

                //ToDo send email to user
                es.Send(MessageHelper.GetPushApiUploadFailHeader(_dataset.Id),
                                MessageHelper.GetPushApiUploadFailMessage(_dataset.Id, _user.UserName, new string[] { ex.Message }),
                                new List<string>() { _user.Email },
                                new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                                );

                return false;
            }
            finally
            {
                Debug.WriteLine("end of upload");
            }
        }

    }
}