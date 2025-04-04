using BExIS.Utils.Route;
using NameParser;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BExIS.Web.Shell.Controllers.API
{
    /// <summary>
    ///
    /// </summary>
    public class PersonNameComponentsController : ApiController
    {
        /// <summary>
        /// Get the separate parts of a person's name.
        /// </summary>
        /// <param name="name">complete name of the person</param>
        /// <returns>separated person names (e.g. first, middle, last,...)</returns>
        [HttpPost, PostRoute("api/personNameComponents")]
        public HttpResponseMessage Post([FromBody] string name)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, new HumanName(name));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}