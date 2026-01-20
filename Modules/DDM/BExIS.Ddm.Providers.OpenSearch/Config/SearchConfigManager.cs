using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BExIS.Utils.Models;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;  
using Newtonsoft.Json.Converters;
using Vaiona.Utils.Cfg;
using Category = BExIS.Utils.Models.Category;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public static class SearchConfigManager
    {
        private static string _configFilePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "OpenSearch", "Config", "SearchConfig.json");

        private static volatile SearchConfig _configSnapshot;
        private static readonly object _Lock = new object();

        public static SearchConfig GetSnapshot()
        {
            if (_configSnapshot == null)
            {
                Reload();
            }
            return _configSnapshot;

        }

        public static void Reload()
        {
            lock (_Lock)
            {
                var json = File.ReadAllText(_configFilePath);

                var settings = new JsonSerializerSettings
                {
                    Converters =
                {
                    new StringEnumConverter(),
                    //new CalcBlockListConverter()
                }
                };

                var newConfig = JsonConvert.DeserializeObject<SearchConfig>(json, settings);

                if (newConfig == null)
                    throw new InvalidOperationException("Invalid config");

                Interlocked.Exchange(ref _configSnapshot, newConfig);
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

        public static Dictionary<string, GlobalComponent> GetHeaderComponentsAsDict()
        {
            Dictionary<string, GlobalComponent> globalComponentsDict = new Dictionary<string, GlobalComponent>();

            var facets = GetGlobalFacets();
            foreach (var component in facets)
            {
                if (component.HeaderItem)
                    globalComponentsDict.Add("facet", component);
            }
            var categories = GetGlobalCategories();
            foreach (var component in categories)
            {
                if (component.HeaderItem)
                    globalComponentsDict.Add("category", component);
            }
            var props = GetGlobalProperties();
            foreach (var component in props)
            {
                if (component.HeaderItem)
                    globalComponentsDict.Add("property", component);
            }
            var generals = GetGlobalGenerals();
            foreach (var component in generals)
            {
                if (component.HeaderItem)
                    globalComponentsDict.Add("general", component);
            }


            return globalComponentsDict;
        }

        public static Dictionary<string, GlobalComponent> GetGlobalComponentsAsDict()
        {
            Dictionary<string, GlobalComponent> globalComponentsDict = new Dictionary<string, GlobalComponent>();

            var facets = GetGlobalFacets();
            foreach (var component in facets)
                globalComponentsDict.Add("facet", component);

            var categories = GetGlobalCategories();
            foreach (var component in categories)
                globalComponentsDict.Add("category", component);
            
            var props = GetGlobalProperties();
            foreach (var component in props)
                globalComponentsDict.Add("property", component);
            
            var generals = GetGlobalGenerals();
            foreach (var component in generals)
                globalComponentsDict.Add("general", component);
            
            return globalComponentsDict;
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
                .SelectMany(c => c.SearchComponents.Facets);
        }


        public static IEnumerable<LocalComponent> GetLocalProperties()
        {
            return _configSnapshot.Local
                .Where(c => c != null && c.SearchComponents != null && c.SearchComponents.Properties != null)
                .SelectMany(c => c.SearchComponents.Facets);
        }


        public static IEnumerable<LocalComponent> GetLocalGenerals()
        {
            return _configSnapshot.Local
                .Where(c => c != null && c.SearchComponents != null && c.SearchComponents.General != null)
                .SelectMany(c => c.SearchComponents.Facets);
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




        // nullable
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
