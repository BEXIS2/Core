using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using BExIS.Dlm.Entities.Data;
using BExIS.Xml.Services;

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

    public class DatasetGridRowModel
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }

        public static DatasetGridRowModel Convert(Dataset dataset)
        {
            XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)dataset.MetadataStructure.Extra);
            XElement temp = XmlUtility.GetXElementByAttribute("nodeRef", "name", "title", xDoc);

            string xpath = temp.Attribute("value").Value.ToString();
            string title = dataset.Versions.Last().Metadata.SelectSingleNode(xpath).InnerText;

            return new DatasetGridRowModel()
            {
                Id = dataset.Id,
                Version = dataset.VersionNo,
                Title = title
            };
        }
    }
}