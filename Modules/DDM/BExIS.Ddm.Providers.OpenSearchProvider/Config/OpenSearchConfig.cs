using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BExIS.Utils.Models;
using OpenSearch.Client;
using OpenSearch.Net;
using Vaiona.Utils.Cfg;
using System.Reflection;
using System.Net.Sockets;

namespace BExIS.Ddm.Providers.OpenSearchProvider.Config
{
    public static class OpenSearchConfig
    {
        private static XmlDocument _configXml;
        //private static  _reader; // TODO

        private static searchInitObjects _searchInitObj = new searchInitObjects();

        // SearchInitObject in LuceneProvider
        private static List<Facet> _allFacets = _searchInitObj.AllFacets;
        private static List<Property> _allProperties = _searchInitObj.AllProperties;
        private static List<Category> _allCategories = _searchInitObj.AllCategories;

        private static HashSet<string> _numericProperties = new HashSet<string>();
        private static Boolean _isLoaded = false;

        public static List<XmlNode> facetXmlNodeList = new List<XmlNode>();
        public static List<XmlNode> propertyXmlNodeList = new List<XmlNode>();
        public static List<XmlNode> categoryXmlNodeList = new List<XmlNode>();
        public static List<XmlNode> generalXmlNodeList = new List<XmlNode>();
        public static List<XmlNode> headerItemXmlNodeList = new List<XmlNode>();

        private static OpenSearchClient _client = OpenSearchProvider.Client;

        // 
        private static string _xmlConfigFilePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Lucene", "Config", "LuceneConfig.xml");

        //private static string _configFilePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "OpenSearch", "OpenSearchMapping.json");
        /// <summary>
        /// Reset all values to defaults
        /// </summary>
        public static void Reset()
        {
            _isLoaded = false;
            _configXml = null;
            //_reader = get new index reader blabla TODO
            _searchInitObj = new searchInitObjects();
            _allFacets = _searchInitObj.AllFacets;
            _allProperties = _searchInitObj.AllProperties;
            _allCategories = _searchInitObj.AllCategories;

            _numericProperties = new HashSet<string>();
            facetXmlNodeList = new List<XmlNode>();
            propertyXmlNodeList = new List<XmlNode>();
            categoryXmlNodeList = new List<XmlNode>();
            generalXmlNodeList = new List<XmlNode>();
            headerItemXmlNodeList = new List<XmlNode>();
        }

        public static void LoadConfig()
        {
            if (!_isLoaded)
            {
                Load();
                _isLoaded = true;
            }
        }
        //await OpenSearchIndexManager.EnsureIndexMatchesConfigAsync(OpenSearchProvider.Client, "defaultIndex", "Mappings/default-index.json");
        private static void VerifyMapping()
        {
            string indexName = OpenSearchProvider.DefaultIndex;

            // check if index already exists
            var existsResponse = _client.Indices.Exists(indexName);
            if (!existsResponse.Exists)
            {
                // case: index not exist
                // TODO: Try-Catch
                var json_config = File.ReadAllText(path: _xmlConfigFilePath);
                var createResponse = _client.LowLevel.Indices.Create<StringResponse>(indexName, json_config);
                
                if (!createResponse.Success)
                {
                    throw new Exception($"Index creation failed: {createResponse.Body}");
                }
                Debug.WriteLine($"Index {indexName} was created!");
            }
            else
            {
                // case: index exist --> verify index mapping!
                var currentMapping = _client.LowLevel.Indices.GetMapping<StringResponse>(indexName);
                var expectedMappingJson = File.ReadAllText(path: _xmlConfigFilePath);
                // TODO: new method to verify Equality is needed, because both are equal
                var areEqual = JsonDocument.Parse(currentMapping.Body).RootElement
                        .GetProperty(indexName)
                        .GetProperty("mappings")
                        .ToString() == JsonDocument.Parse(expectedMappingJson).RootElement.GetProperty("mappings").ToString();
                if (!areEqual)
                {
                    Debug.WriteLine($"The mapping from Index '{indexName}' is not equal with the provided json mapping");
                    // TODO
                }
                else
                {
                    Debug.WriteLine($"The mappping from index '{indexName}' and the provided json mapping, are equal!");
                }
            }            
        }

