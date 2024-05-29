using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Helper.API;
using BExIS.Modules.Dcm.UI.Models.API;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Route;
using BExIS.Utils.Upload;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Vaiona.Entities.Common;
using Vaiona.Web.Mvc.Modularity;
using BExIS.Modules.Dcm.UI.Helpers;
using System.Xml;
using BExIS.Dim.Entities.Mappings;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request datasets or a view on data sets and get the result in
    /// CSV formats.
    /// </summary>
    /// <remarks>
    /// This class is designed as a Web API to allow various client tools request datasets or a view on data sets and get the result in
    /// CSV formats.
    /// The design follows the RESTFull pattern mentioned in http://www.asp.net/web-api/overview/older-versions/creating-a-web-api-that-supports-crud-operations
    /// CSV formatter is implemented in the DataTupleCsvFormatter class in the Models folder.
    /// The formatter is registered in the WebApiConfig as an automatic formatter, so if the clinet sets the request's Mime type to text/csv, this formatter will be automatically engaged.
    /// text/xml and text/json return XML and JSON content accordingly.
    /// </remarks>
    public class DataInController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        
        // PUT: api/data/5
        /// <summary>
        /// append and update data to an existing dataset
        /// </summary>
        [BExISApiAuthorize]
        [PutRoute("api/Data")]
        public async Task<HttpResponseMessage> Put([FromBody] DataApiModel data)
        {
            var settings = ModuleManager.GetModuleSettings("Dcm");

            var request = Request.CreateResponse();
            User user = null;
            string error = "";

            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            DataStructureManager dataStructureManager = new DataStructureManager();

            DatasetVersion workingCopy = new DatasetVersion();
            List<DataTuple> rows = new List<DataTuple>();

            //load from apiConfig
            int cellLimit = 100000;
            Int32.TryParse(settings.GetValueByKey("celllimit").ToString(), out cellLimit);

            try
            {
                #region security

                user = ControllerContext.RouteData.Values["user"] as User;

                if (user == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Token is not valid.");

                //check permissions

                //entity permissions
                if (data.DatasetId > 0)
                {
                    Dataset d = datasetManager.GetDataset(data.DatasetId);
                    if (d == null)
                        return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset with the id (" + data.DatasetId + ") does not exist.");

                    if (!entityPermissionManager.HasEffectiveRight(user.Name, typeof(Dataset), data.DatasetId, RightType.Write))
                        return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "The token is not authorized to write into the dataset.");
                }

                #endregion security

                #region incomming values check

                // check incomming values

                if (data.DatasetId == 0) error += "dataset id should be greater then 0.";
                //if (data.UpdateMethod == null) error += "update method is not set";
                //if (data.Count == 0) error += "count should be greater then 0. ";
                if (data.Columns == null) error += "cloumns should not be null. ";
                if (data.Data == null) error += "data is empty. ";

                if (!string.IsNullOrEmpty(error))
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, error);

                #endregion incomming values check

                Dataset dataset = datasetManager.GetDataset(data.DatasetId);
                if (dataset == null)
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Dataset not exist.");

                DatasetVersion dsv = datasetManager.GetDatasetLatestVersion(dataset);
                string title = dsv.Title;

                if ((data.Data.Count() * data.Columns.Count()) > cellLimit)
                {
                    #region async upload with big data

                    // if dataste is not in the dataset

                    DataApiHelper helper = new DataApiHelper(dataset, user, data, title, UploadMethod.Update);
                    Task.Run(() => helper.Run());

                    #endregion async upload with big data

                    Debug.WriteLine("end of api call");

                    return Request.CreateResponse(HttpStatusCode.OK, "Data has been successfully received and is being processed. For larger data, as in this case, we will keep you informed by mail about the next steps.");
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
                            return Request.CreateResponse(HttpStatusCode.ExpectationFailed, "The Datastructure does not exist.");

                        APIDataReader reader = new APIDataReader(dataStructure, new ApiFileReaderInfo());

                        List<VariableIdentifier> source = new List<VariableIdentifier>();
                        reader.SetSubmitedVariableIdentifiers(data.Columns.ToList());

                        #region primary key check

                        List<VariableInstance> pkVariables = new List<VariableInstance>();
                        pkVariables = dataStructure.Variables.Where(v => v.IsKey).ToList();

                        // prepare pk index list from data
                        int[] primaryKeyIndexes = new int[pkVariables.Count];
                        for (int i = 0; i <= dataStructure.Variables.Count-1; i++)
                        {
                            var variable = dataStructure.Variables.ElementAt(i);
                            if (variable.IsKey)
                            {
                                primaryKeyIndexes[i] = data.Columns.ToList().IndexOf(variable.Label);
                            }
                        }

                        //check primary with data : uniqueness
                        //bool IsUniqueInDb = uploadHelper.IsUnique2(dataset.Id, variables.Select(v=>v.Id).ToList()); // may can removed
                        bool IsUniqueInData = uploadHelper.IsUnique(primaryKeyIndexes, data.Data);

                        if (!IsUniqueInData)
                        {
                            StringBuilder sb = new StringBuilder("Error/s in Primary Keys selection:<br>");
                            //if (!IsUniqueInDb) sb.AppendLine("The selected key is not unique in the data in the dataset.");
                            if (!IsUniqueInData) sb.AppendLine("The selected key is not unique in the received data.");

                            return Request.CreateResponse(HttpStatusCode.ExpectationFailed, sb.ToString());
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

                            return Request.CreateResponse(HttpStatusCode.ExpectationFailed, sb.ToString());
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

                            return Request.CreateResponse(HttpStatusCode.ExpectationFailed, sb.ToString());
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
                                var splittedDatatuples = uploadHelper.GetSplitDatatuples(datatuples, pkVariables.Select(v=>v.Id).ToList(), workingCopy, ref datatupleFromDatabaseIds);
                                datasetManager.EditDatasetVersion(workingCopy, splittedDatatuples["new"], splittedDatatuples["edit"], null);
                            }

                            // update metadata based on system keys mappings
                            workingCopy.Metadata = setSystemValuesToMetadata(workingCopy.Id, datasetManager.GetDatasetVersionCount(workingCopy.Id) + 1, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata, true);

                            ////set modification
                            workingCopy.ModificationInfo = new EntityAuditInfo()
                            {
                                Performer = user.UserName,
                                Comment = "Data",
                                ActionType = AuditActionType.Edit
                            };
                            datasetManager.EditDatasetVersion(workingCopy, null, null, null);

                            datasetManager.CheckInDataset(dataset.Id, data.Data.Length + " rows via api.", user.UserName);

                            //send email
                            es.Send(MessageHelper.GetUpdateDatasetHeader(dataset.Id),
                                MessageHelper.GetUpdateDatasetMessage(dataset.Id, title, user.DisplayName, typeof(Dataset).Name),
                                new List<string>() { user.Email },
                                       new List<string>() { GeneralSettings.SystemEmail }
                                );
                        }

                        #endregion update data

                        return Request.CreateResponse(HttpStatusCode.OK, "Data successfully uploaded.");
                    }
                    catch (Exception ex)
                    {
                        //ToDo send email to user
                        es.Send(MessageHelper.GetPushApiUploadFailHeader(dataset.Id, title),
                                   MessageHelper.GetPushApiUploadFailMessage(dataset.Id, user.UserName, new string[] { "Upload failed: " + ex.Message }),
                                   new List<string>() { user.Email },
                                   new List<string>() { GeneralSettings.SystemEmail }
                                   );

                        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
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
                request.Dispose();
            }
        }

        // DELETE: api/data/5
        /// <summary>
        /// Deletes an existing dataset
        /// </summary>
        /// <param name="id"></param>
        [ApiExplorerSettings(IgnoreApi = true)]
        [BExISApiAuthorize]
        [DeleteRoute("api/Data")]
        public void Delete(int id)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        private XmlDocument setSystemValuesToMetadata(long datasetid, long version, long metadataStructureId, XmlDocument metadata, bool updateData = false)
        {
            SystemMetadataHelper SystemMetadataHelper = new SystemMetadataHelper();

            Key[] myObjArray = { };

            if(updateData)myObjArray = new Key[] { Key.Id, Key.Version, Key.DateOfVersion, Key.DataLastModified };
            else myObjArray = new Key[] { Key.Id, Key.Version, Key.DataCreationDate, Key.DataLastModified };

            metadata = SystemMetadataHelper.SetSystemValuesToMetadata(datasetid, version, metadataStructureId, metadata, myObjArray);

            return metadata;
        }
    }
}