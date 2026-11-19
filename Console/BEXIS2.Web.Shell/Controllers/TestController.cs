
using BEXIS2.Web.Shell.DB;
using BEXIS2.Web.Shell.Models;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using System.Data;
using System.Diagnostics;
using DbSession = NHibernate.ISession; // Alias definieren
using EntityTemplate2Manager = BEXIS2.Dlm.Services.Data.EntityTemplate2Manager; // Alias definieren

namespace BEXIS2.Web.Shell.Controllers
{
    public class TestController : Controller
    {
        private readonly DatasetManager _datasetManager;
        private readonly EntityTemplate2Manager _entityTemplateManager;


        // Die Session (Unit of Work) wird automatisch per DI bereitgestellt
        public TestController(DatasetManager datasetManager, EntityTemplate2Manager entityTemplateManager)
        {
            _datasetManager = datasetManager;
            _entityTemplateManager = entityTemplateManager;
        }

        public async Task<IActionResult> Index()
        {
            SvelteAssetService _svelteAssets = new SvelteAssetService(HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>());

            ViewData["start"] = _svelteAssets.GetScript("shell");
            ViewData["app"] = _svelteAssets.GetApp("shell");

            //var datasets = await _datasetManager.GetAllAsync();
            //var templates = _entityTemplateManager.GetAllAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
