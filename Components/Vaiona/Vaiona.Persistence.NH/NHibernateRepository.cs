using System;
using System.Collections.Generic;
using Vaiona.Persistence.Api;

namespace Vaiona.Persistence.NH
{
    /// <summary>
    /// The methods of the repository, do not push the changes to the underlying database! to do so you need to commit the transaction which is under the control of the unit of work!
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class NHibernateRepository<TEntity> : NHibernateReadonlyRepository<TEntity>, IRepository<TEntity> where TEntity : class
    {
        internal NHibernateRepository(NHibernateUnitOfWork uow)
            : base(uow)
        {
        }

        public IUnitOfWork UnitOfWork
        { get { return (this.UoW as IUnitOfWork); } }

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

        public bool Delete(IEnumerable<long> entityId)
        {
            throw new NotImplementedException();
        }

        public bool Delete(long entityId)
        {
            throw new NotImplementedException();
        }

        public int Execute(string queryString, Dictionary<string, object> parameters, bool isNativeOrORM = false, int timeout = 100)
        {
            throw new NotImplementedException();
        }

        public bool IsTransient(object proxy)
        {
            bool? result = NHibernate.Engine.ForeignKeys.IsTransientSlow(proxy.GetType().FullName, proxy, (this.UnitOfWork as NHibernateUnitOfWork).Session.GetSessionImplementation());
            return (result == null ? false : (bool)result);
        }

        public TEntity Merge(TEntity entity)
        {
            //session.Lock(entity, LockMode.None);
            UoW.Session.Merge<TEntity>(entity);
            return (entity);
        }

        public bool Put(TEntity entity)
        {
            //session.Lock(entity, LockMode.None);
            applyStateInfo(entity);
            applyAuditInfo(entity);
            UoW.Session.SaveOrUpdate(entity);
            return (true);
        }

        public bool Put(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                //session.Lock(entity, LockMode.None);
                applyStateInfo(entity);
                applyAuditInfo(entity);
                UoW.Session.SaveOrUpdate(entity);
            }
            return (true);
        }

        private void applyAuditInfo(TEntity entity)
        {
            // check unsaved-value-check to know whether object is new or updated. use this info for state management
            // check whether entity is a BaseEntity, BusinessEntity or something else
            // throw new NotImplementedException();
        }

        private void applyStateInfo(TEntity entity)
        {
            // check unsaved-value-check to know whether object is new or updated. use this info for state management
            // throw new NotImplementedException();
        }
    }
}