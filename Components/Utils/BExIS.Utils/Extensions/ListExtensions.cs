using BExIS.Security.Entities.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Utils.Extensions
{
    public static class ListExtensions
    {
        public static int ToInt(this List<RightType> rights)
        {
            return rights.Count != rights.Distinct().Count() ? 0 : rights.Select(r => Math.Pow(2, (int)r)).ToList().Sum(Convert.ToInt32);
        }

        public static bool[] ToBoolArray(this List<RightType> rights)
        {
            var array = new bool[Enum.GetValues(typeof(RightType)).Length];

            foreach (var rightType in Enum.GetValues(typeof(RightType)).Cast<RightType>())
            {
                array[(int)rightType] = rights.Contains(rightType);
            }

            return array;
        }
    }
}
