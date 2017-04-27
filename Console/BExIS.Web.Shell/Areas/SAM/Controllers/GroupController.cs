using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class GroupController : Controller
    {
        [GridAction(EnableCustomBinding = true)]
        public ActionResult Groups_Select(GridCommand command)
        {
            var groupManager = new GroupManager();

            groupManager.Create(new Group() { Name = $"Group{DateTime.Now}" });

            // Source + Transformation - Data
            var groups = groupManager.Groups.ToGroupGridRowModel();
            var total = groups.Count();

            // Paging
            groups = groups.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize);

            return View(new GridModel<GroupGridRowModel> { Data = groups.ToList(), Total = total });
        }

        public ActionResult Index()
        {
            return View(new GridModel<GroupGridRowModel>());
        }
    }
}