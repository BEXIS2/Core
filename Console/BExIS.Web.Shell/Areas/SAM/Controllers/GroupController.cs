using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Subjects;
using System.Collections;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class GroupController : Controller
    {
        [GridAction]
        public ActionResult Groups_Select(GridState model)
        {
            var groupManager = new GroupManager();

            // Source + Transformation - Data
            var groups = groupManager.Groups.ToGroupGridRowModel();
            var total = groups.Count();

            // Filtering
            if (!string.IsNullOrEmpty(model.Filter))
            {
                groups = groups.Where(model.Filter);
            }

            // Sorting
            if (!string.IsNullOrEmpty(model.OrderBy))
            {
                groups = groups.Where(model.OrderBy);
            }

            // Paging
            groups = groups.Skip((model.Page - 1) * model.Size)
                .Take(model.Size);

            IEnumerable data = groups.ToIList();

            var result = new GridModel()
            {
                Data = data,
                Total = total
            };

            return Json(result);
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