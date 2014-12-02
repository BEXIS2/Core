using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;

namespace BExIS.Xml.Services
{
    public class XmlDatasetHelper
    {
        /// <summary>
        /// returns a value of a metadata node
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetInformation(DatasetVersion datasetVersion, AttributeNames name)
        {
            // get MetadataStructure 
            MetadataStructure metadataStructure = datasetVersion.Dataset.MetadataStructure;
            XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)datasetVersion.Dataset.MetadataStructure.Extra);
            XElement temp = XmlUtility.GetXElementByAttribute(nodeNames.nodeRef.ToString(), "name", name.ToString(), xDoc);

            string xpath = temp.Attribute("value").Value.ToString();
            string title = datasetVersion.Metadata.SelectSingleNode(xpath).InnerText;

            return title;
        }

        /// <summary>
        /// returns a value of a metadata node
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetInformation(Dataset dataset, AttributeNames name)
        {
            DatasetManager dm = new DatasetManager();
            DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(dataset);

            return GetInformation(datasetVersion,name);
        }

    }

    public enum nodeNames
    { 
        nodeRef,
        convertRef
    }

    public enum AttributeNames
    {
        title
    }

}
