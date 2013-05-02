using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Search.Model;

namespace BExIS.Search.Api
{
    public interface ISearchProvider
    {

        SearchModel DefaultSearchModel { get; }
        SearchModel WorkingSearchModel { get; }

        //SearchModel GetAllSearchComponents();
        //IEnumerable<Property> GetProperties();
        //IEnumerable<Category> GetCorkimategories();
        //IEnumerable<Facet> GetFacets();

        
        SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults);
        SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults, SearchCriteria searchCriteria);
        SearchModel UpdateFacets(SearchCriteria searchCriteria);
        SearchModel Get(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1);
        SearchModel SearchAndUpdate(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1);
        void SearchAndUpdate(SearchCriteria searchCriteria);
    }
}
