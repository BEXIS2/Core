using Microsoft.Practices.Unity;
using System;

namespace Vaiona.IoC.Unity
{
    public class UnityPerRequestLifetimeManager : LifetimeManager
    {
        private readonly string key;

        public UnityPerRequestLifetimeManager(string key)
        {
            this.key = key;
        }

        public UnityPerRequestLifetimeManager(Type key)
        {
            this.key = key.FullName;
        }

        public UnityPerRequestLifetimeManager()
        {
            this.key = new object().GetHashCode().ToString();
        }

        public override object GetValue()
        {
            return UnityPerRequestHttpModule.GetValue(key);
        }

        public override void RemoveValue()
        {
            var disposable = GetValue() as IDisposable;
            disposable?.Dispose();
            UnityPerRequestHttpModule.SetValue(key, null);
        }

        public override void SetValue(object newValue)
        {
            UnityPerRequestHttpModule.SetValue(key, newValue);
        }
    }
}