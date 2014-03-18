using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BExIS.Xml.Services
{

    public enum XmlNodeMode
    {
        xPath,
        type
    }

    public class XmlWriter
    {
        protected XmlNodeMode _mode;
        protected XDocument _tempXDoc;


        protected XElement CreateXElement(string name, XmlNodeType type)
        {
            if (_mode.Equals(XmlNodeMode.type))
            {
                XElement element = new XElement(type.ToString());
                element.SetAttributeValue("name", name);

                return element;
            }

            if (_mode.Equals(XmlNodeMode.xPath))
            {
                return new XElement(name.Replace(" ", ""));
            }

            return null;
        }

        protected bool Exist(string name)
        {
            if (_mode.Equals(XmlNodeMode.type))
            {
                if (_tempXDoc.Root.Elements().Where(p => p.Attribute("name").Equals(name)).Count() > 0)
                    return true;
                else
                    return false;
            }

            if (_mode.Equals(XmlNodeMode.xPath))
            {
                if (_tempXDoc.Root.Elements(name.Replace(" ","")).Count() > 0)
                    return true;
                else
                    return false;
            }

            return false;
        }

        protected bool Exist(string name, int number)
        {
            if (_mode.Equals(XmlNodeMode.type))
            {
                if (_tempXDoc.Root.Elements().Where(p => p.Attribute("name").Equals(name)
                    && p.Attribute("number").Equals(number.ToString())).Count() > 0)
                    return true;
                else
                    return false;
            }

            if (_mode.Equals(XmlNodeMode.xPath))
            {
                if (_tempXDoc.Root.Elements(name.Replace(" ", "")).Where(p => p.Attribute("number") != null && p.Attribute("number").Value.Equals(number.ToString())).Count() > 0)
                    return true;
                else
                    return false;
            }

            return false;
        }

        protected bool Exist(string name, XElement source)
        {
            if (_mode.Equals(XmlNodeMode.type))
            {
                if (source.Elements().Where(p => p.Attribute("name").Equals(name)).Count() > 0)
                    return true;
                else
                    return false;
            }

            if (_mode.Equals(XmlNodeMode.xPath))
            {
                if (source.Elements(name.Replace(" ", "")).Count() > 0)
                    return true;
                else
                    return false;
            }

            return false;
        }

        protected bool Exist(string name, int number, XElement source)
        {
            if (_mode.Equals(XmlNodeMode.type))
            {
                if (source.Elements().Where(p => p.Attribute("name").Equals(name)
                    && p.Attribute("number").Equals(number.ToString())).Count() > 0)
                    return true;
                else
                    return false;
            }

            if (_mode.Equals(XmlNodeMode.xPath))
            {
                if (source.Elements(name.Replace(" ", "")).Where(p => p.Attribute("number") != null && p.Attribute("number").Value.Equals(number.ToString())).Count() > 0)
                    return true;
                else
                    return false;
            }

            return false;
        }

        protected XElement Get(string name)
        {
            if (_mode.Equals(XmlNodeMode.type))
            {
                return _tempXDoc.Root.Elements().Where(p => p.Attribute("name").Equals(name)).FirstOrDefault();

            }

            if (_mode.Equals(XmlNodeMode.xPath))
            {
                return _tempXDoc.Root.Elements(name.Replace(" ", "")).FirstOrDefault();
            }

            return null;
        }

        protected XElement Get(string name, int number)
        {
            if (_mode.Equals(XmlNodeMode.type))
            {
                return _tempXDoc.Root.Elements().Where(p => p.Attribute("name").Equals(name) && p.Attribute("number").Equals(number.ToString())).FirstOrDefault();

            }

            if (_mode.Equals(XmlNodeMode.xPath))
            {
                return _tempXDoc.Root.Elements(name.Replace(" ", "")).Where(p => p.Attribute("number") != null && p.Attribute("number").Value.Equals(number.ToString())).FirstOrDefault();
            }

            return null;
        }

        protected XElement Get(string name, XElement source)
        {
            if (_mode.Equals(XmlNodeMode.type))
            {
                return source.Elements().Where(p => p.Attribute("name").Equals(name)).FirstOrDefault();

            }

            if (_mode.Equals(XmlNodeMode.xPath))
            {
                return source.Elements(name.Replace(" ", "")).FirstOrDefault();
            }

            return null;
        }

        protected XElement Get(string name, int number, XElement source)
        {
            if (_mode.Equals(XmlNodeMode.type))
            {
                return source.Elements().Where(p => p.Attribute("name").Equals(name) && p.Attribute("number").Equals(number.ToString())).FirstOrDefault();

            }

            if (_mode.Equals(XmlNodeMode.xPath))
            {
                return source.Elements(name.Replace(" ", "")).Where(p => p.Attribute("number") != null && p.Attribute("number").Value.Equals(number.ToString())).FirstOrDefault();
            }

            return null;
        }


        #region get list of xelement
            protected List<XElement> GetChildren(string name, XElement source)
            {
                if (_mode.Equals(XmlNodeMode.type))
                {
                    return source.Elements().Where(p => p.Attribute("name").Equals(name)).ToList();

                }

                if (_mode.Equals(XmlNodeMode.xPath))
                {
                    return source.Elements(name.Replace(" ", "")).ToList();
                }

                return null;
            }
        #endregion

            #region static
                public static XmlDocument ToXmlDocument(XDocument xDocument)
                {
                    var xmlDocument = new XmlDocument();
                    using (var xmlReader = xDocument.CreateReader())
                    {
                        xmlDocument.Load(xmlReader);
                    }
                    return xmlDocument;
                }

                public static XDocument ToXDocument(XmlDocument xmlDocument)
                {
                    using (var nodeReader = new XmlNodeReader(xmlDocument))
                    {
                        nodeReader.MoveToContent();
                        return XDocument.Load(nodeReader);
                    }
                }
            #endregion

    }

}
