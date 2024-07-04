using Microsoft.Practices.Unity;
using System.Web;

namespace Vaiona.IoC.Unity
{
    public class UnityPerSessionLifetimeManager : LifetimeManager
    {
        private string sessionKey;

        public UnityPerSessionLifetimeManager(string sessionKey)
        {
            this.sessionKey = sessionKey;
        }

        public override object GetValue()
        {
            return HttpContext.Current.Session[this.sessionKey];
        }

        public override void RemoveValue()
        {
            HttpContext.Current.Session.Remove(this.sessionKey);
        }

        public override void SetValue(object newValue)
        {
            HttpContext.Current.Session[this.sessionKey] = newValue;
        }
    }
}