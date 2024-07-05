using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using System;

namespace BExIS.Ext.Services
{
    public static class AuthorizationDelegationImplementor
    {
        public static void CheckAuthorization(string areaName, string controllerName, string actionName, string username, bool isAuthenticated)
        {
            // validate the call using the extensibility information (modules, tasks, actions, etc)
            // Call security authorization api utilizing the IoC, Singleton lifetime
            //throw an exception based on the result

            // Ask for specific URLs (LogOn, Register, ...)

            using (var operationManager = new OperationManager())
            using (var featurePermissionManager = new FeaturePermissionManager())
            using (var userManager = new UserManager())
            {
                var operation = operationManager.Find(areaName, controllerName, "*");
                if (operation == null)
                {
                    throw new UnauthorizedAccessException();
                }

                var feature = operation.Feature;

                if (feature == null) return;

                var result = userManager.FindByNameAsync(username);
                if (!featurePermissionManager.HasAccess(result.Result?.Id, feature.Id))
                {
                    throw new UnauthorizedAccessException();
                }
            }
        }
    }
}