using System;
using System.Collections.Generic;
using System.Data;

namespace Vaiona.Persistence.Api
{
    // maybe its better to change it to IWorkPackage
    public enum CacheMode
    {
        //
        // Summary:
        //     The session will never interact with the cache, except to invalidate cache items
        //     when updates occur
        Ignore = 0,

        //
        // Summary:
        //     The session will never read items from the cache, but will add items to the cache
        //     as it reads them from the database.
        Put = 1,

        //
        // Summary:
        //     The session may read items from the cache, but will not add items, except to
        //     invalidate items when updates occur
        Get = 2,

        //
        // Summary:
        //     The session may read items from the cache, and add items to the cache
        Normal = 3,

        //
        // Summary:
        //     The session will never read items from the cache, but will add items to the cache
        //     as it reads them from the database. In this mode, the effect of hibernate.cache.use_minimal_puts
        //     is bypassed, in order to force a cache refresh
        Refresh = 5
    }

    public interface IUnitOfWork : IDisposable
    {
        IPersistenceManager PersistenceManager { get; }

        IReadOnlyRepository<TEntity> GetReadOnlyRepository<TEntity>(CacheMode cacheMode = CacheMode.Ignore) where TEntity : class;

        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        void ClearCache(bool applyChanages = true);

        /// <summary>
        /// Commits all the changed made by associated repositories.
        /// Closes the transaction, so the unit is not usable to commit anymore. Use allowMultipleCommit if you need multiple consecutive commits using one UoW instance, which
        /// commits all the changes, but then begins another transaction so that the UoW is ready to be reused.
        /// Good for multiple step actions, cumulative commit, and so on/// </summary>
        void Commit();

        /// <summary>
        /// Rolls back all the changes and closed the transaction. If allowMultipleCommit is true, then the method
        /// rolls back all the changes but keeps the transaction open (by beginning a new one) in order to UoW to remain usable.
        /// </summary>
        void Ignore();

        /// <summary>
        /// Executes a named query that returns ay most a single result.
        /// This command is designed to operate at a level upper than the entities and
        /// is meant to be used for checking existance, creating DDL objects, etc.
        /// </summary>
        /// <typeparam name="T">The scalar data type of the return value. In most of the cases it is Boolean or Integer.</typeparam>
        /// <param name="queryName">Query name as in the ORM mappings</param>
        /// <param name="parameters">A dictionary of parameter name/values to be passed to the query.</param>
        /// <returns></returns>
        T Execute<T>(string queryName, Dictionary<string, object> parameters = null);

        List<T> ExecuteList<T>(string queryName, Dictionary<string, object> parameters = null);

        T ExecuteDynamic<T>(string queryString, Dictionary<string, object> parameters = null);

        int ExecuteNonQuery(string queryString, Dictionary<string, object> parameters = null);

        object ExecuteScalar(string queryString, Dictionary<string, object> parameters = null);

        DataTable ExecuteQuery(string queryString, Dictionary<string, object> parameters = null);

        event EventHandler BeforeCommit;

        event EventHandler AfterCommit;

        event EventHandler BeforeIgnore;

        event EventHandler AfterIgnore;
    }
}