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
        private readonly GroupManager _groupManager;
        private readonly UserManager _userManager;

        public UsersController(GroupManager groupManager, UserManager userManager)
        {
            _groupManager = groupManager;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<bool> AddUserToGroup(long userId, string groupName)
        {
            try
            {
                var result = await _userManager.AddToRoleAsync(userId, groupName);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                return false;
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
            try
            {
                if (!ModelState.IsValid) return PartialView("_Create", model);

                var user = new User { UserName = model.UserName, FullName = model.UserName, Email = model.Email.Trim() };

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { area = "", userId = user.Id, code },
                        Request.Url.Scheme);
                    await
                        _userManager.SendEmailAsync(user.Id, "Set your password!",
                            "Please set your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return Json(new { success = true });
                }

                AddErrors(result);

                return PartialView("_Create", model);
            }
            catch (Exception ex)
            {
                return PartialView("_Create", model);
            }
        }

        [HttpPost]
        public async Task Delete(long userId)
        {
            try
            {
                var user = _userManager.FindByIdAsync(userId).Result;

                for (int i = 0; i < user.Groups.Count; i++)
                {
                    var @group = user.Groups.ElementAt(i);
                    await removeUserFromGroup(user.Id, @group.Name);
                }

                await _userManager.DeleteAsync(user);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult Groups(long userId)
        {
            return PartialView("_Groups", userId);
        }

        [GridAction]
        public ActionResult Groups_Select(long userId)
        {
            try
            {
                var groups = new List<GroupMembershipGridRowModel>();

                foreach (var group in _groupManager.Roles)
                {
                    groups.Add(GroupMembershipGridRowModel.Convert(group, userId));
                }

                return View(new GridModel<GroupMembershipGridRowModel> { Data = groups });
            }
            catch (Exception ex)
            {
                return View(new GridModel<GroupMembershipGridRowModel> { Data = new List<GroupMembershipGridRowModel>() });
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
            try
            {
                var result = await _userManager.RemoveFromRoleAsync(userId, groupName);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public ActionResult Update(long userId)
        {
            try
            {
                var user = _userManager.FindByIdAsync(userId).Result;
                return PartialView("_Update", UpdateUserModel.Convert(user));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public ActionResult Update(UpdateUserModel model)
        {
            using (var partyManager = new PartyManager())
            using (var partyTypeManager = new PartyTypeManager())
            {
                // check wheter model is valid or not
                if (!ModelState.IsValid) return PartialView("_Update", model);

                // check if a user with the incoming id exist
                var user = _userManager.FindByIdAsync(model.Id).Result;
                if (user == null) return PartialView("_Update", model);

                // if the email is changed, the system needs to check, if the incoming email allready exist by a other user or not
                if (user.Email.Trim() != model.Email.Trim())
                {
                    // check duplicate email cause of client validation is not working in a telerik window :(
                    var duplicateUser = _userManager.FindByEmailAsync(model.Email).Result;
                    if (duplicateUser != null) ModelState.AddModelError("Email", "The email address exists already.");
                    if (!ModelState.IsValid) return PartialView("_Update", model);

                    using (var emailService = new EmailService())
                    {
                        emailService.Send(MessageHelper.GetUpdateEmailHeader(),
                            MessageHelper.GetUpdaterEmailMessage(user.DisplayName, user.Email, model.Email),
                            GeneralSettings.SystemEmail
                            );
                    }   
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

                _userManager.UpdateAsync(user);
                return Json(new { success = true });
            }
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Users_Select(GridCommand command)
        {
            try
            {
                var users = new List<UserGridRowModel>();
                int count = _userManager.Users.Count();
                if (command != null)// filter subjects based on grid filter settings
                {
                    FilterExpression filter = TelerikGridHelper.Convert(command.FilterDescriptors.ToList());
                    OrderByExpression orderBy = TelerikGridHelper.Convert(command.SortDescriptors.ToList());

                    users = _userManager.Find(filter, orderBy, command.Page, command.PageSize, out count).Select(UserGridRowModel.Convert).ToList();
                }
                else
                {
                    users = _userManager.Users.Select(UserGridRowModel.Convert).ToList();
                    count = _userManager.Users.Count();
                }

                return View(new GridModel<UserGridRowModel> { Data = users, Total = count });
            }
            catch(Exception ex)
            {
                return View(new GridModel<UserGridRowModel> { Data = new List<UserGridRowModel>(), Total = 0 });
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
            try
            {
                var user = _userManager.FindByEmailAsync(email);

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
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ValidateUsername(string username, long id = 0)
        {
            try
            {
                var user = _userManager.FindByNameAsync(username);

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
            catch (Exception ex)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Remote Validation
    }
}