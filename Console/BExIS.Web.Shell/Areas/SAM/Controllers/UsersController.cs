using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
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
            return PartialView("_Create", new CreateUserModel());
        }

        [HttpPost]
        public ActionResult Create(CreateUserModel model)
        {
            if (ModelState.IsValid)
            {
                var userManager = new UserManager(new UserStore());
                userManager.CreateAsync(new User()
                {
                    Email = model.Email,
                    UserName = model.UserName,
                    IsAdministrator = model.IsAdministrator
                });

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

        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        public ActionResult Memberships_Select(Dictionary<long, bool> selection, long userId)
        {
            var groupManager = new GroupManager();
            var groups = groupManager.Groups.Select(g => GroupMembershipGridRowModel.Convert(g, userId)).ToList();

            return View(new GridModel<GroupMembershipGridRowModel> { Data = groups });
        }

        public ActionResult Update(long userId)
        {
            // get user
            var model = new UpdateUserModel();

            return PartialView("_Update", model);
        }

        [HttpPost]
        public ActionResult Update(UpdateUserModel model, List<long> selectedGroups)
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

        [GridAction]
        public ActionResult Users_Select()
        {
            var userManager = new UserManager(new UserStore());
            var users = userManager.Users.Select(UserGridRowModel.Convert).ToList();

            return View(new GridModel<UserGridRowModel> { Data = users });
        }
    }
}