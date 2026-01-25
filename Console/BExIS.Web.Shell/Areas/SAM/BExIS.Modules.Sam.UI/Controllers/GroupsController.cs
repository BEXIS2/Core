using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.Utils.NH.Querying;
using Microsoft.AspNet.Identity;
using System;
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
        [HttpPost]
        public async Task<bool> AddUserToGroupAsync(long userId, string groupName)
        {
            using (var userManager = new UserManager())
            using (var identityUserService = new IdentityUserService(userManager))
            {
                try
                {
                    var user = identityUserService.FindByIdAsync(userId).Result;
                    var result = await identityUserService.AddToRoleAsync(user.Id, groupName);
                    return result.Succeeded;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public ActionResult Create()
        {
            return PartialView("_Create", new CreateGroupModel());
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(CreateGroupModel model)
        {
            using (var groupManager = new GroupManager())
            using (var identityGroupService = new IdentityGroupService(groupManager))
            {
                try
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
                catch(Exception ex)
                {
                    return PartialView("_Create", model);
                }
                
            }
        }

        [HttpPost]
        public async Task<bool> DeleteAsync(long groupId)
        {
            using (var groupManager = new GroupManager())
            using (var identityGroupService = new IdentityGroupService(groupManager))
            {
                try
                {
                    var result = await identityGroupService.DeleteByIdAsync(groupId);
                    return result;
                }
                catch(Exception ex)
                {
                    return false;
                }
                
            }
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Groups_Select(GridCommand command)
        {
            using (var groupManager = new GroupManager())
            using (var identityGroupService = new IdentityGroupService(groupManager))
            {
                try
                {
                    var groups = new List<GroupGridRowModel>();
                    int count = identityGroupService.Roles.Count();
                    if (command != null)// filter subjects based on grid filter settings
                    {
                        FilterExpression filter = TelerikGridHelper.Convert(command.FilterDescriptors.ToList());
                        OrderByExpression orderBy = TelerikGridHelper.Convert(command.SortDescriptors.ToList());

                        groups = identityGroupService.GetGroups(filter, orderBy, command.Page, command.PageSize, out count).Select(GroupGridRowModel.Convert).ToList();
                    }
                    else
                    {
                        groups = identityGroupService.Roles.Select(GroupGridRowModel.Convert).ToList();
                        count = identityGroupService.Roles.Count();
                    }

                    return View(new GridModel<GroupGridRowModel> { Data = groups, Total = count });
                }
                catch(Exception ex)
                {
                    return View(new GridModel<GroupGridRowModel> { Data = new List<GroupGridRowModel>(), Total = 0 });
                }
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
        public async Task<bool> RemoveUserFromGroupAsync(long userId, string groupName)
        {
            using (var userManager = new UserManager())
            using (var identityUserService = new IdentityUserService(userManager))
            {
                try
                {
                    var user = identityUserService.FindByIdAsync(userId).Result;
                    var result = await identityUserService.RemoveFromRoleAsync(user.Id, groupName);
                    return result.Succeeded;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }    
        }

        /// <summary>
        /// ToDo: Documentation
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<ActionResult> UpdateAsync(long groupId)
        {
            using (var groupManager = new GroupManager())
            using (var identityGroupService = new IdentityGroupService(groupManager))
            {
                try
                {
                    var group = await identityGroupService.FindByIdAsync(groupId);
                    return PartialView("_Update", UpdateGroupModel.Convert(group));
                }
                catch(Exception ex)
                {
                    return View();
                }
            }           
        }

        /// <summary>
        /// ToDo: Documentation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> UpdateAsync(UpdateGroupModel model)
        {
            using (var groupManager = new GroupManager())
            using (var identityGroupService = new IdentityGroupService(groupManager))
            {
                try
                {
                    // check wheter model is valid or not
                    if (!ModelState.IsValid) return PartialView("_Update", model);

                    // check if a group with the incoming id exist
                    var group = identityGroupService.FindByIdAsync(model.Id).Result;
                    if (group == null) return PartialView("_Update", model);

                    // check group name exist
                    if (identityGroupService.FindByNameAsync(model.Name).Result != null &&
                        !identityGroupService.FindByNameAsync(model.Name).Result.Id.Equals(model.Id))
                    {
                        ModelState.AddModelError("Name", "The name exists already.");
                        if (!ModelState.IsValid) return PartialView("_Update", model);
                    }

                    group.Name = model.Name;
                    group.DisplayName = group.Name;
                    group.Description = model.Description;

                    await identityGroupService.UpdateAsync(group);
                    return Json(new { success = true });
                }
                catch(Exception ex)
                {
                    return Json(new { success = false });
                }
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
            using (var userManager = new UserManager())
            {
                try
                {
                    var users = new List<UserMembershipGridRowModel>();

                    foreach (var user in userManager.Users)
                    {
                        users.Add(UserMembershipGridRowModel.Convert(user, groupName));
                    }

                    return View(new GridModel<UserMembershipGridRowModel> { Data = users, Total = users.Count });
                }
                catch (Exception ex)
                {
                    return View(new GridModel<UserMembershipGridRowModel> { Data = new List<UserMembershipGridRowModel>(), Total = 0 });
                }
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
            using (var groupManager = new GroupManager())
            using (var identityGroupService = new IdentityGroupService(groupManager))
            {
                try
                {
                    var group = identityGroupService.FindByNameAsync(username);

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
                catch(Exception ex)
                {
                    var error = string.Format(CultureInfo.InvariantCulture, "The groupname exists already.", username);
                    return Json(error, JsonRequestBehavior.AllowGet);
                }
            }

                
        }

        #endregion Remote Validation
    }
}