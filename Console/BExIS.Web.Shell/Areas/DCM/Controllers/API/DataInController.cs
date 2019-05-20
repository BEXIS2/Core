using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Helpers.API;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Orm.NH.Qurying;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Output;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models.API;
using BExIS.Modules.Dcm.UI.Helper.API;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Upload;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request datasets or a view on data sets and get the result in
    /// either of XML, JSON, or CSV formats.
    /// </summary>
    /// <remarks>
    /// This class is designed as a Web API to allow various client tools request datasets or a view on data sets and get the result in
    /// either of XML, JSON, or CSV formats.
    /// The design follows the RESTFull pattern mentioned in http://www.asp.net/web-api/overview/older-versions/creating-a-web-api-that-supports-crud-operations
    /// CSV formatter is implemented in the DataTupleCsvFormatter class in the Models folder.
    /// The formatter is registered in the WebApiConfig as an automatic formatter, so if the clinet sets the request's Mime type to text/csv, this formatter will be automatically engaged.
    /// text/xml and text/json return XML and JSON content accordingly.
    /// </remarks>
    public class DataInController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        // POST: api/data
        /// <summary>
        /// append data to a dataset
        /// </summary>
        /// <param name="data"></param>
        [BExISApiAuthorize]
        [Route("api/Data")]
        public async Task<HttpResponseMessage> Post([FromBody]PushDataApiModel data)
        {
            var request = Request.CreateResponse();
            User user = null;
            string error = "";

            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            DataStructureManager dataStructureManager = new DataStructureManager();
            ApiConfigurator apiHelper = new ApiConfigurator();

            DatasetVersion workingCopy = new DatasetVersion();
            List<DataTuple> rows = new List<DataTuple>();

            //load from apiConfig
            int cellLimit = 10000;
            if (apiHelper != null && apiHelper.Settings.ContainsKey(ApiConfigurator.CELLS))
            {
                Int32.TryParse(apiHelper.Settings[ApiConfigurator.CELLS], out cellLimit);
            }

            try
            {
                #region security

                string token = this.Request.Headers.Authorization?.Parameter;

                if (String.IsNullOrEmpty(token))
                {
                    request.Content = new StringContent("Bearer token not exist.");

                    return request;
                }

                user = userManager.Users.Where(u => u.Token.Equals(token)).FirstOrDefault();

                if (user == null)
                {
                    request.Content = new StringContent("Token is not valid.");

                    return request;
                }

                //check permissions

                //entity permissions
                if (data.DatasetId > 0)
                {
                    Dataset d = datasetManager.GetDataset(data.DatasetId);
                    if (d == null)
                    {
                        request.Content = new StringContent("the dataset with the id (" + data.DatasetId + ") does not exist.");

                        return request;
                    }

                    if (!entityPermissionManager.HasEffectiveRight(user.Name, "Dataset", typeof(Dataset), data.DatasetId, RightType.Write))
                    {
                        request.Content = new StringContent("The token is not authorized to write into the dataset.");

                        return request;
                    }
                }

                #endregion security

                #region incomming values check

                // check incomming values

                if (data.DatasetId == 0) error += "dataset id should be greater then 0.";
                //if (data.UpdateMethod == null) error += "update method is not set";
                if (data.Columns == null) error += "cloumns should not be null. ";
                if (data.Data == null) error += "data is empty. ";

                if (!string.IsNullOrEmpty(error))
                {
                    request.Content = new StringContent(error);

                    return request;
                }

                #endregion incomming values check

                Dataset dataset = datasetManager.GetDataset(data.DatasetId);
                if (dataset == null)
                {
                    request.Content = new StringContent("Dataset not exist.");

                    return request;
                }

                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                string title = xmlDatasetHelper.GetInformation(dataset, NameAttributeValues.title);

                if ((data.Data.Count() * data.Columns.Count()) > cellLimit)
                {
                    #region async upload with big data

                    // if dataste is not in the dataset

                    DataApiHelper helper = new DataApiHelper(dataset, user, data, title, UploadMethod.Append);
                    Task.Run(() => helper.Run());

                    #endregion async upload with big data

                    request.Content = new StringContent("Data has been successfully received and is being processed. For larger data, as in this case, we will keep you informed by mail about the next steps.");

                    Debug.WriteLine("end of api call");

                    return request;
                }
                else
                {
                    #region direct upload

                    var es = new EmailService();

                    try
                    {
                        //load strutcured data structure
                        StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataset.DataStructure.Id);
                        List<Error> errors = new List<Error>();
                        if (dataStructure == null)
                        {
                            request.Content = new StringContent("The Datastructure does not exist.");
                            return request;
                        }

                        APIDataReader reader = new APIDataReader(dataStructure, new ApiFileReaderInfo());

                        List<VariableIdentifier> source = new List<VariableIdentifier>();
                        reader.SetSubmitedVariableIdentifiers(data.Columns.ToList());
                        //validate datastructure
                        foreach (string c in data.Columns)
                        {
                            source.Add(new VariableIdentifier() { name = c });
                        }

                        errors = reader.ValidateComparisonWithDatatsructure(source);
                        if (errors != null && errors.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder("The Datastructure is not valid.");

                            foreach (var e in errors)
                            {
                                sb.AppendLine(e.ToHtmlString());
                            }

                            request.Content = new StringContent(sb.ToString());
                            return request;
                        }

                        errors = new List<Error>();
                        // validate rows
                        for (int i = 0; i < data.Data.Length; i++)
                        {
                            string[] row = data.Data[i];
                            errors.AddRange(reader.ValidateRow(row.ToList(), i));
                        }

                        if (errors != null && errors.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder("The Data is not valid.");

                            foreach (var e in errors)
                            {
                                sb.AppendLine(e.ToHtmlString());
                            }

                            request.Content = new StringContent(sb.ToString());
                            return request;
                        }

                        if (datasetManager.IsDatasetCheckedOutFor(dataset.Id, user.UserName) || datasetManager.CheckOutDataset(dataset.Id, user.UserName))
                        {
                            workingCopy = datasetManager.GetDatasetWorkingCopy(dataset.Id);

                            List<DataTuple> datatuples = new List<DataTuple>();

                            for (int i = 0; i < data.Data.Length; i++)
                            {
                                string[] row = data.Data[i];

                                datatuples.Add(reader.ReadRow(row.ToList(), i));
                            }

                            if (datatuples.Count > 0)
                            {
                                datasetManager.EditDatasetVersion(workingCopy, datatuples, null, null);
                            }

                            datasetManager.CheckInDataset(dataset.Id, "upload data via api", user.UserName);

                            //send email
                            es.Send(MessageHelper.GetUpdateDatasetHeader(),
                                MessageHelper.GetUpdateDatasetMessage(dataset.Id, title, user.UserName),
                                new List<string>() { user.Email },
                                       new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                                );
                        }

                        request.Content = new StringContent("Data successfully uploaded.");
                        return request;
                    }
                    catch (Exception ex)
                    {
                        //ToDo send email to user
                        es.Send(MessageHelper.GetPushApiUploadFailHeader(dataset.Id, title),
                                   MessageHelper.GetPushApiUploadFailMessage(dataset.Id, user.UserName, new string[] { "Upload failed: " + ex.Message }),
                                   new List<string>() { user.Email },
                                   new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                                   );

                        request.Content = new StringContent(ex.Message);
                        return request;
                    }

                    #endregion direct upload
                }
            }
            finally
            {
                datasetManager.Dispose();
                entityPermissionManager.Dispose();
                dataStructureManager.Dispose();
                userManager.Dispose();
            }
        }

        // PUT: api/data/5
        /// <summary>
        /// append and update data to an existing dataset
        /// </summary>
        [BExISApiAuthorize]
        [Route("api/Data")]
        public async Task<HttpResponseMessage> Put([FromBody]PutDataApiModel data)
        {
            var request = Request.CreateResponse();
            User user = null;
            string error = "";

            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            DataStructureManager dataStructureManager = new DataStructureManager();
            ApiConfigurator apiHelper = new ApiConfigurator();

            DatasetVersion workingCopy = new DatasetVersion();
            List<DataTuple> rows = new List<DataTuple>();

            //load from apiConfig
            int cellLimit = 10000;
            if (apiHelper != null && apiHelper.Settings.ContainsKey(ApiConfigurator.CELLS))
            {
                Int32.TryParse(apiHelper.Settings[ApiConfigurator.CELLS], out cellLimit);
            }

            try
            {
                #region security

                string token = this.Request.Headers.Authorization?.Parameter;

                if (String.IsNullOrEmpty(token))
                {
                    request.Content = new StringContent("Bearer token not exist.");

                    return request;
                }

                user = userManager.Users.Where(u => u.Token.Equals(token)).FirstOrDefault();

                if (user == null)
                {
                    request.Content = new StringContent("Token is not valid.");

                    return request;
                }

                //check permissions

                //entity permissions
                if (data.DatasetId > 0)
                {
                    Dataset d = datasetManager.GetDataset(data.DatasetId);
                    if (d == null)
                    {
                        request.Content = new StringContent("the dataset with the id (" + data.DatasetId + ") does not exist.");

                        return request;
                    }

                    if (!entityPermissionManager.HasEffectiveRight(user.Name, "Dataset", typeof(Dataset), data.DatasetId, RightType.Write))
                    {
                        request.Content = new StringContent("The token is not authorized to write into the dataset.");

                        return request;
                    }
                }

                #endregion security

                #region incomming values check

                // check incomming values

                if (data.DatasetId == 0) error += "dataset id should be greater then 0.";
                //if (data.UpdateMethod == null) error += "update method is not set";
                //if (data.Count == 0) error += "count should be greater then 0. ";
                if (data.Columns == null) error += "cloumns should not be null. ";
                if (data.Data == null) error += "data is empty. ";
                if (data.PrimaryKeys == null || data.PrimaryKeys.Count() == 0) error += "the UpdateMethod update has been selected but there are no primary keys available. ";

                if (!string.IsNullOrEmpty(error))
                {
                    request.Content = new StringContent(error);

                    return request;
                }

                #endregion incomming values check

                Dataset dataset = datasetManager.GetDataset(data.DatasetId);
                if (dataset == null)
                {
                    request.Content = new StringContent("Dataset not exist.");

                    return request;
                }

                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                string title = xmlDatasetHelper.GetInformation(dataset, NameAttributeValues.title);

                if ((data.Data.Count() * data.Columns.Count()) > cellLimit)
                {
                    #region async upload with big data

                    // if dataste is not in the dataset

                    DataApiHelper helper = new DataApiHelper(dataset, user, data, title, UploadMethod.Update);
                    Task.Run(() => helper.Run());

                    #endregion async upload with big data

                    request.Content = new StringContent("Data has been successfully received and is being processed. For larger data, as in this case, we will keep you informed by mail about the next steps.");

                    Debug.WriteLine("end of api call");

                    return request;
                }
                else
                {
                    #region direct update

                    var es = new EmailService();
                    UploadHelper uploadHelper = new UploadHelper();

                    try
                    {
                        //load strutcured data structure
                        StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataset.DataStructure.Id);
                        List<Error> errors = new List<Error>();
                        if (dataStructure == null)
                        {
                            request.Content = new StringContent("The Datastructure does not exist.");
                            return request;
                        }

                        APIDataReader reader = new APIDataReader(dataStructure, new ApiFileReaderInfo());

                        List<VariableIdentifier> source = new List<VariableIdentifier>();
                        reader.SetSubmitedVariableIdentifiers(data.Columns.ToList());

                        #region primary key check

                        //prepare  primary keys ids from the exiting dataset
                        List<long> variableIds = new List<long>();
                        foreach (var variable in dataStructure.Variables)
                        {
                            if (data.PrimaryKeys.Any(p => p.ToLower().Equals(variable.Label.ToLower()))) variableIds.Add(variable.Id);
                        }

                        // prepare pk index list from data
                        int[] primaryKeyIndexes = new int[data.PrimaryKeys.Length];
                        for (int i = 0; i < data.PrimaryKeys.Length; i++)
                        {
                            string pk = data.PrimaryKeys[i];
                            primaryKeyIndexes[i] = data.Columns.ToList().IndexOf(pk);
                        }

                        //check primary with data : uniqueness
                        bool IsUniqueInDb = uploadHelper.IsUnique2(dataset.Id, variableIds);
                        bool IsUniqueInData = uploadHelper.IsUnique(primaryKeyIndexes, data.Data);

                        if (!IsUniqueInDb || !IsUniqueInData)
                        {
                            StringBuilder sb = new StringBuilder("Error/s in Primary Keys selection:<br>");
                            if (!IsUniqueInDb) sb.AppendLine("The selected key is not unique in the data in the dataset.");
                            if (!IsUniqueInData) sb.AppendLine("The selected key is not unique in the received data.");

                            request.Content = new StringContent(sb.ToString());
                            return request;
                        }

                        #endregion primary key check

                        #region validate datastructure

                        foreach (string c in data.Columns)
                        {
                            source.Add(new VariableIdentifier() { name = c });
                        }

                        errors = reader.ValidateComparisonWithDatatsructure(source);
                        if (errors != null && errors.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder("The Datastructure is not valid.");

                            foreach (var e in errors)
                            {
                                sb.AppendLine(e.ToHtmlString());
                            }

                            request.Content = new StringContent(sb.ToString());
                            return request;
                        }

                        #endregion validate datastructure

                        #region validate data

                        errors = new List<Error>();
                        // validate rows
                        for (int i = 0; i < data.Data.Length; i++)
                        {
                            string[] row = data.Data[i];
                            errors.AddRange(reader.ValidateRow(row.ToList(), i));
                        }

                        if (errors != null && errors.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder("The Data is not valid.");

                            foreach (var e in errors)
                            {
                                sb.AppendLine(e.ToHtmlString());
                            }

                            request.Content = new StringContent(sb.ToString());
                            return request;
                        }

                        #endregion validate data

                        #region update data

                        List<long> datatupleFromDatabaseIds = datasetManager.GetDatasetVersionEffectiveTupleIds(datasetManager.GetDatasetLatestVersion(dataset.Id));

                        if (datasetManager.IsDatasetCheckedOutFor(dataset.Id, user.UserName) || datasetManager.CheckOutDataset(dataset.Id, user.UserName))
                        {
                            workingCopy = datasetManager.GetDatasetWorkingCopy(dataset.Id);

                            List<DataTuple> datatuples = new List<DataTuple>();

                            for (int i = 0; i < data.Data.Length; i++)
                            {
                                string[] row = data.Data[i];

                                datatuples.Add(reader.ReadRow(row.ToList(), i));
                            }

                            //Update Method -- UPDATE
                            //splite datatuples into new and updated tuples

                            if (datatuples.Count > 0)
                            {
                                var splittedDatatuples = uploadHelper.GetSplitDatatuples(datatuples, variableIds, workingCopy, ref datatupleFromDatabaseIds);
                                datasetManager.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                            }

                            datasetManager.CheckInDataset(dataset.Id, "upload data via api", user.UserName);

                            //send email
                            es.Send(MessageHelper.GetUpdateDatasetHeader(),
                                MessageHelper.GetUpdateDatasetMessage(dataset.Id, title, user.UserName),
                                new List<string>() { user.Email },
                                       new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                                );
                        }

                        #endregion update data

                        request.Content = new StringContent("Data successfully uploaded.");
                        return request;
                    }
                    catch (Exception ex)
                    {
                        //ToDo send email to user
                        es.Send(MessageHelper.GetPushApiUploadFailHeader(dataset.Id, title),
                                   MessageHelper.GetPushApiUploadFailMessage(dataset.Id, user.UserName, new string[] { "Upload failed: " + ex.Message }),
                                   new List<string>() { user.Email },
                                   new List<string>() { ConfigurationManager.AppSettings["SystemEmail"] }
                                   );

                        request.Content = new StringContent(ex.Message);
                        return request;
                    }

                    #endregion direct update
                }
            }
            finally
            {
                datasetManager.Dispose();
                entityPermissionManager.Dispose();
                dataStructureManager.Dispose();
                userManager.Dispose();
            }
        }

        // DELETE: api/data/5
        /// <summary>
        /// Deletes an existing dataset
        /// </summary>
        /// <param name="id"></param>
        [ApiExplorerSettings(IgnoreApi = true)]
        [BExISApiAuthorize]
        [Route("api/Data")]
        public void Delete(int id)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}