using NHibernate;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using Vaiona.Persistence.Api;

namespace Vaiona.PersistenceProviders.NH
{
    public class NHibernateBulkUnitOfWork : IUnitOfWork
    {
        private const int LongQueryTimeOut = 3600; //seconds

        internal Conversation Conversation = null;
        private bool autoCommit = false;
        private bool throwExceptionOnError = true;

        public IPersistenceManager PersistenceManager { get; internal set; }

        internal NHibernateBulkUnitOfWork(NHibernatePersistenceManager persistenceManager, Conversation conversation, bool autoCommit = false, bool throwExceptionOnError = true)
        {
            this.PersistenceManager = persistenceManager;
            this.autoCommit = autoCommit;
            this.throwExceptionOnError = throwExceptionOnError;
            this.Conversation = conversation;
            this.Conversation.Start(this);
        }

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

        public void ClearCache(bool applyChanges = true)
        {
            //Session.
        }

        public IStatelessSession Session
        {
            get
            {
                return this.Conversation.GetStatelessSession();
            }
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

                    if (Session.Transaction.WasCommitted)
                    {
                        // log the changes detected in previous steps
                        if (AfterCommit != null)
                            AfterCommit(this, EventArgs.Empty);
                    }
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
            try
            {
                if (Session.Transaction.IsActive)
                {
                    if (BeforeIgnore != null)
                        BeforeIgnore(this, EventArgs.Empty);
                    Session.Transaction.Rollback();
                    if (Session.Transaction.WasRolledBack)
                    {
                        if (AfterIgnore != null)
                            AfterIgnore(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                if (throwExceptionOnError)
                    throw ex;
            }
        }

        //public void IgnoreAndContinue()
        //{
        //    Ignore();
        //    this.session.Transaction.Begin();
        //}

        public T Execute<T>(string queryName, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            T result = default(T);
            IStatelessSession session = this.Session;
            try
            {
                //session.BeginTransaction();
                IQuery query = session.GetNamedQuery(queryName);
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

        public List<T> ExecuteList<T>(string queryName, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && parameters.Any(p => p.Value == null))
                throw new ArgumentException("The parameter array has a null element", "parameters");
            lock (this)
            {
                var result = new List<T>();
                IStatelessSession session = this.Session;
                try
                {
                    IQuery query = session.GetNamedQuery(queryName);
                    if (parameters != null)
                    {
                        foreach (var item in parameters)
                        {
                            query.SetParameter(item.Key, item.Value);
                        }
                    }
                    result = query.List<T>().ToList();
                }
                catch
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
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");

            T result = default(T);
            IStatelessSession session = this.Session;
            try
            {
                //session.BeginTransaction();
                IQuery query = session.CreateSQLQuery(queryString);
                if (parameters != null)
                {
                    foreach (var item in parameters)
                    {
                        query.SetParameter(item.Key, item.Value);
                    }
                }
                //query.executeUpdate(); // ??
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

        public int ExecuteNonQuery(string queryString, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");
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
                            command.Parameters.Add(new SqlParameter(item.Key, item.Value));
                        }
                    }
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                //session.Transaction.Rollback();
                throw new Exception(string.Format($"Failed for execute the submitted native query. Reason: '{ex.Message}'"));
            }
            finally
            {
                // Do Nothing
            }
            return result;
        }

        public object ExecuteScalar(string queryString, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");
            object result = null;
            try
            {
                using (ITransaction transaction = this.Session.BeginTransaction())
                {
                    DbCommand command = this.Session.Connection.CreateCommand();
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

        public DataTable ExecuteQuery(string queryString, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && !Contract.ForAll(parameters, (KeyValuePair<string, object> p) => p.Value != null))
                throw new ArgumentException("The parameter array has a null element", "parameters");
            DataTable table = new DataTable();
            try
            {
                using (var con = new SqlConnection(this.Session.Connection.ConnectionString))
                {
                    using (var cmd = new SqlCommand(queryString, con))
                    {
                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            cmd.CommandType = CommandType.Text;
                            adapter.Fill(table);
                        }
                    }
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
            return table;
        }

        private bool isDisposed = false;

        ~NHibernateBulkUnitOfWork()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these
            // operations, as well as in your methods that use the resource.
            if (!isDisposed)
            {
                if (disposing)
                {
                    disposeResources();
                    isDisposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void disposeResources()
        {
            try
            {
                if (autoCommit & !Session.Transaction.WasCommitted)
                    this.Commit();
                else
                    this.Ignore();
                this.Conversation.End(this);
            }
            catch (ObjectDisposedException) // object is already disposed
            {
                return;
            }
            catch (Exception ex)
            {
                if (throwExceptionOnError)
                    throw ex;
            }
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