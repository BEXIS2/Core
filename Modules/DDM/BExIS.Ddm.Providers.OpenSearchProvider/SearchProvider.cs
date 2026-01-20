using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using BExIS.Ddm.Api;
using BExIS.Ddm.Providers.OpenSearchProvider.Config;
using BExIS.Utils.Models;
using OpenSearch.Client;
using Vaiona.Logging;

namespace BExIS.Ddm.Providers.OpenSearchProvider
{
    public class SearchProvider : ISearchProvider
    {
        public static Dictionary<object, WeakReference> Providers = new Dictionary<object, WeakReference>();

        public SearchModel DefaultSearchModel { get; private set; }
        public SearchModel WorkingSearchModel { get; private set; }

        private OpenSearchIndexer _indexer = new OpenSearchIndexer();

        private QueryContainer _searchQuery;

        // CTOR
        public SearchProvider()
        {
            this.Load();
            Providers.Add(this.GetHashCode(), new WeakReference(this));
        }

        ~SearchProvider()
        {
            Providers.Remove(this.GetHashCode());
        }

        #region ClassMethods

        private void Load(bool forceReset = false)
        {
            if (forceReset == true) OpenSearchConfig.Reset();

            OpenSearchConfig.LoadConfig(); // first call of SearchConfig --> init SearchConfig (static)
            DefaultSearchModel = InitDefault();
            WorkingSearchModel = InitWorking();
            WorkingSearchModel = Get(WorkingSearchModel.CriteriaComponent);
        }

        private SearchModel InitDefault()
        {
            SearchModel model = new SearchModel();

            model.SearchComponent.Facets = OpenSearchConfig.GetFacets().ToList();
            model.SearchComponent.Properties = new List<Property>(OpenSearchConfig.GetProperties());
            model.SearchComponent.Categories = new List<Category>(OpenSearchConfig.GetCategories());
            model.SearchComponent.TextBoxSearchValues = new List<TextValue>();
            model.SearchComponent.Generals = new List<General>()
                        { new General()
                                { Name="gen_isPublic", DefaultValue = "FALSE", DisplayName = "Is dataset public", Value = "FALSE", IsVisible = false},
                          new General()
                                { Name="gen_entity_name", DefaultValue = "", DisplayName = "Type", Value = "", IsVisible = true},
                          new General()
                                { Name="gen_doi", DefaultValue = "", DisplayName = "DOI", Value = "FALSE", IsVisible = true},
                          new General()
                                { Name="gen_modifieddate", DefaultValue = "", DisplayName = "Last modified date", Value = "", IsVisible = true},
                        new General()
                                { Name="gen_entitytemplate", DefaultValue = "", DisplayName = "Template", Value = "", IsVisible = true},
                        };

            return model;
        }

        private SearchModel InitWorking()
        {
            SearchModel model = new SearchModel();
            //facets
            model.SearchComponent.Facets = OpenSearchConfig.GetFacetsCopy();

            //properties
            model.SearchComponent.Properties = new List<Property>(OpenSearchConfig.GetPropertiesCopy());

            //categories
            model.SearchComponent.Categories = new List<Category>(OpenSearchConfig.GetCategoriesCopy());

            //Textvalues
            model.SearchComponent.TextBoxSearchValues = new List<TextValue>();

            model.SearchComponent.Generals = new List<General>()
                        { new General()
                                { Name="gen_isPublic", DefaultValue = "FALSE", DisplayName = "Is dataset public", Value = "FALSE", IsVisible = false},
                          new General()
                                { Name="gen_entity_name", DefaultValue = "", DisplayName = "Type", Value = "", IsVisible = true},
                          new General()
                                { Name="gen_doi", DefaultValue = "", DisplayName = "DOI", Value = "FALSE", IsVisible = true},
                          new General()
                                { Name="gen_modifieddate", DefaultValue = "", DisplayName = "Last modified date", Value = "", IsVisible = true},
                        new General()
                                { Name="gen_entitytemplate", DefaultValue = "", DisplayName = "Template", Value = "", IsVisible = true},
                        };
            return model;
            //throw new NotImplementedException();
        }

