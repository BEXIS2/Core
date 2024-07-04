using NHibernate;
using NHibernate.Cfg;
using System;
using System.Diagnostics;
using Vaiona.IoC;
using Vaiona.Persistence.Api;

namespace Vaiona.PersistenceProviders.NH
{
    public class NHibernateSessionProvider : ISessionProvider, IDisposable
    {
        private ISession session;
        private NHibernatePersistenceManager pManager = null;

        public NHibernateSessionProvider()
        {
            pManager = (NHibernatePersistenceManager)IoCFactory.Container.Resolve<IPersistenceManager>();
            ISessionFactory sessionFactory = (ISessionFactory)pManager.Factory;
            Configuration cf = (Configuration)pManager.Configuration;
            session = sessionFactory.OpenSession(cf.Interceptor);
        }

        public object getSession()
        {
            return session;
        }

        public void Dispose()
        {
            if (session.IsOpen)
                session.Close();
            if (pManager.ShowQueries) // do this before disposing the session and setting it to null
                Trace.WriteLine("SQL output at:" + DateTime.Now.ToString() + "--> " + "A conversation was closed. ID: " + session.GetHashCode());
            session.Dispose();
            session = null;
            // GC.Collect();
        }
    }
}