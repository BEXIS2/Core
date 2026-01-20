using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class GeneralGlobalSnapshot
    {
        public bool ShowOnlyCompletedMetadata { get; }

        private GeneralGlobalSnapshot(bool showOnlyCompletedMetadata)
        {
            ShowOnlyCompletedMetadata = showOnlyCompletedMetadata;
        }

        public static GeneralGlobalSnapshot FromDto(GlobalGeneral dto)
        {
            return new GeneralGlobalSnapshot(dto.ShowOnlyCompletedMetadata);
        }
    }

}
