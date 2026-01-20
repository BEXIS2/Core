using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Ddm.Providers.OpenSearch.Config.enums;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class SearchComponentConfigObj
    {
        private readonly int id;
        private readonly string _componentName;
        private readonly string _description;
        private readonly string _placeholder;
        private readonly bool _headerItem;
        private readonly bool _defaultHeaderItem;
        private readonly EntityTemplate _entityTemplate;
        private readonly DataTypeId _dataTypeId;
        private readonly List<string> _metadataNodes;
        private readonly SearchComponentBaseType _componentType;


        public SearchComponentConfigObj(GlobalComponent globalObj, LocalComponent localObj, SearchComponentBaseType componentType)
        {
            id = globalObj.Id;
            _componentName = globalObj.ComponentName;
            _description = globalObj.Description;
            _placeholder = globalObj.Placeholder;
            _headerItem = globalObj.HeaderItem;
            _defaultHeaderItem = globalObj.DefaultHeaderItem;
            _dataTypeId = localObj.DataTypeId;
            _metadataNodes = localObj.MetadataNodes;
            _componentType = componentType;
        }
    }
}
