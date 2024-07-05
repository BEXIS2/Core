using System;
using System.IO;
using System.Linq;
using System.Web;

// This is needed for the DependencyResolver...wish they would've just used Common Service Locator!
using Vaiona.Persistence.Api;

namespace Vaiona.Web.HttpModules
{
    /// <summary>
    /// inspired by http://nhforge.org/blogs/nhibernate/archive/2011/03/03/effective-nhibernate-session-management-for-web-apps.aspx
    /// </summary>
    public class SessionPerRequestModule : IHttpModule
    {
        private IPersistenceManager pManager = null;
        private static readonly string[] NoPersistenceFileExtensions = new string[] { ".jpg", ".gif", ".png", ".css", ".js", ".swf", ".xap" };

        public void Init(HttpApplication context)
        {
            context.BeginRequest += ContextBeginRequest;
            context.EndRequest += ContextEndRequest;
            context.Error += ContextError;
            //pManager = PersistenceFactory.GetPersistenceManager();
        }

        private void ContextBeginRequest(object sender, EventArgs e)
        {
            if (isStatic(sender as HttpApplication))
            {
                return;
            }
            //pManager.StartConversation();
        }

        private void ContextEndRequest(object sender, EventArgs e)
        {
            if (isStatic(sender as HttpApplication))
            {
                return;
            }
            //pManager.ShutdownConversation();
        }

        private void ContextError(object sender, EventArgs e)
        {
            //pManager.EndContext();
        }

        public void Dispose()
        { }

        private static bool isStatic(HttpApplication application)
        {
            if (application == null || application.Context == null)
            {
                return true;
            }
            string fileExtension = Path.GetExtension(application.Context.Request.PhysicalPath);
            return fileExtension == null || NoPersistenceFileExtensions.Contains(fileExtension.ToLower());
        }
    }
}