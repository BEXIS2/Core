using BExIS.Dlm.Entities.Data;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Vim.UI.Models
{
    public class DatasetModels
    {
        public string ContentContact { get; set; }
        public long Id { get; set; }

        public string Owner { get; set; }
        public string TechnicalContact { get; set; }
        public string Title { get; set; }
        public int Version { get; set; }

        public static DatasetModels Convert(Dataset dataset)
        {
            return new DatasetModels()
            {
                Id = dataset.Id,
                Version = dataset.VersionNo,
                Title = dataset.Versions.Last().Metadata.SelectNodes("Metadata/Description/Description/Title/Title")[0].InnerText,
                TechnicalContact = dataset.Versions.Last().Metadata.SelectNodes("Metadata/TechnicalContact/Person/Name/Name")[0].InnerText,
                ContentContact = dataset.Versions.Last().Metadata.SelectNodes("Metadata/ContentContact/Person/Name/Name")[0].InnerText,
                Owner = dataset.Versions.Last().Metadata.SelectNodes("Metadata/Owner/Owner/FullName/Name")[0].InnerText
            };
        }
    }

    public class VisualizationModels
    {
        public Dictionary<string, int> allDatasets = new Dictionary<string, int>();
        public Dictionary<string, int> allActivities = new Dictionary<string, int>();
        public Dictionary<string, int> createdDatasets = new Dictionary<string, int>();

        //public Dictionary<string, int> checkedInDatasets = new Dictionary<string, int>();
        //public Dictionary<string, int> checkedOutDatasets = new Dictionary<string, int>();
        public Dictionary<string, int> deletedDatasets = new Dictionary<string, int>();
    }
}