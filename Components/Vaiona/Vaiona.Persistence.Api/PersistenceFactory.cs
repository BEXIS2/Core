using System;
using Vaiona.IoC;

namespace Vaiona.Persistence.Api
{
    public static class PersistenceFactory
    {
        public static IPersistenceManager GetPersistenceManager()
        {
            IPersistenceManager persistenceManager = null;
            try
            {
                persistenceManager = IoCFactory.Container.Resolve<IPersistenceManager>() as IPersistenceManager;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load persistence manager", ex);
            }
            return (persistenceManager);
        }

        public static IUnitOfWork GetUnitOfWork(this object obj)
        {
            IPersistenceManager persistenceManager = IoCFactory.Container.Resolve<IPersistenceManager>() as IPersistenceManager;
            IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateUnitOfWork(false, true);
            return (uow);
        }

        public static IUnitOfWork GetBulkUnitOfWork(this object obj)
        {
            IPersistenceManager persistenceManager = IoCFactory.Container.Resolve<IPersistenceManager>() as IPersistenceManager;
            IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateBulkUnitOfWork(false, true);
            return (uow);
        }

        public static IUnitOfWork GetIsolatedUnitOfWork(this object obj)
        {
            IPersistenceManager persistenceManager = IoCFactory.Container.Resolve<IPersistenceManager>() as IPersistenceManager;
            IUnitOfWork uow = persistenceManager.UnitOfWorkFactory.CreateIsolatedUnitOfWork(false, true);
            return (uow);
        }
    }
}