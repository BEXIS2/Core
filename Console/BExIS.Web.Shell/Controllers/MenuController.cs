using BExIS.App.Bootstrap.Helpers;
using BExIS.Security.Entities.Subjects;
using BExIS.UI.Models;
using BExIS.Web.Shell.Helpers;
using System;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Controllers
{
    public class MenuController : Controller
    {
        // GET: Menu
        public JsonResult Index()
        {
            string userName = "";
            bool isAuthenticated = false;

            if (HttpContext.User != null && !string.IsNullOrEmpty(HttpContext.User.Identity.Name))
            {
                userName = HttpContext.User.Identity.Name;
                isAuthenticated = true;
            }
            else
            {
                var authorization = HttpContext.Request.Headers.Get("Authorization");
                User user = null;
                var res = BExISAuthorizeHelper.GetUserFromAuthorization(authorization, out user);
                if (user != null)
                {
                    userName = user.Name;
                    isAuthenticated = true;
                }
            }


            if (Session["Menu"] != null)
            {
                return Json((Menu)Session["Menu"], JsonRequestBehavior.AllowGet);
            }


            Menu menu = new Menu();
            // load logo
            if (Session.GetTenant() != null && !string.IsNullOrEmpty(Session.GetTenant().Brand))
            {
                string name = Session.GetTenant().ShortName;
                string mime = MimeMapping.GetMimeMapping(Session.GetTenant().Brand);
                byte[] image = System.IO.File.ReadAllBytes(Session.GetTenant().BrandPath);
                string data = Convert.ToBase64String(image);
                menu.Logo = new Logo(name, mime, data, 40);
            }

            //load bars
            menu.LaunchBar = MenuHelper.MenuBar("lunchbarRoot");
            menu.MenuBar = MenuHelper.MenuBarSecured("menubarRoot", userName);
            menu.Settings = MenuHelper.MenuBarSecured("settingsRoot", userName, true);

            menu.AccountBar = MenuHelper.AccountBar(isAuthenticated, userName);

            if (Session.GetTenant().ExtendedMenus != null)
                menu.Extended = MenuHelper.ExtendedMenu(Session.GetTenant().ExtendedMenus.Element("ExtendedMenu"));

            // set menu to session if user is authenticated
            if(!string.IsNullOrEmpty(userName) && isAuthenticated) Session["Menu"] = menu;

            return Json(menu, JsonRequestBehavior.AllowGet);
        }
    }
}