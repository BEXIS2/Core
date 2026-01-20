using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class GlobalComponentSnapshot
    {
        public int Id { get; }
        public string ComponentName { get; }
        public string Description { get; }
        public string Placeholder { get; }
        public bool HeaderItem { get; }
        public bool DefaultHeaderItem { get; }

        private GlobalComponentSnapshot(
            int id,
            string componentName,
            string description,
            string placeholder,
            bool headerItem,
            bool defaultHeaderItem)
        {
            Id = id;
            ComponentName = componentName;
            Description = description;
            Placeholder = placeholder;
            HeaderItem = headerItem;
            DefaultHeaderItem = defaultHeaderItem;
        }

        public static GlobalComponentSnapshot FromDto(GlobalComponent dto)
        {
            return new GlobalComponentSnapshot(
                dto.Id,
                dto.ComponentName,
                dto.Description,
                dto.Placeholder,
                dto.HeaderItem,
                dto.DefaultHeaderItem
            );
        }
    }

}
