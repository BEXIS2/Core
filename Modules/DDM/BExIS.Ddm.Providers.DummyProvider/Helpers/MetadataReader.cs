using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using BExIS.Ddm.Model;
using BExIS.Ddm.Providers.DummyProvider.Helpers.Helpers.Search;
using Vaiona.Util.Cfg;


namespace BExIS.Ddm.Providers.DummyProvider.Helpers
{
    public static class MetadataReader
    {

        public static List<Metadata> ListOfMetadata
        {
            get;
            set;
        }


        /// <summary>
        /// Get all metadata as a list
        /// </summary>
        /// <returns>List of metadata objects</returns>
        public static List<Metadata> GetAllMetadataDatasets() {

            List<Metadata> data = new List<Metadata>();

            string path = AppConfiguration.GetModuleWorkspacePath("Search") + "/DummyData/";//HttpContext.Current.Server.MapPath("/App_Data/xml/");

                // 1. Beispiel
                Metadata md1 = new Metadata();
                md1.LoadXml(path + "metadataXml1.xml");
                data.Add(md1);

                // 2. Beispiel
                Metadata md2 = new Metadata();
                md2.LoadXml(path + "metadataXml2.xml");
                data.Add(md2);

                // 3. Beispiel
                Metadata md3 = new Metadata();
                md3.LoadXml(path + "metadataXml3.xml");
                data.Add(md3);

                // 4. Beispiel
                Metadata md4 = new Metadata();
                md4.LoadXml(path + "metadataXml4.xml");
                data.Add(md4);

                // 5. Beispiel
                Metadata md5 = new Metadata();
                md5.LoadXml(path + "5.xml");
                data.Add(md5);

                // 6. Beispiel
                Metadata md6 = new Metadata();
                md6.LoadXml(path + "6.xml");
                data.Add(md6);

                // 7. Beispiel
                Metadata md7 = new Metadata();
                md7.LoadXml(path + "metadataXml100.xml");
                data.Add(md7);

                // 8. Beispiel
                Metadata md8 = new Metadata();
                md8.LoadXml(path + "metadataXml101.xml");
                data.Add(md8);

                // 9. Beispiel
                Metadata md9 = new Metadata();
                md9.LoadXml(path + "metadataXml102.xml");
                data.Add(md9);

                // 10. Beispiel
                Metadata md10 = new Metadata();
                md10.LoadXml(path + "metadataXml103.xml");
                data.Add(md10);

                // 11. Beispiel
                Metadata md11 = new Metadata();
                md11.LoadXml(path + "metadataXml104.xml");
                data.Add(md11);

                // 12. Beispiel
                Metadata md12 = new Metadata();
                md12.LoadXml(path + "metadataXml105.xml");
                data.Add(md12);



            return data;
        }

        /// <summary>
        ///<para>Get a filtered Metadata list</para> 
        ///<para>Where value from the node is equal</para>
        /// </summary>
        /// <param name="data">List of metadata objects</param>
        /// <param name="node">Name of the node</param>
        /// <param name="value">Filtervalue</param>
        /// <returns>Filtered metadata list</returns>
        public static List<Metadata> GetAllMetadataDatasetsByNode(List<Metadata> data, string node, string value)
        {
            List<Metadata> newData = new List<Metadata>();

            foreach (Metadata m in data)
            { 
                if (m.IsValueInNode(node,value)) newData.Add(m);
            }

            return newData;
        }

        /// <summary>
        ///<para>Get a filtered Metadata list</para> 
        ///<para>Where value from the node is equal</para>
        /// </summary>
        /// <param name="data">List of metadata objects</param>
        /// <param name="node">Name of the node</param>
        /// <param name="value">Filtervalue</param>
        /// <returns>Filtered metadata list</returns>
        public static List<Metadata> GetAllMetadataDatasetsByRangeNode(List<Metadata> data, string node, string value, bool desc)
        {
            List<Metadata> newData = new List<Metadata>();

            foreach (Metadata m in data)
            {
                if (m.IsDateInRangeOf(Convert.ToInt16(value),node,desc)) newData.Add(m);
            }

            return newData;
        }

