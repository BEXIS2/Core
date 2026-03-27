using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Vaiona.IoC;

namespace BExIS.App.Bootstrap
{
    public class UnityHttpControllerActivator : IHttpControllerActivator
    {
        public IHttpController Create(
            HttpRequestMessage request,
            HttpControllerDescriptor controllerDescriptor,
            Type controllerType)
        {
            if (controllerType == null)
                throw new ArgumentNullException(nameof(controllerType));

            try
            {
                // Hier wird der ApiController mit allen Dependencies über Unity aufgelöst
                var controller = IoCFactory.Container.Resolve(controllerType) as IHttpController;

                if (controller == null)
                {
                    throw new InvalidOperationException(
                        $"Unity konnte keinen Instanz für ApiController {controllerType.FullName} erstellen.");
                }

                return controller;
            }
            catch (Exception ex)
            {
                // Optional: Logging
                // Vaiona.Logging.ILogger.LogError(ex, $"ApiController activation failed: {controllerType.FullName}");

                // Saubere Fehlerantwort für die API
                throw new HttpResponseException(
                    request.CreateErrorResponse(
                        System.Net.HttpStatusCode.InternalServerError,
                        $"Fehler beim Erstellen des Controllers {controllerType.Name}: {ex.Message}"));
            }
        }
    }
}
