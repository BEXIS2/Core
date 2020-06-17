﻿using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dim.UI.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request metadata of a datasets and get the result in XML.
    /// </summary>
    public class MetadataInController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        // POST: api/Metadata
        [BExISApiAuthorize]
        [PostRoute("api/Metadata")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Metadata/5
        [BExISApiAuthorize]
        [PutRoute("api/Metadata/{id}")]
        public async Task<HttpResponseMessage> Put(int id)
        {
            var request = Request.CreateResponse();
            User user = null;
            string error = "";

            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

            try
            {
                #region security

                string token = this.Request.Headers.Authorization?.Parameter;

                if (String.IsNullOrEmpty(token))
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Bearer token not exist.");

                user = userManager.Users.Where(u => u.Token.Equals(token)).FirstOrDefault();

                if (user == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Token is not valid.");

                //check permissions

                //entity permissions
                if (id > 0)
                {
                    Dataset d = datasetManager.GetDataset(id);
                    if (d == null)
                        return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset with the id (" + id + ") does not exist.");

                    if (!entityPermissionManager.HasEffectiveRight(user.Name, typeof(Dataset), id, RightType.Write))
                        return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "The token is not authorized to write into the dataset.");
                }

                #endregion security

                #region check incomming metadata

                Stream requestStream = await this.Request.Content.ReadAsStreamAsync();

                string contentType = this.Request.Content.Headers.ContentType.MediaType;

                if (string.IsNullOrEmpty(contentType) || (!contentType.Equals("application/xml") && !contentType.Equals("text/plain")))
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "The transmitted file is not a xml document.");

                if (requestStream == null)
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Metadata xml was not received.");

                #endregion check incomming metadata

                #region incomming values check

                // check incomming values

                if (id == 0) error += "dataset id should be greater then 0.";
                ////if (data.UpdateMethod == null) error += "update method is not set";
                ////if (data.Count == 0) error += "count should be greater then 0. ";
                //if (data.Columns == null) error += "cloumns should not be null. ";
                //if (data.Data == null) error += "data is empty. ";
                //if (data.PrimaryKeys == null || data.PrimaryKeys.Count() == 0) error += "the UpdateMethod update has been selected but there are no primary keys available. ";

                if (!string.IsNullOrEmpty(error))
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, error);

                #endregion incomming values check

                Dataset dataset = datasetManager.GetDataset(id);
                if (dataset == null)
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Dataset not exist.");

                #region convert metadata

                XmlDocument metadataForImport = new XmlDocument();
                metadataForImport.Load(requestStream);

                // metadataStructure ID
                var metadataStructureId = dataset.MetadataStructure.Id;
                var metadataStructrueName = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId).Name;

                // loadMapping file
                var path_mappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), XmlMetadataImportHelper.GetMappingFileName(metadataStructureId, TransmissionType.mappingFileImport, metadataStructrueName));

                // XML mapper + mapping file
                var xmlMapperManager = new XmlMapperManager(TransactionDirection.ExternToIntern);
                xmlMapperManager.Load(path_mappingFile, "IDIV");

                // generate intern metadata without internal attributes
                var metadataResult = xmlMapperManager.Generate(metadataForImport, 1, true);

                // generate intern template metadata xml with needed attribtes
                var xmlMetadatWriter = new XmlMetadataWriter(BExIS.Xml.Helpers.XmlNodeMode.xPath);
                var metadataXml = xmlMetadatWriter.CreateMetadataXml(metadataStructureId,
                    XmlUtility.ToXDocument(metadataResult));

                var metadataXmlTemplate = XmlMetadataWriter.ToXmlDocument(metadataXml);

                // set attributes FROM metadataXmlTemplate TO metadataResult
                var completeMetadata = XmlMetadataImportHelper.FillInXmlValues(metadataResult,
                    metadataXmlTemplate);

                #endregion convert metadata

                if (completeMetadata != null)
                {
                    string title = "";
                    if (datasetManager.IsDatasetCheckedOutFor(id, user.Name) || datasetManager.CheckOutDataset(id, user.Name))
                    {
                        DatasetVersion workingCopy = datasetManager.GetDatasetWorkingCopy(id);
                        workingCopy.Metadata = completeMetadata;
                        workingCopy.Title = xmlDatasetHelper.GetInformation(id, completeMetadata, NameAttributeValues.title);
                        workingCopy.Description = xmlDatasetHelper.GetInformation(id, completeMetadata, NameAttributeValues.description);

                        //check if modul exist
                        int v = 1;
                        if (workingCopy.Dataset.Versions != null && workingCopy.Dataset.Versions.Count > 1) v = workingCopy.Dataset.Versions.Count();

                        //set status
                        if (workingCopy.StateInfo == null) workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo();
                        workingCopy.StateInfo.State = DatasetStateInfo.NotValid.ToString();

                        title = workingCopy.Title;
                        if (string.IsNullOrEmpty(title)) title = "No Title available.";

                        datasetManager.EditDatasetVersion(workingCopy, null, null, null);
                        datasetManager.CheckInDataset(id, "via api.", user.Name, ViewCreationBehavior.None);
                    }

                    // ToDo add Index update to this api
                    //if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
                    //{
                    //    var x = this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", datasetId } });
                    //}

                    LoggerFactory.LogData(id.ToString(), typeof(Dataset).Name, Vaiona.Entities.Logging.CrudState.Created);

                    var es = new EmailService();
                    es.Send(MessageHelper.GetUpdateDatasetHeader(),
                        MessageHelper.GetUpdateDatasetMessage(id, title, user.UserName),
                        ConfigurationManager.AppSettings["SystemEmail"]
                        );
                }

                return Request.CreateErrorResponse(HttpStatusCode.OK, "Metadata successfully updated ");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                datasetManager.Dispose();
                entityPermissionManager.Dispose();
                userManager.Dispose();
            }
        }

        // DELETE: api/Metadata/5
        [ApiExplorerSettings(IgnoreApi = true)]
        [BExISApiAuthorize]
        [DeleteRoute("api/Metadata")]
        public void Delete(int id)
        {
        }
    }
}