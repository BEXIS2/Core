using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Authorization;
using System.Web.Mvc;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class PublishController : Controller
    {
        // GET: Publish
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Entrypoint for Publish Hook
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [BExISEntityAuthorize(typeof(Dataset), "id", RightType.Read)]
        public ActionResult Start(long id, int version = 0)
        {
            return RedirectToAction("getPublishDataPartialView", "Submission", new { datasetId = id, versionId = version });
        }
    }
}