        private static void LoadAutoCompleteIndex(string jsonPath, string indexName)
        {
            // Prüfen, ob Index bereits existiert
            var existsResponse = _client.Indices.Exists(indexName);
            if (existsResponse.Exists)
            {
                // Index existiert bereits, ggf. loggen oder einfach return
                Console.WriteLine($"Index '{indexName}' existiert bereits.");
                return;
            }
            var json = File.ReadAllText(jsonPath);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            var lowResponse = _client.LowLevel.Indices.Create<BytesResponse>(indexName, jsonBytes);
            if (!lowResponse.Success)
            {
                throw new Exception("Could not create Auto Complete Index from JSON");
            }
        }

        private static void Load()
        {
            LoadAutoCompleteIndex("C:\\Users\\Paul\\Desktop\\autocompleteindex2.json", OpenSearchProvider.AutoCompleteIndex); // TODO: path
            string configDirPath = Path.GetDirectoryName(_xmlConfigFilePath);
            if(!Directory.Exists(configDirPath))Directory.CreateDirectory(configDirPath);

            _configXml = new XmlDocument();
            _configXml.Load(_xmlConfigFilePath);
            XmlNodeList fieldProperties = _configXml.GetElementsByTagName("field");

            Category categoryDefault = new Category();
            categoryDefault.Name = "All";
            categoryDefault.Value = "All";
            categoryDefault.DisplayName = "All";
            categoryDefault.DefaultValue = "nothing";
            _allCategories.Add(categoryDefault);
            foreach (XmlNode fieldProperty in fieldProperties)
            {
                if (!fieldProperty.Attributes.GetNamedItem("type").Value.ToLower().Equals("primary_data_field"))
                {
                    string headerItem = fieldProperty.Attributes.GetNamedItem("header_item").Value;
                    string storeItem = fieldProperty.Attributes.GetNamedItem("store").Value;

                    if (headerItem.ToLower().Equals("yes") && storeItem.ToLower().Equals("yes"))
                    {
                        headerItemXmlNodeList.Add(fieldProperty);
                    }

                    String fieldType = fieldProperty.Attributes.GetNamedItem("type").Value;
                    
                    String fieldName = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;

                    String primitiveType = fieldProperty.Attributes.GetNamedItem("primitive_type").Value;

                    // if not string than it is a numeric value?!?
                    if (!primitiveType.ToLower().Equals("string"))
                    {
                        _numericProperties.Add(fieldName.ToLower());
                    }

                    // case: Facet_field
                    if (fieldType.ToLower().Equals("facet_field"))
                    {
                        _allFacets.Add(GetFacetField(fieldProperty, fieldName));
                    }
                    else if (fieldType.ToLower().Equals("property_field"))
                    {
                        _allProperties.Add(GetPropertyField(fieldProperty, fieldName));
                    }
                    else if (fieldType.ToLower().Equals("category_field"))
                    {
                        _allCategories.Add(GetCategoryField(fieldProperty, fieldName));
                    }
                    else if (fieldType.ToLower().Equals("general_field"))
                    {
                        generalXmlNodeList.Add(fieldProperty);
                    }
                }
                else if (fieldProperty.Attributes.GetNamedItem("type").Value.ToLower().Equals("primary_data_field"))
                {
                    categoryXmlNodeList.Add(fieldProperty);
                    Category cDefault = new Category();
                    cDefault.Name = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;
                    cDefault.DisplayName = fieldProperty.Attributes.GetNamedItem("display_name").Value;
                    cDefault.Value = fieldProperty.Attributes.GetNamedItem("lucene_name").Value; ;
                    cDefault.DefaultValue = "nothing";
                    _allCategories.Add(cDefault);
                }
            }
        }

