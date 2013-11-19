using System;
using System.Collections.Generic;
using System.Linq;
using BExIS.Search.Api;
using BExIS.Search.Model;
using BExIS.Search.Providers.LuceneProvider.Config;
using BExIS.Search.Providers.LuceneProvider.Indexer;
using BExIS.Search.Providers.LuceneProvider.Searcher;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

namespace BExIS.Search.Providers.LuceneProvider
{
    public class SearchProvider : ISearchProvider
    {
        public static Dictionary<object, WeakReference> Providers = new Dictionary<object, WeakReference>();

        public SearchModel DefaultSearchModel { get; private set; }
        public SearchModel WorkingSearchModel { get; private set; }

        private Query bexisSearching;

        public SearchProvider()
        {
            load();
            Providers.Add(this.GetHashCode() , new WeakReference(this));
        }

        ~SearchProvider()
        {
            Providers.Remove(this.GetHashCode());
        }

        public void Reload()
        {
            load(true);
        }

        private void load(bool forceReset=false)
        {
            if (forceReset == true)
                SearchConfig.Reset();
            SearchConfig.LoadConfig();

            this.DefaultSearchModel = initDefault();
            this.WorkingSearchModel = initWorking(); //init(WorkingSearchModel); // its better to make a clone form DefualtSearchModel than calling the function twice
            //this.DefaultSearchModel = Get(this.WorkingSearchModel.CriteriaComponent);
            this.WorkingSearchModel = Get(this.WorkingSearchModel.CriteriaComponent);
        }


        #region ISearchDataModel Member

        private SearchModel initDefault()
        {
            SearchModel model = new SearchModel();
            //facets
            model.SearchComponent.Facets = SearchConfig.getFacets().ToList();

            //properties
            model.SearchComponent.Properties = new List<Property>(SearchConfig.getProperties());

            //categories
            model.SearchComponent.Categories = new List<Category>(SearchConfig.getCategories());

            //Textvalues
            model.SearchComponent.TextBoxSearchValues = new List<TextValue>();

            return model;
            //throw new NotImplementedException();
        }
            
        private SearchModel initWorking()
        {
            SearchModel model = new SearchModel();
            //facets
            model.SearchComponent.Facets = SearchConfig.getFacetsCopy();

            //properties
            model.SearchComponent.Properties = new List<Property>(SearchConfig.getPropertiesCopy());

            //categories
            model.SearchComponent.Categories = new List<Category>(SearchConfig.getCategoriesCopy());


            //Textvalues
            model.SearchComponent.TextBoxSearchValues = new List<TextValue>();

            return model;
            //throw new NotImplementedException();
        }

        public SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults)
        {
            if (searchType.Equals("basedon")) getQueryFromCriteria(this.WorkingSearchModel.CriteriaComponent);
            if (searchType.Equals("new")) getQueryFromCriteria(new SearchCriteria());
            this.WorkingSearchModel.SearchComponent.TextBoxSearchValues = BexisIndexSearcher.doTextSearch(this.bexisSearching, filter, value);
            return this.WorkingSearchModel;
        }

        public SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults, SearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        public SearchModel UpdateFacets(SearchCriteria searchCriteria = null)
        {
            if (searchCriteria == null)
                searchCriteria = new SearchCriteria();
            getQueryFromCriteria(searchCriteria);

            this.WorkingSearchModel.SearchComponent.Facets = BexisIndexSearcher.facetSearch(this.bexisSearching, this.WorkingSearchModel.SearchComponent.Facets);

            return this.WorkingSearchModel;
        }

        public SearchModel Get(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1)
        {

            getQueryFromCriteria(searchCriteria);
            this.WorkingSearchModel.ResultComponent = BexisIndexSearcher.search(bexisSearching, SearchConfig.headerItemXmlNodeList);
            return this.WorkingSearchModel;
            //return ResultObjectBuilder.ConvertListOfMetadataToSearchResultObject(MetadataReader.ListOfMetadata, pageSize, currentPage);
            //return null;
        }

        public void SearchAndUpdate(SearchCriteria searchCriteria)
        {
            this.WorkingSearchModel = Get(searchCriteria);
            this.WorkingSearchModel = UpdateFacets(searchCriteria);
        }
    
        public SearchModel SearchAndUpdate(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1)
        {
            this.WorkingSearchModel = Get(searchCriteria, pageSize, currentPage);
            this.WorkingSearchModel = UpdateFacets(searchCriteria);

            return this.WorkingSearchModel;
        }
       