        private void GetQueryFromCriteria(SearchCriteria searchCriteria)
        {
            List<QueryContainer> mustQueries = new List<QueryContainer>();

            if (searchCriteria.SearchCriteriaList.Count() > 0)
            {
                foreach (SearchCriterion sco in searchCriteria.SearchCriteriaList)
                {
                    if (sco.Values.Count > 0)
                    {
                        if (sco.SearchComponent.Type.Equals(SearchComponentBaseType.General))
                        {
                            var shouldQueries = new List<QueryContainer>();
                            String fieldName = sco.SearchComponent.Name;

                            foreach (String value in sco.Values)
                            {
                                shouldQueries.Add(new TermQuery()
                                {
                                    Field = fieldName,
                                    Value = value,
                                });
                            }

                            var innerBool = new BoolQuery
                            {
                                Should = shouldQueries,
                                MinimumShouldMatch = 1
                            };

                            mustQueries.Add(innerBool);

                        }
                        else if (sco.SearchComponent.Type.Equals(SearchComponentBaseType.Category))
                        {
                            string fieldName = $"category_{sco.SearchComponent.Name}";
                            List<QueryContainer> shouldQueries = new List<QueryContainer>();

                            List<string> fields;
                            if (fieldName.ToLower() == "category_all")
                            {
                                fields = _indexer.CategoryFieldList;
                                fields.AddRange(_indexer.StoredFieldList);
                                fields.Add("ng_all");
                            }
                            else
                            {
                                fields = new List<string> { fieldName };
                            }

                            foreach (string value in sco.Values)
                            {
                                string encodedValue = value;
                                if (string.IsNullOrEmpty(encodedValue))
                                {
                                    encodedValue = "*";
                                }

                                shouldQueries.Add(new MultiMatchQuery()
                                {
                                    Query = encodedValue,
                                    Fields = Infer.Fields(fields.ToArray()),
                                    Fuzziness = Fuzziness.Auto,
                                    Operator = Operator.And,
                                    Slop = 5,
                                    Type = TextQueryType.MostFields
                                });

                            }

                            mustQueries.Add(new BoolQuery
                            {
                                Should = shouldQueries
                            });
                        }
                        else if (sco.SearchComponent.Type.Equals(SearchComponentBaseType.Facet))
                        {
                            String fieldName = $"facet_{sco.SearchComponent.Name}";
                            List<QueryContainer> mustFacet = new List<QueryContainer>();
                            List<QueryContainer> shouldFacet = new List<QueryContainer>();


                            foreach (String value in sco.Values)
                            {
                                var valueSplitted = value.Split(new[] { " - " }, StringSplitOptions.None);
                                QueryContainer query;
                                try
                                {
                                    double startNum, endNum;
                                    if (valueSplitted.Length == 2 &&
                                        DateTime.TryParse(valueSplitted[0], out var startDate) &&
                                        DateTime.TryParse(valueSplitted[1], out var endDate))
                                    {
                                        query = new DateRangeQuery
                                        {
                                            Field = fieldName,
                                            GreaterThanOrEqualTo = startDate,
                                            LessThanOrEqualTo = endDate
                                        };
                                    }
                                    else if (valueSplitted.Length == 2 &&
                                            double.TryParse(valueSplitted[0], out startNum) &&
                                            double.TryParse(valueSplitted[1], out endNum)) // check if range values are numbers
                                    {
                                        query = new NumericRangeQuery()
                                        {
                                            Field = fieldName,
                                            GreaterThanOrEqualTo = startNum,
                                            LessThanOrEqualTo = endNum
                                        };
                                    }
                                    else
                                    {
                                        query = new TermQuery
                                        {
                                            Field = $"{fieldName}.keyword",
                                            Value = value
                                        };
                                    }

                                    if (sco.ValueSearchOperation == "AND")
                                        mustFacet.Add(query);
                                    else
                                        shouldFacet.Add(query);
                                }
                                catch (Exception excep)
                                {
                                    LoggerFactory.GetFileLogger().LogCustom(excep.Message);
                                    LoggerFactory.GetFileLogger().LogCustom(excep.InnerException.Message);
                                }
                            } // end foreach

                            if (sco.ValueSearchOperation == "AND")
                                mustQueries.AddRange(mustFacet);
                            else
                            {
                                mustQueries.Add(new BoolQuery
                                {
                                    Should = shouldFacet,
                                    MinimumShouldMatch = 1
                                });
                            }
                        }
                        else if (sco.SearchComponent.Type.Equals(SearchComponentBaseType.Property))
                        {
                            String fieldName = $"property_{sco.SearchComponent.Name}";
                            List<QueryContainer> shoudQueries = new List<QueryContainer>();

                            if (sco.SearchComponent is Property pp && pp.UIComponent?.ToLower() == "range")
                            {
                                fieldName = $"property_numeric_{sco.SearchComponent.Name}";

                                DateTime dd = new DateTime(Int32.Parse(sco.Values[0]), 1, 1, 1, 1, 1);
                                if (int.TryParse(sco.Values[0], out int year))
                                {
                                    var date = new DateTime(year, 1, 1, 1, 1, 1);
                                    long ticks = date.Ticks;

                                    var rangeQuery = new LongRangeQuery
                                    {
                                        Field = fieldName,
                                        GreaterThan = (pp.Direction == Direction.increase) ? ticks : (long?)null,
                                        LessThanOrEqualTo = (pp.Direction == Direction.increase) ? ticks : (long?)null,
                                    };
                                    mustQueries.Add(rangeQuery);
                                }
                            }
                            else // case: not range
                            {

                                foreach (string value in sco.Values)
                                {
                                    if (value.ToLower() == "all")
                                    {
                                        shoudQueries.Add(new MatchAllQuery());
                                    }
                                    else
                                    {
                                        var encodedValue = value;
                                        if (OpenSearchConfig.GetNumericPropertiers().Contains(sco.SearchComponent.Name.ToLower()))
                                        {
                                        }
                                        else
                                        {
                                            shoudQueries.Add(new TermQuery
                                            {
                                                Field = $"{fieldName}.keyword",
                                                Value = encodedValue
                                            });
                                        }
                                    }
                                }
                                mustQueries.Add(new BoolQuery
                                {
                                    Should = shoudQueries,
                                    MinimumShouldMatch = 1
                                });
                            }
                        }

                    }
                    else
                    {
                        //do nothing yet
                    }
                }
                this._searchQuery = new BoolQuery
                {
                    Must = mustQueries
                };
            } // end if sco.Count > 0
            else
            {
                this._searchQuery = new MatchAllQuery();
            }
        }

