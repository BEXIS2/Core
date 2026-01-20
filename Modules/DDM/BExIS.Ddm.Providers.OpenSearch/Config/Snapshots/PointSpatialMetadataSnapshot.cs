using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class PointSpatialMetadataSnapshot : SpatialMetadataSnapshot
    {
        public double Longitude { get; }
        public double Latitude { get; }
        public double Radius { get; }

        private PointSpatialMetadataSnapshot(
            double longitude,
            double latitude,
            double radius)
            : base("point")
        {
            Longitude = longitude;
            Latitude = latitude;
            Radius = radius;
        }

        public static PointSpatialMetadataSnapshot FromDto(PointSpatialMetadata dto)
            => new PointSpatialMetadataSnapshot(
                dto.Longitude,
                dto.Latitude,
                dto.Radius
            );
    }

}
