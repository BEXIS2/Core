using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dim.UI.Controllers
{
    /// <summary>
    /// This class is designed as a Web API to allow various client tools request metadata of a datasets and get the result in XML.
    /// </summary>
    public class AttachmentOutController : ApiController
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        // GET: api/Attachment
        /// <summary>
        /// With the Get function you get an overview of the exiting datasets and there attachments.
        /// </summary>
        /// <remarks>
        /// With the Get function you get an overview of the exiting datasets and there attachments.
        /// </remarks>
        /// <returns>List of ApiDatasetAttachmentsModel</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Attachment")]
        public List<ApiDatasetAttachmentsModel> Get()
        {
            DatasetManager dm = new DatasetManager();
            List<ApiDatasetAttachmentsModel> tmp = new List<ApiDatasetAttachmentsModel>();
            try
            {
                var datasetIds = dm.GetDatasetIds();

                foreach (long id in datasetIds)
                {
                    var model = GetApiDatasetAttachmentsModel(id, dm);
                    if (model != null) tmp.Add(model);
                }

                return tmp;
            }
            finally
            {
                dm.Dispose();
            }
        }

        // GET: api/Attachment/{id}
        /// <summary>
        /// With the Get function you get an overview of one specific dataset and there attachments.
        /// </summary>
        /// <remarks>
        /// With the Get function you get an overview of one specific dataset and there attachments.
        /// </remarks>
        /// <returns>ApiDatasetAttachmentsModel</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Attachment/{id}")]
        public HttpResponseMessage Get(int id)
        {
            if (id == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset id can not be 0.");

            using (DatasetManager datasetManager = new DatasetManager())
            using (UserManager userManager = new UserManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (EntityManager entityManager = new EntityManager())
            {
                bool isPublic = false;
                User user = null;

                try
                {
                    #region is public

                    long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                    entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;

                    isPublic = entityPermissionManager.Exists(null, entityTypeId.Value, id);

                    #endregion is public

                    #region security


                    user = ControllerContext.RouteData.Values["user"] as User;

                    if (!isPublic && user == null)
                        return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Token is not valid.");

                    //check permissions

                    //entity permissions
                    if (id > 0)
                    {
                        Dataset d = datasetManager.GetDataset(id);
                        if (d == null)
                            return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset with the id (" + id + ") does not exist.");

                        if (!isPublic && !entityPermissionManager.HasEffectiveRight(user.Name, typeof(Dataset), id, RightType.Read))
                            return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "The token is not authorized to write into the dataset.");
                    }

                    #endregion security

                    var dataset = datasetManager.GetDataset(id);
                    if (dataset == null) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset with the id (" + id + ") does not exist.");

                    var model = GetApiDatasetAttachmentsModel(id, datasetManager);

                    using (var response = this.Request.CreateResponse(HttpStatusCode.OK))
                    {
                        string json = JsonConvert.SerializeObject(model);
                        response.Content = new StringContent(json, Encoding.UTF8, "application/json");

                        if (model != null) return response;

                        return null;
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }

            }
        }

        // GET: api/Metadata/5
        // HttpResponseMessage response = new HttpResponseMessage { Content = new StringContent(doc.innerXml, Encoding.UTF8,"application/xml") };

        /// <summary>
        /// This Get function has been extended by a parameter id. The id refers to the dataset. The metadata will be loaded from the dataset
        /// </summary>
        /// <param name="id">Dataset Id</param>
        /// <returns>Xml Document</returns>
        [BExISApiAuthorize]
        [GetRoute("api/Attachment/{id}/{attachmentid}")]
        public HttpResponseMessage Get(int id, long attachmentid)
        {
            if (id == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset id can not be 0.");
            if (attachmentid == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the attachment id can not be 0.");

            DatasetManager datasetManager = new DatasetManager();
            UserManager userManager = new UserManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            EntityManager entityManager = new EntityManager();

            bool isPublic = false;
            User user = null;

            try
            {
                // if a dataset is public, then the api should also return data if there is no token for a user

                #region is public

                entityManager = new EntityManager();
                long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;

                isPublic = entityPermissionManager.Exists(null, entityTypeId.Value, id);

                #endregion is public

                #region security

                user = ControllerContext.RouteData.Values["user"] as User;

                if (!isPublic && user == null)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Token is not valid.");

                //check permissions

                //entity permissions
                if (id > 0)
                {
                    Dataset d = datasetManager.GetDataset(id);
                    if (d == null)
                        return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset with the id (" + id + ") does not exist.");

                    if (!isPublic && !entityPermissionManager.HasEffectiveRight(user.Name, typeof(Dataset), id, RightType.Read))
                        return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "The token is not authorized to write into the dataset.");
                }

                #endregion security

                //check if dataset exist
                var dataset = datasetManager.GetDataset(id);
                if (dataset == null) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset with the id (" + id + ") does not exist.");

                var model = GetApiDatasetAttachmentsModel(id, datasetManager);

                //check if attachment belongs to the dataset exist
                if (!model.Attachments.Any(a => a.Id.Equals(attachmentid)))
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the attechment with the id (" + attachmentid + ") does not belong to the dataset with the id (" + id + ").");

                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(id);

                var attachment = datasetVersion.ContentDescriptors.Where(a => a.Id.Equals(attachmentid)).FirstOrDefault();

                // check if file exist or not
                string path = Path.Combine(AppConfiguration.DataPath, attachment.URI);

                //converting file into bytes array
                var dataBytes = File.ReadAllBytes(path);
                //adding bytes to memory stream
                var dataStream = new FileStream(path, FileMode.OpenOrCreate);

                HttpResponseMessage httpResponseMessage;

                httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                httpResponseMessage.Content = new StreamContent(dataStream);
                httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = attachment.Name;
                httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(attachment.MimeType);

                return httpResponseMessage;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                datasetManager.Dispose();
                userManager.Dispose();
                entityPermissionManager.Dispose();
                entityManager.Dispose();
            }
        }

        private ApiDatasetAttachmentsModel GetApiDatasetAttachmentsModel(long id, DatasetManager dm)
        {
            ApiDatasetAttachmentsModel model = new ApiDatasetAttachmentsModel();
            DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(id);

            if (datasetVersion != null && datasetVersion.ContentDescriptors != null)
            {
                model.DatasetId = id;

                foreach (var cd in datasetVersion.ContentDescriptors)
                {
                    if (cd != null && cd.URI.ToLower().Contains("attachments"))
                    {
                        ApiSimpleAttachmentModel attachmentModel = new ApiSimpleAttachmentModel();
                        attachmentModel.Id = cd.Id;
                        attachmentModel.Name = cd.Name;
                        attachmentModel.MimeType = cd.MimeType;
                        model.Attachments.Add(attachmentModel);
                    }
                }

                return model;
            }

            return null;
        }
    }
}