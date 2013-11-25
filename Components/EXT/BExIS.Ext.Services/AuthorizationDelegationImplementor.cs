using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Ext.Services
{
    public static class AuthorizationDelegationImplementor
    {
        public static void CheckAuthorization(string areaName, string controllerName, string actionName, string userName, bool isAuthenticated)
        {
            // validate the call using the extensibility information (modules, tasks, actions, etc)
            // Call security authorization api utilizing the IoC, Singleton lifetime
            //throw an exception based on the result
        }
    }
}
