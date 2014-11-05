using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.Auth.Models;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.Auth.Controllers
{
    public class GroupsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        #region Grid View

        // A
        public bool AddUserToGroup(long userId, long groupId)
        {
            SubjectManager subjectManager = new SubjectManager();

            return subjectManager.AddUserToGroup(userId, groupId);
        }

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

        // D
        [HttpPost]
        public void Delete(long id)
        {
            SubjectManager subjectManager = new SubjectManager();
            subjectManager.DeleteGroupById(id);
        }

        // E
        public ActionResult Edit(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            Group group = subjectManager.GetGroupById(id);

            if (group != null)
            {
                return PartialView("_EditPartial", GroupUpdateModel.Convert(group));
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("Window_Details", "The group does not exist!"));
            }
        }

        [HttpPost]
        public ActionResult Edit(GroupUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                SubjectManager subjectManager = new SubjectManager();

                Group group = subjectManager.GetGroupById(model.Id);

                if (group != null)
                {
                    group.Name = model.GroupName;
                    group.Description = model.Description;

                    subjectManager.UpdateGroup(group);

                    return PartialView("_ShowPartial", GroupReadModel.Convert(group));
                }
                else
                {
                    return PartialView("_InfoPartial", new InfoModel("Window_Details", "The group does not exist!"));
                }
            }

            return PartialView("_EditPartial", model);
        }

        // G
        public ActionResult Groups()
        {
            return View();
        }

        [GridAction]
        public ActionResult Groups_Select()
        {
            SubjectManager subjectManager = new SubjectManager();

            // DATA
            IQueryable<Group> data = subjectManager.GetAllGroups();

            List<GroupGridRowModel> groups = new List<GroupGridRowModel>();
            data.ToList().ForEach(g => groups.Add(GroupGridRowModel.Convert(g)));

            return View(new GridModel<GroupGridRowModel> { Data = groups });
        }

        // M
        public ActionResult Membership(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            Group group = subjectManager.GetGroupById(id); ;

            if (group != null)
            {
                ViewData["GroupId"] = id;

                return PartialView("_MembershipPartial");
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("Window_Details", "The group does not exist!"));
            }
        }

        [GridAction]
        public ActionResult Membership_Select(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            // DATA
            Group group = subjectManager.GetGroupById(id);

            List<GroupMembershipGridRowModel> users = new List<GroupMembershipGridRowModel>();

            if (group != null)
            {
                IQueryable<User> data = subjectManager.GetAllUsers();

                data.ToList().ForEach(u => users.Add(GroupMembershipGridRowModel.Convert(group.Id, u, subjectManager.IsUserInGroup(u.Id, group.Id))));
            }

            return View(new GridModel<GroupMembershipGridRowModel> { Data = users });
        }

        // R
        public bool RemoveUserFromGroup(long userId, long groupId)
        {
            SubjectManager subjectManager = new SubjectManager();

            return subjectManager.RemoveUserFromGroup(userId, groupId);
        }

        // S
        public ActionResult Show(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            Group group = subjectManager.GetGroupById(id);

            if (group != null)
            {
                return PartialView("_ShowPartial", GroupReadModel.Convert(group));
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("Window_Details", "The group does not exist!"));
            }
        }

        // U
        public ActionResult Update(long id)
        {
            return PartialView("_UpdatePartial", id);
        }

        #endregion


        #region Validation

        public JsonResult ValidateGroupName(string groupName, long id = 0)
        {
            SubjectManager subjectManager = new SubjectManager();

            Group group = subjectManager.GetGroupByName(groupName);

            if (group == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (group.Id == id)
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

        #endregion

    }
}
