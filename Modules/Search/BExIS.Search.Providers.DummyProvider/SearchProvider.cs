using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Search.Api;
using BExIS.Search.Model;
using BExIS.Search.Providers.DummyProvider.Helpers;
using BExIS.Search.Providers.DummyProvider.Helpers.Helpers.Search;


namespace BExIS.Search.Providers.DummyProvider
{
    public class SearchProvider : ISearchProvider
    {
        public SearchModel WorkingSearchModel { get; private set; }
        public SearchModel DefaultSearchModel { get; private set; }


        public SearchProvider()
        {

            this.DefaultSearchModel = initDefault();
            this.WorkingSearchModel = initWorking(); //init(WorkingSearchModel); // its better to make a clone form DefualtSearchModel than calling the function twice
            
            //this.DefaultSearchModel = Get(this.WorkingSearchModel.CriteriaComponent);
            this.WorkingSearchModel = Get(this.WorkingSearchModel.CriteriaComponent);
        }

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

        public IEnumerable<Property> GetProperties()
        {
            return this.WorkingSearchModel.SearchComponent.Properties;
        }

        public IEnumerable<Category> GetCategories()
        {
            return this.WorkingSearchModel.SearchComponent.Categories;
        }

        public IEnumerable<Facet> GetFacets()
        {
            return this.WorkingSearchModel.SearchComponent.Facets;
        }

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

        public SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults, SearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        public SearchModel UpdateFacets(SearchCriteria searchCriteria)
        {
            MetadataReader.ListOfMetadata = MetadataReader.GetAllMetadataDatasetsWithListOfSearchCriteria(MetadataReader.ListOfMetadata, searchCriteria);
            //facets
            this.WorkingSearchModel.SearchComponent.Facets = FacetBuilder.GetAllNodesAsListFromData(FacetBuilder.CATEGORY_TYPE, MetadataReader.ListOfMetadata);

            return this.WorkingSearchModel;
        }

        public SearchModel Get(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1)
        {

            MetadataReader.ListOfMetadata = MetadataReader.GetAllMetadataDatasets();
            MetadataReader.ListOfMetadata = MetadataReader.GetAllMetadataDatasetsWithListOfSearchCriteria(MetadataReader.ListOfMetadata, searchCriteria);

            this.WorkingSearchModel.ResultComponent = ResultObjectBuilder.ConvertListOfMetadataToSearchResultObject(MetadataReader.ListOfMetadata, pageSize, currentPage);


            return this.WorkingSearchModel;
        }

        #region ISearchProvider Member

        public SearchModel SearchAndUpdate(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1)
        {
            this.WorkingSearchModel = Get(searchCriteria, pageSize, currentPage);
            this.WorkingSearchModel = UpdateFacets(searchCriteria);

            return this.WorkingSearchModel;
        }

        #endregion


        #region ISearchProvider Member


        public void SearchAndUpdate(SearchCriteria searchCriteria)
        {
            this.WorkingSearchModel = Get(searchCriteria);
            this.WorkingSearchModel = UpdateFacets(searchCriteria);
        }

        #endregion
    }

}
