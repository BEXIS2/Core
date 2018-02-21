using BExIS.App.Testing;
using BExIS.Utils.Config;
using NUnit.Framework;
using System.Web.Mvc;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Controllers.Tests
{
    [TestFixture()]
    public class HomeControllerTests
    {
        private TestSetupHelper helper = null;
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
        }

        [Test()]
        public void IndexTest()
        {
            HomeController controller = new HomeController();
            controller.ControllerContext = helper.BuildHttpContext();
            ViewResult result = controller.Index() as ViewResult;

            Assert.That(result, Is.Not.Null);

            Assert.That(result.ViewBag.Title, Is.Not.Null);

            string tenantName = controller.ControllerContext.HttpContext.Session.GetTenant().ShortName;
            Assert.That(result.ViewBag.Title.Contains(tenantName));
        }

        [Test()]
        public void SessionTimeoutTest()
        {
            HomeController controller = new HomeController();
            controller.ControllerContext = helper.BuildHttpContext();
            ViewResult result = controller.SessionTimeout() as ViewResult;

            Assert.That(result, Is.Not.Null);

            Assert.That(result.ViewBag.Title, Is.Not.Null);

            string tenantName = controller.ControllerContext.HttpContext.Session.GetTenant().ShortName;
            Assert.That(result.ViewBag.Title.Contains(tenantName));
        }
    }
}