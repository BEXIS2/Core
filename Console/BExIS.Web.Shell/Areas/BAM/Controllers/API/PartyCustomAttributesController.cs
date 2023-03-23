using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Models;
using BExIS.Utils.Route;
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
        public class PartyTypesController : ApiController
        {
            [HttpGet, GetRoute("api/partyCustomAttributes/{partyTypeName}")]
            public async Task<HttpResponseMessage> Get(string partyTypeName)
            {
                try
                {
                    using (var partyTypeManager = new PartyTypeManager())
                    {
                        var partyType = partyTypeManager.FindByName(partyTypeName);

                        if (partyType == null)
                            return Request.CreateResponse(HttpStatusCode.BadRequest, $"partytype with name: {partyTypeName} does not exist.");

                        IEnumerable<ReadPartyCustomAttributeModel> customAttributes = partyType.CustomAttributes.Select(c => ReadPartyCustomAttributeModel.Convert(c));
                        return Request.CreateResponse(HttpStatusCode.OK, customAttributes);
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }

            [HttpGet, GetRoute("api/partyCustomAttributes/{partyTypeId}")]
            public async Task<HttpResponseMessage> Get(long partyTypeId)
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
}
