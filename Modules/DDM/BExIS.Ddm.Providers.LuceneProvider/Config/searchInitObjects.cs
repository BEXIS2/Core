using System.Collections.Generic;

using BExIS.Ddm.Model;

/// <summary>
///
/// </summary>        
namespace BExIS.Ddm.Providers.LuceneProvider.Config
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    class searchInitObjects 
    {
        public  List<Facet> AllFacets = new List<Facet>();
        public  List<Property> AllProperties = new List<Property>();
        public  List<Category> AllCategories = new List<Category>();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public searchInitObjects() { 
        
        }
    }
}