        #endregion

        #region ISearchProviderMethods

        public BExIS.Utils.Models.SearchModel Get(BExIS.Utils.Models.SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1)
        {
            GetQueryFromCriteria(searchCriteria);
            WorkingSearchModel.ResultComponent = _indexer.Search(_searchQuery, OpenSearchConfig.headerItemXmlNodeList);

            return WorkingSearchModel;
        }

        public BExIS.Utils.Models.SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults)
        {
            if (searchType.Equals("basedon")) GetQueryFromCriteria(this.WorkingSearchModel.CriteriaComponent);
            if (searchType.Equals("new")) GetQueryFromCriteria(new SearchCriteria());

            // encoding special characters for lucene
            //value = EncoderHelper.Encode(value);

            this.WorkingSearchModel.SearchComponent.TextBoxSearchValues = _indexer.DoTextSearch(_searchQuery, filter, value);
            return this.WorkingSearchModel;
        }

        public BExIS.Utils.Models.SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults, BExIS.Utils.Models.SearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            Load(true);
        }

        public BExIS.Utils.Models.SearchModel SearchAndUpdate(BExIS.Utils.Models.SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1)
        {
            this.WorkingSearchModel = Get(searchCriteria, pageSize, currentPage);
            this.WorkingSearchModel = UpdateFacets(searchCriteria);
            this.WorkingSearchModel = UpdateProperties(searchCriteria);
            this.WorkingSearchModel.ResultComponent.Rows = this.WorkingSearchModel.ResultComponent.Rows.OrderByDescending(r => Convert.ToDecimal(r.Values.First())).ToList();

            return this.WorkingSearchModel;
        }

        public void SearchAndUpdate(BExIS.Utils.Models.SearchCriteria searchCriteria)
        {
            this.WorkingSearchModel = Get(searchCriteria);
            this.WorkingSearchModel = UpdateFacets(searchCriteria);
            this.WorkingSearchModel = UpdateProperties(searchCriteria);
            this.WorkingSearchModel.ResultComponent.Rows = this.WorkingSearchModel.ResultComponent.Rows.OrderByDescending(r => Convert.ToDecimal(r.Values.First()));
        }

        public BExIS.Utils.Models.SearchModel UpdateFacets(BExIS.Utils.Models.SearchCriteria searchCriteria)
        {
            if (searchCriteria == null)
                searchCriteria = new SearchCriteria();
            GetQueryFromCriteria(searchCriteria);

            this.WorkingSearchModel.SearchComponent.Facets = _indexer.FacetSearch(_searchQuery, this.WorkingSearchModel.SearchComponent.Facets);

            return this.WorkingSearchModel;
        }

        public void UpdateIndex(Dictionary<long, IndexingAction> datasetsToIndex, bool onlyReleasedTags)
        {
            OpenSearchIndexer bexisIndexer = new OpenSearchIndexer();
            bexisIndexer.UpdateIndex(datasetsToIndex, onlyReleasedTags);

            Reload();
        }

        public BExIS.Utils.Models.SearchModel UpdateProperties(BExIS.Utils.Models.SearchCriteria searchCriteria)
        {
            if (searchCriteria == null)
                searchCriteria = new SearchCriteria();
            GetQueryFromCriteria(searchCriteria);

            this.WorkingSearchModel.SearchComponent.Properties = _indexer.PropertySearch(_searchQuery, this.WorkingSearchModel.SearchComponent.Properties);

            return this.WorkingSearchModel;
        }

        public void UpdateSingleDatasetIndex(long datasetId, IndexingAction indAction, bool onlyReleasedTags)
        {
            _indexer.UpdateSingleDatasetIndex(datasetId, indAction, onlyReleasedTags);
            Reload();
        }
        #endregion
    }
}
