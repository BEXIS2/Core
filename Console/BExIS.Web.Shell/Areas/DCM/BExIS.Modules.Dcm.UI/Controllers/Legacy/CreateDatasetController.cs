using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Modules.Dcm.UI.Models.CreateDataset;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Extensions;
using BExIS.Xml.Helpers;
using NHibernate.Cfg.MappingSchema;
using BEXIS.JSON.Helpers;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Entities.Common;
using Vaiona.Logging;
using Vaiona.Persistence.Api;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class CreateDatasetController : BaseController
    {
        private CreateTaskmanager TaskManager;
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();


        #region Submit And Create And Finish And Cancel and Reset

        public JsonResult Submit(bool valid, string commitMessage, long entityId)
        {
            try
            {
                // create and submit Dataset

                long datasetId = SubmitDataset(entityId, valid, "Dataset", commitMessage);

                return Json(new { result = "redirect", url = Url.Action("Show", "Data", new { area = "DDM", id = datasetId }) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Submit a Dataset based on the imformations
        /// in the CreateTaskManager
        /// </summary>
        public long SubmitDataset(long entityId, bool valid, string entityname, string commitMessage = "")
        {
            #region create dataset

            // the entityname can be wrong due to the mixed use from different modules. If its an update its set explicite again in #setEntitynNameNew

            using (DatasetManager dm = new DatasetManager())
            using (DataStructureManager dsm = new DataStructureManager())
            using (ResearchPlanManager rpm = new ResearchPlanManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (EntityTemplateManager entityTemplateManager = new EntityTemplateManager())
            using (MetadataStructureManager msm = new MetadataStructureManager())
            {
                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            
                string title = "";
                long datasetId = 0;
                bool newDataset = true;

                try
                {
                    TaskManager = FormHelper.GetTaskManager(entityId);

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
                    {
   
                        long metadataStructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID]);


                        // for e new dataset
                        if (!TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
                        {
                            long datastructureId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.DATASTRUCTURE_ID]);
                            long researchPlanId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.RESEARCHPLAN_ID]);

                            DataStructure dataStructure = dsm.StructuredDataStructureRepo.Get(datastructureId);
                            //if datastructure is not a structured one
                            if (dataStructure == null) dataStructure = dsm.UnStructuredDataStructureRepo.Get(datastructureId);

                            ResearchPlan rp = rpm.Repo.Get(researchPlanId);

                            MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                            // the hole class is old code, will be removed
                            EntityTemplate entityTemplate = entityTemplateManager.Repo.Get(1);

                            var ds = dm.CreateEmptyDataset(dataStructure, rp, metadataStructure, entityTemplate);
                            datasetId = ds.Id;

                            // add security
                            if (GetUsernameOrDefault() != "DEFAULT")
                            {
                                entityPermissionManager.CreateAsync<User>(GetUsernameOrDefault(), entityname, typeof(Dataset), ds.Id, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList()).GetAwaiter().GetResult();
                            }
                        }
                        // update existing dataset
                        else
                        {
                            datasetId = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
                            entityname = xmlDatasetHelper.GetEntityName(datasetId); // ensure the correct entityname is used #setEntitynNameNew
                            newDataset = false;
                        }

     

                        if (dm.IsDatasetCheckedOutFor(datasetId, GetUsernameOrDefault()) || dm.CheckOutDataset(datasetId, GetUsernameOrDefault()))
                        {
                            DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(datasetId);

                            if (TaskManager.Bus.ContainsKey(CreateTaskmanager.METADATA_XML))
                            {


                                XDocument xMetadata = (XDocument)TaskManager.Bus[CreateTaskmanager.METADATA_XML];
                                workingCopy.Metadata = Xml.Helpers.XmlWriter.ToXmlDocument(xMetadata);

                                workingCopy.Title = xmlDatasetHelper.GetInformation(datasetId, workingCopy.Metadata, NameAttributeValues.title);
                                workingCopy.Description = xmlDatasetHelper.GetInformation(datasetId, workingCopy.Metadata, NameAttributeValues.description);

                                //check if modul exist
                                int v = 1;
                                if (workingCopy.Dataset.Versions != null && workingCopy.Dataset.Versions.Count > 1) v = workingCopy.Dataset.Versions.Count();

                                TaskManager.Bus[CreateTaskmanager.METADATA_XML] = setSystemValuesToMetadata(datasetId, v, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata, newDataset);


                                // check if metadata is valid against the metadatastructure
                                XmlMetadataConverter xmlMetadataConverter = new XmlMetadataConverter();
                                MetadataStructureConverter metadataStructureConverter = new MetadataStructureConverter();
                                // check validation status
                                var jsonSchema = metadataStructureConverter.ConvertToJsonSchema(metadataStructureId);
                                var json = xmlMetadataConverter.ConvertTo(workingCopy.Metadata);
                                valid = json.IsValid(jsonSchema);

                            }

                

                            //set status
                            workingCopy = setStateInfo(workingCopy, valid);
                            //set modifikation
                            workingCopy = setModificationInfo(workingCopy, newDataset, GetUsernameOrDefault(), "Metadata");

                            title = workingCopy.Title;
                            if (string.IsNullOrEmpty(title)) title = "No Title available.";

                            TaskManager.AddToBus(CreateTaskmanager.ENTITY_TITLE, title);//workingCopy.Metadata.SelectNodes("Metadata/Description/Description/Title/Title")[0].InnerText);
                            TaskManager.AddToBus(CreateTaskmanager.ENTITY_ID, datasetId);

                            dm.EditDatasetVersion(workingCopy, null, null, null);
                            var tagType = newDataset? TagType.None : TagType.Copy;

                            dm.CheckInDataset(datasetId, commitMessage, GetUsernameOrDefault(), ViewCreationBehavior.None, tagType);

                            #region set releationships

                            //todo check if dim is active
                            // todo call to  a function in dim
                            setRelationships(datasetId, workingCopy.Dataset.MetadataStructure.Id, workingCopy.Metadata, entityname);

                            // references

                            #endregion set releationships

                            #region set references

                            setReferences(workingCopy);

                            #endregion set references

                            if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
                            {
                                var x = this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", datasetId } });
                            }

                            LoggerFactory.LogData(datasetId.ToString(), typeof(Dataset).Name, Vaiona.Entities.Logging.CrudState.Created);

                            using(var emailService = new EmailService())
                            {
                                if (newDataset)
                                {
                                    emailService.Send(MessageHelper.GetCreateDatasetHeader(datasetId, entityname),
                                        MessageHelper.GetCreateDatasetMessage(datasetId, title, GetUsernameOrDefault(), entityname),
                                        GeneralSettings.SystemEmail
                                        );
                                }
                                else
                                {
                                    emailService.Send(MessageHelper.GetMetadataUpdatHeader(datasetId, entityname),
                                        MessageHelper.GetUpdateDatasetMessage(datasetId, title, GetUsernameOrDefault(), entityname),
                                        GeneralSettings.SystemEmail
                                        );
                                }
                            }                            
                        }

                        Session.Remove(FormHelper.GetTaskManagerKey(entityId));

                        return datasetId;
                    }
                }
                catch (Exception ex)
                {
                    using (var emailService = new EmailService())
                    {
                        emailService.Send(MessageHelper.GetMetadataUpdatHeader(datasetId, entityname),
                            ex.Message,
                            GeneralSettings.SystemEmail
                            );
                    }
                        
                    string message = String.Format("error appears by create/update dataset with id: {0} , error: {1} ", datasetId.ToString(), ex.Message);
                    LoggerFactory.LogCustom(message);
                }
            }

            #endregion create dataset

            return -1;
        }

        #region Options

        public ActionResult Cancel()
        {
            //public ActionResult LoadMetadata(long datasetId, bool locked = false, bool created = false, bool fromEditMode = false, bool resetTaskManager = false, XmlDocument newMetadata = null)

            //TaskManager = FormHelper.GetTaskManager(entityId);
            //if (TaskManager != null)
            //{
            //    DatasetManager dm = new DatasetManager();
            //    long datasetid = -1;
            //    bool resetTaskManager = true;
            //    XmlDocument metadata = null;
            //    bool editmode = false;
            //    bool created = false;

            //    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
            //    {
            //        datasetid = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
            //    }

            //    if (datasetid > -1 && dm.IsDatasetCheckedIn(datasetid))
            //    {
            //        metadata = dm.GetDatasetLatestMetadataVersion(datasetid);
            //        editmode = true;
            //        created = true;
            //    }

            //    return RedirectToAction("LoadMetadata", "Form", new { area = "Dcm", entityId = datasetid, created = created, locked = true, fromEditMode = editmode, resetTaskManager = resetTaskManager, newMetadata = metadata });
            //}

            //return RedirectToAction("StartMetadataEditor", "Form");
            return null;
        }

        public ActionResult Copy(long entityId)
        {
            TaskManager = FormHelper.GetTaskManager(entityId);
            if (TaskManager != null)
            {
                if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
                {
                    long datasetid = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);

                    return RedirectToAction("Index", "CreateDataset", new { id = datasetid, type = "DatasetId" });
                }
            }
            //Index(long id = -1, string type = "")
            return RedirectToAction("Index", "CreateDataset", new { id = -1, type = "DatasetId" });
        }

        public ActionResult Reset(long entityId)
        {
            //public ActionResult LoadMetadata(long datasetId, bool locked = false, bool created = false, bool fromEditMode = false, bool resetTaskManager = false, XmlDocument newMetadata = null)

            TaskManager = FormHelper.GetTaskManager(entityId);
            if (TaskManager != null)
            {
                using (DatasetManager dm = new DatasetManager())
                {
                    long datasetid = -1;
                    bool resetTaskManager = true;
                    XmlDocument metadata = null;
                    bool editmode = false;
                    bool created = false;

                    if (TaskManager.Bus.ContainsKey(CreateTaskmanager.ENTITY_ID))
                    {
                        datasetid = Convert.ToInt64(TaskManager.Bus[CreateTaskmanager.ENTITY_ID]);
                    }

                    if (datasetid > -1 && dm.IsDatasetCheckedIn(datasetid))
                    {
                        metadata = dm.GetDatasetLatestMetadataVersion(datasetid);
                        editmode = true;
                        created = true;
                    }

                    return RedirectToAction("LoadMetadata", "Form", new { area = "Dcm", entityId = datasetid, locked = false, created = created, fromEditMode = editmode, resetTaskManager = resetTaskManager, newMetadata = metadata });
                }
            }

            return RedirectToAction("StartMetadataEditor", "Form");
        }

        /// <summary>
        /// redirect to the DDM/Data/ShowData Action
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ShowData(long id)
        {
            return this.Run("DDM", "Data", "ShowData", new RouteValueDictionary { { "id", id } });
        }


        #endregion Options

        #endregion Submit And Create And Finish And Cancel and Reset

        #region Helper

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
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

        public List<ListViewItem> LoadDatasetViewList()
        {
            List<ListViewItem> temp = new List<ListViewItem>();

            DatasetManager datasetManager = new DatasetManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            //get all datasetsid where the current userer has access to
            UserManager userManager = new UserManager();
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

            try
            {
                List<long> datasetIds = entityPermissionManager.GetKeys(GetUsernameOrDefault(), "Dataset",
                    typeof(Dataset), RightType.Write).Result;

                List<DatasetVersion> datasetVersions = datasetManager.GetDatasetLatestVersions(datasetIds, false);
                foreach (var dsv in datasetVersions)
                {
                    string title = dsv.Title;
                    string description = dsv.Description;

                    temp.Add(new ListViewItem(dsv.Dataset.Id, title, description));
                }

                return temp.OrderBy(p => p.Title).ToList();
            }
            finally
            {
                datasetManager.Dispose();
                entityPermissionManager.Dispose();
                userManager.Dispose();
            }
        }

        public List<ListViewItemWithType> LoadDataStructureViewList()
        {
            DataStructureManager dsm = new DataStructureManager();
            try
            {
                List<ListViewItemWithType> temp = new List<ListViewItemWithType>();

                foreach (DataStructure dataStructure in dsm.AllTypesDataStructureRepo.Get())
                {
                    string title = dataStructure.Name;
                    string type = "";
                    if (dataStructure is StructuredDataStructure)
                    {
                        type = "structured";
                    }

                    if (dataStructure is UnStructuredDataStructure)
                    {
                        type = "unstructured";
                    }

                    temp.Add(new ListViewItemWithType(dataStructure.Id, title, dataStructure.Description, type));
                }

                return temp.OrderBy(p => p.Title).ToList();
            }
            finally
            {
                dsm.Dispose();
            }
        }

        public List<ListViewItem> LoadMetadataStructureViewList()
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            try
            {
                IEnumerable<MetadataStructure> metadataStructureList = metadataStructureManager.Repo.Get();

                List<ListViewItem> temp = new List<ListViewItem>();

                foreach (MetadataStructure metadataStructure in metadataStructureList)
                {
                    if (xmlDatasetHelper.IsActive(metadataStructure.Id) &&
                        xmlDatasetHelper.HasEntityType(metadataStructure.Id, "bexis.dlm.entities.data.dataset"))
                    {
                        string title = metadataStructure.Name;

                        temp.Add(new ListViewItem(metadataStructure.Id, title, metadataStructure.Description));
                    }
                }

                return temp.OrderBy(p => p.Title).ToList();
            }
            finally
            {
                metadataStructureManager.Dispose();
            }
        }

        private DataStructureType GetDataStructureType(long id)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();

            try
            {
                DataStructure dataStructure = dataStructureManager.AllTypesDataStructureRepo.Get(id);

                if (dataStructure is StructuredDataStructure)
                {
                    return DataStructureType.Structured;
                }

                if (dataStructure is UnStructuredDataStructure)
                {
                    return DataStructureType.Unstructured;
                }

                return DataStructureType.Structured;
            }
            finally
            {
                dataStructureManager.Dispose();
            }
        }

        private void setAdditionalFunctions(long entityId)
        {
            TaskManager = FormHelper.GetTaskManager(entityId);

            //set function actions of COPY, RESET,CANCEL,SUBMIT
            ActionInfo copyAction = new ActionInfo();
            copyAction.ActionName = "Copy";
            copyAction.ControllerName = "CreateDataset";
            copyAction.AreaName = "DCM";

            ActionInfo resetAction = new ActionInfo();
            resetAction.ActionName = "Reset";
            resetAction.ControllerName = "Form";
            resetAction.AreaName = "DCM";

            ActionInfo cancelAction = new ActionInfo();
            cancelAction.ActionName = "Cancel";
            cancelAction.ControllerName = "Form";
            cancelAction.AreaName = "DCM";

            ActionInfo submitAction = new ActionInfo();
            submitAction.ActionName = "Submit";
            submitAction.ControllerName = "CreateDataset";
            submitAction.AreaName = "DCM";

            TaskManager.Actions.Add(CreateTaskmanager.CANCEL_ACTION, cancelAction);
            TaskManager.Actions.Add(CreateTaskmanager.COPY_ACTION, copyAction);
            TaskManager.Actions.Add(CreateTaskmanager.RESET_ACTION, resetAction);
            TaskManager.Actions.Add(CreateTaskmanager.SUBMIT_ACTION, submitAction);
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
                                    if (mappingExist(sourceTypes,id, sourceId, relationship.Id))
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

        private XDocument setSystemValuesToMetadata(long datasetid, long version, long metadataStructureId, XmlDocument metadata, bool newDataset)
        {
            SystemMetadataHelper SystemMetadataHelper = new SystemMetadataHelper();

            Key[] myObjArray = { };

            if (newDataset) myObjArray = new Key[] { Key.Id, Key.Version, Key.DateOfVersion, Key.MetadataCreationDate, Key.MetadataLastModfied };
            else myObjArray = new Key[] { Key.Id, Key.Version, Key.DateOfVersion, Key.MetadataLastModfied };

            metadata = SystemMetadataHelper.SetSystemValuesToMetadata(datasetid, version, metadataStructureId, metadata, myObjArray);

            return XmlUtility.ToXDocument(metadata);
        }

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
                                        DateTime.Now
                                    ));
                            }
                        }
                    }
                }

                return tmp;
            }
        }

        private DatasetVersion setStateInfo(DatasetVersion workingCopy, bool valid)
        {
            //StateInfo
            if (workingCopy.StateInfo == null) workingCopy.StateInfo = new Vaiona.Entities.Common.EntityStateInfo();

            if (valid)
                workingCopy.StateInfo.State = DatasetStateInfo.Valid.ToString();
            else workingCopy.StateInfo.State = DatasetStateInfo.NotValid.ToString();

            return workingCopy;
        }

        private DatasetVersion setModificationInfo(DatasetVersion workingCopy, bool newDataset, string user, string comment)
        {
            // modifikation info
            if (workingCopy.StateInfo == null) workingCopy.ModificationInfo = new EntityAuditInfo();

            if (newDataset)
                workingCopy.ModificationInfo.ActionType = AuditActionType.Create;
            else
                workingCopy.ModificationInfo.ActionType = AuditActionType.Edit;

            //set performer
            workingCopy.ModificationInfo.Performer = string.IsNullOrEmpty(user) ? "" : user;

            //set comment
            workingCopy.ModificationInfo.Comment = string.IsNullOrEmpty(comment) ? "" : comment;

            //set time
            workingCopy.ModificationInfo.Timestamp = DateTime.Now;

            return workingCopy;
        }

        #endregion Helper
    }
}