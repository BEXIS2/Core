using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace BExIS.Dim.Entities.Export.GBIF
{

    public enum GbifDataType
    {
        metadata,
        occurrence,
        checklist,
        samplingEvent
    }

    public class GBFICrendentials
    {
        [JsonProperty("Server")]
        public string Server { get; set; }


        [JsonProperty("installationKey")]
        public string InstallationKey { get; set; }

        [JsonProperty("organisationKey")]
        public string OrganisationKey { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    #region apis

    public class GbifCreateDatasetRequest
    {
        [JsonProperty("installationKey")]
        public string InstallationKey { get; set; }

        [JsonProperty("publishingOrganizationKey")]
        public string PublishingOrganizationKey { get; set; }

        [JsonProperty("type")]
        public GbifDataType Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class GbifAddEndpointRequest
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        public GbifAddEndpointRequest(string url)
        {
            Url = url; 
            Type = "DWC_ARCHIVE";
        }
    }

    #endregion


    #region xml

    [XmlRoot("archive")]
    public class Archive
    {
        [XmlAttribute("xmlns")]
        public string Xmlns { get; set; }

        [XmlAttribute("metadata")]
        public string Metadata { get; set; }

        [XmlElement("core")]
        public Core Core { get; set; }

        [XmlElement("extension")]
        public List<Extension> Extension { get; set; }

        public Archive()
        {
            Xmlns = String.Empty;
            Metadata = String.Empty;
            Core = new Core();
        }
    }

    public class Base
    {
        [XmlAttribute("encoding")]
        public string Encoding { get; set; }

        [XmlAttribute("fieldsTerminatedBy")]
        public string FieldsTerminatedBy { get; set; }

        [XmlAttribute("linesTerminatedBy")]
        public string LinesTerminatedBy { get; set; }

        [XmlAttribute("fieldsEnclosedBy")]
        public string FieldsEnclosedBy { get; set; }

        [XmlAttribute("ignoreHeaderLines")]
        public string IgnoreHeaderLines { get; set; }

        [XmlAttribute("rowType")]
        public string RowType { get; set; }

        [XmlArrayItem("location")]
        public List<string> files { get; set; }

        [XmlArrayItem("field")]
        public List<Field> fields { get; set; }

        public Base()
        {
            Encoding = String.Empty;
            FieldsTerminatedBy = String.Empty;
            LinesTerminatedBy = String.Empty;
            FieldsEnclosedBy = String.Empty;
            IgnoreHeaderLines = String.Empty;
            RowType = String.Empty;
            files = new List<string>();
            fields = new List<Field>();
        }
    }

    public class Core : Base
    {
        [XmlElement("id")]
        public Id Id { get; set; }
    }

    public class Extension : Base
    {
        [XmlElement("coreid")]
        public Id CoreId { get; set; }
    }

    public class Field
    {
        [XmlAttribute("index")]
        public int Index { get; set; }

        [XmlAttribute("term")]
        public string Term { get; set; }
    }

    public class Id
    {
        [XmlAttribute("index")]
        public int Index { get; set; }
    }

    #endregion xml

    #region json

    public class DWTerms
    {
        public GbifDataType Type { get; set; }

        public List<Field> Field { get; set; }
    }

    public class DWExtTerms
    {
        public string Type { get; set; }

        public List<Field> Field { get; set; }
    }

    #endregion json

    public class ExtentionEntity
    {
        public int IdIndex { get; set; }
        public long Version { get; set; }
        public long StructureId { get; set; }

        public DWCExtention Extention { get; set; }

        public string dataPath { get; set; }
    }

    public class DWCExtention
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("linkName")]
        public string LinkName { get; set; }
        [JsonProperty("rowType")]
        public string RowType { get; set; }

        [JsonProperty("requiredFields")]
        public List<string> RequiredFields { get; set; }

        public DWCExtention()
        {
            RequiredFields = new List<string>();
        }
    }
}

