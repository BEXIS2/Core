using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Util.Cfg;
using BExIS.Core.IoC.Unity;

namespace BExIS.Core.IoC
{
    public static class IoCFactory
    {
        private static IoCContainer container = null;

        public static void StartContainer(string configFilePath, string containerName, params object[] optionals)
        {
            if (container == null)
            {
                string typeName = AppConfiguration.IoCProviderTypeInfo;
                Type concreteIoCType = typeof(UnityIoC);
                try
                {
                    concreteIoCType = Type.GetType(typeName);
                }
                catch {} // use the default IoC
                container = Activator.CreateInstance(concreteIoCType, configFilePath, containerName, optionals) as IoCContainer;
            }
            else
                throw new InvalidOperationException("The IoC container is already loaded. Destroy it first if you want ro reconfigure it");
            // Registrations
            //container.RegisterType<myFinanceData, myFinanceData>(new HttpContextLifetimeManager<myFinanceData>());
            //ControllerBuilder.Current.SetControllerFactory(new Vaiona.Web.Controllers.UnityControllerFactory(container));
        }

        public static void ShutdownContainer()
        {
            container = null;
        }

        public static IoCContainer Container
        {
            get
            {
                return (container);
            }
        }

    }
}
