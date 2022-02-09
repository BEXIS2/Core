using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using System.Web;

namespace System.Web
{
    public static class ApplicationExtenssions
    {
        public static MemoryCache GetAccessRuleCache(this HttpApplicationStateBase app)
        {
            MemoryCache actionCache = null;
            actionCache = app["AccessRuleCache"] as MemoryCache;
            if (actionCache == null)
            {
                actionCache = new MemoryCache("AccessRuleCache");
                app["AccessRuleCache"] = actionCache;
            }
            return (actionCache);
        }

        public static void ClearAccessRuleCache(this HttpApplicationStateBase app)
        {
            app["AccessRuleCache"] = null;
        }     
    }
}
