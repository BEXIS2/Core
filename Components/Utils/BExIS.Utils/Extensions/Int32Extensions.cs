using BExIS.Security.Entities.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Utils.Extensions
{
    public static class Int32Extensions
    {
        public static List<RightType> ToRightTypes(this int rights)
        {
            var binary = Convert.ToString(rights, 2);
            return Enum.GetValues(typeof(RightType)).Cast<int>().Where(right => right < binary.Length && binary.ElementAt((binary.Length - 1) - right) == '1').Cast<RightType>().ToList();
        }
    }
}
