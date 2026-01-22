using System.Web.Mvc;
using Vaiona.Web.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.Extensions;
using System;

namespace BExIS.Modules.Pum.UI.Controllers
{
    public class ViewController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}