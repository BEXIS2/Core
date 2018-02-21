using BExIS.App.Bootstrap;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Vaiona.IoC;
using Vaiona.MultiTenancy.Api;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.App.Testing
{
    public class TestSetupHelper
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
            if (!app.Started)
                throw new System.InvalidOperationException("The test environmnet has not been started yet. call app.Start(...) before calling this method.");

            var httpCtxMock = new Mock<HttpContextBase>();
            var httpSessionMock = new Mock<HttpSessionStateBase>();

            ITenantResolver tenantResolver = IoCFactory.Container.Resolve<ITenantResolver>();

            httpSessionMock.Setup(x => x["CurrentTenant"]).Returns(tenantResolver.DefaultTenant);
            httpCtxMock.Setup(ctx => ctx.Session).Returns(httpSessionMock.Object);
            ControllerContext controllerCtx = new ControllerContext();
            controllerCtx.HttpContext = httpCtxMock.Object;
            return controllerCtx;
        }
    }
}
