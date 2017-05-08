using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Subjects;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            return View(new GridModel<UserGridRowModel>());
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Users_Select(GridCommand command)
        {
            var userManager = new UserManager(new UserStore());

            // Source + Transformation - Data
            var users = userManager.Users.ToUserGridRowModel();

            // Filtering
            var filtered = users.Where(ExpressionBuilder.Expression<GroupGridRowModel>(command.FilterDescriptors));
            var total = filtered.Count();

            // Sorting
            var sorted = (IQueryable<GroupGridRowModel>)filtered.Sort(command.SortDescriptors);

            // Paging
            var paged = sorted.Skip((command.Page - 1) * command.PageSize)
                .Take(command.PageSize);

            return View(new GridModel<GroupGridRowModel> { Data = paged.ToList(), Total = total });
        }
    }
}