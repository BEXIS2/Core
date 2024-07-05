using BExIS.Utils.Models;
using System.Collections.Generic;

/// <summary>
///
/// </summary>
namespace BExIS.Ddm.Providers.LuceneProvider.Config
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    internal class searchInitObjects
    {
        public List<Facet> AllFacets = new List<Facet>();
        public List<Property> AllProperties = new List<Property>();
        public List<Category> AllCategories = new List<Category>();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public searchInitObjects()
        {
        }
    }
}