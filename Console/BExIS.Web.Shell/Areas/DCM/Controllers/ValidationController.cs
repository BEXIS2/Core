using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ValidationController : Controller
    {
        // GET: Validation
        public ActionResult Index()
        {
            return View();
        }

        // GET: Validation
        public ActionResult Start(long id, int version)
        {
            throw new NotImplementedException();
            //return View();
        }
    }
}