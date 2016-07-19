using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Xml.Helpers;

namespace BExIS.Xml.Services
{
    public class XmlDatasetHelper
    {
        #region get

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetInformation(long datasetid, NameAttributeValues name)
        {
            DatasetManager dm = new DatasetManager();
            Dataset dataset = dm.GetDataset(datasetid);
            DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(dataset);

            return GetInformation(datasetVersion, name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetInformation(DatasetVersion datasetVersion, NameAttributeValues name)
        {
            // get MetadataStructure 
            if (datasetVersion != null && datasetVersion.Dataset != null &&
                datasetVersion.Dataset.MetadataStructure != null && datasetVersion.Metadata != null)
            {
                MetadataStructure metadataStructure = datasetVersion.Dataset.MetadataStructure;
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument) datasetVersion.Dataset.MetadataStructure.Extra);
                XElement temp = XmlUtility.GetXElementByAttribute(nodeNames.nodeRef.ToString(), "name", name.ToString(),
                    xDoc);

                string xpath = temp.Attribute("value").Value.ToString();

                XmlNode node = datasetVersion.Metadata.SelectSingleNode(xpath);

                string title = "";
                if (node != null)
                    title = datasetVersion.Metadata.SelectSingleNode(xpath).InnerText;

                return title;
            }
            return string.Empty;
        }

        /// <summary>
        /// returns a value of a metadata node
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetInformation(Dataset dataset, NameAttributeValues name)
        {
            DatasetManager dm = new DatasetManager();
            DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(dataset);

            return GetInformation(datasetVersion, name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetExportInformation(long datasetid, TransmissionType type)
        {
            DatasetManager dm = new DatasetManager();
            Dataset dataset = dm.GetDataset(datasetid);
            DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(dataset);

            return GetExportInformation(datasetVersion, type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="type"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public static string GetExportInformation(DatasetVersion datasetVersion, TransmissionType type,
            AttributeNames returnType = AttributeNames.value)
        {
            // get MetadataStructure 
            if (datasetVersion != null && datasetVersion.Dataset != null &&
                datasetVersion.Dataset.MetadataStructure != null && datasetVersion.Metadata != null)
            {
                MetadataStructure metadataStructure = datasetVersion.Dataset.MetadataStructure;
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument) datasetVersion.Dataset.MetadataStructure.Extra);
                IEnumerable<XElement> temp = XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), "type",
                    type.ToString(), xDoc);

                string value = temp.First().Attribute(returnType.ToString()).Value;

                return value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="field"></param>
        /// <param name="fieldValue"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public static string GetExportInformation(DatasetVersion datasetVersion, AttributeNames field, string fieldValue,
            AttributeNames returnType = AttributeNames.value)
        {
            // get MetadataStructure 
            if (datasetVersion != null && datasetVersion.Dataset != null &&
                datasetVersion.Dataset.MetadataStructure != null && datasetVersion.Metadata != null)
            {
                MetadataStructure metadataStructure = datasetVersion.Dataset.MetadataStructure;
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)datasetVersion.Dataset.MetadataStructure.Extra);
                IEnumerable<XElement> temp = XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), field.ToString(),
                    fieldValue, xDoc);

                string value = temp.First().Attribute(returnType.ToString()).Value;

