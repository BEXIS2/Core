using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using BExIS.Dlm.Entities.Data;
using System.Xml;
using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Core.Serialization;
using System.Xml.Serialization;
using Vaiona.Persistence.Api;
using Vaiona.IoC;
using Vaiona.Util.Cfg;
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
