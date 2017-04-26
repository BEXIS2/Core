using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BExIS.Security.Services.Subjects;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class SubjectController : Controller
    {
        public ActionResult CreateGroup()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateGroup(GroupCreationModel model)
        {
            return View(model);
        }

        public ActionResult CreateUser()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateUser(UserCreationModel model)
        {
            return View(model);
        }

        public ActionResult Index()
        {
            GroupManager groupManager = new GroupManager();

            for (int i = 0; i < 1000; i++)
            {
                groupManager.Create(new Group() { Name = $"Group_{i}" });
            }

            return View();
        }

        [GridAction]
        public ActionResult Subjects_Select(GridActionAttribute filters)
        {
            SubjectManager subjectManager = new SubjectManager();
            List<SubjectGridRowModel> subjects = subjectManager.Subjects.Select(s => SubjectGridRowModel.Convert(s)).ToList();

            return View(new GridModel<SubjectGridRowModel> { Data = subjects });
        }
    }
}