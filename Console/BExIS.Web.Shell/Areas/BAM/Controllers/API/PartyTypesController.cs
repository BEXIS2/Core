using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Models;
using BExIS.Utils.Route;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Telerik.Web.Mvc.Extensions;

namespace BExIS.Modules.Bam.UI.Controllers.API
{
    //[BExISApiAuthorize]
    [Route("api")]
    public class PartyTypesController : ApiController
    {
        [HttpGet, GetRoute("api/partyTypes")]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                using (var partyTypeManager = new PartyTypeManager())
                {
                    var partyTypes = partyTypeManager.PartyTypes.Select(p => ReadPartyTypeModel.Convert(p)).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, partyTypes);
                }
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/partyTypes/{partyTypeId}")]
        public async Task<HttpResponseMessage> Get(long partyTypeId)
        {
            try
            {
                using (var partyTypeManager = new PartyTypeManager())
                {
                    var partyType = partyTypeManager.FindById(partyTypeId);

                    if (partyType == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"partytype with id: {partyTypeId} does not exist.");
                    
                    return Request.CreateResponse(HttpStatusCode.OK, ReadPartyTypeModel.Convert(partyType));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/partyTypes/{partyTypeName}")]
        public async Task<HttpResponseMessage> Get(string partyTypeName)
        {
            try
            {
                using (var partyTypeManager = new PartyTypeManager())
                {
                    var partyType = partyTypeManager.FindByName(partyTypeName);

                    if (partyType == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"partytype with name: {partyTypeName} does not exist.");

                    return Request.CreateResponse(HttpStatusCode.OK, ReadPartyTypeModel.Convert(partyType));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
