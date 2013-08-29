using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BExIS.Search.Providers.LuceneProvider.Helpers;
using BExIS.Search.Providers.LuceneProvider.Indexer;
using BExIS.Search.Api;
using BExIS.Search.Model;

namespace BExIS.Search.Providers.LuceneProvider
{
    public class SearchDesigner:ISearchDesigner
    {
        private XmlDocument _configXML;
        private List<SearchAttribute> _searchAttributeList = new List<SearchAttribute>();
        private List<string> _metadataNodes = new List<string>();

        public List<SearchAttribute> Get()
        {
            this.Load();
            this._metadataNodes = new List<string>();
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
                SearchAttribute sa = new SearchAttribute();
                sa.id = index;
                //Names
                if(fieldProperty.Attributes.GetNamedItem("display_name")!=null)
                    sa.displayName = fieldProperty.Attributes.GetNamedItem("display_name").Value;

                if(fieldProperty.Attributes.GetNamedItem("lucene_name")!=null)
                    sa.sourceName = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;

                if(fieldProperty.Attributes.GetNamedItem("metadata_name")!=null)
                    sa.metadataName = fieldProperty.Attributes.GetNamedItem("metadata_name").Value;
                

                //types
                if(fieldProperty.Attributes.GetNamedItem("type")!=null)
                    sa.searchType = SearchAttribute.GetSearchType(fieldProperty.Attributes.GetNamedItem("type").Value);

                if(fieldProperty.Attributes.GetNamedItem("primitive_type")!=null)
                    sa.dataType = SearchAttribute.GetDataType(fieldProperty.Attributes.GetNamedItem("primitive_type").Value);

             

                //// parameter for index
                if(fieldProperty.Attributes.GetNamedItem("store")!=null)
                    sa.store = SearchAttribute.GetBoolean(fieldProperty.Attributes.GetNamedItem("store").Value);

                if(fieldProperty.Attributes.GetNamedItem("multivalued")!=null)
                    sa.multiValue = SearchAttribute.GetBoolean(fieldProperty.Attributes.GetNamedItem("multivalued").Value);

                if(fieldProperty.Attributes.GetNamedItem("analyzed")!=null)
                    sa.analysed = SearchAttribute.GetBoolean(fieldProperty.Attributes.GetNamedItem("analyzed").Value);

                if(fieldProperty.Attributes.GetNamedItem("norm")!=null)
                    sa.norm = SearchAttribute.GetBoolean(fieldProperty.Attributes.GetNamedItem("norm").Value);

                if(fieldProperty.Attributes.GetNamedItem("boost")!=null)
                     sa.boost = Convert.ToDouble(fieldProperty.Attributes.GetNamedItem("boost").Value);


                // Resultview
                    if(fieldProperty.Attributes.GetNamedItem("header_item")!=null)
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

                this._searchAttributeList.Add(sa);
                index++;
            }

        }

        public List<string> GetMetadataNodes()
        {
            if (_metadataNodes.Count > 0)
                return _metadataNodes;
            else
            {
                _metadataNodes.Add("bgc:title");
                _metadataNodes.Add("bgc:language");
                _metadataNodes.Add("bgc:owner");
                _metadataNodes.Add("bgc:author");
                _metadataNodes.Add("bgc:projectName");
                _metadataNodes.Add("bgc:projectLeader");
                _metadataNodes.Add("bgc:institute");
                _metadataNodes.Add("bgc:zipCode");
                _metadataNodes.Add("bgc:city");
                _metadataNodes.Add("bgc:email");
                _metadataNodes.Add("bgc:url");
                _metadataNodes.Add("bgc:grassland");
                _metadataNodes.Add("bgc:forest");
                _metadataNodes.Add("plotLevel");
                _metadataNodes.Add("bgc:taxon");
                _metadataNodes.Add("bgc:process");
                _metadataNodes.Add("bgc:service");
                _metadataNodes.Add("bgc:introduction");
                _metadataNodes.Add("bgc:type");
                _metadataNodes.Add("bgc:instruments");
                _metadataNodes.Add("bgc:calibration");
                _metadataNodes.Add("bgc:procedures");
                _metadataNodes.Add("bgc:acronym");
                _metadataNodes.Add("bgc:meaning");
                _metadataNodes.Add("bgc:keyword");
                _metadataNodes.Add("bgc:format");
                _metadataNodes.Add("bgc:startDate");
                _metadataNodes.Add("bgc:endDate");
                _metadataNodes.Add("bgc:dateEntry");
                _metadataNodes.Add("bgc:dateLastModified");
                _metadataNodes.Add("bgc:qualityLevel");
                _metadataNodes.Add("bgc:dataStatus");
                _metadataNodes.Add("bgc:keyExplo");
                _metadataNodes.Add("bgc:version");
                _metadataNodes.Add("bgc:id");

                _metadataNodes.Sort();

                return _metadataNodes;
            }
        }

        public void Set(List<SearchAttribute> SearchAttributeList)
        {
            this._searchAttributeList = SearchAttributeList;
            Save();
        }

        // write xml config file
        private void Save()
        {
            if (this._configXML == null)
            { 
                this._configXML = new XmlDocument();
                //this._configXML.Load(FileHelper.ConfigFilePath);
            }

            //XmlNodeList fieldProperties = this._configXML.GetElementsByTagName("field");
             
            XmlElement r = this._configXML.CreateElement("luceneConfig");

            this._configXML.AppendChild(r);

            XmlElement root = this._configXML.DocumentElement;

            //XmlNodeList list = new XmlNodeList();

            foreach (SearchAttribute sa in this._searchAttributeList)
            {
                XmlElement xe = this._configXML.CreateElement("field");
                xe = SetAttributesToNode(xe, sa);
                root.AppendChild(xe);
            }

            //root.AppendChild(xe);

            this._configXML.Save(FileHelper.ConfigFilePath);
            
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

        public void Reload()
        {
            BexisIndexer bi = new BexisIndexer();
            
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


            // ResultView
            xa = this._configXML.CreateAttribute("header_item");
            xa.Value = SearchAttribute.GetBooleanAsString(sa.headerItem);
            xmlElement.Attributes.Append(xa);

            xa = this._configXML.CreateAttribute("default_visible_item");
            xa.Value = SearchAttribute.GetBooleanAsString(sa.headerItem);
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

    }
}
