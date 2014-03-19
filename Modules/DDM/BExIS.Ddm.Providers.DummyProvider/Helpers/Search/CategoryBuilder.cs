using System.Collections.Generic;
using System.Xml;
using BExIS.Ddm.Model;
using Vaiona.Util.Cfg;

namespace BExIS.Ddm.Providers.DummyProvider.Helpers.Helpers.Search
{
    public class CategoryBuilder
    {

        /// <summary>
        /// consist the configuration xml
        /// </summary>
        static XmlDocument _source = new XmlDocument();

        /// <summary>
        /// 
        /// </summary>
        public static string DROPDOWN_TYPE = "dropdown";

        private static void LoadXml()
        {
            XmlReader xr = XmlReader.Create(AppConfiguration.GetModuleWorkspacePath("Search") + "/DummyData/Category.xml");
            _source.Load(xr);
        }

        /// <summary>
        /// <para>Get a IEnumerable list of node for the category or dropdown.</para>
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
        /// <param name="type"> criteria/dropbox</param>
        /// <returns>IEnumerable list</returns>
        public static IEnumerable<Category> GetAllRootNodesAsList(string type)
        {

            LoadXml();

            List<Category> l = new List<Category>();

            XmlNodeList typeList = _source.GetElementsByTagName(type);
            XmlNode root = typeList.Item(0);

            foreach (XmlNode x in root.ChildNodes)
            {
                Category c = new Category();

                c.Name = x.LocalName;
                c.Value = x.LocalName;
                c.DefaultValue =  x.Attributes[DataHelperConstClass.CATEGORY_DEFAULT].InnerText;

                l.Add(c);
            }

            return l;
        }


    }
}
