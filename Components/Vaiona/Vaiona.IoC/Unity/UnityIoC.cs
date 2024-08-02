using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;

namespace Vaiona.IoC.Unity
{
    public class UnityIoC : IoCContainer
    {
        private IUnityContainer container = null;
        private Dictionary<string, UnityIoC> children = new Dictionary<string, UnityIoC>();
        private Dictionary<Type, object> instances = new Dictionary<Type, object>();

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
        }

        public void RegisterHeirarchical(Type from, Type to)
        {
            this.container.RegisterType(from, to, new HierarchicalLifetimeManager());
        }

        public void Register(Type from, Type to)
        {
            this.container.RegisterType(from, to, new TransientLifetimeManager());
        }

        public bool IsRegistered(Type t, string name)
        {
            return container.IsRegistered(t, name);
        }

        public object Resolve(Type t)
        {
            return (container.Resolve(t));
        }

        public T Resolve<T>()
        {
            return (container.Resolve<T>());
        }

        public T Resolve<T>(string name)
        {
            return (container.Resolve<T>(name));
        }

        public bool IsRegistered<T>(string name)
        {
            return container.IsRegistered<T>(name);
        }

        public void RegisterPerRequest(Type from, Type to)
        {
            this.container.RegisterType(from, to, new UnityPerRequestLifetimeManager(from));
        }

        private static readonly string IoCContainerPerRequestKey = "IoCRerRequestKey_Container";

        public void StartRequestLevelContainer()
        {
            string key = IoCContainerPerRequestKey;
            UnityIoC child = new UnityIoC(container.CreateChildContainer());
            UnityPerRequestHttpModule.AddToContext(key, child);
        }

        public void ShutdownRequestLevelContainer()
        {
            try
            {
                string key = IoCContainerPerRequestKey;
                UnityPerRequestHttpModule.RemoveFromContext(key);
            }
            catch { }
        }

        public T ResolveForRequest<T>()
        {
            try
            {
                string key = IoCContainerPerRequestKey;
                UnityIoC child = (UnityIoC)UnityPerRequestHttpModule.GetValueFromContext(key);
                if (child != null)
                {
                    T o = child.container.Resolve<T>();
                    return (o);
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }

            return default(T);
        }

        public object ResolveForRequest(Type t)
        {
            string key = IoCContainerPerRequestKey;
            UnityIoC child = (UnityIoC)UnityPerRequestHttpModule.GetValueFromContext(key);
            if (child != null)
            {
                object o = child.Resolve(t);
                return (o);
            }
            return null;
        }

        public void StartSessionLevelContainer()
        {
            string key = HttpContext.Current?.Session.SessionID;
            if (!children.ContainsKey(key))
            {
                UnityIoC child = new UnityIoC(container.CreateChildContainer());
                children.Add(key, child);
            }
        }

        public void ShutdownSessionLevelContainer()
        {
            try
            {
                string key = HttpContext.Current?.Session.SessionID;
                if (!string.IsNullOrEmpty(key) && this.children.ContainsKey(key))
                    children.Remove(key);
            }
            catch { }
        }

        public T ResolveForSession<T>()
        {
            try
            {
                string key = HttpContext.Current?.Session.SessionID;
                if (this.children.ContainsKey(key))
                {
                    UnityIoC child = this.children[key];
                    T o = child.container.Resolve<T>();
                    return (o);
                }
            }
            catch (Exception ex)
            {
                return default(T);
            }

            return default(T);
        }

        public object ResolveForSession(Type t)
        {
            string key = HttpContext.Current.Session.SessionID;
            if (this.children.ContainsKey(key))
            {
                IoCContainer container = this.children[key];
                object o = container.Resolve(t);
                return (o);
            }
            return null;
        }

        public void Teardown(object obj)
        {
            container.Teardown(obj);
        }
    }
}