using BExIS.App.Bootstrap.Attributes;
using BExIS.Utils.Route;
using BExIS.Web.Shell.Models;
using NameParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BExIS.Web.Shell.Controllers.API
{
    public class NamesController : ApiController
    {
        [HttpPost, PostRoute("api/names")]
        public async Task<HttpResponseMessage> Post([FromBody]string name)
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
