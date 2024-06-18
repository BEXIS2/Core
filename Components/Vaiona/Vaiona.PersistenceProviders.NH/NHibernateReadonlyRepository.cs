using NHibernate;
using NHibernate.Engine;
using NHibernate.Linq;
using NHibernate.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Vaiona.Persistence.Api;

namespace Vaiona.PersistenceProviders.NH
{
    public class NHibernateReadonlyRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : class
    {
        protected IUnitOfWork UoW = null;
        private NHibernate.CacheMode cacheMode = NHibernate.CacheMode.Ignore;

        internal NHibernateReadonlyRepository(IUnitOfWork uow, Vaiona.Persistence.Api.CacheMode cacheMode)
        {
            this.UoW = uow;
            this.cacheMode = (NHibernate.CacheMode)Enum.Parse(typeof(NHibernate.CacheMode), Enum.GetName(typeof(Vaiona.Persistence.Api.CacheMode), cacheMode));
        }

        public IUnitOfWork UnitOfWork
        { get { return (UoW); } }

        public void Evict()
        {
            if (UoW is NHibernateUnitOfWork)
                ((NHibernateUnitOfWork)UoW).Session.Clear(); // .SessionFactory.Evict(typeof(TEntity));
        }

        public void Evict(object id)
        {
            if (UoW is NHibernateUnitOfWork)
                ((NHibernateUnitOfWork)UoW).Session.SessionFactory.Evict(typeof(TEntity), id);
        }

        public void Evict(TEntity entity)
        {
            if (UoW is NHibernateUnitOfWork)
                ((NHibernateUnitOfWork)UoW).Session.Evict(entity);
        }

        public TEntity Get(long id)
        {
            lock (UoW)
            {
                // NHibernateUtil.Initialize( paths
                if (UoW is NHibernateUnitOfWork)
                    return (((NHibernateUnitOfWork)UoW).Session.Get<TEntity>(id));
                else if (UoW is NHibernateBulkUnitOfWork)
                    return (((NHibernateBulkUnitOfWork)UoW).Session.Get<TEntity>(id));
                return default(TEntity);
            }
        }

        public bool IsTransient(object proxy)
        {
            ISessionImplementor isim = null;
            if (UoW is NHibernateUnitOfWork)
                isim = (this.UnitOfWork as NHibernateUnitOfWork).Session.GetSessionImplementation();
            else if (UoW is NHibernateBulkUnitOfWork)
                isim = (this.UnitOfWork as NHibernateBulkUnitOfWork).Session.GetSessionImplementation();
            bool? result = NHibernate.Engine.ForeignKeys.IsTransientSlow(proxy.GetType().FullName, proxy, isim);
            return (result == null ? false : (bool)result);
        }

        public TEntity Reload(TEntity entity)
        {
            if (entity == null || IsTransient(entity))
                throw new InvalidOperationException("Passed entity is either NULL or transient.");
            lock (UoW)
            {
                if (UoW is NHibernateUnitOfWork)
                {
                    Evict(entity);
                    var uow = ((NHibernateUnitOfWork)UoW);
                    IClassMetadata metaInfo = uow.Session.SessionFactory.GetClassMetadata(typeof(TEntity));
                    if (metaInfo.HasIdentifierProperty)
                    {
                        object idValue = entity.GetType().GetProperty(metaInfo.IdentifierPropertyName).GetValue(entity, null);
                        return (uow.Session.Get<TEntity>(idValue));
                    }
                }
                // stateless sessions have no access to the session factory!
                return (default(TEntity));
            }
        }

        public TEntity Refresh(Int64 id)
        {
            lock (UoW)
            {
                Evict(id);
                if (UoW is NHibernateUnitOfWork)
                    return (((NHibernateUnitOfWork)UoW).Session.Load<TEntity>(id));
                return default(TEntity);
            }
        }

        public IList<TEntity> Get(Expression<Func<TEntity, bool>> expression)
        {
            lock (UoW)
            {
                return (this.Query(expression).ToList());
            }
        }

        public IList<TEntity> Get()
        {
            lock (UoW)
            {
                return (this.Query().ToList());
            }
        }

        public IList<TEntity> Get(string namedQuery, Dictionary<string, object> parameters)
        {
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");
            lock (UoW)
            {
                IQuery query = null;
                if (UoW is NHibernateUnitOfWork)
                {
                    query = ((NHibernateUnitOfWork)UoW).Session.GetNamedQuery(namedQuery);
                    query.SetCacheMode(cacheMode);
                }
                else if (UoW is NHibernateBulkUnitOfWork)
                {
                    query = ((NHibernateBulkUnitOfWork)UoW).Session.GetNamedQuery(namedQuery);
                }
                if (parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        query.SetParameter(item.Key, item.Value);
                    }
                }
                return (query.List<TEntity>());
            }
        }

