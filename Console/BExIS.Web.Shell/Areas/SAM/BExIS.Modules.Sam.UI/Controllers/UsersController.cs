using BExIS.Dlm.Services.Party;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Helpers;
using BExIS.Utils.Config;
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
    public class UsersController : BaseController
    {
        [HttpPost]
        public async Task<bool> AddUserToGroup(long userId, string groupName)
        {
            var identityUserService = new IdentityUserService();

            try
            {
                var result = await identityUserService.AddToRoleAsync(userId, groupName);
                return result.Succeeded;
            }
            finally
            {
                identityUserService.Dispose();
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
            var identityUserService = new IdentityUserService();

            try
            {
                if (!ModelState.IsValid) return PartialView("_Create", model);

                var user = new User { UserName = model.UserName, FullName = model.UserName, Email = model.Email.Trim() };

                var result = await identityUserService.CreateAsync(user);
                if (result.Succeeded)
                {
                    var code = await identityUserService.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { area = "", userId = user.Id, code },
                        Request.Url.Scheme);
                    await
                        identityUserService.SendEmailAsync(user.Id, "Set your password!",
                            "Please set your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return Json(new { success = true });
                }

                AddErrors(result);

                return PartialView("_Create", model);
            }
            finally
            {
                identityUserService.Dispose();
            }
        }

        [HttpPost]
        public async Task Delete(long userId)
        {
            var userManager = new UserManager();

            try
            {
                var user = userManager.FindByIdAsync(userId).Result;

                for (int i = 0; i < user.Groups.Count; i++)
                {
                    var @group = user.Groups.ElementAt(i);
                    await removeUserFromGroup(user.Id, @group.Name);
                }

                await userManager.DeleteAsync(user);
            }
            catch (Exception ex)
            {
                throw;
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
        public async Task<bool> RemoveUserFromGroup(long userId, string groupName)
        {
            return await removeUserFromGroup(userId, groupName);
        }

        private async Task<bool> removeUserFromGroup(long userId, string groupName)
        {
            var identityUserService = new IdentityUserService();

            try
            {
                var result = await identityUserService.RemoveFromRoleAsync(userId, groupName);
                return result.Succeeded;
            }
            finally
            {
                identityUserService.Dispose();
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
            using (var userManager = new UserManager())
            using (var partyManager = new PartyManager())
            using (var partyTypeManager = new PartyTypeManager())
            {


                // check wheter model is valid or not
                if (!ModelState.IsValid) return PartialView("_Update", model);

                // check if a user with the incoming id exist
                var user = userManager.FindByIdAsync(model.Id).Result;
                if (user == null) return PartialView("_Update", model);

                // if the email is changed, the system needs to check, if the incoming email allready exist by a other user or not
                if (user.Email.Trim() != model.Email.Trim())
                {
                    // check duplicate email cause of client validation is not working in a telerik window :(
                    var duplicateUser = userManager.FindByEmailAsync(model.Email).Result;
                    if (duplicateUser != null) ModelState.AddModelError("Email", "The email address exists already.");
                    if (!ModelState.IsValid) return PartialView("_Update", model);

                    var es = new EmailService();
                    es.Send(MessageHelper.GetUpdateEmailHeader(),
                        MessageHelper.GetUpdaterEmailMessage(user.DisplayName, user.Email, model.Email),
                        GeneralSettings.SystemEmail
                        );
                }
                user.Email = model.Email.Trim();

                // Update email in party
                if (GeneralSettings.UsePersonEmailAttributeName)
                {
                    var party = partyManager.GetPartyByUser(user.Id);

                    var nameProp = partyTypeManager.PartyCustomAttributeRepository.Get(attr => (attr.PartyType == party.PartyType) && (attr.Name == GeneralSettings.PersonEmailAttributeName)).FirstOrDefault();
                    if (nameProp != null)
                    {
                        partyManager.AddPartyCustomAttributeValue(party, nameProp, user.Email);
                    }
                }

                userManager.UpdateAsync(user);
                return Json(new { success = true });
            }
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Users_Select(GridCommand command)
        {
            var userManager = new UserManager();

            try
            {
                var users = new List<UserGridRowModel>();
                int count = userManager.Users.Count();
                if (command != null)// filter subjects based on grid filter settings
                {
                    FilterExpression filter = TelerikGridHelper.Convert(command.FilterDescriptors.ToList());
                    OrderByExpression orderBy = TelerikGridHelper.Convert(command.SortDescriptors.ToList());

                    users = userManager.GetUsers(filter, orderBy, command.Page, command.PageSize, out count).Select(UserGridRowModel.Convert).ToList();
                }
                else
                {
                    users = userManager.Users.Select(UserGridRowModel.Convert).ToList();
                    count = userManager.Users.Count();
                }

                return View(new GridModel<UserGridRowModel> { Data = users, Total = count });
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

        #region Remote Validation

        [AllowAnonymous]
        [HttpPost]
        public JsonResult ValidateEmail(string email, long id = 0)
        {
            var userManager = new UserManager();

            try
            {
                var user = userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (user.Id == id)
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var error = string.Format(CultureInfo.InvariantCulture, "The email address exists already.", email);

                        return Json(error, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            finally
            {
                userManager.Dispose();
            }
        }

        public JsonResult ValidateUsername(string username, long id = 0)
        {
            var userManager = new UserManager();

            try
            {
                var user = userManager.FindByNameAsync(username);

                if (user == null)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (user.Id == id)
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var error = string.Format(CultureInfo.InvariantCulture, "The username exists already.", username);

                        return Json(error, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            finally
            {
                userManager.Dispose();
            }
        }

        #endregion Remote Validation
    }
}