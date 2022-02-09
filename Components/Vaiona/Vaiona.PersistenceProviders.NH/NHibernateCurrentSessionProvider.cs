using System;
using System.Collections.Generic;
using System.Web;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;
using System.Runtime.Remoting.Messaging;
using Vaiona.Utils.Cfg;
using Vaiona.IoC;
using Vaiona.Persistence.Api;

namespace Vaiona.PersistenceProviders.NH
{
    /// <summary>
    /// Taken from http://nhforge.org/blogs/nhibernate/archive/2011/03/03/effective-nhibernate-session-management-for-web-apps.aspx
    /// </summary>
    public class NHibernateCurrentSessionProvider : ICurrentSessionContext
    {
        private readonly ISessionFactoryImplementor _factory;
        public const string CURRENT_SESSION_CONTEXT_KEY = "NHibernateCurrentSessionFactory";

        public NHibernateCurrentSessionProvider(ISessionFactoryImplementor factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Retrieve the current session for the session factory.
        /// </summary>
        /// <returns></returns>
        public ISession CurrentSession() {
            ISession session;
            session = getPerHttpRequestSession();
            //var currentSessionFactoryMap = GetCurrentFactoryMap();
            
            //if (currentSessionFactoryMap == null || !currentSessionFactoryMap.TryGetValue(_factory, out session)) {
            //    return null;
            //}
            return (session);
        }

        private ISession getPerHttpRequestSession()
        {
            ISession session = null;
            ISessionProvider provider = IoCFactory.Container.ResolveForRequest<ISessionProvider>(); // returns a session per http request.
            if (provider != null) // when no http request is available, it may return null
            {
                session = (ISession)provider.getSession();
            }
            return session;
        }
    }
}