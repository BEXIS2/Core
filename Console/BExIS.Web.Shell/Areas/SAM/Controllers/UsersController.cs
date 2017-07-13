using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Extensions;
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
            Session["Groups"] = new List<long>();
            return PartialView("_Create", new CreateUserModel());
        }

        [HttpPost]
        public ActionResult Create(CreateUserModel model)
        {
            if (ModelState.IsValid)
            {
                RedirectToAction("Index");
            }

            return PartialView("_Create", model);
        }

        public ActionResult Delete(long id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Delete(DeleteUserModel model)
        {
            if (ModelState.IsValid)
            {
                RedirectToAction("Index");
            }

            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Groups_Select(GridCommand command)
        {
            var groupManager = new GroupManager();

            // Source + Transformation - Data
            var groups = groupManager.Groups;
            var total = groups.Count();

            // Filtering
            var filters = command.FilterDescriptors as List<FilterDescriptor>;

            if (filters != null)
            {
                groups = groups.FilterBy<Group, GroupMembershipGridRowModel>(filters);
            }

            // Sorting
            var sorted = groups.Sort(command.SortDescriptors) as IQueryable<Group>;

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize).ToList();

            // Paging
            var groupIds = Session["Groups"] as List<long>;
            var data = paged.Select(x => GroupMembershipGridRowModel.Convert(x, groupIds));

            return View(new GridModel<GroupMembershipGridRowModel> { Data = data, Total = total });
        }

        public ActionResult Index()
        {
            return View(new GridModel<UserGridRowModel>());
        }

        public void SetMembership(long groupId, bool value)
        {
            var groupIds = Session["Groups"] as List<long>;

            if (value)
            {
                groupIds.Add(groupId);
            }
            else
            {
                groupIds.Remove(groupId);
            }

            Session["Groups"] = groupIds;
        }

        public ActionResult Update(long userId)
        {
            // get user
            var model = new UpdateUserModel();
            Session["Groups"] = new List<long>();

            return PartialView("_Update", model);
        }

        [HttpPost]
        public ActionResult Update(UpdateUserModel model)
        {
            return PartialView("_Update", model);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Users_Select(GridCommand command)
        {
            var userManager = new UserManager(new UserStore());

            // Source + Transformation - Data
            var users = userManager.Users;
            var total = users.Count();

            // Filtering
            var filters = command.FilterDescriptors as List<FilterDescriptor>;

            if (filters != null)
            {
                users = users.FilterBy<User, UserGridRowModel>(filters);
            }

            // Sorting
            var sorted = users.Sort(command.SortDescriptors) as IQueryable<User>;

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize).ToList();

            // Data
            var data = paged.Select(UserGridRowModel.Convert);

            return View(new GridModel<UserGridRowModel> { Data = data, Total = total });
        }
    }
}