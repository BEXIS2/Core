using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class LocalSnapshot
    {
        public int EntityTemplateId { get; }
        public bool IndexNotCompletedMetadata { get; }
        public LocalSearchComponentSnapshot SearchComponents { get; }
        public LocalSpatialDataSnapshot SpatialData { get; }
        public LocalPrimaryDataSnapshot PrimaryData { get; }
        public ExternalSourceSnapshot ExternalSources { get; }

        private LocalSnapshot(
            int entityTemplateId,
            bool indexNotCompletedMetadata,
            LocalSearchComponentSnapshot searchComponents,
            LocalSpatialDataSnapshot spatialData,
            LocalPrimaryDataSnapshot primaryData,
            ExternalSourceSnapshot externalSources)
        {
            EntityTemplateId = entityTemplateId;
            IndexNotCompletedMetadata = indexNotCompletedMetadata;
            SearchComponents = searchComponents;
            SpatialData = spatialData;
            PrimaryData = primaryData;
            ExternalSources = externalSources;
        }

        public static LocalSnapshot FromDto(LocalConfig dto)
        {
            return new LocalSnapshot(
                dto.EntityTemplateId,
                dto.IndexNotCompletedMetadata,
                LocalSearchComponentSnapshot.FromDto(dto.SearchComponents),
                LocalSpatialDataSnapshot.FromDto(dto.SpatialData),
                LocalPrimaryDataSnapshot.FromDto(dto.PrimaryData),
                ExternalSourceSnapshot.FromDto(dto.ExternalSource)
            );
        }
    }

}
