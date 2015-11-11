using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.SAM.Models;
using Telerik.Web.Mvc;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.SAM.Controllers
{
    public class GroupsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        // --------------------------------------------------
        // GROUPS
        // --------------------------------------------------

        #region Groups

        public ActionResult Groups()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Groups");
            return View();
        }

        [GridAction]
        public ActionResult Groups_Select()
        {
            SubjectManager subjectManager = new SubjectManager();
            List<GroupGridRowModel> groups = subjectManager.GetAllGroups().Select(g => GroupGridRowModel.Convert(g)).ToList();

            return View(new GridModel<GroupGridRowModel> { Data = groups });
        }

        #endregion Groups

        [HttpPost]
        public void Delete(long id)
        {
            SubjectManager subjectManager = new SubjectManager();
            subjectManager.DeleteGroupById(id);
        }

        public ActionResult Edit(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            Group group = subjectManager.GetGroupById(id);

            return PartialView("_EditPartial", GroupEditModel.Convert(group));
        }

        [HttpPost]
        public ActionResult Edit(GroupEditModel model)
        {
            if (ModelState.IsValid)
            {
                SubjectManager subjectManager = new SubjectManager();

                Group group = subjectManager.GetGroupById(model.GroupId);

                group.Name = model.GroupName;
                group.Description = model.Description;

                long[] users = group.Users.Select(g => g.Id).ToArray();

                foreach (long userId in users)
                {
                    subjectManager.RemoveUserFromGroup(userId, @group.Id);
                }

                if (Session["Users"] != null)
                {
                    foreach (GroupMembershipGridRowModel user in (GroupMembershipGridRowModel[]) Session["Users"])
                    {
                        subjectManager.AddUserToGroup(user.Id, group.Id);
                    }
                }

                subjectManager.UpdateGroup(group);

                return Json(new { success = true });
            }
            else
            {
                return PartialView("_EditPartial", model);
            }
        }

        [GridAction]
        public ActionResult Membership_Select(long id, long[] selectedUsers)
        {
            SubjectManager subjectManager = new SubjectManager();

            List<GroupMembershipGridRowModel> users = new List<GroupMembershipGridRowModel>();

            if (selectedUsers != null)
            {
                users = subjectManager.GetAllUsers().Select(u => GroupMembershipGridRowModel.Convert(u, selectedUsers.Contains(u.Id))).ToList();
            }
            else
            {
                Group group = subjectManager.GetGroupById(id);

                users = subjectManager.GetAllUsers().Select(u => GroupMembershipGridRowModel.Convert(u, u.Groups.Any(g => g.Id == id))).ToList();
            }

            return View(new GridModel<GroupMembershipGridRowModel> { Data = users });
        }

        public void SetMembership(GroupMembershipGridRowModel[] users)
        {
            Session["Users"] = users;
        }

        #region Grid View

        // C
        public ActionResult Create()
        {
            return PartialView("_CreatePartial");
        }

        [HttpPost]
        public ActionResult Create(GroupCreateModel model)
        {
            if (ModelState.IsValid)
            {
                SubjectManager subjectManager = new SubjectManager();
                subjectManager.CreateGroup(model.GroupName, model.Description);

                return Json(new { success = true });
            }

            return PartialView("_CreatePartial", model);
        }

        #endregion Grid View

        #region Validation

        public JsonResult ValidateGroupName(string groupName, long groupId = 0)
        {
            SubjectManager subjectManager = new SubjectManager();

            Group group = subjectManager.GetGroupByName(groupName);

            if (group == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (group.Id == groupId)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string error = String.Format(CultureInfo.InvariantCulture, "The group name already exists.", groupName);

                    return Json(error, JsonRequestBehavior.AllowGet);
                }
            }
        }

        #endregion Validation
    }
}