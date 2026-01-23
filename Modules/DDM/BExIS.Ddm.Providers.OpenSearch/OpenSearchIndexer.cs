using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using BExIS.Ddm.Api;
using BExIS.Ddm.Providers.OpenSearch.Config;
using BExIS.Ddm.Providers.OpenSearch.Config.enums;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using BExIS.Utils.Models;
using DocumentFormat.OpenXml.Drawing.Charts;
using OpenSearch.Client;
using OpenSearch.Net;
using Vaiona.Entities.Common;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;
using Vaiona.Utils.IO;

namespace BExIS.Ddm.Providers.OpenSearch
{
    public class OpenSearchIndexer : IDisposable
    {
        // TODO: kann vermutlich weg, da nicht genutzt?
        private List<Facet> _allFacets = new List<Facet>();
        private List<Property> _allProperties = new List<Property>();
        private List<Category> _allCategories = new List<Category>();
        private bool _reIndex = false;
        private bool _isIndexConfigured = false;
        private bool _includePrimaryData = false;

        // TODO: In Lucene waren das arrays, frage ist:
        // - sind listen ueberhaupt notwendig
        // - wie oft kommen Werte hinzu
        // - passiert das ueberhaupt?
        //public List<string> CategoryFieldList = new List<string>();
        //public List<string> PropertyFieldList = new List<string>();
        //public List<string> FacetFieldList = new List<string>();
        //public List<string> StoredFieldList = new List<string>();


        private EntityManager _entityManager;
        private EntityPermissionManager _entityPermissionManager;
        private long? _entityTypeId;

        // OpenSearch-Rest-Client
        private OpenSearchClient _client = OpenSearchProvider.Client;
        private string _mappingJson = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "OpenSearch", "mapping.json");
        //private string _configFilePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene", "Config", "LuceneConfig.xml");

        public OpenSearchIndexer()
        {
            _entityPermissionManager = new EntityPermissionManager();
            _entityManager = new EntityManager();
            _entityTypeId = _entityManager.FindByName(typeof(Dataset).Name)?.Id;
            _entityTypeId = _entityTypeId.HasValue ? _entityTypeId.Value : -1;


            //EnsureMappingMatches(OpenSearchProvider.DefaultIndex, _mappingJson);
        }

        public void EnsureMappingMatches(string indexName, string mappingJsonPath)
        {
            string mappingJson = File.ReadAllText(mappingJsonPath);
            var mappingDoc = JsonDocument.Parse(mappingJson);
            var desiredMapping = mappingDoc.RootElement.GetProperty("mappings");

            // Konvertiere Mapping zu Dictionary
            var desiredDict = JsonNode.Parse(desiredMapping.GetRawText());

            // Prüfe ob Index existiert
            var existsResponse = _client.Indices.Exists(indexName);
            if (!existsResponse.Exists)
            {
                Console.WriteLine($"Index '{indexName}' existiert nicht. Wird neu erstellt...");

                var createResponse = _client.LowLevel.Indices.Create<BytesResponse>(
                    indexName,
                    PostData.String(mappingJson)
                );

                if (!createResponse.Success)
                    throw new Exception($"Fehler beim Erstellen des Index: {createResponse.DebugInformation}");

                Console.WriteLine("Index erfolgreich erstellt.");
                return;
            }

            // Mapping abrufen
            var getMappingResponse = _client.Indices.GetMapping(new GetMappingRequest(indexName));
            if (!getMappingResponse.IsValid)
            {
                throw new Exception($"Mapping konnte nicht abgerufen werden: {getMappingResponse.OriginalException?.Message}");
            }

            var existingMappingJson = JsonSerializer.Serialize(getMappingResponse.Indices[indexName].Mappings);
            var existingDict = JsonNode.Parse(existingMappingJson);

            // Vergleichen
            if (JsonEquals(desiredDict, existingDict))
            {
                Console.WriteLine("Mapping ist identisch. Keine Aktion notwendig.");
            }
            else
            {
                Console.WriteLine("Mapping unterscheidet sich! manuell prüfen oder Reindex einleiten.");
                var response = _client.Indices.Delete(indexName);
                if (response.IsValid && response.Acknowledged)
                {
                    Console.WriteLine($"Index '{indexName}' is deleted!");
                }
            }
        }

        private bool JsonEquals(JsonNode a, JsonNode b)
        {
            return a != null && b != null && a.ToJsonString() == b.ToJsonString();
        }
        // extract infos from _configXML to member lists
        private void LoadBeforeIndexing()
        {
            var globalComponents = SearchConfigManager.GetAllComponents();
            Category category = new Category();
            category.Name = "All";
            category.Value = "All";
            category.DefaultValue = "nothing";
            _allCategories.Add(category);

            foreach (var item in globalComponents)
            {
                if (item.ComponentType == SearchComponentBaseType.Facet)
                {

                    Facet c = new Facet();
                    c.Name = item.ComponentName;
                    c.Text = item.ComponentName;
                    c.Value = item.ComponentName;
                    c.Childrens = new List<Facet>();
                    _allFacets.Add(c);
                }
                else if (item.ComponentType == SearchComponentBaseType.Property)
                {
                    // TODO: Not supported yet
                    //Property c = new Property();
                    //c.Name = item.Value.ComponentName;
                    //c.DisplayName = item.Value.ComponentName;
                    //var localComponent = SearchConfigManager.GetLocalSearchComponentById(item.Value.Id, item.Key);
                    //c.DataSourceKey = localComponent.MetadataNodes[0]; // TODO: expected a single string but there can be multi Nodes?!?

                    ////c.UIComponent = fieldProperty.Attributes.GetNamedItem("uiComponent").Value; ;
                    //c.AggregationType = "distinct";
                    //c.DefaultValue = "All";
                    //c.DataType = localComponent.DataTypeId.ToString();
                    //_allProperties.Add(c);
                }
                else if (item.ComponentType == SearchComponentBaseType.Category)
                {
                    //categoryXmlNodeList.Add(fieldProperty);
                    Category c = new Category();
                    c.Name = item.ComponentName;
                    c.Value = item.ComponentName;
                    c.DefaultValue = "nothing";
                    _allCategories.Add(c);
                }
                else if (item.ComponentType == SearchComponentBaseType.General)
                {
                    // TODO: redundant nach neuer SearchConfig?
                    //generalXmlNodeList.Add(fieldProperty);
                }
            }
        }

