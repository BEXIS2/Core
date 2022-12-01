using BExIS.Dlm.Entities.Data;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class EntitiesController : BaseController
    {
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateEntityModel model)
        {
            return View();
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult Entities_Select(GridCommand command)
        {
            var entityManager = new EntityManager();

            try
            {
                // Source + Transformation - Data
                var entities = entityManager.Entities;

                // Filtering
                var filtered = entities;
                var total = filtered.Count();

                // Sorting
                var sorted = (IQueryable<GroupGridRowModel>)filtered.Sort(command.SortDescriptors);

                // Paging
                var paged = sorted.Skip((command.Page - 1) * command.PageSize)
                    .Take(command.PageSize);

                return View(new GridModel<GroupGridRowModel> { Data = paged.ToList(), Total = total });
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        // GET: Entity
        public ActionResult Index()
        {
            var entityManager = new EntityManager();

            try
            {
                entityManager.Create(new Entity() { EntityType = typeof(Dataset), EntityStoreType = typeof(DatasetStore), Securable = true, UseMetadata = true });
                return View();
            }
            finally
            {
                entityManager.Dispose();
            }
        }
    }
}