        private static Facet GetFacetField(XmlNode field, string fieldName)
        {
            facetXmlNodeList.Add(field);
            Facet facet = new Facet();
            facet.Name = fieldName;
            facet.Text = fieldName;
            facet.Value = fieldName;
            facet.DisplayName = field.Attributes.GetNamedItem("display_name").Value;
            facet.Childrens = new List<Facet>();
            List<Facet> childListDefault = new List<Facet>();
            try
            {
                //var searchResponse = _client.Search<Dictionary<string, object>>(s => s
                //    .Index(OpenSearchProvider.DefaultIndex)
                //    .Size(0) // keine Dokumente zurückgeben – nur Aggregationen
                //    .Query(q => q.MatchAll())
                //    .Aggregations(aggs => aggs
                //        .Terms("facet_terms", t => t
                //            .Field("facet_" + fieldName + ".keyword") // ".keyword" für Aggregationen auf Textfeld
                //            .Size(1000) // Anzahl der erwarteten Buckets TODO: evtl anpassen?
                //        )
                //    )
                //);

                var response = _client.Search<object>(s => s
                    .Index(OpenSearchProvider.DefaultIndex)
                    .Size(0) // keine Dokumente zurückgeben – nur Aggregationen
                    .Aggregations(a => a
                        .Terms("facet_values", t => t
                            .Field("facet_" + fieldName)
                            .Size(1000)
                            .Aggregations(aa => aa
                                .TopHits("top_docs", th => th
                                    .Size(10)
                                )
                            )
                        )
                    )
                );

                // Zugriff auf Aggregation "facet_values"
                var facetAgg = response.Aggregations.Terms("facet_values");
                int cCount = 0;

                if (facetAgg != null)
                {
                    foreach (var bucket in facetAgg.Buckets)
                    {
                        if (!string.IsNullOrEmpty(bucket.Key))
                        {
                            var ccDefault = new Facet
                            {
                                Name = bucket.Key,
                                Text = bucket.Key,
                                Value = bucket.Key,
                                Count = (int)bucket.DocCount
                            };

                            if (ccDefault.Count > 0) cCount++;

                            // Zugriff auf die Top-Hits-Aggregation im aktuellen Bucket
                            var topHits = bucket.TopHits("top_docs");
                            foreach (var doc in topHits.Documents<object>())
                            {
                                var dict = doc as IDictionary<string, object>;
                                var fieldValue = dict?["facet_" + fieldName]?.ToString();
                                ccDefault.Name = fieldValue;
                                ccDefault.Text = fieldValue;
                                ccDefault.Value = fieldValue;
                                if(!facet.Childrens.Exists(x => x.Name == ccDefault.Name))
                                    facet.Childrens.Add(ccDefault);

                                // Weiterverarbeiten, z. B. Child-Facet anlegen ...
                            }
                        }
                    }
                }

                facet.Count = cCount;
            }
            catch (Exception ex)
            {
            }
            return facet;
        }
        private static Property GetPropertyField(XmlNode field, string fieldName)
        {
            propertyXmlNodeList.Add(field);
            Property cDefault = new Property();
            cDefault.Name = field.Attributes.GetNamedItem("lucene_name").Value;
            cDefault.DisplayName = field.Attributes.GetNamedItem("display_name").Value;
            cDefault.DisplayTitle = field.Attributes.GetNamedItem("display_name").Value;
            cDefault.DataSourceKey = field.Attributes.GetNamedItem("metadata_name").Value;
            cDefault.UIComponent = field.Attributes.GetNamedItem("uiComponent").Value; ;
            cDefault.AggregationType = field.Attributes.GetNamedItem("aggregationType").Value;
            cDefault.DefaultValue = "All";
            cDefault.DataType = field.Attributes.GetNamedItem("primitive_type").Value;
            if (cDefault.UIComponent.ToLower().Equals("range") && cDefault.DataType.ToLower().Equals("data"))
            {
                string direction = field.Attributes.GetNamedItem("direction").Value;
                if (direction.ToLower().Equals("increase"))
                {
                    cDefault.Direction = Direction.increase;
                }
                else
                {
                    cDefault.Direction = Direction.decrease;
                }
            }

            // Starte Aggregation in OpenSearch auf "property_" + fieldName
            var response = _client.Search<object>(s => s
                .Index("datasets") // ggf. Indexnamen anpassen
                .Size(0) // Keine Dokumente zurückgeben, nur Aggregation
                .Aggregations(agg => agg
                    .Terms("values", t => t
                        .Field($"property_{fieldName}.keyword")
                        .Size(100)
                    )
                )
            );
            if (response.IsValid && response.Aggregations.TryGetValue("values", out var aggObj) && aggObj is BucketAggregate bucketAgg)
            {
                var valueList = bucketAgg.Items
                    .OfType<KeyedBucket<object>>()
                    .Select(b => b.Key.ToString())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .OrderBy(k => k)
                    .ToList();

                cDefault.Values = valueList;
            }
            else
            {
                // Fallback / Logging / Fehlerbehandlung
                throw new Exception("Fehler beim Abrufen von Aggregationsdaten aus OpenSearch.");
            }
            return cDefault;
        }
        private static Category GetCategoryField(XmlNode field, string fieldName)
        {
            categoryXmlNodeList.Add(field);

            Category cDefault = new Category();
            cDefault.Name = field.Attributes.GetNamedItem("lucene_name").Value;
            cDefault.DisplayName = field.Attributes.GetNamedItem("display_name").Value;
            cDefault.Value = field.Attributes.GetNamedItem("lucene_name").Value; ;
            cDefault.DefaultValue = "nothing";
            return cDefault;
        }


