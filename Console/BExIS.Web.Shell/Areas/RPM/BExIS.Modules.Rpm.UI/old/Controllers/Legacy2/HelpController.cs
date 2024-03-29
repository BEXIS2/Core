﻿using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Utils.Helpers;
using BExIS.Xml.Helpers;
using System;
using System.IO;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using Vaiona.Utils.Cfg;
using System.IO;
using System.Xml.Linq;
using BExIS.Xml.Helpers;
using BExIS.UI.Helpers;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class HelpController : Controller
    {
        //
        // GET: /DDM/Help/

        public ActionResult Index()
        {
            string helpurl = ModuleManager.GetModuleSettings("RPM").GetEntryValue("help").ToString();

            //add default link if not set
            if (String.IsNullOrEmpty(helpurl))
            {
                helpurl = ManualHelper.GetUrl(AppConfiguration.ApplicationVersion, "DCM");
            }

            return Redirect(helpurl);
        }
    }
}