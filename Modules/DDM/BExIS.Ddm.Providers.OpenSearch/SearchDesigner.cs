using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BExIS.Ddm.Api;
using BExIS.Ddm.Providers.OpenSearch.Config;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Utils.Models;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Vaiona.Utils.Cfg;

namespace BExIS.Ddm.Providers.OpenSearch
{
    public class SearchDesigner : ISearchDesigner
    {
        private List<SearchAttribute> _searchAttributeList = new List<SearchAttribute>();
        private List<SearchMetadataNode> _metadataNodes = new List<SearchMetadataNode>();
        private XmlMetadataHelper xmlMetadataHelper = new XmlMetadataHelper();

        private bool _includePrimaryData = false;

        private OpenSearchIndexer _indexer = new OpenSearchIndexer();

        public List<SearchAttribute> Get()
        {
            this.Load();
            this._metadataNodes = new List<SearchMetadataNode>();
            return this._searchAttributeList;
        }

        private void Load()
        {
            int index = 0;
            var entityTemplateNames = GetEntityTemplatesAsDict();

            LoadComponentType(
                SearchComponentBaseType.Facet,
                SearchConfigManager.GetGlobalFacets(),
                (lc) => lc.SearchComponents.Facets,
                ref index,
                entityTemplateNames);

            LoadComponentType(
                SearchComponentBaseType.Category,
                SearchConfigManager.GetGlobalCategories(),
                (lc) => lc.SearchComponents.Categories,
                ref index,
                entityTemplateNames);

            LoadComponentType(
                SearchComponentBaseType.General,
                SearchConfigManager.GetGlobalGenerals(),
                (lc) => lc.SearchComponents.General,
                ref index,
                entityTemplateNames);

            LoadComponentType(
                SearchComponentBaseType.Property,
                SearchConfigManager.GetGlobalProperties(),
                (lc) => lc.SearchComponents.Properties,
                ref index,
                entityTemplateNames);
        }
        private void LoadComponentType(
            SearchComponentBaseType type,
            IEnumerable<GlobalComponent> globalComponents,
            Func<LocalConfig, IEnumerable<LocalComponent>> localSelector,
            ref int index,
            Dictionary<long , string> entityTemplateNames)
        {
            foreach (var globalComponent in globalComponents)
            {
                var localConfigs =
                    SearchConfigManager.GetRelatedLocalConfigsByGlobalId(globalComponent.Id, type);

                foreach (var localConfig in localConfigs)
                {
                    var etName = entityTemplateNames.TryGetValue(
                        localConfig.EntityTemplateId, out var name)
                        ? name
                        : null;

                    foreach (var localComponent in localSelector(localConfig)
                             .Where(c => c.GlobalId == globalComponent.Id))
                    {
                        var sa = SearchViewModelMapper.ToSearchAttribute(
                            index,
                            globalComponent,
                            localComponent,
                            type,
                            localConfig.EntityTemplateId,
                            etName);

                        _searchAttributeList.Add(sa);
                        index++;
                    }
                }
            }
        }

        private static Dictionary<long, string> GetEntityTemplatesAsDict()
        {
            using (var manager = new EntityTemplateManager())
            {
                return manager.Repo
                    .Query(e => e.Activated)
                    .ToDictionary(e => e.Id, e => e.Name);
            }
        }

        private static List<EntityTemplate> GetEntityTemplates()
        {

            using (var entityTemplateManager = new EntityTemplateManager())
            {
                return entityTemplateManager.Repo
                    .Query(e => e.Activated)
                    .ToList();
            }
        }

        public bool IsPrimaryDataIncluded()
        {
            return this._includePrimaryData;
        }

        public List<SearchMetadataNode> GetMetadataNodes()
        {
            if (_metadataNodes.Count > 0)
                return _metadataNodes;
            else
            {
                using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
                {
                    List<long> ids = new List<long>();

                    ids = metadataStructureManager.Repo.Query().Select(p => p.Id).ToList();

                    foreach (long id in ids)
                    {
                        _metadataNodes.AddRange(GetAllXPathsOfSimpleAttributes(id));
                    }

                    _metadataNodes = _metadataNodes.Distinct().ToList();
                    _metadataNodes.Sort((x, y) => String.Compare(x.DisplayName, y.DisplayName));
                    return _metadataNodes;
                }
            }
        }