                return value;
            }
            return string.Empty;
        }

        public static string GetExportInformation(DatasetVersion datasetVersion, TransmissionType type, string name,
            AttributeNames returnType = AttributeNames.value)
        {
            // get MetadataStructure 
            if (datasetVersion != null && datasetVersion.Dataset != null &&
                datasetVersion.Dataset.MetadataStructure != null && datasetVersion.Metadata != null)
            {
                MetadataStructure metadataStructure = datasetVersion.Dataset.MetadataStructure;
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)datasetVersion.Dataset.MetadataStructure.Extra);

                Dictionary<string,string> queryDic = new Dictionary<string, string>();
                queryDic.Add(AttributeNames.name.ToString(), name);
                queryDic.Add(AttributeNames.type.ToString(), type.ToString());

                IEnumerable<XElement> temp = XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), queryDic, xDoc);

                string value = temp.First().Attribute(returnType.ToString()).Value;

                return value;
            }
            return string.Empty;
        }


        /// <summary>
        /// returns a value of a metadata node
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllExportInformation(long datasetid, TransmissionType type,
            AttributeNames returnType = AttributeNames.value)
        {
            DatasetManager dm = new DatasetManager();
            Dataset dataset = dm.GetDataset(datasetid);
            DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(dataset);

            // get MetadataStructure 
            if (datasetVersion != null && datasetVersion.Dataset != null &&
                datasetVersion.Dataset.MetadataStructure != null && datasetVersion.Metadata != null)
            {
                MetadataStructure metadataStructure = datasetVersion.Dataset.MetadataStructure;
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument) datasetVersion.Dataset.MetadataStructure.Extra);
                IEnumerable<XElement> temp = XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), AttributeNames.type.ToString(),
                    type.ToString(), xDoc);

                List<string> tmpList = new List<string>();
                foreach (var element in temp)
                {
                    tmpList.Add(element.Attribute(returnType.ToString()).Value);
                }

                return tmpList;
            }
            return null;
        }

        #endregion

        #region add

        public static XmlDocument AddReferenceToXml(XmlDocument Source, string nodeName, string nodePath, string nodeType, string destinationPath)
        {

            //XmlDocument doc = new XmlDocument();
            XmlNode extra;
            if (Source != null)
            {
                if (Source.DocumentElement == null)
                {
                    extra = Source.CreateElement("extra", "");
                    Source.AppendChild(extra);
                }
            }

            XmlNode x = createMissingNodes(destinationPath, Source.DocumentElement, Source, nodeName);

            //check attrviute of the xmlnode
            if (x.Attributes.Count > 0)
            {
                foreach (XmlAttribute attr in x.Attributes)
                {
                    if (attr.Name == "name") attr.Value = nodeName;
                    if (attr.Name == "value") attr.Value = nodePath;
                    if (attr.Name == "type") attr.Value = nodeType;
                }
            }
            else
            {
                XmlAttribute name = Source.CreateAttribute("name");
                name.Value = nodeName;
                XmlAttribute value = Source.CreateAttribute("value");
                value.Value = nodePath;
                XmlAttribute type = Source.CreateAttribute("type");
                type.Value = nodeType;

                x.Attributes.Append(name);
                x.Attributes.Append(value);
                x.Attributes.Append(type);

            }

            return Source;

        }

        private static XmlNode createMissingNodes(string destinationParentXPath, XmlNode parentNode, XmlDocument doc,
            string name)
        {
            string dif = destinationParentXPath;

            List<string> temp = dif.Split('/').ToList();
            temp.RemoveAt(0);

            XmlNode parentTemp = parentNode;

            foreach (string s in temp)
            {
                if (XmlUtility.GetXmlNodeByName(parentTemp, s) == null)
                {
                    XmlNode t = XmlUtility.CreateNode(s, doc);

                    parentTemp.AppendChild(t);
                    parentTemp = t;
                }
                else
                {
                    XmlNode t = XmlUtility.GetXmlNodeByName(parentTemp, s);

                    if (temp.Last().Equals(s))
                    {
                        if (!t.Attributes["name"].Equals(name))
                        {
                            t = XmlUtility.CreateNode(s, doc);
                            parentTemp.AppendChild(t);
                        }

                    }

                    parentTemp = t;
                }
            }

            return parentTemp;
        }

        #endregion


    }

    public enum nodeNames
    { 
        nodeRef,
        convertRef
    }

    public enum NameAttributeValues
    {
        title,
        description
    }

    public enum AttributeNames
    {
        name,
        value,
        type,
    }

    public enum AttributeType
    {
        xpath
    }

    public enum TransmissionType
    {
        mappingFileExport,
        mappingFileImport
    }

}
