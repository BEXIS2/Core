using BExIS.App.Bootstrap;
using BExIS.UI.Helpers;
using BExIS.Utils;
using BExIS.Utils.Config;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Vaiona.IoC;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult FAQ()
        {
            
            string helpurl = GeneralSettings.FAQ.ToString();

            return Redirect(helpurl);
        }
    }
}