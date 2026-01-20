using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public interface IPrimaryDataSnapshot
    {        bool ToIndex { get; }
        IReadOnlyList<PrimaryDataAggregationSnapshot> Calc { get; }
    }
}
