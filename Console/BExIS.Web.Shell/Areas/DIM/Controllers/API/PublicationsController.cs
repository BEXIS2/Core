using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Services.Publications;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BExIS.Modules.Dim.UI.Controllers.API
{
    public class PublicationsController : ApiController
    {
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

        [HttpGet, GetRoute("api/publications")]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                using (var publicationManager = new PublicationManager())
                {
                    var publications = publicationManager.Publications.Select(p => ReadPublicationModel.Convert(p)).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
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
                    switch(operation.ToLower())
                    {
                        case "download":

                            break;

                        case "delete":
                            publicationManager.DeleteById(publicationId);
                            break;

                        default:
                            break;
                    }

                    // specific actions/operations
                    switch (publication.Broker.Name.ToLower())
                    {
                        case "gbif":
                            break;

                        case "gfbio":
                            break;

                        case "pensoft":
                            break;

                        case "vaelastrasz":
                            break;

                        default:
                            break;
                    }

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