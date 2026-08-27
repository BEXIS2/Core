using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Smm.UI.Helpers
{
    public class ConversionHelper
    {
        public static HashSet<long> ConvertStringListToLongHashSet(IEnumerable<string> stringList)
        {
            var longHashSet = new HashSet<long>();

            if (stringList == null) return longHashSet;

            foreach (var str in stringList)
            {
                if (string.IsNullOrWhiteSpace(str)) continue;
                if (long.TryParse(str, out long result))
                {
                    longHashSet.Add(result);
                }
                else
                {
                    // TODO:
                    // Handle the case where parsing fails, e.g., log a warning or throw an exception
                    // For this example, we will simply ignore invalid entries
                }
            }
            return longHashSet;
        }
    }
}