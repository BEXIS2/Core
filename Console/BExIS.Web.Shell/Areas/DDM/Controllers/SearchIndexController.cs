using BExIS.Ddm.Api;
using BExIS.Utils.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;
using Vaiona.IoC;

namespace BExIS.Modules.Ddm.UI.Controllers
{

    /// <summary>
    /// This Search Index API is only for internal comminication
    /// 
    /// </summary>
    public class SearchIndexController: Controller
    {
        // GET: api/SearchIndex
        /// <summary>
        /// Reindex full search
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ReIndex()
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>(this.Session.SessionID) as ISearchProvider;
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
        public HttpResponseMessage Redinex(int id)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>(this.Session.SessionID) as ISearchProvider;
            provider?.UpdateSingleDatasetIndex(id, IndexingAction.CREATE);
            return null;
        }



        /// <summary>
        /// free text search over the index
        /// </summary>
        /// <param name="value">search value</param>
        /// <returns></returns>
        public SearchModel Get(string value)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>(this.Session.SessionID) as ISearchProvider;
            return provider?.GetTextBoxSearchValues(value, "", "new", 10);
        }
    }

}
