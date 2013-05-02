using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.Web;

namespace BExIS.Core.IoC.Unity
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
