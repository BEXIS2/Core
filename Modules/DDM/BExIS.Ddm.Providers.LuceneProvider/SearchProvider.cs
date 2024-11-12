using BExIS.Ddm.Api;
using BExIS.Ddm.Providers.LuceneProvider.Config;
using BExIS.Ddm.Providers.LuceneProvider.Helpers;
using BExIS.Ddm.Providers.LuceneProvider.Indexer;
using BExIS.Ddm.Providers.LuceneProvider.Searcher;
using BExIS.Utils.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Logging;
using SearchCriteria = BExIS.Utils.Models.SearchCriteria;
using SearchCriterion = BExIS.Utils.Models.SearchCriterion;
using SearchModel = BExIS.Utils.Models.SearchModel;

/// <summary>
///
/// </summary>
namespace BExIS.Ddm.Providers.LuceneProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class SearchProvider : ISearchProvider
    {
        public static Dictionary<object, WeakReference> Providers = new Dictionary<object, WeakReference>();

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
        public SearchModel WorkingSearchModel { get; private set; }

        private Query bexisSearching;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public SearchProvider()
        {
            load();
            Providers.Add(this.GetHashCode(), new WeakReference(this));
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        ~SearchProvider()
        {
            Providers.Remove(this.GetHashCode());
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public void Reload()
        {
            load(true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="forceReset"></param>
        /// <return></return>
        private void load(bool forceReset = false)
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
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

            /// Add the general searchable properties, these are usually not visible through the UI.
            /// Here, there is one built-in field that is used to filter public datasets
            ///
            model.SearchComponent.Generals = new List<General>()
                        { new General()
                                { Name="gen_isPublic", DefaultValue = "FALSE", DisplayName = "Is dataset public", Value = "FALSE", IsVisible = false},
                          new General()
                                { Name="gen_entity_name", DefaultValue = "", DisplayName = "Type", Value = "", IsVisible = true}
                        };

            return model;
            //throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        /// <returns></returns>
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

            model.SearchComponent.Generals = new List<General>()
                        { new General()
                                { Name="gen_isPublic", DefaultValue = "FALSE", DisplayName = "Is dataset public", Value = "FALSE", IsVisible = false},
                         new General()
                                { Name="gen_entity_name", DefaultValue = "", DisplayName = "Type", Value = "", IsVisible = true}
                        };

            return model;
            //throw new NotImplementedException();
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
            if (searchType.Equals("basedon")) getQueryFromCriteria(this.WorkingSearchModel.CriteriaComponent);
            if (searchType.Equals("new")) getQueryFromCriteria(new SearchCriteria());

            // encoding special characters for lucene
            value = EncoderHelper.Encode(value);

            this.WorkingSearchModel.SearchComponent.TextBoxSearchValues = BexisIndexSearcher.doTextSearch(this.bexisSearching, filter, value);
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

        public SearchModel UpdateFacets(SearchCriteria searchCriteria = null)
        {
            if (searchCriteria == null)
                searchCriteria = new SearchCriteria();
            getQueryFromCriteria(searchCriteria);

            this.WorkingSearchModel.SearchComponent.Facets = BexisIndexSearcher.facetSearch(this.bexisSearching, this.WorkingSearchModel.SearchComponent.Facets);

            return this.WorkingSearchModel;
        }

        public SearchModel UpdateProperties(SearchCriteria searchCriteria = null)
        {
            if (searchCriteria == null)
                searchCriteria = new SearchCriteria();
            getQueryFromCriteria(searchCriteria);

            this.WorkingSearchModel.SearchComponent.Properties = BexisIndexSearcher.propertySearch(this.bexisSearching, this.WorkingSearchModel.SearchComponent.Properties);

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
            getQueryFromCriteria(searchCriteria);
            this.WorkingSearchModel.ResultComponent = BexisIndexSearcher.search(bexisSearching, SearchConfig.headerItemXmlNodeList);

            return this.WorkingSearchModel;
        }

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
            this.WorkingSearchModel = UpdateProperties(searchCriteria);
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
        public SearchModel SearchAndUpdate(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1)
        {
            this.WorkingSearchModel = Get(searchCriteria, pageSize, currentPage);
            this.WorkingSearchModel = UpdateFacets(searchCriteria);
            this.WorkingSearchModel = UpdateProperties(searchCriteria);

            return this.WorkingSearchModel;
        }

        public void UpdateIndex(Dictionary<long, IndexingAction> datasetsToIndex)
        {
            BexisIndexer bexisIndexer = new BexisIndexer();
            bexisIndexer.updateIndex(datasetsToIndex);

            Reload();
        }

        public void UpdateSingleDatasetIndex(long datasetId, IndexingAction indAction)
        {
            BexisIndexer bexisIndexer = new BexisIndexer();
            bexisIndexer.updateSingleDatasetIndex(datasetId, indAction);

            Reload();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="searchCriteria"></param>
        /// <return></return>
        private void getQueryFromCriteria(SearchCriteria searchCriteria)
        {
            if (searchCriteria.SearchCriteriaList.Count() > 0)
            {
                bexisSearching = new BooleanQuery();
                foreach (SearchCriterion sco in searchCriteria.SearchCriteriaList)
                {
                    if (sco.Values.Count > 0)
                    {
                        if (sco.SearchComponent.Type.Equals(SearchComponentBaseType.General))
                        {
                            String fieldName = sco.SearchComponent.Name;
                            BooleanQuery bexisSearchingGeneral = new BooleanQuery();
                            foreach (String value in sco.Values)
                            {
                                String encodedValue = value;
                                Query query = new TermQuery(new Term(fieldName, encodedValue));
                                bexisSearchingGeneral.Add(query, Occur.SHOULD);
                            }
                            ((BooleanQuery)bexisSearching).Add(bexisSearchingGeneral, Occur.MUST);
                        }
                        else if (sco.SearchComponent.Type.Equals(SearchComponentBaseType.Category))
                        {
                            BooleanQuery bexisSearchingCategory = new BooleanQuery();
                            String fieldName = "category_" + sco.SearchComponent.Name;
                            QueryParser parser;
                            if (fieldName.ToLower().Equals("category_all"))
                            {
                                List<string> temp2 = BexisIndexSearcher.getCategoryFields().ToList();
                                temp2.AddRange(BexisIndexSearcher.getStoredFields().ToList());
                                temp2.Add("ng_all");
                                parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, temp2.ToArray(), new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
                            }
                            else
                            {
                                parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, fieldName, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
                            }
                            foreach (String value in sco.Values)
                            {
                                String encodedValue = EncoderHelper.Encode(value);
                                String newString = null;
                                //string value = val.Replace(")", "").Replace("(", "");
                                char[] delimiter = new char[] { ';', ' ', ',', '!', '.' };
                                string[] parts = encodedValue.ToLower().Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                                for (int i = 0; i < parts.Length; i++)
                                {
                                    newString = newString + " " + parts[i] + "~0.6";
                                }
                                parser.PhraseSlop = 5;
                                parser.DefaultOperator = QueryParser.AND_OPERATOR;
                                string query_value = encodedValue;
                                if (encodedValue.Equals(""))
                                {
                                    query_value = "*:*";
                                }
                                Query query = parser.Parse(query_value);
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
                                String encodedValue = value;
                                string[] list_values = encodedValue.Split(new string[] { " - " }, StringSplitOptions.None);
                                if (list_values.Length == 2)
                                {
                                    try
                                    {
                                        // check if range values are dates
                                        DateTime dateValue_;
                                        DateTime.TryParse(list_values[0], out dateValue_);
                                        DateTime dateValue__;
                                        DateTime.TryParse(list_values[1], out dateValue__);
                                        if ((dateValue_ != DateTime.MinValue) && (dateValue__ != DateTime.MinValue))
                                        {
                                            Query rangeQuery = new TermRangeQuery(fieldName, Lucene.Net.Documents.DateTools.DateToString(dateValue_, Lucene.Net.Documents.DateTools.Resolution.SECOND),
                                                Lucene.Net.Documents.DateTools.DateToString(dateValue__, Lucene.Net.Documents.DateTools.Resolution.SECOND), true, true);
                                            if (sco.ValueSearchOperation == "AND")
                                                bexisSearchingFacet.Add(rangeQuery, Occur.MUST);
                                            else bexisSearchingFacet.Add(rangeQuery, Occur.SHOULD);
                                        }
                                        else // check if range values are numbers
                                        {
                                            double out_;
                                            double out__;

                                            if (double.TryParse(encodedValue.Split('-')[0].Trim(), out out_) && double.TryParse(encodedValue.Split('-')[1].Trim(), out out__))
                                            {
                                                Query rangeQuery = NumericRangeQuery.NewDoubleRange(fieldName, out_, out__, true, true);
                                                if (sco.ValueSearchOperation == "AND")
                                                    bexisSearchingFacet.Add(rangeQuery, Occur.MUST);
                                                else bexisSearchingFacet.Add(rangeQuery, Occur.SHOULD);
                                            }
                                            else
                                            {                                                
                                                Query query = new TermQuery(new Term(fieldName, encodedValue));
                                                if (sco.ValueSearchOperation == "AND")
                                                    bexisSearchingFacet.Add(query, Occur.MUST);
                                                else bexisSearchingFacet.Add(query, Occur.SHOULD);
                                            }
                                        }
                                    }
                                    catch (Exception excep)
                                    {
                                        LoggerFactory.GetFileLogger().LogCustom(excep.Message);
                                        LoggerFactory.GetFileLogger().LogCustom(excep.InnerException.Message);
                                    }
                                }
                                else
                                {
                                    Query query = new TermQuery(new Term(fieldName, encodedValue));
                                    if (sco.ValueSearchOperation == "AND")
                                        bexisSearchingFacet.Add(query, Occur.MUST);
                                    else bexisSearchingFacet.Add(query, Occur.SHOULD);
                                }
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
                                    NumericRangeQuery<long> dateRangeQuery = NumericRangeQuery.NewLongRange(fieldName, dd.Ticks, long.MaxValue, true, true);
                                    ((BooleanQuery)bexisSearching).Add(dateRangeQuery, Occur.MUST);
                                }
                                else
                                {
                                    NumericRangeQuery<long> dateRangeQuery = NumericRangeQuery.NewLongRange(fieldName, long.MinValue, dd.Ticks, true, true);
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
                                        String encodedValue = value;
                                        if (SearchConfig.getNumericProperties().Contains(sco.SearchComponent.Name.ToLower()))
                                        {
                                        }
                                        else
                                        {
                                            Query query = new TermQuery(new Term(fieldName, encodedValue));
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
                using (var sa = new SimpleAnalyzer())
                {
                    QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "id", sa);
                    bexisSearching = parser.Parse("*:*");
                }
            }
        }

        #endregion ISearchDataModel Member
    }
}