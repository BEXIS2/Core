using NHibernate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Vaiona.Persistence.Api;

namespace Vaiona.PersistenceProviders.NH
{
    public class NHibernateUnitOfWork : IUnitOfWork
    {
        private const int LongQueryTimeOut = 3600; //seconds

        //internal ISession Session = null;
        private bool autoCommit = false;

        private bool throwExceptionOnError = true;
        internal Conversation Conversation = null;

        public IPersistenceManager PersistenceManager { get; internal set; }

        internal NHibernateUnitOfWork(NHibernatePersistenceManager persistenceManager, Conversation conversation, bool autoCommit = false, bool throwExceptionOnError = true)
        {
            this.PersistenceManager = persistenceManager;
            this.autoCommit = autoCommit;
            this.throwExceptionOnError = throwExceptionOnError;
            this.Conversation = conversation;
            this.Conversation.Start(this);
        }

#if DEBUG

        public ISession Session
        {
            get
            {
                return this.Conversation.GetSession();
            }
        }

#else
        internal ISession Session
        {
            get
            {
                return this.Conversation.GetSession();
            }
        }
#endif

        public IReadOnlyRepository<TEntity> GetReadOnlyRepository<TEntity>(Vaiona.Persistence.Api.CacheMode cacheMode = Vaiona.Persistence.Api.CacheMode.Ignore) where TEntity : class
        {
            IReadOnlyRepository<TEntity> repo = new NHibernateReadonlyRepository<TEntity>(this, cacheMode);
            return (repo);
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            IRepository<TEntity> repo = new NHibernateRepository<TEntity>(this);
            return (repo);
        }

        public void ClearCache(bool applyChanages = true)
        {
            this.Conversation.Clear(applyChanages);
        }

        public void Commit()
        {
            lock (this)
            {
                try
                {
                    if (BeforeCommit != null)
                        BeforeCommit(this, EventArgs.Empty);
                    // try detect what is going to be committed, adds, deletes, changes, and log some information about them after commit is done!
                    this.Conversation.Commit(this);

                    // log the changes detected in previous steps
                    if (AfterCommit != null)
                        AfterCommit(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    //session.Transaction.Rollback(); //??
                    if (throwExceptionOnError)
                        throw ex;
                }
                //finally // reactivate the transaction for later use by foloowing UoWs
                //{
                //    if (!Session.Transaction.IsActive)
                //        Session.Transaction.Begin(System.Data.IsolationLevel.ReadCommitted);
                //}
            }
        }

        public void Ignore()
        {
            return;
            //lock (this)
            //{
            //    try
            //    {
            //        if (Session.Transaction.IsActive)
            //        {
            //            if (BeforeIgnore != null)
            //                BeforeIgnore(this, EventArgs.Empty);
            //            try
            //            {
            //                if (Session.IsDirty())
            //                    Session.Transaction.Rollback();
            //            }
            //            catch (Exception ex)
            //            { }
            //            if (Session.Transaction.WasRolledBack)
            //            {
            //                if (AfterIgnore != null)
            //                    AfterIgnore(this, EventArgs.Empty);
            //            }
            //        }
            //    }
            //    catch (ObjectDisposedException) // object is already disposed of
            //    {
            //        return;
            //    }
            //    catch (Exception ex)
            //    {
            //        if (throwExceptionOnError)
            //            throw ex;
            //    }
            //    finally // reactivate the transaction for later use by following UoWs
            //    {
            //        if (!Session.Transaction.IsActive)
            //            Session.Transaction.Begin(System.Data.IsolationLevel.ReadCommitted);
            //    }
            //}
        }

        public T Execute<T>(string queryName, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && parameters.Any(p => p.Value == null))
                throw new ArgumentException("The parameter array has a null element", "parameters");
            lock (this)
            {
                T result = default(T);
                ISession session = this.Conversation.GetSession();
                try
                {
                    //session.BeginTransaction();
                    IQuery query = session.GetNamedQuery(queryName);
                    query.SetTimeout(LongQueryTimeOut);
                    if (parameters != null)
                    {
                        foreach (var item in parameters)
                        {
                            query.SetParameter(item.Key, item.Value);
                        }
                    }
                    result = query.UniqueResult<T>();
                    //session.Transaction.Commit();
                }
                catch
                {
                    //session.Transaction.Rollback();
                    throw new Exception(string.Format("Failed for execute named query '{0}'.", queryName));
                }
                finally
                {
                    // Do Nothing
                }
                return result;
            }
        }

        public List<T> ExecuteList<T>(string queryName, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && parameters.Any(p => p.Value == null))
                throw new ArgumentException("The parameter array has a null element", "parameters");
            lock (this)
            {
                var result = new List<T>();
                ISession session = this.Conversation.GetSession();
                try
                {
                    IQuery query = session.GetNamedQuery(queryName);
                    query.SetTimeout(LongQueryTimeOut);
                    if (parameters != null)
                    {
                        foreach (var item in parameters)
                        {
                            query.SetParameter(item.Key, item.Value);
                        }
                    }
                    result = query.List<T>().ToList();
                }
                catch(Exception ex)
                {
                    throw new Exception(string.Format("Failed for execute named query '{0}'.", queryName));
                }
                finally
                {
                    // Do Nothing
                }
                return result;
            }
        }

