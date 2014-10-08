using System;
using System.Reflection;
using System.Web.Mvc;
using BExIS.Security.Providers.Authentication;

namespace BExIS.Web.Shell.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //Assembly a = Assembly.Load("BExIS.Security.Providers");

            //Type myType = a.GetType("BExIS.Security.Providers.Authentication.LdapAuthenticationProvider");

            //IAuthenticationProvider s = (IAuthenticationProvider)Activator.CreateInstance(myType, "ldapHost=dsfds;ldapPort=78");

            return View();
        }
    }
}
