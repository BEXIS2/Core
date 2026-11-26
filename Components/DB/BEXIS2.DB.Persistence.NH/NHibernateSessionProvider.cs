using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace BEXIS2.DB.Persistence.NH
{
    public interface INHibernateSessionProvider
    {
        ISession GetSession(ISessionFactory sessionFactory);

        void Bind(
            ISessionFactory sessionFactory,
            Lazy<ISession> session);

        ISession Unbind(ISessionFactory sessionFactory);
    }

    public sealed class NHibernateSessionProvider : INHibernateSessionProvider
    {
        private static readonly AsyncLocal<ConcurrentDictionary<ISessionFactory, Lazy<ISession>>> Current =
            new AsyncLocal<ConcurrentDictionary<ISessionFactory, Lazy<ISession>>>();

        private static ConcurrentDictionary<ISessionFactory, Lazy<ISession>> Sessions
        {
            get
            {
                var sessions = Current.Value;

                if (sessions == null)
                {
                    sessions = new ConcurrentDictionary<ISessionFactory, Lazy<ISession>>();
                    Current.Value = sessions;
                }

                return sessions;
            }
        }

        public ISession GetSession(ISessionFactory sessionFactory)
        {
            if (sessionFactory == null)
                throw new ArgumentNullException(nameof(sessionFactory));

            var lazySession = Sessions.GetOrAdd(
                sessionFactory,
                factory => new Lazy<ISession>(() => factory.OpenSession()));

            return lazySession.Value;
        }

        public void Bind(
            ISessionFactory sessionFactory,
            Lazy<ISession> session)
        {
            if (sessionFactory == null)
                throw new ArgumentNullException(nameof(sessionFactory));

            if (session == null)
                throw new ArgumentNullException(nameof(session));

            Sessions[sessionFactory] = session;
        }

        public ISession Unbind(ISessionFactory sessionFactory)
        {
            if (sessionFactory == null)
                throw new ArgumentNullException(nameof(sessionFactory));

            Lazy<ISession> lazySession;

            if (!Sessions.TryRemove(sessionFactory, out lazySession))
                return null;

            return lazySession.IsValueCreated
                ? lazySession.Value
                : null;
        }
    }

}