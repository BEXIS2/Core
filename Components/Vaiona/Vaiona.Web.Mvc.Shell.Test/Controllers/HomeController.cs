using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Data;
using Vaiona.Entities.Logging;
using Vaiona.Logging;
using Vaiona.Logging.Aspects;
using Vaiona.MultiTenancy.Api;
using Vaiona.Web.Extensions;
using Vaiona.Persistence.Api;

namespace Vaiona.Web.Mvc.Shell.Test.Controllers
{
    public class HomeController : Controller
    {
        [DoesNotNeedDataAccess]
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome Tenant: " + Session.GetTenant().Id;
            //ITenantRegistrar tenantRegistrar = MultiTenantFactory.GetTenantRegistrar();
            //tenantRegistrar.Inactivate(Session.GetTenant().Id);
            //tenantRegistrar.Activate(Session.GetTenant().Id);

            //tenantRegistrar.MakeDefault("idiv");
            //tenantRegistrar.Unregister("idiv");

            //tenantRegistrar.MakeDefault("bexis");
            //tenantRegistrar.Unregister("idiv");
            //tenantRegistrar.Unregister("bexis");
            testUoW();
            return View();
        }

        private void testUoW()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetReadOnlyRepository<LogEntry>();
            }

            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetReadOnlyRepository<LogEntry>();
            }
        }

        [DoesNotNeedDataAccess]
        [RecordCall]
        public ActionResult Trace()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            return View();
        }

        [DoesNotNeedDataAccess]
        [MeasurePerformance]
        public ActionResult Perf()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            return View();
        }

        [DoesNotNeedDataAccess]
        [Diagnose]
        public ActionResult Diag(int id)
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            int x = MonitoredMethod("ABC", true);
            return View("Index");
        }

        [Diagnose]
        private int MonitoredMethod(string text, bool checkIt)
        {
            return (new Random()).Next();
        }

        [DoesNotNeedDataAccess]
        [LogExceptions]
        public ActionResult Ex()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            throw new Exception("I am an uncaught exception wanted to be logged. Hurray!");
            //return View();
        }

        [DoesNotNeedDataAccess]
        public ActionResult Custom()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            LoggerFactory.LogCustom("I am a custom message, please log me!");
            return View();
        }

        [DoesNotNeedDataAccess]
        public ActionResult About()
        {
            return View();
        }
    }
}
