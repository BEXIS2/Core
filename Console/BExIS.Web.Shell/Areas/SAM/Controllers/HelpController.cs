using System;
using System.IO;
using System.Net;
using System.Web.Mvc;
using BExIS.Modules.Sam.UI.Models;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Administration Manual", this.Session.GetTenant());
            return View();

        }
    }
}