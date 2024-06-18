using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vaiona.IoC.Unity
{
    public class UnityPerRequestHttpModule : IHttpModule
    {
        private static readonly string IoCPerRequestKey = "IoCRerRequestKey_";

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="HttpApplication"/> that provides access to the methods, properties,
        /// and events common to all application objects within an ASP.NET application.</param>
        public void Init(HttpApplication context)
        {
            if (context != null)
            {
                context.EndRequest += onEndRequest;
                context.BeginRequest += onBeginRequest;
                //context.Error += onError;
            }
            else
            {
                throw new ArgumentNullException(nameof(context));
            }
        }

        public static object GetValueFromContext(string key)
        {
            if (HttpContext.Current != null &&
                HttpContext.Current.Items.Contains(key))
                return HttpContext.Current.Items[key];
            else
                return null;
        }

        public static void AddToContext(string key, object value)
        {
            if (HttpContext.Current != null)
                HttpContext.Current.Items[key] = value;
        }

        public static void RemoveFromContext(string key)
        {
            if (HttpContext.Current != null)
                HttpContext.Current.Items.Remove(key);
        }

        private void onError(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void onBeginRequest(object sender, EventArgs e)
        {
            IoCFactory.Container.StartRequestLevelContainer();
        }

        private void onEndRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;

            var dict = getDictionary(app.Context);

            if (dict != null)
            {
                var keys = dict.Keys.ToList();
                foreach (var key in keys)
                {
                    var value = dict[key];
                    if (value is IDisposable)
                        ((IDisposable)value).Dispose();
                    dict.Remove(key);
                }
                app.Context.Items.Remove(IoCPerRequestKey);
            }
            IoCFactory.Container.ShutdownRequestLevelContainer();
        }

        public static object GetValue(object key)
        {
            var dict = getDictionary(HttpContext.Current);

            if (dict != null)
            {
                object obj = null;
                if (dict.TryGetValue(key, out obj))
                {
                    return obj;
                }
            }
            return null;
        }

        public static void SetValue(object key, object value)
        {
            if (key == null || value == null)
                return;

            var dict = getDictionary(HttpContext.Current);
            if (dict == null)
            {
                dict = new Dictionary<object, object>();
                HttpContext.Current.Items[IoCPerRequestKey] = dict;
            }

            dict[key] = value;
        }

        /// <summary>
        /// Disposes the resources used by this module.
        /// </summary>
        public void Dispose()
        {
        }

        private static Dictionary<object, object> getDictionary(HttpContext context)
        {
            if (context == null)
            {
                throw new InvalidOperationException(
                    "The PerRequestLifetimeManager can only be used in the context of an HTTP request. Possible causes for this error are using the lifetime manager on a non-ASP.NET application, or using it in a thread that is not associated with the appropriate synchronization context.");
            }

            var dict = (Dictionary<object, object>)context.Items[IoCPerRequestKey];

            return dict;
        }
    }
}