        private static void Load_bak()
        {
            // if not exists --> create
            string configDirPath = Path.GetDirectoryName(_xmlConfigFilePath);
            if (!Directory.Exists(configDirPath)) Directory.CreateDirectory(configDirPath);
            VerifyMapping();


            _configXml = new XmlDocument();
            _configXml.Load(_xmlConfigFilePath);
            XmlNodeList fieldProperties = _configXml.GetElementsByTagName("field");


            Category categoryDefault = new Category();
            categoryDefault.Name = "All";
            categoryDefault.Value = "All";
            categoryDefault.DisplayName = "All";
            categoryDefault.DefaultValue = "nothing";
            _allCategories.Add(categoryDefault);

            foreach (XmlNode fieldProperty in fieldProperties)
            {
                if (!fieldProperty.Attributes.GetNamedItem("type").Value.ToLower().Equals("primary_data_field"))
                {
                    String headerItem = fieldProperty.Attributes.GetNamedItem("header_item").Value;
                    String storeItem = fieldProperty.Attributes.GetNamedItem("store").Value;
                    if (headerItem.ToLower().Equals("yes") && storeItem.ToLower().Equals("yes"))
                    {
                        headerItemXmlNodeList.Add(fieldProperty);
                    }

                    String fieldType = fieldProperty.Attributes.GetNamedItem("type").Value;
                    String fieldName = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;

                    String primitiveType = fieldProperty.Attributes.GetNamedItem("primitive_type").Value;
                    if (!primitiveType.ToLower().Equals("string"))
                    {
                        _numericProperties.Add(fieldName.ToLower());
                    }


                    // check if fieldType is equal with: facet_field | property_field | category_fiel | general_field
                    if (fieldType.ToLower().Equals("facet_field"))
                    {
                        facetXmlNodeList.Add(fieldProperty);
                        Facet defaultFacet = new Facet();
                        defaultFacet.Name = fieldName;
                        defaultFacet.Text = fieldName;
                        defaultFacet.Value = fieldName;
                        defaultFacet.DisplayName = fieldProperty.Attributes.GetNamedItem("display_name").Value;

                        defaultFacet.Childrens = new List<Facet>();
                        List<Facet> opensearchDefaults = new List<Facet>();
                        try
                        {
                            string indexName = OpenSearchProvider.DefaultIndex;

                            var response = _client.Search<object>(s => s
                                .Index(indexName)
                                .Size(0)
                                .Query(q => q.MatchAll()) // entspricht Lucene: *:*
                                .Aggregations(a => a
                                    .Terms("facet_counts", t => t
                                        .Field($"facet_{fieldName}.keyword")
                                        .Size(100) // oder höher
                                        .Aggregations(aa => aa
                                            .TopHits("top_docs", th => th
                                                .Size(10) // wie viele Dokumente pro Facettenwert
                                            )
                                        )
                                    )
                                )
                            );


                            var AllFacetsDefault = new List<Facet>();
                            var cDefault = new Facet();
                            int cCount = 0;

                            var termsAgg = response.Aggregations.Terms("facet_counts");
                            if (termsAgg != null)
                            {
                                foreach (var bucket in termsAgg.Buckets)
                                {
                                    if (!string.IsNullOrEmpty(bucket.Key))
                                    {
                                        var ccDefault = new Facet
                                        {
                                            Name = bucket.Key,
                                            Text = bucket.Key,
                                            Value = bucket.Key,
                                            Count = (int)bucket.DocCount.GetValueOrDefault()
                                        };

                                        if (ccDefault.Count > 0)
                                            cCount++;

                                        var topHits = bucket.TopHits("top_docs");
                                        foreach (var hit in topHits.Hits<object>())
                                        {
                                            var source = hit.Source as IDictionary<string, object>;
                                            if (source != null && source.TryGetValue($"facet_{fieldName}", out var value))
                                            {
                                                var stringValue = value?.ToString();
                                                if (!ccDefault.Childrens.Exists(x => x.Name == stringValue))
                                                {
                                                    ccDefault.Childrens.Add(new Facet
                                                    {
                                                        Name = stringValue,
                                                        Text = stringValue,
                                                        Value = stringValue
                                                    });
                                                }
                                            }
                                        }

                                        cDefault.Childrens.Add(ccDefault);
                                    }
                                }

                                cDefault.Count = cCount;
                                AllFacetsDefault.Add(cDefault);
                            }
                            }
                        catch
                        {

                        }
                    }
                    else if (fieldType.ToLower().Equals("property_field"))
                    {
                        propertyXmlNodeList.Add(fieldProperty);
                        Property cDefault = new Property();
                        //c.Id = x.Attributes[Property.ID].InnerText;
                        cDefault.Name = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;
                        cDefault.DisplayName = fieldProperty.Attributes.GetNamedItem("display_name").Value;
                        cDefault.DisplayTitle = fieldProperty.Attributes.GetNamedItem("display_name").Value;
                        cDefault.DataSourceKey = fieldProperty.Attributes.GetNamedItem("metadata_name").Value;
                        cDefault.UIComponent = fieldProperty.Attributes.GetNamedItem("uiComponent").Value; ;
                        cDefault.AggregationType = fieldProperty.Attributes.GetNamedItem("aggregationType").Value;
                        cDefault.DefaultValue = "All";
                        cDefault.DataType = fieldProperty.Attributes.GetNamedItem("primitive_type").Value;
                        if (cDefault.UIComponent.ToLower().Equals("range") && cDefault.DataType.ToLower().Equals("date"))
                        {
                            String direction = fieldProperty.Attributes.GetNamedItem("direction").Value;

                            if (direction.ToLower().Equals("increase"))
                            {
                                cDefault.Direction = Direction.increase;
                            }
                            else
                            {
                                cDefault.Direction = Direction.decrease;
                            }
                        }

                        // Starte Aggregation in OpenSearch auf "property_" + fieldName
                        var response = _client.Search<object>(s => s
                            .Index("datasets") // ggf. Indexnamen anpassen
                            .Size(0) // Keine Dokumente zurückgeben, nur Aggregation
                            .Aggregations(agg => agg
                                .Terms("values", t => t
                                    .Field($"property_{fieldName}.keyword")
                                    .Size(100)
                                )
                            )
                        );

                        if (response.IsValid && response.Aggregations.TryGetValue("values", out var aggObj) && aggObj is BucketAggregate bucketAgg)
                        {
                            var valueList = bucketAgg.Items
                                .OfType<KeyedBucket<object>>()
                                .Select(b => b.Key.ToString())
                                .Where(k => !string.IsNullOrWhiteSpace(k))
                                .OrderBy(k => k)
                                .ToList();

                            cDefault.Values = valueList;
                            _allProperties.Add(cDefault);
                        }
                        else
                        {
                            // Fallback / Logging / Fehlerbehandlung
                            throw new Exception("Fehler beim Abrufen von Aggregationsdaten aus OpenSearch.");
                        }

                    } 
                    else if (fieldType.ToLower().Equals("category_field"))
                    {
                        categoryXmlNodeList.Add(fieldProperty);

                        Category cDefault = new Category();
                        cDefault.Name = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;
                        cDefault.DisplayName = fieldProperty.Attributes.GetNamedItem("display_name").Value;
                        cDefault.Value = fieldProperty.Attributes.GetNamedItem("lucene_name").Value; ;
                        cDefault.DefaultValue = "nothing";

                        _allCategories.Add(cDefault);
                    }
                    else if (fieldType.ToLower().Equals("general_field"))
                    {
                        generalXmlNodeList.Add(fieldProperty);
                    }
                }
                // case: fieldProperty is primary_data_field
                {
                    categoryXmlNodeList.Add(fieldProperty);
                    Category cDefault = new Category();
                    cDefault.Name = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;
                    cDefault.DisplayName = fieldProperty.Attributes.GetNamedItem("display_name").Value;
                    cDefault.Value = fieldProperty.Attributes.GetNamedItem("lucene_name").Value; ;
                    cDefault.DefaultValue = "nothing";
                    _allCategories.Add(cDefault);
                }
            }
                    
        }

