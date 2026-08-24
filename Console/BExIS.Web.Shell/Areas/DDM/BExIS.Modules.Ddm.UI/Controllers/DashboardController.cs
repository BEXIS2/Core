using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.Utils.Models;
using BExIS.Xml.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Vaiona.Persistence.Api;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class DashboardController : Controller
    {
        private readonly UserManager<User, long> _userManager;
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        public DashboardController(UserManager<User, long> userManager)
        {
            _userManager = userManager;
        }

        public ActionResult Index()
        {
            string module = "DDM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            // load settings
            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            ViewData["use_tags"] = moduleSettings.GetValueByKey("use_tags");

            return View();
        }
    }
         
}