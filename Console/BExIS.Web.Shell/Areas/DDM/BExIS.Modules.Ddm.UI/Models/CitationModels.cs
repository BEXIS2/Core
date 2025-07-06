using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class CitationModel
    {
        public string CitationString { get; set; }
    }

    public class ReadCitationModel
    {
        public string Title { get; set; }
        public string Version { get; set; }
    }

    public enum ReadCitationFormat
    {
        APA,
        Text
    }

    public enum DownloadCitationFormat
    {
        APA,
        RIS,
        Text,
        Bibtex
    }


    [XmlRoot("data")]
    public class CitationDataModel
    {
        [XmlElement("title")]
        [Required(ErrorMessage = "Title is required")]
        [MinLength(1, ErrorMessage = "Title cannot be empty")]
        public string Title { get; set; }

        [XmlElement("version")]
        public string Version { get; set; }

        [XmlElement("projects")]
        public List<string> Projects { get; set; }

        [XmlElement("date")]
        [Required(ErrorMessage = "Title is required")]
        [MinLength(1, ErrorMessage = "Title cannot be empty")]
        public string Date { get; set; }

        [XmlElement("doi")]
        public string DOI { get; set; }

        [XmlElement("url")]
        [Required(ErrorMessage = "Title is required")]
        [MinLength(1, ErrorMessage = "Title cannot be empty")]
        public string URL { get; set; }

        [XmlArray("authorNames")]
        [XmlArrayItem("authorName")]
        [MinCapacity(1)]
        public List<string> Authors { get; set; }

        [XmlElement("publisher")]
        [Required(ErrorMessage = "Title is required")]
        [MinLength(1, ErrorMessage = "Title cannot be empty")]
        public string Publisher { get; set; }

        [XmlElement("accessDate")]
        [Required(ErrorMessage = "Title is required")]
        [MinLength(1, ErrorMessage = "Title cannot be empty")]
        public string AccessDate { get; set; }

        public CitationDataModel()
        {
            Authors = new List<string>();
            Projects = new List<string>();
        }
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

    public class CitationSettings
    {
        public string Instance { get; set; }
        public string Publisher { get; set; }
        public bool ShowCitation { get; set; }
        public ReadCitationFormat ReadCitationFormat { get; set; }

        public List<DownloadCitationFormat> DownloadCitationFormats { get; set; }

        public CitationSettings()
        {
            Instance = "BEXIS2";
            Publisher = "BEXIS2";
            ShowCitation = false;
            ReadCitationFormat = ReadCitationFormat.Text;
            DownloadCitationFormats = new List<DownloadCitationFormat>() { };
        }
    }

}