using BExIS.Ddm.Providers.LuceneProvider.Helpers;
using BExIS.Ddm.Providers.LuceneProvider.Searcher;
using BExIS.Utils.Models;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

/// <summary>
///
/// </summary>        
namespace BExIS.Ddm.Providers.LuceneProvider.Config
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public static class SearchConfig
    {
        static XmlDocument configXML;
        static IndexReader _Reader = BexisIndexSearcher.getIndexReader();

        private static searchInitObjects sIO = new searchInitObjects();

        private static List<Facet> AllFacetsDefault = sIO.AllFacets;
        private static List<Property> AllPropertiesDefault = sIO.AllProperties;
        private static List<Category> AllCategoriesDefault = sIO.AllCategories;

        private static HashSet<string> numericProperties = new HashSet<string>();
        private static Boolean isLoaded = false;

        public static List<XmlNode> facetXmlNodeList = new List<XmlNode>();
        public static List<XmlNode> propertyXmlNodeList = new List<XmlNode>();
        public static List<XmlNode> categoryXmlNodeList = new List<XmlNode>();
        public static List<XmlNode> generalXmlNodeList = new List<XmlNode>();
        public static List<XmlNode> headerItemXmlNodeList = new List<XmlNode>();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>               
        public static void Reset()
        {
            isLoaded = false;
            configXML = null;
            _Reader = BexisIndexSearcher.getIndexReader();

            sIO = new searchInitObjects();

            AllFacetsDefault = sIO.AllFacets;
            AllPropertiesDefault = sIO.AllProperties;
            AllCategoriesDefault = sIO.AllCategories;

            numericProperties = new HashSet<string>();

            facetXmlNodeList = new List<XmlNode>();
            propertyXmlNodeList = new List<XmlNode>();
            categoryXmlNodeList = new List<XmlNode>();
            generalXmlNodeList = new List<XmlNode>();
            headerItemXmlNodeList = new List<XmlNode>();

        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public static void LoadConfig()
        {
            if (!isLoaded)
            {
                Load();
                isLoaded = true;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        private static void Load()
        {
            // check if directory exist
            string dicPath = Path.GetDirectoryName(FileHelper.ConfigFilePath);
            if (!Directory.Exists(dicPath)) Directory.CreateDirectory(dicPath); // create if not exist

            configXML = new XmlDocument();

            configXML.Load(FileHelper.ConfigFilePath);
            XmlNodeList fieldProperties = configXML.GetElementsByTagName("field");

            Category categoryDefault = new Category();
            categoryDefault.Name = "All";
            categoryDefault.Value = "All";
            categoryDefault.DisplayName = "All";
            categoryDefault.DefaultValue = "nothing";
            AllCategoriesDefault.Add(categoryDefault);
            foreach (XmlNode fieldProperty in fieldProperties)
            {
                if (!fieldProperty.Attributes.GetNamedItem("type").Value.ToLower().Equals("primary_data_field"))
                {
                    String headerItem = fieldProperty.Attributes.GetNamedItem("header_item").Value;
                    String storeItem = fieldProperty.Attributes.GetNamedItem("store").Value;
                    if (headerItem.ToLower().Equals("yes") && storeItem.ToLower().Equals("yes"))
                    {
                        headerItemXmlNodeList.Add(fieldProperty);
                    }

                    String fieldType = fieldProperty.Attributes.GetNamedItem("type").Value;
                    String fieldName = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;

                    String primitiveType = fieldProperty.Attributes.GetNamedItem("primitive_type").Value;
                    if (!primitiveType.ToLower().Equals("string"))
                    {
                        numericProperties.Add(fieldName.ToLower());
                    }

                    if (fieldType.ToLower().Equals("facet_field"))
                    {
                        facetXmlNodeList.Add(fieldProperty);
                        Facet cDefault = new Facet();
                        cDefault.Name = fieldName;
                        cDefault.Text = fieldName;
                        cDefault.Value = fieldName;
                        cDefault.DisplayName = fieldProperty.Attributes.GetNamedItem("display_name").Value;

                        cDefault.Childrens = new List<Facet>();
                        List<Facet> lcDefault = new List<Facet>();
                        try
                        {
                            _Reader = BexisIndexSearcher.getIndexReader();
                            Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "id", new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30)).Parse("*:*");
                            using (SimpleFacetedSearch sfs = new SimpleFacetedSearch(_Reader, new string[] { "facet_" + fieldName }))
                            {
                                SimpleFacetedSearch.Hits hits = sfs.Search(query);

                                int cCount = 0;
                                foreach (SimpleFacetedSearch.HitsPerFacet hpg in hits.HitsPerFacet)
                                {
                                    if (!hpg.Name.ToString().Equals(""))
                                    {
                                        Facet ccDefault = new Facet();
                                        ccDefault.Name = hpg.Name.ToString();
                                        ccDefault.Text = hpg.Name.ToString();
                                        ccDefault.Value = hpg.Name.ToString();
                                        ccDefault.Count = (int)hpg.HitCount;
                                        if (ccDefault.Count > 0) cCount++;
                                        cDefault.Childrens.Add(ccDefault);
                                    }
                                }
                                cDefault.Count = cCount;
                                AllFacetsDefault.Add(cDefault);
                            }

                        }
                        catch
                        {

                        }
                    }

                    else if (fieldType.ToLower().Equals("property_field"))
                    {
                        propertyXmlNodeList.Add(fieldProperty);
                        Property cDefault = new Property();
                        //c.Id = x.Attributes[Property.ID].InnerText;
                        cDefault.Name = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;
                        cDefault.DisplayName = fieldProperty.Attributes.GetNamedItem("display_name").Value;
                        cDefault.DisplayTitle = fieldProperty.Attributes.GetNamedItem("display_name").Value;
                        cDefault.DataSourceKey = fieldProperty.Attributes.GetNamedItem("metadata_name").Value;
                        cDefault.UIComponent = fieldProperty.Attributes.GetNamedItem("uiComponent").Value; ;
                        cDefault.AggregationType = fieldProperty.Attributes.GetNamedItem("aggregationType").Value;
                        cDefault.DefaultValue = "All";
                        cDefault.DataType = fieldProperty.Attributes.GetNamedItem("primitive_type").Value;
                        if (cDefault.UIComponent.ToLower().Equals("range") && cDefault.DataType.ToLower().Equals("date"))
                        {
                            String direction = fieldProperty.Attributes.GetNamedItem("direction").Value;

                            if (direction.ToLower().Equals("increase"))
                            {
                                cDefault.Direction = Direction.increase;
                            }
                            else
                            {
                                cDefault.Direction = Direction.decrease;
                            }
                        }


                        Query query = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "id", new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29)).Parse("*:*");
                        try
                        {
                            _Reader = BexisIndexSearcher.getIndexReader();


                            using (SimpleFacetedSearch sfs = new SimpleFacetedSearch(_Reader, new string[] { "property_" + fieldName }))
                            {
                                SimpleFacetedSearch.Hits hits = sfs.Search(query);
                                List<string> laDefault = new List<string>();
                                foreach (SimpleFacetedSearch.HitsPerFacet hpg in hits.HitsPerFacet)
                                {
                                    if (!string.IsNullOrEmpty(hpg?.Name?.ToString()))
                                        laDefault.Add(hpg.Name.ToString());
                                }

                                //if (!cDefault.UIComponent.ToLower().Equals("range")) { laDefault.Add("All"); };
                                laDefault.Sort();
                                cDefault.Values = laDefault;
                                AllPropertiesDefault.Add(cDefault);
                            }
                        }
                        catch
                        {
                            throw;
                        }
                    }
                    else if (fieldType.ToLower().Equals("category_field"))
                    {
                        categoryXmlNodeList.Add(fieldProperty);

                        Category cDefault = new Category();
                        cDefault.Name = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;
                        cDefault.DisplayName = fieldProperty.Attributes.GetNamedItem("display_name").Value;
                        cDefault.Value = fieldProperty.Attributes.GetNamedItem("lucene_name").Value; ;
                        cDefault.DefaultValue = "nothing";

                        AllCategoriesDefault.Add(cDefault);
                    }
                    else if (fieldType.ToLower().Equals("general_field"))
                    {
                        generalXmlNodeList.Add(fieldProperty);
                    }

                }
                else if (fieldProperty.Attributes.GetNamedItem("type").Value.ToLower().Equals("primary_data_field"))
                {
                    categoryXmlNodeList.Add(fieldProperty);
                    Category cDefault = new Category();
                    cDefault.Name = fieldProperty.Attributes.GetNamedItem("lucene_name").Value;
                    cDefault.DisplayName = fieldProperty.Attributes.GetNamedItem("display_name").Value;
                    cDefault.Value = fieldProperty.Attributes.GetNamedItem("lucene_name").Value; ;
                    cDefault.DefaultValue = "nothing";
                    AllCategoriesDefault.Add(cDefault);
                }


            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public static void reloadConfig()
        {
            isLoaded = false;
            LoadConfig();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public static HashSet<string> getNumericProperties() { return numericProperties; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public static IEnumerable<Facet> getFacets() { return AllFacetsDefault; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public static IEnumerable<Property> getProperties() { return AllPropertiesDefault; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public static IEnumerable<Category> getCategories() { return AllCategoriesDefault; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public static IEnumerable<Facet> getFacetsCopy()
        {
            searchInitObjects sIOCopy = DeepCopy<searchInitObjects>(sIO);
            return sIOCopy.AllFacets;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public static IEnumerable<Property> getPropertiesCopy()
        {
            searchInitObjects sIOCopy = DeepCopy<searchInitObjects>(sIO);
            return sIOCopy.AllProperties;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public static IEnumerable<Category> getCategoriesCopy()
        {
            searchInitObjects sIOCopy = DeepCopy<searchInitObjects>(sIO);
            return sIOCopy.AllCategories;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("Object cannot be null");
            return (T)Process(obj, new Dictionary<object, object>() { });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="obj"></param>
        /// <param name="circular"></param>
        /// <returns></returns>
        static object Process(object obj, Dictionary<object, object> circular)
        {
            if (obj == null)
                return null;
            Type type = obj.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }
            if (type.IsArray)
            {
                if (circular.ContainsKey(obj))
                    return circular[obj];
                string typeNoArray = type.FullName.Replace("[]", string.Empty);
                Type elementType = Type.GetType(typeNoArray + ", " + type.Assembly.FullName);
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                circular[obj] = copied;
                for (int i = 0; i < array.Length; i++)
                {
                    object element = array.GetValue(i);
                    object copy = null;
                    if (element != null && circular.ContainsKey(element))
                        copy = circular[element];
                    else
                        copy = Process(element, circular);
                    copied.SetValue(copy, i);
                }
                return Convert.ChangeType(copied, obj.GetType());
            }
            if (type.IsClass)
            {
                if (circular.ContainsKey(obj))
                    return circular[obj];
                object toret = Activator.CreateInstance(obj.GetType());
                circular[obj] = toret;
                System.Reflection.FieldInfo[] fields = GetAllFields(type).ToArray();
                foreach (System.Reflection.FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                        continue;
                    object copy = circular.ContainsKey(fieldValue) ? circular[fieldValue] : Process(fieldValue, circular);
                    field.SetValue(toret, copy);
                }
                return toret;
            }
            else
                throw new ArgumentException("Unknown type");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<System.Reflection.FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<System.Reflection.FieldInfo>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            return t.GetFields(flags).Union(GetAllFields(t.BaseType));
        }

    }
}
