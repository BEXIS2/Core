using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Utils.Route;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml;
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
                var datasetIds = dm.GetDatasetLatestIds();

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
            if (id == 0) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset with the id (" + id + ") does not exist.");

            DatasetManager dm = new DatasetManager();
            try
            {
                var dataset = dm.GetDataset(id);
                if (dataset == null) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset with the id (" + id + ") does not exist.");

                var model = GetApiDatasetAttachmentsModel(id, dm);

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                string json = JsonConvert.SerializeObject(model);
                response.Content = new StringContent(json, Encoding.UTF8, "application/json");

                if (model != null) return response;

                return null;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                dm.Dispose();
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

            DatasetManager dm = new DatasetManager();

            try
            {
                //check if dataset exist
                var dataset = dm.GetDataset(id);
                if (dataset == null) return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the dataset with the id (" + id + ") does not exist.");

                var model = GetApiDatasetAttachmentsModel(id, dm);

                //check if attachment belongs to the dataset exist
                if (!model.Attachments.Any(a => a.Id.Equals(attachmentid)))
                    return Request.CreateErrorResponse(HttpStatusCode.PreconditionFailed, "the attechment with the id (" + attachmentid + ") does not belong to the dataset with the id (" + id + ").");

                DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(id);

                var attachment = datasetVersion.ContentDescriptors.Where(a => a.Id.Equals(attachmentid)).FirstOrDefault();

                // check if file exist or not
                string path = Path.Combine(AppConfiguration.DataPath, attachment.URI);

                //converting file into bytes array
                var dataBytes = File.ReadAllBytes(path);
                //adding bytes to memory stream
                var dataStream = new MemoryStream(dataBytes);

                HttpResponseMessage httpResponseMessage;

                httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
                httpResponseMessage.Content = new StreamContent(dataStream);
                httpResponseMessage.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = attachment.Name;
                httpResponseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(attachment.MimeType);

                return httpResponseMessage;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            finally
            {
                dm.Dispose();
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