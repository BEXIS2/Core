using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public abstract class SpatialMetadataSnapshot
    {
        public string Type { get; private set; }

        protected SpatialMetadataSnapshot(string type)
        {
            Type = type;
        }

        public static SpatialMetadataSnapshot FromDto(SpatialMetadata dto)
        {
            if (dto == null)
                throw null;

            var bbox = dto as BBoxSpatialMetadata;
            if (bbox != null)
                return BBoxSpatialMetadataSnapshot.FromDto(bbox);

            var point = dto as PointSpatialMetadata;
            if (point != null)
                return PointSpatialMetadataSnapshot.FromDto(point);

            throw new InvalidOperationException(
                "Unknown spatial metadata dto type: " + dto.GetType().FullName);
        }
    }


}
