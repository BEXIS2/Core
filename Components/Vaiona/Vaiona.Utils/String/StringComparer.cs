using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vaiona.Utils.String
{
    public class CultureInvariantStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase);            
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }       

}
