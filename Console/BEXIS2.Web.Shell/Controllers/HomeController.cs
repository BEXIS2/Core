using BEXIS2.Dlm.Entities.Data;
using BEXIS2.Web.Shell.DB;
using BEXIS2.Web.Shell.Models;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using System.Data;
using System.Diagnostics;
using DbSession = NHibernate.ISession; // Alias definieren

namespace BEXIS2.Web.Shell.Controllers
{
    public class HomeController : Controller
    {
        private readonly DatasetManager _datasetManager;

        // Die Session (Unit of Work) wird automatisch per DI bereitgestellt
        public HomeController(DatasetManager datasetManager)
        {
            _datasetManager = datasetManager;
        }

        public async Task<IActionResult> Index()
        {
            var datasets = await _datasetManager.GetAllAsync();
            return View(datasets);
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
