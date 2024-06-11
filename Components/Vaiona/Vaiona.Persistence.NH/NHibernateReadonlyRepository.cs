using NHibernate;
using NHibernate.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Vaiona.Persistence.Api;

namespace Vaiona.Persistence.NH
{
    public class NHibernateReadonlyRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : class
    {
        protected NHibernateUnitOfWork UoW = null;

        public IUnitOfWork UnitOfWork => throw new NotImplementedException();

        internal NHibernateReadonlyRepository(NHibernateUnitOfWork uow)
        {
            this.UoW = uow;
        }

        public void Evict<TEntity>()
        {
            UoW.Session.SessionFactory.Evict(typeof(TEntity));
        }

        public void Evict<TEntity>(object id)
        {
            UoW.Session.SessionFactory.Evict(typeof(TEntity), id);
        }

        public void Evict(TEntity entity)
        {
            UoW.Session.Evict(entity);
        }

        public void ClearCache()
        {
            UoW.Session.Clear();
        }

        public TEntity Get(long id)
        {
            // NHibernateUtil.Initialize( paths
            return (UoW.Session.Get<TEntity>(id));
        }

        public TEntity Reload(TEntity entity)
        {
            Evict(entity);
            IClassMetadata metaInfo = UoW.Session.SessionFactory.GetClassMetadata(typeof(TEntity));
            if (metaInfo.HasIdentifierProperty)
            {
                object idValue = entity.GetType().GetProperty(metaInfo.IdentifierPropertyName).GetValue(entity, null);
                return (UoW.Session.Get<TEntity>(idValue));
            }
            return (default(TEntity));
        }

        public TEntity Refresh(Int64 id)
        {
            Evict<TEntity>(id);
            return (UoW.Session.Get<TEntity>(id));
        }

        public IList<TEntity> Get(Expression<Func<TEntity, bool>> expression)
        {
            return (this.Query(expression).ToList());
        }

        public IList<TEntity> Get()
        {
            return (this.Query().ToList());
        }

        public IList<TEntity> Get(string namedQuery, Dictionary<string, object> parameters)
        {
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            IQuery query = UoW.Session.GetNamedQuery(namedQuery);
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    query.SetParameter(item.Key, item.Value);
                }
            }
            return (query.List<TEntity>());
        }

        public IList Get2(string namedQuery, Dictionary<string, object> parameters)
        {
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            IQuery query = UoW.Session.GetNamedQuery(namedQuery);
            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    query.SetParameter(item.Key, item.Value);
                }
            }
            return (query.List());
        }

        public IList<TEntity> Get(string queryString, Dictionary<string, object> parameters, bool isNativeOrORM = false)
        {
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            IQuery query = null;
            if (isNativeOrORM == false) // ORM native query: like HQL
            {
                query = UoW.Session.CreateQuery(queryString);
            }
            else // Database native query
            {
                query = UoW.Session.CreateSQLQuery(queryString).AddEntity(typeof(TEntity));
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

        public IList<TEntity> Get(object criteria)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> Query()
        {
            return (UoW.Session.Query<TEntity>());
        }

        public IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> expression)
        {
            return (this.Query().Where(expression).AsQueryable());
        }

        public IQueryable<TEntity> Query(string expression)
        {
            return (null); // use DynamicLinq to implement this method
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

        public void LoadIfNot(object proxy)
        {
            if (NHibernateUtil.IsInitialized(proxy))
            {
                NHibernateUtil.Initialize(proxy);
            }
        }

        public void Evict()
        {
            throw new NotImplementedException();
        }

        public void Evict(object id)
        {
            throw new NotImplementedException();
        }
    }
}