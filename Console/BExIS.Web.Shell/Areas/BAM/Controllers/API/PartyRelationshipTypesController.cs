using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BExIS.Modules.Bam.UI.Controllers.API
{
    public class PartyRelationshipTypesController : ApiController
    {
        [HttpGet, Route("api/partyRelationshipTypes")]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                using (var partyRelationshipTypeManager = new PartyRelationshipTypeManager())
                {
                    var partyRelationshipTypes = partyRelationshipTypeManager.PartyRelationshipTypes.Select(p => ReadPartyRelationshipTypeModel.Convert(p)).ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, partyRelationshipTypes);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, Route("api/partyRelationshipTypes/{partyRelationshipTypeId}")]
        public async Task<HttpResponseMessage> GetById(long partyRelationshipTypeId)
        {
            try
            {
                using (var partyRelationshipTypeManager = new PartyRelationshipTypeManager())
                {
                    var partyRelationshipType = partyRelationshipTypeManager.PartyRelationshipTypes.Where(p => p.Id == partyRelationshipTypeId).FirstOrDefault();

                    if (partyRelationshipType == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"partytype with id: {partyRelationshipTypeId} does not exist.");

                    return Request.CreateResponse(HttpStatusCode.OK, ReadPartyRelationshipTypeModel.Convert(partyRelationshipType));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}