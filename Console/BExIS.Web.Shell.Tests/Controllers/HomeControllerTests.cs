﻿using BExIS.App.Testing;
using BExIS.Utils.Config;
using FluentAssertions;
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

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            helper.Dispose();
        }

        [Test()]
        public void IndexTest()
        {
            HomeController controller = new HomeController();
            controller.ControllerContext = helper.BuildHttpContext("javad");
            ViewResult result = controller.Index() as ViewResult;
            
            result.Should().NotBeNull(); // Assert.That(result, Is.Not.Null);

            string title = result.ViewBag.Title;
            title.Should().NotBeNullOrWhiteSpace(); // Assert.That(result.ViewBag.Title, Is.Not.Null);

            string tenantName = controller.ControllerContext.HttpContext.Session.GetTenant().ShortName;
            title.Should().Contain(tenantName); // Assert.That(result.ViewBag.Title.Contains(tenantName));
        }

        [Test()]
        public void SessionTimeoutTest()
        {
            HomeController controller = new HomeController();
            controller.ControllerContext = helper.BuildHttpContext();
            ViewResult result = controller.SessionTimeout() as ViewResult;

            result.Should().NotBeNull(); // Assert.That(result, Is.Not.Null);

            string title = result.ViewBag.Title;
            title.Should().NotBeNullOrWhiteSpace(); // Assert.That(result.ViewBag.Title, Is.Not.Null);

            string tenantName = controller.ControllerContext.HttpContext.Session.GetTenant().ShortName;
            title.Should().Contain(tenantName); // Assert.That(result.ViewBag.Title.Contains(tenantName));
        }
    }
}