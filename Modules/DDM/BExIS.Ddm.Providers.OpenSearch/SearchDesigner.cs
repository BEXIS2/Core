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

        //read json config file
        //private void Load()
        //{
        //    int index = 0;
        //    var entityTemplates = GetEntityTemplatesAsDict();


        //    // Looking for Facets in config
        //    foreach (var globalComponent in SearchConfigManager.GetGlobalFacets())
        //    {
        //        var localConfigs = SearchConfigManager.GetRelatedLocalConfigsByGlobalId(globalComponent.Id, SearchComponentBaseType.Facet);
        //        foreach (var localConfig in localConfigs)
        //        {
        //            foreach (var facet in localConfig.SearchComponents.Facets)
        //            {
        //                if (facet.GlobalId == globalComponent.Id)
        //                {
        //                    var etName = GetEntityTemplateName(localConfig.EntityTemplateId);
        //                    var sa =  SearchViewModelMapper.ToSearchAttribute(index, globalComponent, facet, SearchComponentBaseType.Facet, localConfig.EntityTemplateId, etName);
        //                    _searchAttributeList.Add(sa);
        //                    index++;
        //                }
        //            }
        //        }
        //    }

        //    // Categories
        //    foreach (var globalComponent in SearchConfigManager.GetGlobalCategories())
        //    {
        //        var localConfigs = SearchConfigManager.GetRelatedLocalConfigsByGlobalId(globalComponent.Id, SearchComponentBaseType.Category);
        //        foreach (var localConfig in localConfigs)
        //        {
        //            foreach (var category in localConfig.SearchComponents.Categories)
        //            {
        //                if (category.GlobalId == globalComponent.Id)
        //                {
        //                    var etName = GetEntityTemplateName(localConfig.EntityTemplateId);
        //                    var sa = SearchViewModelMapper.ToSearchAttribute(index, globalComponent, category, SearchComponentBaseType.Category, localConfig.EntityTemplateId, etName);
        //                    _searchAttributeList.Add(sa);
        //                    index++;
        //                }
        //            }
        //        }
        //    }

        //    // Generals
        //    foreach (var globalComponent in SearchConfigManager.GetGlobalGenerals())
        //    {
        //        var localConfigs = SearchConfigManager.GetRelatedLocalConfigsByGlobalId(globalComponent.Id, SearchComponentBaseType.General);
        //        foreach (var localConfig in localConfigs)
        //        {
        //            foreach (var general in localConfig.SearchComponents.General)
        //            {
        //                if (general.GlobalId == globalComponent.Id)
        //                {
        //                    var etName = GetEntityTemplateName(localConfig.EntityTemplateId);
        //                    var sa = SearchViewModelMapper.ToSearchAttribute(index, globalComponent, general, SearchComponentBaseType.General, localConfig.EntityTemplateId, etName);
        //                    _searchAttributeList.Add(sa);
        //                    index++;
        //                }
        //            }
        //        }
        //    }

        //    // Properties
        //    foreach (var globalComponent in SearchConfigManager.GetGlobalProperties())
        //    {
        //        var localConfigs = SearchConfigManager.GetRelatedLocalConfigsByGlobalId(globalComponent.Id, SearchComponentBaseType.Property);
        //        foreach (var localConfig in localConfigs)
        //        {
        //            foreach (var property in localConfig.SearchComponents.Properties)
        //            {
        //                if (property.GlobalId == globalComponent.Id)
        //                {
        //                    var etName = GetEntityTemplateName(localConfig.EntityTemplateId);
        //                    var sa = SearchViewModelMapper.ToSearchAttribute(index, globalComponent, property, SearchComponentBaseType.Property, localConfig.EntityTemplateId, etName);
        //                    _searchAttributeList.Add(sa);
        //                    index++;
        //                }
        //            }
        //        }
        //    }
        //}

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
            this._searchAttributeList = SearchAttributeList;
            Save();
        }

        public void Set(List<SearchAttribute> SearchAttributeList, bool includePrimaryData)
        {
            this._searchAttributeList = SearchAttributeList;

            var newConfig = SearchViewModelMapper.ToSearchConfig(SearchAttributeList);
            this._includePrimaryData = includePrimaryData;
            Save();
        }

        // write xml config file
        private void Save()
        {
            SearchConfigManager.Save();
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
