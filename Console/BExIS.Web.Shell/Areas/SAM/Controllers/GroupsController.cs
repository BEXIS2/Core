using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class GroupsController : Controller
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupName"></param>
        [HttpPost]
        public void AddUserToGroup(long userId, string groupName)
        {
            var userManager = new UserManager();

            try
            {
                var user = userManager.FindByIdAsync(userId).Result;
                userManager.AddToGroupAsync(user.Id, groupName);
            }
            finally
            {
                userManager.Dispose();
            }
        }

        /// <summary>
        /// ToDo: Documentation
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView("_Create");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Create(CreateGroupModel model)
        {
            var groupManager = new GroupManager();

            try
            {
                if (!ModelState.IsValid) return PartialView("_Create", model);

                var group = new Group()
                {
                    Name = model.Name,
                    Description = model.Description
                };

                var result = await groupManager.CreateAsync(group);
                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }

                AddErrors(result);

                return PartialView("_Create", model);
            }
            finally
            {
                groupManager.Dispose();
            }
        }

        [GridAction]
        public ActionResult Groups_Select()
        {
            var groupManager = new GroupManager();

            try
            {
                var groups = groupManager.Groups.Select(GroupGridRowModel.Convert).ToList();

                return View(new GridModel<GroupGridRowModel> { Data = groups });
            }
            finally
            {
                groupManager.Dispose();
            }
        }

        /// <summary>
        /// ToDo: Documentation
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// ToDo: Documentation
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupName"></param>
        [HttpPost]
        public void RemoveUserFromGroup(long userId, string groupName)
        {
            var userManager = new UserManager();

            try
            {
                var user = userManager.FindByIdAsync(userId).Result;
                userManager.RemoveFromGroupAsync(user.Id, groupName);
            }
            finally
            {
                userManager.Dispose();
            }
        }

        /// <summary>
        /// ToDo: Documentation
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public ActionResult Update(long groupId)
        {
            var groupManager = new GroupManager();

            try
            {
                var group = groupManager.FindByIdAsync(groupId).Result;
                return View("_Update", UpdateGroupModel.Convert(group));
            }
            finally
            {
                groupManager.Dispose();
            }
        }

        /// <summary>
        /// ToDo: Documentation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Update(UpdateGroupModel model)
        {
            var groupManager = new GroupManager();

            try
            {
                if (!ModelState.IsValid) return PartialView("_Update", model);

                var group = groupManager.FindByIdAsync(model.Id).Result;

                if (group == null) return PartialView("_Update", model);

                group.Name = model.Name;
                group.Description = model.Description;

                groupManager.UpdateAsync(group);
                return Json(new { success = true });
            }
            finally
            {
                groupManager.Dispose();
            }
        }

        /// <summary>
        /// ToDo: Documentation
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public ActionResult Users(string groupName)
        {
            return PartialView("_Users", groupName);
        }

        [GridAction]
        public ActionResult Users_Select(string groupName = "")
        {
            var userManager = new UserManager();

            try
            {
                var users = new List<UserMembershipGridRowModel>();

                foreach (var user in userManager.Users)
                {
                    users.Add(UserMembershipGridRowModel.Convert(user, groupName));
                }

                return View(new GridModel<UserMembershipGridRowModel> { Data = users });
            }
            finally
            {
                userManager.Dispose();
            }
        }

        #region Hilfsprogramme

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        #endregion Hilfsprogramme
    }
}