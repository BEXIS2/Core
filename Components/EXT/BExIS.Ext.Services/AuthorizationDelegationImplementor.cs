using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;

namespace BExIS.Ext.Services
{
    public static class AuthorizationDelegationImplementor
    {
        public static void CheckAuthorization(string areaName, string controllerName, string actionName, string userName, bool isAuthenticated)
        {
            // validate the call using the extensibility information (modules, tasks, actions, etc)
            // Call security authorization api utilizing the IoC, Singleton lifetime
            //throw an exception based on the result

            // Ask for specific URLs (LogOn, Register, ...)

            List<string> publics = new List<string>();

            publics.Add("auth.account");
            publics.Add("site.nav");
            publics.Add("shell.home");
            publics.Add("system.utils"); 

            if (!publics.Contains(areaName.ToLower() + "." + controllerName.ToLower()) && !publics.Contains(areaName.ToLower() + "." + controllerName.ToLower() + "." + actionName.ToLower()))
            {
                if (string.IsNullOrWhiteSpace(userName) || !isAuthenticated)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    PermissionManager permissionManager = new PermissionManager();

                    if (!permissionManager.CheckFeatureAccessForUser(userName, areaName, controllerName, "*"))
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
            }
        }
    }
}
