using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class PointSpatialMetadataSnapshot : SpatialMetadataSnapshot
    {
        public string Longitude { get; }
        public string Latitude { get; }
        public string Radius { get; }

        private PointSpatialMetadataSnapshot(
            string longitude,
            string latitude,
            string radius)
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
