using System;
using System.Linq;
using BExIS.Core.IoC;

namespace BExIS.Core.Persistence.Api
{
    public static class PersistenceUtil
    {
        public static IUnitOfWork GetUnitOfWork(this object obj)
        {
            IPersistenceManager persistenceManager = IoCFactory.Container.Resolve<IPersistenceManager>() as IPersistenceManager;
            IUnitOfWork uow = persistenceManager.CreateUnitOfWork(false, true, false);
            return (uow);
        }

        public static IUnitOfWork GetMultipleCommitUnitOfWork(this object obj)
        {
            IPersistenceManager persistenceManager = IoCFactory.Container.Resolve<IPersistenceManager>() as IPersistenceManager;
            IUnitOfWork uow = persistenceManager.CreateUnitOfWork(false, true, true);
            return (uow);
        }
    }
}
