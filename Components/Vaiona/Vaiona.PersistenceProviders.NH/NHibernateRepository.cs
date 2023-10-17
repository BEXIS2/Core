using NHibernate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Vaiona.Persistence.Api;

namespace Vaiona.PersistenceProviders.NH
{
    /// <summary>
    /// The methods of the repository, do not push the changes to the underlying database! to do so you need to commit the transaction which is under the control of the unit of work!
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class NHibernateRepository<TEntity> : NHibernateReadonlyRepository<TEntity>, IRepository<TEntity> where TEntity : class
    {
        //public IUnitOfWork UnitOfWork { get { return(this.UoW as IUnitOfWork);}  }

        internal NHibernateRepository(IUnitOfWork uow)
            : base(uow, Persistence.Api.CacheMode.Normal) // this cache mode should be changed according to the uow's session's cache mode
        {
        }

        public bool Delete(TEntity entity)
        {
            lock (UoW)
            {
                if (UoW is NHibernateUnitOfWork)
                {
                    ((NHibernateUnitOfWork)UoW).Session.Delete(entity);
                    return true;
                }
                else if (UoW is NHibernateBulkUnitOfWork)
                {
                    ((NHibernateBulkUnitOfWork)UoW).Session.Delete(entity);
                    return (true);
                }
                return (false);
            }
        }

        public bool Delete(IEnumerable<TEntity> entities)
        {
            lock (UoW)
            {
                foreach (var entity in entities)
                {
                    //UoW.Session.Delete(entity);
                    if (!Delete(entity))
                        return false;
                }
                return (true);
            }
        }

        public bool Delete(long entityId)
        {
            lock (UoW)
            {
                try
                {
                    string queryString = string.Format("DELETE FROM {0} e WHERE e.Id = :id", typeof(TEntity).Name);
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("id", entityId);
                    this.Execute(queryString, parameters);
                }
                catch (Exception ex)
                {
                    return false;
                }
                return (true);
            }
        }

        public bool Delete(IEnumerable<Int64> entityIds)
        {
            lock (UoW)
            {
                try
                {
                    string queryString = string.Format("DELETE FROM {0} e WHERE e.Id IN (:idsList)", typeof(TEntity).Name);
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("idsList", entityIds);
                    this.Execute(queryString, parameters);
                }
                catch (Exception ex)
                {
                    return false;
                }
                return (true);
            }
        }

        /// <summary>
        /// Use this only for delete or update in bulk mode
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="parameters"></param>
        /// <param name="isNativeOrORM"></param>
        /// <param name="timeoput">Query Timeout in seconds</param>
        /// <returns></returns>
        public int Execute(string queryString, Dictionary<string, object> parameters, bool isNativeOrORM = false, int timeout = 100)
        {
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            lock (UoW)
            {
                IQuery query = null;
                if (isNativeOrORM == false) // ORM native query: like HQL
                {
                    if (UoW is NHibernateUnitOfWork)
                        query = ((NHibernateUnitOfWork)UoW).Session.CreateQuery(queryString);
                    else if (UoW is NHibernateBulkUnitOfWork)
                        query = ((NHibernateBulkUnitOfWork)UoW).Session.CreateQuery(queryString);
                }
                else // Database native query
                {
                    //query = UoW.Session.CreateSQLQuery(queryString).AddEntity(typeof(TEntity));
                    if (UoW is NHibernateUnitOfWork)
                        query = ((NHibernateUnitOfWork)UoW).Session.CreateSQLQuery(queryString).AddEntity(typeof(TEntity));
                    else if (UoW is NHibernateBulkUnitOfWork)
                        query = ((NHibernateBulkUnitOfWork)UoW).Session.CreateSQLQuery(queryString).AddEntity(typeof(TEntity));
                }
                if (parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        if (item.Value is IList || item.Value is ICollection)
                        {
                            query.SetParameterList(item.Key, (IEnumerable)item.Value);
                        }
                        else
                        {
                            query.SetParameter(item.Key, item.Value);
                        }
                    }
                }
                query.SetTimeout(timeout);
                return (query.ExecuteUpdate());
            }
        }

        //needs more tests
        public TEntity Merge(TEntity entity)
        {
            //session.Lock(entity, LockMode.None);
            //UoW.Session.Merge<TEntity>(entity);
            if (UoW is NHibernateUnitOfWork)
                ((NHibernateUnitOfWork)UoW).Session.Merge<TEntity>(entity);
            return (entity);
        }

        /// <summary>
        /// In Stateless Mode, it only INSERTs the entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Put(TEntity entity)
        {
            lock (UoW)
            {
                //session.Lock(entity, LockMode.None);
                applyStateInfo(entity);
                applyAuditInfo(entity);
                //UoW.Session.SaveOrUpdate(entity);
                if (UoW is NHibernateUnitOfWork)
                {
                    ISession session = ((NHibernateUnitOfWork)UoW).Session;
                    // best effort to lock the row for writing, but it my fail on objects that carry a bag/set/collection of child entities.
                    // This locking mechanism needs to be revised.
                    try
                    {
                        if (!IsTransient(entity))
                            session.Lock(entity, LockMode.UpgradeNoWait);
                    }
                    catch (Exception ex) // do nothing for now!
                    { }
                    finally
                    {
                        session.SaveOrUpdate(entity);
                    }
                    return true;
                }
                else if (UoW is NHibernateBulkUnitOfWork)
                {   // check to see whether the entity is a new object to be inserted or an existing one to be updated.
                    // the stateless session does not keep track of the entities!
                    ((NHibernateBulkUnitOfWork)UoW).Session.Insert(entity);
                    return (true);
                }
                return (false);
            }
        }

        public bool Put(IEnumerable<TEntity> entities)
        {
            lock (UoW)
            {
                if (UoW is NHibernateUnitOfWork)
                {
                    return putStatefull(((NHibernateUnitOfWork)UoW).Session, entities);
                }
                else if (UoW is NHibernateBulkUnitOfWork)
                {
                    return putStateless(((NHibernateBulkUnitOfWork)UoW).Session, entities);
                }
                return (false);
            }
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

        private bool putStatefull(ISession session, IEnumerable<TEntity> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    applyStateInfo(entity);
                    applyAuditInfo(entity);
                    if (!IsTransient(entity))
                        session.Lock(entity, LockMode.Read);
                    session.SaveOrUpdate(entity);
                }
                return true;
            }
            catch { return false; }
        }

        private bool putStateless(IStatelessSession session, IEnumerable<TEntity> entities)
        {
            try
            {
                foreach (var entity in entities)
                {
                    applyStateInfo(entity);
                    applyAuditInfo(entity);
                    session.Insert(entity);
                }
                return true;
            }
            catch { return false; }
        }
    }
}