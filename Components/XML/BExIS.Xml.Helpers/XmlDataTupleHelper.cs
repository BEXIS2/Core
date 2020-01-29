using BExIS.Dlm.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BExIS.Xml.Helpers
{
    public class XmlDataTupleHelper
    {
        public XmlDocument Convert(List<VariableValue> variableValues)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement root = doc.CreateElement("r");

            foreach (var vv in variableValues)
            {
                var vvAsXml = convert(vv, doc);
                root.AppendChild(vvAsXml);
            }

            doc.AppendChild(root);

            return doc;
        }

        private XmlElement convert(VariableValue variableValue, XmlDocument doc)
        {
            XmlElement vv = doc.CreateElement("vv"); // variable value
            XmlElement v = doc.CreateElement("v"); // value
            v.InnerText = variableValue.Value.ToString();
            XmlElement vid = doc.CreateElement("vid"); // variableid
            vid.InnerText = variableValue.VariableId.ToString();

            vv.AppendChild(v);
            vv.AppendChild(vid);

            return vv;
        }

        public List<VariableValue> Convert(XmlDocument doc)
        {
            List<VariableValue> tmp = new List<VariableValue>();

            foreach (var vvAsXmlElement in doc.DocumentElement.ChildNodes)
            {
                string vid = ((XmlElement)vvAsXmlElement).GetElementsByTagName("vid")?.Item(0)?.InnerText;
                string v = ((XmlElement)vvAsXmlElement).GetElementsByTagName("v")?.Item(0)?.InnerText;

                VariableValue vv = new VariableValue();

                if (!string.IsNullOrEmpty(v)) vv.Value = v;
                if (!string.IsNullOrEmpty(vid)) vv.VariableId = Int64.Parse(vid);

                tmp.Add(vv);
            }

            return tmp;
        }
    }
}