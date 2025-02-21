using BExIS.Utils.Models;
using System.Collections.Generic;

/// <summary>
///
/// </summary>
namespace BExIS.Ddm.Api
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public interface ISearchProvider
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        SearchModel DefaultSearchModel { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        SearchModel WorkingSearchModel { get; }

        //SearchModel GetAllSearchComponents();
        //IEnumerable<Property> GetProperties();
        //IEnumerable<Category> GetCorkimategories();
        //IEnumerable<Facet> GetFacets();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <param name="filter"></param>
        /// <param name="searchType"></param>
        /// <param name="numberOfResults"></param>
        /// <returns></returns>
        SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="value"></param>
        /// <param name="filter"></param>
        /// <param name="searchType"></param>
        /// <param name="numberOfResults"></param>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults, SearchCriteria searchCriteria);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        SearchModel UpdateFacets(SearchCriteria searchCriteria);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        SearchModel UpdateProperties(SearchCriteria searchCriteria);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="searchCriteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="currentPage"></param>
        /// <returns></returns>
        SearchModel Get(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="searchCriteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="currentPage"></param>
        /// <returns></returns>
        SearchModel SearchAndUpdate(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="searchCriteria"></param>
        void SearchAndUpdate(SearchCriteria searchCriteria);

        /// <summary>
        /// Update a list of datasets to the index
        /// </summary>
        /// <param name="datasetsToIndex"></param>
        void UpdateIndex(Dictionary<long, IndexingAction> datasetsToIndex, bool onlyReleasedTags);

        /// <summary>
        /// Update single Dataset to to the index
        /// </summary>
        /// <param name="datasetId"></param>
        /// <param name="indAction"></param>
        void UpdateSingleDatasetIndex(long datasetId, IndexingAction indAction, bool onlyReleasedTags);

        void Reload();
    }
}