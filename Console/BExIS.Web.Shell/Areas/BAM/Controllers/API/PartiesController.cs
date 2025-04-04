using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Models;
using BExIS.Utils.Route;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BExIS.Modules.Bam.UI.Controllers.API
{
    //[BExISApiAuthorize]
    public class PartiesController : ApiController
    {
        [HttpGet, GetRoute("api/parties/{partyId}")]
        public async Task<HttpResponseMessage> GetByIdAsync(long partyId)
        {
            try
            {
                using (var partyManager = new PartyManager())
                {
                    var party = partyManager.GetParty(partyId);

                    if (party == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"party with id: {partyId} does not exist.");

                    return Request.CreateResponse(HttpStatusCode.OK, ReadPartyModel.Convert(party));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}