        private void ConfigureBexisIndexing(bool recreateIndex)
        {
            SearchConfigManager.Reload();

            LoadBeforeIndexing();
            _isIndexConfigured = true;
        }


        public void ReIndex(bool onlyReleasedTags)
        {
            _reIndex = true;
            Index(onlyReleasedTags);
            // Broadcast auf alle Providers?!?
            // TODO: Verify
            foreach (var weakRef in SearchProvider.Providers.Values.Where(p => p.IsAlive))
            {
                var provider = weakRef.Target as SearchProvider;
                provider?.Reload(); // Muss für OpenSearch angepasst werden
            }

            // OpenSearch kennt neue Daten erst nach Refresh
            var refreshResponse = _client.Indices.Refresh(OpenSearchProvider.DefaultIndex);
            if (!refreshResponse.IsValid)
            {
                // Fehlerbehandlung
                Console.WriteLine($"Fehler beim Refresh: {refreshResponse.ServerError}");
            }

            _reIndex = false;
        }

        public void Index(bool onlyReleasedTags)
        {
            // Method is only called when the user press the 'Refresh Search' button in 'Manage Search'
            // this will lead to delete all from index and write all back in refreshed index
            ConfigureBexisIndexing(true);

            List<string> errors = new List<string>();
            try
            {
                //ToDo only enitities from type dataset should be indexed in this index
                using (var publicationManager = new PublicationManager())
                using (DatasetManager dm = new DatasetManager())
                {
                    // if the system is using tags, then only datasets that have a tag and its released should be indexeds

                    IList<long> ids = new List<long>();
                    if (!onlyReleasedTags) ids = dm.GetDatasetLatestIds(); // index all datasets
                    else ids = dm.GetDatasetIdsWithTag(); // index only with tags

                    IList<long> ids_rev = ids.Reverse().ToList();

                    // TODO: Bulk von OpenSearch hier nutzen?
                    foreach (var id in ids_rev)
                    {
                        try
                        {
                            string entityTemplate = "";
                            string date = "";
                            DatasetVersion version = null;


                            // set system fields
                            if (version != null)
                            {
                                date = version.ModificationInfo?.Timestamp?.ToString("yyyy-MM-dd");
                                entityTemplate = version.Dataset.EntityTemplate.Name;
                            }

                            WriteBexisIndex(id, onlyReleasedTags, dm);
                            //GC.Collect();
                        }
                        catch (Exception ex)
                        {
                            errors.Add(string.Format("Enountered a probelm indexing dataset '{0}'. Details: {1}", id, ex.Message));
                        }
                    }
                    //GC.Collect();

                    if (errors.Count > 0)
                        throw new Exception(string.Join("\n\r", errors));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GC.Collect();
                using (var emailService = new EmailService())
                {
                    emailService.Send(MessageHelper.GetSearchReIndexHeader(),
                        MessageHelper.GetSearchReIndexMessage(errors),
                        GeneralSettings.SystemEmail);
                }

            }
        }


        /// <summary>
        /// Basierend auf in der Config deklarierten HeaderItems wird der Index durchsucht um die Inhalte auf Oberflaeche anzuzeigen zukoenne.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="headerItemList"></param>
        /// <returns></returns>
        public SearchResult Search(QueryContainer query, List<SearchComponentConfigObj> headerItemList)
        {
            int n = 0;
            DatasetManager dm = null;
            try
            {
                dm = new DatasetManager();
                n = dm.DatasetRepo.Get().Count;
                if (n == 0) n = 1000;
            }
            catch
            {
                n = 1000;
            }
            finally
            {
                dm.Dispose();
                if (n <= 0) n = 1000;
            }

            SearchResult search_res = new SearchResult();
            search_res.PageSize = 10;
            search_res.CurrentPage = 1;
            search_res.NumberOfHits = 100;

            List<HeaderItem> header = new List<HeaderItem>();
            List<HeaderItem> defaultHeader = new List<HeaderItem>();

            // create id
            HeaderItem id = new HeaderItem();
            id.DisplayName = "ID";
            id.Name = "ID";
            id.DataType = "Integer";
            search_res.Id = id;
            header.Add(id);
            defaultHeader.Add(id);

            // create entity
            HeaderItem entity = new HeaderItem();
            entity.DisplayName = "Type";
            entity.Name = "entity_name";
            entity.DataType = "string";
            entity.Placeholder = "entity";
            header.Add(entity);

            // create entitytemplate
            HeaderItem entitytemplate = new HeaderItem();
            entitytemplate.DisplayName = "Template";
            entitytemplate.Name = "entitytemplate";
            entitytemplate.DataType = "string";
            entitytemplate.Placeholder = "entitytemplate";
            header.Add(entitytemplate);

            // create date
            HeaderItem modifieddate = new HeaderItem();
            modifieddate.DisplayName = "Last modified date";
            modifieddate.Name = "modifieddate";
            modifieddate.DataType = "string";
            modifieddate.Placeholder = "date";
            header.Add(modifieddate);

            // create date
            HeaderItem headerItemDoi = new HeaderItem();
            headerItemDoi.DisplayName = "DOI";
            headerItemDoi.Name = "doi";
            headerItemDoi.DataType = "string";
            headerItemDoi.Placeholder = "doi";
            header.Add(headerItemDoi);

            //DefaultHeader.Add(entity);

            foreach (var item in headerItemList)
            {
                HeaderItem hi = new HeaderItem();
                hi.Name = item.ComponentName;
                hi.DisplayName = item.ComponentName;
                hi.Placeholder = item.Placeholder;
                header.Add(hi);

                if(item.DefaultHeaderItem)
                {
                    defaultHeader.Add(hi);
                }
            }

            List<Row> rowList = new List<Row>();
            string valueLastEntity = ""; // var to store last entity value
            bool moreThanOneEntityFound = false; // var to set, if more than one entity name was found

            // ---> Search in index
            // 1. OpenSearch-Query ausführen
            var searchResponse = _client.Search<Dictionary<string, object>>(s => s
                .Index(OpenSearchProvider.DefaultIndex)
                .Query(q => q.MatchAll())
                .Size(n) // Anzahl der Treffer, optional anpassen
            );


            foreach (var hit in searchResponse.Hits)
            {
                var source = hit.Source;
                Row r = new Row();
                List<object> ValueList = new List<object>();

                source.TryGetValue("doc_id", out var docId);
                source.TryGetValue("gen_entity_name", out var entityName);
                source.TryGetValue("gen_entitytemplate", out var template);
                source.TryGetValue("gen_modifieddate", out var modifiedDate);
                source.TryGetValue("gen_doi", out var doi);

                ValueList.Add(docId);
                ValueList.Add(entityName);
                ValueList.Add(template);
                ValueList.Add(modifiedDate);
                ValueList.Add(doi);

                if (!moreThanOneEntityFound && entityName?.ToString() != valueLastEntity && valueLastEntity != "")
                {
                    moreThanOneEntityFound = true;
                }
                valueLastEntity = entityName?.ToString();


                // Weitere dynamische Felder basierend auf XML-Konfiguration
                foreach (var item in headerItemList)
                {
                    string componentName = item.ComponentName;

                    if (item.ComponentType == SearchComponentBaseType.Facet)
                    {
                        componentName = "facet_" + item.ComponentName;
                    }
                    else if (item.ComponentType == SearchComponentBaseType.Category)
                    {
                        componentName = "category_" + item.ComponentName;
                    }
                    else if (item.ComponentType == SearchComponentBaseType.Property)
                    {
                        componentName = "property_" + item.ComponentName;
                    }

                    if (source.TryGetValue(componentName, out var fieldValues))
                    {
                        if (fieldValues is IEnumerable<object> multipleValues)
                        {
                            ValueList.Add(string.Join(", ", multipleValues.Select(v => v.ToString())));
                        }
                        else
                        {
                            ValueList.Add(fieldValues?.ToString());
                        }
                    }
                    else
                    {
                        // TODO: Verify is necessary
                        ValueList.Add(""); // Leerer Eintrag wenn Feld nicht vorhanden
                    }
                }

                r.Values = ValueList;
                rowList.Add(r); // Liste mit deinen Rows
            }
            if (moreThanOneEntityFound == true)
            {
                defaultHeader.Add(entity);
            }

            search_res.Header = header;
            search_res.DefaultVisibleHeaderItem = defaultHeader;
            search_res.Rows = rowList;

            return search_res;
        }

        public IEnumerable<Facet> FacetSearch(QueryContainer query, IEnumerable<Facet> facets,
            int sizePerFacet = 100           // max. Bucket‑Größe (entspricht Lucenes Treffer‑Limit)
        )
        {
            var result = new List<Facet>();

            foreach (var f in facets)
            {
                // 1. Suche + Aggregation für genau EIN Facettenfeld
                string facetField = $"facet_{f.Name}.keyword"; // .keyword wichtig wegen Aggregation!!!
                string aggName = $"facet_{f.Name}";              // Aggregationsname = Feldname

                var searchResponse = _client.Search<dynamic>(s => s
                    .Index(OpenSearchProvider.DefaultIndex)     // TODO: DefaultIndex anders behandeln
                    .Query(q => query)                         // Filter (= Lucene‑Query)
                    .Size(0)                                   // reine Aggregationsabfrage
                    .Aggregations(a => a
                        .Terms(aggName, t => t
                            .Field(facetField)
                            .Size(sizePerFacet)
                            // Top‑Hits liefern Dokumente je Bucket – ersetzt hpg.Documents
                            .Aggregations(aa => aa
                                .TopHits("docs", th => th.Size(sizePerFacet))
                            )
                        )
                    )
                );

                // 2. Haupt‑Facet‑Objekt aufbauen
                var facetObj = new Facet
                {
                    Name = f.Name,
                    Text = f.Text,
                    Value = f.Value,
                    DisplayName = f.DisplayName,
                    Childrens = new List<Facet>()
                };

                int childCount = 0;

                var buckets = searchResponse
                    .Aggregations
                    .Terms(aggName)?
                    .Buckets
                    ?? new List<KeyedBucket<string>>();


                foreach (var bucket in buckets)
                {
                    var bucketKey = bucket.Key?.ToString() ?? string.Empty;
                    var bucketCount = (int)bucket.DocCount;

                    if (string.IsNullOrWhiteSpace(bucketKey)) continue;

                    // 3. Child‑Facet aus dem Bucket erzeugen
                    var cc = new Facet
                    {
                        Name = bucketKey,
                        Text = bucketKey,
                        Value = bucketKey,
                        Count = bucketCount
                    };

                    if (bucketCount > 0) childCount++;

                    // 4. Zusatz‑Logik: XML‑Konfig und Werte einzelner Dokumente durchgehen
                    //    (ersetzt das Schleifen­konstrukt über hpg.Documents)

                    // Top‑Hits‑Dokumente des Buckets
                    var docs = bucket.TopHits("docs")?.Documents<dynamic>() ?? Enumerable.Empty<dynamic>();

                    foreach (var facet in SearchConfigManager.GetGlobalSearchComponent().Facets)
                    {
                        // search for facet in possible multiple entityTemplates
                        var allLocalFacets = SearchConfigManager.GetAllLocalSearchComponentsById(facet.Id, "facet");

                        foreach (var localFacet in allLocalFacets)
                        {

                            var primType = localFacet.DataTypeId;
                            var displayName = facet.ComponentName;

                            if (primType != DataTypeId.Date &&
                                primType != DataTypeId.DateNanos &&
                                primType != DataTypeId.Text &&
                                displayName.Contains(facetObj.DisplayName.ToLower()))
                            {
                                foreach (var doc in docs)
                                {
                                    if (doc is IDictionary<string, object> dict &&
                                        dict.TryGetValue(facetField, out var raw))
                                    {
                                        // raw kann Single‑ oder Multi‑Value sein
                                        IEnumerable<string> values;

                                        if (raw is IEnumerable<object> col)
                                        {
                                            values = col.Select(v => v?.ToString());
                                        }
                                        else
                                        {
                                            values = new[] { raw?.ToString() };
                                        }


                                        foreach (var val in values.Where(v => !string.IsNullOrEmpty(v)))
                                        {
                                            if (!facetObj.Childrens.Any(x => x.Name == val))
                                            {
                                                facetObj.Childrens.Add(new Facet
                                                {
                                                    Name = val,
                                                    Text = val,
                                                    Value = val
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    

                    facetObj.Childrens.Add(cc);   // analog zum Original immer hinzufügen
                }

                facetObj.Count = childCount;
                result.Add(facetObj);
            }

            return result;
        }

        public IEnumerable<Property> PropertySearch(QueryContainer query, IEnumerable<Property> properties)
        {
            foreach (var property in properties)
            {
                string fieldName = $"property_{property.Name}.keyword";

                var searchResponse = _client.Search<dynamic>(s => s
                    .Index(OpenSearchProvider.DefaultIndex)
                    .Size(0)
                    .Query(q => query) // bereits vorbereiteter QueryContainer
                    .Aggregations(agg => agg
                        .Terms("agg", t => t
                            .Field(fieldName)
                            .Size(1000)
                        )
                    )
                );

                var buckets = searchResponse.Aggregations.Terms("agg")?.Buckets
                              ?? Array.Empty<KeyedBucket<string>>();

                var values = buckets
                    .Where(b => !string.IsNullOrEmpty(b.Key))
                    .Select(b => b.Key)
                    .ToList();

                property.Values = values;
            }

            return properties;
        }


        public IEnumerable<TextValue> DoTextSearch(QueryContainer origQuery, string queryFilter, string searchText)
        {
            List<TextValue> result = new List<TextValue>();
            ISearchResponse<AutoCompleteDocument> response = null;

            if (queryFilter == "all")
            {
                response = _client.Search<AutoCompleteDocument>(s => s
                    .Index(OpenSearchProvider.AutoCompleteIndex)
                    .Suggest(su => su
                        .Completion("autocomplete_suggest", cs => cs
                            .Field(f => f.All)
                            .Prefix(searchText)
                            .Size(10)
                        )
                    )
                );
            }
            else
            {
                response = _client.Search<AutoCompleteDocument>(s => s
                    .Index(OpenSearchProvider.AutoCompleteIndex)
                    .Suggest(su => su
                        .Completion("autocomplete_suggest", cs => cs
                            .Field(f => f.Suggest)
                            .Prefix(searchText)
                            .Size(10)
                            .Contexts(c => c
                                .Context("category", cc => cc.Context(queryFilter)
                                )
                            )
                        )
                    )
                );
            }
            if (response != null)
            {
                var suggestions = response.Suggest["autocomplete_suggest"]
                    .SelectMany(sm => sm.Options)
                    .GroupBy(gb => gb.Text) // filter duplikate
                    .Select(s => s.First())
                    .Take(7)
                    .ToList();

                foreach (var opt in suggestions)
                {
                    TextValue tv = new TextValue
                    {
                        Name = opt.Text,
                        Value = opt.Text,
                    };
                    result.Add(tv);
                }

            }
            return result;
        }

        public void UpdateIndex(Dictionary<long, IndexingAction> datasetsToIndex, bool onlyReleasedTags)
        {
            using (DatasetManager dm = new DatasetManager())
            {
                try
                {
                    if (!_isIndexConfigured)
                    {
                        this.ConfigureBexisIndexing(false);
                    }
                    foreach (KeyValuePair<long, IndexingAction> pair in datasetsToIndex)
                    {
                        DatasetVersion version = dm.GetDatasetLatestVersion(pair.Key);
                        string entityTemplate = "";
                        string date = "";

                        if (version != null)
                        {
                            // doi
                            entityTemplate = version.Dataset.EntityTemplate.Name;
                            date = version.ModificationInfo?.Timestamp?.ToString("yyyy-MM-dd");
                        }

                        // check if index already exists
                        var existsResponse = _client.DocumentExists<object>(pair.Key, idx =>
                            idx.Index(OpenSearchProvider.DefaultIndex)
                        );

                        if (pair.Value == IndexingAction.CREATE)
                        {
                            if (existsResponse.Exists)
                            {
                                _client.Delete<object>(pair.Key, idx => idx.Index(OpenSearchProvider.DefaultIndex));
                                _client.Delete<object>(pair.Key, idx => idx.Index(OpenSearchProvider.AutoCompleteIndex));
                            }
                            WriteBexisIndex(pair.Key, onlyReleasedTags, dm);
                        }
                        else if (pair.Value == IndexingAction.UPDATE)
                        {
                            if (existsResponse.Exists)
                            {
                                _client.Delete<object>(pair.Key, idx => idx.Index(OpenSearchProvider.DefaultIndex));
                                _client.Delete<object>(pair.Key, idx => idx.Index(OpenSearchProvider.AutoCompleteIndex));

                            }
                            WriteBexisIndex(pair.Key, onlyReleasedTags, dm);
                        }
                        else if (pair.Value == IndexingAction.DELETE)
                        {
                            _client.Delete<object>(pair.Key, idx => idx.Index(OpenSearchProvider.DefaultIndex));
                            _client.Delete<object>(pair.Key, idx => idx.Index(OpenSearchProvider.AutoCompleteIndex));

                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void UpdateSingleDatasetIndex(long datasetId, IndexingAction indAction, bool onlyReleasedTags)
        {
            using (DatasetManager dm = new DatasetManager())
            {
                try
                {
                    if (!_isIndexConfigured)
                    {
                        ConfigureBexisIndexing(false);
                    }

                    if (indAction == IndexingAction.CREATE)
                    {
                        // 1. Suche nach dem Dokument mit der passenden ID
                        var searchResponse = _client.Search<object>(s => s
                            .Index(OpenSearchProvider.DefaultIndex) // TODO: defaultindex
                            .Query(q => q
                                .Term(t => t
                                    .Field("doc_id")
                                    .Value(datasetId.ToString())
                                )
                            )
                            .Size(1)
                        );
                        _includePrimaryData = false;

                        if (!searchResponse.IsValid)
                        {
                            // Loggen oder Fehlerbehandlung bei Verbindungsproblemen
                            throw new Exception($"OpenSearch Query failed: {searchResponse.OriginalException?.Message}");
                        }

                        // 2. Wenn kein Treffer: neu schreiben
                        if (searchResponse.Total < 1)
                        {
                            WriteBexisIndex(datasetId, onlyReleasedTags, dm);
                        }
                        else
                        {
                            // 3. Existiert: zuerst löschen
                            var deleteMain = _client.DeleteByQuery<object>(d => d
                                .Index(OpenSearchProvider.DefaultIndex) // TODO
                                .Query(q => q
                                    .Term(t => t
                                        .Field("doc_id")
                                        .Value(datasetId.ToString())
                                    )
                                )
                            );

                            var deleteAutoComplete = _client.DeleteByQuery<object>(d => d
                                .Index(OpenSearchProvider.AutoCompleteIndex) // TODO
                                .Query(q => q
                                    .Term(t => t
                                        .Field("id")
                                        .Value(datasetId.ToString())
                                    )
                                )
                            );

                            if (!deleteMain.IsValid || !deleteAutoComplete.IsValid)
                            {
                                throw new Exception("Fehler beim Löschen aus einem der Indizes.");
                            }

                            // 4. Neu schreiben
                            WriteBexisIndex(datasetId, onlyReleasedTags, dm);
                        }
                    }
                    else if (indAction == IndexingAction.DELETE)
                    {
                        var deleteMain = _client.DeleteByQuery<object>(d => d
                            .Index(OpenSearchProvider.DefaultIndex) // TODO
                            .Query(q => q
                                .Term(t => t
                                    .Field("doc_id")
                                    .Value(datasetId.ToString())
                                )
                            )
                        );

                        var deleteAutoComplete = _client.DeleteByQuery<object>(d => d
                            .Index(OpenSearchProvider.AutoCompleteIndex) // TODO
                            .Query(q => q
                                .Term(t => t
                                    .Field("id")
                                    .Value(datasetId.ToString())
                                )
                            )
                        );

                        if (!deleteMain.IsValid || !deleteAutoComplete.IsValid)
                        {
                            throw new Exception("Fehler beim Löschen aus einem der Indizes.");
                        }
                    }
                    else if (indAction == IndexingAction.UPDATE)
                    {
                        var deleteMain = _client.DeleteByQuery<object>(d => d
                            .Index(OpenSearchProvider.DefaultIndex) // TODO
                            .Query(q => q
                                .Term(t => t
                                    .Field("doc_id")
                                    .Value(datasetId.ToString())
                                )
                            )
                        );

                        var deleteAutoComplete = _client.DeleteByQuery<object>(d => d
                            .Index(OpenSearchProvider.AutoCompleteIndex) // TODO
                            .Query(q => q
                                .Term(t => t
                                    .Field("id")
                                    .Value(datasetId.ToString())
                                )
                            )
                        );

                        if (!deleteMain.IsValid || !deleteAutoComplete.IsValid)
                        {
                            throw new Exception("Fehler beim Löschen aus einem der Indizes.");
                        }

                        // 4. Neu schreiben
                        WriteBexisIndex(datasetId, onlyReleasedTags, dm);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {

                }
            }
        }

        private void WriteBexisIndex(long id, bool onlyReleasedTags, DatasetManager dm)
        {
            string docId = id.ToString();//metadata.GetElementsByTagName("bgc:id")[0].InnerText;
            string doi = "";
            string date = "";
            string entityTemplate = "";
            DatasetVersion version = null;
            XmlDocument metadata = null;

            if (onlyReleasedTags)
            {
                var latestTag = dm.GetLatestTag(id, true);
                if (latestTag != null)
                {
                    version = dm.GetLatestVersionByTagNr(id, latestTag.Nr);
                    metadata = version.Metadata;
                }
            }
            else
            {
                version = dm.GetDatasetLatestVersion(id);
                metadata = dm.GetDatasetLatestMetadataVersion(id);
            }

            if (version != null)
            {
                // doi
                entityTemplate = version.Dataset.EntityTemplate.Name;
                date = version.ModificationInfo?.Timestamp?.ToString("yyyy-MM-dd");
                if (date == null) version.CreationInfo?.Timestamp?.ToString("yyyy-MM-dd");
                if (date == null) date = "";
            }
            List<XmlNode> faceNodes = new List<XmlNode>();

            //List<XmlNode> faceNodes = facetXmlNodeList;
            //XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            //var entityName = xmlDatasetHelper.GetEntityName(id);
            var entityName = "";

            // Doc to Index
            List<string> ng_all_list = new List<string>();
            Dictionary<string, object> doc = new Dictionary<string, object>
            {
                ["doc_id"] = docId,
                ["gen_isPublic"] = _entityPermissionManager.ExistsAsync(_entityTypeId.Value, id).Result,
                ["gen_entity_name"] = entityName,
                ["gen_modifieddate"] = date,
                ["gen_entitytemplate"] = entityTemplate,
                ["ng_all"] = ng_all_list
            };

            // autocomplete to Index
            List<string> autocomplete_ng_all_list = new List<string>();

            // autocomplete index new
            var autocompleteDoc = new AutoCompleteDocument(docId);


            IndexSystemInfos(id, doi, entityName, entityTemplate, doc, docId, autocompleteDoc);

            foreach (XmlNode facet in faceNodes)
            {
                string multivalued = facet.Attributes.GetNamedItem("multivalued").Value;
                string[] metadataElementNames = facet.Attributes.GetNamedItem("metadata_name").Value.Split(',');
                string lucene_name = facet.Attributes.GetNamedItem("lucene_name").Value;

                // holt Elemente aus der Datenbank
                foreach (string metadataElementName in metadataElementNames)
                {
                    if (!string.IsNullOrEmpty(metadataElementName))
                    {
                        XmlNodeList elemList = metadata.SelectNodes(metadataElementName);
                        if (elemList != null)
                        {
                            for (int i = 0; i < elemList.Count; i++)
                            {
                                string eleme = elemList[i].InnerText;
                                if (!elemList[i].InnerText.Trim().Equals(""))
                                {
                                    // write_data_facet()-Method in lucene BexisIndexer.cs
                                    AppendFacetDataToDocument(facet, elemList[i].InnerText.Trim(), doc, docId, metadataElementName, ng_all_list, autocompleteDoc);
                                }
                            }
                        }
                    }
                }
            } // end foreach metadataElementNames
            List<XmlNode> propertyNodes = new List<XmlNode>();

            //List<XmlNode> propertyNodes = propertyXmlNodeList;
            foreach (XmlNode property in propertyNodes)
            {
                string multivalued = property.Attributes.GetNamedItem("multivalued").Value;
                string lucene_name = property.Attributes.GetNamedItem("lucene_name").Value;
                string[] metadataElementNames = property.Attributes.GetNamedItem("metadata_name").Value.Split(',');

                foreach (string metadataElementName in metadataElementNames)
                {
                    XmlNodeList elemList = metadata.SelectNodes(metadataElementName);
                    if (elemList != null)
                    {
                        string primitiveType = property.Attributes.GetNamedItem("primitive_type").Value;
                        if (elemList[0] != null)
                        {
                            if (primitiveType.ToLower().Equals("string"))
                            {
                                // indexing
                                doc["property_" + lucene_name] = elemList[0].InnerText;
                                ng_all_list.Add(elemList[0].InnerText);
                                //doc["ng_all"] = elemList[0].InnerText;

                                // autocomplete
                                autocompleteDoc.AddCategory(elemList[0].InnerText, lucene_name);
                                //AddAutoCompleteDocument(autocompleteDoc, autocomplete_ng_all_list, lucene_name, elemList[0].InnerText);
                                //WriteAutoCompleteIndex(docId, lucene_name, elemList[0].InnerText);
                                //WriteAutoCompleteIndex(docId, "ng_all", elemList[0].InnerText);
                            }
                            else if (primitiveType.ToLower().Equals("date"))
                            {
                                DateTime MyDateTime = new DateTime();

                                if (DateTime.TryParse(elemList[0].InnerText, out MyDateTime))
                                {
                                    long t = MyDateTime.Ticks;

                                    doc["property_numeric_" + lucene_name] = MyDateTime.Ticks;
                                    string dateToString = MyDateTime.Date.ToString("d", CultureInfo.CreateSpecificCulture("en-US"));
                                    doc["property_" + lucene_name] = dateToString;
                                    autocompleteDoc.AddCategory(MyDateTime.Date.ToString(), lucene_name);
                                    //AddAutoCompleteDocument(autocompleteDoc, autocomplete_ng_all_list, lucene_name, MyDateTime.Date.ToString());
                                    //WriteAutoCompleteIndex(docId, lucene_name, MyDateTime.Date.ToString());
                                    //WriteAutoCompleteIndex(docId, "ng_all", MyDateTime.Date.ToString());
                                }
                            }
                            else if (primitiveType.ToLower().Equals("integer"))
                            {
                                doc["property_numeric" + lucene_name] = Convert.ToInt32(elemList[0].InnerText);
                                doc["property_" + lucene_name] = elemList[0].InnerText;
                                //  writeAutoCompleteIndex(lucene_name, elemList[0].InnerText);
                            }
                            else if (primitiveType.ToLower().Equals("double"))
                            {
                                doc["property_numeric" + lucene_name] = Convert.ToDouble(elemList[0].InnerText);
                                doc["property_" + lucene_name] = elemList[0].InnerText;
                                autocompleteDoc.AddCategory(elemList[0].InnerText, lucene_name);
                                //AddAutoCompleteDocument(autocompleteDoc, autocomplete_ng_all_list, lucene_name, elemList[0].InnerText);
                                //WriteAutoCompleteIndex(docId, lucene_name, elemList[0].InnerText);
                                //WriteAutoCompleteIndex(docId, "ng_all", elemList[0].InnerText);
                            }
                        }
                    }
                }
            } // end foreach propertyNodes
            List<XmlNode> categoryNodes = new List<XmlNode>();
            //List<XmlNode> categoryNodes = categoryXmlNodeList;

            // add categories to index
            foreach (XmlNode category in categoryNodes)
            {
                string primitiveType = category.Attributes.GetNamedItem("primitive_type").Value;
                string lucene_name = category.Attributes.GetNamedItem("lucene_name").Value;
                string analysing = category.Attributes.GetNamedItem("analysed").Value;
                float boosting = Convert.ToSingle(category.Attributes.GetNamedItem("boost").Value);

                if (!category.Attributes.GetNamedItem("type").Value.Equals("primary_data_field"))
                {
                    string multivalued = category.Attributes.GetNamedItem("multivalued").Value;
                    string storing = category.Attributes.GetNamedItem("store").Value;

                    string[] metadataElementNames = category.Attributes.GetNamedItem("metadata_name").Value.Split(',');

                    foreach (string metadataElementName in metadataElementNames)
                    {
                        XmlNodeList elemList = metadata.SelectNodes(metadataElementName);
                        if (elemList != null)
                        {
                            for (int i = 0; i < elemList.Count; i++)
                            {
                                doc["category_" + lucene_name] = elemList[i].InnerText;
                                doc["ng_" + lucene_name] = elemList[i].InnerText;
                                ng_all_list.Add(elemList[i].InnerText);
                                //doc["ng_all"] = elemList[i].InnerText;
                                autocompleteDoc.AddCategory(elemList[i].InnerText, lucene_name);
                                //AddAutoCompleteDocument(autocompleteDoc, autocomplete_ng_all_list, lucene_name, elemList[i].InnerText);
                                //WriteAutoCompleteIndex(docId, lucene_name, elemList[i].InnerText);
                                //WriteAutoCompleteIndex(docId, "ng_all", elemList[i].InnerText);
                            }
                        }
                    }
                }
                else
                {
                    //if the primary data index exist in the config - this means the primary data should be indexed
                    _includePrimaryData = true;
                }
            } // end foreach categoryNodes
            IndexPrimaryData(id, categoryNodes, doc, docId, metadata, ng_all_list, autocompleteDoc);

            //List<XmlNode> generalNodes = generalXmlNodeList;
            List<XmlNode> generalNodes = new List<XmlNode>();

            foreach (XmlNode general in generalNodes)
            {
                string multivalued = general.Attributes.GetNamedItem("multivalued").Value;
                string primitiveType = general.Attributes.GetNamedItem("primitive_type").Value;
                string lucene_name = general.Attributes.GetNamedItem("lucene_name").Value;

                string storing = general.Attributes.GetNamedItem("store").Value;
                string analysing = general.Attributes.GetNamedItem("analysed").Value;

                float boosting = Convert.ToSingle(general.Attributes.GetNamedItem("boost").Value);

                string[] metadataElementNames = general.Attributes.GetNamedItem("metadata_name").Value.Split(',');

                foreach (string metadataElementName in metadataElementNames)
                {
                    XmlNodeList elemList = metadata.SelectNodes(metadataElementName);
                    for (int i = 0; i < elemList.Count; i++)
                    {
                        if (!elemList[i].InnerText.Trim().Equals(""))
                        {
                            doc[lucene_name] = elemList[i].InnerText;
                            ng_all_list.Add(elemList[i].InnerText);
                            //doc["ng_all"] = elemList[i].InnerText;
                            autocompleteDoc.AddCategory(elemList[i].InnerText, lucene_name);
                            //AddAutoCompleteDocument(autocompleteDoc, autocomplete_ng_all_list, lucene_name, elemList[i].InnerText);

                            //WriteAutoCompleteIndex(docId, lucene_name, elemList[i].InnerText);
                            //WriteAutoCompleteIndex(docId, "ng_all", elemList[i].InnerText);
                        }
                    }
                }
            }

            WriteAutocompleteIndex(docId, autocompleteDoc);

            var indexResponse = _client.Index(doc, i => i
                .Index(OpenSearchProvider.DefaultIndex)
                .Id(doc["doc_id"].ToString())
            );
        }

        private void WriteAutocompleteIndex(string docId, AutoCompleteDocument autocompleteDoc)
        {
            var indexName = OpenSearchProvider.AutoCompleteIndex;
            autocompleteDoc.WithAllCompletion(); // TODO: dirty hack
            var response = _client.Index(autocompleteDoc, i => i.Index(indexName).Id(docId));
            if (!response.IsValid)
            {
                // TODO: Loggen oder Exception werfen - erfragen!
                Console.WriteLine($"Fehler beim Indexieren von Autocomplete-Dokument: {response.ServerError}");
            }
        }

        private void AppendFacetDataToDocument(XmlNode facet, object val, Dictionary<string, object> doc, string docId, string variableNodeLabel, List<string> ng_all, AutoCompleteDocument autocompleteDoc)
        {
            string name = facet.Attributes.GetNamedItem("lucene_name").Value;

            if (facet.Attributes.GetNamedItem("primitive_type")?.Value.ToLower() == "string")
            {
                doc["facet_" + name] = val.ToString();
                ng_all.Add(val.ToString());
                autocompleteDoc.AddCategory(val.ToString(), name);
                //AddAutoCompleteDocument(autocompleteDoc, autocompleteNgAllList, name, val.ToString());
                //WriteAutoCompleteIndex(docId, name, val.ToString());
                //WriteAutoCompleteIndex(docId, "ng_all", val.ToString());
            }
            else
            {
                if (facet.Attributes.GetNamedItem("primitiv_type")?.Value.ToLower() == "date")
                {
                    DateTime dateValue = DateTime.MinValue;
                    DateTime dateValue_ = dateValue;

                    DateTime.TryParse(val.ToString(), out dateValue);
                    if ((dateValue != dateValue_) && (!string.IsNullOrEmpty(val.ToString())))
                    {
                        string formattedDate = dateValue.ToString("yyyy-MM-dd");
                        doc["facet_" + name] = formattedDate;
                    }
                    else
                    {
                        DebugEmptyNodes(variableNodeLabel, " ", docId);
                    }
                }
                else if ((facet.Attributes.GetNamedItem("primitive_type").Value.ToLower().Contains("int")) ||
                      (facet.Attributes.GetNamedItem("primitive_type").Value.ToLower().Contains("double")) ||
                      (facet.Attributes.GetNamedItem("primitive_type").Value.ToLower().Contains("decimal")) ||
                      (facet.Attributes.GetNamedItem("primitive_type").Value.ToLower().Contains("number")))
                {
                    if (!string.IsNullOrEmpty(val.ToString()))
                        doc["facet_" + name] = double.Parse(val.ToString());
                    else
                    {
                        DebugEmptyNodes(variableNodeLabel, " ", docId);
                    }
                }
            }
        }

        private void DebugEmptyNodes(string extractedNodePath, string type, string datasetId)
        {
            LoggerFactory.GetFileLogger().LogCustom("==> Dataset ID: " + datasetId + " has Empty value for the " + type + " named : " + extractedNodePath);

        }

        private List<string> GetListOfValuesFromDataStructure(StructuredDataStructure sds)
        {
            List<string> tmp = new List<string>();

            foreach (var variable in sds.Variables)
            {
                if (variable.VariableTemplate != null)
                {
                    tmp.Add(variable.VariableTemplate.Label);
                    if (!string.IsNullOrEmpty(variable.VariableTemplate.Description) && variable.VariableTemplate.Description != "Unknown")
                        tmp.Add(variable.VariableTemplate.Description);
                }

                tmp.Add(variable.Label);
                tmp.Add(variable.Description);
            }

            return tmp;
        }
        private List<string> GetAllStringValuesFromTable(System.Data.DataTable dataTable)
        {
            List<string> tmp = new List<string>();

            // get list of index, where a itemarray is a string
            List<int> indexes = new List<int>();

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                DataColumn dc = dataTable.Columns[i];
                if (dc.DataType.Equals(typeof(string))) indexes.Add(i);
            }

            foreach (var index in indexes)
            {
                tmp.AddRange(dataTable.AsEnumerable().Select(s => s.Field<string>(index)).ToArray<string>());
            }

            tmp = tmp.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

            return tmp;
        }

        private void IndexStructureDataStructcure(StructuredDataStructure sds, Dictionary<string, object> doc, string docId, AutoCompleteDocument autoCompleteDoc)
        {
            if (sds == null)
                return;

            List<string> sdsStrings = GetListOfValuesFromDataStructure(sds);

            String primitiveType = "string";
            String lucene_name = "data_structure_field";
            String analysing = "yes";
            float boosting = 3;

            foreach (string pDataValue in sdsStrings)
            // Loop through List with foreach
            {
                if (string.IsNullOrEmpty(pDataValue))
                {
                    doc["category_" + lucene_name] = pDataValue;
                    doc["ng_" + lucene_name] = pDataValue;
                    doc["ng_all"] = pDataValue;
                    autoCompleteDoc.AddCategory(pDataValue, lucene_name);
                    //WriteAutoCompleteIndex(docId, lucene_name, pDataValue);
                    //WriteAutoCompleteIndex(docId, "ng_all", pDataValue);
                }
            }
        }

        private void IndexPrimaryData(long id, List<XmlNode> categoryNodes, Dictionary<string, object> doc, string docId, XmlDocument metadata, List<string> ng_all, AutoCompleteDocument autocompleteDoc)
        {
            using (DatasetManager dm = new DatasetManager())
            using (DataStructureManager dsm = new DataStructureManager())
            {
                if (!dm.IsDatasetCheckedIn(id))
                    return;

                DatasetVersion dsv = dm.GetDatasetLatestVersion(id);

                // if dataset has no structure -> nothing to do
                if (dsv.Dataset.DataStructure == null) return;

                StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);
                if (sds == null)
                    return;

                IndexStructureDataStructcure(sds, doc, docId, autocompleteDoc);

                if (!_includePrimaryData)
                    return;

                // Javad: check if the dataset is "checked-in". If yes, then use the paging version of the GetDatasetVersionEffectiveTuples method
                // number of tuples for the for loop is also available via GetDatasetVersionEffectiveTupleCount
                // a proper fetch (page) size can be obtained by calling dm.PreferedBatchSize
                int fetchSize = dm.PreferedBatchSize;
                long tupleSize = dm.GetDatasetVersionEffectiveTupleCount(dsv);
                long noOfFetchs = tupleSize / fetchSize + 1;

                if (tupleSize > 0)
                {
                    for (int round = 0; round < noOfFetchs; round++)
                    {
                        List<string> primaryDataStringToindex = null;
                        using (System.Data.DataTable table = dm.GetLatestDatasetVersionTuples(dsv.Dataset.Id, round, fetchSize))
                        {
                            primaryDataStringToindex = GetAllStringValuesFromTable(table); // should take the table
                            table.Dispose();
                        }

                        foreach (XmlNode category in categoryNodes)
                        {
                            String primitiveType = category.Attributes.GetNamedItem("primitive_type").Value;
                            String lucene_name = category.Attributes.GetNamedItem("lucene_name").Value;
                            String analysing = category.Attributes.GetNamedItem("analysed").Value;
                            float boosting = Convert.ToSingle(category.Attributes.GetNamedItem("boost").Value);

                            if (category.Attributes.GetNamedItem("type").Value.Equals("primary_data_field"))
                            {
                                if (primaryDataStringToindex != null && primaryDataStringToindex.Count > 0)
                                {
                                    primaryDataStringToindex = primaryDataStringToindex.Distinct().ToList();
                                    foreach (string pDataValue in primaryDataStringToindex)
                                    // Loop through List with foreach
                                    {
                                        doc["category_" + lucene_name] = pDataValue;
                                        doc["ng_" + lucene_name] = pDataValue;
                                        ng_all.Add(pDataValue);
                                        //doc["ng_all"] = pDataValue;

                                        autocompleteDoc.AddCategory(pDataValue, lucene_name);
                                        //WriteAutoCompleteIndex(docId, lucene_name, pDataValue);
                                        //WriteAutoCompleteIndex(docId, "ng_all", pDataValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void IndexSystemInfos(long id, string doi, string entity, string template, Dictionary<string, object> doc, string docId, AutoCompleteDocument autocompleteDoc)
        {
            // ID
            string idStr = id.ToString();
            doc["category_id"] = idStr;
            doc["ng_id"] = idStr;
            doc["ng_all"] = idStr;
            autocompleteDoc.AddCategory(idStr, "category_id");
            //WriteAutoCompleteIndex(docId, "id", idStr);
            //WriteAutoCompleteIndex(docId, "ng_all", idStr);

            // DOI
            doc["category_doi"] = doi;
            doc["ng_doi"] = doi;
            doc["ng_all"] = doi;
            autocompleteDoc.AddCategory(doi, "category_doi");
            //AddAutoCompleteDocument(autocompleteDoc, autocompleteNgAllList, "doi", doi);
            //WriteAutoCompleteIndex(docId, "ng_all", doi);

            // Entity
            doc["category_entity"] = entity;
            doc["ng_entity"] = entity;
            doc["ng_all"] = entity;
            autocompleteDoc.AddCategory(entity, "category_entity");

            //AddAutoCompleteDocument(autocompleteDoc, autocompleteNgAllList, "entity", entity);
            //WriteAutoCompleteIndex(docId, "ng_all", entity);

            // Template
            doc["category_template"] = template;
            doc["ng_template"] = template;
            doc["ng_all"] = template;
            autocompleteDoc.AddCategory(template, "category_template");
            //AddAutoCompleteDocument(autocompleteDoc, autocompleteNgAllList, "template", template);
            //WriteAutoCompleteIndex(docId, "ng_all", template);

        }

        public void Dispose()
        {
        }
    }
}
