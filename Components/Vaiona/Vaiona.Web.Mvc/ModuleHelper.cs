using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Vaiona.Web.Mvc
{
    public static class ModuleHelper
    {
        public static Assembly ApplicationAssembly = null;

        public static void Init(HttpApplication app, Assembly applicationAssembly)
        {
            ApplicationAssembly = applicationAssembly;
        }

        public static readonly List<AreaRegistration> Areas = new List<AreaRegistration>();

        public static void LoadModules()
        {
        }

        public static void RegisterAreaInfo(AreaRegistration registrationInfo)
        {
            if (!Areas.Contains(registrationInfo))
                Areas.Add(registrationInfo);
        }

        public static List<string> GetAreaNames()
        {
            return (ModuleHelper.Areas.Select(p => p.AreaName).ToList());
        }

        public static List<Type> GetControllersByArea(AreaRegistration area)
        {
            //AreaRegistration area = Areas.Where(a => a.AreaName.Equals(areaName)).FirstOrDefault();
            List<Type> controllers = GetControllers();
            //controllers = controllers.Where(t => t.GetCustomAttributes(typeof(AreaAttribute), true).Count() > 0).ToList();
            controllers = (from c in controllers
                           from a in c.GetCustomAttributes(typeof(AreaAttribute), true).Cast<AreaAttribute>()
                           where (a.Registration.Equals(area.GetType()))
                           //&& (c.GetCustomAttributes(typeof(Display))
                           select c).ToList();
            return (controllers);
        }

        public static List<MethodInfo> GetActionsByController(Type controllerType)
        {
            List<MethodInfo> actions = controllerType.GetMethods().Where(p => p.IsPublic).ToList();
            actions = (from a in actions
                           //from ano in a.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Cast<AreaAttribute>() // should be done in the security package
                       where (typeof(ActionResult).IsAssignableFrom(a.ReturnType))
                       select a
                       ).ToList();
            return (actions);
        }

        public static List<Type> GetControllers()
        {
            List<Type> controllers =
            ModuleHelper.ApplicationAssembly.GetTypes() // it should be replaced with assemblies of modules
                    .Where(t =>
                        t != null
                        && t.IsPublic &&
                        t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)
                        && !t.IsAbstract
                        && typeof(IController).IsAssignableFrom(t)
                    ).ToList();
            return (controllers);
        }
    }
}