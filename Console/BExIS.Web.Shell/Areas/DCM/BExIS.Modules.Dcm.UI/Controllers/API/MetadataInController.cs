using BExIS.App.Bootstrap.Attributes;
using BExIS.Ddm.Api;
using BExIS.Ddm.Providers.LuceneProvider;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Services.Party;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Helpers;
using BExIS.Utils.Extensions;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using BEXIS.JSON.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Entities.Common;
using Vaiona.IoC;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;
using BExIS.Modules.Dcm.UI.Helpers;

namespace BExIS.Modules.Dim.UI.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request metadata of a datasets and get the result in XML.
    /// </summary>
    public class MetadataInController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        private readonly ISearchProvider _provider;

        public MetadataInController(SearchProvider searchProvider)
        {
            _provider = searchProvider;
        }

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
            string comment = "Update via API";

            DatasetManager datasetManager = new DatasetManager();
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

                                var commentProperty = metadataJson.Property("@comment");
                                if (commentProperty != null && commentProperty.Value != null && commentProperty.Value.ToString().Length > 0)
                                {
                                    comment = commentProperty.Value.ToString();
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


                        int vNr = datasetManager.GetDatasetVersionCount(workingCopy.Id) + 1;
                        AuditActionType t = vNr == 1 ? AuditActionType.Create : AuditActionType.Edit;

                        ////set modification
                        workingCopy.ModificationInfo = new EntityAuditInfo()
                        {
                            Performer = user.UserName,
                            Comment = "Metadata",
                            ActionType = t,
                            Timestamp = DateTime.Now
                        };
                        double tag = workingCopy.Tag != null ? workingCopy.Tag.Nr : 0;

                        // set system values
                        setSystemValuesToMetadata(workingCopy.Id, datasetManager.GetDatasetVersionCount(workingCopy.Id) + 1, tag, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata);

                        datasetManager.EditDatasetVersion(workingCopy, null, null, null);
                        datasetManager.CheckInDataset(id, comment, user.Name, ViewCreationBehavior.None);

                        #region set releationships

                        //todo check if dim is active
                        // todo call to  a function in dim
                        setRelationships(id, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata, workingCopy.Dataset.EntityTemplate.EntityType.Name);

                        // references

                        #endregion set releationships

                        #region set references

                        setReferences(workingCopy);

                        #endregion set references

                        //update search
                        var useTags = (bool)ModuleManager.GetModuleSettings("DDM").GetValueByKey("use_tags");
                        await reindex(id, useTags);
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
                request.Dispose();
            }
        }

        //toDo this function to DIM or BAM ??
        /// <summary>
        /// this function is parsing the xmldocument to
        /// create releationships based on releationshiptypes between datasets and person parties
        /// </summary>
        /// <param name="datasetid"></param>
        /// <param name="metadataStructureId"></param>
        /// <param name="metadata"></param>
        private void setRelationships(long datasetid, long metadataStructureId, XmlDocument metadata, string entityname)
        {
            using (PartyManager partyManager = new PartyManager())
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            using (PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager())
            {
                try
                {
                    using (var uow = this.GetUnitOfWork())
                    {
                        //check if mappings exist between system/relationships and the metadatastructure/attr
                        // get all party mapped nodes
                        IEnumerable<XElement> complexElements = XmlUtility.GetXElementsByAttribute("partyid", XmlUtility.ToXDocument(metadata));

                        // get all relationshipTypes where entityname is involved
                        var relationshipTypes = uow.GetReadOnlyRepository<PartyRelationshipType>().Get().Where(
                            p => p.AssociatedPairs.Any(
                                ap => ap.SourcePartyType.Title.ToLower().Equals(entityname.ToLower()) || ap.TargetPartyType.Title.ToLower().Equals(entityname.ToLower())
                                ));

                        #region delete relationships

                        foreach (var relationshipType in relationshipTypes)
                        {
                            // go through each associated realtionship type pair (e.g. Person - Dataset, Person - Publication)
                            foreach (var partyTpePair in relationshipType.AssociatedPairs)
                            {
                                // check if entityname is source or target and delete all found party realationships
                                if (partyTpePair.SourcePartyType.Title.ToLower().Equals(entityname.ToLower()))
                                {
                                    IEnumerable<PartyRelationship> relationships = uow.GetReadOnlyRepository<PartyRelationship>().Get().Where(
                                            r =>
                                            r.SourceParty != null && r.SourceParty.Name.Equals(datasetid.ToString()) &&
                                            r.PartyTypePair != null && r.PartyTypePair.Id.Equals(partyTpePair.Id)
                                        );

                                    IEnumerable<long> partyids = complexElements.Select(i => Convert.ToInt64(i.Attribute("partyid").Value));

                                    foreach (PartyRelationship pr in relationships)
                                    {
                                        partyManager.RemovePartyRelationship(pr);
                                    }
                                }
                                else if (partyTpePair.TargetPartyType.Title.ToLower().Equals(entityname.ToLower()))
                                {
                                    IEnumerable<PartyRelationship> relationships = uow.GetReadOnlyRepository<PartyRelationship>().Get().Where(
                                            r =>
                                            r.TargetParty != null && r.TargetParty.Name.Equals(datasetid.ToString()) &&
                                            r.PartyTypePair != null && r.PartyTypePair.Id.Equals(partyTpePair.Id)
                                        );

                                    IEnumerable<long> partyids = complexElements.Select(i => Convert.ToInt64(i.Attribute("partyid").Value));

                                    foreach (PartyRelationship pr in relationships)
                                    {
                                        partyManager.RemovePartyRelationship(pr);
                                    }
                                }
                            }
                        }

                        #endregion delete relationships

                        #region add relationship

                        foreach (XElement item in complexElements)
                        {
                            if (item.HasAttributes)
                            {
                                long sourceId = Convert.ToInt64(item.Attribute("roleId").Value);
                                long id = Convert.ToInt64(item.Attribute("id").Value);
                                string type = item.Attribute("type").Value;
                                long partyid = Convert.ToInt64(item.Attribute("partyid").Value);

                                LinkElementType sourceType = LinkElementType.MetadataNestedAttributeUsage;

                                List<LinkElementType> sourceTypes = new List<LinkElementType>();


                                if (type.Equals("MetadataPackageUsage")) sourceTypes.Add(LinkElementType.MetadataPackageUsage);
                                if (type.Equals("MetadataPackage")) sourceTypes.Add(LinkElementType.MetadataPackage);
                                if (type.Equals("MetadataAttributeUsage"))
                                {
                                    sourceTypes.Add(LinkElementType.MetadataAttributeUsage);
                                    sourceTypes.Add(LinkElementType.MetadataNestedAttributeUsage);
                                }

                                if (type.Equals("MetadataAttribute"))
                                {
                                    sourceTypes.Add(LinkElementType.MetadataAttributeUsage);
                                    sourceTypes.Add(LinkElementType.MetadataNestedAttributeUsage);
                                    sourceTypes.Add(LinkElementType.SimpleMetadataAttribute);
                                    sourceTypes.Add(LinkElementType.ComplexMetadataAttribute);
                                }


                                foreach (var relationship in relationshipTypes)
                                {
                                    // when mapping in both directions are exist
                                    if (mappingExist(sourceTypes, id, sourceId, relationship.Id))
                                    {
                                        // create releationship

                                        // create a Party for the dataset
                                        var customAttributes = new Dictionary<String, String>();
                                        customAttributes.Add("Name", datasetid.ToString());
                                        customAttributes.Add("Id", datasetid.ToString());

                                        // get or create datasetParty if not exists
                                        Party datasetParty = partyManager.GetPartyByCustomAttributeValues(partyTypeManager.PartyTypeRepository.Get(cc => cc.Title == entityname).First(), customAttributes).FirstOrDefault();
                                        if (datasetParty == null) datasetParty = partyManager.Create(partyTypeManager.PartyTypeRepository.Get(cc => cc.Title == entityname).First(), "[description]", null, null, customAttributes);

                                        // get user party
                                        var person = partyManager.GetParty(partyid);

                                        // add party relationships
                                        foreach (var partyTpePair in relationship.AssociatedPairs)
                                        {
                                            if (partyTpePair.SourcePartyType.Title.ToLower().Equals(entityname.ToLower()) || partyTpePair.TargetPartyType.Title.ToLower().Equals(entityname.ToLower()))
                                            {
                                                if (partyTpePair != null && person != null && datasetParty != null)
                                                {
                                                    if (!uow.GetReadOnlyRepository<PartyRelationship>().Get().Any(
                                                        r =>
                                                        r.SourceParty != null && r.SourceParty.Id.Equals(person.Id) &&
                                                        r.PartyTypePair != null && r.PartyTypePair.Id.Equals(partyTpePair.Id) &&
                                                        r.TargetParty.Id.Equals(datasetParty.Id)
                                                    ))
                                                    {
                                                        partyManager.AddPartyRelationship(
                                                            person.Id,
                                                            datasetParty.Id,
                                                            relationship.Title,
                                                            "",
                                                            partyTpePair.Id

                                                            );
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        #endregion add relationship
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private bool mappingExist(List<LinkElementType> list, long usageId, long typeId, long releationshipId)
        {

            foreach (var sourceType in list)
            {
                if (MappingUtils.ExistMappings(usageId, sourceType, releationshipId, LinkElementType.PartyRelationshipType) && MappingUtils.ExistMappings(releationshipId, LinkElementType.PartyRelationshipType, usageId, sourceType))
                    return true;
                if (MappingUtils.ExistMappings(typeId, sourceType, releationshipId, LinkElementType.PartyRelationshipType) && MappingUtils.ExistMappings(releationshipId, LinkElementType.PartyRelationshipType, typeId, sourceType))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Set References for the dataset version. This function is parsing the xmldocument to create references based on the entity reference types between datasets and other entities.
        /// </summary>
        /// <param name="datasetVersion"></param>
        private void setReferences(DatasetVersion datasetVersion)
        {
            using (EntityReferenceManager entityReferenceManager = new EntityReferenceManager())
            using (EntityManager entityManager = new EntityManager())
            {
                EntityReferenceHelper helper = new EntityReferenceHelper();
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                if (datasetVersion != null)
                {
                    List<EntityReference> refs = getAllMetadataReferences(datasetVersion);

                    foreach (var singleRef in refs)
                    {
                        if (!entityReferenceManager.Exist(singleRef, true, true))
                            entityReferenceManager.Create(singleRef);
                    }
                }
            }
        }

        private List<EntityReference> getAllMetadataReferences(DatasetVersion datasetVersion)
        {
            using (DatasetManager datasetManager = new DatasetManager())
            using (EntityManager entityManager = new EntityManager())
            using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
            {
                List<EntityReference> tmp = new List<EntityReference>();
                EntityReferenceHelper helper = new EntityReferenceHelper();
                MappingUtils mappingUtils = new MappingUtils();
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

                long id = 0;
                long typeid = 0;
                int version = 0;

                if (datasetVersion != null)
                {
                    long metadataStrutcureId = datasetVersion.Dataset.MetadataStructure.Id;

                    //get entity type like dataset or sample
                    string entityName = xmlDatasetHelper.GetEntityNameFromMetadatStructure(metadataStrutcureId, metadataStructureManager);
                    Entity entityType = entityManager.Entities.Where(e => e.Name.Equals(entityName)).FirstOrDefault();

                    //get id of the entity type
                    id = datasetVersion.Dataset.Id;
                    typeid = entityType.Id;
                    version = datasetVersion.Dataset.Versions.Count();

                    // if mapping to entites type exist
                    if (MappingUtils.ExistMappingWithEntityFromRoot(
                        datasetVersion.Dataset.MetadataStructure.Id,
                        BExIS.Dim.Entities.Mappings.LinkElementType.MetadataStructure,
                        typeid))
                    {
                        //load metadata and searching for the entity Attrs
                        XDocument metadata = XmlUtility.ToXDocument(datasetVersion.Metadata);
                        IEnumerable<XElement> xelements = XmlUtility.GetXElementsByAttribute(EntityReferenceXmlAttribute.entityid.ToString(), metadata);

                        foreach (XElement e in xelements)
                        {
                            //get attributes from xml node
                            long xId = 0;
                            int xVersion = 0;
                            long xTypeId = 0;

                            if (Int64.TryParse(e.Attribute(EntityReferenceXmlAttribute.entityid.ToString()).Value.ToString(), out xId) &&
                                Int32.TryParse(e.Attribute(EntityReferenceXmlAttribute.entityversion.ToString()).Value.ToString(), out xVersion) &&
                                Int64.TryParse(e.Attribute(EntityReferenceXmlAttribute.entitytype.ToString()).Value.ToString(), out xTypeId)
                                )
                            {
                                //entityName = xmlDatasetHelper.GetEntityNameFromMetadatStructure(metadataStrutcureId, new Dlm.Services.MetadataStructure.MetadataStructureManager());
                                //entityType = entityManager.Entities.Where(e => e.Name.Equals(entityName)).FirstOrDefault();
                                string xpath = e.GetAbsoluteXPath();

                                tmp.Add(new EntityReference(
                                        id,
                                        typeid,
                                        version,
                                        xId,
                                        xTypeId,
                                        xVersion,
                                        xpath,
                                        DefaultEntitiyReferenceType.MetadataLink.GetDisplayName(),
                                        DateTime.Now,
                                        "",
                                        ""
                                    ));
                            }
                        }
                    }
                }

                return tmp;
            }
        }

        /// <summary>
        /// Reindex the dataset in the search provider. This function is calling the search provider to update the index for the dataset with the given id.
        /// The useTags parameter indicates whether to use tags or not.
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="useTags"></param>
        /// <returns></returns>
        private async Task<bool> reindex(long datasetId, bool useTags)
        {

            // reindex
            _provider?.UpdateSingleDatasetIndex(datasetId, (IndexingAction)Enum.Parse(typeof(IndexingAction), "CREATE"), useTags);

            return true;

        }


        private XmlDocument setSystemValuesToMetadata(long datasetid, long version, double tag, long metadataStructureId, XmlDocument metadata)
        {
            SystemMetadataHelper systemMetadataHelper = new SystemMetadataHelper();

            Key[] myObjArray = { };

            myObjArray = new Key[] { Key.Id, Key.Version, Key.Tag, Key.DateOfVersion, Key.DataLastModified };


            metadata = systemMetadataHelper.SetSystemValuesToMetadata(datasetid, version, tag, metadataStructureId, metadata, myObjArray);

            return metadata;
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