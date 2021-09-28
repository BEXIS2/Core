using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using System.IO;
using System.Xml.Linq;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;
using BExIS.Dlm.Services.Data;
using BExIS.UI.Helpers;

namespace BExIS.Modules.Bam.UI.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult Index()
        {
            SettingsHelper settingsHelper = new SettingsHelper("BAM");
            string helpurl = settingsHelper.GetValue("help");

            return Redirect(helpurl);

        }
    }
}