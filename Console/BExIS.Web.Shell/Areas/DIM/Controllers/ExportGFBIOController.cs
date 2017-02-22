using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Output;
using System.Collections.Generic;
using System.Web.Http;

namespace BExIS.Web.Shell.Areas.DIM.Controllers
{
    public class ExportGFBIOController : ApiController
    {
        /// <summary>
        /// Show all Datasetids with export functions
        /// </summary>
        /// <returns></returns>
        // GET: api/Export
        public IEnumerable<long> Get()
        {
            DatasetManager dm = new DatasetManager();
            var datasetIds = dm.GetDatasetLatestIds();

            return datasetIds;
        }

        // GET: api/Export/5
        public GFBIODataCenterFormularObject Get(int id)
        {
            return OutputDatasetManager.GetGFBIODataCenterFormularObject(id);
        }

        // POST: api/Export
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Export/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Export/5
        public void Delete(int id)
        {
        }
    }
}