        private static string GetEntityTemplateName(long id)
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            {
                return entityTemplateManager.Repo
                    .Query(e => e.Activated && e.Id == id)
                    .Select(e => e.Name)
                    .FirstOrDefault();
            }
        }

        public List<SearchMetadataNode> GetAllXPathsOfSimpleAttributes(long id)
        {
            return xmlMetadataHelper.GetAllXPathsOfSimpleAttributes(id);
        }

        public void Set(List<SearchAttribute> SearchAttributeList)
        {
            // deprecated: old coding model, should be removed in future, but necessary for now
            this._searchAttributeList = SearchAttributeList;
            // parse old VielModel to new SearchConfig obj format
            var newConfig = SearchViewModelMapper.ToSearchConfig(SearchAttributeList);
            //newConfig.IncludePrimaryData = includePrimaryData;

            // convert to json string
            var json = newConfig.ToJson();

            // validate 
            if (!SearchConfigManager.ValidateConfig(json))
            {
                throw new InvalidOperationException("Config is invalid");
            }

            // save 
            SearchConfigManager.Save(json);
        }

        //public void Set(List<SearchAttribute> SearchAttributeList, bool includePrimaryData)
        //{
        //    this._searchAttributeList = SearchAttributeList;

        //    var newConfig = SearchViewModelMapper.ToSearchConfig(SearchAttributeList);
        //    this._includePrimaryData = includePrimaryData;
        //    Save();
        //}
        public void Set(List<SearchAttribute> searchAttributeList, bool includePrimaryData)
        {
            // deprecated: old coding model, should be removed in future, but necessary for now
            this._searchAttributeList = searchAttributeList;

            // parse old VielModel to new SearchConfig obj format
            //var newConfig = SearchViewModelMapper.ToSearchConfig(searchAttributeList);

            // FOR TESTING: Loading an edited configuration
            var edit_config = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "OpenSearch", "Config", "edit_SearchConfig.json");
            if (!File.Exists(edit_config))
                return;
            var json_str = File.ReadAllText(edit_config);
            var settings = new JsonSerializerSettings
            {
                Converters =
                {
                    new StringEnumConverter(),
                    // new CalcBlockListConverter()
                    new SpatialMetadataConverter()
                }
            };

            var newConfig = JsonConvert.DeserializeObject<SearchConfig>(json_str, settings);

            if (newConfig == null)
                throw new InvalidOperationException("Invalid config");

            //newConfig.IncludePrimaryData = includePrimaryData;

            // convert to json string
            var json = newConfig.ToJson();

            // validate 
            if (!SearchConfigManager.ValidateConfig(json))
            {
                throw new InvalidOperationException("Config is invalid");
            }

            // save 
            SearchConfigManager.Save(json);
            SearchConfigManager.Reload();

            // for testing integrated idx
            //_indexer.IndexIntegratedDatasets(2,false);

            // testing spatial data
            var xml = @"
                <root>
                    <west>-10</west>
                    <east>10</east>
                    <north>50</north>
                    <south>40</south>
                    <lat>51.0</lat>
                    <lon>11.0</lon>
                    <radius>5</radius>
                </root>";

            var metadata = new XmlDocument();
            metadata.LoadXml(xml);
            var doc = new Dictionary<string, object>();

            _indexer.AppendBoundingBoxSpatialData(SearchConfigManager.GetLocalConfigForEntityTemplate(2), metadata, doc);

            _indexer.TestToIdx(doc);
        }


        /// <summary>
        /// Method locks, than load default config and save to config file path -> Release Lock
        /// </summary>
        public void Reset()
        {
            SearchConfigManager.RestoreDefaultConfig();
        }

        public void Reload(bool onlyReleasedTags)
        {
            try
            {
                _indexer.ReIndex(onlyReleasedTags);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Dispose()
        {
            _indexer.Dispose();
        }
    }
}
