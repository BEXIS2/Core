using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.IO.Transform.Output;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class StructuresController : ApiController
    {
        // GET: api/Structures
        /// <summary>
        /// Return a list of id´s from all structures
        /// </summary>
        /// <returns>list of structure id´s</returns>
        [BExISApiAuthorize]
        public IEnumerable<long> Get()
        {
            var dataStructureIds = this.GetUnitOfWork().GetReadOnlyRepository<DataStructure>().Query().Select(d => d.Id).ToList();
            return dataStructureIds;
        }

        // GET: api/Structures/5
        /// <summary>
        /// Return a structure based on the id.
        /// </summary>
        /// <param name="id">identifier of the structure</param>
        /// <returns>structure</returns>
        [BExISApiAuthorize]
        public DataStructureDataTable Get(long id)
        {
            // The model object, Structure, can not have access to the services, or data
            return new DataStructureDataTable(id);
        }

        // POST: api/Structures
        [ApiExplorerSettings(IgnoreApi = true)]
        public void Post([FromBody] string value)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // PUT: api/Structures/5
        [ApiExplorerSettings(IgnoreApi = true)]
        public void Put(int id, [FromBody] string value)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        // DELETE: api/Structures/5
        [ApiExplorerSettings(IgnoreApi = true)]
        public void Delete(int id)
        {
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
    }
}