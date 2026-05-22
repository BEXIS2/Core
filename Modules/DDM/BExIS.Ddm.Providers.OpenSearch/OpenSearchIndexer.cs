using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
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
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using OpenSearch.Client;
using OpenSearch.Net;
using Vaiona.Entities.Common;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;
using Vaiona.Utils.IO;
using Category = BExIS.Utils.Models.Category;
using DataTable = System.Data.DataTable;

namespace BExIS.Ddm.Providers.OpenSearch
{
    public class OpenSearchIndexer : IDisposable
    {
        // TODO: kann vermutlich weg, da nicht genutzt?
        //private List<Facet> _allFacets = new List<Facet>();
        //private List<Property> _allProperties = new List<Property>();
        //private List<Category> _allCategories = new List<Category>();
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

            var existingMappingJson = System.Text.Json.JsonSerializer.Serialize(getMappingResponse.Indices[indexName].Mappings);
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
            //_allCategories.Add(category);

            foreach (var item in globalComponents)
            {
                if (item.ComponentType == SearchComponentBaseType.Facet)
                {

                    Facet c = new Facet();
                    c.Name = item.ComponentName;
                    c.Text = item.ComponentName;
                    c.Value = item.ComponentName;
                    c.Childrens = new List<Facet>();
                    //_allFacets.Add(c);
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
                    //_allCategories.Add(c);
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
                            WriteToIndex(id, onlyReleasedTags, dm);
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
                //GC.Collect();
                //using (var emailService = new EmailService())
                //{
                //    emailService.Send(MessageHelper.GetSearchReIndexHeader(),
                //        MessageHelper.GetSearchReIndexMessage(errors),
                //        GeneralSettings.SystemEmail);
                //}

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
                .Query(q => query)
                .Size(n)
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

        public SearchResult SearchWithGeo(
            QueryContainer baseQuery,
            List<SearchComponentConfigObj> headerItemList,
            double? latitude = null,
            double? longitude = null,
            double? radiusKm = null,
            double? topLeftLat = null,
            double? topLeftLon = null,
            double? bottomRightLat = null,
            double? bottomRightLon = null)
        {
            int n = 1000;

            SearchResult search_res = new SearchResult
            {
                PageSize = 10,
                CurrentPage = 1,
                NumberOfHits = 100
            };

            var header = new List<HeaderItem>();
            var defaultHeader = new List<HeaderItem>();

            // Standard Header
            HeaderItem id = new HeaderItem();
            id.DisplayName = "ID";
            id.Name = "ID";
            id.DataType = "Integer";
            search_res.Id = id;
            header.Add(id);
            defaultHeader.Add(id);

            header.Add(new HeaderItem { Name = "entity_name", DisplayName = "Type", DataType = "string", Placeholder = "entity" });
            header.Add(new HeaderItem { Name = "entitytemplate", DisplayName = "Template", DataType = "string", Placeholder = "entitytemplate" });
            header.Add(new HeaderItem { Name = "modifieddate", DisplayName = "Last modified date", DataType = "string", Placeholder = "date" });
            header.Add(new HeaderItem { Name = "doi", DisplayName = "DOI", DataType = "string", Placeholder = "doi" });

            // geo header
            header.Add(new HeaderItem { Name = "geo", DisplayName = "Location", DataType = "string", Placeholder = "geodata" });


            foreach (var item in headerItemList)
            {
                HeaderItem hi = new HeaderItem();
                hi.Name = item.ComponentName;
                hi.DisplayName = item.ComponentName;
                hi.Placeholder = item.Placeholder;
                header.Add(hi);

                if (item.DefaultHeaderItem)
                {
                    defaultHeader.Add(hi);
                }
            }
            // create filter for geo data
            QueryContainer geoFilter = null;

            if (latitude.HasValue && longitude.HasValue && radiusKm.HasValue)
            {
                geoFilter = new GeoDistanceQuery
                {
                    Field = "gen_geopoint",
                    Location = new GeoLocation(latitude.Value, longitude.Value),
                    Distance = $"{radiusKm.Value}km"
                };
            }
            else if (topLeftLat.HasValue && topLeftLon.HasValue &&
                     bottomRightLat.HasValue && bottomRightLon.HasValue)
            {
                geoFilter = new GeoBoundingBoxQuery
                {
                    Field = "gen_geopoint",
                    BoundingBox = new BoundingBox
                    {
                        TopLeft = new GeoLocation(topLeftLat.Value, topLeftLon.Value),
                        BottomRight = new GeoLocation(bottomRightLat.Value, bottomRightLon.Value)
                    }
                };
            }

            var finalQuery = new BoolQuery
            {
                Must = new List<QueryContainer> { baseQuery ?? new MatchAllQuery() },
                Filter = geoFilter != null ? new List<QueryContainer> { geoFilter } : null
            };

            var searchResponse = _client.Search<Dictionary<string, object>>(s => s
                .Index(OpenSearchProvider.DefaultIndex)
                .Query(q => finalQuery)
                .Size(n)
            );

            var rowList = new List<Row>();

            foreach (var hit in searchResponse.Hits)
            {
                var source = hit.Source;
                var values = new List<object>();

                source.TryGetValue("doc_id", out var docId);
                source.TryGetValue("gen_entity_name", out var entityName);
                source.TryGetValue("gen_entitytemplate", out var template);
                source.TryGetValue("gen_modifieddate", out var modifiedDate);
                source.TryGetValue("gen_doi", out var doi);

                values.Add(docId);
                values.Add(entityName);
                values.Add(template);
                values.Add(modifiedDate);
                values.Add(doi);

                // geosearch
                if (source.TryGetValue("gen_geopoint", out var geoObj))
                {
                    if (geoObj is Dictionary<string, object> geoDict)
                    {
                        var lat = geoDict.ContainsKey("lat") ? geoDict["lat"] : null;
                        var lon = geoDict.ContainsKey("lon") ? geoDict["lon"] : null;

                        values.Add($"{lat}, {lon}");
                    }
                    else
                    {
                        values.Add("");
                    }
                }
                else if (source.TryGetValue("gen_geobbox", out var bboxObj))
                {
                    values.Add("BBox");
                }
                else
                {
                    values.Add("");
                }

                // dynamic fields
                foreach (var item in headerItemList)
                {
                    string componentName = item.ComponentName;

                    if (item.ComponentType == SearchComponentBaseType.Facet)
                        componentName = "facet_" + componentName;
                    else if (item.ComponentType == SearchComponentBaseType.Category)
                        componentName = "category_" + componentName;
                    else if (item.ComponentType == SearchComponentBaseType.Property)
                        componentName = "property_" + componentName;

                    if (source.TryGetValue(componentName, out var fieldValues))
                    {
                        if (fieldValues is IEnumerable<object> multi)
                            values.Add(string.Join(", ", multi));
                        else
                            values.Add(fieldValues?.ToString());
                    }
                    else
                    {
                        values.Add("");
                    }
                }

                rowList.Add(new Row { Values = values });
            }

            search_res.Header = header;
            search_res.DefaultVisibleHeaderItem = header;
            search_res.Rows = rowList;

            return search_res;
        }

        public IEnumerable<Dictionary<string, object>> GeoPointSearch(
            QueryContainer baseQuery,
            double latitude,
            double longitude,
            double radiusKm,
            int size = 1000)
        {
            var searchResponse = _client.Search<Dictionary<string, object>>(s => s
                .Index(OpenSearchProvider.DefaultIndex)
                .Size(size)
                .Query(q => q
                    .Bool(b => b
                        .Must(baseQuery ?? new MatchAllQuery())
                        .Filter(f => f
                            .GeoDistance(g => g
                                .Field("gen_geopoint")
                                .Location(latitude, longitude)
                                .Distance($"{radiusKm}km")
                            )
                        )
                    )
                )
            );

            return searchResponse.Hits.Select(h => h.Source);
        }
        public IEnumerable<Dictionary<string, object>> GeoBoundingBoxSearch(
            QueryContainer baseQuery,
            double topLeftLat,
            double topLeftLon,
            double bottomRightLat,
            double bottomRightLon,
            int size = 1000)
        {
            var searchResponse = _client.Search<Dictionary<string, object>>(s => s
                .Index(OpenSearchProvider.DefaultIndex)
                .Size(size)
                .Query(q => q
                    .Bool(b => b
                        .Must(baseQuery ?? new MatchAllQuery())
                        .Filter(f => f
                            .GeoBoundingBox(g => g
                                .Field("gen_geopoint")
                                .BoundingBox(bb => bb
                                    .TopLeft(topLeftLat, topLeftLon)
                                    .BottomRight(bottomRightLat, bottomRightLon)
                                )
                            )
                        )
                    )
                )
            );

            return searchResponse.Hits.Select(h => h.Source);
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

            // For Debugging
            ///*
            var lowLevelResponse = _client.LowLevel.Search<StringResponse>(
                OpenSearchProvider.AutoCompleteIndex,
                PostData.Serializable(new
                {
                    suggest = new
                    {
                        autocomplete_suggest = new
                        {
                            prefix = searchText,
                            completion = new
                            {
                                field = "all",
                                size = 10
                            }
                        }
                    }
                })
            );

            var json = lowLevelResponse.Body;
            Debug.WriteLine(json);

            // */
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
                            WriteToIndex(pair.Key, onlyReleasedTags, dm);
                        }
                        else if (pair.Value == IndexingAction.UPDATE)
                        {
                            if (existsResponse.Exists)
                            {
                                _client.Delete<object>(pair.Key, idx => idx.Index(OpenSearchProvider.DefaultIndex));
                                _client.Delete<object>(pair.Key, idx => idx.Index(OpenSearchProvider.AutoCompleteIndex));

                            }
                            WriteToIndex(pair.Key, onlyReleasedTags, dm);
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

        //public void IndexIntegratedDatasets(long entityTemplateId, bool onlyReleasedTags, DatasetManager dm)

        public void IndexIntegratedDatasets(long entityTemplateId, bool onlyReleasedTags)
        {
            // integratedIndex is related to EntityTemplate, so templateId is necessary to get related config
            var localConfig = SearchConfigManager.GetLocal(entityTemplateId);
            // get path to folder where the external xml files are located
            var folderPath = localConfig?.ExternalSource?.LocalPath;
            
            if (string.IsNullOrWhiteSpace(folderPath))

                throw new ArgumentException("Folder path must not be empty.", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException(folderPath);

            // gather all xml files in topdir and subfirs either
            var xmlFiles = Directory
                .EnumerateFiles(folderPath, "*.xml", SearchOption.AllDirectories)
                .ToList();

            if (!xmlFiles.Any())
                return;

            foreach (var file in xmlFiles)
            {
                try
                {
                    // TODO: Woher die ID bekommen?
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(file);

                    // will not work, because dataManager DM is only necessary when working with the postgres db
                    // WriteToIndex(id, onlyReleasedTags, dm);

                    var name = xmlDoc.SelectSingleNode("//name")?.InnerText;
                    var source = xmlDoc.SelectSingleNode("//source")?.InnerText;
                    var externalName = xmlDoc.SelectSingleNode("//external_name")?.InnerText;

                    IntegratedSourceToIdx(entityTemplateId, xmlDoc);
                }
                catch (Exception ex)
                {
                    // TODO: Logging
                    // logger.Error(ex, $"Failed to index file {file}");
                }
            }
        }

        private void IntegratedSourceToIdx(long entityTemplateId, XmlDocument metadata)
        {
            string doi = "";
            string date = "2020-01-01";
            string entityTemplate = "Test Data Template";
            // TODO: how to handle external document ids?
            long id = 99999999;
            var entityName = "";
            List<string> ng_all_list = new List<string>();
            // document for index
            Dictionary<string, object> doc = new Dictionary<string, object>
            {
                ["doc_id"] = id,
                ["gen_modifieddate"] = date,
                ["gen_entitytemplate"] = entityTemplate,
                ["ng_all"] = ng_all_list
            };

            // autocomplete to Index
            List<string> autocomplete_ng_all_list = new List<string>();
            // autocomplete index new
            var autocompleteDoc = new AutoCompleteDocument(id.ToString());

            SystemInfosToIndex(id, doi, entityName, entityTemplate, doc, id, autocompleteDoc);

            var globalSearchComponents = SearchConfigManager.GetGlobalSearchComponent();

            // ----------- SEARCH COMPONENTS
            // get dataset
            var localConfig = SearchConfigManager.GetLocalConfigForEntityTemplate(entityTemplateId);

            // facets
            if (globalSearchComponents.FacetsToIndex)
            {
                foreach (var facet in localConfig.SearchComponents.Facets)
                {
                    foreach (var metadataNode in facet.MetadataNodes)
                    {
                        XmlNodeList elementList = metadata.SelectNodes(metadataNode);
                        // check if data are at this position in dataset
                        if (elementList != null)
                        {
                            foreach (XmlNode element in elementList)
                            {
                                string elementName = element.InnerText;
                                if (!element.InnerText.Trim().Equals(""))
                                    AppendFacetToDocument(facet, element.InnerText.Trim(), doc, id, metadataNode, ng_all_list, autocompleteDoc);
                            }
                        }

                    }
                }
            } // end facets

            // category
            // TODO: in der alten Version beginnt bei den Categories das indexing fuer primary data?!?
            if (globalSearchComponents.CategoriesToIndex)
            {
                foreach (var category in localConfig.SearchComponents.Categories)
                {
                    foreach (var metadataNode in category.MetadataNodes)
                    {
                        XmlNodeList elementList = metadata.SelectNodes(metadataNode);
                        if (elementList != null)
                        {
                            foreach (XmlNode elemnt in elementList)
                            {
                                var globalComponent = SearchConfigManager.GetGlobalSearchComponentById(category.GlobalId, SearchComponentBaseType.Category);
                                string value = elemnt.InnerText;
                                doc["category_" + globalComponent.ComponentName] = value;
                                doc["ng_" + globalComponent.ComponentName] = value;
                                ng_all_list.Add(value);
                                //doc["ng_all"] = elemList[i].InnerText;
                                autocompleteDoc.AddCategory(value, globalComponent.ComponentName);
                            }
                        }
                    }
                }
            }

            // generals
            if (globalSearchComponents.GeneralsToIndex)
            {
                foreach (var general in localConfig.SearchComponents.General)
                {
                    foreach (var metadataNode in general.MetadataNodes)
                    {
                        XmlNodeList elementList = metadata.SelectNodes(metadataNode);
                        if (elementList != null)
                        {
                            foreach (XmlNode element in elementList)
                            {
                                var globalComponent = SearchConfigManager.GetGlobalSearchComponentById(general.GlobalId, SearchComponentBaseType.General);
                                string value = element.InnerText;
                                doc[globalComponent.ComponentName] = value;
                                ng_all_list.Add(value);
                                //doc["ng_all"] = elemList[i].InnerText;
                                autocompleteDoc.AddCategory(value, globalComponent.ComponentName);
                            }

                        }
                    }
                }
            }

            // properties
            if (globalSearchComponents.PropertiesToIndex)
            {
                foreach (var prop in localConfig.SearchComponents.Properties)
                {
                    throw new NotSupportedException("Properties are not supported to index");
                }
            }
            var globalConfig = SearchConfigManager.GetGlobal();
            //var localConfig = SearchConfigManager.GetLocal(entityTemplateId);

            WriteAutocompleteIndex(id, autocompleteDoc);
            var indexResponse = _client.Index(doc, i => i
                .Index(OpenSearchProvider.DefaultIndex)
                .Id(doc["doc_id"].ToString())
            );
            
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
                            WriteToIndex(datasetId, onlyReleasedTags, dm);
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
                            WriteToIndex(datasetId, onlyReleasedTags, dm);
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
                        WriteToIndex(datasetId, onlyReleasedTags, dm);
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

        /// <summary>
        /// Triggers when someone upload or edit a dataset, used to write data to opensearch index via SearchConfig.json
        /// </summary>
        /// <param name="id"></param>
        /// <param name="onlyReleasedTags"></param>
        /// <param name="dm"></param>
        /// <exception cref="NotSupportedException"></exception>
        private void WriteToIndex(long id, bool onlyReleasedTags, DatasetManager dm)
        {
            string doi = "";
            string date = "";
            string entityTemplate = "";
            long entityTemplateId;
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
                entityTemplateId = version.Dataset.EntityTemplate.Id;
                date = version.ModificationInfo?.Timestamp?.ToString("yyyy-MM-dd");
                if (date == null) version.CreationInfo?.Timestamp?.ToString("yyyy-MM-dd");
                if (date == null) date = "";
            }



            var entityName = "";
            List<string> ng_all_list = new List<string>();
            // document for index
            Dictionary<string, object> doc = new Dictionary<string, object>
            {
                ["doc_id"] = id,
                ["gen_isPublic"] = _entityPermissionManager.ExistsAsync(_entityTypeId.Value, id).Result,
                ["gen_entity_name"] = entityName,
                ["gen_modifieddate"] = date,
                ["gen_entitytemplate"] = entityTemplate,
                ["ng_all"] = ng_all_list
            };

            // autocomplete to Index
            List<string> autocomplete_ng_all_list = new List<string>();
            // autocomplete index new
            var autocompleteDoc = new AutoCompleteDocument(id.ToString());

            SystemInfosToIndex(id, doi, entityName, entityTemplate, doc, id, autocompleteDoc);

            var globalSearchComponents = SearchConfigManager.GetGlobalSearchComponent();

            // ----------- SEARCH COMPONENTS
            // get dataset
            if (dm.GetDataset(id) is Dataset dataset)
            {
                entityTemplateId = dataset.EntityTemplate.Id;
                var localConfig = SearchConfigManager.GetLocalConfigForEntityTemplate(entityTemplateId);

                // facets
                if (globalSearchComponents.FacetsToIndex)
                {
                    foreach (var facet in localConfig.SearchComponents.Facets)
                    {
                        foreach (var metadataNode in facet.MetadataNodes)
                        {
                            XmlNodeList elementList = metadata.SelectNodes(metadataNode);
                            // check if data are at this position in dataset
                            if (elementList != null)
                            {
                                foreach (XmlNode element in elementList)
                                {
                                    string elementName = element.InnerText;
                                    // if ist wichtig, weil leere Felder mit "" haeufig codiert sind bzw als \"NonEmptyStringType\""
                                    if (!element.InnerText.Trim().Equals(""))
                                        AppendFacetToDocument(facet, element.InnerText.Trim(), doc, id, metadataNode, ng_all_list, autocompleteDoc);
                                }
                            }

                        }
                    }
                } // end facets

                // category
                // TODO: in der alten Version beginnt bei den Categories das indexing fuer primary data?!?
                if (globalSearchComponents.CategoriesToIndex)
                {
                    foreach (var category in localConfig.SearchComponents.Categories)
                    {
                        foreach (var metadataNode in category.MetadataNodes)
                        {
                            XmlNodeList elementList = metadata.SelectNodes(metadataNode);
                            if (elementList != null)
                            {
                                foreach (XmlNode elemnt in elementList)
                                {
                                    var globalComponent = SearchConfigManager.GetGlobalSearchComponentById(category.GlobalId, SearchComponentBaseType.Category);
                                    string value = elemnt.InnerText;
                                    doc["category_" + globalComponent.ComponentName] = value;
                                    doc["ng_" + globalComponent.ComponentName] = value;
                                    ng_all_list.Add(value);
                                    //doc["ng_all"] = elemList[i].InnerText;
                                    autocompleteDoc.AddCategory(value, globalComponent.ComponentName);
                                }
                            }
                        }
                    }
                }

                // generals
                if (globalSearchComponents.GeneralsToIndex)
                {
                    foreach (var general in localConfig.SearchComponents.General)
                    {
                        foreach (var metadataNode in general.MetadataNodes)
                        {
                            XmlNodeList elementList = metadata.SelectNodes(metadataNode);
                            if (elementList != null)
                            {
                                foreach (XmlNode element in elementList)
                                {
                                    var globalComponent = SearchConfigManager.GetGlobalSearchComponentById(general.GlobalId, SearchComponentBaseType.General);
                                    string value = element.InnerText;
                                    doc[globalComponent.ComponentName] = value;
                                    ng_all_list.Add(value);
                                    //doc["ng_all"] = elemList[i].InnerText;
                                    autocompleteDoc.AddCategory(value, globalComponent.ComponentName);
                                }

                            }
                        }
                    }
                }

                // properties
                if (globalSearchComponents.PropertiesToIndex)
                {
                    foreach (var prop in localConfig.SearchComponents.Properties)
                    {
                        throw new NotSupportedException("Properties are not supported to index");
                    }
                }
                var globalConfig = SearchConfigManager.GetGlobal();
                //var localConfig = SearchConfigManager.GetLocal(entityTemplateId);

                // Ingest Geodata
                AppendBoundingBoxSpatialData(localConfig, metadata,doc);
                AppendGeoPointSpatialData(localConfig, metadata, doc);

                if (dm.IsDatasetCheckedIn(id))
                {
                    AppendPrimaryData(globalConfig, localConfig, dm.GetDatasetLatestVersion(id),doc, ng_all_list, autocompleteDoc, id);
                }

                WriteAutocompleteIndex(id, autocompleteDoc);
                var indexResponse = _client.Index(doc, i => i
                    .Index(OpenSearchProvider.DefaultIndex)
                    .Id(doc["doc_id"].ToString())
                );
            }
        }

        public void TestToIdx(Dictionary<string, object> doc)
        {
            var indexResponse = _client.Index(doc, i => i
                .Index("bexissearchindex2")
                .Id(doc["doc_id"].ToString())
            );
            Debug.WriteLine(indexResponse);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
            Debug.WriteLine(json);

            var response = _client.LowLevel.Index<StringResponse>(
                "bexissearchindex2",
                "888888",
                global::OpenSearch.Net.PostData.String(json));

            Debug.WriteLine(response.Body);
        }

        // Add bbox data to existing opensearch document
        public void AppendBoundingBoxSpatialData(LocalConfig localConfig, XmlDocument metadata, Dictionary<string, object> doc)
        {
            if (metadata == null || localConfig?.SpatialData?.SpatialMetadata == null)
                return;

            var bbox = localConfig.SpatialData.SpatialMetadata as BBoxSpatialMetadata;
            if (bbox == null)
                return;

            double? west = GetDouble(metadata, bbox.WestBoundLongitude);
            double? east = GetDouble(metadata, bbox.EastBoundLongitude);
            double? north = GetDouble(metadata, bbox.NorthBoundLatitude);
            double? south = GetDouble(metadata, bbox.SouthBoundLatitude);

            if (west.HasValue && east.HasValue && north.HasValue && south.HasValue)
            {
                doc["gen_geobbox"] = new
                {
                    type = "envelope",
                    coordinates = new[]
                    {
                        new[] { west.Value, north.Value },
                        new[] { east.Value, south.Value }
                    }
                };
                Debug.WriteLine(JsonConvert.SerializeObject(doc, Newtonsoft.Json.Formatting.Indented));

            }
        }

        // Add geopoint data to existing opensearch document

        public void AppendGeoPointSpatialData(LocalConfig localConfig, XmlDocument metadata, Dictionary<string, object> doc)
        {
            if (metadata == null || localConfig?.SpatialData?.SpatialMetadata == null)
                return;

            var point = localConfig.SpatialData.SpatialMetadata as PointSpatialMetadata;
            if (point == null)
                return;

            double? lat = GetDouble(metadata, point.Latitude);
            double? lon = GetDouble(metadata, point.Longitude);
            double? radius = GetDouble(metadata, point.Radius);

            if (lat.HasValue && lon.HasValue)
            {
                doc["gen_geopoint"] = new Dictionary<string, object>
                {
                    { "lat", lat.Value },
                    { "lon", lon.Value }
                };

                if (radius.HasValue)
                {
                    doc["location_radius"] = radius.Value;
                }
            }
        }

        // Helper for Geodata parsing from xml
        private double? GetDouble(XmlDocument doc, string xpath)
        {
            if (string.IsNullOrWhiteSpace(xpath))
                return null;

            var node = doc.SelectSingleNode(xpath);

            if (node == null || string.IsNullOrWhiteSpace(node.InnerText))
                return null;

            if (double.TryParse(node.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                return value;

            return null;
        }

        private void AppendPrimaryData(GlobalConfig globalConfig, LocalConfig localConfig, DatasetVersion datasetVersion, Dictionary<string, object> doc, List<string> ngAllList, AutoCompleteDocument autoCompleteDocument, long docId)
        {
            if (!localConfig.PrimaryData.ToIndex)
                return;

            if (datasetVersion == null) return;

            if (localConfig.PrimaryData.Calc != null)
            {
                using (DatasetManager dm = new DatasetManager())
                using (DataStructureManager dsm = new DataStructureManager())
                {
                    if (datasetVersion.Dataset.DataStructure == null) return;
                    StructuredDataStructure dataStructure = dsm.StructuredDataStructureRepo.Get(datasetVersion.Dataset.DataStructure.Id);
                    if (dataStructure == null) return;

                    if (!_includePrimaryData) return;

                    int fetchSize = dm.PreferedBatchSize;
                    long tupleSize = dm.GetDatasetVersionEffectiveTupleCount(datasetVersion);
                    long noOfFetchs = tupleSize / fetchSize + 1;

                    if (tupleSize > 0)
                    {
                        for (int round = 0; round < noOfFetchs; round++)
                        {
                            List<string> primaryDataStringsToIndex = new List<string>();
                            using (DataTable table = dm.GetLatestDatasetVersionTuples(datasetVersion.Dataset.Id, round, fetchSize))
                            {
                                primaryDataStringsToIndex = GetAllStringValuesFromTable(table);
                            }

                        }
                    }




                    // variablen --> meanings
                    // localconfig hol die liste von calc-objects
                    // global gibt es keine meanings?
                    /*
                             "primary_data": {
                                "to_index": true,
                                "calc": [
                                    {
                                    "operation": "min/max",
                                    "allowed_meanings": [123,456,789]
                                    }
                                ],

                     */




                    AppendDataStructure(dataStructure, doc, docId);

                }
            }


        }

        private void AppendDataStructure(StructuredDataStructure dataStructure, Dictionary<string, object> doc, long docId)
        {
            List<string> dataStructureStringList = GetListOfValuesFromDataStructure(dataStructure);
            var s = dataStructure.Variables;
            foreach (var variable in s)
            {
                var t = variable.Meanings;
            }
            // Variableinstance hat meanings hinterlegt

        }

        private void AppendFacetToDocument(LocalComponent facet, string val, Dictionary<string, object> doc, long docId, string variableNodeLabel, List<string> ng_all, AutoCompleteDocument autoCompleteDocument)
        {
            var globalComponent = SearchConfigManager.GetGlobalSearchComponentById(facet.GlobalId, SearchComponentBaseType.Facet);
            if (facet.DataTypeId == DataTypeId.Text)
            {
                doc["facet_" + globalComponent.ComponentName] = val.ToString();
                ng_all.Add(val.ToString());
                autoCompleteDocument.AddCategory(val.ToString(), globalComponent.ComponentName);
            }
            else
            {
                if (facet.DataTypeId == DataTypeId.Date)
                {
                    DateTime dateValue = DateTime.MinValue;
                    DateTime dateValue_ = dateValue;

                    DateTime.TryParse(val.ToString(), out dateValue);
                    if ((dateValue != dateValue_) && (!string.IsNullOrEmpty(val.ToString())))
                    {
                        string formattedDate = dateValue.ToString("yyyy-MM-dd");
                        doc["facet_" + globalComponent.ComponentName] = formattedDate;
                    }
                    else
                    {
                        DebugEmptyNodes(variableNodeLabel, " ", docId.ToString());
                    }
                }
                else if (facet.DataTypeId == DataTypeId.Integer || 
                    facet.DataTypeId == DataTypeId.Double || 
                    facet.DataTypeId == DataTypeId.Float || 
                    facet.DataTypeId == DataTypeId.Long ||
                    facet.DataTypeId == DataTypeId.HalfFloat)
                {
                    if (!string.IsNullOrEmpty(val.ToString()))
                        doc["facet_" + globalComponent.ComponentName] = double.Parse(val.ToString());
                    else
                    {
                        DebugEmptyNodes(variableNodeLabel, " ", docId.ToString());
                    }
                }
            }
        }

        private void SystemInfosToIndex(long id, string doi, string entity, string template, Dictionary<string, object> doc, long docId, AutoCompleteDocument autocompleteDoc)
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


        private void WriteAutocompleteIndex(long docId, AutoCompleteDocument autocompleteDoc)
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
