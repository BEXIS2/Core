using System;
using System.Collections.Generic;
using BExIS.Ddm.Api;
using BExIS.Ddm.Model;
using BExIS.Ddm.Providers.DummyProvider.Helpers;
using BExIS.Ddm.Providers.DummyProvider.Helpers.Helpers.Search;

/// <summary>
///
/// </summary>        
namespace BExIS.Ddm.Providers.DummyProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class SearchProvider : ISearchProvider
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public SearchModel WorkingSearchModel { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public SearchModel DefaultSearchModel { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public SearchProvider()
        {

            this.DefaultSearchModel = initDefault();
            this.WorkingSearchModel = initWorking(); //init(WorkingSearchModel); // its better to make a clone form DefualtSearchModel than calling the function twice
            
            //this.DefaultSearchModel = Get(this.WorkingSearchModel.CriteriaComponent);
            this.WorkingSearchModel = Get(this.WorkingSearchModel.CriteriaComponent);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public SearchModel initDefault()
        {
            SearchModel model = new SearchModel();

            MetadataReader.ListOfMetadata = MetadataReader.GetAllMetadataDatasets();

            //facets
            model.SearchComponent.Facets = FacetBuilder.GetAllNodesAsListFromData(FacetBuilder.CATEGORY_TYPE, MetadataReader.ListOfMetadata);

            //properties
            PropertyBuilder propertyBuilder = new PropertyBuilder(MetadataReader.ListOfMetadata);
            model.SearchComponent.Properties = propertyBuilder.AllProperties;

            //categories
            model.SearchComponent.Categories = CategoryBuilder.GetAllRootNodesAsList(CategoryBuilder.DROPDOWN_TYPE);

            //Textvalues
            model.SearchComponent.TextBoxSearchValues = new List<TextValue>();

            return model;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public SearchModel initWorking()
        {
            SearchModel model = new SearchModel();

            MetadataReader.ListOfMetadata = MetadataReader.GetAllMetadataDatasets();

            //facets
            model.SearchComponent.Facets = FacetBuilder.GetAllNodesAsListFromData(FacetBuilder.CATEGORY_TYPE, MetadataReader.ListOfMetadata);

            //properties
            PropertyBuilder propertyBuilder = new PropertyBuilder(MetadataReader.ListOfMetadata);
            model.SearchComponent.Properties = propertyBuilder.AllProperties;

            //categories
            model.SearchComponent.Categories = CategoryBuilder.GetAllRootNodesAsList(CategoryBuilder.DROPDOWN_TYPE);

            //Textvalues
            model.SearchComponent.TextBoxSearchValues = new List<TextValue>();

            return model;

        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public IEnumerable<Property> GetProperties()
        {
            return this.WorkingSearchModel.SearchComponent.Properties;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public IEnumerable<Category> GetCategories()
        {
            return this.WorkingSearchModel.SearchComponent.Categories;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public IEnumerable<Facet> GetFacets()
        {
            return this.WorkingSearchModel.SearchComponent.Facets;
        }
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
        public SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults)
        {
            if (searchType == "new") MetadataReader.ListOfMetadata = MetadataReader.GetAllMetadataDatasets();

            if (filter != null && filter != "all")
            {
                this.WorkingSearchModel.SearchComponent.TextBoxSearchValues = MetadataReader.GetAllTextValuesByNodeDistinct(filter, MetadataReader.ListOfMetadata);
            }
            else this.WorkingSearchModel.SearchComponent.TextBoxSearchValues = MetadataReader.GetAllValuesAsListDistinct(MetadataReader.ListOfMetadata);

            return this.WorkingSearchModel;
        }

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
        public SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults, SearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        public SearchModel UpdateFacets(SearchCriteria searchCriteria)
        {
            MetadataReader.ListOfMetadata = MetadataReader.GetAllMetadataDatasetsWithListOfSearchCriteria(MetadataReader.ListOfMetadata, searchCriteria);
            //facets
            this.WorkingSearchModel.SearchComponent.Facets = FacetBuilder.GetAllNodesAsListFromData(FacetBuilder.CATEGORY_TYPE, MetadataReader.ListOfMetadata);

            return this.WorkingSearchModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="searchCriteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="currentPage"></param>
        /// <returns></returns>
        public SearchModel Get(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1)
        {

            MetadataReader.ListOfMetadata = MetadataReader.GetAllMetadataDatasets();
            MetadataReader.ListOfMetadata = MetadataReader.GetAllMetadataDatasetsWithListOfSearchCriteria(MetadataReader.ListOfMetadata, searchCriteria);

            this.WorkingSearchModel.ResultComponent = ResultObjectBuilder.ConvertListOfMetadataToSearchResultObject(MetadataReader.ListOfMetadata, pageSize, currentPage);


            return this.WorkingSearchModel;
        }

        #region ISearchProvider Member

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="searchCriteria"></param>
        /// <param name="pageSize"></param>
        /// <param name="currentPage"></param>
        /// <returns></returns>
        public SearchModel SearchAndUpdate(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1)
        {
            this.WorkingSearchModel = Get(searchCriteria, pageSize, currentPage);
            this.WorkingSearchModel = UpdateFacets(searchCriteria);

            return this.WorkingSearchModel;
        }

        #endregion


        #region ISearchProvider Member

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="searchCriteria"></param>
        /// <return></return>
        public void SearchAndUpdate(SearchCriteria searchCriteria)
        {
            this.WorkingSearchModel = Get(searchCriteria);
            this.WorkingSearchModel = UpdateFacets(searchCriteria);
        }

        #endregion
    }

}
