using System;
using System.IO;
using System.Xml;

namespace Vaiona.Core.Serialization
{
    public class XmlObjectTransformer : IObjectTransfromer
    {
        #region Attributes

        private ObjGraphXmlSerializer serializer = null;
        private ObjGraphXmlDeserializer deSerializer = null;

        #endregion Attributes

        public object ExportTo(Type sourceType, object source, string instanceName, string rootElementName = "Root", int version = 1, bool resetExportInfo = false)
        {
            if (resetExportInfo)
                serializer = null;
            if (serializer == null || serializer.ObjectType != sourceType)
            {
                serializer = new ObjGraphXmlSerializer(source, sourceType);
            }
            return (serializer.Serialize(source, version, instanceName, rootElementName));
        }

        public object ExportTo<S>(object source, string instanceName, string rootElementName = "Root", int version = 1, bool resetExportInfo = false)
        {
            return (ExportTo(typeof(S), source, instanceName, rootElementName, version, resetExportInfo));
        }

        public object ImportFrom(Type returnType, object source, ITypeResolver typeResolver = null, int version = 1, bool resetImportInfo = false)
        {
            if (resetImportInfo)
                deSerializer = null;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc = (XmlDocument)source;
            }
            catch
            {
                var resourceDocument = new XmlDocument();
                var sw = new StringWriter();
                var xw = new XmlTextWriter(sw);
                ((XmlNode)source).WriteTo(xw);
                var temp = sw.ToString();
                resourceDocument.LoadXml(temp);

                doc = resourceDocument;
                //doc.ImportNode((XmlNode)source, true);
                //doc = ((XmlNode)source).OwnerDocument;
            }
            if (deSerializer == null)
                deSerializer = new ObjGraphXmlDeserializer();
            return (deSerializer.Deserialize(doc, version, typeResolver));
        }

        public T ImportFrom<T>(object source, ITypeResolver typeResolver = null, int version = 1, bool resetImportInfo = false)
        {
            return (((T)ImportFrom(typeof(T), source, typeResolver, version, resetImportInfo)));
        }
    }
}