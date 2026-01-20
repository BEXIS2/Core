using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class BBoxSpatialMetadataSnapshot : SpatialMetadataSnapshot
    {
        public double WestBoundLongitude { get; }
        public double EastBoundLongitude { get; }
        public double SouthBoundLatitude { get; }
        public double NorthBoundLatitude { get; }

        private BBoxSpatialMetadataSnapshot(
            double west,
            double east,
            double south,
            double north)
            : base("bbox")
        {
            WestBoundLongitude = west;
            EastBoundLongitude = east;
            SouthBoundLatitude = south;
            NorthBoundLatitude = north;
        }

        public static BBoxSpatialMetadataSnapshot FromDto(BBoxSpatialMetadata dto)
            => new BBoxSpatialMetadataSnapshot(
                dto.WestBoundLongitude,
                dto.EastBoundLongitude,
                dto.SouthBoundLatitude,
                dto.NorthBoundLatitude
            );
    }

}
