using System;
using Vaiona.IoC.Unity;
using Vaiona.Utils.Cfg;

namespace Vaiona.IoC
{
    public static class IoCFactory
    {
        private static IoCContainer container = null;

        public static void StartContainer(string configFilePath, string containerName, params object[] optionals)
        {
            try
            {
                if (container == null)
                {
                    string typeName = AppConfiguration.IoCProviderTypeInfo;
                    Type concreteIoCType = typeof(UnityIoC);
                    try
                    {
                        concreteIoCType = Type.GetType(typeName);
                    }
                    catch
                    {
                        concreteIoCType = typeof(UnityIoC);
                    } // use the default IoC
                    container = Activator.CreateInstance(concreteIoCType, configFilePath, containerName, optionals) as IoCContainer;
                }
                else
                {
                    throw new System.TypeLoadException("The IoC container is already loaded. Destroy it first if you want or reconfigure it.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
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