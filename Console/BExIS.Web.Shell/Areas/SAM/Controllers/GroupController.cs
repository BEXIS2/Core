using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class GroupController : Controller
    {
        [GridAction]
        public ActionResult Groups_Select(FilterDescriptor filters)
        {
            var groupManager = new GroupManager();
            var groups = groupManager.Groups.Select(g => GroupGridRowModel.Convert(g)).ToList();

            return View(new GridModel<GroupGridRowModel> { Data = groups });
        }

        // GET: Group
        public ActionResult Index()
        {
            var groupManager = new GroupManager();
            for (int i = 0; i < 1000; i++)
            {
                groupManager.Create(new Group() { Name = $"Gruppe{i}" });
            }

            return View();
        }
    }
}