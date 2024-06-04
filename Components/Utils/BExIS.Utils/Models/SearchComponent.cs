using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///
/// </summary>
namespace BExIS.Utils.Models
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class SearchComponent
    {
        /// <summary>
        /// represents the search components of the search UI
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>
        public SearchComponent()
        {
            this.Categories = new List<Category>();
            this.Properties = new List<Property>();
            this.Facets = new List<Facet>();
            this.TextBoxSearchValues = new List<TextValue>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsSearchComponent(string name, string value)
        {
            if (this.ContainsFacet(name)) return true;
            if (this.ContainsProperty(name)) return true;
            if (this.ContainsCategory(name)) return true;
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <param name="typeOf"></param>
        /// <returns></returns>
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
                case SearchComponentBaseType.General:
                    {
                        if (this.ContainsGeneral(name)) return this.GetGeneral(name);
                        break;
                    }
            }
            return null;
        }

        #region Facets

        /// <summary>
        /// a list of (hierarchical) facets
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public IEnumerable<Facet> Facets { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="facetName"></param>
        /// <returns></returns>
        public bool ContainsFacet(string facetName)
        {
            return Facets.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).Count() > 0;
        }

        /// <summary>
        /// Get a facet from Facets where facename and parentname equal
        /// to facets child
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="facetName"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public Facet GetFacet(string facetName)
        {
            if (Facets.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() != null)
                return Facets.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            else
            {
                foreach (Facet facet in Facets)
                {
                    if (facet.Childrens.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() != null)
                        return facet.Childrens.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                }
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="facetName"></param>
        /// <returns></returns>
        public IEnumerable<Facet> GetFacetChildrens(string facetName)
        {
            return Facets.Where(p => p.Name.Equals(facetName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault().Childrens;
        }

        #endregion Facets

        #region General

        public IEnumerable<General> Generals { get; set; }

        public bool ContainsGeneral(string name)
        {
            return Generals.Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() > 0;
        }

        public General GetGeneral(string name)
        {
            return (Generals.Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault());
        }

        #endregion General

        #region Category

        // -------------------------------------------------------------------
        // Catgegory

        /// <summary>
        /// usable for category-list in dropdown
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public IEnumerable<Category> Categories { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsCategory(string name)
        {
            return Categories.Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() > 0;
        }

        /// <summary>
        /// Get a category from Categories where value equal
        /// to facets category
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="facetName"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public Category GetCategory(string name)
        {
            return (Categories.Where(p => p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault());
        }

        #endregion Category

        #region TextValues

        //-------------------------------------------------------------------

        // textSearchValues

        // not sure how to represent it
        //  - now we'll got a list with all values in type string, but I assume there a too many values to  be handled smoothly by application
        //  - maybe better to make a direct connection to lucene ? - what would be the data type ?

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public IEnumerable<TextValue> TextBoxSearchValues { get; set; }

        #endregion TextValues

        #region Properties

        //-------------------------------------------------------------------
        // Properties

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public IEnumerable<Property> Properties { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsProperty(string name)
        {
            return Properties.Where(p => p.DataSourceKey.Equals(name, StringComparison.InvariantCultureIgnoreCase)).Count() > 0;
        }

        /// <summary>
        /// Get a proptery from Properties where value and DataSourceKey are equal
        /// otherwise null
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="facetName"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public Property GetProperty(string name)
        {
            return (Properties.Where(p => p.DataSourceKey.Equals(name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault());
        }

        #endregion Properties
    }
}