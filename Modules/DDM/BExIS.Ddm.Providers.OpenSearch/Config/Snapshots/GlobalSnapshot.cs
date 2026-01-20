using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ddm.Providers.OpenSearch.Config.Snapshots
{
    public sealed class GlobalSnapshot
    {
        public GlobalSearchComponentSnapshot SearchComponents { get; }
        public GlobalPrimaryDataSnapshot PrimaryData { get; }
        public GlobalSpatialDataSnapshot SpatialData { get; }
        public GeneralGlobalSnapshot General { get; }

        private GlobalSnapshot(
            GlobalSearchComponentSnapshot searchComponents,
            GlobalPrimaryDataSnapshot primaryData,
            GlobalSpatialDataSnapshot spatialData,
            GeneralGlobalSnapshot general)
        {
            SearchComponents = searchComponents;
            PrimaryData = primaryData;
            SpatialData = spatialData;
            General = general;
        }

        public static GlobalSnapshot FromDto(GlobalConfig dto)
        {
            return new GlobalSnapshot(
                GlobalSearchComponentSnapshot.FromDto(dto.SearchComponents),
                GlobalPrimaryDataSnapshot.FromDto(dto.PrimaryData),
                GlobalSpatialDataSnapshot.FromDto(dto.SpatialData),
                GeneralGlobalSnapshot.FromDto(dto.General)
            );
        }
    }

}
