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

        /// <summary>
        /// returns a value of a metadata node
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetInformation(long datasetid, AttributeNames name)
        {
            DatasetManager dm = new DatasetManager();
            Dataset dataset = dm.GetDataset(datasetid);
            DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(dataset);

            return GetInformation(datasetVersion, name);
        }

        /// <summary>
        /// returns a value of a metadata node
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetInformation(DatasetVersion datasetVersion, AttributeNames name)
        {
            // get MetadataStructure 
            if (datasetVersion != null && datasetVersion.Dataset != null && datasetVersion.Dataset.MetadataStructure != null &&  datasetVersion.Metadata != null)
            {
                MetadataStructure metadataStructure = datasetVersion.Dataset.MetadataStructure;
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)datasetVersion.Dataset.MetadataStructure.Extra);
                XElement temp = XmlUtility.GetXElementByAttribute(nodeNames.nodeRef.ToString(), "name", name.ToString(), xDoc);

                string xpath = temp.Attribute("value").Value.ToString();

                XmlNode node = datasetVersion.Metadata.SelectSingleNode(xpath);

                string title = "";
                if(node != null)
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
        title,
        description
    }

}
