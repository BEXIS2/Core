using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Models.Create;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Helpers;
using BExIS.Utils.Config;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml.XPath;
using Vaiona.Entities.Common;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class CreateController : Controller
    {
        // GET: Create
        public ActionResult Index()
        {
            string pageId = "create";

            ViewData["PageId"] = pageId;
            ViewData["PageScript"] = SvelteHelper.GetPageScript("DCM", pageId);

            return View();
        }

        /// <summary>
        /// Get data based on a EntityTemplate 
        /// System keys and datatypes
        /// list of datastructures if exist
        /// List of file types
        /// </summary>
        /// <param name="id">entity template id</param>
        /// <returns></returns>
        [JsonNetFilter]
        public JsonResult Get(long id)
        {
            if (id <= 0) throw new ArgumentNullException("The id of entitytemplate does not exist.");

            CreateModel model = new CreateModel();

            using (var entityTemplateManager = new EntityTemplateManager())
            using (var datastructureManager = new DataStructureManager())
            using (var metadataAttributeManager = new MetadataAttributeManager())
            using (var mappingManager = new MappingManager())
            {

                var entityTemplate = entityTemplateManager.Repo.Get(id);

                if (entityTemplate == null) throw new ArgumentNullException("The entitytemplate with id (" + id + ") does not exist.");

                model.Id = entityTemplate.Id;
                model.Name = entityTemplate.Name;
                model.Description = entityTemplate.Description;

                model.FileTypes = entityTemplate.AllowedFileTypes;

                // get structure names
                if (entityTemplate.DatastructureList != null)
                {
                    var structures = datastructureManager.AllTypesDataStructureRepo.Query(d => entityTemplate.DatastructureList.Contains(d.Id)).Select(d => d.Name);
                    model.Datastructures = structures.ToList();
                }

                // Get Metadata Input Fields
                if (entityTemplate.MetadataFields.Any())
                {
                    foreach (var key in entityTemplate.MetadataFields)
                    {
                        var systemKey = (Key)key;
                        long metadataStructureId = entityTemplate.MetadataStructure.Id;
                        LinkElement target = null;

                        // when there is a mapping get link element
                        if (MappingUtils.HasTarget(key, metadataStructureId, out target))
                        {
                            // get Datatype of link element
                            var datatype = MappingUtils.GetDataType(target);

                            model.InputFields.Add(new MetadataInputField()
                            {
                                Index = key,
                                Name = systemKey.ToString(),
                                Type = datatype
                            });
                        }
                    }
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult Create(CreateModel data)
        {
            if (data == null) return Json(false);
            if (data.Id <= 0) throw new ArgumentException("Entity Template id should not be 0 or less.");

            long datasetId = 0;
            string title = "";
            string description = "";

            using (DatasetManager dm = new DatasetManager())
            using (DataStructureManager dsm = new DataStructureManager())
            using (ResearchPlanManager rpm = new ResearchPlanManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (MetadataStructureManager msm = new MetadataStructureManager())
            using (GroupManager gm = new GroupManager())
            using (EntityTemplateManager entityTemplateManager = new EntityTemplateManager())
            {
                // create Entity based on entity template
                // load entitytemplate
                var entityTemplate = entityTemplateManager.Repo.Get(data.Id);
                // check metadata structure
                var metadataStructure = msm.Repo.Get(entityTemplate.MetadataStructure.Id);

                StructuredDataStructure dataStructure = null;

                // check datastructrue if exist -3 cases
                // 1. no selecttion - datastrutcure stays null
                // 2. one  - set it
                // 3. more then one - dont set it - datastrutcure stays null
                if (entityTemplate.HasDatastructure)
                {
                    // when template has only one datastructure set it
                    if (entityTemplate.DatastructureList.Count == 1)
                    {
                        long datastructureId = entityTemplate.DatastructureList.FirstOrDefault();
                        dataStructure = dsm.StructuredDataStructureRepo.Get(datastructureId);
                    }
                }

                // create empty Dataset
                ResearchPlan rp = rpm.Repo.Get(1); // not used but needed

                var ds = dm.CreateEmptyDataset(dataStructure, rp, metadataStructure, entityTemplate);
                datasetId = ds.Id;

                #region prepare metadata

                // generate xml based on metadatastructure
                var xmlMetadatWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                var metadataXml = xmlMetadatWriter.CreateMetadataXml(metadataStructure.Id);

                // get values from metadata field inputs and set it into the metadata
                foreach (var item in data.InputFields)
                {
                    if (item.Name.ToLower() == "title") title = item.Value;
                    if (item.Name.ToLower() == "description") description = item.Value;

                    LinkElement target = new LinkElement();
                    // if target exist
                    if (MappingUtils.HasTarget(item.Index, metadataStructure.Id, out target))
                    {
                        // get XElement by XPath
                        if (!string.IsNullOrEmpty(target.XPath))
                        {
                            var targetXElement = metadataXml.XPathSelectElement(target.XPath);
                            // set value
                            targetXElement.Value = item.Value;
                        }
                        else // find element by Name
                        {
                            var targetXElement = XmlUtility.GetXElementByAttribute(target.Name, "id", target.ElementId.ToString(), metadataXml);

                            // may the mapping is on type, then it is the last element and the value should set there
                            if (targetXElement.Descendants().First() == null) targetXElement.Value = item.Value;
                            else
                                // if the mappings go thow a usage, the value shoudl set on the type and this element is the child of the usage
                                // e.g. mappingt to title is usage, set value to title/titleType = "title of the dataset" 
                                // set value
                                targetXElement.Descendants().First().Value = item.Value;
                        }
                    }
                }



                #endregion


                #region  update version

                if (dm.IsDatasetCheckedOutFor(datasetId, GetUsernameOrDefault()) || dm.CheckOutDataset(datasetId, GetUsernameOrDefault()))
                {
                    DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(datasetId);

                    workingCopy.Metadata = XmlUtility.ToXmlDocument(metadataXml);

                    // set title if exist
                    workingCopy.Title = title;
                    workingCopy.Description = description;

                    //set status - not valid
                    workingCopy = setStateInfo(workingCopy, false);

                    //set modification - create
                    workingCopy = setModificationInfo(workingCopy, true, GetUsernameOrDefault(), "Metadata");

                    // save version in database
                    dm.EditDatasetVersion(workingCopy, null, null, null);
                    // close check out
                    dm.CheckInDataset(datasetId, "", GetUsernameOrDefault(), ViewCreationBehavior.None);
                }


                #endregion

                #region  add permissions

                if (entityTemplate.PermissionGroups != null)
                {
                    foreach (var groupId in entityTemplate.PermissionGroups)
                    {
                        var group = gm.Groups.Where(g => g.Id.Equals(groupId)).FirstOrDefault();
                        entityPermissionManager.Create<Group>(group.Name, entityTemplate.EntityType.Name, typeof(Dataset), ds.Id, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
                    }
                }

                if (GetUsernameOrDefault() != "DEFAULT")
                {
                    entityPermissionManager.Create<User>(GetUsernameOrDefault(), entityTemplate.EntityType.Name, typeof(Dataset), ds.Id, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());
                }


                #endregion

                #region  send notifications

                List<string> destinations = new List<string>();
                // add system email
                destinations.Add(GeneralSettings.SystemEmail);

                if (entityTemplate.NotificationGroups != null)
                {
                    // add emails from groups
                    foreach (var groupId in entityTemplate.NotificationGroups)
                    {
                        var group = gm.Groups.Where(g => g.Id.Equals(groupId)).FirstOrDefault();
                        destinations.AddRange(group.Users.Select(u => u.Email));
                    }
                }
                // remove dubplicates
                destinations = destinations.Distinct().ToList();

                var es = new EmailService();
                es.Send(MessageHelper.GetCreateDatasetHeader(datasetId, entityTemplate.Name),
                    MessageHelper.GetCreateDatasetMessage(datasetId, title, GetUsernameOrDefault(), entityTemplate.Name),
                    destinations
                    );


                #endregion

            }

            return Json(new { success = true, id = datasetId });
        }

        #region helper

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

        #endregion 
    }
}
