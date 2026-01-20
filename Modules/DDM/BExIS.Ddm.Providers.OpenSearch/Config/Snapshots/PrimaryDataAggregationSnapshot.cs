using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Ddm.Providers.OpenSearch.Config.enums;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public class PrimaryDataAggregationSnapshot
    {
        public AggregationOperator Operation { get; set; }
        public IReadOnlyList<int> AllowedMeanings { get; set; } = new List<int>();

        private PrimaryDataAggregationSnapshot(AggregationOperator operation, IReadOnlyList<int> allowedMeanings)
        {
            Operation = operation;
            AllowedMeanings = allowedMeanings;
        }
        public static PrimaryDataAggregationSnapshot FromDto(PrimaryDataAggregation dto)
        {
            return new PrimaryDataAggregationSnapshot(
                dto.Operation,
                dto.AllowedMeanings.ToList().AsReadOnly()
            );
        }
    }
}
