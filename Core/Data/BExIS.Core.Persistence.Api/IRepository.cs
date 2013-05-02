using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace BExIS.Core.Persistence.Api
{
    public interface IReadOnlyRepository<TEntity> where TEntity : class // BaseEntity
    {
        void Evict<TEntity>();
        void Evict<TEntity>(object id);
        void Evict(TEntity entity);
        void ClearCache();

        TEntity Reload(TEntity entity);
        TEntity Refresh(Int64 id);
        TEntity Get(Int64 id);
        
        IList<TEntity> Get();
        IList<TEntity> Get(Expression<Func<TEntity, bool>> expression);
        IList<TEntity> Get(string namedQuery, Dictionary<string, object> parameters);
        IList<TEntity> Get(string queryString, Dictionary<string, object> parameters, bool isNativeOrORM = false); //isNativeOrORM == false => ORM, else => Native
        IList<TEntity> Get(object criteria); // need more wrok

        IQueryable<TEntity> Query();
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> expression);
        IQueryable<TEntity> Query(string expression);
        IQueryable<TEntity> Query(object criteria); // need more wrok

        bool IsPropertyLoaded(object proxy, string propertyName);
        bool IsLoaded(object proxy);
        void Load(object proxy);
        void LoadIfNot(object proxy);
    }

    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : class // BaseEntity
    {
        bool Put(TEntity entity);
        bool Put(IEnumerable<TEntity> entities);        
        bool Delete(TEntity entity);
        bool Delete(IEnumerable<TEntity> entities);
        
        IUnitOfWork UnitOfWork { get; }
    }

    //public interface IIntKeyedRepository<TEntity> : IRepository<TEntity> where TEntity : class
    //{
    //    TEntity FindBy(int id);
    //}
}
