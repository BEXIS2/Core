using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public class GlobalSearchComponentSnapshot
    {
        public bool FacetsToIndex { get; set; }
        public bool CategoriesToIndex { get; set; }
        public bool PropertiesToIndex { get; set; }
        public bool GeneralsToIndex { get; set; }
        public IReadOnlyList<GlobalComponentSnapshot> Facets { get; }
        public IReadOnlyList<GlobalComponentSnapshot> Categories { get; }
        public IReadOnlyList<GlobalComponentSnapshot> Properties { get; }
        public IReadOnlyList<GlobalComponentSnapshot> General { get; }

        private GlobalSearchComponentSnapshot(
            bool facetsToIndex,
            bool categoriesToIndex,
            bool propertiesToIndex,
            bool generalsToIndex,
            IReadOnlyList<GlobalComponentSnapshot> facets,
            IReadOnlyList<GlobalComponentSnapshot> categories,
            IReadOnlyList<GlobalComponentSnapshot> properties,
            IReadOnlyList<GlobalComponentSnapshot> general)
        {
            FacetsToIndex = facetsToIndex;
            CategoriesToIndex = categoriesToIndex;
            PropertiesToIndex = propertiesToIndex;
            GeneralsToIndex = generalsToIndex;
            Facets = facets;
            Categories = categories;
            Properties = properties;
            General = general;
        }

        public static GlobalSearchComponentSnapshot FromDto(GlobalSearchComponent dto)
        {
            return new GlobalSearchComponentSnapshot(
                dto.FacetsToIndex,
                dto.CategoriesToIndex,
                dto.PropertiesToIndex,
                dto.GeneralsToIndex,
                dto.Facets.Select(GlobalComponentSnapshot.FromDto).ToList().AsReadOnly(),
                dto.Categories.Select(GlobalComponentSnapshot.FromDto).ToList().AsReadOnly(),
                dto.Properties.Select(GlobalComponentSnapshot.FromDto).ToList().AsReadOnly(),
                dto.General.Select(GlobalComponentSnapshot.FromDto).ToList().AsReadOnly()
            );
        }
    }
}
