using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class BBoxSpatialMetadataSnapshot : SpatialMetadataSnapshot
    {
        public string WestBoundLongitude { get; }
        public string EastBoundLongitude { get; }
        public string SouthBoundLatitude { get; }
        public string NorthBoundLatitude { get; }

        private BBoxSpatialMetadataSnapshot(
            string west,
            string east,
            string south,
            string north)
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
