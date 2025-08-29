using BExIS.Security.Entities.Requests;
using System.Web.Helpers;
using System.Web.Mvc;

namespace BExIS.App.Bootstrap.Attributes
{
    public class ValidateAntiForgeryTokenOnPost: FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;

            if (filterContext.HttpContext.Request.HttpMethod == "POST")
            {
                var cookieToken = request.Cookies[AntiForgeryConfig.CookieName]?.Value;

                // check for token in form data
                var formToken = request.Form["__RequestVerificationToken"];

                // check header for post from javascript
                if (formToken==null)
                {
                    formToken = request.Headers["__RequestVerificationToken"];
                }

                AntiForgery.Validate(cookieToken, formToken);
                //AntiForgery.Validate();
            }
        }
    }
}