        public static void ReloadConfig()
        {
            _isLoaded = false;
            LoadConfig();
        }

        public static HashSet<string> GetNumericPropertiers()
        { 
            return _numericProperties; 
        }

        public static IEnumerable<Facet> GetFacets()
        { 
            return _allFacets; 
        }

        public static IEnumerable<Property> GetProperties()
        {
            return _allProperties;
        }

        public static IEnumerable<Category> GetCategories()
        {
            return _allCategories;
        }

        public static IEnumerable<Facet> GetFacetsCopy()
        {
            searchInitObjects sIOCopy = DeepCopy<searchInitObjects>(_searchInitObj);
            return sIOCopy.AllFacets;
        }

        public static IEnumerable<Property> GetPropertiesCopy()
        {
            searchInitObjects sIOCopy = DeepCopy<searchInitObjects>(_searchInitObj);
            return sIOCopy.AllProperties;
        }

        public static IEnumerable<Category> GetCategoriesCopy()
        {
            searchInitObjects sIOCopy = DeepCopy<searchInitObjects>(_searchInitObj);
            return sIOCopy.AllCategories;
        }

        public static T DeepCopy<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("Object cannot be null");
            return (T)Process(obj, new Dictionary<object, object>() { });
        }


        private static object Process(object obj, Dictionary<object, object> circular)
        {
            if (obj == null)
                return null;
            Type type = obj.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }
            if (type.IsArray)
            {
                if (circular.ContainsKey(obj))
                    return circular[obj];
                string typeNoArray = type.FullName.Replace("[]", string.Empty);
                Type elementType = Type.GetType(typeNoArray + ", " + type.Assembly.FullName);
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                circular[obj] = copied;
                for (int i = 0; i < array.Length; i++)
                {
                    object element = array.GetValue(i);
                    object copy = null;
                    if (element != null && circular.ContainsKey(element))
                        copy = circular[element];
                    else
                        copy = Process(element, circular);
                    copied.SetValue(copy, i);
                }
                return Convert.ChangeType(copied, obj.GetType());
            }
            if (type.IsClass)
            {
                if (circular.ContainsKey(obj))
                    return circular[obj];
                object toret = Activator.CreateInstance(obj.GetType());
                circular[obj] = toret;
                System.Reflection.FieldInfo[] fields = GetAllFields(type).ToArray();
                foreach (System.Reflection.FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                        continue;
                    object copy = circular.ContainsKey(fieldValue) ? circular[fieldValue] : Process(fieldValue, circular);
                    field.SetValue(toret, copy);
                }
                return toret;
            }
            else
                throw new ArgumentException("Unknown type");
        }

        public static IEnumerable<System.Reflection.FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<System.Reflection.FieldInfo>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            return t.GetFields(flags).Union(GetAllFields(t.BaseType));
        }

    }

}