        public T ExecuteDynamic<T>(string queryString, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && parameters.Any(p => p.Value == null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            lock (this)
            {
                T result = default(T);
                ISession session = this.Conversation.GetSession();
                try
                {
                    //session.BeginTransaction();
                    IQuery query = session.CreateSQLQuery(queryString);
                    query.SetTimeout(LongQueryTimeOut);
                    if (parameters != null)
                    {
                        foreach (var item in parameters)
                        {
                            query.SetParameter(item.Key, item.Value);
                        }
                    }
                    result = query.UniqueResult<T>();
                    //session.Transaction.Commit();
                }
                catch (Exception ex)
                {
                    //session.Transaction.Rollback();
                    throw new Exception(string.Format($"Failed for execute the submitted native query.  Reason: '{ex.Message}'"));
                }
                finally
                {
                    // Do Nothing
                }
                return result;
            }
        }

        public int ExecuteNonQuery(string queryString, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && parameters.Any(p => p.Value == null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            lock (this)
            {
                int result = 0;
                try
                {
                    using (ITransaction transaction = this.Session.BeginTransaction())
                    {
                        DbCommand command = this.Session.Connection.CreateCommand();
                        command.CommandTimeout = LongQueryTimeOut;
                        command.Connection = this.Session.Connection;

                        transaction.Enlist(command);

                        command.CommandText = queryString;
                        if (parameters != null)
                        {
                            foreach (var item in parameters)
                            {
                                command.Parameters.Add(new SqlParameter(item.Key, item.Value)); // sql paramater must be changed to a more generic one
                            }
                        }
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    //session.Transaction.Rollback();
                    throw new Exception(string.Format($"Failed for execute the submitted native query.  Reason: '{ex.Message}'"), ex);
                }
                finally
                {
                    // Do Nothing
                }
                return result;
            }
        }

        public object ExecuteScalar(string queryString, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && parameters.Any(p => p.Value == null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            lock (this)
            {
                object result = null;
                try
                {
                    using (ITransaction transaction = this.Session.BeginTransaction())
                    {
                        DbCommand command = this.Session.Connection.CreateCommand();
                        command.CommandTimeout = LongQueryTimeOut;
                        command.Connection = this.Session.Connection;

                        transaction.Enlist(command);

                        command.CommandText = queryString;
                        if (parameters != null)
                        {
                            foreach (var item in parameters)
                            {
                                command.Parameters.Add(new SqlParameter(item.Key, item.Value)); // sql paramater must be changed to a more generic one
                            }
                        }
                        result = command.ExecuteScalar();

                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    //session.Transaction.Rollback();
                    throw new Exception(string.Format($"Failed for execute the submitted native query.  Reason: '{ex.Message}'"));
                }
                finally
                {
                    // Do Nothing
                }
                return result;
            }
        }

        public DataTable ExecuteQuery(string queryString, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && parameters.Any(p => p.Value == null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            lock (this)
            {
                DataTable table = new DataTable();
                try
                {
                    using (IDbCommand command = this.Session.Connection.CreateCommand())
                    {
                        command.Connection = this.Session.Connection;
                        command.CommandTimeout = LongQueryTimeOut;
                        command.CommandText = queryString;

                        if (parameters != null)
                        {
                            foreach (var item in parameters)
                            {
                                command.Parameters.Add(new SqlParameter(item.Key, item.Value)); // sql paramater must be changed to a more generic, IDbParameter
                            }
                        }

                        IDataReader dr = command.ExecuteReader(CommandBehavior.SingleResult);
                        table.Load(dr);
                    }
                }
                catch (Exception ex)
                {
                    //session.Transaction.Rollback();
                    throw new Exception(string.Format($"Failed for execute the submitted native query.  Reason: '{ex.Message}'"), ex);
                }
                finally
                {
                    // Do Nothing
                }
                return table;
            }
        }

        private bool isDisposed = false;

        ~NHibernateUnitOfWork()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these
            // operations, as well as in your methods that use the resource.
            try
            {
                if (!isDisposed)
                {
                    if (disposing)
                    {
                        disposeResources();
                        isDisposed = true;
                    }
                }
            }
            catch
            { // do nothing
            }
        }

        private void disposeResources()
        {
            if (Session == null)
                return;
            if (autoCommit & !Session.Transaction.WasCommitted)
                this.Commit();
            else
                this.Ignore();
            // Do not close the session, as it is usually shared between multiple units of work in a single HTTP request context. The conversation object takes care of it.
            this.Conversation.End(this);
            // unhook the event handlers appropriately
            BeforeCommit = null;
            AfterCommit = null;
            BeforeIgnore = null;
            AfterIgnore = null;
        }

        public event EventHandler BeforeCommit;

        public event EventHandler AfterCommit;

        public event EventHandler BeforeIgnore;

        public event EventHandler AfterIgnore;
    }
}