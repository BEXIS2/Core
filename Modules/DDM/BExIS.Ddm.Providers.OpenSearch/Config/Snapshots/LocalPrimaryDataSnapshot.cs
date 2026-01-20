using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class LocalPrimaryDataSnapshot : IPrimaryDataSnapshot
    {
        public bool ToIndex { get; }
        public IReadOnlyList<PrimaryDataAggregationSnapshot> Calc { get; }

        private LocalPrimaryDataSnapshot(
            bool toIndex,
            IReadOnlyList<PrimaryDataAggregationSnapshot> calc)
        {
            ToIndex = toIndex;
            Calc = calc;
        }

        public static LocalPrimaryDataSnapshot FromDto(LocalPrimaryData dto)
        {
            return new LocalPrimaryDataSnapshot(
                dto.ToIndex,
                dto.Calc.Select(PrimaryDataAggregationSnapshot.FromDto).ToList().AsReadOnly()
            );
        }
    }
}
