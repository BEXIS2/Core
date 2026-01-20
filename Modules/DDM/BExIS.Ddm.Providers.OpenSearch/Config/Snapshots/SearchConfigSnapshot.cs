using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Utils.Models;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class SearchConfigSnapshot
    {
        public GlobalSnapshot Global { get; }
        public IReadOnlyList<LocalSnapshot> Local { get; }

        private SearchConfigSnapshot(GlobalSnapshot global, IReadOnlyList<LocalSnapshot> local)
        {
            Global = global;
            Local = local;
        }

        public static SearchConfigSnapshot FromDto(SearchConfig dto)
        {
            return new SearchConfigSnapshot(
                GlobalSnapshot.FromDto(dto.Global),
                dto.Local.Select(LocalSnapshot.FromDto)
                         .ToList()
                         .AsReadOnly()
            );

        }

    //public IReadOnlyList<Facet> Facets { get; }
    //public IReadOnlyList<Property> Properties { get; }
    //public IReadOnlyList<Category> Categories { get; }

    //public IReadOnlyList<SearchComponentBase> HeaderItems { get; }
    //public ISet<string> NumericProperties { get; }

    //public int EntityTemplateId { get; }

    //public SearchConfigSnapshot(
    //    int entityTemplateId,
    //    IEnumerable<Facet> facets,
    //    IEnumerable<Property> properties,
    //    IEnumerable<Category> categories,
    //    IEnumerable<SearchComponentBase> headerItems,
    //    IEnumerable<string> numericProperties)
    //{
    //    EntityTemplateId = entityTemplateId;
    //    Facets = facets.ToList().AsReadOnly();
    //    Properties = properties.ToList().AsReadOnly();
    //    Categories = categories.ToList().AsReadOnly();
    //    HeaderItems = headerItems.ToList().AsReadOnly();
    //    NumericProperties = new HashSet<string>(numericProperties);
    //}
    }
}
