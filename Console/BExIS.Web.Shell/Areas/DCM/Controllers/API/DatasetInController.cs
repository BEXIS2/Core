﻿using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Models.API;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
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
        public HttpResponseMessage Post([FromBody]PostApiDatasetModel dataset)
        {
            var request = Request.CreateResponse();
            User user = null;
            string error = "";
            long datasetId = 0;
            long researchPlanId = 1;

            DatasetManager datasetManager = new DatasetManager();
            DataStructureManager dataStructureManager = new DataStructureManager();
            ResearchPlanManager researchPlanManager = new ResearchPlanManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

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

                MetadataStructureManager msm = new MetadataStructureManager();
                MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(dataset.MetadataStructureId);

                if (metadataStructure == null)
                {
                    request.Content = new StringContent("A metadata structure with id " + dataset.MetadataStructureId + "does not exist.");
                    return request;
                }

                var newDataset = datasetManager.CreateEmptyDataset(dataStructure, rp, metadataStructure);
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
            finally
            {
                datasetManager.Dispose();
                entityPermissionManager.Dispose();
                userManager.Dispose();
            }
        }

        // PUT api/<controller>/5
        [ApiExplorerSettings(IgnoreApi = true)]
        [BExISApiAuthorize]
        [PutRoute("api/Dataset")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [ApiExplorerSettings(IgnoreApi = true)]
        [BExISApiAuthorize]
        [DeleteRoute("api/Dataset")]
        public void Delete(int id)
        {
        }
    }
}