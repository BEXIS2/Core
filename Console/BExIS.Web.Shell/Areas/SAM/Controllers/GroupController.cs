using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Subjects;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class GroupController : Controller
    {
        [GridAction]
        public ActionResult Groups_Select(GridState model)
        {
            var groupManager = new GroupManager();
            var groups = groupManager.Groups.Where(model.Filter)
                .OrderBy(model.OrderBy)
                .Skip((model.Page - 1) * model.Size)
                .Take(model.Size)
                .Select(g => GroupGridRowModel.Convert(g))
                .ToList();

            return View(new GridModel<GroupGridRowModel> { Data = groups });
        }

        // GET: Group
        public ActionResult Index()
        {
            //var groupManager = new GroupManager();
            //for (int i = 0; i < 1000; i++)
            //{
            //    groupManager.Create(new Group() { Name = $"Gruppe{i}" });
            //}

            return View();
        }
    }
}