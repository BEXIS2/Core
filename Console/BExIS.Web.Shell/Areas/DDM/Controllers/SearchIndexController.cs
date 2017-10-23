using BExIS.Ddm.Api;
using BExIS.Utils.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using Vaiona.IoC;

namespace BExIS.Modules.Ddm.UI.Controllers
{

    /// <summary>
    /// This Search Index API is only for internal comminication
    /// 
    /// </summary>
    public class SearchIndexController : Controller
    {
        // GET: api/SearchIndex
        /// <summary>
        /// Reindex full search
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ReIndex()
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
            provider?.Reload();

            return null;
        }

        public IEnumerable<string> ReIndexSingle(long id = 0)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

            if (id == 0)
            {
                provider?.Reload();
            }
            else
            {
                provider?.UpdateSingleDatasetIndex(id, IndexingAction.CREATE);

            }
            return null;
        }


        /// <summary>
        /// free text search over the index
        /// </summary>
        /// <param name="value">search value</param>
        /// <returns></returns>
        public SearchModel Get(string value)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
            return provider?.GetTextBoxSearchValues(value, "", "new", 10);
        }
    }

}
