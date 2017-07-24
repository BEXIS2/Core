using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class UsersController : Controller
    {
        public ActionResult Create()
        {
            return PartialView("_Create");
        }

        [HttpPost]
        public ActionResult Create(CreateUserModel model)
        {
            if (ModelState.IsValid)
            {
                var userManager = new UserManager(new UserStore());
                userManager.CreateAsync(new User()
                {
                    Email = model.Email,
                    UserName = model.UserName,
                    IsAdministrator = model.IsAdministrator
                });

                RedirectToAction("Index");
            }

            return PartialView("_Create", model);
        }

        [HttpPost]
        public void Delete(long userId)
        {
            var userStore = new UserStore();
            userStore.Delete(userId);
        }

        public void Groups_Save(UserMembershipGridRowModel[] groups)
        {
            Session["SelectedGroups"] = groups.Where(g => g.IsUserInGroup).Select(g => g.Id).ToList();
        }

        [GridAction]
        public ActionResult Groups_Select(long userId)
        {
            var userStore = new UserStore();
            var user = userStore.FindById(userId);

            List<long> selectedGroups = new List<long>();
            if (Session["SelectedGroups"] == null)
            {
                selectedGroups.AddRange(user.Groups.Select(g => g.Id).ToList());
            }
            else
            {
                selectedGroups.AddRange(Session["SelectedGroups"] as List<long>);
            }

            var groupManager = new GroupManager();
            var groups = groupManager.Groups.Select(g => GroupMembershipGridRowModel.Convert(g, selectedGroups)).ToList();

            return View(new GridModel<GroupMembershipGridRowModel> { Data = groups });
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Update(long userId)
        {
            var userStore = new UserStore();
            var user = userStore.FindById(userId);

            return PartialView("_Update", UpdateUserModel.Convert(user));
        }

        [HttpPost]
        public ActionResult Update(UpdateUserModel model)
        {
            return PartialView("_Update", model);
        }

        [GridAction]
        public ActionResult Users_Select()
        {
            var userManager = new UserManager(new UserStore());
            var users = userManager.Users.Select(UserGridRowModel.Convert).ToList();

            return View(new GridModel<UserGridRowModel> { Data = users });
        }
    }
}