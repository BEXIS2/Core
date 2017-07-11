using BExIS.Ddm.Api;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Vaiona.IoC;

namespace BExIS.Modules.Ddm.UI.Controllers
{

    /// <summary>
    /// This Search Index API is only for internal comminication
    /// 
    /// </summary>
    public class SearchIndexController : ApiController
    {
        // GET: api/SearchIndex
        /// <summary>
        /// Reindex full search
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> Get()
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            provider?.Reload();

            return null;
        }

        // GET: api/SearchIndex/5
        /// <summary>
        /// id = dataset id and this get will reindex search based on the dataset
        /// </summary>
        /// <param name="id">Dataset Id</param>
        /// <returns></returns>
        /// <remarks> 
        /// </remarks>
        public HttpResponseMessage Get(int id)
        {

            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            provider?.UpdateSingleDatasetIndex(id, IndexingAction.CREATE);


            return null;
        }


    }

}
