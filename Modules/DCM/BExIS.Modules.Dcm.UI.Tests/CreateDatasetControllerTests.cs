using BExIS.App.Testing;
using BExIS.Modules.Dcm.UI.Controllers;
using BExIS.Utils.Config;
using FluentAssertions;
using NUnit.Framework;
using System.Web.Mvc;
using Vaiona.Web.Extensions;

namespace BExIS.Modules.Dcm.UI.Tests
{
    [TestFixture()]
    public class CreateDatasetControllerTests
    {
        private TestSetupHelper helper = null;
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            helper = new TestSetupHelper(WebApiConfig.Register, false);
            
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper.Dispose();
        }

        [Test()]
        public void IndexTest()
        {
            //CreateDatasetController controller = new CreateDatasetController();
            //controller.ControllerContext = helper.BuildHttpContext("david");
            //CreateDatasetController result = controller.Index() as ViewResult;

            //result.Should().NotBeNull(); // Assert.That(result, Is.Not.Null);

            //string title = result.ViewBag.Title;
            //title.Should().NotBeNullOrWhiteSpace(); // Assert.That(result.ViewBag.Title, Is.Not.Null);

            //string tenantName = controller.ControllerContext.HttpContext.Session.GetTenant().ShortName;
            //title.Should().Contain(tenantName); // Assert.That(result.ViewBag.Title.Contains(tenantName));
        }


        //ToDo Create test with index parameters, for datasets, metadata structures and data structures

    }
}
