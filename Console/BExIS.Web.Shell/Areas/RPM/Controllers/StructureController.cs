using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BExIS.Web.Shell.Areas.RPM.Models;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class StructureController : ApiController
    {
        // GET: api/Structure
        public IEnumerable<string> Get()
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // GET: api/Structure/5
        public Structure Get(int id)
        {
            return new Structure(id);
        }

        // POST: api/Structure
        public void Post([FromBody]string value)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // PUT: api/Structure/5
        public void Put(int id, [FromBody]string value)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // DELETE: api/Structure/5
        public void Delete(int id)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}
