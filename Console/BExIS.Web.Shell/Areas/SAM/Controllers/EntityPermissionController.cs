using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Subjects;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class EntityPermissionController : Controller
    {
        public ActionResult Index()
        {
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