        /// <summary>
        /// returns a list of un-typed objects.
        /// </summary>
        /// <param name="namedQuery">Name of the query to be retrieved from the mapping files</param>
        /// <param name="parameters">Parameter values to be passed to the query</param>
        /// <returns></returns>
        public IList Get2(string namedQuery, Dictionary<string, object> parameters)
        {
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            lock (UoW)
            {
                IQuery query = null;
                if (UoW is NHibernateUnitOfWork)
                {
                    query = ((NHibernateUnitOfWork)UoW).Session.GetNamedQuery(namedQuery);
                    query.SetCacheMode(cacheMode);
                }
                else if (UoW is NHibernateBulkUnitOfWork)
                {
                    query = ((NHibernateBulkUnitOfWork)UoW).Session.GetNamedQuery(namedQuery);
                }
                if (parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        query.SetParameter(item.Key, item.Value);
                    }
                }
                return (query.List()); // returns an un-typed list, a list of objects!
            }
        }

        public IList<TEntity> Get(string queryString, Dictionary<string, object> parameters, bool isNativeOrORM = false)
        {
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            lock (UoW)
            {
                IQuery query = null;
                if (isNativeOrORM == false) // ORM native query: like HQL
                {
                    if (UoW is NHibernateUnitOfWork)
                    {
                        query = ((NHibernateUnitOfWork)UoW).Session.CreateQuery(queryString);
                        query.SetCacheMode(cacheMode);
                    }
                    else if (UoW is NHibernateBulkUnitOfWork)
                    {
                        query = ((NHibernateBulkUnitOfWork)UoW).Session.CreateQuery(queryString);
                    }
                }
                else // Database native query
                {
                    //query = UoW.Session.CreateSQLQuery(queryString).AddEntity(typeof(TEntity));
                    if (UoW is NHibernateUnitOfWork)
                    {
                        query = ((NHibernateUnitOfWork)UoW).Session.CreateSQLQuery(queryString).AddEntity(typeof(TEntity));
                        query.SetCacheMode(cacheMode);
                    }
                    else if (UoW is NHibernateBulkUnitOfWork)
                    {
                        query = ((NHibernateBulkUnitOfWork)UoW).Session.CreateSQLQuery(queryString).AddEntity(typeof(TEntity));
                    }
                }
                if (parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        query.SetParameter(item.Key, item.Value);
                    }
                }
                return (query.List<TEntity>());
            }
        }

        public IList<TEntity> Get(object criteria)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> Query()
        {
            lock (UoW)
            {
                if (UoW is NHibernateUnitOfWork)
                    return (((NHibernateUnitOfWork)UoW).Session.Query<TEntity>().CacheMode(cacheMode));
                else if (UoW is NHibernateBulkUnitOfWork)
                    return (((NHibernateBulkUnitOfWork)UoW).Session.Query<TEntity>());
                return null;
            }
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> expression)
        {
            lock (UoW)
            {
                return (this.Query().Where(expression).AsQueryable());
            }
        }

        public IQueryable<TEntity> Query(string expression)
        {
            lock (UoW)
            {
                return (null); // use DynamicLinq to implement this method
            }
        }

        public IQueryable<TEntity> Query(object criteria)
        {
            throw new NotImplementedException();
        }

        public bool IsPropertyLoaded(object proxy, string propertyName)
        {
            return (NHibernateUtil.IsPropertyInitialized(proxy, propertyName));
        }

        public bool IsLoaded(object proxy)
        {
            return (NHibernateUtil.IsInitialized(proxy));
        }

        public void Load(object proxy)
        {
            NHibernateUtil.Initialize(proxy);
        }

        //public IQueryable<TEntity> QueryWithPath<TRelated>(Expression<Func<TEntity, IEnumerable<TRelated>>> relatedObjectSelector, params List<Expression<Func<TEntity, IEnumerable<TRelated>>>> relatedObjectSelectors)
        //{
        //    var q = UoW.Session.Query<TEntity>().FetchMany(relatedObjectSelector);
        //    foreach (var item in relatedObjectSelectors)
        //    {
        //        q = q.ThenFetchMany<TRelated>(item);
        //    }
        //    return (UoW.Session.Query<TEntity>().FetchMany(relatedObjectSelector));
        //}

        public void LoadIfNot(object proxy)
        {
            lock (UoW)
            {
                if (NHibernateUtil.IsInitialized(proxy))
                {
                    NHibernateUtil.Initialize(proxy);
                }
            }
        }
    }
}