using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Vaiona.Web.Security.Az
{
    public interface IAuthorizationService
    {
        bool IsAuthorized(AuthorizationContext filterContext, string accessRule, string actionKey, string parentActionKey);
        bool IsAuthorized(AuthorizationContext filterContext, object accessRule, string actionKey, string parentActionKey);
    }
}
