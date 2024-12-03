using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.Export;
using BExIS.Dim.Helpers.Models;
using BExIS.Dim.Helpers.Vaelastrasz;
using BExIS.Dim.Services.Publications;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;


namespace BExIS.Modules.Dim.UI.Controllers.API
{
    public class PublicationsController : ApiController
    {
        [HttpGet, GetRoute("api/publications")]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                using (var publicationManager = new PublicationManager())
                {
                    var publications = publicationManager.Publications.Select(p => ReadPublicationModel.Convert(p)).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, publications);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/publications/{publicationId}")]
        public async Task<HttpResponseMessage> GetById(long publicationId)
        {
            try
            {
                using (var publicationManager = new PublicationManager())
                {
                    var publication = await publicationManager.FindByIdAsync(publicationId);

                    if (publication == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"publication with id: {publicationId} does not exist.");

                    return Request.CreateResponse(HttpStatusCode.OK, ReadPublicationModel.Convert(publication));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPut, PutRoute("api/publications/{id}/{operation}")]
        public async Task<HttpResponseMessage> OperateById(long publicationId, string operation)
        {
            try
            {
                using (var publicationManager = new PublicationManager())
                using (var brokerManager = new BrokerManager())
                {
                    var publication = await publicationManager.FindByIdAsync(publicationId);

                    if (publication == null)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "");

                    // general actions/operations
                    switch (operation.ToLower())
                    {
                        case "download":

                            Tuple<string, string> filePath = null;

                            switch (publication.Broker.Name.ToLower())
                            {
                                case "gbif":

                                    GBIFDataRepoConverter dataRepoConverter = new GBIFDataRepoConverter(publication.Broker, (GbifDataType)Enum.Parse(typeof(GbifDataType), publication.Broker.Type));
                                    filePath = new Tuple<string, string>(dataRepoConverter.Convert(publication.DatasetVersion.Id), "application/zip");
                                    break;

                                case "pangeae":

                                    break;

                                default:
                                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "");
                            }

                            if (filePath == null)
                                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "");

                            byte[] fileBytes;
                            using (var fileStream = new FileStream(filePath.Item1, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                            {
                                fileBytes = new byte[fileStream.Length];
                                await fileStream.ReadAsync(fileBytes, 0, (int)fileStream.Length);
                            }

                            var result = new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new ByteArrayContent(fileBytes)
                            };

                            // Set the content type and the content disposition for download
                            var fileName = Path.GetFileName(filePath.Item1);
                            result.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileName));
                            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = fileName
                            };

                            return result;

                        case "accept":

                            switch (publication.Broker.Name.ToLower())
                            {
                                case "vaelastrasz":

                                    //VaelastraszConverter vaelastraszConverter = new VaelastraszConverter(publication.Broker, publication.Broker.Type.ToLower());
                                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "accept");

                                default:
                                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "accept");
                            }

                        case "reject":

                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "reject");

                        default:
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost, PostRoute("api/publications")]
        public async Task<HttpResponseMessage> Post(CreatePublicationModel model)
        {
            try
            {
                using (var publicationManager = new PublicationManager())
                using (var brokerManager = new BrokerManager())
                {
                    var broker = await brokerManager.FindByIdAsync(model.BrokerId);

                    if (broker == null)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "");

                    var publication = new Publication()
                    {
                        Broker = broker,
                        Status = model.Status,
                    };

                    var result = publicationManager.Create(publication);

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPut, PutRoute("api/publications/{publicationId}")]
        public async Task<HttpResponseMessage> PutById(long publicationId, UpdatePublicationModel model)
        {
            try
            {
                using (var publicationManager = new PublicationManager())
                using (var brokerManager = new BrokerManager())
                {
                    var publication = await publicationManager.FindByIdAsync(publicationId);

                    if (publication == null)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "");

                    publicationManager.Update(publication);

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}