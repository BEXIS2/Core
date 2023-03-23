using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Models;
using System;
using System.Linq;
using System.Web.Http;
using Telerik.Web.Mvc.Extensions;

namespace BExIS.Modules.Bam.UI.Controllers.API
{
    //[BExISApiAuthorize]
    [Route("api")]
    public class PartyTypesController : ApiController
    {
        [HttpGet]
        [ActionName("partytypes")]
        public IHttpActionResult Get()
        {
            try
            {
                using (var partyTypeManager = new PartyTypeManager())
                {
                    var partyTypes = partyTypeManager.PartyTypes.Select(p => ReadPartyTypeModel.Convert(p)).ToList();

                    return Ok(partyTypes);
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [ActionName("partytypes")]
        public IHttpActionResult Get(long id)
        {
            try
            {
                using (var partyTypeManager = new PartyTypeManager())
                {
                    var partyType = partyTypeManager.FindById(id);

                    if (partyType == null)
                        return BadRequest($"partytype with id: {id} does not exist.");

                    return Ok(ReadPartyTypeModel.Convert(partyType));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [ActionName("partytypes")]
        public IHttpActionResult Get(string name)
        {
            try
            {
                using (var partyTypeManager = new PartyTypeManager())
                {
                    var partyType = partyTypeManager.FindByName(name);

                    if (partyType == null)
                        return BadRequest($"partytype with name: {name} does not exist.");

                    return Ok(ReadPartyTypeModel.Convert(partyType));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
