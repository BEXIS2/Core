using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Utils.Models;

namespace BExIS.Ddm.Providers.OpenSearchProvider
{
    internal class searchInitObjects
    {
        public List<Facet> AllFacets = new List<Facet>();
        public List<Property> AllProperties = new List<Property>();
        public List<Category> AllCategories = new List<Category>();

        public searchInitObjects()
        {
        }
    }
}
