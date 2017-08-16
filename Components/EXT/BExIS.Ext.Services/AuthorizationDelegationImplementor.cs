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

            var operationManager = new OperationManager();

            var operation = operationManager.Find(areaName, controllerName, "*");
            if (operation == null)
            {
                throw new UnauthorizedAccessException();
            }

            var feature = operation.Feature;

            if (feature == null) return;

            var userManager = new UserManager(new UserStore());
            var result = userManager.FindByNameAsync(username);

            var featurePermissionManager = new FeaturePermissionManager();
            if (!featurePermissionManager.HasAccess(result.Result, feature))
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}
