using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BExIS.Modules.Bam.UI.Controllers.API
{
    public class PartyRelationshipsController : ApiController
    {
        [HttpGet, Route("api/partyRelationships")]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                using (var partyRelationshipManager = new PartyRelationshipManager())
                {
                    var partyRelationships = partyRelationshipManager.PartyRelationships.Select(p => ReadPartyRelationshipModel.Convert(p)).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, partyRelationships);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, Route("api/partyRelationships/{partyRelationshipId}")]
        public async Task<HttpResponseMessage> GetById(long partyRelationshipId)
        {
            try
            {
                using (var partyRelationshipManager = new PartyRelationshipManager())
                {
                    var partyRelationship = partyRelationshipManager.FindById(partyRelationshipId);

                    if (partyRelationship == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"partyrelationship with id: {partyRelationshipId} does not exist.");

                    return Request.CreateResponse(HttpStatusCode.OK, ReadPartyRelationshipModel.Convert(partyRelationship));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}
