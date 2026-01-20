using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    internal interface IPrimaryData
    {
        bool ToIndex { get; }
        List<PrimaryDataAggregation> Calc { get; }
    }
}
