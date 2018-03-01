using BExIS.Security.Entities.Authorization;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Utils.Extensions
{
    public static class ListExtensions
    {
        public static int ToInt(this List<RightType> rights)
        {
            return rights.Aggregate(0, (current, right) => current | (int)right);
        }
    }
}