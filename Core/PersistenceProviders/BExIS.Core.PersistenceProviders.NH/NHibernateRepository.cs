using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using BExIS.Core.Persistence.Api;

namespace BExIS.Core.PersistenceProviders.NH
{
    /// <summary>
    /// The methods of the repository, do not push the changes to the underlying database! to do so you need to commit the transaction which is under the control of the unit of work!
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class NHibernateRepository<TEntity> : NHibernateReadonlyRepository<TEntity>, IRepository<TEntity> where TEntity : class
    {
        public IUnitOfWork UnitOfWork { get { return(this.UoW as IUnitOfWork);}  }

        internal NHibernateRepository(NHibernateUnitOfWork uow)
            : base(uow)
        {
        }

        public bool Put(TEntity entity)
        {
            //session.Lock(entity, LockMode.None);
            UoW.Session.SaveOrUpdate(entity);
            return (true);
        }

        public bool Put(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                //session.Lock(entity, LockMode.None);
                UoW.Session.SaveOrUpdate(entity);
            }
            return (true);
        }

        public bool Delete(TEntity entity)
        {
            UoW.Session.Delete(entity);
            return (true);
        }

        public bool Delete(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                UoW.Session.Delete(entity);
            }
            return (true);
        }
    }
}
