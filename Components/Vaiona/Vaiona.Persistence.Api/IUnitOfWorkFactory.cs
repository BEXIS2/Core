using System;

namespace Vaiona.Persistence.Api
{
    public enum TypeOfUnitOfWork
    {
        Normal = 1,
        Isolated = 2,
        Bulk = 3,
    }

    public interface IUnitOfWorkFactory
    {
        IUnitOfWork CreateUnitOfWork(bool autoCommit = false, bool throwExceptionOnError = true,
            EventHandler beforeCommit = null, EventHandler afterCommit = null, EventHandler beforeIgnore = null, EventHandler afterIgnore = null);

        IUnitOfWork CreateBulkUnitOfWork(bool autoCommit = false, bool throwExceptionOnError = true,
            EventHandler beforeCommit = null, EventHandler afterCommit = null, EventHandler beforeIgnore = null, EventHandler afterIgnore = null);

        IUnitOfWork CreateIsolatedUnitOfWork(bool autoCommit = false, bool throwExceptionOnError = true,
            EventHandler beforeCommit = null, EventHandler afterCommit = null, EventHandler beforeIgnore = null, EventHandler afterIgnore = null);

        //object GetCurrentConversation();
        //void StartConversation();
        ///// <summary>
        ///// Closes the session and rollbacks all the changes
        ///// </summary>
        //void ShutdownConversation();

        ///// <summary>
        ///// Closes the session but first commits all the changes
        ///// </summary>
        //void EndConversation();
        //void EndContext();
    }
}