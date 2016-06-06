using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;
using System.IO;
using BExIS.IO.Transform.Input;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Areas.Site.Controllers
{
    public class ImpressumController : Controller
    {
        // GET: Site/PrivacyPolicy
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Imprint", this.Session.GetTenant());
            return View();
        }

    }
}