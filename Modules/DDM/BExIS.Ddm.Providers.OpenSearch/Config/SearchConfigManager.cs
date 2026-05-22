using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using BExIS.Utils.Models;
using Json.Schema;
using Newtonsoft.Json;  
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Category = BExIS.Utils.Models.Category;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public static class SearchConfigManager
    {
        private static string _configFilePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "OpenSearch", "Config", "SearchConfig.json");
        private static string _defaultConfigFilePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "OpenSearch", "Config", "backup", "SearchConfig_backup.json)");
        private static string _configSchemaPath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "OpenSearch", "Config", "backup", "SearchConfig_schema.json");

        private static volatile SearchConfig _configSnapshot;
        private static readonly object _Lock = new object();

        public static SearchConfig GetSnapshot()
        {
            if (_configSnapshot != null)
                return _configSnapshot;

            lock (_Lock)
            {
                if (_configSnapshot == null)
                {
                    Reload();
                }

                return _configSnapshot;
            }
        }

        public static bool ValidateConfig(string configToValidate)
        {
            if (string.IsNullOrWhiteSpace(configToValidate))
                return false;

            try
            {
                // parsing string to json
                JToken json;
                try
                {
                    json = JToken.Parse(configToValidate);
                }
                catch (JsonReaderException)
                {
                    return false; // invalid JSON
                }

                // load schema
                if (!File.Exists(_configSchemaPath))
                    return false;

                JSchema schema;
                try
                {
                    var schemaText = File.ReadAllText(_configSchemaPath);
                    schema = JSchema.Parse(schemaText);
                }
                catch (Exception)
                {
                    return false; // Schema invalid
                }

                // validate process
                IList<string> errors;
                bool isValid = json.IsValid(schema, out errors);

                // check result is valid
                if (!isValid && errors != null)
                {
                    foreach (var error in errors)
                    {
                        Console.WriteLine("Validation error: " + error);
                    }
                }
                return isValid;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public static void Reload(string filePath = null)
        {
            lock (_Lock)
            {
                var path = string.IsNullOrWhiteSpace(filePath)
                    ? _configFilePath
                    : filePath;
                

                if (!File.Exists(path))
                    throw new FileNotFoundException("Config file not found", path);

                var json = File.ReadAllText(path);
                var settings = new JsonSerializerSettings
                {
                    Converters =
            {
                new StringEnumConverter(),
                // new CalcBlockListConverter()
                new SpatialMetadataConverter()
            }
                };

                var newConfig = JsonConvert.DeserializeObject<SearchConfig>(json, settings);

                if (newConfig == null)
                    throw new InvalidOperationException("Invalid config");

                Interlocked.Exchange(ref _configSnapshot, newConfig);
            }
        }


        public static void RestoreDefaultConfig()
        {
            {
                lock (_Lock)
                {
                    // Restore
                    var json = File.ReadAllText(_defaultConfigFilePath);

                    var settings = new JsonSerializerSettings
                    {
                        Converters =
                            {
                                new StringEnumConverter()
                            }
                    };

                    var defaultConfig = JsonConvert.DeserializeObject<SearchConfig>(json, settings);

                    if (defaultConfig == null)
                        throw new InvalidOperationException("Invalid default config");

                    _configSnapshot = defaultConfig;

                    // Save
                    var outputJson = JsonConvert.SerializeObject(_configSnapshot, Formatting.Indented);
                    File.WriteAllText(_configFilePath, outputJson);
                }
            }
        }


        public static void Save(string json)
        {
            lock (_Lock)
            {
                var tempFile = _configFilePath + ".tmp";

                File.WriteAllText(tempFile, json);
                File.Copy(tempFile, _configFilePath, true);
                File.Delete(tempFile);
            }
        }


        public static IEnumerable<GlobalComponent> GetHeaderComponents()
        {
            return GetGlobalFacets()
                .Concat(GetGlobalCategories())
                .Concat(GetGlobalProperties())
                .Concat(GetGlobalGenerals())
                .Where(c => c.HeaderItem);
        }

        private static List<SearchComponentConfigObj> GetSearchComponentConfigObj(SearchComponentBaseType componentType)
        {
            List<SearchComponentConfigObj> configComponents = new List<SearchComponentConfigObj>();
            foreach (var globalObj in GetGlobalObj(componentType))
            {
                foreach (var localObj in GetLocalObj(componentType))
                {
                    if (globalObj.Id == localObj.GlobalId)
                        configComponents.Add(new SearchComponentConfigObj(globalObj, localObj, componentType));
                }
            }

            return configComponents;
        }

        public static IEnumerable<SearchComponentConfigObj> GetAllHeaderComponents()
        {

            return GetSearchComponentConfigObj(SearchComponentBaseType.Facet)
                .Concat(GetSearchComponentConfigObj(SearchComponentBaseType.Category))
                .Concat(GetSearchComponentConfigObj(SearchComponentBaseType.Property))
                .Concat(GetSearchComponentConfigObj(SearchComponentBaseType.General))
                .Where(c => c.HeaderItem);
        }

        public static IEnumerable<SearchComponentConfigObj> GetAllComponents()
        {

            return GetSearchComponentConfigObj(SearchComponentBaseType.Facet)
                .Concat(GetSearchComponentConfigObj(SearchComponentBaseType.Category))
                .Concat(GetSearchComponentConfigObj(SearchComponentBaseType.Property))
                .Concat(GetSearchComponentConfigObj(SearchComponentBaseType.General));
        }

        public static IEnumerable<GlobalComponent> GetGlobalFacets() => _configSnapshot.Global.SearchComponents.Facets;
        public static IEnumerable<GlobalComponent> GetGlobalCategories() => _configSnapshot.Global.SearchComponents.Categories;
        public static IEnumerable<GlobalComponent> GetGlobalProperties() => _configSnapshot.Global.SearchComponents.Properties;
        public static IEnumerable<GlobalComponent> GetGlobalGenerals() => _configSnapshot.Global.SearchComponents.General;

        public static IEnumerable<LocalComponent> GetLocalFacets()
        {
            return _configSnapshot.Local
                .Where(c => c != null && c.SearchComponents != null && c.SearchComponents.Facets != null)
                .SelectMany(c => c.SearchComponents.Facets);
        }

        public static IEnumerable<LocalComponent> GetLocalCategories()
        {
            return _configSnapshot.Local
                .Where(c => c != null && c.SearchComponents != null && c.SearchComponents.Categories != null)
                .SelectMany(c => c.SearchComponents.Categories);
        }


        public static IEnumerable<LocalComponent> GetLocalProperties()
        {
            return _configSnapshot.Local
                .Where(c => c != null && c.SearchComponents != null && c.SearchComponents.Properties != null)
                .SelectMany(c => c.SearchComponents.Properties);
        }


        public static IEnumerable<LocalComponent> GetLocalGenerals()
        {
            return _configSnapshot.Local
                .Where(c => c != null && c.SearchComponents != null && c.SearchComponents.General != null)
                .SelectMany(c => c.SearchComponents.General);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityTemplateId"></param>
        /// <returns></returns>
        public static LocalConfig GetLocalConfigForEntityTemplate(long entityTemplateId)
        {
            foreach (var localConfig in _configSnapshot.Local)
            {
                if (localConfig.EntityTemplateId == entityTemplateId) return localConfig;
            }
            return null;
        }

        public static GlobalComponent GetGlobalSearchComponentById(int componentId, SearchComponentBaseType type)
        {
            IEnumerable<GlobalComponent> source;
            switch (type)
            {
                case SearchComponentBaseType.Category:
                    source = _configSnapshot.Global.SearchComponents.Categories;
                    break;
                case SearchComponentBaseType.Facet:
                    source = _configSnapshot.Global.SearchComponents.Facets;
                    break;
                case SearchComponentBaseType.General:
                    source = _configSnapshot.Global.SearchComponents.General;
                    break;
                case SearchComponentBaseType.Property:
                    source = _configSnapshot.Global.SearchComponents.Properties;
                    break;
                default:
                    throw new InvalidEnumArgumentException(
                        "Invalid Search Component Type: " + type);
            }

            return source.SingleOrDefault(c => c.Id == componentId);
        }

        public static List<LocalComponent> GetAllLocalSearchComponentsById(int componentId, string componentType)
        {
            List<LocalComponent> result = new List<LocalComponent>();
            foreach (var entityTemplate in _configSnapshot.Local)
            {
                if (componentType == "facet")
                {
                    foreach (var component in entityTemplate.SearchComponents.Facets)
                    {
                        if (component.GlobalId == componentId)
                            result.Add(component);
                    }
                }
                else if (componentType == "category")
                {
                    foreach (var component in entityTemplate.SearchComponents.Categories)
                    {
                        if (component.GlobalId == componentId)
                            result.Add(component);
                    }
                }
                else if (componentType == "property")
                {
                    foreach (var component in entityTemplate.SearchComponents.Properties)
                    {
                        if (component.GlobalId == componentId)
                            result.Add(component);
                        ;
                    }
                }
                else if (componentType == "general")
                {
                    foreach (var component in entityTemplate.SearchComponents.General)
                    {
                        if (component.GlobalId == componentId)
                            result.Add(component);
                    }
                }
            }
            return result;
        }

        private static IEnumerable<GlobalComponent> GetGlobalObj(SearchComponentBaseType type)
        {
            if (type == SearchComponentBaseType.Category)
                return GetGlobalCategories();
            else if (type == SearchComponentBaseType.Facet)
                return GetGlobalFacets();
            else if (type == SearchComponentBaseType.General)
                return GetGlobalGenerals();
            else if (type == SearchComponentBaseType.Property)
                return GetGlobalProperties();
            else
            {
                new InvalidEnumArgumentException("Invalid Search Component Type: " + type);
                return Enumerable.Empty<GlobalComponent>();
            }
        }
        private static IEnumerable<LocalComponent> GetLocalObj(SearchComponentBaseType type)
        {
            if (type == SearchComponentBaseType.Category)
                return GetLocalCategories();
            else if (type == SearchComponentBaseType.Facet)
                return GetLocalFacets();
            else if (type == SearchComponentBaseType.General)
                return GetLocalGenerals();
            else if (type == SearchComponentBaseType.Property)
                return GetLocalProperties();
            else
            {
                new InvalidEnumArgumentException("Invalid Search Component Type: " + type);
                return Enumerable.Empty<LocalComponent>();
            }
        }

        /// <summary>
        /// Returns a List<LocalConfig> colleciton which is basically a list of Entity Templates by comparing globalId from GlobalComponent and the Search ComponentBaseType like Facet, Category,...
        /// </summary>
        /// <param name="localConfigs"></param>
        /// <param name="globalId"></param>
        /// <param name="componentType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidEnumArgumentException"></exception>
        public static List<LocalConfig> GetRelatedLocalConfigsByGlobalId(int globalId, SearchComponentBaseType componentType)
        {
            List<LocalConfig> results = new List<LocalConfig>();
            foreach (var localConfig in _configSnapshot.Local)
            {
                if (componentType == SearchComponentBaseType.Facet)
                {
                    foreach (var component in localConfig.SearchComponents.Facets)
                    {
                        if (globalId == component.GlobalId)
                        {
                            results.Add(localConfig);
                        }
                    }
                }
                else if (componentType == SearchComponentBaseType.Category)
                {
                    foreach (var component in localConfig.SearchComponents.Categories)
                    {
                        if (globalId == component.GlobalId)
                        {
                            results.Add(localConfig);
                        }
                    }
                }
                else if (componentType == SearchComponentBaseType.General)
                {
                    foreach (var component in localConfig.SearchComponents.General)
                    {
                        if (globalId == component.GlobalId)
                        {
                            results.Add(localConfig);
                        }
                    }
                }
                else if (componentType == SearchComponentBaseType.Property)
                {
                    foreach (var component in localConfig.SearchComponents.Properties)
                    {
                        if (globalId == component.GlobalId)
                        {
                            results.Add(localConfig);
                        }
                    }
                }
                else
                {
                    throw new InvalidEnumArgumentException("Search Component Base Type is not supported: " + componentType);
                }
            }
            return results;
        }

        // glaube fehlerhaft, denn die Methode bricht shcon beim ersten Fund ab, mehrere sind aber moeglich?
        public static List<string> GetMetadataNodes(int componentId, string componentType)
        {
            foreach (var entityTemplate in _configSnapshot.Local)
            {
                if (componentType == "facet")
                {
                    foreach (var component in entityTemplate.SearchComponents.Facets)
                    {
                        if (component.GlobalId == componentId)
                        {
                            return component.MetadataNodes;
                        }
                    }
                }
                else if (componentType == "category")
                {
                    foreach (var component in entityTemplate.SearchComponents.Categories)
                    {
                        if (component.GlobalId == componentId)
                            return component.MetadataNodes;
                    }
                }
                else if (componentType == "property")
                {
                    foreach (var component in entityTemplate.SearchComponents.Properties)
                    {
                        if (component.GlobalId == componentId)
                            return component.MetadataNodes;
                    }
                }
                else if (componentType == "general")
                {
                    foreach (var component in entityTemplate.SearchComponents.General)
                    {
                        if (component.GlobalId == componentId)
                            return component.MetadataNodes;
                    }
                }
            }
            return null;
        }

        public static GlobalSearchComponent GetGlobalSearchComponent()
        {
            return _configSnapshot.Global.SearchComponents;
        }

        public static GlobalConfig GetGlobal() => _configSnapshot.Global;
        public static LocalConfig GetLocal(long entityTemplateId) => _configSnapshot.Local.FirstOrDefault(et => et.EntityTemplateId == entityTemplateId);

        public static Dictionary<GlobalSearchComponent, List<LocalConfig>> GetLocalConfigForComponent(string componentType, int componentId)
        {
            var result = new Dictionary<GlobalSearchComponent, List<LocalConfig>>();

            if (componentType == "facet")
            {

            }
            else if (componentType == "categorie")
            {
            }
            else if (componentType == "property")
            {
            }
            else if (componentType == "general")
            {
            }
            else
            {
                // Error! ComponentType not supported!
            }

                return result;
        }




        public static IEnumerable<Facet> GetFacets()
        {
            List<GlobalComponent> facets = _configSnapshot.Global.SearchComponents.Facets;
            List<Facet> result = new List<Facet>();
            foreach (GlobalComponent fac in facets)
            {
                Facet facet = new Facet();
                facet.Name = fac.ComponentName;
                facet.Text = fac.ComponentName;
                facet.Value = fac.ComponentName;
                facet.DisplayName = fac.ComponentName;


                result.Add(facet);
            }

            return result;
        }

        public static IEnumerable<Category> GetCategories()
        {
            List<GlobalComponent> globalCats = _configSnapshot.Global.SearchComponents.Categories;
            List<Category> result = new List<Category>();

            foreach (GlobalComponent component in globalCats)
            {
                Category category = new Category();
                category.Name = component.ComponentName;
                category.DisplayName = component.ComponentName;
                category.Value = component.ComponentName;
                category.DefaultValue = "nothing";
                result.Add(category);

            }
            return result;
        }

        public static IEnumerable<General> GetGenerals()
        {
            List<GlobalComponent> globalComponents = _configSnapshot.Global.SearchComponents.General;
            List<General> result = new List<General>();

            return result;
        }

        public static IEnumerable<Property> GetProperties()
        {
            List<Property> props = new List<Property>();
            List<GlobalComponent> propComponent = _configSnapshot.Global.SearchComponents.Properties;
            foreach (GlobalComponent component in propComponent) {
                Property property = new Property();
                property.Name = component.ComponentName;
                property.DisplayName = component.ComponentName;

            }
            return props;

        }
    }

}
