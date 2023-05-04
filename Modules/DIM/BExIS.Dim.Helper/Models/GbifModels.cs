using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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

        public Archive()
        {
            Xmlns = String.Empty;
            Metadata = String.Empty;
            Core = new Core();
        }

    }

    public class Core
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

        [XmlElement("id")]
        public Id Id { get; set; }

        [XmlArrayItem("field")]
        public List<Field> fields { get; set; }

        public Core()
        {
            Encoding = String.Empty;
            FieldsTerminatedBy = String.Empty;
            LinesTerminatedBy = String.Empty;
            FieldsEnclosedBy = String.Empty;
            IgnoreHeaderLines = String.Empty;
            RowType = String.Empty;
            files = new List<string>();
            Id = new Id();
            fields = new List<Field>();

        }
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

    #endregion

    #region json

    public class DWTerms
    {
        public GbifDataType Type { get; set; }

        public List<Field> Field { get; set; }
    }

    #endregion
}
