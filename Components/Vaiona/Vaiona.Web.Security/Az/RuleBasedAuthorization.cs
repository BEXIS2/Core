using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Web.Security.Az.Parser;
using System.Web.Mvc;
using System.Runtime.Caching;
using System.Configuration;
using System.Web;
using System.Web.SessionState;

namespace Vaiona.Web.Security.Az
{
    class RuleBasedAuthorization: IAuthorizationService
    {
        //private static MemoryCache actionCache = null;
        static RuleBasedAuthorization()
        {
            //actionCache = new MemoryCache("ActionCache");
        }

        public bool IsAuthorized(AuthorizationContext filterContext, string accessRule, string actionKey, string parentActionKey)
        {
            bool? result = null;
            try
            {
                RoleParser parser = new RoleParser();
                IAccessRule accessRuleObj = parser.Parse(accessRule);
                result = accessRuleObj.Evaluate(
                    role => filterContext.HttpContext.User.IsInRole(role)
                    , userName => filterContext.HttpContext.User.Identity.Name.ToLowerInvariant() == userName.ToLowerInvariant()
                    );
            }
            catch
            {
                result = false;
            }
            return (result.HasValue? result.Value: false);
        }


        public bool IsAuthorized(AuthorizationContext filterContext, object accessRule, string actionKey, string parentActionKey)
        {
            throw new NotImplementedException();
        }
    }
}
