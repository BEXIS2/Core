using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class GroupsController : Controller
    {
        public ActionResult Create()
        {
            Session["UserIds"] = new List<long>();
            return PartialView("_Create", new CreateGroupModel());
        }

        [HttpPost]
        public ActionResult Create(CreateGroupModel model)
        {
            if (ModelState.IsValid)
            {
                var groupManager = new GroupManager();

                groupManager.Create(new Group() { Name = model.GroupName, Description = model.Description, GroupType = (GroupType)model.GroupType });

                return RedirectToAction("Index");
            }

            return PartialView("_Create");
        }

        public ActionResult Delete(long id)
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(DeleteGroupModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("Index");
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
                groups = groups.FilterBy<Group, GroupGridRowModel>(filters);
            }

            // Sorting
            var sorted = groups.Sort(command.SortDescriptors) as IQueryable<Group>;

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize).ToList();

            // Data
            var data = paged.Select(GroupGridRowModel.Convert);

            return View(new GridModel<GroupGridRowModel> { Data = data, Total = total });
        }

        public ActionResult Index()
        {
            var gm = new GroupManager();

            for (int i = 0; i < 1000; i++)
            {
                gm.Create(new Group() { Description = $"Desc of group {i}", Name = $"Group{i}" });
            }
            return View(new GridModel<GroupGridRowModel>());
        }

        public void SetMembership(long userId, bool value)
        {
            var userIds = Session["UserIds"] as List<long>;

            if (value)
            {
                userIds.Add(userId);
            }
            else
            {
                userIds.Remove(userId);
            }

            Session["UserIds"] = userIds;
        }

        public ActionResult Update()
        {
            // get group
            var model = new UpdateGroupModel();
            Session["UserIds"] = new List<long>();

            return PartialView("_Update", model);
        }

        [HttpPost]
        public ActionResult Update(UpdateGroupModel model)
        {
            return PartialView("_Update");
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
                users = users.FilterBy<User, UserMembershipGridRowModel>(filters);
            }

            // Sorting
            var sorted = users.Sort(command.SortDescriptors) as IQueryable<User>;

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize).ToList();

            // Paging
            var userIds = Session["UserIds"] as List<long>;
            var data = paged.Select(x => UserMembershipGridRowModel.Convert(x, userIds));

            return View(new GridModel<UserMembershipGridRowModel> { Data = data, Total = total });
        }
    }
}