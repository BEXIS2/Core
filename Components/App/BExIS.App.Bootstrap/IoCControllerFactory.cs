using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.IoC;

namespace BExIS.App.Bootstrap
{
    public class IoCControllerFactory : DefaultControllerFactory
    {
        protected override IController GetControllerInstance(
            RequestContext requestContext,
            Type controllerType)
        {
            if (controllerType == null)
                return null;

            try
            {
                // 👉 HIER wird IoC genutzt
                return (IController)IoCFactory.Container.Resolve(controllerType);
            }
            catch
            {
                // Fallback auf Default
                return base.GetControllerInstance(requestContext, controllerType);
            }
        }
    }
}
