using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using BEXIS.JSON.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
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
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Metadata/5
        /// <summary>
        /// Import metadata via json or xml to a specifiy entity
        /// </summary>
        /// <remarks>
        /// In the Metadata PUT Api there are two different ways to import metadata.
        ///
        /// 1. XML
        /// Send an xml in the xml content to update the metadata, each xpath is checked and if there is a possible mapping, the fields are updated.
        ///
        /// 2. JSON
        /// In relation to the dataset with a metadatastructure, the incoming metadata as json is validated against the associated JSON schema. Only if the json is valid, the metadata is updated.
        ///
        /// </remarks>
        /// <param name="id">identifier for an specifiy entity e.g. dataset in the system </param>
        /// <exception cref="HttpStatusCode.PreconditionFailed"></exception>
        /// <exception cref="HttpStatusCode.ExpectationFailed"></exception>
        /// <exception cref="HttpStatusCode.InternalServerError"></exception>
        /// <returns>Message</returns>
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
            XmlMetadataConverter converter = new XmlMetadataConverter();
            MetadataStructureConverter metadataStructureConverter = new MetadataStructureConverter();
            JSchema schema;
            try
            {
                #region security

                user = ControllerContext.RouteData.Values["user"] as User;

                if (user == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Token is not valid.");

                //check permissions

                //entity permissions
                if (id > 0)
                {
                    Dataset d = datasetManager.GetDataset(id);
                    if (d == null)
                        return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset with the id (" + id + ") does not exist.");

                    if (!entityPermissionManager.HasEffectiveRightsAsync(user.Name, typeof(Dataset), id, RightType.Write).Result)
                        return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "The token is not authorized to write into the dataset.");
                }

                #endregion security

                #region check incomming metadata

                Stream requestStream = await this.Request.Content.ReadAsStreamAsync();

                string contentType = this.Request.Content.Headers.ContentType.MediaType;

                if (string.IsNullOrEmpty(contentType) || (!contentType.Equals("application/xml") && !contentType.Equals("application/json") && !contentType.Equals("text/plain")))
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "The transmitted file is not a xml document.");

                if (requestStream == null)
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Metadata xml was not received.");

                #endregion check incomming metadata

                #region incomming values check

                // check incomming values

                if (id == 0) error += "dataset id should be greater then 0.";

                if (!string.IsNullOrEmpty(error))
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, error);

                #endregion incomming values check

                Dataset dataset = datasetManager.GetDataset(id);
                if (dataset == null)
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "Dataset not exist.");

                #region convert metadata

                XmlDocument completeMetadata = null;

                if (contentType.Equals("application/xml"))
                {
                    #region application/xml

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
                    completeMetadata = XmlMetadataImportHelper.FillInXmlValues(metadataResult,
                        metadataXmlTemplate);

                    #endregion application/xml
                }
                else
                if (contentType.Equals("application/json"))
                {
                    #region application/json

                    using (var streamReader = new StreamReader(requestStream))
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        JsonSerializer serializer = new JsonSerializer();

                        try
                        {
                            JObject metadataJson = serializer.Deserialize<JObject>(jsonReader);

                            long mdid = 0;
                            if (metadataJson.ContainsKey("@id"))
                            {
                                if (Int64.TryParse(metadataJson.Property("@id").Value.ToString(), out mdid))
                                {
                                    schema = metadataStructureConverter.ConvertToJsonSchema(mdid);

                                    List<string> notAllowedElements = new List<string>();
                                    if (converter.HasValidStructure(metadataJson, mdid, out notAllowedElements))
                                    {
                                        completeMetadata = converter.ConvertTo(metadataJson);
;
                                    }
                                    else
                                    {
                                        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "the json does not have the expected structure");
                                    }
                                }
                                else
                                {
                                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "the json does not have the expected structure");
                                }
                            }
                            else
                            {
                                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "the json does not contain any information about the metadata structure");
                            }
                        }
                        catch (JsonReaderException)
                        {
                            Console.WriteLine("Invalid JSON.");
                        }
                    }

                    #endregion application/json
                }

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
                        var jsonSchema = metadataStructureConverter.ConvertToJsonSchema(workingCopy.Dataset.MetadataStructure.Id);
                        var json = converter.ConvertTo(workingCopy.Metadata);
                        bool valid = json.IsValid(jsonSchema);

                        //set state based on valid or not valid
                        if (workingCopy.StateInfo == null) workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo();
                        workingCopy.StateInfo.State = valid ? DatasetStateInfo.Valid.ToString() : DatasetStateInfo.NotValid.ToString();

                        title = workingCopy.Title;
                        if (string.IsNullOrEmpty(title)) title = "No Title available.";

                        datasetManager.EditDatasetVersion(workingCopy, null, null, null);
                        datasetManager.CheckInDataset(id, "via API", user.Name, ViewCreationBehavior.None);
                    }

                    LoggerFactory.LogData(id.ToString(), typeof(Dataset).Name, Vaiona.Entities.Logging.CrudState.Created);

                    using (var emailService = new EmailService())
                    {
                        emailService.Send(MessageHelper.GetMetadataUpdatHeader(id, typeof(Dataset).Name),
                                                    MessageHelper.GetUpdateDatasetMessage(id, title, user.DisplayName, typeof(Dataset).Name),
                                                    GeneralSettings.SystemEmail
                                                    );
                    }
                        
                }

                return Request.CreateErrorResponse(HttpStatusCode.OK, "Metadata successfully updated via api.");
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
                request.Dispose();
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