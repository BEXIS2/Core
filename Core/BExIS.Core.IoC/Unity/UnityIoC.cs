using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.Configuration;
using Microsoft.Practices.Unity.Configuration;
using System.Web;

namespace BExIS.Core.IoC.Unity
{
    public class UnityIoC : IoCContainer
    {
        IUnityContainer container = null;

        public UnityIoC(IUnityContainer container)
        {
            this.container = container;
        }

        public UnityIoC(string configFilePath, string containerName, params object[] optionals)
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = configFilePath;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            UnityConfigurationSection section = (UnityConfigurationSection)config.GetSection("unity");

            container = new UnityContainer();
            container.LoadConfiguration(section, containerName);

            //container = new UnityContainer();
            //container.LoadConfiguration(containerName);
        }

        public object Resolve<T>()
        {
            return (container.Resolve<T>());
        }

        public object Resolve(Type t)
        {
            return (container.Resolve(t));
        }

        public IoCContainer CreateSessionLevelContainer()
        {
            UnityIoC ioc = new UnityIoC(container.CreateChildContainer());
            return (ioc);
        }

        public object ResolveForSession<T>()
        {
            object o = (HttpContext.Current.Session["SessionLevelContainer"] as IoCContainer).Resolve<T>();
            return (o);
        }

        public object ResolveForSession(Type t)
        {
            object o = (HttpContext.Current.Session["SessionLevelContainer"] as IoCContainer).Resolve(t);
            return (o);

        }

    }
}
