using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class LocalSearchComponentSnapshot
    {
        public IReadOnlyList<LocalComponentSnapshot> Facets { get; }
        public IReadOnlyList<LocalComponentSnapshot> Categories { get; }
        public IReadOnlyList<LocalComponentSnapshot> Properties { get; }
        public IReadOnlyList<LocalComponentSnapshot> General { get; }

        private LocalSearchComponentSnapshot(
            IReadOnlyList<LocalComponentSnapshot> facets,
            IReadOnlyList<LocalComponentSnapshot> categories,
            IReadOnlyList<LocalComponentSnapshot> properties,
            IReadOnlyList<LocalComponentSnapshot> general)
        {
            Facets = facets;
            Categories = categories;
            Properties = properties;
            General = general;
        }

        public static LocalSearchComponentSnapshot FromDto(LocalSearchComponent dto)
        {
            return new LocalSearchComponentSnapshot(
                dto.Facets.Select(LocalComponentSnapshot.FromDto).ToList().AsReadOnly(),
                dto.Categories.Select(LocalComponentSnapshot.FromDto).ToList().AsReadOnly(),
                dto.Properties.Select(LocalComponentSnapshot.FromDto).ToList().AsReadOnly(),
                dto.General.Select(LocalComponentSnapshot.FromDto).ToList().AsReadOnly()
            );
        }
    }

}
