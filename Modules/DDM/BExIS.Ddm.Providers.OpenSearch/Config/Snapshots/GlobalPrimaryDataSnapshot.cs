using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class GlobalPrimaryDataSnapshot : IPrimaryDataSnapshot
    {
        public bool ToIndex { get; }
        public IReadOnlyList<PrimaryDataAggregationSnapshot> Calc { get; }
        public GlobalPrimarySpatialDataSnapshot SpatialData { get; }

        private GlobalPrimaryDataSnapshot(
            bool toIndex,
            IReadOnlyList<PrimaryDataAggregationSnapshot> calc,
            GlobalPrimarySpatialDataSnapshot spatialData)
        {
            ToIndex = toIndex;
            Calc = calc;
            SpatialData = spatialData;
        }

        public static GlobalPrimaryDataSnapshot FromDto(GlobalPrimaryData dto)
        {
            return new GlobalPrimaryDataSnapshot(
                dto.ToIndex,
                dto.Calc.Select(PrimaryDataAggregationSnapshot.FromDto).ToList().AsReadOnly(),
                GlobalPrimarySpatialDataSnapshot.FromDto(dto.SpatialData)
            );
        }
    }
}
