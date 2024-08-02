using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.Utils.NH.Querying;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class GroupsController : BaseController
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupName"></param>
        [HttpPost]
        public async Task<bool> AddUserToGroup(long userId, string groupName)
        {
            var identityUserService = new IdentityUserService();

            try
            {
                var user = identityUserService.FindByIdAsync(userId).Result;
                var result = await identityUserService.AddToRoleAsync(user.Id, groupName);
                return result.Succeeded;
            }
            finally
            {
                identityUserService.Dispose();
            }
        }

        /// <summary>
        /// ToDo: Documentation
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView("_Create", new CreateGroupModel());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Create(CreateGroupModel model)
        {
            using (var identityGroupService = new IdentityGroupService())
            {
                if (!ModelState.IsValid) return PartialView("_Create", model);

                var group = new Group()
                {
                    Name = model.Name,
                    DisplayName = model.Name,
                    Description = model.Description
                };

                var result = await identityGroupService.CreateAsync(group);
                if (result.Succeeded)
                {
                    return Json(new { success = true });
                }

                AddErrors(result);

                return PartialView("_Create", model);
            }
        }

        [HttpPost]
        public async Task<bool> Delete(long groupId)
        {
            using (var identityGroupService = new IdentityGroupService())
            {
                var group = identityGroupService.FindByIdAsync(groupId).Result;

                foreach (var user in group.Users)
                {
                    await RemoveUserFromGroup(user.Id, @group.Name);
                }

                var result = await identityGroupService.DeleteAsync(group);
                return result.Succeeded;
            }
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Groups_Select(GridCommand command)
        {
            var groupManager = new GroupManager();

            try
            {
                var groups = new List<GroupGridRowModel>();
                int count = groupManager.Groups.Count();
                if (command != null)// filter subjects based on grid filter settings
                {
                    FilterExpression filter = TelerikGridHelper.Convert(command.FilterDescriptors.ToList());
                    OrderByExpression orderBy = TelerikGridHelper.Convert(command.SortDescriptors.ToList());

                    groups = groupManager.GetGroups(filter, orderBy, command.Page, command.PageSize, out count).Select(GroupGridRowModel.Convert).ToList();
                }
                else
                {
                    groups = groupManager.Groups.Select(GroupGridRowModel.Convert).ToList();
                    count = groupManager.Groups.Count();
                }

                return View(new GridModel<GroupGridRowModel> { Data = groups, Total = count });
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
            string module = "SAM";

            //ViewData["app"] = SvelteHelper.GetApp(module);
            //ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        /// <summary>
        /// ToDo: Documentation
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupName"></param>
        [HttpPost]
        public async Task<bool> RemoveUserFromGroup(long userId, string groupName)
        {
            var identityUserService = new IdentityUserService();

            try
            {
                var user = identityUserService.FindByIdAsync(userId).Result;
                var result = await identityUserService.RemoveFromRoleAsync(user.Id, groupName);
                return result.Succeeded;
            }
            finally
            {
                identityUserService.Dispose();
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
                return PartialView("_Update", UpdateGroupModel.Convert(group));
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
                // check wheter model is valid or not
                if (!ModelState.IsValid) return PartialView("_Update", model);

                // check if a group with the incoming id exist
                var group = groupManager.FindByIdAsync(model.Id).Result;
                if (group == null) return PartialView("_Update", model);

                // check group name exist
                if (groupManager.FindByNameAsync(model.Name).Result != null &&
                    !groupManager.FindByNameAsync(model.Name).Result.Id.Equals(model.Id))
                {
                    ModelState.AddModelError("Name", "The name exists already.");
                    if (!ModelState.IsValid) return PartialView("_Update", model);
                }

                group.Name = model.Name;
                group.DisplayName = group.Name;
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

        #region Remote Validation

        public JsonResult ValidateGroupname(string username, long id = 0)
        {
            var groupManager = new GroupManager();

            try
            {
                var group = groupManager.FindByNameAsync(username);

                if (group == null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (group.Id == id)
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var error = string.Format(CultureInfo.InvariantCulture, "The groupname exists already.", username);

                        return Json(error, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            finally
            {
                groupManager.Dispose();
            }
        }

        #endregion Remote Validation
    }
}