using System;
using System.Collections.Generic;

namespace Vaiona.Persistence.Api
{
    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : class // BaseEntity
    {
        bool Delete(TEntity entity);

        bool Delete(IEnumerable<TEntity> entities);

        /// <summary>
        /// Deletes a set of entities using thier IDs
        /// </summary>
        /// <param name="entityId">An enumerable containing the entity IDs to be deleted</param>
        /// <returns>True if all deleted</returns>
        /// <remarks>Not guranteed to work on inhertited entities. Table Per Class, Table per Concrete Class. Test it before use.</remarks>
        bool Delete(IEnumerable<Int64> entityId);

        bool Delete(long entityId);

        int Execute(string queryString, Dictionary<string, object> parameters, bool isNativeOrORM = false, int timeout = 100);

        bool IsTransient(object proxy);

        TEntity Merge(TEntity entity);

        bool Put(TEntity entity);

        bool Put(IEnumerable<TEntity> entities);

        //IUnitOfWork UnitOfWork { get; }
    }

    //public interface IIntKeyedRepository<TEntity> : IRepository<TEntity> where TEntity : class
    //{
    //    TEntity FindBy(int id);
    //}
}