using BExIS.App.Bootstrap.Extensions;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Models;
using BExIS.Web.Shell.Helpers;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Controllers
{
    public class MenuController : Controller
    {
        private readonly UserManager _userManager;

        public MenuController(UserManager userManager)
        {
            _userManager = userManager;
        }
        // GET: Menu
        public JsonResult Index()
        {
            string userName = "";
            string fullName = "";
            bool isAuthenticated = false;

            if (HttpContext.User != null && !string.IsNullOrEmpty(HttpContext.User.Identity.Name))
            {
                var user = _userManager.FindByNameAsync(HttpContext.User.Identity.Name).Result;

                userName = user.UserName;
                fullName = user.FullName ?? user.UserName;
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
                    fullName = user.FullName ?? user.UserName;
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
            MenuHelper.AdditionalHelpBar(menu.LaunchBar.FirstOrDefault(i =>i.Title.Equals("Help")));


            menu.AccountBar = MenuHelper.AccountBar(isAuthenticated, fullName);

            if (Session.GetTenant().ExtendedMenus != null)
                menu.Extended = MenuHelper.ExtendedMenu(Session.GetTenant().ExtendedMenus.Element("ExtendedMenu"));

            // set menu to session if user is authenticated
            if(!string.IsNullOrEmpty(userName) && isAuthenticated) Session["Menu"] = menu;

            return Json(menu, JsonRequestBehavior.AllowGet);
        }
    }
}