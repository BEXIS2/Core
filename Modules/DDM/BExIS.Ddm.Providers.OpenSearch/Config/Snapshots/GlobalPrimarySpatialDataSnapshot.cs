using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class GlobalPrimarySpatialDataSnapshot
    {
        public IReadOnlyList<int> AllowedDataTypeIds { get; }
        public int LatMeaning { get; }
        public int LongMeaning { get; }

        private GlobalPrimarySpatialDataSnapshot(
            IReadOnlyList<int> allowedDataTypeIds,
            int latMeaning,
            int longMeaning)
        {
            AllowedDataTypeIds = allowedDataTypeIds;
            LatMeaning = latMeaning;
            LongMeaning = longMeaning;
        }

        public static GlobalPrimarySpatialDataSnapshot FromDto(GlobalSpatialPrimaryData dto)
        {
            if (dto == null) return null;

            return new GlobalPrimarySpatialDataSnapshot(
                dto.AllowedDataTypeIds.ToList().AsReadOnly(),
                dto.LatMeaning,
                dto.LongMeaning
            );
        }
    }
}