        /// <summary>
        /// Get all metadata where the value is inside
        /// </summary>
        /// <param name="data">List of metadata objects</param>
        /// <param name="value"></param>
        /// <returns>List of metadata objects</returns>
        public static List<Metadata> GetAllMetadataDatasetsByValue(List<Metadata> data, string value)
        {
            List<Metadata> newData = new List<Metadata>();

            foreach (Metadata m in data)
            {
                if(m.IsValueInMetadata(value))newData.Add(m);
            }

            return newData;

        }

        // filter list by Value

        //public static List<Metadata> GetAllMetadataDatasetsWithListOfSearchValues(List<Metadata> data, Dictionary<string,List<string>> searchValues)
        //{
        //    IEnumerable<Metadata> newData = new List<Metadata>();
        //    List<Metadata> temp = new List<Metadata>();

        //    if (searchValues.Count() > 0)
        //    {
        //        bool first = true;
        //        foreach (KeyValuePair<string,List<string>> kvp in searchValues)
        //        {
        //            IEnumerable<Metadata> tempValue = new List<Metadata>();
        //            temp = new List<Metadata>();
        //            if (kvp.Value.Count > 0)
        //            {

        //                IEnumerable<Metadata> newValueData = new List<Metadata>();
        //                foreach (string value in kvp.Value)
        //                {
        //                    if (kvp.Key == "") temp = GetAllMetadataDatasetsByValue(data, value);
        //                    else temp = GetAllMetadataDatasetsByNode(data, kvp.Key, value);

        //                    if (tempValue.Count() == 0) tempValue = temp;
        //                    else
        //                    {
        //                        tempValue = tempValue.Union(temp);
        //                    }
        //                }

        //                if (first == false)
        //                {
        //                    newData = newData.Intersect(tempValue);
        //                }
        //                else 
        //                {
        //                    newData = tempValue;
        //                    first = false;
        //                }
        //            }
        //            else
        //            {
        //                if (first == true)
        //                {
        //                    newData = data;
        //                    first = false;
        //                }
        //                else newData = newData.Intersect(data);
        //            }

                    

        //        }


        //        return newData.ToList();
        //    }
        //    else return data;
        //}

        // filter list by Value
        public static List<Metadata> GetAllMetadataDatasetsWithListOfSearchCriteria(List<Metadata> data, SearchCriteria searchCriteria)
        {
            IEnumerable<Metadata> newData = new List<Metadata>();
            List<Metadata> temp = new List<Metadata>();



            if (searchCriteria.SearchCriteriaList.Count() > 0)
            {
                bool first = true;

                foreach (SearchCriterion sco in searchCriteria.SearchCriteriaList)
                {
                    IEnumerable<Metadata> tempValue = new List<Metadata>();
                    temp = new List<Metadata>();

                    if (sco.Values.Count > 0)
                    {

                        IEnumerable<Metadata> newValueData = new List<Metadata>();
                        foreach (string value in sco.Values)
                        {
                            //newValueData = new List<Metadata>();

                            if (sco.SearchComponent.Name == "all") temp = GetAllMetadataDatasetsByValue(data, value);
                            else
                            {
                                if (sco.SearchComponent.Type == SearchComponentBaseType.Property)
                                {
                                    Property sc = (Property)sco.SearchComponent;
                                    if (sc.UIComponent == Property.UI_RANGE && sc.DataType == DataHelperConstClass.PROPERTY_DATATYPE_DATE)
                                    {
                                        bool desc = false;
                                        if (sc.DataSourceKey == "endDate")
                                            desc = true;

                                        temp = GetAllMetadataDatasetsByRangeNode(data, sco.SearchComponent.Name, value, desc);
                                    }
                                    else
                                    {
                                        temp = GetAllMetadataDatasetsByNode(data, sco.SearchComponent.Name, value);
                                    }
                                }
                                else
                                {
                                        temp = GetAllMetadataDatasetsByNode(data, sco.SearchComponent.Name, value);
                                }
                            }

                            if (tempValue.Count() == 0) tempValue = temp;
                            else
                            {

                                tempValue = tempValue.Union(temp);
                            }

                        }

                        if (first == false)
                        {
                            newData = newData.Intersect(tempValue);
                        }
                        else
                        {
                            newData = tempValue;
                            first = false;
                        }
                    }
                    else
                    {
                        if (first == true)
                        {
                            newData = data;
                            first = false;
                        }
                        else newData = newData.Intersect(data);
                    }
                }

                return newData.ToList();
            }
            else return data;
        }

