using System.Collections.Generic;
using System.Linq;
using System.Xml;
using BExIS.Ddm.Model;
using Vaiona.Utils.Cfg;

namespace BExIS.Ddm.Providers.DummyProvider.Helpers.Helpers.Search
{
    public static class FacetBuilder
    {
        /// <summary>
        /// consist the configuration xml
        /// </summary>
        static XmlDocument _source = new XmlDocument();

        /// <summary>
        /// 
        /// </summary>
        public static string CATEGORY_TYPE = "criteria";

        /// <summary>
        /// 
        /// </summary>
        public static string DROPDOWN_TYPE = "dropdown";
       
        /// <summary>
        /// load static Caetgory.xml file
        /// </summary>
        private static void LoadXml()
        {
            XmlReader xr = XmlReader.Create(AppConfiguration.GetModuleWorkspacePath("DDM") + "/DummyData/Category.xml");
            _source.Load(xr);
        }

        /// <summary>
        /// <para>Get all node from type and there childrens from the metadata list.</para>
        /// <para>The static type define the possible types:</para>
        /// <list type="bullet">
        /// <listheader>
        /// <term>types</term>
        /// <description>description</description>
        /// </listheader>
        /// <item>
        /// <term>CATEGORY_TYPE</term>
        /// <description>description</description>
        ///</item>
        ///<item>
        /// <term>DROPDOWN_TYPE</term>
        /// <description>description</description>
        ///</item>
        ///</list>
        /// </summary>
        /// <param name="type">criteria/dropbox</param>
        /// <param name="metadataList">list of metadata objects</param>
        /// <returns> retrun all nodes as IEnumerable from a Metadata List </returns>
        public static IEnumerable<Facet> GetAllNodesAsListFromData(string type, List<Metadata> metadataList)
        {
            LoadXml();
            List<Facet> l = new List<Facet>();

            XmlNodeList typeList = _source.GetElementsByTagName(type);
            XmlNode root = typeList.Item(0);

            foreach (XmlNode x in root.ChildNodes)
            {
                Facet c = new Facet();
                c.Name = x.LocalName;
                c.Text = x.LocalName;
                c.Value = x.LocalName;
                c.Childrens = new List<Facet>();
                
                List<Facet> lc = MetadataReader.GetAllCategoriesByNodeDistinct(c, metadataList);

                if (lc.Count()>0)
                {
                    int childCount = 0;
                    foreach (Facet c_child in lc)
                    {
                        childCount += c_child.Count;
                        c.Childrens.Add(c_child);

                    }

                    c.Text = c.Name;
                    c.Count += childCount;
                }
                else c.Count = 0;
                l.Add(c);
            }
            return l;
        }
    }
}