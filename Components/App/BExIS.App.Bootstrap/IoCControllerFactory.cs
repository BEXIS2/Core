using System;
using System.Web;
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
            // Fall 1: MVC konnte den Controller-Typ nicht finden (z. B. "wp-includes", "dasisteintest")
            if (controllerType == null)
            {
                // WICHTIG: HttpException 404 werfen → MVC behandelt das als 404
                throw new HttpException(404, "The requested controller was not found.");
            }

            try
            {
                // Versuch, den Controller über Unity aufzulösen
                var controller = IoCFactory.Container.Resolve(controllerType) as IController;

                if (controller == null)
                {
                    throw new HttpException(404, $"Could not create controller instance for type: {controllerType.FullName}");
                }

                return controller;
            }
            catch (Exception ex)
            {
                // Loggen (empfohlen)
                // Vaiona.Logging.ILogger.LogError(ex, $"Controller resolution failed for {controllerType.FullName}");

                // HttpException 404 werfen → sauberes 404 statt InvalidOperationException
                throw new HttpException(404,
                    $"The controller '{controllerType.Name}' could not be created.", ex);
            }
        }
    }
}