        // filter list by Value
        public static Metadata GetMetadataDatasetByID(List<Metadata> data, string id)
        {
            foreach (Metadata m in data)
            {
                if (m.IsValueInMetadata(id)) return m;
            }

            return null;

        }

        //  ---------------------------------------
        ///Functiom for Category and properties 
        //---------------------------------------
        public static IEnumerable<string> GetAllValuesByNode(string nodeName, List<Metadata> metadataList){
        
             List<string> l = new List<string>();   

             foreach(Metadata m in metadataList )
             {
                 foreach (XmlNode x in m.GetValueFromNode(nodeName))
                 {
                     /*bool isIn = false;
                     foreach (string s in l)
                     {
                         if (x.InnerText == s) isIn = true;
                     }

                     if(isIn == false)l.Add(x.InnerText);*/
                     l.Add(x.InnerText);
                 }
             }

            return l;
        }
     
        public static IEnumerable<TextValue> GetAllTextValuesByNodeDistinct(string nodeName, List<Metadata> metadataList)
        {

            List<TextValue> l = new List<TextValue>();

            foreach (Metadata m in metadataList)
            {
                foreach (XmlNode x in m.GetValueFromNode(nodeName))
                {
                    bool isIn = false;
                    foreach (TextValue s in l)
                    {
                        if (x.InnerText == s.Value) isIn = true;
                    }

                    if(isIn == false){
                        TextValue tv = new TextValue();
                        tv.Name = x.InnerText;
                        tv.Value=x.InnerText;
                        l.Add(tv);
                    };
                    //l.Add(x.InnerText);
                }
            }

            return l;
        }

        public static IEnumerable<string> GetAllValuesByNodeDistinct(string nodeName, List<Metadata> metadataList)
        {

            List<string> l = new List<string>();

            foreach (Metadata m in metadataList)
            {
                foreach (XmlNode x in m.GetValueFromNode(nodeName))
                {
                    bool isIn = false;
                    foreach (string s in l)
                    {
                        if (x.InnerText == s) isIn = true;
                    }

                    if (isIn == false)
                    {
                        
                        l.Add(x.InnerText);
                    };
                    //l.Add(x.InnerText);
                }
            }

            return l;
        }

        public static List<Facet> GetAllCategoriesByNodeDistinct(Facet facet, List<Metadata> metadataList)
        {

            List<Facet> l = new List<Facet>();

            foreach (Metadata m in metadataList)
            {
                foreach (XmlNode x in m.GetValueFromNode(facet.Name))
                {
                    bool isIn = false;
                    foreach (Facet s in l)
                    {
                        if (x.InnerText == s.Name) 
                        {
                            s.Count++;
                            s.Text = s.Name;
                            isIn = true;
                        }
                    }

                    if (isIn == false)
                    {
                        Facet c = new Facet();
                        c.Parent = facet;
                        c.Name = x.InnerText;
                        c.Count = 1;
                        c.Text = c.Name;
                        l.Add(c);
                    }
                }
            }

            return l;
        }

        public static IEnumerable<TextValue> GetAllValuesAsListDistinct(List<Metadata> metadataList)
        {

            List<TextValue> l = new List<TextValue>();

            // gehe alle Metadaten druch
            foreach (Metadata m in metadataList)
            {
                // gehe alle Values in Metadata durch
                foreach (string s in (m.GetAllValuesAsList()))
                {

                    //split strings to single words
                    Array a = s.Split(new Char[] { ' ' });
                    if (a.Length > 1)
                    {
                        foreach (string astring in a)
                        {
                            bool isIn = false;
                            // prüfe ob string schon in List
                            foreach (TextValue s2 in l)
                            {
                                if (astring == s2.Value) isIn = true;
                            }

                            if (isIn == false) {
                                //jedes wort wird eingefügt
                                TextValue tv = new TextValue();
                                tv.Name = astring;
                                tv.Value = astring;
                                l.Add(tv);
                               
                            };
                            //kompletter string wird eingefügt
                      
                        }


                        
                    }
                   
                       bool isInList = false;
                       // prüfe ob string schon in List
                       foreach (TextValue s2 in l)
                       {
                           if (s == s2.Value) isInList = true;
                       }

                       if (isInList == false) {
                           TextValue tv = new TextValue();
                           tv.Name = s;
                           tv.Value = s;
                           l.Add(tv); 
                       
                       }
                    
                }
            }

            return l;
        }
    }
}