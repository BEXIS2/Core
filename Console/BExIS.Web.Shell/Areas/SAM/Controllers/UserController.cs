using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Subjects;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        public ActionResult Users_Select(GridActionAttribute filters)
        {
            var userManager = new UserManager(new UserStore());
            var users = userManager.Users.Select(u => UserGridRowModel.Convert(u)).ToList();

            return View(new GridModel<UserGridRowModel> { Data = users });
        }
    }
}