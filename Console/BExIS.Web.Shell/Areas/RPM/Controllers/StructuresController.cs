using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BExIS.Web.Shell.Areas.RPM.Models;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class StructuresController : ApiController
    {
        // GET: api/Structures
        public IEnumerable<string> Get()
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // GET: api/Structures/5
        public Structure Get(int id)
        {
            // The model object, Structure, can not have access to the services, or data
            return new Structure(id);
        }

        // POST: api/Structures
        public void Post([FromBody]string value)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // PUT: api/Structures/5
        public void Put(int id, [FromBody]string value)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // DELETE: api/Structures/5
        public void Delete(int id)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}
