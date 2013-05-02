using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BExIS.Search.Model;

namespace BExIS.Search.Providers.LuceneProvider.Config
{

    class searchInitObjects 
    {
        public  List<Facet> AllFacets = new List<Facet>();
        public  List<Property> AllProperties = new List<Property>();
        public  List<Category> AllCategories = new List<Category>();

        public searchInitObjects() { 
        
        }
    }
}
