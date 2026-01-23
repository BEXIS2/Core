using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BExIS.Ddm.Api;
using BExIS.Ddm.Providers.OpenSearch.Config;
using BExIS.Utils.Models;
using OpenSearch.Client;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;

namespace BExIS.Ddm.Providers.OpenSearch
{
    public class SearchProvider : ISearchProvider
    {
        public static Dictionary<object, WeakReference> Providers = new Dictionary<object, WeakReference>();

        private string _configFilePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "OpenSearch", "Config", "SearchConfig.json");
        public SearchModel DefaultSearchModel { get; private set; }
        public SearchModel WorkingSearchModel { get; private set; }
        private OpenSearchIndexer _indexer = new OpenSearchIndexer();

        private QueryContainer _searchQuery;

        public SearchProvider()
        {
            Load();
            
        }

        private void Load(bool forceReset = false)
        {
            if (forceReset == true)
            {
                SearchConfigManager.Reload();
            }
SearchConfigManager.Reload();
            DefaultSearchModel = InitDefault();
            WorkingSearchModel = InitWorking();
            WorkingSearchModel = Get(WorkingSearchModel.CriteriaComponent);
        }
        private SearchModel InitDefault()
        {
            SearchModel model = new SearchModel();
            model.SearchComponent.Facets = SearchConfigManager.GetFacets().ToList();
            model.SearchComponent.Properties = SearchConfigManager.GetProperties();
            model.SearchComponent.Categories = SearchConfigManager.GetCategories();
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
            model.SearchComponent.Facets = SearchConfigManager.GetFacets();
            model.SearchComponent.Properties = SearchConfigManager.GetProperties();
            model.SearchComponent.Categories = SearchConfigManager.GetCategories();
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

        private void GetQueryFromCriteria(SearchCriteria searchCriteria)
        {
            List<QueryContainer> mustQueries = new List<QueryContainer>();

            if (searchCriteria.SearchCriteriaList.Count > 0)
            {
                foreach (SearchCriterion criteria in searchCriteria.SearchCriteriaList)
                {
                    if (criteria.Values.Count > 0)
                    {
                        if (criteria.SearchComponent.Type.Equals(Utils.Models.SearchComponentBaseType.General))
                        {
                            var shouldQueries = new List<QueryContainer>();
                            string fieldName = criteria.SearchComponent.Name;

                            foreach (string value in criteria.Values)
                            {
                                shouldQueries.Add(new TermQuery()
                                {
                                    Field = fieldName,
                                    Value = value
                                });
                            }

                            var innerBool = new BoolQuery()
                            {
                                Should = shouldQueries,
                                MinimumShouldMatch = 1
                            };
                            mustQueries.Add(innerBool);
                        }
                    }
                    else if (criteria.SearchComponent.Type.Equals(Utils.Models.SearchComponentBaseType.Category))
                    {
                        string fieldName = $"category_{criteria.SearchComponent.Name}";
                        List<QueryContainer> shouldQueries = new List<QueryContainer>();

                        List<string> fields;
                        if (fieldName.ToLower() == "category_all")
                        {
                            fields = new List<string>();
                            foreach (var cat in SearchConfigManager.GetGlobalCategories())
                            {
                                fields.Add("category_"+cat.ComponentName);
                            }

                            //fields = _indexer.CategoryFieldList;
                            //fields.AddRange(_indexer.StoredFieldList);
                            fields.Add("ng_all");
                        }
                        else
                        {
                            fields = new List<string> { fieldName };
                        }

                        foreach (string value in criteria.Values)
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
                    else if (criteria.SearchComponent.Type.Equals(Utils.Models.SearchComponentBaseType.Facet))
                    {
                        String fieldName = $"facet_{criteria.SearchComponent.Name}";
                        List<QueryContainer> mustFacet = new List<QueryContainer>();
                        List<QueryContainer> shouldFacet = new List<QueryContainer>();


                        foreach (String value in criteria.Values)
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

                                if (criteria.ValueSearchOperation == "AND")
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

                        if (criteria.ValueSearchOperation == "AND")
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
                    else if (criteria.SearchComponent.Type.Equals(Utils.Models.SearchComponentBaseType.Property))
                    {
                        String fieldName = $"property_{criteria.SearchComponent.Name}";
                        List<QueryContainer> shoudQueries = new List<QueryContainer>();

                        if (criteria.SearchComponent is Property pp && pp.UIComponent?.ToLower() == "range")
                        {
                            fieldName = $"property_numeric_{criteria.SearchComponent.Name}";

                            DateTime dd = new DateTime(int.Parse(criteria.Values[0]), 1, 1, 1, 1, 1);
                            if (int.TryParse(criteria.Values[0], out int year))
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

                            foreach (string value in criteria.Values)
                            {
                                if (value.ToLower() == "all")
                                {
                                    shoudQueries.Add(new MatchAllQuery());
                                }
                                else
                                {
                                    var encodedValue = value;
                                    //if (SearchConfigManager.GetNumericPropertiers().Contains(criteria.SearchComponent.Name.ToLower()))
                                    //{
                                    //}
                                    //else
                                    //{
                                        shoudQueries.Add(new TermQuery
                                        {
                                            Field = $"{fieldName}.keyword",
                                            Value = encodedValue
                                        });
                                    //}
                                }
                            }
                            mustQueries.Add(new BoolQuery
                            {
                                Should = shoudQueries,
                                MinimumShouldMatch = 1
                            });
                        }
                    }
                    else
                    {
                        // do nothing yet
                    }
                    }
                this._searchQuery = new BoolQuery
                {
                    Must = mustQueries
                };
            } // end if criteria.Count > 0
            else
            {
                _searchQuery = new MatchAllQuery();
            }
        }

        public SearchModel Get(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1)
        {
            GetQueryFromCriteria(searchCriteria);
            WorkingSearchModel.ResultComponent = _indexer.Search(_searchQuery, SearchConfigManager.GetAllComponents().ToList());
            //WorkingSearchModel.ResultComponent = _indexer.Search(_searchQuery, SearchConfigManager.GetAllHeaderComponents().ToList());


            return WorkingSearchModel;
        }

        public SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults)
        {
            if (searchType.Equals("basedon")) GetQueryFromCriteria(this.WorkingSearchModel.CriteriaComponent);
            if (searchType.Equals("new")) GetQueryFromCriteria(new SearchCriteria());

            // encoding special characters for lucene
            //value = EncoderHelper.Encode(value);

            this.WorkingSearchModel.SearchComponent.TextBoxSearchValues = _indexer.DoTextSearch(_searchQuery, filter, value);
            return this.WorkingSearchModel;
        }

        public SearchModel GetTextBoxSearchValues(string value, string filter, string searchType, int numberOfResults, SearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            SearchConfigManager.Reload();
        }

        public SearchModel SearchAndUpdate(SearchCriteria searchCriteria, int pageSize = 10, int currentPage = 1)
        {
            this.WorkingSearchModel = Get(searchCriteria, pageSize, currentPage);
            this.WorkingSearchModel = UpdateFacets(searchCriteria);
            this.WorkingSearchModel = UpdateProperties(searchCriteria);
            this.WorkingSearchModel.ResultComponent.Rows = this.WorkingSearchModel.ResultComponent.Rows.OrderByDescending(r => Convert.ToDecimal(r.Values.First())).ToList();

            return this.WorkingSearchModel;
        }

        public void SearchAndUpdate(SearchCriteria searchCriteria)
        {
            this.WorkingSearchModel = Get(searchCriteria);
            this.WorkingSearchModel = UpdateFacets(searchCriteria);
            this.WorkingSearchModel = UpdateProperties(searchCriteria);
            this.WorkingSearchModel.ResultComponent.Rows = this.WorkingSearchModel.ResultComponent.Rows.OrderByDescending(r => Convert.ToDecimal(r.Values.First()));
        }

        public SearchModel UpdateFacets(SearchCriteria searchCriteria)
        {
            if (searchCriteria == null)
                searchCriteria = new SearchCriteria();
            GetQueryFromCriteria(searchCriteria);

            this.WorkingSearchModel.SearchComponent.Facets = _indexer.FacetSearch(_searchQuery, this.WorkingSearchModel.SearchComponent.Facets);

            return this.WorkingSearchModel;
        }

        public SearchModel UpdateProperties(SearchCriteria searchCriteria)
        {
            if (searchCriteria == null)
                searchCriteria = new SearchCriteria();
            GetQueryFromCriteria(searchCriteria);

            this.WorkingSearchModel.SearchComponent.Properties = _indexer.PropertySearch(_searchQuery, this.WorkingSearchModel.SearchComponent.Properties);

            return this.WorkingSearchModel;
        }

        public void UpdateIndex(Dictionary<long, IndexingAction> datasetsToIndex, bool onlyReleasedTags)
        {
            OpenSearchIndexer bexisIndexer = new OpenSearchIndexer();
            bexisIndexer.UpdateIndex(datasetsToIndex, onlyReleasedTags);

            Reload();
        }


        public void UpdateSingleDatasetIndex(long datasetId, IndexingAction indAction, bool onlyReleasedTags)
        {
            _indexer.UpdateSingleDatasetIndex(datasetId, indAction, onlyReleasedTags);
            Reload();
        }
    }
}
