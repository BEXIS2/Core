using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Web.Shell.Areas.Auth.Models
{
    public class DatasetModel
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }
        public string TechnicalContact { get; set; }
        public string ContentContact { get; set; }
        public string Owner { get; set; }

        public static DatasetModel Convert(Dataset dataset)
        {
            return new DatasetModel()
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