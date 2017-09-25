using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class UsersController : BaseController
    {
        [HttpPost]
        public void AddUserToGroup(long userId, string groupName)
        {
            var userManager = new UserManager();

            try
            {
                userManager.AddToGroupAsync(userId, groupName);
            }
            finally
            {
                userManager.Dispose();
            }
        }

        public ActionResult Create()
        {
            return PartialView("_Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateUserModel model)
        {
            var userManager = new UserManager();

            try
            {
                if (!ModelState.IsValid) return PartialView("_Create", model);

                var user = new User { UserName = model.UserName, Email = model.Email };

                var result = await userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    var code = await userManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { area = "", userId = user.Id, code },
                        Request.Url.Scheme);
                    await
                        userManager.SendEmailAsync(user.Id, "Set your password!",
                            "Please set your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return Json(new { success = true });
                }

                AddErrors(result);

                return PartialView("_Create", model);
            }
            finally
            {
                userManager.Dispose();
            }
        }

        [HttpPost]
        public void Delete(long userId)
        {
            var userManager = new UserManager();

            try
            {
                var user = userManager.FindByIdAsync(userId).Result;
                userManager.DeleteAsync(user);
            }
            finally
            {
                userManager.Dispose();
            }
        }

        public ActionResult Groups(long userId)
        {
            return PartialView("_Groups", userId);
        }

        [GridAction]
        public ActionResult Groups_Select(long userId)
        {
            var groupManager = new GroupManager();

            try
            {
                var groups = new List<GroupMembershipGridRowModel>();

                foreach (var group in groupManager.Groups)
                {
                    groups.Add(GroupMembershipGridRowModel.Convert(group, userId));
                }

                return View(new GridModel<GroupMembershipGridRowModel> { Data = groups });
            }
            finally
            {
                groupManager.Dispose();
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public void RemoveUserFromGroup(long userId, string groupName)
        {
            var userManager = new UserManager();

            try
            {
                userManager.RemoveFromGroupAsync(userId, groupName);
            }
            finally
            {
                userManager.Dispose();
            }
        }

        public ActionResult Update(long userId)
        {
            var userManager = new UserManager();

            try
            {
                var user = userManager.FindByIdAsync(userId).Result;
                return PartialView("_Update", UpdateUserModel.Convert(user));
            }
            finally
            {
                userManager.Dispose();
            }
        }

        [HttpPost]
        public ActionResult Update(UpdateUserModel model)
        {
            var userManager = new UserManager();

            try
            {
                if (!ModelState.IsValid) return PartialView("_Update", model);

                var user = userManager.FindByIdAsync(model.Id).Result;

                if (user == null) return PartialView("_Update", model);

                user.Email = model.Email;

                userManager.UpdateAsync(user);
                return Json(new { success = true });
            }
            finally
            {
                userManager.Dispose();
            }
        }

        [GridAction]
        public ActionResult Users_Select()
        {
            var userManager = new UserManager();

            try
            {
                var users = userManager.Users.Select(UserGridRowModel.Convert).ToList();

                return View(new GridModel<UserGridRowModel> { Data = users });
            }
            finally
            {
                userManager.Dispose();
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}