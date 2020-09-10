using BExIS.Dlm.Entities.Data;
using BExIS.Xml.Helpers;
using System.Linq;

namespace BExIS.Modules.Sam.UI.Models
{
    public class DatasetGridRowModel
    {
        public long Id { get; set; }

        public bool IsPublic { get; set; }
        public string Title { get; set; }

        public static DatasetGridRowModel Convert(DatasetVersion datasetVersion, bool isPublic)
        {
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

            return new DatasetGridRowModel()
            {
                Id = datasetVersion.Dataset.Id,
                Title = datasetVersion.Title,
                IsPublic = isPublic
            };
        }
    }

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
}