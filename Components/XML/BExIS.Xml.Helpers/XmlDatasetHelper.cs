using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Xml.Helpers
{
    public class XmlDatasetHelper
    {
        #region get

        /// <summary>
        /// Return a value of the attribute from the incoming metadata
        /// </summary>
        /// <param name="datasetid"></param>
        /// <param name="metadata"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetInformation(long datasetid, XmlDocument metadata, NameAttributeValues name)
        {
            using (var unitOfWork = this.GetUnitOfWork())
            using (DatasetManager dm = new DatasetManager())
            using (MetadataStructureManager msm = new MetadataStructureManager())
            {
                if (datasetid <= 0) return String.Empty;

                var dataset = dm.GetDataset(datasetid);

                MetadataStructure metadataStructure = msm.Repo.Get(dataset.MetadataStructure.Id);

                if ((XmlDocument)metadataStructure.Extra == null) return string.Empty;

                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                XElement temp = XmlUtility.GetXElementByAttribute(nodeNames.nodeRef.ToString(), "name", name.ToString(),
                    xDoc);

                string xpath = temp.Attribute("value").Value.ToString();
                string value = metadata.SelectSingleNode(xpath).InnerText;

                return string.IsNullOrWhiteSpace(value) ? "not available" : value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="datasetid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetInformation(long datasetid, NameAttributeValues name)
        {
            DatasetManager dm = new DatasetManager();
            try
            {
                //DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(datasetid);
                var dataset = dm.GetDataset(datasetid);

                if (!dm.IsDatasetCheckedIn(datasetid)) return string.Empty;

                var versionId = dm.GetDatasetLatestVersionId(datasetid);

                return GetInformationFromVersion(versionId, dataset.MetadataStructure.Id, name);
            }
            finally
            {
                dm.Dispose();
            }
        }

        /// <summary>
        /// Information in metadata is stored as xml
        /// get back the vale of an attribute
        /// e.g. title  = "dataset title"
        /// /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetInformationFromVersion(long datasetVersionId, long metadataStructureId, NameAttributeValues name)
        {
            using (var unitOfWork = this.GetUnitOfWork())
            using (DatasetManager dm = new DatasetManager())
            using (MetadataStructureManager msm = new MetadataStructureManager())
            {
                if (datasetVersionId <= 0) return String.Empty;
                if (metadataStructureId <= 0) return String.Empty;

                MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                if ((XmlDocument)metadataStructure.Extra == null) return string.Empty;

                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                XElement temp = XmlUtility.GetXElementByAttribute(nodeNames.nodeRef.ToString(), "name", name.ToString(),
                    xDoc);

                string xpath = temp.Attribute("value").Value.ToString();

                return dm.GetMetadataValueFromDatasetVersion(datasetVersionId, xpath);
            }
        }

        public Dictionary<long, string> GetInformationFromVersions(List<long> datasetVersionIds, long metadataStructureId, NameAttributeValues name)
        {
            using (var unitOfWork = this.GetUnitOfWork())
            using (DatasetManager dm = new DatasetManager())
            using (MetadataStructureManager msm = new MetadataStructureManager())
            {
                if (datasetVersionIds.Any(d => d <= 0)) return null;
                if (metadataStructureId <= 0) return null;

                MetadataStructure metadataStructure = msm.Repo.Get(metadataStructureId);

                if ((XmlDocument)metadataStructure.Extra == null) return null;

                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                XElement temp = XmlUtility.GetXElementByAttribute(nodeNames.nodeRef.ToString(), "name", name.ToString(),
                    xDoc);

                string xpath = temp.Attribute("value").Value.ToString();

                return dm.GetMetadataValueFromDatasetVersion(datasetVersionIds, xpath);
            }
        }

        /// <summary>
        /// Information in metadata is stored as xml
        /// get back the vale of an attribute
        /// e.g. title  = "dataset title"
        /// /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetInformationFromVersion(long datasetVersionId, NameAttributeValues name)
        {
            using (var unitOfWork = this.GetUnitOfWork())
            {
                DatasetVersion datasetVersion = unitOfWork.GetReadOnlyRepository<DatasetVersion>().Get(datasetVersionId);

                return GetInformationFromVersion(
                    datasetVersion.Id,
                    datasetVersion.Dataset.MetadataStructure.Id,
                    name);
            }
        }

        /// <summary>
        /// Information in metadata is stored as xml
        /// get back the vale of an attribute
        /// e.g. title  = "dataset title"
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetInformation(Dataset dataset, NameAttributeValues name)
        {
            DatasetManager dm = new DatasetManager();
            try
            {
                DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(dataset);

                return GetInformationFromVersion(datasetVersion.Id, dataset.MetadataStructure.Id, name);
            }
            finally
            {
                dm.Dispose();
            }
        }

        /// <summary>
        /// Information in metadata is stored as xml
        /// get back the xpath of an attribute
        /// e.g. title  = metadata/dataset/title
        /// </summary>
        /// <param name="metadataStructure"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetInformationPath(long metadataStructureId, NameAttributeValues name)
        {
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);

            if ((XmlDocument)metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                XElement temp = XmlUtility.GetXElementByAttribute(nodeNames.nodeRef.ToString(), "name", name.ToString(),
                    xDoc);

                string xpath = temp.Attribute("value").Value.ToString();

                return xpath;
            }
            return "";
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="datasetid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetTransmissionInformation(long datasetid, TransmissionType type)
        {
            DatasetManager dm = new DatasetManager();

            try
            {
                Dataset dataset = dm.GetDataset(datasetid);
                DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(dataset);

                return GetTransmissionInformation(datasetVersion.Id, type);
            }
            finally
            {
                dm.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="type"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public string GetTransmissionInformation(long datasetVersionId, TransmissionType type,
            AttributeNames returnType = AttributeNames.value)
        {
            DatasetVersion datasetVersion = this.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(datasetVersionId);
            Dataset dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(datasetVersion.Dataset.Id);
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(dataset.MetadataStructure.Id);

            // get MetadataStructure
            if (datasetVersion != null && dataset != null &&
                metadataStructure != null && datasetVersion.Metadata != null && metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
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
        public string GetTransmissionInformation(long datasetVersionId, AttributeNames field, string fieldValue,
            AttributeNames returnType = AttributeNames.value)
        {
            DatasetVersion datasetVersion = this.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(datasetVersionId);
            Dataset dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(datasetVersion.Dataset.Id);
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(dataset.MetadataStructure.Id);

            // get MetadataStructure
            if (datasetVersion != null && dataset != null &&
                metadataStructure != null && datasetVersion.Metadata != null && metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> temp = XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), field.ToString(),
                    fieldValue, xDoc);

                string value = temp.First().Attribute(returnType.ToString()).Value;

                return value;
            }
            return string.Empty;
        }

        public string GetTransmissionInformation(long datasetVersionId, TransmissionType type, string name,
            AttributeNames returnType = AttributeNames.value)
        {
            DatasetVersion datasetVersion = this.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(datasetVersionId);
            Dataset dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(datasetVersion.Dataset.Id);
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(dataset.MetadataStructure.Id);

            // get MetadataStructure
            if (datasetVersion != null && dataset != null &&
                metadataStructure != null && datasetVersion.Metadata != null && metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)datasetVersion.Dataset.MetadataStructure.Extra);

                Dictionary<string, string> queryDic = new Dictionary<string, string>();
                queryDic.Add(AttributeNames.name.ToString(), name);
                queryDic.Add(AttributeNames.type.ToString(), type.ToString());

                IEnumerable<XElement> temp = XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), queryDic, xDoc);

                string value = temp?.First().Attribute(returnType.ToString()).Value;

                return value;
            }
            return string.Empty;
        }

        public bool HasImportInformation(long metadataStructureId)
        {
            // get MetadataStructure
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);

            if (metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> tmp = XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), AttributeNames.type.ToString(),
                    TransmissionType.mappingFileImport.ToString(), xDoc);

                if (tmp.Any()) return true;
            }
            return false;
        }

        public bool HasExportInformation(long metadataStructureId)
        {
            // get MetadataStructure
            // TODO Refactor Manager in Helper
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);

            if (metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> tmp = XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), AttributeNames.type.ToString(),
                    TransmissionType.mappingFileExport.ToString(), xDoc);

                if (tmp.Any()) return true;
            }
            return false;
        }

        /// <summary>
        /// returns a value of a metadata node
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<string> GetAllTransmissionInformation(long datasetid, TransmissionType type,
            AttributeNames returnType = AttributeNames.value)
        {
            Dataset dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(datasetid);
            DatasetManager dm = new DatasetManager();
            try
            {
                DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(dataset);

                // get MetadataStructure
                if (datasetVersion != null && datasetVersion.Dataset != null &&
                    datasetVersion.Dataset.MetadataStructure != null &&
                    datasetVersion.Dataset.MetadataStructure.Extra != null &&
                    datasetVersion.Metadata != null)
                {
                    return GetAllTransmissionInformationFromMetadataStructure(datasetVersion.Dataset.MetadataStructure.Id,
                        type, returnType);
                }
                return null;
            }
            finally
            {
                dm.Dispose();
            }
        }

        /// <summary>
        /// returns a List of all transmission nodes in the metadataStructure
        /// </summary>
        /// <param name="metadatastrutcureId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<string> GetAllTransmissionInformationFromMetadataStructure(long metadataStructureId, TransmissionType type,
            AttributeNames returnType = AttributeNames.value)
        {
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);

            List<string> tmpList = new List<string>();

            try
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> temp = XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), AttributeNames.type.ToString(),
                    type.ToString(), xDoc);

                foreach (var element in temp)
                {
                    tmpList.Add(element.Attribute(returnType.ToString()).Value);
                }
            }
            catch (Exception)
            {
                return new List<string>();
            }

            return tmpList;
        }

        public bool IsActive(long metadataStructureId)
        {
            // get MetadataStructure

            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);

            if (metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                XElement tmp = XmlUtility.GetXElementsByAttribute(nodeNames.parameter.ToString(), AttributeNames.name.ToString(),
                    NameAttributeValues.active.ToString(), xDoc).FirstOrDefault();

                if (tmp != null)
                {
                    try
                    {
                        return Convert.ToBoolean(tmp.Attribute(AttributeNames.value.ToString()).Value);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public bool HasTransmission(long datasetid, TransmissionType type)
        {
            Dataset dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(datasetid);
            DatasetManager dm = new DatasetManager();
            try
            {
                DatasetVersion datasetVersion = dm.GetDatasetLatestVersion(dataset);

                // get MetadataStructure
                if (datasetVersion != null && datasetVersion.Dataset != null &&
                datasetVersion.Dataset.MetadataStructure != null &&
                datasetVersion.Dataset.MetadataStructure.Extra != null &&
                datasetVersion.Metadata != null)
                {
                    MetadataStructure metadataStructure = datasetVersion.Dataset.MetadataStructure;
                    XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)datasetVersion.Dataset.MetadataStructure.Extra);
                    IEnumerable<XElement> temp = XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), AttributeNames.type.ToString(),
                        type.ToString(), xDoc);

                    if (temp != null && temp.Any()) return true;
                }
                return false;
            }
            finally
            {
                dm.Dispose();
            }
        }

        public bool HasMetadataStructureTransmission(long metadataStructureId, TransmissionType type)
        {
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);

            // get MetadataStructure
            if (metadataStructure != null && metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> temp = XmlUtility.GetXElementsByAttribute(nodeNames.convertRef.ToString(), AttributeNames.type.ToString(),
                    type.ToString(), xDoc);

                if (temp != null && temp.Any()) return true;
            }
            return false;
        }

        //todo entity extention
        public string GetEntityType(long datasetid)
        {
            DatasetManager datasetManager = new DatasetManager();
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            try
            {
                Dataset dataset = datasetManager.GetDataset(datasetid);

                // get MetadataStructure
                if (dataset != null)
                {
                    return GetEntityTypeFromMetadatStructure(dataset.MetadataStructure.Id, metadataStructureManager);
                }
                return string.Empty;
            }
            finally
            {
                datasetManager.Dispose();
                metadataStructureManager.Dispose();
            }
        }

        public string GetEntityName(long datasetid)
        {
            DatasetManager datasetManager = new DatasetManager();
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            try
            {
                Dataset dataset = datasetManager.GetDataset(datasetid);

                // get MetadataStructure
                if (dataset != null)
                {
                    return GetEntityNameFromMetadatStructure(dataset.MetadataStructure.Id);
                }
                return string.Empty;
            }
            finally
            {
                datasetManager.Dispose();
                metadataStructureManager.Dispose();
            }
        }

        //todo entity extention
        public string GetEntityTypeFromMetadatStructure(long metadataStructureId, MetadataStructureManager metadataStructureManager)
        {
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);

            // get MetadataStructure
            if (metadataStructure != null && metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> tmp = XmlUtility.GetXElementByNodeName(nodeNames.entity.ToString(), xDoc);
                if (tmp.Any())
                    return tmp.First().Attribute("value").Value;
            }

            return string.Empty;
        }

        //todo entity extention
        public string GetEntityNameFromMetadatStructure(long metadataStructureId)
        {
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);
            // get MetadataStructure
            if (metadataStructure != null && metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> tmp = XmlUtility.GetXElementByNodeName(nodeNames.entity.ToString(), xDoc);
                if (tmp.Any())
                    return tmp.First().Attribute("name").Value;
            }

            return string.Empty;
        }

        public string GetEntityNameFromMetadatStructure(long metadataStructureId, MetadataStructureManager metadataStructureManager)
        {
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);

            // get MetadataStructure
            if (metadataStructure != null && metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> tmp = XmlUtility.GetXElementByNodeName(nodeNames.entity.ToString(), xDoc);
                if (tmp.Any())
                    return tmp.First().Attribute("name").Value;
            }

            return string.Empty;
        }

        //todo entity extention
        public bool HasEntityType(long metadataStructureId, string entityClassPath)
        {
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);

            // get MetadataStructure
            if (metadataStructure != null && metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> tmp = XmlUtility.GetXElementByNodeName(nodeNames.entity.ToString(), xDoc);
                if (tmp.Any())
                {
                    foreach (var entity in tmp)
                    {
                        string tmpEntityClassPath = "";
                        if (entity.HasAttributes && entity.Attribute("value") != null)
                            tmpEntityClassPath = entity.Attribute("value").Value.ToLower();

                        if (tmpEntityClassPath.Equals(entityClassPath.ToLower())) return true;
                    }
                }
            }
            return false;
        }

        public bool HasEntity(long metadataStructureId, string name)
        {
            MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(metadataStructureId);

            // get MetadataStructure
            if (metadataStructure != null && metadataStructure.Extra != null)
            {
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
                IEnumerable<XElement> tmp = XmlUtility.GetXElementByNodeName(nodeNames.entity.ToString(), xDoc);
                if (tmp.Any())
                {
                    foreach (var entity in tmp)
                    {
                        string tmpname = "";
                        if (entity.HasAttributes && entity.Attribute("name") != null)
                            tmpname = entity.Attribute("name").Value.ToLower();

                        if (tmpname.Equals(name.ToLower())) return true;
                    }
                }
            }
            return false;
        }

        #endregion get

        #region add

        public XmlDocument AddReferenceToXml(XmlDocument Source, string nodeName, string nodeValue, string nodeType, string destinationPath)
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
                    if (attr.Name == "value") attr.Value = nodeValue;
                    if (attr.Name == "type") attr.Value = nodeType;
                }
            }
            else
            {
                XmlAttribute name = Source.CreateAttribute("name");
                name.Value = nodeName;
                XmlAttribute value = Source.CreateAttribute("value");
                value.Value = nodeValue;
                XmlAttribute type = Source.CreateAttribute("type");
                type.Value = nodeType;

                x.Attributes.Append(name);
                x.Attributes.Append(value);
                x.Attributes.Append(type);
            }

            return Source;
        }

        public XmlDocument AddReferenceToXml(XmlDocument Source, string nodeName, string nodeValue, string nodeType, string destinationPath, Dictionary<string, string> additionalAttributes)
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
                    if (attr.Name == "value") attr.Value = nodeValue;
                    if (attr.Name == "type") attr.Value = nodeType;
                }
            }
            else
            {
                XmlAttribute name = Source.CreateAttribute("name");
                name.Value = nodeName;
                XmlAttribute value = Source.CreateAttribute("value");
                value.Value = nodeValue;
                XmlAttribute type = Source.CreateAttribute("type");
                type.Value = nodeType;

                x.Attributes.Append(name);
                x.Attributes.Append(value);
                x.Attributes.Append(type);
            }

            if (additionalAttributes.Keys.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in additionalAttributes)
                {
                    bool exist = false;
                    if (x.Attributes.Count > 0)
                    {
                        foreach (XmlAttribute attr in x.Attributes)
                        {
                            if (attr.Name.Equals(kvp.Key))
                            {
                                attr.Value = kvp.Value;
                                exist = true;
                                break;
                            }
                        }
                    }

                    if (!exist)
                    {
                        XmlAttribute attr = Source.CreateAttribute(kvp.Key);
                        attr.Value = kvp.Value;
                        x.Attributes.Append(attr);
                    }
                }
            }

            return Source;
        }

        private XmlNode createMissingNodes(string destinationParentXPath, XmlNode parentNode, XmlDocument doc,
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

        #endregion add

        #region set

        /// <summary>
        /// Sets the value of the node of the XmlDocument xmlDoc specified by the parameter "name" to the given value
        /// </summary>
        /// <param name="datasetVersion"></param>
        /// <param name="xmlDoc"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public XmlDocument SetInformation(DatasetVersion datasetVersion, XmlDocument xmlDoc, NameAttributeValues name, string value)
        {
            // get MetadataStructure
            if (datasetVersion != null && datasetVersion.Dataset != null && datasetVersion.Dataset.MetadataStructure != null && datasetVersion.Metadata != null)
            {
                MetadataStructure metadataStructure = datasetVersion.Dataset.MetadataStructure;
                XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)datasetVersion.Dataset.MetadataStructure.Extra);
                XElement temp = XmlUtility.GetXElementByAttribute(nodeNames.nodeRef.ToString(), "name", name.ToString(), xDoc);

                string xpath = temp.Attribute("value").Value.ToString();
                xmlDoc.SelectSingleNode(xpath).InnerText = value;
                return xmlDoc;
            }
            return null;
        }

        #endregion set
    }

    public enum nodeNames
    {
        nodeRef,
        convertRef,
        entity,
        parameter
    }

    public enum NameAttributeValues
    {
        title,
        description,
        active
    }

    public enum AttributeNames
    {
        name,
        value,
        type,
    }

    public enum AttributeType
    {
        xpath,
        entity,
        parameter
    }

    public enum TransmissionType
    {
        mappingFileExport,
        mappingFileImport
    }
}