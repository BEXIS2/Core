using System;
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
                var controller = (IController)IoCFactory.Container.Resolve(controllerType);

                // Optional: Loggen, falls du willst
                // Vaiona.Logging.ILogger.Log($"Resolved controller: {controllerType.FullName}");

                return controller;
            }
            catch (Exception ex)
            {
                // ← Hier siehst du jetzt den echten Fehler!
                // z. B. "Unity cannot resolve type SignInManager" oder "IAuthenticationManager not registered"
                throw new InvalidOperationException(
                    $"Unity konnte Controller {controllerType.FullName} nicht erstellen.", ex);
            }
        }
    }
}