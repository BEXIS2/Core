using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using BExIS.Security.Entities;
using BExIS.Security.Services;
using BExIS.Web.Shell.Areas.Auth.Models;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.Auth.Controllers
{
    public class UsersController : Controller
    {
        public ActionResult Users()
        {
            UserManager userManager = new UserManager();

            // TOTAL
            ViewData["total"] = userManager.GetAllUsers().Count();

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Users_Select(GridCommand command)
        {
            UserManager userManager = new UserManager();

            // DATA
            IQueryable<User> data = userManager.GetAllUsers();

            #region Filtering

            // Filtering
            if (command.FilterDescriptors.Any())
            {
                data = data.Where(ExpressionBuilder.Expression<User>(command.FilterDescriptors));
            }

            #endregion

            #region Sorting

            // Sorting
            if (command.SortDescriptors.Any())
            {
                foreach (SortDescriptor sortDescriptor in command.SortDescriptors)
                {
                    if (sortDescriptor.SortDirection == ListSortDirection.Ascending)
                    {
                        switch (sortDescriptor.Member)
                        {
                            case "Id":
                                data = data.OrderBy(u => u.Id);
                                break;
                            case "Name":
                                data = data.OrderBy(u => u.Name);
                                break;
                            case "Email":
                                data = data.OrderBy(u => u.Email);
                                break;
                            case "RegistrationDate":
                                data = data.OrderBy(u => u.RegistrationDate);
                                break;
                            case "LastLoginDate":
                                data = data.OrderBy(u => u.LastLoginDate);
                                break;
                            case "LastActivityDate":
                                data = data.OrderBy(u => u.LastActivityDate);
                                break;
                            case "IsApproved":
                                data = data.OrderBy(u => u.IsApproved);
                                break;
                            case "IsLockedOut":
                                data = data.OrderBy(u => u.IsLockedOut);
                                break;
                            case "Comment":
                                data = data.OrderBy(u => u.Comment);
                                break;
                        }
                    }
                    else
                    {
                        switch (sortDescriptor.Member)
                        {
                            case "Id":
                                data = data.OrderByDescending(u => u.Id);
                                break;
                            case "Name":
                                data = data.OrderByDescending(u => u.Name);
                                break;
                            case "Email":
                                data = data.OrderByDescending(u => u.Email);
                                break;
                            case "RegistrationDate":
                                data = data.OrderByDescending(u => u.RegistrationDate);
                                break;
                            case "LastLoginDate":
                                data = data.OrderByDescending(u => u.LastLoginDate);
                                break;
                            case "LastActivityDate":
                                data = data.OrderByDescending(u => u.LastActivityDate);
                                break;
                            case "IsApproved":
                                data = data.OrderByDescending(u => u.IsApproved);
                                break;
                            case "IsLockedOut":
                                data = data.OrderByDescending(u => u.IsLockedOut);
                                break;
                            case "Comment":
                                data = data.OrderByDescending(u => u.Comment);
                                break;
                        }
                    }

                }
            }

            #endregion

            #region Paging

            int total = data.Count();

            #endregion

            data = data.Skip(command.PageSize * (command.Page - 1)).Take(command.PageSize);

            List<UserModel> users = new List<UserModel>();
            data.ToList().ForEach(u => users.Add(UserModel.Convert(u)));

            return View(new GridModel<UserModel> { Data = users, Total = total });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult Users_Delete(int id, GridCommand command)
        {
            UserManager userManager = new UserManager();

            User user = userManager.GetUserById(id);

            if (user != null)
            {
                userManager.Delete(user);
            }

            return Users_Select(command);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult Users_Update(int id, GridCommand command)
        {
            UserManager userManager = new UserManager();

            User user = userManager.GetUserById(id);

            TryUpdateModel(user);

            userManager.Update(user);

            return Users_Select(command);
        }
    }
}
