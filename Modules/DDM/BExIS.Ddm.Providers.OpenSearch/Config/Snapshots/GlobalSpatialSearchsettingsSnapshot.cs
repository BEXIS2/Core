using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Ddm.Providers.OpenSearch.Config.enums;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public class GlobalSpatialSearchsettingsSnapshot
    {
        public CrsType Crs { get; }
        public IReadOnlyList<string> AxisOrder { get; }
        public BasemapType Basemap { get; }
        public string StartExtend { get; }

        private GlobalSpatialSearchsettingsSnapshot(CrsType crs, IReadOnlyList<string> axisOrder, BasemapType basemap, string startExtend)
        {
            Crs = crs;
            AxisOrder = axisOrder;
            Basemap = basemap;
            StartExtend = startExtend;
        }

        public static GlobalSpatialSearchsettingsSnapshot FromDto(GlobalSpatialSearchSettings dto)
        {
            return new GlobalSpatialSearchsettingsSnapshot(
                dto.Crs,
                dto.AxisOrder,
                dto.Basemap,
                dto.StartExtend
            );
        }
    }
}
