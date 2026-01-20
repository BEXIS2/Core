using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class LocalSpatialDataSnapshot
    {
        public SpatialMetadataSnapshot SpatialMetadata { get; }

        private LocalSpatialDataSnapshot(SpatialMetadataSnapshot spatialMetadata)
        {
            SpatialMetadata = spatialMetadata;
        }

        public static LocalSpatialDataSnapshot FromDto(LocalSpatialData dto)
            => new LocalSpatialDataSnapshot(
                SpatialMetadataSnapshot.FromDto(dto.SpatialMetadata)
            );
    }

}
