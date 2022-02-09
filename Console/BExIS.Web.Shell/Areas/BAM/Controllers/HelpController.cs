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
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Bam.UI.Controllers
{
    public class HelpController : Controller
    {
        // GET: Help
        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("BAM").GetEntryValue("help").ToString();

            return Redirect(helpurl);
        }
    }
}