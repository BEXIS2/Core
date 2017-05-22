using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
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
            return View();
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

            return View();
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
            var groups = groupManager.Groups.ToGroupGridRowModel();

            // Filtering
            var filtered = groups.Where(ExpressionBuilder.Expression<GroupGridRowModel>(command.FilterDescriptors));
            var total = filtered.Count();

            // Sorting
            var sorted = (IQueryable<GroupGridRowModel>)filtered.Sort(command.SortDescriptors);

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize);

            return View(new GridModel<GroupGridRowModel> { Data = paged.ToList(), Total = total });
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Memberships_Select(GridCommand command)
        {
            var userManager = new UserManager(new UserStore());

            // Source + Transformation - Data
            var users = userManager.Users.ToUserGridRowModel();

            // Filtering
            var filtered = users.Where(ExpressionBuilder.Expression<UserMembershipGridRowModel>(command.FilterDescriptors));
            var total = filtered.Count();

            // Sorting
            var sorted = (IQueryable<UserMembershipGridRowModel>)filtered.Sort(command.SortDescriptors);

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize);

            return View(new GridModel<UserMembershipGridRowModel> { Data = paged.ToList(), Total = total });
        }

        public ActionResult Index()
        {
            var groupManager = new GroupManager();
            for (int i = 0; i < 100; i++)
            {
                groupManager.Create(new Group() { Name = $"Group {i}", Description = $"Description of Group {i}", GroupType = GroupType.Private });
            }

            return View();
        }
    }
}