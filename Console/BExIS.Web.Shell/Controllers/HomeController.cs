using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using BExIS.Dlm.Entities.Data;
using System.Xml;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Core.Serialization;
using System.Xml.Serialization;
using BExIS.Core.Persistence.Api;
using BExIS.Core.IoC;
using BExIS.Core.Util.Cfg;
using BExIS.Core.UI;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Data;
using System.IO;

namespace BExIS.Web.Shell.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
