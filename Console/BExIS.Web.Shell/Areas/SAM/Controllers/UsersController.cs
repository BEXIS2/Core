using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class UsersController : Controller
    {
        [HttpPost]
        public void AddUserToGroup(long userId, long groupId)
        {
            var userStore = new UserStore();
            var user = userStore.FindById(userId);

            userStore.AddToGroupAsync(user, groupId);
        }

        public ActionResult Create()
        {
            return PartialView("_Create");
        }

        [HttpPost]
        public ActionResult Create(CreateUserModel model)
        {
            if (!ModelState.IsValid) return PartialView("_Create", model);

            var userStore = new UserStore();
            userStore.Create(new User()
            {
                Email = model.Email,
                UserName = model.UserName,
                IsAdministrator = model.IsAdministrator
            });

            return Json(new { success = true });
        }

        [HttpPost]
        public void Delete(long userId)
        {
            var userStore = new UserStore();
            userStore.Delete(userId);
        }

        public ActionResult Groups(long userId)
        {
            return PartialView("_Groups", userId);
        }

        [GridAction]
        public ActionResult Groups_Select(long userId = 0)
        {
            var groupManager = new GroupManager();
            var userStore = new UserStore();
            var groups = groupManager.Groups.Select(g => GroupMembershipGridRowModel.Convert(g, userId)).ToList();

            return View(new GridModel<GroupMembershipGridRowModel> { Data = groups });
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public void RemoveUserFromGroup(long userId, long groupId)
        {
            var userStore = new UserStore();
            var user = userStore.FindById(userId);

            userStore.RemoveFromGroupAsync(user, groupId);
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
            if (!ModelState.IsValid) return PartialView("_Update", model);

            var userStore = new UserStore();
            var user = userStore.FindById(model.Id);

            if (user == null) return PartialView("_Update", model);

            user.Email = model.Email;
            user.IsAdministrator = model.IsAdministrator;

            userStore.Update(user);
            return Json(new { success = true });
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