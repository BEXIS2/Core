using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Security;
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

            publics.Add("Auth.Account");
            publics.Add("Site.Nav");
            publics.Add("Shell.Home");

            if (!publics.Contains(areaName + "." + controllerName))
            {
                TaskContext taskContext = new TaskContext()
                {
                    AreaName = areaName,
                    ControllerName = controllerName,
                    ActionName = "*"
                };

                UserManager userManager = new UserManager();
                User user = userManager.GetUserByName(userName);

                if (user == null)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    SecurityService securityService = new SecurityService();
                    securityService.HasTaskAccess(user, taskContext);
                }
            }
        }
    }
}
