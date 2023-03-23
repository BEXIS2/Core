using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BExIS.Modules.Bam.UI.Controllers.API
{
    //[BExISApiAuthorize]
    [Route("api")]
    public class PartiesController : ApiController
    {
        [HttpGet]
        [ActionName("parties")]
        public IHttpActionResult Get(long partyId)
        {
            try
            {
                using (var partyManager = new PartyManager())
                {
                    var party = partyManager.GetParty(partyId);

                    if (party == null)
                        return BadRequest($"party with id: {partyId} does not exist.");

                    return Ok(ReadPartyModel.Convert(party));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
