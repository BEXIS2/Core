using BExIS.Utils.Helpers;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            string name = "test";
            var x = RegExHelper.IsFilenameValid(name);

            name = "test | filename";

            x = RegExHelper.IsFilenameValid(name);

            name = RegExHelper.GetCleanedFilename(name);

            name = "des<>";
            x = RegExHelper.IsFilenameValid(name);
            name = RegExHelper.GetCleanedFilename(name);

            name = "123\"";
            x = RegExHelper.IsFilenameValid(name);
            name = RegExHelper.GetCleanedFilename(name);


            return View();
        }
    }
}