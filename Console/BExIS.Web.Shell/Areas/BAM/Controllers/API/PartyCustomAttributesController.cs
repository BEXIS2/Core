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
    public class PartyCustomAttributesController : ApiController
    {
        [HttpGet, Route("api/partyCustomAttributes/{partyTypeId}")]
        public async Task<HttpResponseMessage> GetByPartyTypeIdAsync(long partyTypeId)
        {
            try
            {
                using (var partyTypeManager = new PartyTypeManager())
                {
                    var partyType = partyTypeManager.FindById(partyTypeId);

                    if (partyType == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"partytype with id: {partyTypeId} does not exist.");

                    IEnumerable<ReadPartyCustomAttributeModel> customAttributes = partyType.CustomAttributes.Select(c => ReadPartyCustomAttributeModel.Convert(c));
                    return Request.CreateResponse(HttpStatusCode.OK, customAttributes);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}