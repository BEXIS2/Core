using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Subjects;
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
            return PartialView("_Create");
        }

        [HttpPost]
        public ActionResult Create(CreateUserModel model)
        {
            if (ModelState.IsValid)
            {
                var userStore = new UserStore();
                var user = userStore.Create(model.UserName, model.Email, model.IsAdministrator);
                RedirectToAction("Index");
            }

            return PartialView("_Create", model);
        }

        [HttpPost]
        public void Delete(long userId)
        {
            var userStore = new UserStore();
            userStore.Delete(userId);
        }

        public void Groups_Save(UserMembershipGridRowModel[] groups)
        {
            Session["SelectedGroups"] = groups.Where(g => g.IsUserInGroup).Select(g => g.Id).ToList();
        }

        [GridAction]
        public ActionResult Groups_Select(long userId, Dictionary<long, bool> selection)
        {
            updateSelection(selection);

            var groupManager = new GroupManager();
            var groups = groupManager.Groups.Select(g => GroupMembershipGridRowModel.Convert(g, Session["SelectedGroups"] as HashSet<long>)).ToList();

            return View(new GridModel<GroupMembershipGridRowModel> { Data = groups });
        }

        private void updateSelection(Dictionary<long, bool> selection)
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

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Update(long userId)
        {
            var userStore = new UserStore();
            var user = userStore.FindById(userId);

            return PartialView("_Update", UpdateUserModel.Convert(user));
        }

        [HttpPost]
        public ActionResult Update(UpdateUserModel model, string test)
        {
            return PartialView("_Update", model);
        }

        [GridAction]
        public ActionResult Users_Select()
        {
            var userManager = new UserManager(new UserStore());
            var users = userManager.Users.Select(UserGridRowModel.Convert).ToList();

            return View(new GridModel<UserGridRowModel> { Data = users });
        }
    }
}