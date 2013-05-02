using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Core.Persistence.Api
{
    public interface IUnitOfWork: IDisposable
    {
        IPersistenceManager PersistenceManager { get; }
        IReadOnlyRepository<TEntity> GetReadOnlyRepository<TEntity>() where TEntity : class;
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        /// <summary>
        /// Commits all the changed made by associated repositories.
        /// Closes the transaction, so the unit is not usable to commit anymore. Use allowMultipleCommit if you need multiple consecutive commits using one UoW instance, which 
        /// commits all the changes, but then begins another transaction so that the UoW is ready to be reused.
        /// Good for multiple step actions, commulative commit, and so on/// </summary>
        void Commit();


        /// <summary>
        /// Rolls back all the changes and closed the transaction. If allowMultipleCommit is true, then the method 
        /// rolls back all the changes but keeps the transaction open (by beginning a new one) in order to UoW to remain usable.
        /// </summary>
        void Ignore();
        
        event EventHandler BeforeCommit;
        event EventHandler AfterCommit;

        event EventHandler BeforeIgnore;
        event EventHandler AfterIgnore;
    }
}
