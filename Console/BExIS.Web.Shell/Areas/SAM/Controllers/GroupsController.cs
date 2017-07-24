using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Subjects;
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
            return View("_Create");
        }

        [HttpPost]
        public ActionResult Create(CreateGroupModel model)
        {
            return View("_Create");
        }

        [GridAction]
        public ActionResult Groups_Select()
        {
            var groupManager = new GroupManager();
            var groups = groupManager.Groups.Select(GroupGridRowModel.Convert).ToList();

            return View(new GridModel<GroupGridRowModel> { Data = groups });
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Update(long groupId)
        {
            var groupManager = new GroupManager();

            var group = groupManager.FindById(groupId);

            var userManager = new UserManager(new UserStore());
            var userMemberships = userManager.Users.Select(u => UserMembershipGridRowModel.Convert(u, group.Id)).ToList();

            return View("_Update", UpdateGroupModel.Convert(group));
        }

        [HttpPost]
        public ActionResult Update(UpdateGroupModel model, List<long> selectedUsers)
        {
            return View("_Update");
        }

        [GridAction]
        public ActionResult UserMemberships_Select(long groupId = 0)
        {
            var userManager = new UserManager(new UserStore());
            var userMemberships = userManager.Users.Select(u => UserMembershipGridRowModel.Convert(u, groupId)).ToList();

            return View(new GridModel<UserMembershipGridRowModel> { Data = userMemberships });
        }
    }
}