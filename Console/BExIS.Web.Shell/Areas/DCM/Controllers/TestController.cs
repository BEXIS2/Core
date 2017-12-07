using BExIS.Security.Services.Utilities;
using BExIS.Utils.Helpers;
using System.Configuration;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {

            var es = new EmailService();
            var datasetId = 1;
            var title = "my cool dataset";
            es.Send(MessageHelper.GetCreateDatasetHeader(),
                MessageHelper.GetCreateDatasetMessage(datasetId, title, "David Schöne"),
                ConfigurationManager.AppSettings["SystemEmail"]
                );


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