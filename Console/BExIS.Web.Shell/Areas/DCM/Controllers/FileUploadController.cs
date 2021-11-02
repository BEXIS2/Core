using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class FileUploadController : Controller
    {
        /// <summary>
        /// entry for hook
        /// </summary>
        /// <returns></returns>
        public ActionResult Start()
        {
            //return View();
            throw new NotImplementedException();
        }

        // GET: FileUpload
        public ActionResult Index()
        {
            return View();
        }
    }
}