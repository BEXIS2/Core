using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Ddm.Providers.OpenSearch.Config.enums;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Utils.Models;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class SearchComponentConfigObj
    {
        public readonly int Id;
        public readonly string ComponentName;
        public readonly string Description;
        public readonly string Placeholder;
        public readonly bool HeaderItem;
        public readonly bool DefaultHeaderItem;
        public readonly long EntityTemplateId;
        public readonly string EntityTemplateName;
        public readonly DataTypeId DataTypeId;
        public readonly List<string> MetadataNodes;
        public readonly SearchComponentBaseType ComponentType;


        public SearchComponentConfigObj(GlobalComponent globalObj, LocalComponent localObj, SearchComponentBaseType componentType)
        {
            Id = globalObj.Id;
            ComponentName = globalObj.ComponentName;
            Description = globalObj.Description;
            Placeholder = globalObj.Placeholder;
            HeaderItem = globalObj.HeaderItem;
            DefaultHeaderItem = globalObj.DefaultHeaderItem;
            DataTypeId = localObj.DataTypeId;
            MetadataNodes = localObj.MetadataNodes;
            ComponentType = componentType;
        }

        private static List<EntityTemplate> GetEntityTemplates(long id)
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            {
                return entityTemplateManager.Repo
                    .Query(e => e.Activated)
                    .ToList();
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
    }
}
