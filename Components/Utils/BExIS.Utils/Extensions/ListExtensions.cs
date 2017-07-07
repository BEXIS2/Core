using BExIS.Security.Entities.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Utils.Extensions
{
    public static class ListExtensions
    {
        public static int ToShort(this List<RightType> rights)
        {
            return rights.Count != rights.Distinct().Count() ? 0 : rights.Select(r => Math.Pow(2, (int)r)).ToList().Sum(Convert.ToInt32);
        }
    }
}
