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
            // Fall 1: Controller-Typ wurde gar nicht gefunden (z.B. /dasisteintest)
            if (controllerType == null)
            {
                return null;                    // → MVC zeigt automatisch 404
            }

            try
            {
                // Versuch über Unity aufzulösen
                var controller = IoCFactory.Container.Resolve(controllerType) as IController;

                if (controller == null)
                {
                    // Sollte eigentlich nie passieren, aber sicher ist sicher
                    return null;
                }

                return controller;
            }
            catch (Exception ex)
            {
                // WICHTIG: Hier NICHT mehr throwen!
                // Stattdessen null zurückgeben → MVC behandelt das als 404

                // Optional: Loggen des Fehlers (sehr empfohlen!)
                // Vaiona.Logging.ILogger.LogError(ex, $"Controller resolution failed for type: {controllerType.FullName}");

                return null;
            }
        }
    }
}