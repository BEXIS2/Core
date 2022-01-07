using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Example
{
    public  class NumberHelper
    {
        public bool IsNumber(object i)
        { 
            if(i==null) return false;

            int a = 0;
            if (Int32.TryParse(i.ToString(), out a)) return true;

            long b = 0;
            if (Int64.TryParse(i.ToString(), out b)) return true;
            double c = 0;
            if (Double.TryParse(i.ToString(), out c)) return true;

            decimal d = 0;
            if (Decimal.TryParse(i.ToString(), out d)) return true;


            return false;
        }

    }
}
