using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using static BExIS.Dim.Helpers.Export.GBIFDataRepoConverter;

namespace BExIS.Dim.Helpers.Models
{
    public enum GbifDataType
    {
        metadata,
        occurrence,
        checklist,
        samplingEvent
    }

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