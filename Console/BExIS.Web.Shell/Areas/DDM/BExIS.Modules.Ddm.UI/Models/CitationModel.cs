using System.Collections.Generic;
using System.Xml.Serialization;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class CitationModel
    {
        public string CitationString { get; set; }
    }

    [XmlRoot("data")]
    public class CitationDataModel
    {
        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("version")]
        public string Version { get; set; }

        [XmlElement("projects")]
        public List<string> Projects { get; set; }

        [XmlElement("date")]
        public string Date { get; set; }

        [XmlElement("doi")]
        public string DOI { get; set; }

        [XmlArray("authorNames")]
        [XmlArrayItem("authorName")]
        public List<string> Authors { get; set; }
    }

    public class CitationDatasetIds
    {
        public string[] DatasetIds { get; set; }
    }

    public class DatasetCitationEntry
    {
        public DatasetCitationEntry()
        {
            Projects = new List<Project>();
        }

        public string URL { get; set; }
        public string Publisher { get; set; }
        public string InstanceName { get; set; }
        public string Year { get; set; }
        public List<string> Authors { get; set; }
        public string Title { get; set; }
        public List<Project> Projects { get; set; }
        public string DatasetId { get; set; }
        public string Version { get; set; }
        public bool IsPublic { get; set; }
        public string DOI { get; set; }
        public string CitationStringTxt { get; set; }
    }
    public class Project
    {
        public string Name { get; set; }
        public long Id { get; set; }
    }
}