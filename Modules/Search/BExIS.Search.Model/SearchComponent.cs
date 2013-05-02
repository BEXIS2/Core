using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Search.Model
{

    public class SearchComponent
    {
        // represents the search components of the search UI


        public SearchComponent()
        {
            this.Categories = new List<Category>();
            this.Properties = new List<Property>();
            this.Facets = new List<Facet>();
            this.TextBoxSearchValues = new List<TextValue>();
        
        }

        public SearchComponentBase GetSearchComponent(string name, SearchComponentBaseType typeOf)
        {
            switch (typeOf)
            {
                case SearchComponentBaseType.Facet:
                    {
                        if (this.ContainsFacet(name)) return this.GetFacet(name);
                        break;
                    }
                case SearchComponentBaseType.Category:
                    {
                        if (this.ContainsCategory(name)) return this.GetCategory(name);
                        break;
                    }
                case SearchComponentBaseType.Property:
                    {
                        if (this.ContainsProperty(name)) return this.GetProperty(name);
                        break;
                    }
            }
            return null;
        }

        public bool ContainsSearchComponent(string name, string value)
        {
            if (this.ContainsFacet(name)) return true;
            if (this.ContainsProperty(name)) return true;
            if (this.ContainsCategory(name)) return true;
            return false;
        }


        #region Facets
        /// <summary>
        /// a list of (hierarchical) facets
        /// </summary>
        public IEnumerable<Facet> Facets { get; set; }

        /// <summary>
        /// Get a facet from Facets where facename and parentname equal 
        /// to facets child
        /// </summary>
        /// <param name="facetName"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public Facet GetFacet(string facetName)
        {
            if(Facets.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() != null)
                return Facets.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            else
            {
                foreach(Facet facet in Facets )
                {
                    if(facet.Childrens.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() != null)
                        return facet.Childrens.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    
                }
            }


            return null;
        }

        public IEnumerable<Facet> GetFacetChildrens(string facetName)
        {
            return Facets.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Childrens;
        }

        public bool ContainsFacet(string facetName)
        {
            return  Facets.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).Count() > 0;
        }
        #endregion

        #region Category
        // -------------------------------------------------------------------
        // Catgegory

        /// <summary>
        /// usable for category-list in dropdown
        /// </summary>
        public IEnumerable<Category> Categories { get; set; }

        /// <summary>
        /// Get a category from Categories where value equal 
        /// to facets category
        /// </summary>
        /// <param name="facetName"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public Category GetCategory(string name)
        {
            return (Categories.Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault());
        }

        public bool ContainsCategory(string name)
        {
            return Categories.Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() > 0;
        }

        #endregion

        #region TextValues
        //-------------------------------------------------------------------

        // textSearchValues

        // not sure how to represent it 
        //  - now we'll got a list with all values in type string, but I assume there a too many values to  be handled smoothly by application
        //  - maybe better to make a direct connection to lucene ? - what would be the data type ?

        public IEnumerable<TextValue> TextBoxSearchValues { get; set; }
     
        #endregion

        #region Properties
        //-------------------------------------------------------------------
        // Properties
        public IEnumerable<Property> Properties { get; set; }

        /// <summary>
        /// Get a proptery from Properties where value and DataSourceKey are equal 
        /// otherwise null
        /// </summary>
        /// <param name="facetName"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public Property GetProperty(string name)
        {
            return (Properties.Where(p => p.DataSourceKey.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault());
        }

        public bool ContainsProperty(string name)
        {
            return Properties.Where(p => p.DataSourceKey.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() > 0;
        }

        #endregion
    }


}
