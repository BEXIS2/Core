using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Security.Entities;
using BExIS.Security.Services;
using BExIS.Web.Shell.Areas.Auth.Models;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.Auth.Controllers
{
    public class RolesController : Controller
    {
        public ActionResult Roles()
        {
            // TOTAL ROLES
            RoleManager roleManager = new RoleManager();
            ViewData["totalRoles"] = roleManager.GetAllRoles().Count();

            // TOTAL ROLES
            UserManager userManager = new UserManager();
            ViewData["totalUsers"] = userManager.GetAllUsers().Count();

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Roles_Select(GridCommand command)
        {
            RoleManager roleManager = new RoleManager();

            // DATA
            IQueryable<Role> data = roleManager.GetAllRoles();

            #region Filtering

            // Filtering
            if (command.FilterDescriptors.Any())
            {
                data = data.Where(ExpressionBuilder.Expression<Role>(command.FilterDescriptors));
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
                                data = data.OrderBy(r => r.Id);
                                break;
                            case "Name":
                                data = data.OrderBy(r => r.Name);
                                break;
                            case "Description":
                                data = data.OrderBy(r => r.Description);
                                break;
                            case "Comment":
                                data = data.OrderBy(r => r.Comment);
                                break;
                        }
                    }
                    else
                    {
                        switch (sortDescriptor.Member)
                        {
                            case "Id":
                                data = data.OrderByDescending(r => r.Id);
                                break;
                            case "Name":
                                data = data.OrderByDescending(r => r.Name);
                                break;
                            case "Description":
                                data = data.OrderByDescending(r => r.Description);
                                break;
                            case "Comment":
                                data = data.OrderByDescending(r => r.Comment);
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

            List<RoleModel> roles = new List<RoleModel>();
            data.ToList().ForEach(r => roles.Add(RoleModel.Convert(r)));
             
            return View(new GridModel<RoleModel> { Data = roles, Total = total });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult Roles_Delete(int id, GridCommand command)
        {
            RoleManager roleManager = new RoleManager();

            Role role = roleManager.GetRoleById(id);

            if (role != null)
            {
                roleManager.Delete(role);
            }

            return Roles_Select(command);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult Roles_Update(int id, GridCommand command)
        {
            RoleManager roleManager = new RoleManager();
            
            Role role = roleManager.GetRoleById(id);

            TryUpdateModel(role);

            roleManager.Update(role);

            return Roles_Select(command);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction(EnableCustomBinding = true)]
        public ActionResult Roles_Insert(GridCommand command)
        {
            RoleModel roleModel = new RoleModel();

            if (TryUpdateModel(roleModel))
            {
                RoleManager roleManager = new RoleManager();

                roleManager.Create(roleModel.RoleName, roleModel.Description, roleModel.Comment);
            }

            return Roles_Select(command);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [GridAction(EnableCustomBinding = true)]
        public JsonResult Roles_Details(int Id)
        {
            RoleManager roleManager = new RoleManager();

            return Json(new { roleModel = RoleModel.Convert(roleManager.GetRoleById(Id)) });
        }
    }
}
