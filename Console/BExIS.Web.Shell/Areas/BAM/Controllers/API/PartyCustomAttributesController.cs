using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Party;
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
    public class PartyCustomAttributesController : ApiController
    {
        //[BExISApiAuthorize]
        [Route("api")]
        public class PartyTypesController : ApiController
        {
            [HttpGet]
            [ActionName("PartyCustomAttributes")]
            public IHttpActionResult Get(string partyTypeName)
            {
                try
                {
                    using (var partyTypeManager = new PartyTypeManager())
                    {
                        var partyType = partyTypeManager.FindByName(partyTypeName);

                        if (partyType == null)
                            return BadRequest($"partytype with name: {partyTypeName} does not exist.");

                        IEnumerable<ReadPartyCustomAttributeModel> customAttributes = partyType.CustomAttributes.Select(c => ReadPartyCustomAttributeModel.Convert(c));
                        return Ok(customAttributes);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            [HttpGet]
            [ActionName("PartyCustomAttributes")]
            public IHttpActionResult Get(long partyTypeId)
            {
                try
                {
                    using (var partyTypeManager = new PartyTypeManager())
                    {
                        var partyType = partyTypeManager.FindById(partyTypeId);

                        if (partyType == null)
                            return BadRequest($"partytype with id: {partyTypeId} does not exist.");

                        IEnumerable<ReadPartyCustomAttributeModel> customAttributes = partyType.CustomAttributes.Select(c => ReadPartyCustomAttributeModel.Convert(c));
                        return Ok(customAttributes);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
    }
}
