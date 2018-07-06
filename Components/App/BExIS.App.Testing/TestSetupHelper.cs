using BExIS.App.Bootstrap;
using Moq;
using System;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Vaiona.IoC;
using Vaiona.MultiTenancy.Api;

namespace BExIS.App.Testing
{
    public class TestSetupHelper : IDisposable
    {
        protected Application app = null;

        public Application AppContext { get { return app; } }

        public TestSetupHelper(Action<HttpConfiguration> configurationCallback, bool configureModules)
        {
            app = new Application(RunStage.Test);
            app.Start(configurationCallback, configureModules);
        }

        public ControllerContext BuildHttpContext()
        {
            return this.BuildHttpContext(null);
        }

        /// <summary>
        /// creates a httpcontxt with an authenticated user
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ControllerContext BuildHttpContext(string userName)
        {
            if (!app.Started)
                throw new System.InvalidOperationException("The test environmnet has not been started yet. call app.Start(...) before calling this method.");

            var httpCtxMock = new Mock<HttpContextBase>();

            ITenantResolver tenantResolver = IoCFactory.Container.Resolve<ITenantResolver>();

            var httpSessionMock = new Mock<HttpSessionStateBase>();
            httpSessionMock.Setup(x => x["CurrentTenant"]).Returns(tenantResolver.DefaultTenant);
            httpCtxMock.Setup(ctx => ctx.Session).Returns(httpSessionMock.Object);

            if (!string.IsNullOrWhiteSpace(userName))
            {
                var validPrincipal = new ClaimsPrincipal(
                   new[]
                   {
                        new ClaimsIdentity(
                            new[] {new Claim(ClaimTypes.NameIdentifier, userName)})
                   });
                httpCtxMock.Setup(ctx => ctx.User).Returns(validPrincipal);
            }

            ControllerContext controllerCtx = new ControllerContext();
            controllerCtx.HttpContext = httpCtxMock.Object;
            return controllerCtx;
        }

        public void Dispose()
        {
            app.Stop();
        }
    }
}
