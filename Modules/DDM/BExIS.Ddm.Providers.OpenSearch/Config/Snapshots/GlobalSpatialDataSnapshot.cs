using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public class GlobalSpatialDataSnapshot
    {
        public bool SpatialSearch { get; }
        public GlobalSpatialSearchsettingsSnapshot Settings { get; }

        private GlobalSpatialDataSnapshot(bool spatialSearch, GlobalSpatialSearchsettingsSnapshot settings)
        {
            SpatialSearch = spatialSearch;
            Settings = settings;
        }

        public static GlobalSpatialDataSnapshot FromDto(GlobalSpatialData dto)
        {
            return new GlobalSpatialDataSnapshot(
                dto.SpatialSearch,
                GlobalSpatialSearchsettingsSnapshot.FromDto(dto.SpatialSearchSettings)
            );
        }
    }
}
