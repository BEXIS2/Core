using NHibernate;
using NHibernate.Cfg;
using System;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace Vaiona.PersistenceProviders.NH
{
    public class NHibernateUnitOfWorkFactory : IUnitOfWorkFactory
    {
        private NHibernatePersistenceManager manager;
        private ISessionFactory sessionFactory;
        private Configuration config;

        public NHibernateUnitOfWorkFactory(NHibernatePersistenceManager pManager, Configuration cfg)
        {
            config = cfg;
            manager = pManager;
            sessionFactory = (ISessionFactory)manager.Factory;
        }

        public IUnitOfWork CreateUnitOfWork(bool autoCommit = false, bool throwExceptionOnError = true,
            EventHandler beforeCommit = null, EventHandler afterCommit = null, EventHandler beforeIgnore = null, EventHandler afterIgnore = null)
        {
            Conversation cnv = new Conversation(sessionFactory, config, TypeOfUnitOfWork.Normal, autoCommit, AppConfiguration.ShowQueries);
            NHibernateUnitOfWork u = new NHibernateUnitOfWork(manager, cnv, autoCommit, throwExceptionOnError);
            u.BeforeCommit += beforeCommit;
            u.AfterCommit += afterCommit;
            u.BeforeIgnore += beforeIgnore;
            u.AfterIgnore += afterIgnore;
            return (u);
        }

        public IUnitOfWork CreateIsolatedUnitOfWork(bool autoCommit = false, bool throwExceptionOnError = true,
            EventHandler beforeCommit = null, EventHandler afterCommit = null, EventHandler beforeIgnore = null, EventHandler afterIgnore = null)
        {
            Conversation cnv = new Conversation(sessionFactory, config, TypeOfUnitOfWork.Isolated, autoCommit, AppConfiguration.ShowQueries);
            NHibernateUnitOfWork u = new NHibernateUnitOfWork(manager, cnv, autoCommit, throwExceptionOnError);
            u.BeforeCommit += beforeCommit;
            u.AfterCommit += afterCommit;
            u.BeforeIgnore += beforeIgnore;
            u.AfterIgnore += afterIgnore;
            return (u);
        }

        public IUnitOfWork CreateBulkUnitOfWork(bool autoCommit = false, bool throwExceptionOnError = true,
            EventHandler beforeCommit = null, EventHandler afterCommit = null, EventHandler beforeIgnore = null, EventHandler afterIgnore = null)
        {
            Conversation cnv = new Conversation(sessionFactory, config, TypeOfUnitOfWork.Bulk, autoCommit, AppConfiguration.ShowQueries);
            NHibernateBulkUnitOfWork u = new NHibernateBulkUnitOfWork(manager, cnv, autoCommit, throwExceptionOnError);
            u.BeforeCommit += beforeCommit;
            u.AfterCommit += afterCommit;
            u.BeforeIgnore += beforeIgnore;
            u.AfterIgnore += afterIgnore;
            return (u);
        }
    }
}