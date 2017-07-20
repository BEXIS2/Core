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
        public ActionResult Groups_Select(Dictionary<long, bool> selection, GridCommand command)
        {
            updateSelection(selection);

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
            var data = paged.Select(x => GroupMembershipGridRowModel.Convert(x, Session["SelectedGroups"] as HashSet<long>));

            return View(new GridModel<GroupMembershipGridRowModel> { Data = data, Total = total });
        }

        public ActionResult Index()
        {
            return View(new GridModel<UserGridRowModel>());
        }

        public ActionResult Update(long userId)
        {
            // get user
            var model = new UpdateUserModel();

            return PartialView("_Update", model);
        }

        [HttpPost]
        public ActionResult Update(UpdateUserModel model)
        {
            return PartialView("_Update", model);
        }

        public void updateSelection(Dictionary<long, bool> selection)
        {
            if (selection == null)
                selection = new Dictionary<long, bool>();

            // Selected Groups
            var selectedGroups = new HashSet<long>();
            if (Session["SelectedGroups"] != null)
                selectedGroups = Session["SelectedGroups"] as HashSet<long>;

            foreach (var item in selection)
            {
                if (item.Value)
                {
                    selectedGroups.Add(item.Key);
                }
                else
                {
                    selectedGroups.Remove(item.Key);
                }
            }

            Session["SelectedGroups"] = selectedGroups;
        }
    }
}