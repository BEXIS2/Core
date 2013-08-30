using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Search.Model
{
    public class SearchCriteria 
    {

        public SearchCriteria() 
        {
            SearchCriteriaList = new List<SearchCriterion>();
        }

        public List<SearchCriterion> SearchCriteriaList { get; set; }

       /// <summary>
       /// Update a SearchCriterion with one value
       /// </summary>
       /// <param name="name"></param>
       /// <param name="value"></param>
       /// <param name="multiSelect"></param>
       /// <param name="range"></param>
       /// <param name="dataType"></param>
        public void Update(SearchComponentBase scb, string value, bool multiSelect = false, string valueSearchOperation = "OR")
        {
    
            //1. prüfe ob name in list vorhanden
            if (ContainsSearchCriterion(scb.Name, scb.Type))
            {
                SearchCriterion sco = Get(scb.Name, scb.Type);

                //2. check if MultiSelect
                if (multiSelect)
                {
                    // diffferent values can be in  the list
                    // if the values is in the list remove the value
                    if (sco.Values.Contains(value))
                    {
                        sco.Values.Remove(value);
                        if (sco.Values.Count() == 0) SearchCriteriaList.Remove(sco);
                    }
                    // else add value to the list
                    else
                    {
                        sco.Values.Add(value);
                    } 
                }
                else
                {
                    // override values of SearchCriterion if not multiselect
                    // or remove if values contains this value
                    if (sco.Values.Contains(value)) sco.Values.Clear();
                    else
                    {
                        sco.Values.Clear();

                        bool isDefault = false;
                        if (scb is Property)
                        {
                            Property p = (Property)scb;
                            if (p.DefaultValue == value) isDefault = true;
                        }

                        if (!isDefault)
                        {
                            sco.Values.Clear();
                            sco.Values.Add(value);
                        }
                        else SearchCriteriaList.Remove(sco);
                    }
                }
            }
            else
            {

                //wenn nicht vorhanden dann füge hinzu
                if (scb != null)
                {
                    SearchCriteriaList.Add(new SearchCriterion(value, multiSelect, valueSearchOperation, scb));
                }
            }
   
        }

        /// <summary>
        /// Update a SearchCriterion with a list of values
        /// multiselect must be true
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        /// <param name="multiSelect"></param>
        /// <param name="range"></param>
        /// <param name="dataType"></param>
        public void Update(SearchComponentBase scb,List<string> values, bool multiSelect=true, string valueSearchOperation = "OR")
        {
            //1. prüfe ob name in list vorhanden
            if (ContainsSearchCriterion(scb.Name, scb.Type))
            {
                SearchCriterion sco = Get(scb.Name, scb.Type);
                //2. check if MultiSelect
                if (multiSelect)
                {
                    if (values.Count > 0) sco.Values = values;
                    else SearchCriteriaList.Remove(sco);
                }
            }
            else
            {
                //wenn nicht vorhanden dann füge hinzu
                if (values.Count>0)
                SearchCriteriaList.Add(new SearchCriterion(values, multiSelect, valueSearchOperation,scb));
            }
        }

        public void Clear()
        {
            SearchCriteriaList = new List<SearchCriterion>();
        }

        public void RemoveSearchCriteria(string name, bool multiselect)
        {
            if (ContainsSearchCriterion(name, multiselect))
                SearchCriteriaList.Remove(Get(name, multiselect));
        }

        public void RemoveValueOfSearchCriteria(string name, string value)
        {
            if (ContainsSearchCriterion(name, value))
            {
                SearchCriterion sco = Get(name, value);
                if (sco != null)
                {
                    sco.Values.Remove(value);
                    if (sco.Values.Count() == 0) SearchCriteriaList.Remove(sco);
                }
            }
        }

        ///// <summary>
        ///// Get a SearchCriterion from SerachCriteriaList
        ///// </summary>
        ///// <param name="name">Name of the SearchCriterion</param>
        ///// <returns>SearchCriterion or null</returns>
        //private SearchCriterion Get(string name) {

        //    return SearchCriteriaList.Where(p => p.SearchComponent.Name.Equals(name, StringComparison.InvariantCulture)).FirstOrDefault();
        //}

        /// <summary>
        /// Get a SearchCriterion from SerachCriteriaList
        /// </summary>
        /// <param name="name">Name of the SearchCriterion</param>
        /// <returns>SearchCriterion or null</returns>
        private SearchCriterion Get(string name, string value)
        {
            var searchcriterion = (from sco in SearchCriteriaList
                                  from va in sco.Values
                                  where (sco.SearchComponent.Name.Equals(name, StringComparison.InvariantCulture)
                                  && (va.Equals(value, StringComparison.InvariantCulture)))
                                  select sco).FirstOrDefault();

            return searchcriterion as SearchCriterion;
        }

        private SearchCriterion Get(string name, bool multiselect)
        {
            return SearchCriteriaList.Where(p => p.SearchComponent.Name.Equals(name, StringComparison.InvariantCulture) && p.MultiSelect.Equals(multiselect)).FirstOrDefault();
        }

        private SearchCriterion Get(string name, SearchComponentBaseType type)
        {
            return SearchCriteriaList.Where(p => p.SearchComponent.Name.Equals(name, StringComparison.InvariantCulture) && p.SearchComponent.Type.Equals(type)).FirstOrDefault();
        }

        /// <summary>
        /// Check if Property in SerachCriteriaList
        /// This function is used to check whether an property is present in the list.
        /// </summary>
        /// <param name="facet"></param>
        /// <returns></returns>
        public Property GetProperty(string Name)
        {
            var sc = (from sco in SearchCriteriaList
                            from v in sco.Values
                            where v.Equals(Name, StringComparison.InvariantCulture) ||
                            sco.SearchComponent.Name.Equals(Name, StringComparison.InvariantCulture)
                            select sco).FirstOrDefault();

            if (sc != null) return sc.SearchComponent as Property;

            return null;
        }


       
        #region contains

        private bool ContainsSearchCriterion(string name) {
            return SearchCriteriaList.Where(p => p.SearchComponent.Name.Equals(name, StringComparison.InvariantCulture)).Count() > 0;
        }

        public bool ContainsSearchCriterion(string name, string value)
        {

            var isIn = (from sco in SearchCriteriaList
                                   from va in sco.Values
                                   where (sco.SearchComponent.Name.Equals(name, StringComparison.InvariantCulture)
                                   && (va.Equals(value, StringComparison.InvariantCulture)))
                                   select value).Count()>0;

            return isIn;
        }

        public bool ContainsSearchCriterion(string name, bool multiselect)
        {
           return SearchCriteriaList.Where(p => p.SearchComponent.Name.Equals(name, StringComparison.InvariantCulture) && p.MultiSelect.Equals(multiselect)).Count()>0;
        }

        private bool ContainsSearchCriterion(string name, SearchComponentBaseType type)
        {
            return SearchCriteriaList.Where(p => p.SearchComponent.Name.Equals(name, StringComparison.InvariantCulture) && p.SearchComponent.Type.Equals(type)).Count()>0;
        }

        public bool ContainsSearchCriterion(string name, string value, SearchComponentBaseType type)
        {

            var isIn = (from sco in SearchCriteriaList
                        from va in sco.Values
                        where (sco.SearchComponent.Name.Equals(name, StringComparison.InvariantCulture)
                        && (va.Equals(value, StringComparison.InvariantCulture)) && sco.SearchComponent.Type.Equals(type))
                        select value).Count() > 0;

            return false;
        }

        
       
        /// <summary>
        /// Check if facet in SerachCriteriaList
        /// This function is used to check whether an facet is present in the list.
        /// </summary>
        /// <param name="facet"></param>
        /// <returns></returns>
        public bool ContainsFacet(Facet facet) {

            var isIn = (from sco in SearchCriteriaList
                        from v in sco.Values
                        where (sco.SearchComponent.Name.Equals(facet.Parent.Name) && v.Equals(facet.Name, StringComparison.InvariantCulture)) ||
                        sco.SearchComponent.Name.Equals(facet.Name, StringComparison.InvariantCulture)
                        select sco).Count() > 0;

            return isIn;
        }

        /// <summary>
        /// Check if Property in SerachCriteriaList
        /// This function is used to check whether an property is present in the list.
        /// </summary>
        /// <param name="facet"></param>
        /// <returns></returns>
        public bool ContainsProperty(Property Property)
        {
            var isIn = (from sco in SearchCriteriaList
                        from v in sco.Values
                        where v.Equals(Property.Name, StringComparison.InvariantCulture) ||
                        sco.SearchComponent.Name.Equals(Property.Name, StringComparison.InvariantCulture)
                        select sco).Count() > 0;

            return isIn;
        }

        /// <summary>
        /// Check if aTextValue in SerachCriteriaList
        /// This function is used to check whether an text is present in the list.
        /// </summary>
        /// <param name="facet"></param>
        /// <returns></returns>
        public bool ContainsCategory(Category category)
        {
            throw new NotImplementedException();
        }


        #endregion
    }

    public class SearchCriterion
    {
        public SearchCriterion(string value, bool multiSelect, string valueSearchOperation , SearchComponentBase searchComponent)
        {
            if (Values == null) Values = new List<string>();
            //Name = name;
            Values.Add(value);
            MultiSelect = multiSelect;
            //Range = range;
            ValueSearchOperation = valueSearchOperation;
            SearchComponent = searchComponent;
        }

        public SearchCriterion(List<string> values, bool multiSelect, string valueSearchOperation, SearchComponentBase searchComponent)
        {
            //Name = name;
            Values = values;
            MultiSelect = multiSelect;
            //Range = range;
            ValueSearchOperation = valueSearchOperation;
            SearchComponent = searchComponent;
        }

        //public string Name { get; set; }
        public bool MultiSelect { get; set; }
        //public bool Range { get; set; }


        public List<string> Values{ get; set; }

        // AND/OR Between List<string> objects in sql statmant
        public string ValueSearchOperation { get; set; }
        public SearchComponentBase SearchComponent { get; set; }
    }
}
