using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class VisualizationModel
    {
        //public string title { get; set; }  // Title of a diagram
        //public Dictionary<string, int> values {get; set;}

        ////test list
        //public List<long> datasetIds { get; set; }

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