        private void getQueryFromCriteria(SearchCriteria searchCriteria)
        {

            if (searchCriteria.SearchCriteriaList.Count() > 0)
            {
                bexisSearching = new BooleanQuery();
                foreach (SearchCriterion sco in searchCriteria.SearchCriteriaList)
                {
                    if (sco.Values.Count > 0)
                    {
                        if (sco.SearchComponent.Type.Equals(SearchComponentBaseType.Category))
                        {
                            BooleanQuery bexisSearchingCategory = new BooleanQuery();
                            String fieldName = "category_" + sco.SearchComponent.Name;
                            QueryParser parser;
                            if (fieldName.ToLower().Equals("category_all"))
                            {
                                parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, BexisIndexSearcher.getCategoryFields(), new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
                            }
                            else
                            {
                                parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, fieldName, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
                            }
                            foreach (String value in sco.Values)
                            {

                                String newString = null;
                              //string value = val.Replace(")", "").Replace("(", "");
                                char[] delimiter = new char[] { ';', ' ', ',', '!', '.' };
                                string[] parts = value.ToLower().Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < parts.Length; i++)
                                {
                                    newString = newString + " " + parts[i] + "~0.6";
                                }
                                parser.PhraseSlop = 5;
                                parser.DefaultOperator = QueryParser.AND_OPERATOR;
                                Query query = parser.Parse(value);
                                bexisSearchingCategory.Add(query, Occur.SHOULD);
                            }
                            ((BooleanQuery)bexisSearching).Add(bexisSearchingCategory, Occur.MUST);
                        }
                        else if (sco.SearchComponent.Type.Equals(SearchComponentBaseType.Facet))
                        {
                            String fieldName = "facet_" + sco.SearchComponent.Name;
                            BooleanQuery bexisSearchingFacet = new BooleanQuery();
                            foreach (String value in sco.Values)
                            {
                                Query query = new TermQuery(new Term(fieldName, value));
                                bexisSearchingFacet.Add(query, Occur.SHOULD);
                            }
                            ((BooleanQuery)bexisSearching).Add(bexisSearchingFacet, Occur.MUST);
                        }
                        else if (sco.SearchComponent.Type.Equals(SearchComponentBaseType.Property))
                        {
                            String fieldName = "property_" + sco.SearchComponent.Name;
                            Property pp = (Property)sco.SearchComponent;
                            if (pp.UIComponent.ToLower().Equals("range"))
                            {
                                fieldName = "property_numeric_" + sco.SearchComponent.Name;
                                DateTime dd = new DateTime(Int32.Parse(sco.Values[0]), 1, 1, 1, 1, 1);
                                if (pp.Direction == Direction.increase)
                                {
                                    NumericRangeQuery<long> dateRangeQuery = NumericRangeQuery.NewLongRange(fieldName , dd.Ticks, long.MaxValue, true, true);
                                    ((BooleanQuery)bexisSearching).Add(dateRangeQuery, Occur.MUST);
                                }
                                else
                                {
                                    NumericRangeQuery<long> dateRangeQuery = NumericRangeQuery.NewLongRange(fieldName , long.MinValue, dd.Ticks, true, true);
                                    ((BooleanQuery)bexisSearching).Add(dateRangeQuery, Occur.MUST);
                                }
                            }
                            else
                            {
                                BooleanQuery bexisSearchingProperty = new BooleanQuery();
                                foreach (String value in sco.Values)
                                {
                                    if (value.ToLower().Equals("all"))
                                    {
                                        Query query = new MatchAllDocsQuery();
                                        bexisSearchingProperty.Add(query, Occur.SHOULD);
                                    }
                                    else
                                    {
                                        if (SearchConfig.getNumericProperties().Contains(sco.SearchComponent.Name.ToLower()))
                                        {


                                        }

                                        else
                                        {
                                            Query query = new TermQuery(new Term(fieldName, value));
                                            bexisSearchingProperty.Add(query, Occur.SHOULD);
                                        }
                                    }
                                }
                                ((BooleanQuery)bexisSearching).Add(bexisSearchingProperty, Occur.MUST);


                            }
                        }
                    }
                    else
                    {
                        //do nothing yet
                    }
                }
            }
            else
            {
                QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "id", new SimpleAnalyzer());
                bexisSearching = parser.Parse("*:*");
            }
        }

        #endregion
    }
}