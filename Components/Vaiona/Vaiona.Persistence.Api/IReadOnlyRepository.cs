using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Vaiona.Persistence.Api
{
    public interface IReadOnlyRepository<TEntity> where TEntity : class // BaseEntity
    {
        void Evict();

        void Evict(object id);

        void Evict(TEntity entity);

        IUnitOfWork UnitOfWork { get; }

        TEntity Reload(TEntity entity);

        TEntity Refresh(Int64 id);

        IList<TEntity> Get();

        IList<TEntity> Get(Expression<Func<TEntity, bool>> expression);

        IList<TEntity> Get(string namedQuery, Dictionary<string, object> parameters);

        IList<TEntity> Get(string queryString, Dictionary<string, object> parameters, bool isNativeOrORM = false); //isNativeOrORM == false => ORM, else => Native

        IList<TEntity> Get(object criteria); // needs more work

        TEntity Get(Int64 id);

        /// <summary>
        /// executes a query which its return type is not known at compile time. proper for reporting and charting purposes
        /// </summary>
        /// <param name="namedQuery"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IList Get2(string namedQuery, Dictionary<string, object> parameters);

        IQueryable<TEntity> Query();

        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> expression);

        IQueryable<TEntity> Query(string expression);

        IQueryable<TEntity> Query(object criteria); // needs more work

        //IQueryable<TEntity> Include<TRelated>(Expression<Func<TEntity, IEnumerable<TRelated>>> relatedObjectSelector);
        bool IsPropertyLoaded(object proxy, string propertyName);

        bool IsLoaded(object proxy);

        void Load(object proxy);

        void LoadIfNot(object proxy);
    }
}