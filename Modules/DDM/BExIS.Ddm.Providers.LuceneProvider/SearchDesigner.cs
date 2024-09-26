using BExIS.Ddm.Api;
using BExIS.Ddm.Providers.LuceneProvider.Helpers;
using BExIS.Ddm.Providers.LuceneProvider.Indexer;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Utils.Models;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace BExIS.Ddm.Providers.LuceneProvider
{
    public class SearchDesigner : ISearchDesigner
    {
        private XmlDocument _configXML;
        private List<SearchAttribute> _searchAttributeList = new List<SearchAttribute>();
        private List<SearchMetadataNode> _metadataNodes = new List<SearchMetadataNode>();
        private bool _includePrimaryData = false;

        private BexisIndexer bexisIndexer = new BexisIndexer();
        private XmlMetadataHelper xmlMetadataHelper = new XmlMetadataHelper();

        public List<SearchAttribute> Get()
        {
            this.Load();
            this._metadataNodes = new List<SearchMetadataNode>();
            return this._searchAttributeList;
        }

        //read xml config file
        private void Load()
        {
            this._configXML = new XmlDocument();
            this._configXML.Load(FileHelper.ConfigFilePath);
            XmlNodeList fieldProperties = this._configXML.GetElementsByTagName("field");

            int index = 0;
            foreach (XmlNode fieldProperty in fieldProperties)
            {
                if (!fieldProperty.Attributes.GetNamedItem("lucene_name").Value.Equals("Primarydata"))
                {
                    SearchAttribute sa = new SearchAttribute();
                    sa.id = index;
                    //Names
                    if (fieldProperty.Attributes.GetNamedItem("display_name") != null)
                        sa.displayName = fieldProperty.Attributes.GetNamedItem("display_name").Value;

                    if (fieldProperty.Attributes.GetNamedItem("lucene_name") != null)
                        sa.sourceName = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;

                    if (fieldProperty.Attributes.GetNamedItem("metadata_name") != null)
                        sa.metadataName = fieldProperty.Attributes.GetNamedItem("metadata_name").Value;

                    //types
                    if (fieldProperty.Attributes.GetNamedItem("type") != null)
                        sa.searchType = SearchAttribute.GetSearchType(fieldProperty.Attributes.GetNamedItem("type").Value);

                    if (fieldProperty.Attributes.GetNamedItem("primitive_type") != null)
                        sa.dataType = SearchAttribute.GetDataType(fieldProperty.Attributes.GetNamedItem("primitive_type").Value);

                    //// parameter for index
                    if (fieldProperty.Attributes.GetNamedItem("store") != null)
                        sa.store = SearchAttribute.GetBoolean(fieldProperty.Attributes.GetNamedItem("store").Value);

                    if (fieldProperty.Attributes.GetNamedItem("multivalued") != null)
                        sa.multiValue = SearchAttribute.GetBoolean(fieldProperty.Attributes.GetNamedItem("multivalued").Value);

                    if (fieldProperty.Attributes.GetNamedItem("analyzed") != null)
                        sa.analysed = SearchAttribute.GetBoolean(fieldProperty.Attributes.GetNamedItem("analyzed").Value);

                    if (fieldProperty.Attributes.GetNamedItem("norm") != null)
                        sa.norm = SearchAttribute.GetBoolean(fieldProperty.Attributes.GetNamedItem("norm").Value);

                    if (fieldProperty.Attributes.GetNamedItem("boost") != null)
                        sa.boost = Convert.ToDouble(fieldProperty.Attributes.GetNamedItem("boost").Value);

                    // Resultview
                    if (fieldProperty.Attributes.GetNamedItem("header_item") != null)
                        sa.headerItem = SearchAttribute.GetBoolean(fieldProperty.Attributes.GetNamedItem("header_item").Value);

                    if (fieldProperty.Attributes.GetNamedItem("default_visible_item") != null)
                        sa.defaultHeaderItem = SearchAttribute.GetBoolean(fieldProperty.Attributes.GetNamedItem("default_visible_item").Value);

                    if (fieldProperty.Attributes.GetNamedItem("direction") != null)
                        sa.direction = SearchAttribute.GetDirection(fieldProperty.Attributes.GetNamedItem("direction").Value);

                    if (fieldProperty.Attributes.GetNamedItem("uiComponent") != null)
                        sa.uiComponent = SearchAttribute.GetUIComponent(fieldProperty.Attributes.GetNamedItem("uiComponent").Value);

                    if (fieldProperty.Attributes.GetNamedItem("aggregationType") != null)
                        sa.aggregationType = SearchAttribute.GetAggregationType(fieldProperty.Attributes.GetNamedItem("aggregationType").Value);

                    if (fieldProperty.Attributes.GetNamedItem("date_format") != null)
                        sa.dateFormat = fieldProperty.Attributes.GetNamedItem("date_format").Value;


                    if (fieldProperty.Attributes.GetNamedItem("placeholder") != null)
                        sa.placeholder = fieldProperty.Attributes.GetNamedItem("placeholder").Value.ToString();

                    this._searchAttributeList.Add(sa);
                    index++;
                }
                else
                {
                    this._includePrimaryData = true;
                }
            }
        }

        public bool IsPrimaryDataIncluded()
        {
            return this._includePrimaryData;
        }

        public List<SearchMetadataNode> GetMetadataNodes()
        {
            if (_metadataNodes.Count > 0)
                return _metadataNodes;
            else
            {
                using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
                {
                    List<long> ids = new List<long>();

                    ids = metadataStructureManager.Repo.Query().Select(p => p.Id).ToList();

                    foreach (long id in ids)
                    {
                        _metadataNodes.AddRange(GetAllXPathsOfSimpleAttributes(id));
                    }

                    _metadataNodes = _metadataNodes.Distinct().ToList();
                    _metadataNodes.Sort((x, y) => String.Compare(x.DisplayName, y.DisplayName));
                    return _metadataNodes;
                }
            }
        }

        public List<SearchMetadataNode> GetAllXPathsOfSimpleAttributes(long id)
        {
            return xmlMetadataHelper.GetAllXPathsOfSimpleAttributes(id);
        }

        public void Set(List<SearchAttribute> SearchAttributeList)
        {
            this._searchAttributeList = SearchAttributeList;
            Save();
        }

        public void Set(List<SearchAttribute> SearchAttributeList, bool includePrimaryData)
        {
            this._searchAttributeList = SearchAttributeList;
            this._includePrimaryData = includePrimaryData;
            Save();
        }

        // write xml config file
        private void Save()
        {
            XmlElement root;
            FileStream fileStream;

            try
            {
                this._configXML = new XmlDocument();
                if (String.IsNullOrEmpty(FileHelper.ConfigFilePath))
                {
                    root = this._configXML.CreateElement("luceneConfig");
                    this._configXML.AppendChild(root);
                }
                else
                {
                    fileStream = new FileStream(FileHelper.ConfigFilePath, FileMode.Open, FileAccess.Read);
                    this._configXML.Load(fileStream);
                    root = this._configXML.DocumentElement;
                    root.RemoveAll();

                    fileStream.Close();
                }

                //add primary data node
                if (this._includePrimaryData)
                {
                    XmlElement xe = this._configXML.CreateElement("field");
                    xe = SetPrimaryDataAttributeToNode(xe);
                    root.AppendChild(xe);
                }

                foreach (SearchAttribute sa in this._searchAttributeList)
                {
                    XmlElement xe = this._configXML.CreateElement("field");
                    xe = SetAttributesToNode(xe, sa);
                    root.AppendChild(xe);
                }

                //root.AppendChild(xe);
                //System.IO.File.
                object lk = new object();
                lock (fileStream = new FileStream(FileHelper.ConfigFilePath, FileMode.Open, FileAccess.Write))
                {
                    fileStream.SetLength(0);
                    this._configXML.Save(fileStream);
                    fileStream.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void Reset()
        {
            this._configXML = new XmlDocument();
            this._configXML.Load(FileHelper.ConfigFilePath);

            XmlDocument BackUpXML = new XmlDocument();
            BackUpXML.Load(FileHelper.ConfigBackUpFilePath);

            this._configXML = BackUpXML;
            this._configXML.Save(FileHelper.ConfigFilePath);
        }

        public void Reload(bool onlyReleasedTags)
        {
            try
            {
                bexisIndexer.ReIndex(onlyReleasedTags);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Dispose()
        {
            bexisIndexer.Dispose();
        }

        private XmlElement SetAttributesToNode(XmlElement xmlElement, SearchAttribute sa)
        {
            // names
            XmlAttribute xa = this._configXML.CreateAttribute("display_name");
            xa.Value = sa.displayName;
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("lucene_name");
            xa.Value = sa.sourceName;
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("metadata_name");
            xa.Value = sa.metadataName;
            xmlElement.Attributes.Append(xa);

            //types
            xa = this._configXML.CreateAttribute("type");
            xa.Value = SearchAttribute.GetSearchTypeAsString(sa.searchType);
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("primitive_type");
            xa.Value = SearchAttribute.GetDataTypeAsString(sa.dataType);
            xmlElement.Attributes.Append(xa);

            // parameter for index
            xa = this._configXML.CreateAttribute("store");
            xa.Value = SearchAttribute.GetBooleanAsString(sa.store);
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("multivalued");
            xa.Value = SearchAttribute.GetBooleanAsString(sa.multiValue);
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("analysed");
            xa.Value = SearchAttribute.GetBooleanAsString(sa.analysed);
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("norm");
            xa.Value = SearchAttribute.GetBooleanAsString(sa.norm);
            xmlElement.Attributes.Append(xa);

            //boost
            xa = this._configXML.CreateAttribute("boost");
            xa.Value = sa.boost.ToString();
            xmlElement.Attributes.Append(xa);

            //placeholder
            xa = this._configXML.CreateAttribute("placeholder");
            xa.Value = sa.placeholder.ToString();
            xmlElement.Attributes.Append(xa);

            // ResultView
            xa = this._configXML.CreateAttribute("header_item");
            xa.Value = SearchAttribute.GetBooleanAsString(sa.headerItem);
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("default_visible_item");
            xa.Value = SearchAttribute.GetBooleanAsString(sa.defaultHeaderItem);
            xmlElement.Attributes.Append(xa);

            // properties
            if (sa.searchType.Equals(SearchComponentBaseType.Property))
            {
                xa = this._configXML.CreateAttribute("direction");
                xa.Value = SearchAttribute.GetDirectionAsString(sa.direction);
                xmlElement.Attributes.Append(xa);

                xa = this._configXML.CreateAttribute("uiComponent");
                xa.Value = SearchAttribute.GetUIComponentAsString(sa.uiComponent);
                xmlElement.Attributes.Append(xa);

                xa = this._configXML.CreateAttribute("aggregationType");
                xa.Value = SearchAttribute.GetAggregationTypeAsString(sa.aggregationType);
                xmlElement.Attributes.Append(xa);

                xa = this._configXML.CreateAttribute("date_format");
                xa.Value = sa.dateFormat;
                xmlElement.Attributes.Append(xa);
            }

            return xmlElement;
        }

        private XmlElement SetPrimaryDataAttributeToNode(XmlElement xmlElement)
        {
            // names
            XmlAttribute xa = this._configXML.CreateAttribute("display_name");
            xa.Value = "Primary Data";
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("lucene_name");
            xa.Value = "Primarydata";
            xmlElement.Attributes.Append(xa);

            //types
            xa = this._configXML.CreateAttribute("type");
            xa.Value = "primary_data_field";
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("primitive_type");
            xa.Value = "String";
            xmlElement.Attributes.Append(xa);

            // parameter for index
            xa = this._configXML.CreateAttribute("store");
            xa.Value = "yes";
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("analysed");
            xa.Value = "yes";
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("norm");
            xa.Value = "yes";
            xmlElement.Attributes.Append(xa);

            //boost
            xa = this._configXML.CreateAttribute("boost");
            xa.Value = "3";
            xmlElement.Attributes.Append(xa);

            return xmlElement;
        }
    }
}