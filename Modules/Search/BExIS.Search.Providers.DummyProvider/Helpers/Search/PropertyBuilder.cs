using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using BExIS.Search.Model;
using Vaiona.Util.Cfg;

namespace BExIS.Search.Providers.DummyProvider.Helpers.Helpers.Search
{
    public class PropertyBuilder
    {

        public static string PROPERTIES_XML_NODE_NAME = "properties";

        /// <summary>
        /// IEnumerable List of all properties
        /// </summary>
        public IEnumerable<Property> AllProperties;

        /// <summary>
        /// the configuration file for the properties as XmlDocument
        /// </summary>
        XmlDocument _source = new XmlDocument();

        XmlReader _xmlReader;


        /// <summary>
        /// Properties will be created based on data(Metadatalist)
        /// </summary>
        /// <param name="data"></param>
        public PropertyBuilder(List<Metadata> data)
        {
            MetadataList = data;

            // load the cofiguration xml file
            LoadXml();

            // create all diffrent properties
            AllProperties = GetAllProperties();


            // load the Values of the Properties from metadatalist
            LoadAllValues(MetadataList);
        }

        public List<Metadata> MetadataList
        {
            get; 
            set;
        }
        
        /// <summary>
        /// Load current static configuration NewProperties.xml document
        /// </summary>
        private void LoadXml(){
            _xmlReader = XmlReader.Create(AppConfiguration.GetModuleWorkspacePath("Search") + "DummyData/NewProperties.xml");
            _source.Load(_xmlReader);
        }

        /// <summary>
        /// Get all property objects based on the configuration
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<Property> GetAllProperties()
        {

            List<Property> l = new List<Property>();

            XmlNodeList typeList = _source.GetElementsByTagName(PROPERTIES_XML_NODE_NAME);
            XmlNode root = typeList.Item(0);

            foreach (XmlNode x in root.ChildNodes)
            {
                Property c = new Property();
                c.Name = x.Attributes[DataHelperConstClass.PROPERTY_DATA_SOURCE_KEY].InnerText;
                c.DisplayName = x.Attributes[DataHelperConstClass.PROPERTY_DISPLAY_TITLE].InnerText;
                c.DataSourceKey = x.Attributes[DataHelperConstClass.PROPERTY_DATA_SOURCE_KEY].InnerText;
                c.UIComponent = x.Attributes[DataHelperConstClass.PROPERTY_UI_COMPONENT].InnerText;
                c.DefaultValue = x.Attributes[DataHelperConstClass.PROPERTY_DEFAULT_VALUE_NODE].InnerText;
                c.AggregationType = x.Attributes[DataHelperConstClass.PROPERTY_AGGREGATION_TYPE].InnerText;
                c.DataType = x.Attributes[DataHelperConstClass.PROPERTY_DATE_TYPE].InnerText;

                if (x.Attributes[DataHelperConstClass.PROPERTY_DIRECTION].InnerText.Equals(Direction.increase.ToString()))
                {
                    c.Direction = Direction.increase;
                }
                else
                {
                    c.Direction = Direction.decrease;
                }

                l.Add(c);
            }

            return l;
        }

        /// <summary>
        /// <para>Load all values for the properties</para>
        /// <para>Depends on all Data</para>
        /// </summary>
        /// <param name="data">List of metadata objects</param>
        private void LoadAllValues(List<Metadata> data) {

            foreach (Property p in AllProperties)
            {
                if (p.DataType == DataHelperConstClass.PROPERTY_DATATYPE_STRING)
                {
                    p.Values = MetadataReader.GetAllValuesByNodeDistinct(p.DataSourceKey, data);

                    if (p.DefaultValue != "")
                    {
                        List<string> l = p.Values.ToList();
                        l.Add(p.DefaultValue);
                        l.Sort();
                        p.Values = l;

                    }
                }

                if (p.DataType == DataHelperConstClass.PROPERTY_DATATYPE_DATE)
                {
                    p.Values = MetadataReader.GetAllValuesByNode(p.DataSourceKey, data);
                    p.Formats = MetadataReader.GetAllValuesByNode(DataHelperConstClass.PROPERTY_DATE_FORMAT, data);

                        if (p.DefaultValue != "")
                        {
                            List<string> l = p.Values.ToList();
                            l.Add(p.DefaultValue);
                            l.Sort();
                            p.Values = l;
                        }
                }
            }
        }
    }
}