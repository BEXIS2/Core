using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models.API;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Entities.Common;

namespace BExIS.Modules.Dcm.UI.Controllers.API
{
    public class DatasetInController : ApiController
    {
        // POST api/ DatasetIn
        /// <summary>
        ///
        /// </summary>
        /// <example>
        ///{
        /// "Title":"Title of my Dataset.",
        /// "Description":"Description of my Dataset.",
        /// "DataStructureId":1,
        /// "MetadataStructureId":1,
        ///}
        /// </example>
        /// <param name="dataset"></param>
        /// <returns> HttpResponseMessage </returns>
        [BExISApiAuthorize]
        [PostRoute("api/Dataset")]
        public HttpResponseMessage Post([FromBody] PostApiDatasetModel dataset)
        {
            var request = Request.CreateResponse();
            User user = null;
            string error = "";
            long datasetId = 0;
            long researchPlanId = 1;

            using (DatasetManager datasetManager = new DatasetManager())
            using (DataStructureManager dataStructureManager = new DataStructureManager())
            using (ResearchPlanManager researchPlanManager = new ResearchPlanManager())
            using (UserManager userManager = new UserManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
            using (EntityTemplateManager entityTemplateManager = new EntityTemplateManager())
            {
                try
                {
                    #region security

                    user = ControllerContext.RouteData.Values["user"] as User;

                    if (user == null)
                    {
                        request.Content = new StringContent("Token is not valid.");

                        return request;
                    }

                    #endregion security

                    #region incomming values check

                    // check incomming values
                    if (dataset.Title == null) error += "title not existing.";
                    if (dataset.Description == null) error += "description not existing.";
                    if (dataset.MetadataStructureId == 0) error += "metadata structure id should not be null. ";
                    if (dataset.DataStructureId == 0) error += "datastructure id should not be null. ";

                    if (!string.IsNullOrEmpty(error))
                    {
                        request.Content = new StringContent(error);

                        return request;
                    }

                    #endregion incomming values check

                    #region create dataset

                    DataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataset.DataStructureId);
                    //if datastructure is not a structured one
                    if (dataStructure == null) dataStructure = dataStructureManager.UnStructuredDataStructureRepo.Get(dataset.DataStructureId);

                    if (dataStructure == null)
                    {
                        request.Content = new StringContent("A data structure with id " + dataset.DataStructureId + "does not exist.");
                        return request;
                    }

                    ResearchPlan rp = researchPlanManager.Repo.Get(researchPlanId);

                    if (rp == null)
                    {
                        request.Content = new StringContent("A research plan with id " + researchPlanId + "does not exist.");
                        return request;
                    }

                    MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(dataset.MetadataStructureId);

                    if (metadataStructure == null)
                    {
                        request.Content = new StringContent("A metadata structure with id " + dataset.MetadataStructureId + "does not exist.");
                        return request;
                    }

                    EntityTemplate entityTemplate = entityTemplateManager.Repo.Get(dataset.EntityTemplateId);

                    if (metadataStructure == null)
                    {
                        request.Content = new StringContent("A EntityTemplate with id " + dataset.EntityTemplateId + "does not exist.");
                        return request;
                    }

                    var newDataset = datasetManager.CreateEmptyDataset(dataStructure, rp, metadataStructure, entityTemplate);
                    datasetId = newDataset.Id;

                    // add security
                    entityPermissionManager.Create<User>(user.UserName, "Dataset", typeof(Dataset), newDataset.Id, Enum.GetValues(typeof(RightType)).Cast<RightType>().ToList());

                    //add title and description to the metadata

                    if (datasetManager.IsDatasetCheckedOutFor(datasetId, user.UserName) || datasetManager.CheckOutDataset(datasetId, user.UserName))
                    {
                        DatasetVersion workingCopy = datasetManager.GetDatasetWorkingCopy(datasetId);

                        XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                        XDocument xdoc = xmlMetadataWriter.CreateMetadataXml(dataset.MetadataStructureId);
                        workingCopy.Metadata = XmlUtility.ToXmlDocument(xdoc);
                        XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
                        workingCopy.Metadata = xmlDatasetHelper.SetInformation(workingCopy, workingCopy.Metadata, NameAttributeValues.title, dataset.Title);
                        workingCopy.Title = dataset.Title;
                        workingCopy.Metadata = xmlDatasetHelper.SetInformation(workingCopy, workingCopy.Metadata, NameAttributeValues.description, dataset.Description);
                        workingCopy.Description = dataset.Description;

                        // update metadata based on system mappings
                        workingCopy.Metadata = setSystemValuesToMetadata(datasetId, 1, dataset.MetadataStructureId, workingCopy.Metadata, true);

                        ////set modification
                        workingCopy.ModificationInfo = new EntityAuditInfo()
                        {
                            Performer = user.UserName,
                            Comment = "Metadata",
                            ActionType = AuditActionType.Create
                        };

                        datasetManager.EditDatasetVersion(workingCopy, null, null, null);
                        datasetManager.CheckInDataset(datasetId, "Title and description were added to the dataset via the api.", user.UserName, ViewCreationBehavior.None);
                    }

                    request.Content = new StringContent("the dataset " + dataset.Title + "(" + datasetId + ") was successfully created.");
                    return request;

                    #endregion create dataset
                }
                catch (Exception ex)
                {
                    request.Content = new StringContent(ex.Message);
                    return request;
                }
            }
        }

        // PUT api/<controller>/5
        [ApiExplorerSettings(IgnoreApi = true)]
        [BExISApiAuthorize]
        [PutRoute("api/Dataset")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        [ApiExplorerSettings(IgnoreApi = true)]
        [BExISApiAuthorize]
        [DeleteRoute("api/Dataset")]
        public void Delete(int id)
        {
        }

        private XmlDocument setSystemValuesToMetadata(long datasetid, long version, long metadataStructureId, XmlDocument metadata, bool newDataset)
        {
            SystemMetadataHelper SystemMetadataHelper = new SystemMetadataHelper();

            Key[] myObjArray = { };

            if (newDataset) myObjArray = new Key[] { Key.Id, Key.Version, Key.DateOfVersion, Key.MetadataCreationDate, Key.MetadataLastModfied };
            else myObjArray = new Key[] { Key.Id, Key.Version, Key.DateOfVersion, Key.MetadataLastModfied };

            metadata = SystemMetadataHelper.SetSystemValuesToMetadata(datasetid, version, metadataStructureId, metadata, myObjArray);

            return metadata;
        }
    }
}